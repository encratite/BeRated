namespace BeRated
{
	public class PlayerIdentity
	{
		public readonly string SteamId;
		public readonly string Name;

		public PlayerIdentity(string steamId, string name)
		{
			SteamId = steamId;
			Name = name;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", Name, SteamId);
		}
	}
}
