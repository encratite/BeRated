using System.Collections.Generic;

namespace BeRated.Cache
{
	class Player
	{
		public string Name { get; set; }

		public string SteamId { get; set; }

		public List<Kill> Kills { get; set; }

		public List<Kill> Deaths { get; set; }

		public Player(string name, string steamId)
		{
			Name = name;
			SteamId = steamId;
			Kills = new List<Kill>();
			Deaths = new List<Kill>();
		}
	}
}
