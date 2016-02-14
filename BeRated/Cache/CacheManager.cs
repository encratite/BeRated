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

        public IEnumerable<Player> Players
        {
            get
            {
                return _Players.Values;
            }
        }

        public IEnumerable<Game> Games
        {
            get
            {
                return _Games.AsReadOnly();
            }
        }

		public Action OnUpdate { get; set; }

		private string _LogPath;
		private string _ConnectionString;
		private int _MaxRounds = MaxRoundsDefault;

		private Thread _ReaderThread = null;

		private Dictionary<string, long> _LogStates = new Dictionary<string, long>();

		private Dictionary<string, Player> _Players = new Dictionary<string, Player>();

        private List<Game> _Games = new List<Game>();

		private Dictionary<string, Team> _PlayerTeams = null;
		private List<Round> _Rounds = new List<Round>();

        private LogParser _LogParser;

		public CacheManager(string logPath, string connectionString)
		{
			_LogPath = logPath;
			_ConnectionString = connectionString;
            _LogParser = new LogParser(this);
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

        public Player GetPlayer(string steamId)
        {
            Player player;
            if (!_Players.TryGetValue(steamId, out player))
                throw new ArgumentException("Unknown Steam ID");
            return player;
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
					ProcessLine(line);
					lineCounter++;
				}
				_LogStates[fileName] = currentFileSize;
			}
		}

		private void ProcessLine(string line)
        {
            var readers = new Func<string, bool>[]
            {
                ReadPlayerKill,
                ReadMaxRounds,
                ReadTeamSwitch,
                ReadDisconnect,
                ReadEndOfRound,
                ReadPurchase
            };
            foreach (var reader in readers)
            {
                bool done = reader(line);
                if (done)
                    break;
            }
			if (OnUpdate != null)
				OnUpdate();
		}

        #region Line processing methods

        private bool ReadPlayerKill(string line)
        {
            var kill = _LogParser.ReadPlayerKill(line);
			if (kill == null)
                return false;
			if (kill.Killer.SteamId == LogParser.BotId || kill.Victim.SteamId == LogParser.BotId || kill.Killer == kill.Victim)
				return true;
			kill.Killer.Kills.Add(kill);
			kill.Victim.Deaths.Add(kill);
			return true;
        }

        private bool ReadMaxRounds(string line)
        {
            int? maxRounds = _LogParser.ReadMaxRounds(line);
			if (maxRounds == null)
                return false;
			_MaxRounds = maxRounds.Value;
            return true;
        }

        private bool ReadTeamSwitch(string line)
        {
            var teamSwitch = _LogParser.ReadTeamSwitch(line);
			if (teamSwitch == null)
                return false;
			string steamId = teamSwitch.Player.SteamId;
			var team = teamSwitch.CurrentTeam;
			if (steamId != LogParser.BotId)
			    _PlayerTeams[steamId] = team;
            return true;
        }

        private bool ReadDisconnect(string line)
        {
            var disconnect = _LogParser.ReadDisconnect(line);
			if (disconnect == null)
                return false;
			string steamId = disconnect.Player.SteamId;
			if (steamId != LogParser.BotId)
				_PlayerTeams.Remove(steamId);
			return true;
        }

        private bool ReadEndOfRound(string line)
        {
            var round = _LogParser.ReadEndOfRound(line);
            if (round == null || round.TerroristScore == 0 && round.CounterTerroristScore == 0)
                return false;
            int roundsPlayed = round.TerroristScore + round.CounterTerroristScore;
            if (roundsPlayed == 1)
                _Rounds.Clear();
            _Rounds.Add(round);
            var winningTeam = _LogParser.GetWinningTeam(round.SfuiNotice);
            foreach (var pair in _PlayerTeams)
            {
                var player = _Players[pair.Key];
                var playerTeam = pair.Value;
                var container = playerTeam == winningTeam ? player.RoundsWon : player.RoundsLost;
                container.Add(round);
            }
            CheckForEndOfGame(round, roundsPlayed);
            return true;
        }

        private bool ReadPurchase(string line)
        {
            var purchase = _LogParser.ReadPurchase(line);
			if (purchase == null)
                return false;
			string steamId = purchase.Player.SteamId;
			if (steamId != LogParser.BotId)
			    purchase.Player.Purchases.Add(purchase);
            return true;
        }

        #endregion

        private void CheckForEndOfGame(Round round, int roundsPlayed)
        {
            int roundsToWin = _MaxRounds / 2 + 1;
            bool terroristsWinGame = round.TerroristScore >= roundsToWin;
            bool counterTerroristsWinGame = round.CounterTerroristScore >= roundsToWin;
            bool draw = roundsPlayed >= _MaxRounds;
            if (!(terroristsWinGame || counterTerroristsWinGame || draw))
                return;
            var outcome = GameOutcome.Draw;
            if (terroristsWinGame)
                outcome = GameOutcome.TerroristsWin;
            else if (counterTerroristsWinGame)
                outcome = GameOutcome.CounterTerroristsWin;
            var game = new Game(_Rounds, outcome);
            _Games.Add(game);
            foreach (var pair in _PlayerTeams)
            {
                var player = _Players[pair.Key];
                var playerTeam = pair.Value;
                List<Game> games;
                bool terroristWin = terroristsWinGame && playerTeam == Team.Terrorist;
                bool counterTerroristWin = counterTerroristsWinGame && playerTeam == Team.CounterTerrorist;
                if (draw)
                    games = player.Draws;
                else if (terroristWin || counterTerroristWin)
                    games = player.Wins;
                else
                    games = player.Losses;
                games.Add(game);
                var players = playerTeam == Team.Terrorist ? game.Terrorists : game.CounterTerrorists;
                players.Add(player);
            }
        }
    }
}
