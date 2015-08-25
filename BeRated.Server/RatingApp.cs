﻿using System;
using System.Collections.Generic;
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
        public AllPlayerStatsModel All(int? days)
        {
			var constraints = new TimeConstraints(days);
            lock (_Database)
            {
                var startParameter = new CommandParameter("time_start", constraints.Start);
                var endParameter = new CommandParameter("time_end", constraints.End);
                using (var reader = _Database.ReadFunction("get_all_player_stats", startParameter, endParameter))
                {
                    var players = reader.ReadAll<GeneralPlayerStatsModel>();
					var sortedPlayers = players.OrderBy(player => player.Name).ToList();
					var stats = new AllPlayerStatsModel
					{
						Days = days,
						Players = sortedPlayers,
					};
					return stats;
                }
            }
        }

        [Controller]
        public PlayerStatsModel Player(int id, int? days)
        {
			var constraints = new TimeConstraints(days);
			lock (_Database)
            {
                var playerStats = new PlayerStatsModel();
                playerStats.Id = id;
                var idParameter = new CommandParameter("player_id", id);
                var startParameter = new CommandParameter("time_start", constraints.Start);
                var endParameter = new CommandParameter("time_end", constraints.End);
                playerStats.Name = _Database.ScalarFunction<string>("get_player_name", idParameter);
                using (var reader = _Database.ReadFunction("get_player_weapon_stats", idParameter, startParameter, endParameter))
                {
                    var weapons = reader.ReadAll<PlayerWeaponStatsModel>();
					playerStats.Weapons = weapons.OrderByDescending(weapon => weapon.Kills).ToList();
                }
                using (var reader = _Database.ReadFunction("get_player_encounter_stats", idParameter, startParameter, endParameter))
                {
                    var encounters = reader.ReadAll<PlayerEncounterStatsModel>();
					playerStats.Encounters = encounters.OrderByDescending(player => player.Encounters).ToList();
                }
                using (var reader = _Database.ReadFunction("get_player_purchases", idParameter, startParameter, endParameter))
                {
                    var purchases = reader.ReadAll<PlayerPurchasesModel>();
					playerStats.Purchases = purchases.OrderByDescending(item => item.TimesPurchased).ToList();
                }
                using (var reader = _Database.ReadFunction("get_player_games", idParameter, startParameter, endParameter))
                {
                    var rows = reader.ReadAll<PlayerGameHistoryModel>();
                    playerStats.Games = rows.Select(row => new PlayerGameModel(row)).ToList();
                }
                return playerStats;
            }
        }
    }
}
