using System.Collections.Generic;

namespace BeRated.Model
{
    public class AllPlayerStats
    {
		public int? Days { get; set; }
        public List<GeneralPlayerStats> Players { get; set; }
    }
}
