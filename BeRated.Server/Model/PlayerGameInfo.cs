namespace BeRated.Model
{
    public class PlayerGameInfo : PlayerInfo
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }

        public GameRating MatchRating { get; set; }
        public GameRating KillRating { get; set; }
    }
}
