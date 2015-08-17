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
    class ServerInstance : IServerInstance
    {
        private DatabaseConnection _Database = null;

        public ServerInstance(string connectionString)
        {
            var connection = new NpgsqlConnection(connectionString);
            _Database = new DatabaseConnection(connection);
        }

        void IDisposable.Dispose()
        {
            if (_Database != null)
            {
                _Database.Dispose();
                _Database = null;
            }
        }

        string IServerInstance.GetMarkup(string json)
        {
            return "<!doctype html>\n<head>\n<title>Title</title>\n<script>var output = JSON.parse('" + json.Replace("'", "\\'") + "'); console.log(output);</script>\n<body>\n</body>\n</html>";
        }

        [ServerMethod]
        public List<AllPlayerStatsModel> GetAllPlayerStats(DateTime? start, DateTime? end)
        {
            lock (_Database)
            {
                var startParameter = new CommandParameter("time_start", start);
                var endParameter = new CommandParameter("time_end", end);
                using (var reader = _Database.ReadFunction("get_all_player_stats", startParameter, endParameter))
                {
                    var rows = reader.ReadAll<AllPlayerStatsRow>();
                    var players = rows.OfType<AllPlayerStatsModel>().ToList();
                    return players;
                }
            }
        }

        [ServerMethod]
        public PlayerStatsModel GetPlayerStats(int playerId, DateTime? start, DateTime? end)
        {
            lock (_Database)
            {
                var playerStats = new PlayerStatsModel();
                playerStats.Id = playerId;
                var idParameter = new CommandParameter("player_id", playerId);
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
