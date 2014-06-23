using System.Collections.Generic;

namespace LogAnalyser
{
	class RatingEvaluation
	{
		public readonly PlayerRating[] Ratings;
		public readonly double Error;

		public RatingEvaluation(PlayerRating[] ratings, double error)
		{
			Ratings = ratings;
			Error = error;
		}
	}
}
