using BeRated.Common;
using System;
using System.Collections.Generic;

namespace BeRated.Model
{
    public class Round
    {
        public DateTime Time { get; set; }
        public Team Winner { get; set; }
        public int TerroristScore { get; set; }
		public int CounterTerroristScore { get; set; }
        public List<Kill> Kills { get; set; }
    }
}
