using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BeRated.Common;
using BeRated.Model;
using RazorEngine.Text;

namespace BeRated.App
{
	public class Helper
	{
		public static string LowerCase(string input)
		{
			if (input != null && input.Length >= 1)
				return input.Substring(0, 1).ToLower() + input.Substring(1);
			else
				return input;
		}

		public static string GetScoreClasses(string baseClass, int score, int otherScore)
		{
			return baseClass + (score > otherScore ? " victory" : "");
		}

		public static string Percentage(decimal? ratio)
		{
			if (ratio.HasValue)
				return ratio.Value.ToString("P1").Replace(" ", "");
			else
				return "-";
		}

		public static string Round(decimal? number)
		{
			if (number.HasValue)
				return number.Value.ToString("0.00");
			else
				return string.Empty;
		}

		public static RawString PlayerLink(string steamId, string name)
		{
			string path = string.Format("/Player?id={0}", steamId);
			var encodedName = HttpUtility.HtmlEncode(name);
            string markup = string.Format("<a href=\"{0}\">{1}</a>", path, encodedName);
			var rawString = new RawString(markup);
            return rawString;
        }

		public static RawString PlayerList(List<PlayerInfo> players)
		{
			var links = players.Select(player => PlayerLink(player.SteamId, player.Name).ToString());
			string markup = string.Join(", ", links);
			var rawString = new RawString(markup);
			return rawString;
		}

		public static string Outcome(GameOutcome outcome)
		{
			switch (outcome)
			{
				case GameOutcome.TerroristsWin:
					return "Terrorists";
				case GameOutcome.CounterTerroristsWin:
					return "Counter-Terrorists";
				case GameOutcome.Draw:
					return "Draw";
			}
			throw new ApplicationException("Invalid outcome.");
		}

		public static string Outcome(PlayerGameOutcome outcome)
		{
			return outcome.ToString();
		}

		public static string PlayerIds(List<PlayerGameInfo> team)
		{
			var idString = team.Select(player => player.SteamId);
			return string.Join(",", idString);
		}

		public static List<RawString> GetTeamRows(List<PlayerGameInfo> counterTerrorists, List<PlayerGameInfo> terrorists)
		{
            var counterTerroristRows = counterTerrorists.Select(player => GetPlayerRow(player, false));
			var terroristRows = terrorists.Select(player => GetPlayerRow(player, true));
            var rows = counterTerroristRows.Concat(terroristRows).ToList();
            return rows;
		}

        private static RawString GetPlayerRow(PlayerGameInfo player, bool terrorist)
        {
            string className = terrorist ? "terroristPlayer" : "counterTerroristPlayer";
            var link = PlayerLink(player.SteamId, player.Name);
            string markup = string.Format("<td class=\"{0}\">{1}</td>\n", className, link);
            markup += string.Format("<td>{0}</td>\n", player.Kills);
            markup += string.Format("<td>{0}</td>\n", player.Deaths);
            return new RawString(markup);
        }
    }
}
