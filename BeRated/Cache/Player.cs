using System.Collections.Generic;

namespace BeRated.Cache
{
	class Player
	{
		public string Name { get; set; }

		public string SteamId { get; set; }

		public List<Kill> Kills { get; set; }

		public List<Kill> Deaths { get; set; }

		public Player(PlayerIdentity playerIdentity)
		{
			Name = playerIdentity.Name;
			SteamId = playerIdentity.Name;
			Kills = new List<Kill>();
			Deaths = new List<Kill>();
		}
	}
}
