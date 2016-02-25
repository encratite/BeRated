using BeRated.Common;

namespace BeRated.Model
{
	public class PlayerEncounterStats : PlayerInfo
	{
		public int Kills { get; set; }
		public int Deaths { get; set; }

		public int Encounters { get { return Kills + Deaths; } }

		public decimal? WinRatio { get { return Ratio.Get(Kills, Encounters); } }

		public PlayerEncounterStats(string name, string steamId)
		{
			SteamId = steamId;
			Name = name;
			Kills = 0;
			Deaths = 0;
		}
	}
}
