using System;

namespace BeRated.Query
{
	class AllPlayerStatsRow
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public int TeamKills { get; set; }
		// Only set if Deaths > 0
		public Decimal? KillDeathRatio { get; set; }
		public int RoundsPlayed { get; set; }
		public Decimal WinPercentage { get; set; }
		public int RoundsPlayedTerrorist { get; set; }
		public Decimal WinPercentageTerrorist { get; set; }
		public int RoundsPlayedCounterTerrorist { get; set; }
		public Decimal WinPercentageCounterTerrorist { get; set; }
		public int GamesPlayed { get; set; }
		public Decimal GameWinPercentage { get; set; }
	}
}
