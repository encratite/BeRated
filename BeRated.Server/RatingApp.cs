using System.Linq;
using Ashod.Database;
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

		public static string GetPlayerPath(int id, int? days)
		{
			string path = string.Format("/Player?id={0}", id);
			if (days.HasValue)
				path += string.Format("&days={0}", days.Value);
			return path;
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
            base.Initialize(_Configuration.ViewPath);
            var connection = new NpgsqlConnection(_Configuration.ConnectionString);
            _Database = new DatabaseConnection(connection);
        }

        [Controller]
        public AllPlayerStats All(int? days)
        {
			var constraints = new TimeConstraints(days);
            lock (_Database)
            {
                var startParameter = new CommandParameter("time_start", constraints.Start);
                var endParameter = new CommandParameter("time_end", constraints.End);
                using (var reader = _Database.ReadFunction("get_all_player_stats", startParameter, endParameter))
                {
                    var players = reader.ReadAll<GeneralPlayerStats>();
					var sortedPlayers = players.OrderBy(player => player.Name).ToList();
					var stats = new AllPlayerStats
					{
						Days = days,
						Players = sortedPlayers,
					};
					return stats;
                }
            }
        }

        [Controller]
        public PlayerStats Player(int id, int? days)
        {
			var constraints = new TimeConstraints(days);
			lock (_Database)
            {
                var playerStats = new PlayerStats();
                playerStats.Id = id;
                var idParameter = new CommandParameter("player_id", id);
                var startParameter = new CommandParameter("time_start", constraints.Start);
                var endParameter = new CommandParameter("time_end", constraints.End);
                playerStats.Name = _Database.ScalarFunction<string>("get_player_name", idParameter);
				playerStats.Days = days;
                using (var reader = _Database.ReadFunction("get_player_weapon_stats", idParameter, startParameter, endParameter))
                {
                    var weapons = reader.ReadAll<PlayerWeaponStats>();
					playerStats.Weapons = weapons.OrderByDescending(weapon => weapon.Kills).ToList();
                }
                using (var reader = _Database.ReadFunction("get_player_purchases", idParameter, startParameter, endParameter))
                {
                    var purchases = reader.ReadAll<PlayerItemStats>();
					playerStats.Purchases = purchases.OrderByDescending(item => item.TimesPurchased).ToList();
                }
				using (var reader = _Database.ReadFunction("get_player_encounter_stats", idParameter, startParameter, endParameter))
				{
					var encounters = reader.ReadAll<PlayerEncounterStats>();
					playerStats.Encounters = encounters.OrderByDescending(player => player.Encounters).ToList();
				}
				using (var reader = _Database.ReadFunction("get_player_games", idParameter, startParameter, endParameter))
                {
                    var games = reader.ReadAll<PlayerGame>();
					playerStats.Games = games.OrderBy(game => game.GameTime).ToList();
                }
                return playerStats;
            }
        }
    }
}
