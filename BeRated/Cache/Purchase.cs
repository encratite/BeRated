using System;

namespace BeRated.Cache
{
	class Purchase
	{
		public DateTime Time { get; set; }
		public Player Player { get; set; }
		public Team Team { get; set; }
		public string Item { get; set; }
	}
}
