using System.Collections.Generic;

namespace BeRated.Model
{
	public class PlayerEncounters : PlayerInfo
	{
		public List<PlayerEncounterStats> Encounters { get; private set; }

        public PlayerEncounters(string name, string steamId, List<PlayerEncounterStats> encounters)
            : base(name, steamId)
        {
            Encounters = encounters;
        }
	}
}
