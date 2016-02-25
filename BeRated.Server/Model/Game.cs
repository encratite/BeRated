using System;
using System.Collections.Generic;
using BeRated.Common;

namespace BeRated.Model
{
	public class Game
	{
        public long Id { get; set; }
		public DateTime Time { get; set; }
        public string Map { get; set; }
		public int TerroristScore { get; set; }
		public int CounterTerroristScore { get; set; }
		public GameOutcome Outcome { get; set; }

		public List<PlayerGameInfo> Terrorists { get; set; }
		public List<PlayerGameInfo> CounterTerrorists { get; set; }
	}
}
