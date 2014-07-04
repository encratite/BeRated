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
		public Decimal? KillDeathRatio;
	}
}
