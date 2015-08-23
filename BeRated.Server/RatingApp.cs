using System;
using System.Collections.Generic;
using System.Linq;
using Ashod.Database;
using BeRated.Database;
using BeRated.Model;
using BeRated.Server;
using Npgsql;

namespace BeRated
{
    public class RatingApp : BaseApp
    {
        private Configuration _Configuration;
        private DatabaseConnection _Database = null;

        public RatingApp(Configuration configuration)
        {
            _Configuration = configuration;
        }

		public static string GetPercentage(decimal ratio)
		{
			return ratio.ToString("P1").Replace(" ", "");
		}

		public override void Dispose()
        {
            if (_Database != null)
            {
                _Database.Dispose();
                _Database = null;
            }
            base.Dispose();
        }

        public void Initialize()
        {
            base.Initialize(_Configuration.TemplatePath);
            var connection = new NpgsqlConnection(_Configuration.ConnectionString);
            _Database = new DatabaseConnection(connection);
        }

        [ServerMethod]
        public List<AllPlayerStatsModel> Index(DateTime? start, DateTime? end, int? days)
        {
			if (days != null)
			{
				var then = DateTime.Now.AddDays((double)-days);
                start = new DateTime(then.Year, then.Month, then.Day);
				end = null;
			}
            lock (_Database)
            {
                var startParameter = new CommandParameter("time_start", start);
                var endParameter = new CommandParameter("time_end", end);
                using (var reader = _Database.ReadFunction("get_all_player_stats", startParameter, endParameter))
                {
                    var rows = reader.ReadAll<AllPlayerStatsRow>();
                    var players = rows.OfType<AllPlayerStatsModel>();
					players = players.OrderBy(player => player.Name);
                    var model = players.ToList();
					return model;
                }
            }
        }

        [ServerMethod]
        public PlayerStatsModel Player(int id, DateTime? start, DateTime? end)
        {
            lock (_Database)
            {
                var playerStats = new PlayerStatsModel();
                playerStats.Id = id;
                var idParameter = new CommandParameter("player_id", id);
                var startParameter = new CommandParameter("time_start", start);
                var endParameter = new CommandParameter("time_end", end);
                playerStats.Name = _Database.ScalarFunction<string>("get_player_name", idParameter);
                using (var reader = _Database.ReadFunction("get_player_weapon_stats", idParameter, startParameter, endParameter))
                {
                    playerStats.Weapons = reader.ReadAll<PlayerWeaponStatsRow>();
                }
                using (var reader = _Database.ReadFunction("get_player_encounter_stats", idParameter, startParameter, endParameter))
                {
                    playerStats.Encounters = reader.ReadAll<PlayerEncounterStatsRow>();
                }
                using (var reader = _Database.ReadFunction("get_player_purchases", idParameter, startParameter, endParameter))
                {
                    playerStats.Purchases = reader.ReadAll<PlayerPurchasesRow>();
                }
                using (var reader = _Database.ReadFunction("get_player_games", idParameter, startParameter, endParameter))
                {
                    var rows = reader.ReadAll<PlayerGameHistoryRow>();
                    playerStats.Games = rows.Select(row => new PlayerGameModel(row)).ToList();
                }
                return playerStats;
            }
        }
    }
}
