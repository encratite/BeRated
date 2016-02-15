using System;
using System.Collections.Generic;

namespace BeRated.Model
{
    public class PlayerGame
	{
		public DateTimeOffset Time { get; set; }
		public int PlayerScore { get; set; }
		public int EnemyScore { get; set; }
		public PlayerGameOutcome Outcome { get; set; }

		public List<PlayerInfo> PlayerTeam { get; set; }
		public List<PlayerInfo> EnemyTeam { get; set; }
	}
}
