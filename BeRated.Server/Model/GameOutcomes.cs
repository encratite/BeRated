using BeRated.Common;

namespace BeRated.Model
{
	public class GameOutcomes
	{
		public int Wins { get; set; }
		public int Losses { get; set; }
		public int Draws { get; set; }

		public int Games { get { return Wins + Losses + Draws; } }

		public decimal? WinRatio { get { return Ratio.Get(Wins, Games); } }

		public GameOutcomes()
		{
			Wins = 0;
			Losses = 0;
			Draws = 0;
		}
	}
}
