using System;

namespace BeRated.Model
{
	public class GeneralPlayerStats : PlayerInfo
	{
		public DateTime? FirstRoundTime { get; set; }
        public DateTime? LastRoundTime { get; set; }

		public decimal? KillsPerRound { get; set; }
		public decimal? KillDeathRatio { get; set; }

		public int Kills { get; set; }
		public int Deaths { get; set; }

		public int GamesPlayed { get; set; }
		public decimal? GameWinRatio { get; set; }

		public int RoundsPlayed { get; set; }
		public decimal? RoundWinRatio { get; set; }
	}
}
