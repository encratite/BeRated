using Ashod;
using System.Threading;

namespace BeRated
{
	static class Application
	{
		static void Main(string[] arguments)
		{
            var configuration = JsonFile.Read<Configuration>();
            using (var server = new Server(configuration.Port, configuration.ConnectionString))
			{
				server.Run();
				var resetEvent = new ManualResetEvent(false);
				resetEvent.WaitOne();
			}
		}
	}
}
