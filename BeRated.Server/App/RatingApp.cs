using System;
using System.Collections.Generic;
using System.Linq;
using Ashod;
using Ashod.Database;
using BeRated.Model;
using BeRated.Model.Encounter;
using BeRated.Model.General;
using BeRated.Model.Item;
using BeRated.Model.Weapon;
using BeRated.Server;
using Microsoft.Owin;
using Npgsql;

namespace BeRated.App
{
	public class RatingApp : BaseApp, IQueryPerformanceLogger
    {
		private readonly static StatsTimeSpan[] _TimeSpans = new []
		{
			new StatsTimeSpan(0, "Today"),
			new StatsTimeSpan(30, "Past 30 days"),
			new StatsTimeSpan(null, "All stats"),
		};

        private Configuration _Configuration;

		private Dictionary<string, CacheEntry> _Cache = new Dictionary<string, CacheEntry>();

        public RatingApp(Configuration configuration)
        {
            _Configuration = configuration;
        }

		public override string GetCachedResponse(IOwinContext context)
		{
			CacheEntry entry;
			if (!_Cache.TryGetValue(context.Request.Uri.PathAndQuery, out entry))
				return null;
			using (var connection = GetConnection())
			{
				var time = connection.ScalarFunction<DateTime>("get_time_of_most_recent_kill");
				if (time > entry.Time)
					return null;
				return entry.Markup;
			}
		}

		public override void OnResponse(IOwinContext context, string markup)
		{
			_Cache[context.Request.Uri.PathAndQuery] = new CacheEntry(markup);
			int maximumCacheSize = _Configuration.CacheSize.Value * 1024 * 1024;
			int cacheSize = 0;
			foreach (var pair in _Cache)
				cacheSize += pair.Value.Markup.Length;
			var pairs = _Cache.OrderBy(pair => pair.Value.Time).ToList();
			while (cacheSize > maximumCacheSize && pairs.Any())
			{
				var pair = pairs.First();
				pairs.RemoveAt(0);
				_Cache.Remove(pair.Key);
				cacheSize -= pair.Value.Markup.Length;
			}
		}

        public void Initialize()
        {
            base.Initialize(_Configuration.ViewPath);
        }

        void IQueryPerformanceLogger.OnQueryEnd(string query, TimeSpan timeSpan)
        {
            string message = string.Format("Executed query in {0} ms: {1}", timeSpan.TotalMilliseconds, query);
            if (timeSpan.TotalMilliseconds < 250)
                Logger.Log(message);
            else if (timeSpan.TotalMilliseconds < 1000)
                Logger.Warning(message);
            else
                Logger.Error(message);
        }

        [Controller]
        public List<TimeSpanGeneralPlayerStats> Players()
        {
			using (var connection = GetConnection())
			{
				var output = _TimeSpans.Select((statsTimeSpan) =>
				{
					var constraints = new TimeConstraints(statsTimeSpan.Days);
					var stats = new TimeSpanGeneralPlayerStats
					{
						TimeSpan = statsTimeSpan
					};
					using (var reader = connection.ReadFunction("get_all_player_stats", constraints.StartParameter, constraints.EndParameter))
					{
						stats.Players = reader.ReadAll<GeneralPlayerStats>();
					}
					return stats;
				}).ToList();
				return output;
			}
		}

		[Controller]
        public List<TimeSpanTeamStats> Teams()
        {
			using (var connection = GetConnection())
			{
				var output = _TimeSpans.Select((timeSpan) =>
				{
					var constraints = new TimeConstraints(timeSpan.Days);
					var stats = new TimeSpanTeamStats
					{
						TimeSpan = timeSpan
					};
					using (var reader = connection.ReadFunction("get_teams", constraints.StartParameter, constraints.EndParameter))
					{
						stats.Stats = reader.ReadAll<TeamStats>();
					}
					return stats;
				}).ToList();
				return output;
			}
		}

        [Controller]
        public PlayerGameStats Games(int id)
        {
            using (var connection = GetConnection())
			{
				var playerStats = CreatePlayerStats<PlayerGameStats>(connection, id);
				var idParameter = GetIdParameter(id);
				using (var reader = connection.ReadFunction("get_player_games", idParameter))
				{
					var games = reader.ReadAll<PlayerGame>();
					playerStats.Games = games.OrderByDescending(game => game.GameTime).ToList();
				}
				return playerStats;
			}
		}

		[Controller]
        public AllPlayerEncounterStats Encounters(int id)
        {
            using (var connection = GetConnection())
			{
				var playerStats = CreatePlayerStats<AllPlayerEncounterStats>(connection, id);
				playerStats.Encounters = _TimeSpans.Select((timeSpan) =>
				{
					var constraints = new TimeConstraints(timeSpan.Days);
					var encounterStats = new TimeSpanPlayerEncounterStats
					{
						TimeSpan = timeSpan,
					};
					var idParameter = GetIdParameter(id);
					using (var reader = connection.ReadFunction("get_player_encounter_stats", idParameter, constraints.StartParameter, constraints.EndParameter))
					{
						var encounters = reader.ReadAll<PlayerEncounterStats>();
						encounterStats.Encounters = encounters.OrderByDescending(player => player.Encounters).ToList();
					}
					return encounterStats;
				}).ToList();
				return playerStats;
			}
		}

		[Controller]
        public AllPlayerWeaponStats Weapons(int id)
        {
            using (var connection = GetConnection())
			{
				var playerStats = CreatePlayerStats<AllPlayerWeaponStats>(connection, id);
				playerStats.Weapons = _TimeSpans.Select((timeSpan) =>
				{
					var constraints = new TimeConstraints(timeSpan.Days);
					var weaponStats = new TimeSpanPlayerWeaponStats
					{
						TimeSpan = timeSpan,
					};
					var idParameter = GetIdParameter(id);
					using (var reader = connection.ReadFunction("get_player_weapon_stats", idParameter, constraints.StartParameter, constraints.EndParameter))
					{
						var weapons = reader.ReadAll<PlayerWeaponStats>();
						weaponStats.Weapons = weapons.OrderByDescending(weapon => weapon.Kills).ToList();
					}
					return weaponStats;
				}).ToList();
				return playerStats;
			}
		}

		[Controller]
        public AllPlayerItemStats Purchases(int id)
        {
			using (var connection = GetConnection())
			{
				var playerStats = CreatePlayerStats<AllPlayerItemStats>(connection, id);
				playerStats.Items = _TimeSpans.Select((timeSpan) =>
				{
					var constraints = new TimeConstraints(timeSpan.Days);
					var itemStats = new TimeSpanPlayerItemStats
					{
						TimeSpan = timeSpan,
					};
					var idParameter = GetIdParameter(id);
					using (var reader = connection.ReadFunction("get_player_purchases", idParameter, constraints.StartParameter, constraints.EndParameter))
					{
						var items = reader.ReadAll<PlayerItemStats>();
						itemStats.Purchases = items.OrderByDescending(weapon => weapon.TimesPurchased).ToList();
					}
					return itemStats;
				}).ToList();
				return playerStats;
			}
		}

		[Controller]
		public TeamMatchupStats Matchup(string team1, string team2)
		{
            using (var connection = GetConnection())
            {
                Func<string, List<PlayerInfo>> readPlayers = (playerIdString) =>
			    {
				    var playerIdParameter = new CommandParameter("player_id_string", playerIdString);
				    using (var reader = connection.ReadFunction("get_player_names", playerIdParameter))
				    {
					    return reader.ReadAll<PlayerInfo>();
				    }
			    };
			    Func<bool, GameOutcomes> readOutcomes = (precise) =>
			    {
				    var teamParameter1 = new CommandParameter("player_id_string1", team1);
				    var teamParameter2 = new CommandParameter("player_id_string2", team2);
				    var preciseParameter = new CommandParameter("precise", precise);
				    using (var reader = connection.ReadFunction("get_matchup_stats", teamParameter1, teamParameter2, preciseParameter))
				    {
					    return reader.ReadAll<GameOutcomes>().First();
				    }
			    };
			    var matchup = new TeamMatchupStats
			    {
				    Team1 = readPlayers(team1),
				    Team2 = readPlayers(team2),
				    ImpreciseOutcomes = readOutcomes(false),
				    PreciseOutcomes = readOutcomes(true),
			    };
			    return matchup;
            }
		}

        private DatabaseConnection GetConnection()
        {
            var sqlConnection = new NpgsqlConnection(_Configuration.ConnectionString);
            var connection = new DatabaseConnection(sqlConnection, this);
            return connection;
        }

		private CommandParameter GetIdParameter(int id)
		{
			return new CommandParameter("player_id", id);
		}

		private string GetPlayerName(DatabaseConnection connection, int id)
		{
			var idParameter = GetIdParameter(id);
			return connection.ScalarFunction<string>("get_player_name", idParameter);
		}
		
		private PlayerStatsType CreatePlayerStats<PlayerStatsType>(DatabaseConnection connection, int id)
			where PlayerStatsType : BasePlayerStats, new()
		{
			var playerStats = new PlayerStatsType()
			{
				Id = id,
				Name = GetPlayerName(connection, id)
			};
			return playerStats;
		}
    }
}
