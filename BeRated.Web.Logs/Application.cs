using System;

namespace BeRated
{
	class Application
	{
		static void Main(string[] arguments)
		{
			if (arguments.Length != 2)
			{
				Console.WriteLine("Usage: <path to CS:GO SRCDS logs directory> <connection string>");
				return;
			}
			string logDirectory = arguments[0];
			string connectionString = arguments[1];
			using (var uploader = new Uploader(logDirectory, connectionString))
			{
				uploader.Run();
			}
		}
	}
}
