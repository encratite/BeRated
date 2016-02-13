using BeRated.Common;

namespace BeRated.Model
{
	public class PlayerEncounterStats
	{
		public string OpponentName { get; set; }
		public string OpponentSteamId { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }

		public int Encounters { get { return Kills + Deaths; } }

		public decimal? WinRatio { get { return Ratio.Get(Kills, Encounters); } }

		private PlayerEncounterStats()
		{
		}

		public PlayerEncounterStats(string name, string steamId)
		{
			OpponentSteamId = steamId;
			OpponentName = name;
			Kills = 0;
			Deaths = 0;
		}
	}
}
