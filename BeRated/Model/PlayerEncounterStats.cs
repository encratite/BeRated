namespace BeRated.Model
{
	public class PlayerEncounterStats
	{
		public int OpponentId { get; set; }
		public string OpponentName { get; set; }
		public int Encounters { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public decimal WinRatio { get; set; }
	}
}
