using Ashod;
using Ashod.Database;
using BeRated.Model;
using BeRated.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;

namespace BeRated.App
{
	public class RatingApp : BaseApp, IQueryPerformanceLogger
    {
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
        public GeneralStats General(int? days)
        {
            using (var connection = GetConnection())
            {
			    var constraints = new TimeConstraints(days);
                var startParameter = new CommandParameter("time_start", constraints.Start);
                var endParameter = new CommandParameter("time_end", constraints.End);
			    var stats = new GeneralStats
			    {
				    Days = days,
			    };
                using (var reader = connection.ReadFunction("get_all_player_stats", startParameter, endParameter))
                {
				    stats.Players = reader.ReadAll<GeneralPlayerStats>();
                }
			    using (var reader = connection.ReadFunction("get_teams", startParameter, endParameter))
			    {
				    stats.Teams = reader.ReadAll<TeamStats>();
                }
			    return stats;
            }
		}

        [Controller]
        public PlayerStats Player(int id, int? days)
        {
            using (var connection = GetConnection())
            {
                var constraints = new TimeConstraints(days);
                var playerStats = new PlayerStats();
                playerStats.Id = id;
                var idParameter = new CommandParameter("player_id", id);
                var startParameter = new CommandParameter("time_start", constraints.Start);
                var endParameter = new CommandParameter("time_end", constraints.End);
                playerStats.Name = connection.ScalarFunction<string>("get_player_name", idParameter);
			    playerStats.Days = days;
			    using (var reader = connection.ReadFunction("get_player_games", idParameter, startParameter, endParameter))
			    {
				    var games = reader.ReadAll<PlayerGame>();
				    playerStats.Games = games.OrderByDescending(game => game.GameTime).ToList();
			    }
			    using (var reader = connection.ReadFunction("get_player_encounter_stats", idParameter, startParameter, endParameter))
			    {
				    var encounters = reader.ReadAll<PlayerEncounterStats>();
				    playerStats.Encounters = encounters.OrderByDescending(player => player.Encounters).ToList();
			    }
			    using (var reader = connection.ReadFunction("get_player_weapon_stats", idParameter, startParameter, endParameter))
                {
                    var weapons = reader.ReadAll<PlayerWeaponStats>();
				    playerStats.Weapons = weapons.OrderByDescending(weapon => weapon.Kills).ToList();
                }
                using (var reader = connection.ReadFunction("get_player_purchases", idParameter, startParameter, endParameter))
                {
                    var purchases = reader.ReadAll<PlayerItemStats>();
				    playerStats.Purchases = purchases.OrderByDescending(item => item.TimesPurchased).ToList();
                }
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
    }
}
