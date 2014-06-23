using BeRated.Common;

namespace LogAnalyser
{
	class PlayerRating
	{
		public readonly PlayerIdentity Identity;
		public readonly double Rating;

		public PlayerRating(PlayerIdentity identity, double rating)
		{
			Identity = identity;
			Rating = rating;
		}
	}
}
