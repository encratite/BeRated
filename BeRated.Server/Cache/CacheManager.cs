using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BeRated.Common;
using Moserware.Skills;
using Team = BeRated.Common.Team;
using SkillPlayer = Moserware.Skills.Player;
using SkillTeam = Moserware.Skills.Team;
using Ashod;

namespace BeRated.Cache
{
	class CacheManager : IDisposable
	{
		private const int MaxRoundsDefault = 30;
        private const int UpdateInterval = 10 * 1000;

        private const int WinnerRank = 1;
        private const int LoserRank = 2;

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
		private int _MaxRounds = MaxRoundsDefault;

		private Thread _ReaderThread = null;

        private HashSet<string> _LogsProcessed = new HashSet<string>();

		private Dictionary<string, Player> _Players = new Dictionary<string, Player>();

        private List<Game> _Games = new List<Game>();

        private int _MatchStartCounter = 0;
        private string _Map = null;
		private Dictionary<string, PlayerGameState> _PlayerStates = new Dictionary<string, PlayerGameState>();
		private List<Round> _Rounds = new List<Round>();
        private List<Kill> _RoundKills = new List<Kill>();

        private LogParser _LogParser;

		public CacheManager(string logPath)
		{
			_LogPath = logPath;
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

		public Player GetPlayer(string name, string steamId, DateTime time)
		{
			Player player;
			if (_Players.TryGetValue(steamId, out player))
			{
				player.SetName(name, time);
			}
			else
			{
				player = new Player(name, steamId, time);
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
                    var files = Directory.GetFiles(_LogPath, "*.log").ToList();
					files.Sort();
                    foreach (var file in files)
                        ProcessLog(file);
                }
                catch(Exception exception)
                {
                    Logger.Exception("Failed to process log files", exception);
                }
                Thread.Sleep(UpdateInterval);
            }
		}

		private void ProcessLog(string path)
		{
			_MaxRounds = MaxRoundsDefault;
            _MatchStartCounter = 0;
            _Map = null;
			_PlayerStates = new Dictionary<string, PlayerGameState>();
            _RoundKills = new List<Kill>();
			string fileName = Path.GetFileName(path);
			if (_LogsProcessed.Contains(fileName))
			{
				// This file has already been processed
				return;
			}
            var fileInfo = new FileInfo(path);
            var lastWriteTimeSpan = DateTime.Now -  fileInfo.LastWriteTime;
			var content = File.ReadAllText(path);
			if (
                lastWriteTimeSpan < TimeSpan.FromHours(1) &&
                !content.Contains("Log file closed\n") &&
                !content.Contains("disconnected (reason \"Punting bot, server is hibernating\")\n")
            )
			{
				// The log file has not been completed yet
                Logger.Warning("Unable to process incomplete file {0}", path);
				return;
			}
			var lines = content.Split('\n');
			int lineCounter = 1;
            try
            {
				foreach (var line in lines)
				{
					ProcessLine(line);
					lineCounter++;
				}
                Logger.Log("Processed {0}", path);
            }
            catch (NotSupportedException)
            {
                Logger.Warning("Unable to process outdated format in {0}", path);
            }
            _LogsProcessed.Add(fileName);
		}

		private void ProcessLine(string line)
        {
            var readers = new Func<string, bool>[]
            {
                ReadServerVersion,
                ReadMap,
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

        private bool ReadServerVersion(string line)
        {
            int? version = _LogParser.ReadServerVersion(line);
            if (version == null)
                return false;
            if (version.Value < 5949)
            {
                // The format of this log file is too old and too annoying to parse, abort
                throw new NotSupportedException();
            }
            return true;
        }

        private bool ReadMap(string line)
        {
            string map = _LogParser.ReadMap(line);
            if (map == null)
                return false;
            _MatchStartCounter++;
            _Map = map;
            return true;
        }

        private bool ReadPlayerKill(string line)
        {
            var kill = _LogParser.ReadPlayerKill(line);
			if (kill == null)
                return false;
			if (
                !IgnoreStats() &&
                kill.Killer.SteamId != LogParser.BotId &&
                kill.Victim.SteamId != LogParser.BotId &&
                kill.Killer != kill.Victim
            )
			{
				kill.Weapon = TranslateWeapon(kill.Weapon);
				var killer = kill.Killer;
				var victim = kill.Victim;
				var killerState = GetPlayerState(killer);
				var victimState = GetPlayerState(victim);
				if (killerState != null)
					killerState.Kills++;
				if (victimState != null)
					victimState.Deaths++;
				killer.Kills.Add(kill);
				victim.Deaths.Add(kill);
				_RoundKills.Add(kill);
			}
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
			if (steamId == LogParser.BotId)
				return false;
            var player = GetPlayer(steamId);
            PlayerGameState state;
            if (!_PlayerStates.TryGetValue(steamId, out state))
            {
                state = new PlayerGameState(player, team);
                _PlayerStates[steamId] = state;
            }
            state.Team = team;
            state.RoundPlayerLeft = null;
			state.Kills = 0;
			state.Deaths = 0;
            return true;
        }

        private bool ReadDisconnect(string line)
        {
            var disconnect = _LogParser.ReadDisconnect(line);
			if (disconnect == null)
                return false;
			string steamId = disconnect.Player.SteamId;
			if (steamId == LogParser.BotId)
				return true;
            PlayerGameState state;
            if (!_PlayerStates.TryGetValue(steamId, out state))
                return true;
			state.RoundPlayerLeft = _Rounds.Count;
			return true;
        }

        private bool ReadEndOfRound(string line)
        {
            var round = _LogParser.ReadEndOfRound(line);
            if (round == null || round.TerroristScore == 0 && round.CounterTerroristScore == 0)
                return false;
            int roundsPlayed = round.TerroristScore + round.CounterTerroristScore;
            if (roundsPlayed == 1)
                _Rounds = new List<Round>();
            round.Kills = _RoundKills;
            _Rounds.Add(round);
            var winningTeam = _LogParser.GetWinningTeam(round.SfuiNotice);
            var playerStates = GetActivePlayerStates();
            foreach (var pair in playerStates)
            {
                var player = _Players[pair.Key];
                var state = pair.Value;
                var team = state.Team;
                var container = team == winningTeam ? player.RoundsWon : player.RoundsLost;
                container.Add(round);
            }
            Action<Player, Rating> setRating = (player, rating) => player.RoundRating = rating;
            bool counterTerroristsWinGame = winningTeam == Team.CounterTerrorist;
            AdjustRatings(counterTerroristsWinGame, playerStates, setRating);
            CheckForEndOfGame(round, roundsPlayed);
            _RoundKills = new List<Kill>();
            return true;
        }

        private bool ReadPurchase(string line)
        {
            var purchase = _LogParser.ReadPurchase(line);
			if (purchase == null)
                return false;
			string steamId = purchase.Player.SteamId;
			if (!IgnoreStats() && steamId != LogParser.BotId)
            {
                var state = _PlayerStates[steamId];
                var team = state.Team;
                purchase.Item = TranslateItem(purchase.Item, team);
			    purchase.Player.Purchases.Add(purchase);
            }
            return true;
        }

        #endregion

        private void CheckForEndOfGame(Round round, int roundsPlayed)
		{
			if (_PlayerStates.Count == 0)
				return;
			int roundsToWin = _MaxRounds / 2 + 1;
			bool terroristsWinGame = round.TerroristScore >= roundsToWin;
			bool counterTerroristsWinGame = round.CounterTerroristScore >= roundsToWin;
			bool draw = !terroristsWinGame && !counterTerroristsWinGame && roundsPlayed >= _MaxRounds;
			if (!(terroristsWinGame || counterTerroristsWinGame || draw))
				return;
			var outcome = GameOutcome.Draw;
			if (terroristsWinGame)
				outcome = GameOutcome.TerroristsWin;
			else if (counterTerroristsWinGame)
				outcome = GameOutcome.CounterTerroristsWin;
			var game = new Game(_Map, _Rounds, outcome);
			_Games.Add(game);
			_Games.Sort((x, y) => x.Time.CompareTo(y.Time));
			var playerStates = GetActivePlayerStates();
			AddGame(terroristsWinGame, counterTerroristsWinGame, draw, game, playerStates);
			if (!draw)
			{
				Action<Player, Rating> setRating = (player, rating) => player.MatchRating = rating;
				AdjustRatings(counterTerroristsWinGame, playerStates, setRating);
			}
            AdjustKillRatings(playerStates);
			AddPlayersToGame(game, playerStates);
		}

		private void AddGame(bool terroristsWinGame, bool counterTerroristsWinGame, bool draw, Game game, List<KeyValuePair<string, PlayerGameState>> playerStates)
		{
			foreach (var pair in playerStates)
			{
				var player = _Players[pair.Key];
				var state = pair.Value;
				List<Game> games;
				if (draw)
					games = player.Draws;
				else if (
					terroristsWinGame && state.Team == Team.Terrorist ||
					counterTerroristsWinGame && state.Team == Team.CounterTerrorist
				)
					games = player.Wins;
				else
					games = player.Losses;
				games.Add(game);
				games.Sort((x, y) => x.Time.CompareTo(y.Time));
			}
		}

		private void AddPlayersToGame(Game game, List<KeyValuePair<string, PlayerGameState>> playerStates)
		{
			foreach (var pair in playerStates)
			{
				var player = _Players[pair.Key];
				var state = pair.Value;
				var players = state.Team == Team.Terrorist ? game.Terrorists : game.CounterTerrorists;
				var ratedPlayer = new RatedPlayer(player, state.PreGameMatchRating, state.PreGameRoundRating, state.PreGameKillRating);
				players.Add(ratedPlayer);
			}
		}

		private void AdjustRatings(bool counterTerroristsWinGame, List<KeyValuePair<string, PlayerGameState>> playerStates, Action<Player, Rating> setRating)
        {
            var counterTerrorists = new SkillTeam();
            var terrorists = new SkillTeam();
            bool counterTerroristsPlaying = false;
            bool terroristsPlaying = false;
            foreach (var pair in playerStates)
            {
                var player = _Players[pair.Key];
                var state = pair.Value;
                var skillPlayer = new SkillPlayer(player);
                if (state.Team == Team.CounterTerrorist)
                {
                    counterTerrorists.AddPlayer(skillPlayer, player.MatchRating);
                    counterTerroristsPlaying = true;
                }
                else
                {
                    terrorists.AddPlayer(skillPlayer, player.MatchRating);
                    terroristsPlaying = true;
                }
            }
            if (!counterTerroristsPlaying || !terroristsPlaying)
                return;
            var teams = Teams.Concat(counterTerrorists, terrorists);
            int counterTerroristRank = counterTerroristsWinGame ? WinnerRank : LoserRank;
            int terroristRank = !counterTerroristsWinGame ? WinnerRank : LoserRank;
            var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo.DefaultGameInfo, teams, counterTerroristRank, terroristRank);
            foreach (var pair in newRatings)
            {
                var player = (Player)pair.Key.Id;
                setRating(player, pair.Value);
            }
        }

		private void AdjustKillRatings(List<KeyValuePair<string, PlayerGameState>> playerStates)
        {
            if (playerStates.Count < 2)
                return;
            var sortedPlayerStates = GetSortedPlayerStates(playerStates);
            var skillTeams = sortedPlayerStates.Select(pair =>
            {
                var player = _Players[pair.Key];
                var skillPlayer = new SkillPlayer(pair);
                var rating = pair.Value.PreGameKillRating;
                var team = new SkillTeam(skillPlayer, rating);
                return team;
            });
            var teams = Teams.Concat(skillTeams.ToArray());
            var ranks = new List<int>();
            for (int i = 1; i <= sortedPlayerStates.Count; i++)
                ranks.Add(i);
            var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo.DefaultGameInfo, teams, ranks.ToArray());
            foreach (var pair in newRatings)
            {
                var statePair = (KeyValuePair<string, PlayerGameState>)pair.Key.Id;
                var player = _Players[statePair.Key];
                player.KillRating = pair.Value;
            }
        }

        private List<KeyValuePair<string, PlayerGameState>> GetSortedPlayerStates(List<KeyValuePair<string, PlayerGameState>> playerStates)
        {
            var sortedPlayerStates = playerStates.ToList();
            sortedPlayerStates.Sort((pair1, pair2) =>
            {
                var state1 = pair1.Value;
                var state2 = pair2.Value;
                int result = state2.Kills.CompareTo(state1.Kills);
                if (result == 0)
                    result = state1.Deaths.CompareTo(state2.Deaths);
                return result;
            });
            return sortedPlayerStates;
        }

        private bool IgnoreStats()
        {
            return _MatchStartCounter < 2;
        }

        private string TranslateWeapon(string weapon)
        {
            var translations = new Dictionary<string, string>
            {
                {  "knife_t", "knife" },
                {  "knife_default_ct", "knife" },
            };
            string translation = null;
            if (translations.TryGetValue(weapon, out translation))
                return translation;
            else
                return weapon;
        }

        private string TranslateItem(string item, Team team)
        {
            var translations = new Dictionary<string, string>
            {
                // Kevlar aliases
                { "vest", "kevlar" },
                { "vesthelm", "assaultsuit" },
            };
            string translation = null;
            if (translations.TryGetValue(item, out translation))
                return translation;

            var pairs = new List<ItemPair>
            {
                // Pistols
                new ItemPair("hkp2000", "glock"),
                new ItemPair("fiveseven", "tec9"),
                // Shotguns
                new ItemPair("mag7", "sawedoff"),
                // SMGs
                new ItemPair("mp9", "mac10"),
                // Rifles
                new ItemPair("famas", "galilar"),
                new ItemPair("m4a1", "ak47"),
                new ItemPair("m4a1_silencer", "ak47"),
                new ItemPair("aug", "sg556"),
                new ItemPair("scar20", "g3sg1"),
                // Grenades
                new ItemPair("incgrenade", "molotov"),
            };
            foreach (var pair in pairs)
            {
                if (pair.Translate(item, team, ref translation))
                    return translation;
            }
            return item;
        }

        private List<KeyValuePair<string, PlayerGameState>> GetActivePlayerStates()
        {
            return _PlayerStates.Where(pair =>
                pair.Value.RoundPlayerLeft == null ||
                _Rounds.Count - pair.Value.RoundPlayerLeft.Value < 2
            ).ToList();
        }

		private PlayerGameState GetPlayerState(Player player)
		{
			PlayerGameState state = null;
			_PlayerStates.TryGetValue(player.SteamId, out state);
			return state;
		}
    }
}
