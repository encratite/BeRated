using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BeRated.Model
{
	public enum GameOutcome
	{
		Loss,
		Win,
		Draw,
	}

	public class PlayerGameModel
	{
		public DateTime GameTime { get; set; }
		public int PlayerScore { get; set; }
		public int EnemyScore { get; set; }
		public List<GamePlayerModel> PlayerTeam { get; set; }
		public List<GamePlayerModel> EnemyTeam { get; set; }
		public GameOutcome Outcome { get; set; }

        private PlayerGameModel()
		{
		}

		public PlayerGameModel(PlayerGameHistoryModel game)
		{
			GameTime = game.GameTime;
			PlayerScore = game.PlayerScore;
			EnemyScore = game.EnemyScore;
			PlayerTeam = GetPlayers(game.PlayerTeam);
			EnemyTeam = GetPlayers(game.EnemyTeam);
			switch (game.Outcome)
			{
				case "loss":
					Outcome = GameOutcome.Loss;
					break;

				case "win":
					Outcome = GameOutcome.Win;
					break;

				case "draw":
					Outcome = GameOutcome.Draw;
					break;

				default:
					throw new ApplicationException("Unknown enum string");
			}
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
