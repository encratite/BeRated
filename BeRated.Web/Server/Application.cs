using System;
using System.Threading;

namespace BeRated
{
	static class Application
	{
		[STAThread]
		static void Main(string[] arguments)
		{
			if (arguments.Length != 2)
				return;

			int port = int.Parse(arguments[0]);
			string connectionString = arguments[1];

			using (var server = new Server(port, connectionString))
			{
				server.Run();
				System.Windows.Forms.Application.Run();
			}
		}
	}
}
