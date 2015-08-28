using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace BeRated.Model
{
	public class PlayerGame
	{
		public DateTime GameTime { get; set; }
		public int PlayerScore { get; set; }
		public int EnemyScore { get; set; }
		public string Outcome { get; set; }

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
				EnemyTeamList = GetPlayers(value);
			}
		}

		public List<PlayerInfo> PlayerTeamList { get; private set; }
		public List<PlayerInfo> EnemyTeamList { get; private set; }

        private List<PlayerInfo> GetPlayers(string playerString)
		{
			var pattern = new Regex("\"\\((\\d+),(?:\\\\\"(.+?)\\\\\"|(.+?))\\)\"[,}]");
			var players = new List<PlayerInfo>();
			foreach (Match match in pattern.Matches(playerString))
			{
				var groups = match.Groups;
				int playerId = int.Parse(groups[1].Value);
				string name1 = groups[2].Value;
				string name2 = groups[3].Value;
				string name = !string.IsNullOrEmpty(name1) ? name1 : name2;
				var player = new PlayerInfo(playerId, name);
				players.Add(player);
			}
			players = players.OrderBy(player => player.Name).ToList();
			return players;
		}
	}
}
