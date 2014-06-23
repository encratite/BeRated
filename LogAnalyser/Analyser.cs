using BeRated.Common;
using System.Collections.Generic;
using System.IO;

namespace LogAnalyser
{
	class Analyser
	{
		Dictionary<string, PlayerInformation> PlayerData;

		public void ProcessLogs(string path)
		{
			PlayerData = new Dictionary<string, PlayerInformation>();
			var files = Directory.GetFiles(path);
			foreach (var file in files)
				ProcessLog(file);
		}

		void ProcessLog(string path)
		{
			var lines = File.ReadLines(path);
			foreach (var line in lines)
				ProcessLine(line);
		}

		void ProcessLine(string line)
		{
			var kill = LogParser.ReadPlayerKill(line);
			if (kill == null)
				return;
			if (kill.Killer.SteamId == LogParser.BotId || kill.Victim.SteamId == LogParser.BotId)
				return;
			var key = kill.Killer.SteamId;
			PlayerInformation information;
			if (!PlayerData.TryGetValue(key, out information))
			{
				information = new PlayerInformation(kill.Killer);
				PlayerData.Add(key, information);
			}
			information.Kills.Add(kill);
		}
	}
}
