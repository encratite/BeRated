set client_min_messages to warning;

drop type if exists game_outcome cascade;

create type game_outcome as enum
(
	'loss',
	'win',
	'draw'
);

drop type if exists player_information cascade;

create type player_information as
(
	id integer,
	name text
);

create or replace function drop_functions() returns void as $$
declare
	function_record record;
	drop_command text;
begin
	for function_record in
		select * from pg_proc inner join pg_namespace ns on (pg_proc.pronamespace = ns.oid) where ns.nspname = 'public' order by proname
	loop
		drop_command := 'drop function ' || function_record.nspname || '.' || function_record.proname || '(' || oidvectortypes(function_record.proargtypes) || ');';
		--raise warning '%', drop_command;
		execute drop_command;
	end loop;
end $$ language 'plpgsql';

select drop_functions();

create function lock_tables() returns void as $$
begin
	lock player;
	lock kill;
	lock round;
	lock round_player;
	lock purchase;
end $$ language 'plpgsql';

create function check_player_id(player_id integer) returns void as $$
begin
	if not exists (select 1 from player where id = player_id) then
		raise exception 'Invalid player ID: %', player_id;
	end if;
end $$ language 'plpgsql';

create function get_player_name(player_id integer) returns text as $$
declare
	name text;
begin
	perform check_player_id(player_id);
	select player.name from player where id = player_id into name;
	return name;
end $$ language 'plpgsql';

create function update_player(name text, steam_id text) returns integer as $$
declare
	player_id integer;
begin
	update player set name = update_player.name where player.steam_id = update_player.steam_id returning id into player_id;
	if not found then
		insert into player (name, steam_id) values (name, steam_id) returning id into player_id;
	end if;
	return player_id;
end $$ language 'plpgsql';

create function get_team(team text) returns team_type as $$
begin
	if team = 'TERRORIST' then
		return 'terrorist'::team_type;
	elsif team = 'CT' then
		return 'counter_terrorist'::team_type;
	else
		raise exception 'Invalid team identifier: %', team;
	end if;
end $$ language 'plpgsql';

create function get_sfui_notice(sfui_notice text) returns sfui_notice_type as $$
begin
	if sfui_notice = 'SFUI_Notice_All_Hostages_Rescued' then
		return 'all_hostages_rescued'::sfui_notice_type;
	elsif sfui_notice = 'SFUI_Notice_Bomb_Defused' then
		return 'bomb_defused'::sfui_notice_type;
	elsif sfui_notice = 'SFUI_Notice_CTs_Win' then
		return 'cts_win'::sfui_notice_type;
	elsif sfui_notice = 'SFUI_Notice_Hostages_Not_Rescued' then
		return 'hostages_not_rescued'::sfui_notice_type;
	elsif sfui_notice = 'SFUI_Notice_Target_Bombed' then
		return 'target_bombed'::sfui_notice_type;
	elsif sfui_notice = 'SFUI_Notice_Target_Saved' then
		return 'target_saved'::sfui_notice_type;
	elsif sfui_notice = 'SFUI_Notice_Terrorists_Win' then
		return 'terrorists_win'::sfui_notice_type;
	else
		raise exception 'Invalid SFUI notice: %', sfui_notice;
	end if;
end $$ language 'plpgsql';

create function get_winning_team(sfui_notice sfui_notice_type) returns team_type as $$
begin
	if sfui_notice = 'all_hostages_rescued'::sfui_notice_type then
		return 'counter_terrorist'::team_type;
	elsif sfui_notice = 'bomb_defused'::sfui_notice_type then
		return 'counter_terrorist'::team_type;
	elsif sfui_notice = 'cts_win'::sfui_notice_type then
		return 'counter_terrorist'::team_type;
	elsif sfui_notice = 'hostages_not_rescued'::sfui_notice_type then
		return 'terrorist'::team_type;
	elsif sfui_notice = 'target_bombed'::sfui_notice_type then
		return 'terrorist'::team_type;
	elsif sfui_notice = 'target_saved'::sfui_notice_type then
		return 'counter_terrorist'::team_type;
	elsif sfui_notice = 'terrorists_win'::sfui_notice_type then
		return 'terrorist'::team_type;
	else
		raise exception 'Unknown SFUI notice enum: %', sfui_notice;
	end if;
end $$ language 'plpgsql';

create function convert_weapon(weapon text) returns text as $$
begin
	if weapon = 'knife_t' or weapon = 'knife_default_ct' then
		return 'knife';
	else
		return weapon;
	end if;
end $$ language 'plpgsql';

create function process_kill(kill_time timestamp, killer_steam_id text, killer_team text, killer_x integer, killer_y integer, killer_z integer, victim_steam_id text, victim_team text, victim_x integer, victim_y integer, victim_z integer, weapon text, headshot boolean) returns void as $$
declare
	killer_id integer;
	killer_team_enum team_type;
	victim_id integer;
	victim_team_enum team_type;
begin
	select get_player_id_by_steam_id(killer_steam_id) into killer_id;
	select get_player_id_by_steam_id(victim_steam_id) into victim_id;
	select get_team(killer_team) into killer_team_enum;
	select get_team(victim_team) into victim_team_enum;
	begin
		insert into kill
		(
			time,
			killer_id,
			killer_team,
			killer_vector,
			victim_id, victim_team,
			victim_vector,
			weapon,
			headshot
		)
		values
		(
			kill_time,
			killer_id,
			killer_team_enum,
			array[killer_x, killer_y, killer_z],
			victim_id,
			victim_team_enum,
			array[victim_x, victim_y, victim_z],
			convert_weapon(weapon),
			headshot
		);
	exception when unique_violation then
	end;
end $$ language 'plpgsql';

create function get_player_kills(player_id integer, team_kills boolean default false) returns int as $$
declare
	kills integer;
begin
	select count(*) from kill where killer_id = player_id and killer_id != victim_id and (not team_kills or killer_team = victim_team) into kills;
	return kills;
end $$ language 'plpgsql';

create function get_player_deaths(player_id integer) returns int as $$
declare
	deaths integer;
begin
	select count(*) from kill where victim_id = player_id into deaths;
	return deaths;
end $$ language 'plpgsql';

create function get_player_kill_death_ratio(player_id integer) returns numeric as $$
declare
	kills integer;
	deaths integer;
begin
	select get_player_kills(player_id) into kills;
	select get_player_deaths(player_id) into deaths;
	if deaths = 0 then
		return null;
	end if;
	return kills::numeric / deaths;
end $$ language 'plpgsql';

create function is_end_of_game(score integer, max_rounds integer) returns boolean as $$
begin
	return score > (max_rounds / 2);
end $$ language 'plpgsql';

create function is_end_of_game(score1 integer, score2 integer, max_rounds integer) returns boolean as $$
begin
	return
		is_end_of_game(score1, max_rounds) or
		is_end_of_game(score2, max_rounds) or
		score1 + score2 >= max_rounds;
end $$ language 'plpgsql';

create function get_player_rounds(player_id integer, team team_type default null, get_rounds_won boolean default null, end_of_game boolean default false) returns integer as $$
declare
	rounds_won integer;
begin
	select count(*)
	from round, round_player
	where
		round.id = round_player.round_id and
		round_player.player_id = get_player_rounds.player_id and
		(
			get_rounds_won is null or
			(
				(get_rounds_won and get_winning_team(round.sfui_notice) = round_player.team) or
				(not get_rounds_won and get_winning_team(round.sfui_notice) != round_player.team)
			)
		) and
		(
			get_player_rounds.team is null or
			get_player_rounds.team = round_player.team
		) and
		(
			not end_of_game or
			is_end_of_game(round.terrorist_score, round.counter_terrorist_score, round.max_rounds)
		)
	into rounds_won;
	return rounds_won;
end $$ language 'plpgsql';

create function get_percentage(x integer, y integer) returns numeric as $$
begin
	return round(x::numeric / y * 100, 1);
end $$ language 'plpgsql';

create function get_player_game_win_percentage(player_id integer) returns numeric as $$
declare
	wins integer;
	games integer;
begin
	select get_player_rounds(player_id, null, true, true) into wins;
	select get_player_rounds(player_id, null, null, true) into games;
	return get_percentage(wins, games);
end $$ language 'plpgsql';

create function get_player_round_win_percentage(player_id integer, team team_type default null) returns numeric as $$
declare
	wins integer;
	games integer;
begin
	select get_player_rounds(player_id, team, true) into wins;
	select get_player_rounds(player_id, team) into games;
	return get_percentage(wins, games);
end $$ language 'plpgsql';

create function get_all_player_stats() returns table
(
	id integer,
	name text,
	kills integer,
	deaths integer,
	team_kills integer,
	kill_death_ratio numeric,
	rounds_played integer,
	win_percentage numeric,
	rounds_played_terrorist integer,
	win_percentage_terrorist numeric,
	rounds_played_counter_terrorist integer,
	win_percentage_counter_terrorist numeric,
	games_played integer,
	game_win_percentage numeric
) as $$
begin
	return query select
		player.id,
		player.name,
		get_player_kills(player.id) as kills,
		get_player_deaths(player.id) as deaths,
		get_player_kills(player.id, true) as kills,
		round(get_player_kill_death_ratio(player.id), 2) as kill_death_ratio,
		get_player_rounds(player.id) as rounds_played,
		get_player_round_win_percentage(player.id) as win_percentage,
		get_player_rounds(player.id, 'terrorist'::team_type) as rounds_played_terrorist,
		get_player_round_win_percentage(player.id, 'terrorist'::team_type) as win_percentage_terrorist,
		get_player_rounds(player.id, 'counter_terrorist'::team_type) as rounds_played_counter_terrorist,
		get_player_round_win_percentage(player.id, 'counter_terrorist'::team_type) as win_percentage_counter_terrorist,
		get_player_rounds(player.id, null, null, true) as games_played,
		get_player_game_win_percentage(player.id) as game_win_percentage
	from player;
end $$ language 'plpgsql';

create function get_player_weapon_kills(player_id integer, weapon text, headshots_only boolean) returns integer as $$
declare
	kills integer;
begin
	select count(*)
	from kill
	where
		killer_id = player_id and
		kill.weapon = get_player_weapon_kills.weapon and
		(not headshots_only or headshot = true)
	into kills;
	return kills;
end $$ language 'plpgsql';

create function get_player_weapon_headshot_percentage(player_id integer, weapon text) returns numeric as $$
declare
	kills integer;
	headshots integer;
begin
	select get_player_weapon_kills(player_id, weapon, false) into kills;
	select get_player_weapon_kills(player_id, weapon, true) into headshots;
	return get_percentage(headshots, kills);
end $$ language 'plpgsql';

create function get_player_weapon_stats(player_id integer) returns table
(
	weapon text,
	kills integer,
	headshots integer,
	headshot_percentage numeric
) as $$
begin
	perform check_player_id(player_id);
	return query select
		kill.weapon,
		get_player_weapon_kills(player_id, kill.weapon, false) as kills,
		get_player_weapon_kills(player_id, kill.weapon, true) as headshots,
		get_player_weapon_headshot_percentage(player_id, kill.weapon) as headshot_percentage
	from kill
	where killer_id = player_id
	group by kill.weapon;
end $$ language 'plpgsql';

create function get_matchup_kills(killer_id integer, victim_id integer) returns int as $$
declare
	kills integer;
begin
	select count(*)
	from kill
	where
		kill.killer_id = get_matchup_kills.killer_id and
		kill.victim_id = get_matchup_kills.victim_id
	into kills;
	return kills;
end $$ language 'plpgsql';

create function get_encounter_win_percentage(player_id integer, opponent_id integer) returns numeric as $$
declare
	kills integer;
	deaths integer;
	encounters integer;
begin
	select get_matchup_kills(player_id, opponent_id) into kills;
	select get_matchup_kills(opponent_id, player_id) into deaths;
	encounters := kills + deaths;
	if encounters = 0 then
		return null;
	end if;
	return get_percentage(kills, encounters);
end $$ language 'plpgsql';

create function get_player_encounter_stats(player_id integer) returns table
(
	opponent_id integer,
	opponent_name text,
	encounters integer,
	kills integer,
	deaths integer,
	win_percentage numeric
) as $$
begin
	perform check_player_id(player_id);
	return query select
		id as opponent_id,
		name as opponent_name,
		get_matchup_kills(player_id, id) + get_matchup_kills(id, player_id) as encounters,
		get_matchup_kills(player_id, id) as kills,
		get_matchup_kills(id, player_id) as deaths,
		get_encounter_win_percentage(player_id, id) as win_percentage
	from player
	where
		id != player_id and
		get_encounter_win_percentage(player_id, id) is not null;
end $$ language 'plpgsql';

create function get_player_id_by_steam_id(steam_id text) returns integer as $$
declare
	player_id integer;
begin
	select id from player where player.steam_id = get_player_id_by_steam_id.steam_id into player_id;
	if not found then
		raise exception 'Unable to find player with Steam ID %', steam_id;
	end if;
	return player_id;
end $$ language 'plpgsql';

create function add_players_to_team(round_id integer, team team_type, steam_ids_string text) returns void as $$
declare
	steam_ids text[];
	loop_steam_id text;
	player_id integer;
begin
	select string_to_array(steam_ids_string, ',') into steam_ids;
	foreach loop_steam_id in array steam_ids loop
		select get_player_id_by_steam_id(loop_steam_id) into player_id;
		insert into round_player
		(
			round_id,
			player_id,
			team
		)
		values
		(
			round_id,
			player_id,
			team
		);
	end loop;
end $$ language 'plpgsql';

create function process_end_of_round(end_of_round_time timestamp, triggering_team text, sfui_notice text, terrorist_score integer, counter_terrorist_score integer, max_rounds integer, terrorist_steam_ids text, counter_terrorist_steam_ids text) returns void as $$
declare
	triggering_team_enum team_type;
	sfui_notice_enum sfui_notice_type;
	round_id integer;
begin
	if exists (select 1 from round where time = end_of_round_time) then
		return;
	end if;
	select get_team(triggering_team) into triggering_team_enum;
	select get_sfui_notice(sfui_notice) into sfui_notice_enum;
	insert into round
	(
		time,
		triggering_team,
		sfui_notice,
		terrorist_score,
		counter_terrorist_score,
		max_rounds
	)
	values
	(
		end_of_round_time,
		triggering_team_enum,
		sfui_notice_enum,
		terrorist_score,
		counter_terrorist_score,
		max_rounds
	)
	returning id into round_id;
	perform add_players_to_team(round_id, 'terrorist'::team_type, terrorist_steam_ids);
	perform add_players_to_team(round_id, 'counter_terrorist'::team_type, counter_terrorist_steam_ids);
end $$ language 'plpgsql';

create function process_purchase(steam_id text, line integer, purchase_time timestamp, team text, item text) returns void as $$
begin
	if exists (select 1 from purchase where time = purchase_time and purchase.line = process_purchase.line) then
		return;
	end if;
	insert into purchase
	(
		player_id,
		line,
		time,
		team,
		item
	)
	values
	(
		get_player_id_by_steam_id(steam_id),
		line,
		purchase_time,
		get_team(team),
		item
	);
end $$ language 'plpgsql';

create function get_player_purchases_in_team(player_id integer, item text, team team_type) returns integer as $$
declare
	purchases integer;
begin
	select count(*) from purchase
	where
		purchase.player_id = get_player_purchases_in_team.player_id and
		purchase.item = get_player_purchases_in_team.item and
		purchase.team = get_player_purchases_in_team.team
	into purchases;
	return purchases;
end $$ language 'plpgsql';

create function get_player_purchases_per_round(player_id integer, item text, team team_type) returns integer[2] as $$
declare
	purchases integer := 0;
	rounds integer := 0;
begin
	if exists(select 1 from purchase where purchase.item = get_player_purchases_per_round.item and purchase.team = get_player_purchases_per_round.team) then
		select count(*) from purchase where purchase.player_id = get_player_purchases_per_round.player_id and purchase.item = get_player_purchases_per_round.item and purchase.team =  get_player_purchases_per_round.team into purchases;
		select get_player_rounds(player_id, get_player_purchases_per_round.team) into rounds;
	end if;
	return array[purchases, rounds];
end $$ language 'plpgsql';

create function get_player_purchases_per_round(player_id integer, item text) returns numeric as $$
declare
	terrorist_result integer[2];
	counter_terrorist_result integer[2];
	purchases integer;
	rounds integer;
begin
	select get_player_purchases_per_round(player_id, item, 'terrorist'::team_type) into terrorist_result;
	select get_player_purchases_per_round(player_id, item, 'counter_terrorist'::team_type) into counter_terrorist_result;
	purchases := terrorist_result[1] + counter_terrorist_result[1];
	rounds := terrorist_result[2] + counter_terrorist_result[2];
	return round(purchases::numeric / rounds, 2);
end $$ language 'plpgsql';

create function get_player_kills_per_purchase(player_id integer, weapon text) returns numeric as $$
declare
	kills integer;
	purchases integer;
begin
	select count(*) from kill where killer_id = player_id and kill.weapon = get_player_kills_per_purchase.weapon into kills;
	if kills = 0 then
		return null;
	end if;
	select count(*) from purchase where purchase.player_id = get_player_kills_per_purchase.player_id and purchase.item = get_player_kills_per_purchase.weapon into purchases;
	return round(kills::numeric / purchases, 2);
end $$ language 'plpgsql';

create function get_player_purchases(player_id integer) returns table
(
	item text,
	times_purchased integer,
	purchases_per_round numeric,
	kills_per_purchase numeric
) as $$
declare
	rounds_played integer;
begin
	select get_player_rounds(player_id) into rounds_played;
	return query select
		purchase.item,
		count(*)::integer as times_purchased,
		get_player_purchases_per_round(get_player_purchases.player_id, purchase.item) as purchases_per_round,
		get_player_kills_per_purchase(get_player_purchases.player_id, purchase.item) as kills_per_purchase
	from purchase
	where purchase.player_id = get_player_purchases.player_id
	group by purchase.item;
end $$ language 'plpgsql';

create function get_player_kills_on_day(player_id integer, day date) returns int as $$
declare
	kills integer;
begin
	select count(*) from kill where killer_id = player_id and killer_id != victim_id and time::date = day into kills;
	return kills;
end $$ language 'plpgsql';

create function get_player_deaths_on_day(player_id integer, day date) returns int as $$
declare
	deaths integer;
begin
	select count(*) from kill where victim_id = player_id and time::date = day into deaths;
	return deaths;
end $$ language 'plpgsql';

create function get_player_kill_death_ratio_on_day(player_id integer, day date) returns numeric as $$
declare
	kills integer;
	deaths integer;
begin
	select get_player_kills_on_day(player_id, day) into kills;
	select get_player_deaths_on_day(player_id, day) into deaths;
	if deaths = 0 then
		return null;
	end if;
	return kills::numeric / deaths;
end $$ language 'plpgsql';

create function get_player_kill_death_ratio_history(player_id integer) returns table
(
	day date,
	kill_death_ratio numeric
) as $$
begin
	perform check_player_id(player_id);
	return query select
		date(time) as day,
		round(get_player_kill_death_ratio_on_day(player_id, time::date), 2) as kill_death_ratio
	from kill
	where victim_id = player_id
	group by time::date
	order by time::date;
end $$ language 'plpgsql';

create function get_player_game_history(player_id integer) returns table
(
	game_time timestamp,
	player_score integer,
	enemy_score integer,
	player_team player_information[],
	enemy_team player_information[],
	outcome game_outcome
) as $$
begin
	perform check_player_id(player_id);
	return query select
		round.time as time,
		case
			when round_player.team = 'terrorist'::team_type
			then round.terrorist_score
			else round.counter_terrorist_score
		end
		as player_score,
		case
			when round_player.team = 'terrorist'::team_type
			then round.counter_terrorist_score
			else round.terrorist_score
		end
		as enemy_score,
		array
		(
			select
				row(r.player_id, get_player_name(r.player_id))::player_information
			from
				round_player as r
			where
				r.round_id = round.id and
				r.team = round_player.team
		)
		as player_team,
		array
		(
			select
				row(r.player_id, get_player_name(r.player_id))::player_information
			from
				round_player as r
			where
				r.round_id = round.id and
				r.team != round_player.team
		)
		as enemy_team,
		case
			when
				round.terrorist_score = round.counter_terrorist_score
			then
				'draw'::game_outcome
			when
				(
					round.terrorist_score >= round.max_rounds / 2.0 and
					round_player.team = 'terrorist'::team_type
				) or
				(
					round.counter_terrorist_score >= round.max_rounds / 2.0 and
					round_player.team = 'counter_terrorist'::team_type
				)
			then
				'win'::game_outcome
			else
				'loss'::game_outcome
		end
		as outcome
	from
		round,
		round_player
	where
		round.id = round_player.round_id and
		round_player.player_id = get_player_game_history.player_id and
		(
			round.terrorist_score >= round.max_rounds / 2.0 or
			round.counter_terrorist_score >= round.max_rounds / 2.0 or
			round.terrorist_score + round.counter_terrorist_score = round.max_rounds
		)
	order by round.time;
end $$ language 'plpgsql';