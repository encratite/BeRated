using System.Collections.Generic;

namespace LogAnalyser
{
	class RatingEvaluation
	{
		public readonly List<PlayerRating> Ratings;
		public readonly double Error;

		public RatingEvaluation(List<PlayerRating> ratings, double error)
		{
			Ratings = ratings;
			Error = error;
		}
	}
}
