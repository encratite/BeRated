using System;
using System.Text.RegularExpressions;

namespace BeRated
{
    public static class LogParser
    {
		public const string BotId = "BOT";

		static Regex KillPattern = new Regex("^L (\\d{2})\\/(\\d{2})\\/(\\d+) - (\\d{2}):(\\d{2}):(\\d{2}): \"(.+?)<\\d+><(.+?)><(TERRORIST|CT)>\" \\[(-?\\d+) (-?\\d+) (-?\\d+)\\] killed \"(.+?)<\\d+><(.+?)><(?:TERRORIST|CT)>\" \\[(-?\\d+) (-?\\d+) (-?\\d+)\\] with \"(.+?)\"( \\(headshot\\))?");

		public static PlayerKill ReadPlayerKill(string line)
		{
			var match = KillPattern.Match(line);
			if (!match.Success)
				return null;
			var groups = match.Groups;
			int offset = 1;
			Func<string> getString = () => groups[offset++].Value;
			Func<int> getInt = () => Convert.ToInt32(getString());
			int month = getInt();
			int day = getInt();
			int year = getInt();
			int hour = getInt();
			int minute = getInt();
			int second = getInt();
			string killerName = getString();
			string killerSteamId = getString();
			var killerTeam = getString() == "CT" ? PlayerTeam.CounterTerrorist : PlayerTeam.Terrorist;
			int killerX = getInt();
			int killerY = getInt();
			int killerZ = getInt();
			string victimName = getString();
			string victimSteamId = getString();
			int victimX = getInt();
			int victimY = getInt();
			int victimZ = getInt();
			string weapon = getString();
			bool headshot = getString() != "";
			var output = new PlayerKill
			{
				Time = new DateTime(year, month, day, hour, minute, second),
				Killer = new PlayerIdentity(killerSteamId, killerName),
				KillerPosition = new Vector(killerX, killerY, killerZ),
				Victim = new PlayerIdentity(victimSteamId, victimName),
				VictimPosition = new Vector(victimX, victimY, victimZ),
				KillerTeam = killerTeam,
				Headshot = headshot,
				Weapon = weapon,
			};
			return output;
		}
    }
}
