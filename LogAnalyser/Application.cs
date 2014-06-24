using System;

namespace LogAnalyser
{
	class Application
	{
		static void Main(string[] arguments)
		{
			if (arguments.Length != 2)
			{
				Console.WriteLine("Usage: <path to SRCDS logs folder> <player data .csv output path>");
				return;
			}
            string logPath = arguments[0];
            string csvPath = arguments[1];
			var analyser = new Analyser();
			analyser.ProcessLogs(logPath);
            analyser.Analyse(csvPath);
			Console.ReadLine();
		}
	}
}
