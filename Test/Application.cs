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
			int winnerDifference;
			int loserDifference;
			PlayerRating.AdjustRatings(winner.Rating, loser.Rating, out winnerDifference, out loserDifference);
			winner.Kills++;
			loser.Deaths++;
			// Console.WriteLine("{0} ({1}, {2}) killed {3} ({4}, {5})", winner, winner.Rating, winnerDifference, loser, loser.Rating, loserDifference);
		}
		
		static void Main(string[] arguments)
		{
			var players = new List<Player>
			{
				new Player(10),
				new Player(11),
				new Player(12),
				new Player(13),
				new Player(14),
				new Player(25),
				new Player(50),
			};

			const int kills = 1000;
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
			foreach (var player in players)
			{
				double? killDeathRatio = null;
				if(player.Deaths > 0)
					killDeathRatio = (double)player.Kills / player.Deaths;
				Console.WriteLine("{0}: {1} ({2})", player.Strength, player.Rating.Rating, killDeathRatio.HasValue ? killDeathRatio.Value.ToString("#0.00") : "N/A");
			}
			Console.ReadLine();
		}
	}
}
