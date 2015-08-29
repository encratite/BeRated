using System;
using System.Collections.Generic;
using System.Linq;
using Ashod.Database;
using BeRated.Model;
using BeRated.Server;
using Npgsql;

namespace BeRated.App
{
	public class RatingApp : BaseApp
    {
        private Configuration _Configuration;
        private DatabaseConnection _Database = null;

        public RatingApp(Configuration configuration)
        {
            _Configuration = configuration;
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
        public GeneralStats General(int? days)
        {
			var constraints = new TimeConstraints(days);
            var startParameter = new CommandParameter("time_start", constraints.Start);
            var endParameter = new CommandParameter("time_end", constraints.End);
			var stats = new GeneralStats
			{
				Days = days,
			};
            using (var reader = _Database.ReadFunction("get_all_player_stats", startParameter, endParameter))
            {
				stats.Players = reader.ReadAll<GeneralPlayerStats>();
            }
			using (var reader = _Database.ReadFunction("get_teams", startParameter, endParameter))
			{
				stats.Teams = reader.ReadAll<TeamStats>();
			}
			return stats;
		}

        [Controller]
        public PlayerStats Player(int id, int? days)
        {
			var constraints = new TimeConstraints(days);
            var playerStats = new PlayerStats();
            playerStats.Id = id;
            var idParameter = new CommandParameter("player_id", id);
            var startParameter = new CommandParameter("time_start", constraints.Start);
            var endParameter = new CommandParameter("time_end", constraints.End);
            playerStats.Name = _Database.ScalarFunction<string>("get_player_name", idParameter);
			playerStats.Days = days;
			using (var reader = _Database.ReadFunction("get_player_games", idParameter, startParameter, endParameter))
			{
				var games = reader.ReadAll<PlayerGame>();
				playerStats.Games = games.OrderByDescending(game => game.GameTime).ToList();
			}
			using (var reader = _Database.ReadFunction("get_player_encounter_stats", idParameter, startParameter, endParameter))
			{
				var encounters = reader.ReadAll<PlayerEncounterStats>();
				playerStats.Encounters = encounters.OrderByDescending(player => player.Encounters).ToList();
			}
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
            return playerStats;
        }

		[Controller]
		public TeamMatchupStats Matchup(string team1, string team2)
		{
			Func<string, List<PlayerInfo>> readPlayers = (playerIdString) =>
			{
				var playerIdParameter = new CommandParameter("player_id_string", playerIdString);
				using (var reader = _Database.ReadFunction("get_player_names", playerIdParameter))
				{
					return reader.ReadAll<PlayerInfo>();
				}
			};
			Func<bool, GameOutcomes> readOutcomes = (precise) =>
			{
				var teamParameter1 = new CommandParameter("player_id_string1", team1);
				var teamParameter2 = new CommandParameter("player_id_string2", team2);
				var preciseParameter = new CommandParameter("precise", precise);
				using (var reader = _Database.ReadFunction("get_matchup_stats", teamParameter1, teamParameter2, preciseParameter))
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
}
