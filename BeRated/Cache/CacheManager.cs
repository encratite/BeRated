using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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

		private Dictionary<string, Player> _Players = new Dictionary<string, Player>();

		private Dictionary<string, Team> _PlayerTeams = null;
		private List<Round> _Rounds = new List<Round>();

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
				if (steamId != LogParser.BotId)
					_Players[steamId] = player;
			}
			return player;
		}

		private void RunReader()
		{
            while (true)
            {
                try
                {
                    var files = Directory.GetFiles(_LogPath).ToList();
					files.Sort();
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
				if (kill.Killer.SteamId == LogParser.BotId || kill.Victim.SteamId == LogParser.BotId || kill.Killer == kill.Victim)
					return;
				kill.Killer.Kills.Add(kill);
				kill.Victim.Deaths.Add(kill);
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
			var round = logParser.ReadEndOfRound(line);
			if (round != null)
			{
				if (round.TerroristScore == 0 && round.CounterTerroristScore == 0)
					return;
				int roundsPlayed = round.TerroristScore + round.CounterTerroristScore;
				if (roundsPlayed == 1)
					_Rounds.Clear();
				_Rounds.Add(round);
				var winningTeam = logParser.GetWinningTeam(round.SfuiNotice);
				foreach (var pair in _PlayerTeams)
				{
					var player = _Players[pair.Key];
					var playerTeam = pair.Value;
					var container = playerTeam == winningTeam ? player.RoundsWon : player.RoundsLost;
					container.Add(round);
				}
				int roundsToWin = _MaxRounds / 2 + 1;
				bool terroristsWinGame = round.TerroristScore >= roundsToWin;
				bool counterTerroristsWinGame = round.CounterTerroristScore >= roundsToWin;
				bool draw = roundsPlayed >= _MaxRounds;
				if (terroristsWinGame || counterTerroristsWinGame || draw)
				{
					var game = new Game(_Rounds);
					foreach (var pair in _PlayerTeams)
					{
						var player = _Players[pair.Key];
						var playerTeam = pair.Value;
						List<Game> container;
						if (draw)
						{
							container = player.Draws;
						}
						else if (
							(terroristsWinGame && playerTeam == Team.Terrorist) ||
							(counterTerroristsWinGame && playerTeam == Team.CounterTerrorist)
						)
						{
							container = player.Wins;
						}
						else
						{
							container = player.Losses;
						}
						container.Add(game);
					}
				}
				return;
			}
			var purchase = logParser.ReadPurchase(line);
			if (purchase != null)
			{
				string steamId = purchase.Player.SteamId;
				if (steamId == LogParser.BotId)
					return;
				purchase.Player.Purchases.Add(purchase);
				return;
			}
		}
	}
}
