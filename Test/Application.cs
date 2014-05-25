using BeRated;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
	class Player
	{
		public readonly int Strength;
		public readonly PlayerRating Rating = new PlayerRating();
		public int Kills = 0;
		public int Deaths = 0;

		public Player(int strength)
		{
			Strength = strength;
		}

		public override string ToString()
		{
			return Strength.ToString();
		}
	}

	class Application
	{
		private static Random _Random = new Random();

		private static Player GetPlayer(List<Player> players)
		{
			int index = _Random.Next(players.Count);
			var player = players[index];
			return player;
		}

		private static void ProcessKill(Player winner, Player loser)
		{
			PlayerRating.UpdateRatings(winner.Rating, loser.Rating);
			winner.Kills++;
			loser.Deaths++;
		}

		private static void EvaluateParameters(int kills = 10000, int? maximumAdjustment = null, int? ratingBase = null, int? exponentDivisor = null, bool hasTarget = false)
		{
			if(maximumAdjustment.HasValue)
				PlayerRating.MaximumAdjustment = maximumAdjustment.Value;
			if(ratingBase.HasValue)
				PlayerRating.Base = ratingBase.Value;
			if(exponentDivisor.HasValue)
				PlayerRating.ExponentDivisor = exponentDivisor.Value;

			var players = new List<Player>
			{
				new Player(10),
				new Player(15),
				new Player(20),
				new Player(25),
				new Player(30),
				new Player(35),
				new Player(40),
				new Player(45),
				new Player(50),
			};

			const int topRatingTarget = 1400;

			var random = new Random();
			for (int i = 0; i < kills; i++)
			{
				var player1 = GetPlayer(players);
				var remainingPlayers = players.Except(new[] { player1 }).ToList();
				var player2 = GetPlayer(remainingPlayers);
				int totalStrength = player1.Strength + player2.Strength;
				double player1Probability = (double)player1.Strength / totalStrength;
				double roll = _Random.NextDouble();
				if (roll < player1Probability)
					ProcessKill(player1, player2);
				else
					ProcessKill(player2, player1);
			}
			players = players.OrderByDescending(player => player.Rating.Rating).ToList();
			if (!hasTarget || players.First().Rating.Rating >= topRatingTarget)
			{
				if(hasTarget)
					Console.WriteLine("maximumAdjustment = {0}, ratingBase = {1}, exponentDivisor = {2}", maximumAdjustment, ratingBase, exponentDivisor);
				foreach (var player in players)
				{
					double? killDeathRatio = null;
					if (player.Deaths > 0)
						killDeathRatio = (double)player.Kills / player.Deaths;
					Console.WriteLine("{0}: {1} ({2})", player.Strength, player.Rating.Rating, killDeathRatio.HasValue ? killDeathRatio.Value.ToString("#0.00") : "N/A");
				}
			}
		}

		private static void ExploreParameters()
		{
			for (int maximumAdjustment = 20; maximumAdjustment <= 100; maximumAdjustment += 10)
			{
				for (int ratingBase = 5; ratingBase <= 50; ratingBase += 5)
				{
					for (int exponentDivisor = 200; exponentDivisor <= 1000; exponentDivisor += 200)
					{
						EvaluateParameters(100000, maximumAdjustment, ratingBase, exponentDivisor, true);
					}
				}
			}
		}
		
		static void Main(string[] arguments)
		{
			EvaluateParameters();
			// ExploreParameters();
			Console.WriteLine("Press enter to continue");
			Console.ReadLine();
		}
	}
}
