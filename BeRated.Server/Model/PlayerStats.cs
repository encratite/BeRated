using System.Collections.Generic;

namespace BeRated.Model
{
	public class PlayerStats
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<PlayerWeaponStats> Weapons { get; set; }
		public List<PlayerItemStats> Purchases { get; set; }
		public List<PlayerEncounterStats> Encounters { get; set; }
		public List<PlayerGame> Games { get; set; }
	}
}
