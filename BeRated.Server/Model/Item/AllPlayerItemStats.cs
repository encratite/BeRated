using System.Collections.Generic;

namespace BeRated.Model.Item
{
	public class AllPlayerItemStats : BasePlayerStats
	{
		public List<TimeSpanPlayerItemStats> Items { get; set; }
	}
}
