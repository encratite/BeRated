using System.Collections.Generic;

namespace BeRated.Model
{
    public class MatchmakingResult
    {
        public double Quality { get; set; }

        public List<PlayerInfo> Team1 { get; set; }
        public List<PlayerInfo> Team2 { get; set; }
    }
}
