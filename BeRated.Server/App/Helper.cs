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

        public static RawString GetScore(Game game)
        {
            string markup = string.Format("<span class=\"{0}\">{1}</span>", GetScoreClasses("counterTerroristScore", game.CounterTerroristScore, game.TerroristScore), game.CounterTerroristScore);
            markup += " - ";
            markup += string.Format("<span class=\"{0}\">{1}</span>", GetScoreClasses("terroristScore", game.TerroristScore, game.CounterTerroristScore), game.TerroristScore);
            return new RawString(markup);
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

        public static RawString GetPlayerList(List<PlayerGameInfo> team, bool terrorists)
		{
			var links = team.OrderBy(player => player.Name).Select(player => string.Format("<li>{0}</li>", PlayerLink(player.SteamId, player.Name)));
			string elements = string.Join("\n", links);
            string className = terrorists ? "terrorists" : "counterTerrorists";
			string markup = string.Format("<ul class=\"{0}\">\n{1}\n</ul>", className, elements);
			return new RawString(markup);
		}

        public static RawString GetGameLink(long id, string map)
        {
            string markup = string.Format("<a href=\"/Game?id={0}\">{1}</a>", id, map);
            return new RawString(markup);
        }

        private static string GetScoreClasses(string baseClass, int score, int otherScore)
		{
			return baseClass + (score > otherScore ? " victory" : "");
		}
    }
}
