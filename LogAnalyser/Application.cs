using System;

namespace LogAnalyser
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
			var analyser = new Analyser();
			analyser.ProcessLogs(arguments[0]);
			analyser.Analyse();
			Console.ReadLine();
		}
	}
}
