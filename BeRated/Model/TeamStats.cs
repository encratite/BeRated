using System.Collections.Generic;

namespace BeRated.Model
{
	public class TeamStats
	{
		public string Players
		{
			set
			{
				PlayerInfo = PlayerGame.GetPlayers(value);
			}
		}

		public List<PlayerInfo> PlayerInfo { get; private set; }
		public int Games { get; set; }
		public int Wins { get; set; }
		public int Losses { get; set; }
		public int Draws { get; set; }
		public decimal? WinRatio { get; set; }
	}
}
