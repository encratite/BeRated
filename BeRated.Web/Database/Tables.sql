set client_min_messages to warning;

drop type if exists team_type cascade;

create type team_type as enum
(
	'terrorist',
	'counter_terrorist'
);

drop type if exists sfui_notice cascade;

create type sfui_notice_type as enum
(
	'all_hostages_rescued',
	'bomb_defused',
	'cts_win',
	'hostages_not_rescued',
	'target_bombed',
	'target_saved',
	'terrorists_win'
);

drop table if exists player cascade;

create table player
(
	id serial primary key,
	-- The latest name of the player
	name text not null,
	steam_id text unique not null
);

drop table if exists kill cascade;

create table kill
(
	time timestamp not null,
	killer_id integer references player (id) not null,
	killer_team team_type not null,
	killer_vector integer[3],
	victim_id integer references player (id) not null,
	victim_team team_type not null,
	victim_vector integer[3],
	weapon text not null,
	headshot boolean not null,
	primary key (time, killer_id, victim_id)
);

drop table if exists round cascade;

create table round
(
	id serial primary key,
	time timestamp unique not null,
	triggering_team team_type not null,
	sfui_notice sfui_notice_type not null,
	terrorist_score integer not null,
	counter_terrorist_score integer not null,
	max_rounds integer not null
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
	player_id integer references player (id) not null,
	-- The line in the log file the purchase was extracted from
	-- Hack to make the primary key work
	line integer not null,
	time timestamp not null,
	team team_type not null,
	item text not null,
	primary key (line, time)
);