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

	public class PlayerGame
	{
		public DateTime GameTime { get; set; }
		public int PlayerScore { get; set; }
		public int EnemyScore { get; set; }

		public string PlayerTeam
		{
			set
			{
				PlayerTeamList = GetPlayers(value);
            }
		}

		public string EnemyTeam
		{
			set
			{
				PlayerTeamList = GetPlayers(value);
			}
		}

		public string Outcome
		{
			set
			{
				OutcomeEnum = GetOutcome(value);
			}
		}

		public List<GamePlayer> PlayerTeamList { get; private set; }
		public List<GamePlayer> EnemyTeamList { get; private set; }
		public GameOutcome OutcomeEnum { get; private set; }

        private List<GamePlayer> GetPlayers(string playerString)
		{
			var pattern = new Regex("\"\\((\\d+),(?:\\\\\"(.+?)\\\\\"|(.+?))\\)\"[,}]");
			var output = new List<GamePlayer>();
			foreach (Match match in pattern.Matches(playerString))
			{
				var groups = match.Groups;
				int playerId = int.Parse(groups[1].Value);
				string name1 = groups[2].Value;
				string name2 = groups[3].Value;
				string name = !string.IsNullOrEmpty(name1) ? name1 : name2;
				var player = new GamePlayer(playerId, name);
				output.Add(player);
			}
			return output;
		}

		private GameOutcome GetOutcome(string outcome)
		{
			switch (outcome)
			{
				case "loss":
					return GameOutcome.Loss;
				case "win":
					return GameOutcome.Win;
				case "draw":
					return GameOutcome.Draw;
				default:
					throw new ApplicationException("Unknown enum string");
			}
		}
	}
}
