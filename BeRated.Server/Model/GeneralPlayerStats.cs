namespace BeRated.Model
{
	public class GeneralPlayerStats
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal? KillDeathRatio { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		// Only set if Deaths > 0, otherwise null
		public int GamesPlayed { get; set; }
		public decimal GameWinRatio { get; set; }
		public int RoundsPlayed { get; set; }
		public decimal RoundWinRatio { get; set; }
	}
}
