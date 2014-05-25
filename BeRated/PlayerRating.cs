using System;

namespace BeRated
{
	public class PlayerRating
	{
		public const int InitialRating = 1000;
		private const int MinimumRating = 0;

		public static int MinimumAdjustmentFactor = 10;
		public static int MaximumAdjustmentFactor = 40;

		public static int MaximumAdjustmentFactorRating = 1400;
		public static int MinimumAdjustmentFactorRating = 1800;

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

		private int GetAdjustedRating(int score, PlayerRating opponent)
		{
			double adjustmentFactor;
			if (Rating <= MaximumAdjustmentFactorRating)
				adjustmentFactor = MaximumAdjustmentFactor;
			else if (Rating >= MinimumAdjustmentFactorRating)
				adjustmentFactor = MinimumAdjustmentFactor;
			else
				adjustmentFactor = MaximumAdjustmentFactor - (double)(Rating - MaximumAdjustmentFactorRating) / (MinimumAdjustmentFactorRating - MaximumAdjustmentFactorRating) * (MaximumAdjustmentFactor - MinimumAdjustmentFactor);
			return Rating + (int)Math.Round(MaximumAdjustmentFactor * (score - 1.0 / (1.0 + Math.Pow(Base, (double)(opponent.Rating - Rating) / ExponentDivisor))));
		}

		public static void UpdateRatings(PlayerRating winner, PlayerRating loser)
		{
			InflationRating(winner, loser);
		}

		private static void InflationRating(PlayerRating winner, PlayerRating loser)
		{
			int winnerRating = winner.GetAdjustedRating(1, loser);
			int loserRating = loser.GetAdjustedRating(0, winner);
			int gain = winnerRating - winner.Rating;
			int loss = loserRating - loser.Rating;
			winner.Rating = winnerRating;
			loser.Rating = loserRating;
		}

		private static void TransactionRating(PlayerRating winner, PlayerRating loser)
		{
			int ratingDifference = loser.Rating - winner.Rating;
			int adjustment = (int)Math.Round(MaximumAdjustmentFactor * (1.0 - 1.0 / (1.0 + Math.Pow(Base, (double)ratingDifference / ExponentDivisor))));
			int loserRating = Math.Max(loser.Rating - adjustment, MinimumRating);
			adjustment = loser.Rating - loserRating;
			int winnerRating = winner.Rating + adjustment;
			winner.Rating = winnerRating;
			loser.Rating = loserRating;
		}

		public override string ToString()
		{
			return Rating.ToString();
		}
	}
}
