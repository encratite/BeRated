using System;
using System.Collections.Generic;
using System.Linq;

namespace BeRated.Cache
{
    class Game
	{
		public DateTimeOffset Time { get { return LastRound.Time; } }

		public List<Round> Rounds { get; private set; }

        public int TerroristScore { get { return LastRound.TerroristScore; } }
        public int CounterTerroristScore { get { return LastRound.CounterTerroristScore; } }

        public GameOutcome Outcome { get; private set; }

        public List<Player> Terrorists { get; private set; }
        public List<Player> CounterTerrorists { get; private set; }

        private Round LastRound { get { return Rounds.Last(); } }

		public Game(List<Round> rounds, GameOutcome outcome)
		{
			Rounds = rounds;
            Outcome = outcome;

            Terrorists = new List<Player>();
            CounterTerrorists = new List<Player>();
		}
	}
}
