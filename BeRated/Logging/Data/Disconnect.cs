using System;

namespace BeRated.Logging.Data
{
	public class Disconnect
	{
		public DateTime Time { get; set; }
		public PlayerIdentity Player { get; set; }
		public string Team { get; set; }
		public string Reason { get; set; }
	}
}
