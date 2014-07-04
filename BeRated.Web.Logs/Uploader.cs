using Ashod.Database;
using Npgsql;
using System;
using System.Data.SqlClient;
using System.IO;

namespace BeRated
{
	class Uploader: IDisposable
	{
		string _LogPath;
		string _ConnectionString;
		DatabaseCommander _Database;

		public Uploader(string logPath, string connectionString)
		{
			_LogPath = logPath;
			_ConnectionString = connectionString;
		}

		void IDisposable.Dispose()
		{
			if (_Database != null)
			{
				_Database.Dispose();
				_Database = null;
			}
		}

		public void Run()
		{
			var connection = new NpgsqlConnection(_ConnectionString);
			_Database = new DatabaseCommander(connection);
			var files = Directory.GetFiles(_LogPath);
			using (var transaction = _Database.Transaction())
			{
				_Database.NonQueryFunction("lock_tables");
				foreach (var file in files)
					ProcessLog(file);
				transaction.Commit();
			}
		}

		void ProcessLog(string path)
		{
			Console.WriteLine(path);
			var lines = File.ReadLines(path);
			foreach (var line in lines)
				ProcessLine(line);
		}

		void ProcessLine(string line)
		{
			var kill = LogParser.ReadPlayerKill(line);
			if (kill != null)
			{
				if (kill.Killer.SteamId == LogParser.BotId || kill.Victim.SteamId == LogParser.BotId)
					return;
				var parameters = new []
				{
					new CommandParameter("kill_time", kill.Time),
					new CommandParameter("killer_name", kill.Killer.Name),
					new CommandParameter("killer_steam_id", kill.Killer.SteamId),
					new CommandParameter("killer_team", kill.KillerTeam),
					new CommandParameter("killer_x", kill.KillerPosition.X),
					new CommandParameter("killer_y", kill.KillerPosition.Y),
					new CommandParameter("killer_z", kill.KillerPosition.Z),
					new CommandParameter("victim_name", kill.Victim.Name),
					new CommandParameter("victim_steam_id", kill.Victim.SteamId),
					new CommandParameter("victim_team", kill.VictimTeam),
					new CommandParameter("victim_x", kill.VictimPosition.X),
					new CommandParameter("victim_y", kill.VictimPosition.Y),
					new CommandParameter("victim_z", kill.VictimPosition.Z),
					new CommandParameter("weapon", kill.Weapon),
					new CommandParameter("headshot", kill.Headshot),
				};
				_Database.NonQueryFunction("process_kill", parameters);
				return;
			}
		}
	}
}
