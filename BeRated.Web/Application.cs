using System;
using System.Threading;

namespace BeRated
{
	static class Application
	{
		[STAThread]
		static void Main()
		{
			using (var server = new Server(65080, ""))
			{
				server.Run();
				System.Windows.Forms.Application.Run();
			}
		}
	}
}
