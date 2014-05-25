set client_min_messages to warning;

drop table if exists player cascade;

create table player
(
	id serial primary key,
	steam_id text unique not null,
	-- The latest name of the player
	name text not null
);

create index player_steam_id_index on player (steam_id);

drop table if exists kill cascade;

create table kill
(
	id serial primary key,
	time timestamp not null,
	killer_id integer references player(id) not null,
	victim_id integer references player(id) not null,
	-- False if the killer is a terrorist, true if he is a counter-terrorist
	killer_is_ct boolean not null,
	weapon text not null,
	distance real not null
);

create index kill_killer_id_time_index on kill (killer_id, time desc);
create index kill_victim_id_time_index on kill (victim_id, time desc);