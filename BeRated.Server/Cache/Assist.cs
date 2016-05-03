using BeRated.Common;
using System;

namespace BeRated.Cache
{
	class Assist
	{
		public DateTime Time { get; set; }
		public Player Assistant { get; set; }
		public Team AssistantTeam { get; set; }
		public Player Victim { get; set; }
		public Team VictimTeam { get; set; }
	}
}
