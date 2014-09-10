using Ashod;
using System;

namespace BeRated
{
	static class Application
	{
		[STAThread]
		static void Main(string[] arguments)
		{
            var configuration = JsonFile.Read<Configuration>();
			using (var server = new Server(configuration.Port, configuration.ConnectionString))
			{
				server.Run();
				System.Windows.Forms.Application.Run();
			}
		}
	}
}
