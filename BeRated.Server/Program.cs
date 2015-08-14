using System;
using System.Threading;
using Ashod;

namespace BeRated.Server
{
    class Program
    {
        static void Main(string[] arguments)
        {
            var configuration = JsonFile.Read<Configuration>();
            using (var server = new WebServer(null, configuration.ServerUrl))
            {
                server.Run();
                Console.WriteLine("Running on {0}", configuration.ServerUrl);
                var resetEvent = new ManualResetEvent(false);
                resetEvent.WaitOne();
            }
        }
    }
}
