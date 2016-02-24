namespace BeRated.Model
{
	public class PlayerInfo
	{
		public string Name { get; private set; }

		public string SteamId { get; private set; }

		public PlayerInfo(string name, string steamId)
        {
            Name = name;
			SteamId = steamId;
		}
	}
}
