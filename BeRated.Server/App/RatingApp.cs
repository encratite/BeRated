using System;
using System.Collections.Generic;
using System.Linq;
using Ashod;
using BeRated.Cache;
using BeRated.Common;
using BeRated.Model;
using BeRated.Server;
using Microsoft.Owin;

namespace BeRated.App
{
	public class RatingApp : BaseApp
    {
        private Configuration _Configuration;
		private CacheManager _Cache;

		private Dictionary<string, CacheEntry> _WebCache = new Dictionary<string, CacheEntry>();

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
			if (_WebCache.TryGetValue(context.Request.Uri.PathAndQuery, out cacheEntry))
				return cacheEntry.Markup;
			else
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
        public GeneralStats General()
        {
			var constraints = GetTimeConstraints();
			var stats = new GeneralStats
            {
                Players = GetGeneralPlayerStats(constraints),
                Teams = GetTeamStats(constraints),
            };
			return stats;
		}

        [Controller]
        public PlayerStats Player(string id)
        {
            var player = _Cache.GetPlayer(id);
			var constraints = GetTimeConstraints();
            var playerStats = new PlayerStats
            {
                SteamId = player.SteamId,
                Name = player.Name,
                Games = GetPlayerGames(player, constraints),
				Encounters = GetPlayerEncounterStats(player, constraints),
				Weapons = GetPlayerWeaponStats(player, constraints),
				Purchases = GetPlayerItemStats(player, constraints),
            };
            return playerStats;
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

		#endregion

		#region Cache access

		private List<GeneralPlayerStats> GetGeneralPlayerStats(TimeConstraints constraints)
        {
            var stats = _Cache.Players.Select(player =>
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

                return new GeneralPlayerStats
                {
                    SteamId = player.SteamId,
		            Name = player.Name,
		            KillDeathRatio = Ratio.Get(kills, deaths),
		            Kills = kills,
		            Deaths = deaths,
		            GamesPlayed = games,
		            GameWinRatio = Ratio.Get(wins, games),
		            RoundsPlayed = roundsPlayed,
		            RoundWinRatio = Ratio.Get(roundsWon, roundsPlayed),
                };
            }).ToList();
            return stats;
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
            return teams;
        }

        private List<PlayerGame> GetPlayerGames(Player player, TimeConstraints constraints)
        {
            var matchingGames = player.Games.Where(game =>
                constraints.Match(game.Time) &&
                (game.Terrorists.Contains(player) || game.CounterTerrorists.Contains(player))
            );
            var games = matchingGames.Select(game =>
            {
                bool isTerrorist = game.Terrorists.Contains(player);
                int terroristScore = game.TerroristScore;
                int counterTerroristScore = game.CounterTerroristScore;
                var terrorists = GetPlayerInfos(game.Terrorists);
                var counterTerrorists = GetPlayerInfos(game.CounterTerrorists);
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
                    Time = game.Time,
                    PlayerScore = isTerrorist ? terroristScore : counterTerroristScore,
                    EnemyScore = isTerrorist ? counterTerroristScore : terroristScore,
                    Outcome = outcome,
                    PlayerTeam = isTerrorist ? terrorists : counterTerrorists,
                    EnemyTeam = isTerrorist ? counterTerrorists : terrorists,
                };
            }).ToList();
            return games;
        }

		private List<PlayerEncounterStats> GetPlayerEncounterStats(Player player, TimeConstraints constraints)
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
			var encounters = statsDictionary.Values.ToList();
			return encounters;
		}

		private List<PlayerWeaponStats> GetPlayerWeaponStats(Player player, TimeConstraints constraints)
		{
			var statsDictionary = new InitializerDictionary<string, PlayerWeaponStats>();
			var kills = player.Kills.Where(kill => constraints.Match(kill.Time));
			foreach (var kill in kills)
			{
				var stats = statsDictionary.Get(kill.Weapon, () => new PlayerWeaponStats(kill.Weapon));
				stats.Kills++;
				if (kill.Headshot)
					stats.Headshots++;
			}
			var weapons = statsDictionary.Values.ToList();
			return weapons;
		}

		private List<PlayerItemStats> GetPlayerItemStats(Player player, TimeConstraints constraints)
		{
			var statsDictionary = new InitializerDictionary<string, PlayerItemStats>();
			var purchases = player.Purchases.Where(purchase => constraints.Match(purchase.Time));
			int kills = player.Kills.Count(kill => constraints.Match(kill.Time));
			int rounds = player.Rounds.Count(round => constraints.Match(round.Time));
			foreach (var purchase in player.Purchases)
			{
				var stats = statsDictionary.Get(purchase.Item, () => new PlayerItemStats(purchase.Item));
				stats.TimesPurchased++;
			}
			var items = statsDictionary.Values.ToList();
			foreach (var item in items)
			{
				item.PurchasesPerRound = Ratio.Get(item.TimesPurchased, rounds);
				item.KillsPerPurchase = Ratio.Get(item.TimesPurchased, kills);
			}
			return items;
		}

		private List<PlayerInfo> GetTeam(string steamIdString)
		{
			var steamIds = steamIdString.Split(',');
			var players = steamIds.Select(steamId => _Cache.GetPlayer(steamId));
			var team = players.Select(player => GetPlayerInfo(player)).ToList();
			return team;
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

        #endregion

		private void UpdateCache(IOwinContext context, string markup)
		{
			_WebCache[context.Request.Uri.PathAndQuery] = new CacheEntry(markup);
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

        private bool IsSameTeam(List<PlayerInfo> team1, List<Player> team2, bool precise = true)
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

		private HashSet<string> GetSteamIds(List<Player> team)
		{
			var ids = team.Select(player => player.SteamId);
			return new HashSet<string>(ids);
		}

        private void AddGameToTeamStats(List<Player> players, bool isTerroristTeam, GameOutcome outcome, List<TeamStats> teams)
        {
            var team = teams.FirstOrDefault(teamStats => IsSameTeam(teamStats.Players, players));
            if (team == null)
            {
                var playerInfos = players.Select(player => GetPlayerInfo(player)).ToList();
                team = new TeamStats(playerInfos);
                teams.Add(team);
            }
            if (outcome == GameOutcome.Draw)
                team.Draws++;
            else if (isTerroristTeam && outcome == GameOutcome.TerroristsWin)
                team.Wins++;
            else
                team.Losses++;
        }

        private PlayerInfo GetPlayerInfo(Player player)
        {
            return new PlayerInfo(player.SteamId, player.Name);
        }

        private List<PlayerInfo> GetPlayerInfos(List<Player> team)
        {
			var players = team.OrderBy(player => player.Name);
            return players.Select(player => GetPlayerInfo(player)).ToList();
        }

		private TimeConstraints GetTimeConstraints()
		{
			var timeConstraints = new TimeConstraints();
			return timeConstraints;
		}

		private void OnCacheUpdate()
		{
			_WebCache.Clear();
		}
    }
}
