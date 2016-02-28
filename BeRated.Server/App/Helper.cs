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
        private const string CounterTerroristClass = "counterTerrorists";
        private const string TerroristClass = "terrorists";

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
				return "-";
		}

		public static string LowerCase(string input)
		{
			if (input != null && input.Length >= 1)
				return input.Substring(0, 1).ToLower() + input.Substring(1);
			else
				return input;
		}

        public static RawString GetScore(Game game)
        {
            return GetScore(game.CounterTerroristScore, game.TerroristScore);
        }

        public static RawString GetScore(int counterTerroristScore, int terroristScore)
        {
            string markup = string.Format("<span class=\"{0}\">{1}</span>", GetScoreClasses(CounterTerroristClass, counterTerroristScore, terroristScore), counterTerroristScore);
            markup += " - ";
            markup += string.Format("<span class=\"{0}\">{1}</span>", GetScoreClasses(TerroristClass, terroristScore, counterTerroristScore), terroristScore);
            return new RawString(markup);
        }

        public static RawString GetPlayerLink(PlayerInfo player, bool? isTerrorist = null)
		{
			string path = string.Format("/Matches?id={0}", player.SteamId);
			var encodedName = HttpUtility.HtmlEncode(player.Name);
            string markup;
            if (isTerrorist.HasValue)
            {
                string className = isTerrorist.Value ? TerroristClass : CounterTerroristClass;
                markup = string.Format("<a class=\"{0}\" href=\"{1}\">{2}</a>", className, path, encodedName);
            }
            else
            {
                markup = string.Format("<a href=\"{0}\">{1}</a>", path, encodedName);
            }
			var rawString = new RawString(markup);
            return rawString;
        }

		public static RawString PlayerList(List<PlayerInfo> players)
		{
			var links = players.Select(player => GetPlayerLink(player).ToString());
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

        public static RawString GetTeam(Team team)
        {
            string markup;
            switch (team)
            {
                case Common.Team.CounterTerrorist:
                    markup = string.Format("<span class=\"{0}\">Counter-Terrorists</span>", CounterTerroristClass);
                    break;
                case Common.Team.Terrorist:
                    markup = string.Format("<span class=\"{0}\">Terrorists</span>", TerroristClass);
                    break;
                default:
                    markup = "Unknown";
                    break;
            }
            return new RawString(markup);
        }

		public static string GetOutcome(PlayerGameOutcome outcome)
		{
			return outcome.ToString();
		}

		public static string GetPlayerIds(List<PlayerGameInfo> team)
		{
			var idString = team.Select(player => player.SteamId);
			return string.Join(",", idString);
		}

        public static RawString GetPlayerList(List<PlayerInfo> team, bool? terrorists = null)
		{
			var links = team.OrderBy(player => player.Name).Select(player => string.Format("<li>{0}</li>", GetPlayerLink(player)));
			string elements = string.Join("\n", links);
			string markup;
			if (terrorists.HasValue)
			{
				string className = terrorists.Value ? TerroristClass : CounterTerroristClass;
				markup = string.Format("<ul class=\"{0}\">\n{1}\n</ul>", className, elements);
			}
			else
			{
				markup = string.Format("<ul>\n{0}\n</ul>", elements);
			}
			return new RawString(markup);
		}

		public static RawString GetPlayerList(List<PlayerGameInfo> team, bool? terrorists = null)
		{
			return GetPlayerList(team.OfType<PlayerInfo>().ToList(), terrorists);
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
