namespace BeRated.Model
{
	public class PlayerInfo
	{
		public string Name { get; set; }
		public string SteamId { get; set; }

        public PlayerInfo()
        {
        }

		public PlayerInfo(string name, string steamId)
        {
            Name = name;
			SteamId = steamId;
		}
	}
}
