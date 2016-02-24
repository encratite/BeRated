namespace BeRated.Model
{
    public class PlayerGameInfo : PlayerInfo
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }

        public PlayerGameInfo(string name, string steamId, int kills, int deaths)
            : base(name, steamId)
        {
            Kills = kills;
            Deaths = deaths;
		}
    }
}
