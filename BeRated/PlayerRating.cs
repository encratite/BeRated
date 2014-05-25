using System;

namespace BeRated
{
	public class PlayerRating
	{
		public const int InitialRating = 1000;
		private const int MinimumRating = 0;

		public static int MaximumAdjustment = 25;
		public static int Base = 10;
		public static int ExponentDivisor = 250;

		public int Rating { get; private set; }

		public PlayerRating()
		{
			Rating = InitialRating;
		}

		public PlayerRating(int rating)
		{
			Rating = rating;
		}

		private int GetAdjustedRating(int score, PlayerRating opponent)
		{
			return Rating + (int)Math.Ceiling(MaximumAdjustment * (score - 1.0 / (1.0 + Math.Pow(Base, (double)(opponent.Rating - Rating) / ExponentDivisor))));
		}

		public static void UpdateRatings(PlayerRating winner, PlayerRating loser)
		{
			int winnerRating = winner.GetAdjustedRating(1, loser);
			int loserRating = loser.GetAdjustedRating(0, winner);
			winner.Rating = winnerRating;
			loser.Rating = loserRating;
		}

		public override string ToString()
		{
			return Rating.ToString();
		}
	}
}
