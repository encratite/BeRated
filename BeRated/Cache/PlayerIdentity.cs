namespace BeRated.Cache
{
	class PlayerIdentity
	{
		public string SteamId { get; private set; }
		public string Name { get; private set; }

		public PlayerIdentity(string name, string steamId)
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
