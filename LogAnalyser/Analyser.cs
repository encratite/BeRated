using BeRated.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogAnalyser
{
	class Analyser
	{
		Random _Random = new Random();
		Dictionary<string, PlayerInformation> _PlayerData;
		Dictionary<string, Dictionary<string, PlayerPerformance>> _PerformanceMatrix;

		public void ProcessLogs(string path)
		{
			_PlayerData = new Dictionary<string, PlayerInformation>();
			_PerformanceMatrix = new Dictionary<string, Dictionary<string, PlayerPerformance>>();
			var files = Directory.GetFiles(path);
			foreach (var file in files)
				ProcessLog(file);
		}

		public void Analyse()
		{
			const int iterations = 1000000;
			RatingEvaluation bestRatings = EvaluateRating();
			for (int i = 0; i < iterations; i++)
			{
				var evaluation = EvaluateRating();
				if (evaluation.Error < bestRatings.Error)
					bestRatings = evaluation;
			}
			Console.WriteLine("Error: {0}", bestRatings.Error);
			var ratings = bestRatings.Ratings.OrderByDescending(x => x.Rating);
			foreach (var rating in ratings)
			{
				var playerInformation = _PlayerData[rating.Identity.SteamId];
				double? killDeathRatio = null;
				if (playerInformation.DeathCount > 0)
					killDeathRatio = (double)playerInformation.KillCount / playerInformation.DeathCount;
				Console.WriteLine("{0} ({1}): KDR {2:0.00}, rating {3:0.00}", rating.Identity.Name, rating.Identity.SteamId, killDeathRatio, rating.Rating);
			}
		}

		void ProcessLog(string path)
		{
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
			var killerInformation = GetPlayerInformation(kill.Killer);
			killerInformation.Kills.Add(kill);
			killerInformation.KillCount++;
			var victimInformation = GetPlayerInformation(kill.Victim);
			victimInformation.DeathCount++;
			var killerPerformance = GetPeformanceEntry(kill.Killer.SteamId, kill.Victim.SteamId);
			killerPerformance.Kills++;
			var victimPerformance = GetPeformanceEntry(kill.Victim.SteamId, kill.Killer.SteamId);
			victimPerformance.Deaths++;
		}

		PlayerInformation GetPlayerInformation(PlayerIdentity playerIdentity)
		{
			PlayerInformation playerInformation;
			if (!_PlayerData.TryGetValue(playerIdentity.SteamId, out playerInformation))
			{
				playerInformation = new PlayerInformation(playerIdentity);
				_PlayerData.Add(playerIdentity.SteamId, playerInformation);
			}
			return playerInformation;
		}

		PlayerPerformance GetPeformanceEntry(string steamId1, string steamId2)
		{
			Dictionary<string, PlayerPerformance> innerDictionary;
			if (!_PerformanceMatrix.TryGetValue(steamId1, out innerDictionary))
			{
				innerDictionary = new Dictionary<string, PlayerPerformance>();
				_PerformanceMatrix.Add(steamId1, innerDictionary);
			}
			PlayerPerformance performance;
			if (!innerDictionary.TryGetValue(steamId2, out performance))
			{
				performance = new PlayerPerformance();
				innerDictionary.Add(steamId2, performance);
			}
			return performance;
		}

		RatingEvaluation EvaluateRating()
		{
			var ratings = new Dictionary<string, PlayerRating>();
			foreach (var player in _PlayerData.Values)
			{
				const double minimumRating = 1.0;
				const double maximumRating = 10.0;
				double rating = _Random.NextDouble() * (maximumRating - minimumRating) + minimumRating;
				var playerRating = new PlayerRating(player.Identity, rating);
				ratings[player.Identity.SteamId] = playerRating;
			}
			double error = 0.0;
			foreach (string steamId1 in _PerformanceMatrix.Keys)
			{
				foreach (string steamId2 in _PerformanceMatrix.Keys)
				{
					if (steamId1 == steamId2)
						continue;
					double rating1 = ratings[steamId1].Rating;
					double rating2 = ratings[steamId2].Rating;
					double expectedValue = ExpectedValue(rating1, rating2);
					var performance = GetPeformanceEntry(steamId1, steamId2);
					int totalEncounters = performance.Kills + performance.Deaths;
					if(totalEncounters == 0)
						continue;
					double actualValue = (double)performance.Kills / totalEncounters;
					error += Math.Pow(expectedValue - actualValue, 2.0);
				}
			}
			error = Math.Sqrt(error);
			var evaluation = new RatingEvaluation(ratings.Values.ToList(), error);
			return evaluation;
		}

		double ExpectedValue(double rating1, double rating2)
		{
			return rating1 / (rating1 + rating2);
		}
	}
}
