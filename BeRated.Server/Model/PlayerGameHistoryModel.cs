using System;

namespace BeRated.Model
{
	public class PlayerGameHistoryModel
	{
		public DateTime GameTime { get; set; }
		public int PlayerScore { get; set; }
		public int EnemyScore { get; set; }
		public string PlayerTeam { get; set; }
		public string EnemyTeam { get; set; }
		public string Outcome { get; set; }
	}
}
