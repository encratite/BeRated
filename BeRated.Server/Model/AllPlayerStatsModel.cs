using System.Collections.Generic;

namespace BeRated.Model
{
    public class AllPlayerStatsModel
    {
		public int? Days { get; set; }
        public List<GeneralPlayerStatsModel> Players { get; set; }
    }
}
