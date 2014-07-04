set client_min_messages to warning;

drop table if exists player cascade;

create table player
(
	id serial primary key,
	steam_id text unique not null,
	-- The latest name of the player
	name text not null
);

drop type if exists team_type cascade;

create type team_type as enum
(
	'terrorist',
	'counter_terrorist'
);

drop table if exists kill cascade;

create table kill
(
	id serial primary key,
	time timestamp not null,
	killer_id integer references player (id) not null,
	killer_team team_type not null,
	killer_vector integer[3],
	victim_id integer references player (id) not null,
	victim_team team_type not null,
	victim_vector integer[3],
	weapon text not null,
	headshot boolean not null,
	unique (time, killer_id, victim_id)
);

drop table if exists round cascade;

create table round
(
	id serial primary key,
	time timestamp unique not null,
	winner team_type not null,
	final_round boolean not null,
	terrorist_score integer not null,
	counter_terrorist_score integer not null
);

drop table if exists round_player cascade;

create table round_player
(
	round_id integer references round (id) not null,
	player_id integer references player (id) not null,
	team team_type not null,
	
	primary key (round_id, player_id)
);

drop table if exists purchase cascade;

create table purchase
(
	player_id integer references player(id) not null,
	-- The line in the log file the purchase was extracted from
	-- Required for unique constraint
	line integer not null,
	time timestamp not null,
	team team_type not null,
	item text not null,
	primary key (line, time)
);

create or replace function lock_tables() returns void as
$$
begin
	lock player;
	lock kill;
	lock round;
	lock round_player;
	lock purchase;
end
$$
language 'plpgsql';

create or replace function update_player(name text, steam_id text) returns integer as
$$
declare
	player_id integer;
begin
	update player set name = update_player.name where player.steam_id = update_player.steam_id returning id into player_id;
	if not found then
		insert into player (steam_id, name) values (steam_id, name) returning id into player_id;
	end if;
	return player_id;
end
$$
language 'plpgsql';

create or replace function get_team(team text) returns team_type as
$$
declare
begin
	if team = 'TERRORIST' then
		return 'terrorist'::team_type;
	elsif team = 'CT' then
		return 'counter_terrorist'::team_type;
	else
		raise exception 'Invalid team identifier: %s', team;
	end if;
end
$$
language 'plpgsql';

create or replace function process_kill(kill_time timestamp, killer_name text, killer_steam_id text, killer_team text, killer_x integer, killer_y integer, killer_z integer, victim_name text, victim_steam_id text, victim_team text, victim_x integer, victim_y integer, victim_z integer, weapon text, headshot boolean) returns void as
$$
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
			weapon,
			headshot
		);
	exception when unique_violation then
	end;
end
$$
language 'plpgsql';