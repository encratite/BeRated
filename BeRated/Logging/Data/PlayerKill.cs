﻿using System;

namespace BeRated.Logging.Data
{
	public class PlayerKill
	{
		public DateTime Time { get; set; }
		public PlayerIdentity Killer { get; set; }
		public string KillerTeam { get; set; }
		public Vector KillerPosition { get; set; }
		public PlayerIdentity Victim { get; set; }
		public string VictimTeam { get; set; }
		public Vector VictimPosition { get; set; }
		public bool Headshot { get; set; }
		public string Weapon { get; set; }
	}
}
