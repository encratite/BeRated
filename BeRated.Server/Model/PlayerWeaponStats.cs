using BeRated.Common;

namespace BeRated.Model
{
	public class PlayerWeaponStats
	{
		public string Weapon { get; set; }
		public int Kills { get; set; }
		public int HeadshotKills { get; set; }
        public int PenetrationKills { get; set; }

		public decimal HeadshotKillRatio { get { return Ratio.Get(HeadshotKills, Kills).Value; } }
        public decimal PenetrationKillRatio { get { return Ratio.Get(PenetrationKills, Kills).Value; } }

		private PlayerWeaponStats()
		{
		}

		public PlayerWeaponStats(string weapon)
		{
			Weapon = weapon;
			Kills = 0;
			HeadshotKills = 0;
            PenetrationKills = 0;
		}
	}
}
