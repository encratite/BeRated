using System;

namespace BeRated
{
	class Application
	{
		static void Main(string[] arguments)
		{
			if (arguments.Length != 1)
			{
				Console.WriteLine("Usage: <path to SRCDS logs folder>");
				return;
			}
            string logPath = arguments[0];
			var analyser = new Analyser();
			analyser.ProcessLogs(logPath);
            analyser.Analyse();
			Console.ReadLine();
		}
	}
}
