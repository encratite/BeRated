using Ashod.Database;
using Ashod.WebSocket;
using BeRated.Query;
using Npgsql;
using System;
using System.Collections.Generic;

namespace BeRated
{
	class Server : WebSocketServer, IDisposable
	{
		string _ConnectionString;
		DatabaseCommander _Database = null;

		public Server(int webSocketPort, string connectionString)
			: base(webSocketPort)
		{
			_ConnectionString = connectionString;
		}

		public override void Run()
		{
			base.Run();
			var connection = new NpgsqlConnection(_ConnectionString);
			_Database = new DatabaseCommander(connection);
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

		#region Web socket methods

		[WebSocketServerMethod]
		List<AllPlayerStatsRow> GetAllPlayerStats()
		{
			using (var reader = _Database.ReadFunction("get_all_player_stats"))
			{
				var players = reader.ReadAll<AllPlayerStatsRow>();
				return players;
			}
		}

		[WebSocketServerMethod]
		PlayerStats GetPlayerStats(int playerId)
		{
			var playerStats = new PlayerStats();
			var idParameter = new CommandParameter("player_id", playerId);
			using (var reader = _Database.ReadFunction("get_player_weapon_stats", idParameter))
			{
				playerStats.Weapons = reader.ReadAll<PlayerWeaponStatsRow>();
			}
			using (var reader = _Database.ReadFunction("get_player_encounter_stats", idParameter))
			{
				playerStats.Encounters = reader.ReadAll<PlayerEncounterStatsRow>();
			}
			return playerStats;
		}

		#endregion
	}
}
