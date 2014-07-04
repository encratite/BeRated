using BeRated.Query;
using System.Collections.Generic;

namespace BeRated
{
	class PlayerStats
	{
		public List<PlayerWeaponStatsRow> Weapons { get; set; }
		public List<PlayerEncounterStatsRow> Encounters { get; set; }
	}
}
