using System;

namespace BeRated.Cache
{
	class Disconnect
	{
		public DateTime Time { get; set; }
		public Player Player { get; set; }
		public string Team { get; set; }
		public string Reason { get; set; }
	}
}
