using System.Collections.Generic;
using System.Linq;
using System.Web;
using BeRated.Model;
using RazorEngine.Text;

namespace BeRated.App
{
	public class Helper
	{
		public static RawString PlayerLink(int id, string name, int? days)
		{
			string path = string.Format("/Player?id={0}", id);
			if (days.HasValue)
				path += string.Format("&days={0}", days.Value);
			var encodedName = HttpUtility.HtmlEncode(name);
            string markup = string.Format("<a href=\"{0}\">{1}</a>", path, encodedName);
			var rawString = new RawString(markup);
            return rawString;
        }

		public static RawString PlayerList(List<GamePlayer> players, int? days)
		{
			var links = players.Select(player => PlayerLink(player.Id, player.Name, days).ToString());
			string markup = string.Join(", ", links);
			var rawString = new RawString(markup);
			return rawString;
		}

		public static string Outcome(string outcome)
		{
			return char.ToUpper(outcome[0]).ToString() + outcome.Substring(1);
		}
	}
}
