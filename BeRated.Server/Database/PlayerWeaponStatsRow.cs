namespace BeRated.Database
{
	public class PlayerWeaponStatsRow
	{
		public string Weapon { get; set; }
		public int Kills { get; set; }
		public int Headshots { get; set; }
		public decimal HeadshotPercentage { get; set; }
	}
}
