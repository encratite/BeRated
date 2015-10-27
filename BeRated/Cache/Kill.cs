using System;

namespace BeRated.Cache
{
	class Kill
	{
		public DateTime Time { get; set; }
		public PlayerIdentity Killer { get; set; }
		public Team KillerTeam { get; set; }
		public Vector KillerPosition { get; set; }
		public PlayerIdentity Victim { get; set; }
		public Team VictimTeam { get; set; }
		public Vector VictimPosition { get; set; }
		public bool Headshot { get; set; }
		public string Weapon { get; set; }
	}
}
