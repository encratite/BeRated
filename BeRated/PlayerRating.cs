using System;

namespace BeRated
{
	public class PlayerRating
	{
		public const int InitialRating = 1000;
		private const int MinimumRating = 0;

		public static int MaximumAdjustment = 100;
		public static int Base = 10;
		public static int ExponentDivisor = 500;

		public int Rating { get; private set; }

		public PlayerRating()
		{
			Rating = InitialRating;
		}

		public PlayerRating(int rating)
		{
			Rating = rating;
		}

		public static void UpdateRatings(PlayerRating winner, PlayerRating loser)
		{
			int ratingDifference = loser.Rating - winner.Rating;
			int adjustment = (int)Math.Ceiling(MaximumAdjustment * (1.0 - 1.0 / (1.0 + Math.Pow(Base, (double)ratingDifference / ExponentDivisor))));
			int loserRating = Math.Max(loser.Rating - adjustment, MinimumRating);
			adjustment = loser.Rating - loserRating;
			int winnerRating = winner.Rating + adjustment;
			winner.Rating = winnerRating;
			loser.Rating = loserRating;
		}

		public static void UpdateRatings(PlayerRating winner, PlayerRating loser, out int winnerDifference, out int loserDifference)
		{
			int winnerRating = winner.Rating;
			int loserRating = loser.Rating;
			UpdateRatings(winner, loser);
			winnerDifference = winner.Rating - winnerRating;
			loserDifference = loser.Rating - loserRating;
		}

		public override string ToString()
		{
			return Rating.ToString();
		}
	}
}
