using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ashod.Database;

namespace BeRated.Cache
{
	class CacheManager : IDisposable
	{
		private const int MaxRoundsDefault = 30;
        private const int UpdateInterval = 10 * 1000;

		private string _LogPath;
		private string _ConnectionString;
		private int _MaxRounds = MaxRoundsDefault;

		private Thread _ReaderThread = null;

		private Dictionary<string, long> _LogStates = new Dictionary<string, long>();

		private Dictionary<string, Team> _PlayerTeams = null;

		private Dictionary<string, Player> _Players = new Dictionary<string, Player>();

		public CacheManager(string logPath, string connectionString)
		{
			_LogPath = logPath;
			_ConnectionString = connectionString;
		}

		public void Dispose()
		{
			if (_ReaderThread != null)
			{
				_ReaderThread.Abort();
				_ReaderThread = null;
			}
		}

		public void Run()
		{
			_ReaderThread = new Thread(RunReader);
			_ReaderThread.Start();
		}

		public Player GetPlayer(string name, string steamId)
		{
			Player player;
			if (_Players.TryGetValue(steamId, out player))
			{
				// Update name, just in case
				player.Name = name;
			}
			else
			{
				player = new Player(name, steamId);
			}
			return player;
		}

		private void RunReader()
		{
            while (true)
            {
                try
                {
                    var files = Directory.GetFiles(_LogPath);
                    foreach (var file in files)
                        ProcessLog(file);
                }
                catch(Exception exception)
                {
                    Console.WriteLine("Failed to update database: {0} ({1})", exception.Message, exception.GetType());
                }
                Thread.Sleep(UpdateInterval);
            }
		}

		private void ProcessLog(string path)
		{
			lock (this)
			{
				_MaxRounds = MaxRoundsDefault;
				_PlayerTeams = new Dictionary<string, Team>();
				string fileName = Path.GetFileName(path);
				var fileInfo = new FileInfo(path);
				long currentFileSize = fileInfo.Length;
				long bytesProcessed;
				if (_LogStates.TryGetValue(fileName, out bytesProcessed) && bytesProcessed >= currentFileSize)
				{
					// This file has already been processed
					return;
				}
				var content = File.ReadAllText(path);
				content = content.Replace("\r", "");
				if (content.Length == 0 || content.Last() != '\n')
				{
					// The log file is currently being written to or has been abandoned, skip it
					return;
				}
				Console.WriteLine("{0} Processing {1}", DateTime.Now, path);
				var lines = content.Split('\n');
				int lineCounter = 1;
				foreach (var line in lines)
				{
					ProcessLine(line, lineCounter);
					lineCounter++;
				}
				_LogStates[fileName] = currentFileSize;
			}
		}

		private void ProcessLine(string line, int lineCounter)
		{
			var logParser = new LogParser(this);
			var kill = logParser.ReadPlayerKill(line);
			if (kill != null)
			{
				if (kill.Killer.SteamId == LogParser.BotId || kill.Victim.SteamId == LogParser.BotId)
					return;
				var parameters = new []
				{
					new CommandParameter("kill_time", kill.Time),
					new CommandParameter("killer_steam_id", kill.Killer.SteamId),
					new CommandParameter("killer_team", kill.KillerTeam),
					new CommandParameter("killer_x", kill.KillerPosition.X),
					new CommandParameter("killer_y", kill.KillerPosition.Y),
					new CommandParameter("killer_z", kill.KillerPosition.Z),
					new CommandParameter("victim_steam_id", kill.Victim.SteamId),
					new CommandParameter("victim_team", kill.VictimTeam),
					new CommandParameter("victim_x", kill.VictimPosition.X),
					new CommandParameter("victim_y", kill.VictimPosition.Y),
					new CommandParameter("victim_z", kill.VictimPosition.Z),
					new CommandParameter("weapon", kill.Weapon),
					new CommandParameter("headshot", kill.Headshot),
				};
				// _Database.NonQueryFunction("process_kill", parameters);
				return;
			}
			int? maxRounds = logParser.ReadMaxRounds(line);
			if (maxRounds != null)
			{
				_MaxRounds = maxRounds.Value;
				return;
			}
			var teamSwitch = logParser.ReadTeamSwitch(line);
			if (teamSwitch != null)
			{
				string steamId = teamSwitch.Player.SteamId;
				var team = teamSwitch.CurrentTeam;
				if (steamId == LogParser.BotId)
					return;
				_PlayerTeams[steamId] = team;
				var parameters = new[]
				{
					new CommandParameter("name", teamSwitch.Player.Name),
					new CommandParameter("steam_id", steamId),
                    new CommandParameter("_time", teamSwitch.Time),
                };
				// _Database.NonQueryFunction("update_player", parameters);
				return;
			}
			var disconnect = logParser.ReadDisconnect(line);
			if (disconnect != null)
			{
				string steamId = disconnect.Player.SteamId;
				if (steamId == LogParser.BotId)
					return;
				_PlayerTeams.Remove(steamId);
			}
			var endOfRound = logParser.ReadEndOfRound(line);
			if (endOfRound != null)
			{
				if (endOfRound.TerroristScore == 0 && endOfRound.CounterTerroristScore == 0)
					return;
				string terroristIds = GetSteamIdsString(LogParser.TerroristTeam);
				string counterTerroristIds = GetSteamIdsString(LogParser.CounterTerroristTeam);
				var parameters = new[]
				{
					new CommandParameter("end_of_round_time", endOfRound.Time),
					new CommandParameter("triggering_team", endOfRound.TriggeringTeam),
					new CommandParameter("sfui_notice", endOfRound.SfuiNotice),
					new CommandParameter("terrorist_score", endOfRound.TerroristScore),
					new CommandParameter("counter_terrorist_score", endOfRound.CounterTerroristScore),
					new CommandParameter("max_rounds", _MaxRounds),
					new CommandParameter("terrorist_steam_ids", terroristIds),
					new CommandParameter("counter_terrorist_steam_ids", counterTerroristIds),
				};
				// _Database.NonQueryFunction("process_end_of_round", parameters);
				return;
			}
			var purchase = logParser.ReadPurchase(line);
			if (purchase != null)
			{
				string steamId = purchase.Player.SteamId;
				if (steamId == LogParser.BotId)
					return;
				var team = _PlayerTeams[steamId];
				var parameters = new[]
				{
					new CommandParameter("steam_id", steamId),
					new CommandParameter("line", lineCounter),
					new CommandParameter("purchase_time", purchase.Time),
					new CommandParameter("team", team),
					new CommandParameter("item", purchase.Item),
				};
				// _Database.NonQueryFunction("process_purchase", parameters);
				return;
			}
		}

		private string GetSteamIdsString(string team)
		{
			throw new NotImplementedException();
		}
	}
}
