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

		public List<Round> Rounds { get; set; }

		public Game(List<Round> rounds)
		{
			Rounds = rounds;
		}
	}
}
