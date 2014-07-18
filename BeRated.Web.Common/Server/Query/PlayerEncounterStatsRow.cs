namespace BeRated.Query
{
	class PlayerEncounterStatsRow
	{
		public int OpponentId { get; set; }
		public string OpponentName { get; set; }
		public int Encounters { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public decimal WinPercentage { get; set; }
	}
}
