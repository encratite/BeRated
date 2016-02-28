using Moserware.Skills;

namespace BeRated.Cache
{
	class RatedPlayer
	{
		public Player Player { get; private set; }

		public Rating PreGameKillRating { get; private set; }
		public Rating PostGameKillRating { get; private set; }

		public RatedPlayer(Player player, Rating preGameKillRating)
		{
			Player = player;
			PreGameKillRating = preGameKillRating;
			PostGameKillRating = player.KillRating;
		}
	}
}
