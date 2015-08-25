using System.Collections.Generic;

namespace BeRated.Model
{
	public class PlayerStatsModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int? Days { get; set; }
		public List<PlayerWeaponStatsModel> Weapons { get; set; }
		public List<PlayerPurchasesModel> Purchases { get; set; }
		public List<PlayerEncounterStatsModel> Encounters { get; set; }
		public List<PlayerGameModel> Games { get; set; }
	}
}
