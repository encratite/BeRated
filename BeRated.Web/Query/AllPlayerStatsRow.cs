using System;

namespace BeRated.Query
{
	class AllPlayerStatsRow
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		// Only set if Deaths > 0
		public Decimal? KillDeathRatio { get; set; }
	}
}
