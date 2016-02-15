namespace BeRated.Model
{
	public class PlayerInfo
	{
		public string SteamId { get; private set; }
		public string Name { get; private set; }

		public PlayerInfo(string steamId, string name)
		{
			SteamId = steamId;
			Name = name;
		}
	}
}
