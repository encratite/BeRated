set client_min_messages to warning;

create or replace function lock_tables() returns void as $$
begin
	lock player;
	lock kill;
	lock round;
	lock round_player;
	lock purchase;
end $$ language 'plpgsql';

create or replace function check_player_id(player_id integer) returns void as $$
begin
	if not exists (select 1 from player where id = player_id) then
		raise exception 'Invalid player ID';
	end if;
end $$ language 'plpgsql';

create or replace function get_player_name(player_id integer) returns text as $$
declare
	name text;
begin
	perform check_player_id(player_id);
	select player.name from player where id = player_id into name;
	return name;
end $$ language 'plpgsql';

create or replace function update_player(name text, steam_id text) returns integer as $$
declare
	player_id integer;
begin
	update player set name = update_player.name where player.steam_id = update_player.steam_id returning id into player_id;
	if not found then
		insert into player (name, steam_id) values (name, steam_id) returning id into player_id;
	end if;
	return player_id;
end $$ language 'plpgsql';

create or replace function get_team(team text) returns team_type as $$
begin
	if team = 'TERRORIST' then
		return 'terrorist'::team_type;
	elsif team = 'CT' then
		return 'counter_terrorist'::team_type;
	else
		raise exception 'Invalid team identifier: %s', team;
	end if;
end $$ language 'plpgsql';

create or replace function convert_weapon(weapon text) returns text as $$
begin
	if weapon = 'knife_t' or weapon = 'knife_default_ct' then
		return 'knife';
	else
		return weapon;
	end if;
end $$ language 'plpgsql';

create or replace function process_kill(kill_time timestamp, killer_name text, killer_steam_id text, killer_team text, killer_x integer, killer_y integer, killer_z integer, victim_name text, victim_steam_id text, victim_team text, victim_x integer, victim_y integer, victim_z integer, weapon text, headshot boolean) returns void as $$
declare
	killer_id integer;
	killer_team_enum team_type;
	victim_id integer;
	victim_team_enum team_type;
begin
	select update_player(killer_name, killer_steam_id) into killer_id;
	select update_player(victim_name, victim_steam_id) into victim_id;
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

create or replace function get_player_kills(player_id integer) returns int as $$
declare
	kills integer;
begin
	select count(*) from kill where killer_id = player_id and killer_id != victim_id into kills;
	return kills;
end $$ language 'plpgsql';

create or replace function get_player_deaths(player_id integer) returns int as $$
declare
	deaths integer;
begin
	select count(*) from kill where victim_id = player_id into deaths;
	return deaths;
end $$ language 'plpgsql';

create or replace function get_player_kill_death_ratio(player_id integer) returns numeric as $$
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

create or replace function get_all_player_stats() returns table
(
	id integer,
	name text,
	kills integer,
	deaths integer,
	kill_death_ratio numeric
) as $$
begin
	return query select
		player.id,
		player.name,
		get_player_kills(player.id) as kills,
		get_player_deaths(player.id) as deaths,
		round(get_player_kill_death_ratio(player.id), 2) as kill_death_ratio
	from player;
end $$ language 'plpgsql';

create or replace function get_player_weapon_kills(player_id integer, weapon text, headshots_only boolean) returns integer as $$
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

create or replace function get_player_weapon_headshot_percentage(player_id integer, weapon text) returns numeric as $$
declare
	kills integer;
	headshots integer;
begin
	select get_player_weapon_kills(player_id, weapon, false) into kills;
	select get_player_weapon_kills(player_id, weapon, true) into headshots;
	return headshots::numeric / kills;
end $$ language 'plpgsql';

create or replace function get_player_weapon_stats(player_id integer) returns table
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
		round(get_player_weapon_headshot_percentage(player_id, kill.weapon) * 100, 1) as headshot_percentage
	from kill
	where killer_id = player_id
	group by kill.weapon;
end $$ language 'plpgsql';

create or replace function get_matchup_kills(killer_id integer, victim_id integer) returns int as $$
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

create or replace function get_encounter_win_percentage(player_id integer, opponent_id integer) returns numeric as $$
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
	return round(kills::numeric / encounters * 100, 1);
end $$ language 'plpgsql';

create or replace function get_player_encounter_stats(player_id integer) returns table
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