using System;

namespace BeRated.Query
{
	class AllPlayerStatsRow
	{
		public int Id;
		public string SteamId;
		public string Name;
		public int Kills;
		public int Deaths;
		// Only set if Deaths > 0
		public Decimal? KillDeathRatio;
	}
}
