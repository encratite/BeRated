using BeRated.Database;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BeRated.Model
{
	class PlayerGameModel
	{
		public DateTime GameTime { get; set; }
		public int PlayerScore { get; set; }
		public int EnemyScore { get; set; }
		public List<GamePlayerModel> PlayerTeam { get; set; }
		public List<GamePlayerModel> EnemyTeam { get; set; }
		public string Outcome { get; set; }

        private PlayerGameModel()
		{
		}

		public PlayerGameModel(PlayerGameHistoryRow row)
		{
			GameTime = row.GameTime;
			PlayerScore = row.PlayerScore;
			EnemyScore = row.EnemyScore;
			PlayerTeam = GetPlayers(row.PlayerTeam);
			EnemyTeam = GetPlayers(row.EnemyTeam);
			Outcome = row.Outcome;
		}

        private List<GamePlayerModel> GetPlayers(string playerString)
		{
			var pattern = new Regex("\"\\((\\d+),(?:\\\\\"(.+?)\\\\\"|(.+?))\\)\"[,}]");
			var output = new List<GamePlayerModel>();
			foreach (Match match in pattern.Matches(playerString))
			{
				var groups = match.Groups;
				int playerId = int.Parse(groups[1].Value);
				string name1 = groups[2].Value;
				string name2 = groups[3].Value;
				string name = !string.IsNullOrEmpty(name1) ? name1 : name2;
				var player = new GamePlayerModel(playerId, name);
				output.Add(player);
			}
			return output;
		}
	}
}
