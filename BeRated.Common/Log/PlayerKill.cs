using System;

namespace BeRated
{
	public class PlayerKill
	{
		public DateTime Time;
		public PlayerIdentity Killer;
		public string KillerTeam;
		public Vector KillerPosition;
		public PlayerIdentity Victim;
		public string VictimTeam;
		public Vector VictimPosition;
		public bool Headshot;
		public string Weapon;
	}
}
