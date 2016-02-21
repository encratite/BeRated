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

		public static string PlayerIds(List<PlayerInfo> team)
		{
			var idString = team.Select(player => player.SteamId);
			return string.Join(",", idString);
		}

		public static RawString MultiLinePlayerList(List<PlayerInfo> team)
		{
			var links = team.Select(player => string.Format("<li>{0}</li>", PlayerLink(player.SteamId, player.Name)));
			string elements = string.Join("\n", links);
			string output = string.Format("<ul>\n{0}\n</ul>", elements);
			return new RawString(output);
		}
	}
}
