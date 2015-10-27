using System;

namespace BeRated.Cache
{
	class Kill
	{
		public DateTime Time { get; set; }

		public Player Killer { get; set; }

		public Team KillerTeam { get; set; }

		public Player Victim { get; set; }

		public Team VictimTeam { get; set; }

		public bool Headshot { get; set; }

		public string Weapon { get; set; }
	}
}
