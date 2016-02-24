using System;
using System.Collections.Generic;

namespace BeRated.Model
{
    public class PlayerGame
	{
		public DateTimeOffset Time { get; set; }
        public string Map { get; set; }
		public int PlayerScore { get; set; }
		public int EnemyScore { get; set; }
		public PlayerGameOutcome Outcome { get; set; }

        public List<PlayerGameInfo> Terrorists { get; set; }
        public List<PlayerGameInfo> CounterTerrorists { get; set; }

		public List<PlayerGameInfo> PlayerTeam { get; set; }
		public List<PlayerGameInfo> EnemyTeam { get; set; }
	}
}
