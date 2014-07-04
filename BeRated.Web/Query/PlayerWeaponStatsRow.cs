using System;

namespace BeRated.Query
{
	class PlayerWeaponStatsRow
	{
		public string Weapon { get; set; }
		public int Kills { get; set; }
		public int Headshots { get; set; }
		public Decimal HeadshotPercentage { get; set; }
	}
}
