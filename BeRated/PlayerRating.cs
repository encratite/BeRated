using System;

namespace BeRated
{
	public class PlayerRating
	{
		private const int InitialRating = 1000;
		private const int MinimumRating = 0;
		private const int MaximumAdjustment = 50;
		private const int Base = 10;
		private const int ExponentDivisor = 500;

		public int Rating { get; private set; }

		public PlayerRating()
		{
			Rating = InitialRating;
		}

		public static void AdjustRatings(PlayerRating winner, PlayerRating loser)
		{
			int ratingDifference = loser.Rating - winner.Rating;
			int adjustment = (int)Math.Ceiling(MaximumAdjustment * (1.0 - 1.0 / (1.0 + Math.Pow(Base, ratingDifference / ExponentDivisor))));
			int loserRating = Math.Max(loser.Rating - adjustment, MinimumRating);
			adjustment = loser.Rating - loserRating;
			int winnerRating = winner.Rating + adjustment;
			winner.Rating = winnerRating;
			loser.Rating = loserRating;
		}

		public static void AdjustRatings(PlayerRating winner, PlayerRating loser, out int winnerDifference, out int loserDifference)
		{
			int winnerRating = winner.Rating;
			int loserRating = loser.Rating;
			AdjustRatings(winner, loser);
			winnerDifference = winner.Rating - winnerRating;
			loserDifference = loser.Rating - loserRating;
		}

		public override string ToString()
		{
			return Rating.ToString();
		}
	}
}
