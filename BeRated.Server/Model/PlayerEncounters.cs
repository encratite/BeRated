using System.Collections.Generic;

namespace BeRated.Model
{
	public class PlayerEncounters : PlayerInfo
	{
		public List<PlayerEncounterStats> Encounters { get; set; }
	}
}
