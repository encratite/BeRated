using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeRated.Logging;

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
