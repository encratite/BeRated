namespace BeRated.Model
{
	public class GameOutcomes
	{
		public int Games { get; set; }
		public int Wins { get; set; }
		public int Losses { get; set; }
		public int Draws { get; set; }
		public decimal? WinRatio { get; set; }
	}
}
