using System;
using System.Collections.Generic;
using System.Linq;

namespace BeRated.Cache
{
    class Game
	{
		public DateTimeOffset Time
		{
			get
			{
				return Rounds.Last().Time;
            }
		}

		public List<Round> Rounds { get; private set; }

        public GameOutcome Outcome { get; private set; }

        public List<Player> Terrorists { get; private set; }
        public List<Player> CounterTerrorists { get; private set; }

		public Game(List<Round> rounds, GameOutcome outcome)
		{
			Rounds = rounds;
            Outcome = outcome;

            Terrorists = new List<Player>();
            CounterTerrorists = new List<Player>();
		}
	}
}
