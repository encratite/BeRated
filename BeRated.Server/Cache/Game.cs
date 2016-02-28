using System;
using System.Collections.Generic;
using System.Linq;
using BeRated.Common;

namespace BeRated.Cache
{
    class Game
	{
        public long Id { get { return Time.Ticks / 10000000; } }

		public DateTime Time { get { return LastRound.Time; } }

        public string Map { get; private set; }

		public List<Round> Rounds { get; private set; }

        public int TerroristScore { get { return LastRound.TerroristScore; } }
        public int CounterTerroristScore { get { return LastRound.CounterTerroristScore; } }

        public GameOutcome Outcome { get; private set; }

        public List<RatedPlayer> Terrorists { get; private set; }
        public List<RatedPlayer> CounterTerrorists { get; private set; }

        private Round LastRound { get { return Rounds.Last(); } }

		public Game(string map, List<Round> rounds, GameOutcome outcome)
		{
            Map = map;
			Rounds = rounds;
            Outcome = outcome;

            Terrorists = new List<RatedPlayer>();
            CounterTerrorists = new List<RatedPlayer>();
		}

        public override string ToString()
        {
            return string.Format("{0} ({1}v{2})", Time.ToString("g"), Terrorists.Count, CounterTerrorists.Count);
        }

		public RatedPlayer GetRatedPlayer(Player player)
		{
			var players = Terrorists.Concat(CounterTerrorists);
			var ratedPlayer = players.First(r => r.Player == player);
			return ratedPlayer;
		}
    }
}
