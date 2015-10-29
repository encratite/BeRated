using System;
using System.Text.RegularExpressions;

namespace BeRated.Cache
{
	class MatchReader
	{
		private int _Offset = 1;
		private Match _Match;

		public MatchReader(Match match)
		{
			_Match = match;
		}

		public string String()
		{
			string output = _Match.Groups[_Offset].Value;
			_Offset++;
			return output;
		}

		public int Int()
		{
			string intString = String();
			int output = int.Parse(intString);
			return output;
		}

		public Team Team()
		{
			string team = String();
			if (team == LogParser.UnassignedTeam)
				return BeRated.Cache.Team.Unassigned;
			else if (team == LogParser.TerroristTeam)
				return BeRated.Cache.Team.Terrorist;
			else if (team == LogParser.CounterTerroristTeam)
				return BeRated.Cache.Team.CounterTerrorist;
			else if (team == LogParser.SpectatorTeam)
				return BeRated.Cache.Team.Spectator;
			else
				throw new ArgumentException("Invalid team string");
		}
	}
}
