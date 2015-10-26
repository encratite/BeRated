using System;

namespace BeRated.Logging.Data
{
	public class Disconnect
	{
		public DateTime Time;
		public PlayerIdentity Player;
		public string Team;
		public string Reason;
	}
}
