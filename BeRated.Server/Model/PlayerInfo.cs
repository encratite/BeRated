namespace BeRated.Model
{
	public class PlayerInfo
	{
		public string SteamId { get; set; }
		public string Name { get; set; }

		public PlayerInfo()
		{
		}

		public PlayerInfo(string steamId, string name)
		{
			SteamId = steamId;
			Name = name;
		}
	}
}
