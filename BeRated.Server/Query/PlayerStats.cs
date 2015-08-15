﻿using BeRated.Query;
using System.Collections.Generic;

namespace BeRated
{
	class PlayerStats
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<PlayerWeaponStatsRow> Weapons { get; set; }
		public List<PlayerEncounterStatsRow> Encounters { get; set; }
		public List<PlayerPurchasesRow> Purchases { get; set; }
		public List<KillDeathRatioHistoryRow> KillDeathRatioHistory { get; set; }
		public List<PlayerGame> Games { get; set; }
	}
}
