using Moserware.Skills;

namespace BeRated.Cache
{
	class RatedPlayer
	{
		public Player Player { get; private set; }

        public Rating PreGameMatchRating { get; private set; }
        public Rating PostGameMatchRating { get; private set; }

		public Rating PreGameKillRating { get; private set; }
		public Rating PostGameKillRating { get; private set; }

		public RatedPlayer(Player player, Rating preGameMatchRating, Rating preGameKillRating)
		{
			Player = player;
            PreGameMatchRating = preGameMatchRating;
            PostGameMatchRating = player.MatchRating;
			PreGameKillRating = preGameKillRating;
			PostGameKillRating = player.KillRating;
		}
	}
}
