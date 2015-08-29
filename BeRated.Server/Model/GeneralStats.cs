using System.Collections.Generic;

namespace BeRated.Model
{
    public class GeneralStats
    {
		public int? Days { get; set; }
        public List<GeneralPlayerStats> Players { get; set; }
		public List<TeamStats> Teams { get; set; }
    }
}
