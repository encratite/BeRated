using System;
using System.Collections.Generic;

namespace BeRated.Model
{
    public class PlayerGame
	{
        public long Id { get; set; }
		public DateTime Time { get; set; }
        public string Map { get; set; }
		public int PlayerScore { get; set; }
		public int EnemyScore { get; set; }
        public bool IsTerrorist { get; set; }
		public PlayerGameOutcome Outcome { get; set; }

		public List<PlayerGameInfo> PlayerTeam { get; set; }
		public List<PlayerGameInfo> EnemyTeam { get; set; }
	}
}
