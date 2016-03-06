using System;

namespace BeRated.Model
{
    public class MatchmakingPlayer : PlayerInfo
    {
        public double MatchRating { get; set; }
        public int Games { get; set; }
        public DateTime? LastRound { get; set; }
    }
}
