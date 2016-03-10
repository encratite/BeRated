using System;
using System.Collections.Generic;
using System.Linq;
using Ashod;
using BeRated.Cache;
using BeRated.Common;
using BeRated.Model;
using BeRated.Server;
using Microsoft.Owin;
using CacheGame = BeRated.Cache.Game;
using CacheKill = BeRated.Cache.Kill;
using CacheRound = BeRated.Cache.Round;
using GameInfo = Moserware.Skills.GameInfo;
using ModelGame = BeRated.Model.Game;
using ModelKill = BeRated.Model.Kill;
using ModelRound = BeRated.Model.Round;
using SkillPlayer = Moserware.Skills.Player;
using SkillTeam = Moserware.Skills.Team;
using SkillTeams = Moserware.Skills.Teams;
using TrueSkillCalculator = Moserware.Skills.TrueSkillCalculator;

namespace BeRated.App
{
	public class RatingApp : BaseApp
    {
        private const string TimeConstraintsCookie = "timeConstraints";

        private Configuration _Configuration;
		private CacheManager _Cache;

		private Dictionary<string, CacheEntry> _WebCache = new Dictionary<string, CacheEntry>();

        private Random _Random = new Random();

        public RatingApp(Configuration configuration)
        {
            _Configuration = configuration;
			_Cache = new CacheManager(_Configuration.LogDirectory);
			_Cache.OnUpdate += OnCacheUpdate;
        }

		public override void Dispose()
		{
			_Cache.Dispose();
			base.Dispose();
		}

		public override string GetCachedResponse(IOwinContext context)
		{
			CacheEntry cacheEntry;
            string key = GetCacheKey(context);
			if (_WebCache.TryGetValue(key, out cacheEntry))
            {
                var cacheTime = cacheEntry.Time;
                var now = DateTimeOffset.Now;
                if (
                    cacheTime.Year == now.Year &&
                    cacheTime.Month == now.Month &&
                    cacheTime.Day == now.Day
                )
                {
				    return cacheEntry.Markup;
                }
                else
                {
                    // The entry is outdated, get rid of it
                    _WebCache.Remove(key);
                }
            }
			return null;
		}

		public override void OnResponse(IOwinContext context, string markup, TimeSpan invokeDuration, TimeSpan renderDuration)
		{
			UpdateCache(context, markup);
			PrintPerformanceMessage(context, invokeDuration, renderDuration);
		}

		public void Initialize()
        {
            base.Initialize(_Configuration.ViewPath);
			_Cache.Run();
        }

		#region Controllers

		[Controller]
        public List<GeneralPlayerStats> Players()
        {
			var constraints = GetTimeConstraints();
			var players = GetGeneralPlayerStats(constraints);
			return players;
		}

		[Controller]
        public List<TeamStats> Teams()
        {
			var constraints = GetTimeConstraints();
			var teams = GetTeamStats(constraints);
			return teams;
		}

		[Controller]
		public List<ModelGame> Games()
		{
			var constraints = GetTimeConstraints();
			var games = GetGames(constraints);
			return games;
		}

        [Controller]
        public object Ratings()
        {
            return null;
        }

        [Controller]
        public List<MatchmakingPlayer> Matchmaker()
        {
            var players = _Cache.Players.Select(player => GetMatchmakingPlayer(player));
            return players.OrderBy(player => player.Name).ToList();
        }

        [Controller]
        public List<MatchmakingResult> Matchmaking(string ids, bool swap)
        {
            var players = GetPlayersFromSteamIds(ids);
            var results = GetMatchmakingResults(players, swap);
            return results;
        }

        [Controller]
        public ModelGame Game(long id)
        {
            var game = _Cache.Games.First(g => g.Id == id);
            var gameModel = GetGame(game);
            return gameModel;
        }

		[Controller]
		public TeamMatchupStats Matchup(string team1, string team2)
		{
			var players1 = GetTeam(team1);
			var players2 = GetTeam(team2);
			var matchup = new TeamMatchupStats
			{
				Team1 = players1,
				Team2 = players2,
				ImpreciseOutcomes = GetGameOutcomes(players1, players2, false),
				PreciseOutcomes = GetGameOutcomes(players1, players2, true),
			};
			return matchup;
		}

		[Controller]
        public GeneralPlayerStats Player(string id)
        {
            var player = _Cache.GetPlayer(id);
            var constraints = GetTimeConstraints();
			var generalPlayerStats = GetGeneralPlayerStats(player, constraints);
			return generalPlayerStats;
        }

		[Controller]
        public Matches Matches(string id)
        {
            var player = _Cache.GetPlayer(id);
            var constraints = GetTimeConstraints();
            var games = new Matches(player.Name, player.SteamId, GetPlayerGames(player, constraints));
            return games;
        }

		[Controller]
        public PlayerEncounters Encounters(string id)
        {
            var player = _Cache.GetPlayer(id);
			var constraints = GetTimeConstraints();
            var encounters = new PlayerEncounters(player.Name, player.SteamId, GetPlayerEncounters(player, constraints));
            return encounters;
        }

		[Controller]
        public PlayerWeapons Weapons(string id)
        {
            var player = _Cache.GetPlayer(id);
			var constraints = GetTimeConstraints();
            var weapons = new PlayerWeapons(player.Name, player.SteamId, GetPlayerWeapons(player, constraints));
            return weapons;
        }

		[Controller]
        public PlayerItems Items(string id)
        {
            var player = _Cache.GetPlayer(id);
			var constraints = GetTimeConstraints();
            var weapons = new PlayerItems(player.Name, player.SteamId, GetPlayerItems(player, constraints));
            return weapons;
        }

        #endregion

        #region JSON controllers

        [JsonController]
        public List<PlayerRatings> GetPlayerRatings(string id)
        {
            var player = _Cache.GetPlayer(id);
            var ratings = new List<PlayerRatings> {  GetPlayerRatings(player) };
            return ratings;
        }

        [JsonController]
        public List<PlayerRatings> GetAllRatings()
        {
            var ratings = _Cache.Players.Select(player => GetPlayerRatings(player)).ToList();
            return ratings;
        }

        [JsonController]
        public MatchmakingTeams GetMatchmakingTeams(string ids)
        {
            var players = GetPlayersFromSteamIds(ids);
            bool swap = _Random.Next(0, 1) == 1;
            var results = GetMatchmakingResults(players, swap);
            var bestResult = results.First();
            var teams = new MatchmakingTeams
            {
                Quality = bestResult.Quality,
                CounterTerrorists = GetSteamIdList(bestResult.Team1),
                Terrorists = GetSteamIdList(bestResult.Team1),
            };
            return teams;
        }

        #endregion

        private List<GeneralPlayerStats> GetGeneralPlayerStats(TimeConstraints constraints)
        {
            var stats = _Cache.Players.Select(player => GetGeneralPlayerStats(player, constraints));
			stats = stats.Where(player => player.Kills + player.Deaths > 0);
			stats = stats.OrderBy(player => player.Name);
            return stats.ToList();
        }

		private GeneralPlayerStats GetGeneralPlayerStats(Player player, TimeConstraints constraints)
        {
            var matchingKills = player.Kills.Where(kill => constraints.Match(kill.Time));
            var matchingDeaths = player.Deaths.Where(death => constraints.Match(death.Time));
            int kills = matchingKills.Count();
            int deaths = matchingDeaths.Count();

            var matchingWins = player.Wins.Where(game => constraints.Match(game.Time));
            var matchingGames = player.Games.Where(game => constraints.Match(game.Time));
            int wins = matchingWins.Count();
            int games = matchingGames.Count();

            var matchingRounds = player.Rounds.Where(round => constraints.Match(round.Time));
            var matchingRoundsWon = player.RoundsWon.Where(round => constraints.Match(round.Time));
            int roundsWon = matchingRoundsWon.Count();
            int roundsPlayed = matchingRounds.Count();

            var firstRound = player.Rounds.FirstOrDefault();
            var lastRound = player.Rounds.LastOrDefault();

            var generalStats = new GeneralPlayerStats
            {
                SteamId = player.SteamId,
                Name = player.Name,
                FirstRoundTime = GetRoundTime(firstRound),
                LastRoundTime = GetRoundTime(lastRound),
                KillsPerRound = Ratio.Get(kills, roundsPlayed),
                KillDeathRatio = Ratio.Get(kills, deaths),
                Kills = kills,
                Deaths = deaths,
                GamesPlayed = games,
                GameWinRatio = Ratio.Get(wins, games),
                RoundsPlayed = roundsPlayed,
                RoundWinRatio = Ratio.Get(roundsWon, roundsPlayed),
            };

            if (games > 0)
            {
                var firstGame = matchingGames.First();
                var startRating = firstGame.GetRatedPlayer(player);
                generalStats.StartMatchRating = startRating.MatchRating.PreGameRating.ConservativeRating;
                generalStats.StartRoundRating = startRating.RoundRating.PreGameRating.ConservativeRating;
                generalStats.StartKillRating = startRating.KillRating.PreGameRating.ConservativeRating;

                var lastGame = matchingGames.Last();
                var endRating = lastGame.GetRatedPlayer(player);
                generalStats.EndMatchRating = endRating.MatchRating.PostGameRating.ConservativeRating;
                generalStats.EndRoundRating = endRating.RoundRating.PostGameRating.ConservativeRating;
                generalStats.EndKillRating = endRating.KillRating.PostGameRating.ConservativeRating;
            }
            if (constraints.Start == null && constraints.End == null)
            {
                generalStats.StartMatchRating = null;
                generalStats.StartRoundRating = null;
                generalStats.StartKillRating = null;
            }

            return generalStats;
        }

        private List<TeamStats> GetTeamStats(TimeConstraints constraints)
        {
            var teams = new List<TeamStats>();
            var games = _Cache.Games.Where(game => constraints.Match(game.Time));
            foreach (var game in games)
            {
                AddGameToTeamStats(game.Terrorists, true, game.Outcome, teams);
                AddGameToTeamStats(game.CounterTerrorists, false, game.Outcome, teams);
            }
			teams = teams.OrderByDescending(team => team.Games).ToList();
            return teams;
        }

        private List<PlayerGame> GetPlayerGames(Player player, TimeConstraints constraints)
		{
            var matchingGames = player.Games.Where(game =>
                constraints.Match(game.Time) &&
                (game.Terrorists.Any(p => p.Player == player) || game.CounterTerrorists.Any(p => p.Player == player))
            );
            matchingGames = matchingGames.OrderByDescending(game => game.Time);
            var games = matchingGames.Select(game =>
            {
                var ratedPlayer = game.GetRatedPlayer(player);
                bool isTerrorist = game.Terrorists.Any(p => p.Player == player);
                int terroristScore = game.TerroristScore;
                int counterTerroristScore = game.CounterTerroristScore;
                var terrorists = GetPlayerInfos(game.Terrorists, game);
                var counterTerrorists = GetPlayerInfos(game.CounterTerrorists, game);
                PlayerGameOutcome outcome;
                if (game.Outcome == GameOutcome.Draw)
                    outcome = PlayerGameOutcome.Draw;
                else if (
                    isTerrorist && game.Outcome == GameOutcome.TerroristsWin ||
                    !isTerrorist && game.Outcome == GameOutcome.CounterTerroristsWin
                )
                    outcome = PlayerGameOutcome.Win;
                else
                    outcome = PlayerGameOutcome.Loss;
                return new PlayerGame
                {
                    Id = game.Id,
                    Time = game.Time,
                    Map = game.Map,
                    PlayerScore = isTerrorist ? terroristScore : counterTerroristScore,
                    EnemyScore = isTerrorist ? counterTerroristScore : terroristScore,
                    IsTerrorist = isTerrorist,
                    Outcome = outcome,
                    MatchRating = GetGameRating(ratedPlayer.MatchRating),
                    KillRating = GetGameRating(ratedPlayer.KillRating),
                    PlayerTeam = isTerrorist ? terrorists : counterTerrorists,
                    EnemyTeam = isTerrorist ? counterTerrorists : terrorists,
                };
            }).ToList();
            return games;
        }

		private List<PlayerEncounterStats> GetPlayerEncounters(Player player, TimeConstraints constraints)
		{
			var statsDictionary = new InitializerDictionary<string, PlayerEncounterStats>();
			Func<Player, PlayerEncounterStats> getStats = (Player statsPlayer) =>
			{
				var stats = statsDictionary.Get(statsPlayer.SteamId, () => new PlayerEncounterStats(statsPlayer.Name, statsPlayer.SteamId));
				return stats;
			};
			var kills = player.Kills.Where(kill => constraints.Match(kill.Time));
			var deaths = player.Deaths.Where(kill => constraints.Match(kill.Time));
			foreach (var kill in kills)
			{
				var stats = getStats(kill.Victim);
				stats.Kills++;
			}
			foreach (var death in deaths)
			{
				var stats = getStats(death.Killer);
				stats.Deaths++;
			}
			var encounters = statsDictionary.Values.OrderByDescending(stats => stats.Encounters).ToList();
			return encounters;
		}

		private List<PlayerWeaponStats> GetPlayerWeapons(Player player, TimeConstraints constraints)
		{
			var statsDictionary = new InitializerDictionary<string, PlayerWeaponStats>();
			var kills = player.Kills.Where(kill => constraints.Match(kill.Time));
			foreach (var kill in kills)
			{
				var stats = statsDictionary.Get(kill.Weapon, () => new PlayerWeaponStats(kill.Weapon));
				stats.Kills++;
				if (kill.Headshot)
					stats.HeadshotKills++;
                if (kill.Penetrated)
                    stats.PenetrationKills++;
			}
			var weapons = statsDictionary.Values.OrderByDescending(stats => stats.Kills).ToList();
			return weapons;
		}

		private List<PlayerItemStats> GetPlayerItems(Player player, TimeConstraints constraints)
		{
			var statsDictionary = new InitializerDictionary<string, PlayerItemStats>();
			var purchases = player.Purchases.Where(purchase => constraints.Match(purchase.Time));
			int kills = player.Kills.Count(kill => constraints.Match(kill.Time));
			int rounds = player.Rounds.Count(round => constraints.Match(round.Time));
			foreach (var purchase in purchases)
			{
				var stats = statsDictionary.Get(purchase.Item, () => new PlayerItemStats(purchase.Item));
				stats.TimesPurchased++;
			}
			var items = statsDictionary.Values.OrderByDescending(stats => stats.TimesPurchased).ToList();
			foreach (var item in items)
			{
				item.PurchasesPerRound = Ratio.Get(item.TimesPurchased, rounds);
				item.KillsPerPurchase = Ratio.Get(item.TimesPurchased, kills);
			}
			return items;
		}

		private List<PlayerInfo> GetTeam(string steamIds)
        {
            var players = GetPlayersFromSteamIds(steamIds);
            var team = players.Select(player => GetPlayerInfo(player)).ToList();
            return team;
        }

        private List<Player> GetPlayersFromSteamIds(string steamIds)
        {
            var tokens = steamIds.Split(',');
            var players = tokens.Select(steamId => _Cache.GetPlayer(steamId)).ToList();
            return players;
        }

        private GameOutcomes GetGameOutcomes(List<PlayerInfo> team1, List<PlayerInfo> team2, bool precise)
		{
			var outcomes = new GameOutcomes();
			foreach (var game in _Cache.Games)
			{
				bool terrorists =
					IsSameTeam(team1, game.Terrorists, precise) &&
					IsSameTeam(team2, game.CounterTerrorists, precise);
				bool counterTerrorists =
					IsSameTeam(team1, game.CounterTerrorists, precise) &&
					IsSameTeam(team2, game.Terrorists, precise);
				if (!terrorists && !counterTerrorists)
					continue;
				if (game.Outcome == GameOutcome.Draw)
					outcomes.Draws++;
				else if (
					game.Outcome == GameOutcome.TerroristsWin && terrorists ||
					game.Outcome == GameOutcome.CounterTerroristsWin && counterTerrorists
				)
					outcomes.Wins++;
				else
					outcomes.Losses++;

			}
			return outcomes;
		}

		private List<PlayerRatingSample> GetRatingSamples(Player player, Func<RatedPlayer, RatingPair> getRating)
		{
			return player.Games.Select(game =>
			{
				var ratedPlayer = game.GetRatedPlayer(player);
				var ratingPair = getRating(ratedPlayer);
				return new PlayerRatingSample
				{
					Time = game.Time,
					Value = ratingPair.PostGameRating.ConservativeRating,
				};
			}).OrderBy(sample => sample.Time).ToList();
		}

        private PlayerRatings GetPlayerRatings(Player player)
        {
            return new PlayerRatings
            {
                Name = player.Name,
                SteamId = player.SteamId,
                MatchRating = GetRatingSamples(player, (ratedPlayer) => ratedPlayer.MatchRating),
                KillRating = GetRatingSamples(player, (ratedPlayer) => ratedPlayer.KillRating),
            };
        }

		private void UpdateCache(IOwinContext context, string markup)
		{
            string key = GetCacheKey(context);
			_WebCache[key] = new CacheEntry(markup);
			int maximumCacheSize = _Configuration.CacheSize.Value * 1024 * 1024;
			int cacheSize = 0;
			foreach (var pair in _WebCache)
				cacheSize += pair.Value.Markup.Length;
			var pairs = _WebCache.OrderBy(pair => pair.Value.Time).ToList();
			while (cacheSize > maximumCacheSize && pairs.Any())
			{
				var pair = pairs.First();
				pairs.RemoveAt(0);
				_WebCache.Remove(pair.Key);
				cacheSize -= pair.Value.Markup.Length;
			}
		}

		private void PrintPerformanceMessage(IOwinContext context, TimeSpan invokeDuration, TimeSpan renderDuration)
		{
			string message = string.Format("Processed request {0} (controller: {1} ms; rendering: {2} ms)", context.Request.Uri.PathAndQuery, invokeDuration.TotalMilliseconds, renderDuration.TotalMilliseconds);
			TimeSpan duration = invokeDuration + renderDuration;
			if (duration.TotalMilliseconds < 250)
				Logger.Log(message);
			else if (duration.TotalMilliseconds < 1000)
				Logger.Warning(message);
			else
				Logger.Error(message);
		}

        private bool IsSameTeam(List<PlayerInfo> team1, List<RatedPlayer> team2, bool precise = true)
        {
            var ids1 = GetSteamIds(team1);
			var ids2 = GetSteamIds(team2);
			if (precise)
				return ids1.SetEquals(ids2);
			else
				return ids1.IsSubsetOf(ids2);
        }

		private HashSet<string> GetSteamIds(List<PlayerInfo> team)
		{
			var ids = team.Select(info => info.SteamId);
			return new HashSet<string>(ids);
		}

		private HashSet<string> GetSteamIds(List<RatedPlayer> team)
		{
			var ids = team.Select(player => player.Player.SteamId);
			return new HashSet<string>(ids);
		}

        private void AddGameToTeamStats(List<RatedPlayer> players, bool isTerroristTeam, GameOutcome outcome, List<TeamStats> teams)
        {
			if (players.Count == 0)
				return;
            var team = teams.FirstOrDefault(teamStats => IsSameTeam(teamStats.Players, players));
            if (team == null)
            {
                var playerInfos = players.Select(player => GetPlayerInfo(player.Player));
				playerInfos = playerInfos.OrderBy(player => player.Name);
                team = new TeamStats(playerInfos.ToList());
                teams.Add(team);
            }
            if (outcome == GameOutcome.Draw)
                team.Draws++;
            else if (
				isTerroristTeam && outcome == GameOutcome.TerroristsWin ||
				!isTerroristTeam && outcome == GameOutcome.CounterTerroristsWin
			)
                team.Wins++;
            else
                team.Losses++;
        }

		private List<ModelGame> GetGames(TimeConstraints constraints)
		{
			var matchingGames = _Cache.Games.Where(game => constraints.Match(game.Time));
			var games = matchingGames.Select(game => GetGame(game));
            games = games.OrderByDescending(game => game.Time);
			return games.ToList();
		}

        private ModelGame GetGame(CacheGame game)
        {
            return new ModelGame
            {
                Id = game.Id,
                Time = game.Time,
                Map = game.Map,
                TerroristScore = game.TerroristScore,
                CounterTerroristScore = game.CounterTerroristScore,
                Outcome = game.Outcome,
                Terrorists = GetPlayerInfos(game.Terrorists, game),
                CounterTerrorists = GetPlayerInfos(game.CounterTerrorists, game),
                Rounds = game.Rounds.Select(round => GetRound(round)).ToList(),
            };
        }

        private ModelRound GetRound(CacheRound round)
        {
            return new ModelRound
            {
                Time = round.Time,
                Winner = round.Winner,
                TerroristScore = round.TerroristScore,
                CounterTerroristScore = round.CounterTerroristScore,
                Kills = round.Kills.Select(kill => GetKill(kill)).ToList(),
            };
        }

        private ModelKill GetKill(CacheKill kill)
        {
            return new ModelKill
            {
                Time = kill.Time,
                Killer = GetPlayerInfo(kill.Killer),
                Victim = GetPlayerInfo(kill.Victim),
                KillerTeam = kill.KillerTeam,
                Weapon = kill.Weapon,
            };
        }

        private PlayerInfo GetPlayerInfo(Player player)
        {
            return new PlayerInfo(player.Name, player.SteamId);
        }

        private List<PlayerInfo> GetPlayerInfos(IEnumerable<Player> players)
        {
            return players.Select(player => GetPlayerInfo(player)).OrderBy(player => player.Name).ToList();
        }

        private List<PlayerGameInfo> GetPlayerInfos(List<RatedPlayer> team, CacheGame game)
        {
            var gameKills = game.Rounds.SelectMany(round => round.Kills).ToList();
            var playerInfos = team.Select(player =>
            {
                var playerInfo = new PlayerGameInfo
                {
                    Name = player.Player.Name,
                    SteamId = player.Player.SteamId,
                    Kills = gameKills.Count(kill => kill.Killer == player.Player),
                    Deaths = gameKills.Count(kill => kill.Victim == player.Player),
                    MatchRating = GetGameRating(player.MatchRating),
                    KillRating = GetGameRating(player.KillRating),
                };
                return playerInfo;
            });
            playerInfos = playerInfos.OrderByDescending(player => player.Kills);
            return playerInfos.ToList();
        }

		private TimeConstraints GetTimeConstraints()
		{
			var constraints = new TimeConstraints();
            string internalName = GetTimeConstraintsCookie();
            if (internalName != null)
            {
                var preset = TimeConstraintPreset.Presets.First(p => p.InternalName == internalName);
                constraints = preset.GetConstraints();
            }
            return constraints;
		}

		private void OnCacheUpdate()
		{
			_WebCache.Clear();
		}

        private string GetCacheKey(IOwinContext context)
        {
            string internalName = GetTimeConstraintsCookie();
            string key = string.Format("{0}&{1}={2}", context.Request.Uri.PathAndQuery, TimeConstraintsCookie, internalName);
            return key;
        }

        private string GetTimeConstraintsCookie()
        {
            string internalName = null;
            Context.Current.Cookies.TryGetValue(TimeConstraintsCookie, out internalName);
            return internalName;
        }

        private GameRating GetGameRating(RatingPair pair)
        {
            return new GameRating(pair.PreGameRating.ConservativeRating, pair.PostGameRating.ConservativeRating);
        }

        private List<MatchmakingResult> GetMatchmakingResults(List<Player> players, bool swapTeams)
        {
            if (players.Count < 3)
                throw new ArgumentException("Not enough players.");
            var results = new List<MatchmakingResult>();
            var team1 = new List<Player> { players.First() };
            var team2 = new List<Player>();
            if (swapTeams)
            {
                var temporaryTeam = team1;
                team1 = team2;
                team2 = temporaryTeam;
            }
            var remainingPlayers = players.Skip(1);
            EvaluateMatchmakingPermutations(team1, team2, remainingPlayers, results);
            return results.OrderByDescending(result => result.Quality).Take(100).ToList();
        }

        private void EvaluateMatchmakingPermutations(IEnumerable<Player> team1, IEnumerable<Player> team2, IEnumerable<Player> remainingPlayers, List<MatchmakingResult> results)
        {
            var player = remainingPlayers.FirstOrDefault();
            if (player != null)
            {
                remainingPlayers = remainingPlayers.Skip(1);
                var playerArray = new [] { player };
                EvaluateMatchmakingPermutations(team1.Concat(playerArray), team2, remainingPlayers, results);
				EvaluateMatchmakingPermutations(team1, team2.Concat(playerArray), remainingPlayers, results);
            }
            else
            {
                if (Math.Abs(team1.Count() - team2.Count()) > 1)
                    return;
                var skillTeam1 = GetSkillTeam(team1);
                var skillTeam2 = GetSkillTeam(team2);
                var teams = SkillTeams.Concat(skillTeam1, skillTeam2);
                double quality = TrueSkillCalculator.CalculateMatchQuality(GameInfo.DefaultGameInfo, teams);
                var result = new MatchmakingResult
                {
                    Quality = quality,
                    Team1 = GetPlayerInfos(team1),
                    Team2 = GetPlayerInfos(team2),
                };
                results.Add(result);
            }
        }

        private SkillTeam GetSkillTeam(IEnumerable<Player> players)
        {
            var team = new SkillTeam();
            foreach (var player in players)
            {
                var skillPlayer = new SkillPlayer(player);
                team.AddPlayer(skillPlayer, player.MatchRating);
            }
            return team;
        }

        private MatchmakingPlayer GetMatchmakingPlayer(Player player)
        {
            var lastRound = player.Rounds.LastOrDefault();
            return new MatchmakingPlayer
            {
                Name = player.Name,
                SteamId = player.SteamId,
                MatchRating = player.MatchRating.ConservativeRating,
                Games = player.Games.Count(),
                LastRound = GetRoundTime(lastRound),
            };
        }

        private DateTime? GetRoundTime(CacheRound round)
        {
            return round != null ? round.Time : (DateTime?)null;
        }

        private List<string> GetSteamIdList(List<PlayerInfo> players)
        {
            return players.Select(player => player.SteamId).ToList();
        }
    }
}
