using System;
using System.Collections.Generic;

namespace BeRated.Cache
{
	class Round
	{
		public DateTime Time { get; set; }
		public Team TriggeringTeam { get; set; }
		public string SfuiNotice { get; set; }
		public int TerroristScore { get; set; }
		public int CounterTerroristScore { get; set; }
        public List<Kill> Kills { get; set; }
	}
}
