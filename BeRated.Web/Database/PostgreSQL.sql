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
	unique (time, killer_id, victim_id)
);

drop table if exists round cascade;

create table round
(
	id serial primary key,
	time timestamp unique not null,
	winner team_type not null,
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
	item text not null,
	primary key (player_id, line, time)
);