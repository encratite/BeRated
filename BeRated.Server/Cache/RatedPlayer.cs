using Moserware.Skills;

namespace BeRated.Cache
{
	class RatedPlayer
	{
		public Player Player { get; private set; }

        public RatingPair MatchRating { get; private set; }
        public RatingPair RoundRating { get; private set; }
		public RatingPair KillRating { get; private set; }

		public RatedPlayer(Player player, Rating preGameMatchRating, Rating preGameRoundRating, Rating preGameKillRating)
		{
			Player = player;
            MatchRating = new RatingPair(preGameMatchRating, player.MatchRating);
            RoundRating = new RatingPair(preGameRoundRating, player.RoundRating);
            KillRating = new RatingPair(preGameKillRating, player.KillRating);
		}
	}
}
