using System;

namespace BeRated.Common
{
	public enum PlayerTeam
	{
		CounterTerrorist,
		Terrorist,
	}

	public class PlayerKill
	{
		public DateTime Time;
		public PlayerIdentity Killer;
		public Vector KillerPosition;
		public PlayerIdentity Victim;
		public Vector VictimPosition;
		public PlayerTeam KillerTeam;
		public bool Headshot;
		public string Weapon;
	}
}
