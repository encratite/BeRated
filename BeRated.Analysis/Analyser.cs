using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeRated
{
	class Analyser
	{
		const double MinimumRating = 1.0;
		const double InitialRating = 100.0;

		const int Iterations = 100000;

		const int ErrorEvaluationEncounterBarrier = 30;

		const double InitialMutationSpeed = 10.0;
		const double MinimumMutationSpeed = 0.1;
		const double MutationSpeedAdjustment = (InitialMutationSpeed - MinimumMutationSpeed) / (Iterations / 2);

		Random _Random = new Random();
		Dictionary<string, PlayerInformation> _PlayerData;
		Dictionary<string, Dictionary<string, PlayerPerformance>> _PerformanceMatrix;

		double _MutationSpeed;

		public void ProcessLogs(string path)
		{
			_PlayerData = new Dictionary<string, PlayerInformation>();
			_PerformanceMatrix = new Dictionary<string, Dictionary<string, PlayerPerformance>>();
			_MutationSpeed = InitialMutationSpeed;
			var files = Directory.GetFiles(path);
			foreach (var file in files)
				ProcessLog(file);
		}

		public void Analyse(string playerCsvOutputPath)
		{
			var initialRatings = new List<PlayerRating>();
			foreach (var playerInformation in _PlayerData.Values)
			{
				var rating = new PlayerRating(playerInformation.Identity, InitialRating);
				initialRatings.Add(rating);
			}
			RatingEvaluation bestRatings = EvaluateRatings(initialRatings.ToArray());
			for (int i = 0; i < Iterations; i++)
			{
				var mutatedRatings = GetMutatedRatings(bestRatings.Ratings);
				var mutatedRatingEvaluation = EvaluateRatings(mutatedRatings);
				if (mutatedRatingEvaluation.Error < bestRatings.Error)
					bestRatings = mutatedRatingEvaluation;
			}
            Console.WriteLine("Error: {0}", bestRatings.Error);
            var ratings = bestRatings.Ratings.OrderByDescending(x => x.Rating);
            using (var csvWriter = new StreamWriter(playerCsvOutputPath, false))
            {
                foreach (var rating in ratings)
                {
                    var playerInformation = _PlayerData[rating.Identity.SteamId];
                    double? killDeathRatio = null;
                    if (playerInformation.DeathCount > 0)
                        killDeathRatio = (double)playerInformation.KillCount / playerInformation.DeathCount;
                    Console.WriteLine("{0} ({1}): KDR {2:0.00}, rating {3:0.00}", rating.Identity.Name, rating.Identity.SteamId, killDeathRatio, rating.Rating);
                    csvWriter.WriteLine("{0},{1},{2},{3},{4:0.00}", rating.Identity.Name, rating.Identity.SteamId, playerInformation.KillCount, playerInformation.DeathCount, rating.Rating);
                }
            }
            PrintEncounterStatistics(ratings);
		}

		void PrintEncounterStatistics(IOrderedEnumerable<PlayerRating> ratings)
		{
			foreach (string steamId1 in _PerformanceMatrix.Keys)
			{
				foreach (string steamId2 in _PerformanceMatrix.Keys)
				{
					if (steamId1 == steamId2)
						continue;
					double rating1 = ratings.Where(x => x.Identity.SteamId == steamId1).First().Rating;
					double rating2 = ratings.Where(x => x.Identity.SteamId == steamId2).First().Rating;
					double expectedValue = ExpectedWinRatio(rating1, rating2) * 100.0;
					var performance = GetPeformanceEntry(steamId1, steamId2);
					int totalEncounters = performance.Kills + performance.Deaths;
					if (totalEncounters == 0)
						continue;
					double actualValue = (double)performance.Kills / totalEncounters * 100.0;
					double difference = actualValue - expectedValue;
					double absoluteDifference = Math.Abs(difference);
					Console.WriteLine("{0} vs. {1}: {2} encounters", _PlayerData[steamId1].Identity.Name, _PlayerData[steamId2].Identity.Name, totalEncounters);
					Console.Write("{0:0.0}% actual, {1:0.0}% expected, ", actualValue, expectedValue);
					var originalColour = Console.ForegroundColor;
					if (totalEncounters >= 15)
					{
						if (absoluteDifference < 5.0)
							Console.ForegroundColor = ConsoleColor.Green;
						else if (absoluteDifference < 10.0)
							Console.ForegroundColor = ConsoleColor.Yellow;
						else
							Console.ForegroundColor = ConsoleColor.Red;
					}
					Console.WriteLine("{0:0.0}% deviation", difference);
					Console.ForegroundColor = originalColour;
				}
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

		RatingEvaluation EvaluateRatings(PlayerRating[] ratings)
		{
			double error = 0.0;
			foreach (string steamId1 in _PerformanceMatrix.Keys)
			{
				foreach (string steamId2 in _PerformanceMatrix.Keys)
				{
					if (steamId1 == steamId2)
						continue;
					double rating1 = ratings.Where(x => x.Identity.SteamId == steamId1).First().Rating;
					double rating2 = ratings.Where(x => x.Identity.SteamId == steamId2).First().Rating;
					double expectedValue = ExpectedWinRatio(rating1, rating2);
					var performance = GetPeformanceEntry(steamId1, steamId2);
					int totalEncounters = performance.Kills + performance.Deaths;
					if(totalEncounters == 0)
						continue;
					double actualValue = (double)performance.Kills / totalEncounters;
					double errorWeight = Math.Pow(Math.Min((double)totalEncounters / ErrorEvaluationEncounterBarrier, 1.0), 2.0);
					error += errorWeight * Math.Pow(expectedValue - actualValue, 2.0);
				}
			}
			var evaluation = new RatingEvaluation(ratings, error);
			return evaluation;
		}

		double ExpectedWinRatio(double rating1, double rating2)
		{
			return rating1 / (rating1 + rating2);
		}

		PlayerRating[] GetMutatedRatings(PlayerRating[] ratings)
		{
			int index = _Random.Next(_PlayerData.Count);
			var playerRating = ratings[index];
			double newRating = playerRating.Rating + (_Random.Next(2) == 1 ? 1 : -1) * _MutationSpeed;
			newRating = Math.Max(newRating, MinimumRating);
			var newPlayerRating = new PlayerRating(playerRating.Identity, newRating);
			_MutationSpeed = Math.Max(_MutationSpeed - MutationSpeedAdjustment, MinimumMutationSpeed);
			var newRatings = ratings.ToArray();
			newRatings[index] = newPlayerRating;
			return newRatings;
		}
	}
}
