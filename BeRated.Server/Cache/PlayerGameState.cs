using Moserware.Skills;
using Team = BeRated.Common.Team;

namespace BeRated.Cache
{
    class PlayerGameState
    {
        public Team Team { get; set; }

        public Rating PreGameMatchRating { get; set; }

        public Rating PreGameKillRating { get; set; }

        public int? RoundPlayerLeft { get; set; }

        public PlayerGameState(Player player, Team team)
        {
            Team = team;
            PreGameMatchRating = player.MatchRating;
            PreGameKillRating = player.KillRating;
            RoundPlayerLeft = null;
        }
    }
}
