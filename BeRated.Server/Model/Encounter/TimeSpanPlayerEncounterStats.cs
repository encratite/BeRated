using System.Collections.Generic;

namespace BeRated.Model.Encounter
{
	public class TimeSpanPlayerEncounterStats : TimeSpanStats
	{
		public List<PlayerEncounterStats> Encounters { get; set; }
	}
}
