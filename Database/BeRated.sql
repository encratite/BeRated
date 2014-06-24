if object_id('dbo.player_kill') is not null
	drop table player_kill

create table player_kill
(
	time datetime not null,
	killer_name nvarchar(128) not null,
	killer_steam_id varchar(32) not null,
	killer_team varchar(16) not null,
	killer_x integer not null,
	killer_y integer not null,
	killer_z integer not null,
	victim_name nvarchar(128) not null,
	victim_steam_id varchar(32) not null,
	victim_team varchar(16) not null,
	victim_x integer not null,
	victim_y integer not null,
	victim_z integer not null,
	headshot bit not null,
	weapon varchar(16) not null
)

alter table player_kill add constraint player_kill_unique unique (time, killer_steam_id, victim_steam_id)