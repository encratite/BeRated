using Moserware.Skills;
using Team = BeRated.Common.Team;

namespace BeRated.Cache
{
    class PlayerGameState
    {
        public Team Team { get; set; }

        public Rating PreGameMatchRating { get; private set; }
        public Rating PreGameRoundRating { get; private set; }
        public Rating PreGameKillRating { get; private set; }

        public int? RoundPlayerLeft { get; set; }

		public int Kills { get; set; }
		public int Deaths { get; set; }

        public PlayerGameState(Player player, Team team)
        {
            Team = team;
            PreGameMatchRating = player.MatchRating;
            PreGameRoundRating = player.RoundRating;
            PreGameKillRating = player.KillRating;
            RoundPlayerLeft = null;
			Kills = 0;
			Deaths = 0;
        }
    }
}
