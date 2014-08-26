using System;

namespace BeRated.Query
{
	class PlayerGameHistoryRow
	{
		public DateTime GameTime { get; set; }
		public int PlayerScore { get; set; }
		public int EnemyScore { get; set; }
		public string PlayerTeam { get; set; }
		public string EnemyTeam { get; set; }
		public string Outcome { get; set; }
	}
}
