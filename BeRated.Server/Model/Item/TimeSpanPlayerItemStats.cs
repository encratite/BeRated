using System.Collections.Generic;

namespace BeRated.Model.Item
{
	public class TimeSpanPlayerItemStats : TimeSpanStats
	{
		public List<PlayerItemStats> Purchases { get; set; }
	}
}
