using System.Collections.Generic;
using System.Linq;
using System.Web;
using BeRated.Model;
using RazorEngine.Text;

namespace BeRated.App
{
	public class Helper
	{
		public static string Percentage(decimal? ratio)
		{
			if (ratio.HasValue)
				return ratio.Value.ToString("P1").Replace(" ", "");
			else
				return "-";
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

		public static string Outcome(string outcome)
		{
			return char.ToUpper(outcome[0]).ToString() + outcome.Substring(1);
		}

		public static string PlayerIds(List<PlayerInfo> team)
		{
			var idString = team.Select(player => player.SteamId);
			return string.Join(",", idString);
		}
	}
}
