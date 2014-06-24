using System;

namespace BeRated
{
	class Application
	{
		static void Main(string[] arguments)
		{
			if (arguments.Length != 3)
			{
				Console.WriteLine("Usage: <path to CS:GO SRCDS logs directory> <SQL server data source> <database>");
				return;
			}
			string logDirectory = arguments[0];
			string dataSource = arguments[1];
			string database = arguments[2];
			var uploader = new Uploader(logDirectory, dataSource, database);
			uploader.Run();
		}
	}
}
