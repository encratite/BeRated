using System;

namespace BeRated.Cache
{
	class Purchase
	{
		public DateTime Time { get; set; }
		public PlayerIdentity Player { get; set; }
		public string Team { get; set; }
		public string Item { get; set; }
	}
}
