using System.Collections.Generic;
using BeRated.Database;

namespace BeRated.Model
{
	public class PlayerStatsModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<PlayerWeaponStatsRow> Weapons { get; set; }
		public List<PlayerEncounterStatsRow> Encounters { get; set; }
		public List<PlayerPurchasesRow> Purchases { get; set; }
		public List<PlayerGameModel> Games { get; set; }
	}
}
