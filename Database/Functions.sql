set client_min_messages to warning;

drop type if exists game_outcome cascade;

create type game_outcome as enum
(
	'win',
	'loss',
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
		begin
			execute drop_command;
		exception when others then
		end;
	end loop;
end $$ language 'plpgsql';

select drop_functions();

create function check_player_id(player_id integer) returns void as $$
begin
	if not exists (select 1 from player where id = player_id) then
		raise exception 'Invalid player ID: %', player_id;
	end if;
end $$ language 'plpgsql';

create function get_player_name(player_id integer) returns text as $$
begin
	perform check_player_id(player_id);
	return (select player.name from player where id = player_id);
end $$ language 'plpgsql';

create function get_player_names(player_id_string text) returns table
(
	id integer,
	name text
) as $$
declare
	player_ids integer[];
begin
	select get_player_ids(player_id_string) into player_ids;
	return query select player.id, player.name from player where player.id = any(player_ids) order by player.name;
end $$ language 'plpgsql';

create function update_player(name text, steam_id text, _time timestamp) returns integer as $$
declare
	player_id integer;
	last_modified timestamp;
begin
	select player.last_modified from player where player.steam_id = update_player.steam_id into last_modified;
	if not found then
		insert into player (name, steam_id, last_modified) values (name, steam_id, _time) returning id into player_id;
	elsif last_modified < _time then
		update player
		set
			name = update_player.name,
			last_modified = _time
		where player.steam_id = update_player.steam_id
		returning id into player_id;
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

create function matches_time_constraints(_time timestamp, time_start timestamp, time_end timestamp) returns boolean as $$
begin
	return
		(time_start is null or _time >= time_start) and
		(time_end is null or _time < time_end);
end $$ language 'plpgsql';

create function get_player_kills(player_id integer, time_start timestamp, time_end timestamp) returns int as $$
declare
	kills integer;
	team_kills integer;
begin
	select count(*)
	from kill
	where
		killer_id = player_id and
		killer_id != victim_id and
		matches_time_constraints(time, time_start, time_end) and
		killer_team != victim_team
	into kills;
	select count(*)
	from kill
	where
		killer_id = player_id and
		killer_id != victim_id and
		matches_time_constraints(time, time_start, time_end) and
		killer_team = victim_team
	into team_kills;
	return kills - team_kills;
end $$ language 'plpgsql';

create function get_player_deaths(player_id integer, time_start timestamp, time_end timestamp) returns int as $$
begin
	return
	(
		select count(*)
		from kill
		where
			victim_id = player_id and
			matches_time_constraints(time, time_start, time_end) and
			killer_team != victim_team
	);
end $$ language 'plpgsql';

create function get_player_kill_death_ratio(player_id integer, time_start timestamp, time_end timestamp) returns numeric as $$
declare
	kills integer;
	deaths integer;
begin
	select get_player_kills(player_id, time_start, time_end) into kills;
	select get_player_deaths(player_id, time_start, time_end) into deaths;
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

create function get_player_rounds(player_id integer, time_start timestamp, time_end timestamp, get_rounds_won boolean default null, end_of_game boolean default false) returns integer as $$
begin
	return
	(
		select count(*)
		from round, round_player
		where
			round.id = round_player.round_id and
			matches_time_constraints(round.time, time_start, time_end) and
			round_player.player_id = get_player_rounds.player_id and
			(
				get_rounds_won is null or
				(
					(
						get_rounds_won and get_winning_team(round.sfui_notice) = round_player.team and
						(not end_of_game or round.terrorist_score != round.counter_terrorist_score)
					) or
					(not get_rounds_won and get_winning_team(round.sfui_notice) != round_player.team)
				)
			) and
			(
				not end_of_game or
				is_end_of_game(round.terrorist_score, round.counter_terrorist_score, round.max_rounds)
			)
	);
end $$ language 'plpgsql';

create function get_ratio(x integer, y integer) returns numeric as $$
begin
	if y = 0 then
		return null;
	end if;
	return x::numeric / y;
end $$ language 'plpgsql';

create function get_player_game_win_ratio(player_id integer, time_start timestamp, time_end timestamp) returns numeric as $$
declare
	wins integer;
	games integer;
begin
	select get_player_rounds(player_id, time_start, time_end, true, true) into wins;
	select get_player_rounds(player_id, time_start, time_end, null, true) into games;
	return get_ratio(wins, games);
end $$ language 'plpgsql';

create function get_player_round_win_ratio(player_id integer, time_start timestamp, time_end timestamp) returns numeric as $$
declare
	wins integer;
	games integer;
begin
	select get_player_rounds(player_id, time_start, time_end, true) into wins;
	select get_player_rounds(player_id, time_start, time_end) into games;
	return get_ratio(wins, games);
end $$ language 'plpgsql';

create function get_all_player_stats(time_start timestamp, time_end timestamp) returns table
(
	id integer,
	name text,
	kills integer,
	deaths integer,
	kill_death_ratio numeric,
	rounds_played integer,
	round_win_ratio numeric,
	games_played integer,
	game_win_ratio numeric
) as $$
begin
	return query select
		player.id,
		player.name,
		get_player_kills(player.id, time_start, time_end) as kills,
		get_player_deaths(player.id, time_start, time_end) as deaths,
		get_player_kill_death_ratio(player.id, time_start, time_end) as kill_death_ratio,
		get_player_rounds(player.id, time_start, time_end) as rounds_played,
		get_player_round_win_ratio(player.id, time_start, time_end) as round_win_ratio,
		get_player_rounds(player.id, time_start, time_end, null, true) as games_played,
		get_player_game_win_ratio(player.id, time_start, time_end) as game_win_ratio
	from player
	where get_player_rounds(player.id, time_start, time_end) > 0
	order by name;
end $$ language 'plpgsql';

create function get_player_weapon_kills(player_id integer, time_start timestamp, time_end timestamp, weapon text, headshots_only boolean) returns integer as $$
begin
	return
	(
		select count(*)
		from kill
		where
			killer_id = player_id and
			matches_time_constraints(time, time_start, time_end) and
			kill.weapon = get_player_weapon_kills.weapon and
			(not headshots_only or headshot) and
			killer_team != victim_team
	);
end $$ language 'plpgsql';

create function get_player_weapon_headshot_ratio(player_id integer, time_start timestamp, time_end timestamp, weapon text) returns numeric as $$
declare
	kills integer;
	headshots integer;
begin
	select get_player_weapon_kills(player_id, time_start, time_end, weapon, false) into kills;
	select get_player_weapon_kills(player_id, time_start, time_end, weapon, true) into headshots;
	return get_ratio(headshots, kills);
end $$ language 'plpgsql';

create function get_player_weapon_stats(player_id integer, time_start timestamp, time_end timestamp) returns table
(
	weapon text,
	kills integer,
	headshots integer,
	headshot_ratio numeric
) as $$
begin
	perform check_player_id(player_id);
	return query select
		kill.weapon,
		get_player_weapon_kills(player_id, time_start, time_end, kill.weapon, false) as kills,
		get_player_weapon_kills(player_id, time_start, time_end, kill.weapon, true) as headshots,
		get_player_weapon_headshot_ratio(player_id, time_start, time_end, kill.weapon) as headshot_ratio
	from kill
	where
		killer_id = player_id and
		matches_time_constraints(time, time_start, time_end)
	group by kill.weapon;
end $$ language 'plpgsql';

create function get_matchup_kills(killer_id integer, victim_id integer, time_start timestamp, time_end timestamp) returns int as $$
begin
	return
	(
		select count(*)
		from kill
		where
			kill.killer_id = get_matchup_kills.killer_id and
			kill.victim_id = get_matchup_kills.victim_id and
			kill.killer_team != kill.victim_team and
			matches_time_constraints(time, time_start, time_end)
	);
end $$ language 'plpgsql';

create function get_encounter_win_ratio(player_id integer, opponent_id integer, time_start timestamp, time_end timestamp) returns numeric as $$
declare
	kills integer;
	deaths integer;
	encounters integer;
begin
	select get_matchup_kills(player_id, opponent_id, time_start, time_end) into kills;
	select get_matchup_kills(opponent_id, player_id, time_start, time_end) into deaths;
	encounters := kills + deaths;
	if encounters = 0 then
		return null;
	end if;
	return get_ratio(kills, encounters);
end $$ language 'plpgsql';

create function get_player_encounter_stats(player_id integer, time_start timestamp, time_end timestamp) returns table
(
	opponent_id integer,
	opponent_name text,
	encounters integer,
	kills integer,
	deaths integer,
	win_ratio numeric
) as $$
begin
	perform check_player_id(player_id);
	return query select
		id as opponent_id,
		name as opponent_name,
		get_matchup_kills(player_id, id, time_start, time_end) + get_matchup_kills(id, player_id, time_start, time_end) as encounters,
		get_matchup_kills(player_id, id, time_start, time_end) as kills,
		get_matchup_kills(id, player_id, time_start, time_end) as deaths,
		get_encounter_win_ratio(player_id, id, time_start, time_end) as win_ratio
	from player
	where
		id != player_id and
		get_encounter_win_ratio(player_id, id, time_start, time_end) is not null;
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

create function split_ids(steam_ids_string text) returns text[] as $$
begin
	return (select string_to_array(steam_ids_string, ','));
end $$ language 'plpgsql';

create function add_players_to_team(round_id integer, team team_type, steam_ids_string text) returns void as $$
declare
	steam_ids text[];
	loop_steam_id text;
	player_id integer;
begin
	select split_ids(steam_ids_string) into steam_ids;
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

create function get_player_purchases_per_round(player_id integer, time_start timestamp, time_end timestamp, item text) returns numeric as $$
declare
	purchases integer := 0;
	rounds integer := 0;
begin
	if exists
	(
		select 1
		from purchase
		where
			purchase.item = get_player_purchases_per_round.item and
			matches_time_constraints(time, time_start, time_end)
	)
	then
		select count(*)
		from purchase
		where
			purchase.player_id = get_player_purchases_per_round.player_id and
			purchase.item = get_player_purchases_per_round.item and
			matches_time_constraints(time, time_start, time_end)
		into purchases;
		select get_player_rounds(player_id, time_start, time_end) into rounds;
	end if;
	if rounds = 0 then
		return null;
	end if;
	return purchases::numeric / rounds;
end $$ language 'plpgsql';

create function get_player_kills_per_purchase(player_id integer, time_start timestamp, time_end timestamp, weapon text) returns numeric as $$
declare
	kills integer;
	purchases integer;
begin
	select count(*)
	from kill
	where
		killer_id = player_id and
		kill.weapon = get_player_kills_per_purchase.weapon and
		matches_time_constraints(time, time_start, time_end)
	into kills;
	if kills = 0 then
		return null;
	end if;
	select count(*)
	from purchase
	where
		purchase.player_id = get_player_kills_per_purchase.player_id and
		purchase.item = get_player_kills_per_purchase.weapon and
		matches_time_constraints(time, time_start, time_end)
	into purchases;
	if purchases = 0 then
		return null;
	end if;
	return kills::numeric / purchases;
end $$ language 'plpgsql';

create function get_player_purchases(player_id integer, time_start timestamp, time_end timestamp) returns table
(
	item text,
	times_purchased integer,
	purchases_per_round numeric,
	kills_per_purchase numeric
) as $$
declare
	rounds_played integer;
begin
	select get_player_rounds(player_id, time_start, time_end) into rounds_played;
	return query select
		purchase.item,
		count(*)::integer as times_purchased,
		get_player_purchases_per_round(get_player_purchases.player_id, time_start, time_end, purchase.item) as purchases_per_round,
		get_player_kills_per_purchase(get_player_purchases.player_id, time_start, time_end, purchase.item) as kills_per_purchase
	from purchase
	where purchase.player_id = get_player_purchases.player_id
	group by purchase.item;
end $$ language 'plpgsql';

create function get_outcome(terrorist_score integer, counter_terrorist_score integer, max_rounds integer, team team_type) returns game_outcome as $$
begin
	return case
		when
			terrorist_score = counter_terrorist_score
		then
			'draw'::game_outcome
		when
			(
				terrorist_score > max_rounds / 2 and
				team = 'terrorist'::team_type
			) or
			(
				counter_terrorist_score > max_rounds / 2 and
				team = 'counter_terrorist'::team_type
			)
		then
			'win'::game_outcome
		else
			'loss'::game_outcome
	end;
end $$ language 'plpgsql';

-- player_team, enemy_team and outcome are cast to text to deal with a missing feature in Npgsql 3.0
-- It's supposed to get fixed in Npgsql 3.1, though
create function get_player_games(player_id integer, time_start timestamp, time_end timestamp) returns table
(
	game_time timestamp,
	player_score integer,
	enemy_score integer,
	player_team text,
	enemy_team text,
	outcome text
) as $$
begin
	perform check_player_id(player_id);
	return query select
		round.time as game_time,
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
		)::text
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
		)::text
		as enemy_team,
		get_outcome(round.terrorist_score, round.counter_terrorist_score, round.max_rounds, round_player.team)::text
		as outcome
	from
		round,
		round_player
	where
		round.id = round_player.round_id and
		matches_time_constraints(round.time, time_start, time_end) and
		round_player.player_id = get_player_games.player_id and
		is_end_of_game(round.terrorist_score, round.counter_terrorist_score, round.max_rounds)
	order by round.time desc;
end $$ language 'plpgsql';

create function get_log_state(file_name text) returns bigint as $$
begin
	return
	(
		select log_state.bytes_processed
		from log_state
		where log_state.file_name = get_log_state.file_name
	);
end $$ language 'plpgsql';

create function update_log_state(file_name text, bytes_processed bigint) returns void as $$
begin
    begin
        insert into log_state (file_name, bytes_processed)
            values (update_log_state.file_name, update_log_state.bytes_processed);
    exception when unique_violation then
        update log_state
            set bytes_processed = update_log_state.bytes_processed
            where log_state.file_name = update_log_state.file_name;
    end;
end $$ language 'plpgsql';

create function get_round_team_player_ids(round_id integer, team team_type) returns integer[] as $$
declare
	player_ids integer[];
begin
	select array_agg(player_id)
	from round_player
	where
		round_player.round_id = get_round_team_player_ids.round_id and
		round_player.team = get_round_team_player_ids.team
	into player_ids;
	-- Requires intarray extension to be enabled for the BeRated database
	return sort(player_ids);
end $$ language 'plpgsql';

create function get_player_ids(player_id_string text) returns integer[] as $$
declare
	player_id_strings text[];
	player_ids integer[];
begin
	select split_ids(player_id_string) into player_id_strings;
	select array_agg(player_ids.player_id) from
	(
		select unnest(player_id_strings)::integer as player_id
	) as player_ids
	into player_ids;
	if cardinality(player_id_strings) != cardinality(player_ids) then
		raise exception 'Invalid player ID';
	end if;
	-- Requires intarray extension to be enabled for the BeRated database
	return sort(player_ids);
end $$ language 'plpgsql';

create function get_opposite_team(team team_type) returns team_type as $$
declare
	other_team team_type := 'terrorist'::team_type;
begin
	if team = 'terrorist'::team_type then
		other_team := 'counter_terrorist'::team_type;
	end if;
	return other_team;
end $$ language 'plpgsql';

create function get_matchup_outcomes(player_ids1 integer[], player_ids2 integer[], team team_type, precise boolean) returns table
(
	outcome game_outcome
) as $$
declare
	other_team team_type := get_opposite_team(team);
begin
	return query
	select get_outcome(terrorist_score, counter_terrorist_score, max_rounds, team) as outcome
	from round
	where
		is_end_of_game(terrorist_score, counter_terrorist_score, max_rounds) and
		(
			(
				not precise and
				player_ids1 <@ get_round_team_player_ids(id, team) and
				player_ids2 <@ get_round_team_player_ids(id, other_team)
			) or
			(
				precise and
				player_ids1 = get_round_team_player_ids(id, team) and
				player_ids2 = get_round_team_player_ids(id, other_team)
			)
		);
end $$ language 'plpgsql';

create function get_matchup_stats(player_id_string1 text, player_id_string2 text, precise boolean) returns table
(
	games integer,
	wins integer,
	losses integer,
	draws integer,
	win_ratio numeric
) as $$
declare
	player_ids1 integer[];
	player_ids2 integer[];
	games integer;
	wins integer;
	losses integer;
	draws integer;
begin
	select get_player_ids(player_id_string1) into player_ids1;
	select get_player_ids(player_id_string2) into player_ids2;
	with outcomes as
	(
		select * from get_matchup_outcomes(player_ids1, player_ids2, 'terrorist'::team_type, precise)
		union all
		select * from get_matchup_outcomes(player_ids1, player_ids2, 'counter_terrorist'::team_type, precise)
	)
	select
		(select count(*) from outcomes) as games,
		(select count(*) from outcomes where outcome = 'win'::game_outcome) as wins,
		(select count(*) from outcomes where outcome = 'loss'::game_outcome) as losses,
		(select count(*) from outcomes where outcome = 'draw'::game_outcome) as draws
	into games, wins, losses, draws;
	return query select
		games,
		wins,
		losses,
		draws,
		get_ratio(wins, games);
end $$ language 'plpgsql';

create function get_teams(team team_type, time_start timestamp, time_end timestamp) returns table
(
	player_ids integer[],
	games integer,
	wins integer,
	losses integer,
	draws integer
)
as $$
begin
	return query
	with games as
	(
		select
			get_round_team_player_ids(id, team) as player_ids,
			get_outcome(terrorist_score, counter_terrorist_score, max_rounds, team) as outcome
		from round
		where
			matches_time_constraints(time, time_start, time_end) and
			is_end_of_game(terrorist_score, counter_terrorist_score, max_rounds)
	)
	select
		current_games.player_ids as player_ids,
		(select count(*)::integer from games where current_games.player_ids = games.player_ids) as games,
		(select count(*)::integer from games where current_games.player_ids = games.player_ids and outcome = 'win'::game_outcome) as wins,
		(select count(*)::integer from games where current_games.player_ids = games.player_ids and outcome = 'loss'::game_outcome) as losses,
		(select count(*)::integer from games where current_games.player_ids = games.player_ids and outcome = 'draw'::game_outcome) as draws
	from games as current_games
	group by current_games.player_ids;
end $$ language 'plpgsql';

create function get_player_info(player_ids integer[]) returns player_information[] as $$
declare
	player_info player_information[];
begin
	select array_agg(row(player_id, get_player_name(player_id))::player_information)
	from unnest(player_ids) as player_id
	into player_info;
	return player_info;
end $$ language 'plpgsql';

create function get_teams(time_start timestamp, time_end timestamp) returns table
(
	-- Should be player_information[]
	players text,
	games integer,
	wins integer,
	losses integer,
	draws integer,
	win_ratio numeric
)
as $$
begin
	return query
	select
		get_player_info(team.player_ids)::text as players,
		sum(team.games)::integer as games,
		sum(team.wins)::integer as wins,
		sum(team.losses)::integer as losses,
		sum(team.draws)::integer as draws,
		get_ratio(sum(team.wins)::integer, sum(team.games)::integer) as win_ratio
	from
	(
		select * from get_teams('terrorist'::team_type, time_start, time_end)
		union all
		select * from get_teams('counter_terrorist'::team_type, time_start, time_end)
	) as team
	-- Workaround for possibly broken games
	where team.player_ids is not null
	group by team.player_ids
	order by games desc;
end $$ language 'plpgsql';

create function get_time_of_most_recent_kill() returns timestamp as $$
	select max(time) from kill;
$$ language sql;