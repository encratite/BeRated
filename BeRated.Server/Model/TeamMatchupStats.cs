using System.Collections.Generic;

namespace BeRated.Model
{
	public class TeamMatchupStats
	{
		public List<PlayerInfo> Team1 { get; set; }
		public List<PlayerInfo> Team2 { get; set; }
		public GameOutcomes ImpreciseOutcomes { get; set; }
		public GameOutcomes PreciseOutcomes { get; set; }
	}
}
