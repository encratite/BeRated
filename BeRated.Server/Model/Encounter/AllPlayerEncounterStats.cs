using System.Collections.Generic;

namespace BeRated.Model.Encounter
{
	public class AllPlayerEncounterStats : BasePlayerStats
	{
		public List<TimeSpanPlayerEncounterStats> Encounters { get; set; }
	}
}
