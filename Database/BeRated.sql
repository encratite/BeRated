if object_id('dbo.player_kill') is not null
	drop table player_kill
go

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
go

if object_id('get_user_name') is not null
	drop function get_user_name
go

create function get_user_name
(
	@steam_id nvarchar(128)
)
returns nvarchar(128)
begin
	return (select top 1 killer_name from player_kill where killer_steam_id = @steam_id)
end
go

if object_id('player_statistics') is not null
	drop view player_statistics
go

create view player_statistics as
	select dbo.get_user_name(kill_subquery.steam_id) as name, kill_subquery.steam_id as steam_id, kill_subquery.kills as kills, death_subquery.deaths as deaths, headshot_subquery.headshots as headshots, convert(decimal(10, 2), (cast(kills as real) / deaths)) as kill_death_ratio, convert(decimal(10, 1), ((cast(headshots as real) / kills) * 100)) as headshot_percentage from
		(select killer_steam_id as steam_id, count(*) as kills from player_kill group by killer_steam_id) as kill_subquery,
		(select killer_steam_id as steam_id, count(*) as headshots from player_kill where headshot = 1 group by killer_steam_id) as headshot_subquery,
		(select victim_steam_id as steam_id, count(*) as deaths from player_kill group by victim_steam_id) as death_subquery
	where
		kill_subquery.steam_id = headshot_subquery.steam_id and kill_subquery.steam_id = death_subquery.steam_id
go