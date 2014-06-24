using System;
using System.Data.SqlClient;
using System.IO;

namespace BeRated
{
	class Uploader: IDisposable
	{
		string _LogPath;
		string _DataSource;
		string _Database;

		SqlConnection _Connection = null;

		public Uploader(string logPath, string dataSource, string database)
		{
			_LogPath = logPath;
			_DataSource = dataSource;
			_Database = database;
		}

		void IDisposable.Dispose()
		{
			if (_Connection != null)
			{
				_Connection.Dispose();
				_Connection = null;
			}
		}

		public void Run()
		{
			var builder = new SqlConnectionStringBuilder
			{
				DataSource = _DataSource,
				InitialCatalog = _Database,
				IntegratedSecurity = true,
			};
			_Connection = new SqlConnection(builder.ConnectionString);
			_Connection.Open();

			var files = Directory.GetFiles(_LogPath);
			foreach (var file in files)
				ProcessLog(file);
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
			if (kill == null)
				return;
			if (kill.Killer.SteamId == LogParser.BotId || kill.Victim.SteamId == LogParser.BotId || kill.Killer.SteamId == kill.Victim.SteamId)
				return;
			string query =
				@"insert into player_kill (
					time,
					killer_name,
					killer_steam_id,
					killer_team,
					killer_x,
					killer_y,
					killer_z,
					victim_name,
					victim_steam_id,
					victim_team,
					victim_x,
					victim_y,
					victim_z,
					headshot,
					weapon
				)
				values (
					@time,
					@killerName,
					@killerSteamId,
					@killerTeam,
					@killerX,
					@killerY,
					@killerZ,
					@victimName,
					@victimSteamId,
					@victimTeam,
					@victimX,
					@victimY,
					@victimZ,
					@headshot,
					@weapon
				)";
			using (var command = new SqlCommand(query, _Connection))
			{
				command.Parameters.AddRange(new SqlParameter[]
					{
						new SqlParameter("@time", kill.Time),
						new SqlParameter("@killerName", kill.Killer.Name),
						new SqlParameter("@killerSteamId", kill.Killer.SteamId),
						new SqlParameter("@killerTeam", kill.KillerTeam),
						new SqlParameter("@killerX", kill.KillerPosition.X),
						new SqlParameter("@killerY", kill.KillerPosition.Y),
						new SqlParameter("@killerZ", kill.KillerPosition.Z),
						new SqlParameter("@victimName", kill.Victim.Name),
						new SqlParameter("@victimSteamId", kill.Victim.SteamId),
						new SqlParameter("@victimTeam", kill.VictimTeam),
						new SqlParameter("@victimX", kill.VictimPosition.X),
						new SqlParameter("@victimY", kill.VictimPosition.Y),
						new SqlParameter("@victimZ", kill.VictimPosition.Z),
						new SqlParameter("@headshot", kill.Headshot),
						new SqlParameter("@weapon", kill.Weapon),
					}
				);
				try
				{
					command.ExecuteNonQuery();
				}
				catch (SqlException)
				{
				}
			}
		}
	}
}
