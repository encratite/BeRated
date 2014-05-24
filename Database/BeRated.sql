set client_min_messages to warning;

drop table if exists player cascade;

create table player
(
	id serial primary key,
	steam_id text unique not null,
	-- The latest name of the player
	name text not null,
	rating integer not null
);

drop type if exists player_team cascade;

create type player_team as enum
(
	'terrorist',
	'counter_terrorist'
);

drop table if exists kill cascade;

create table kill
(
	id serial primary key,
	time timestamp not null,
	killer_id integer references player(id) not null,
	killer_old_rating integer not null,
	killer_new_rating integer not null,
	victim_id integer references player(id) not null,
	victim_old_rating integer not null,
	victim_new_rating integer not null,
	killer_team player_team not null,
	weapon text not null,
	distance real not null
);