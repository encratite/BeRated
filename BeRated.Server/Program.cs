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
            var ratingServer = new ServerInstance();
            using (var server = new WebServer(ratingServer, configuration.ServerUrl))
            {
                server.Run();
                Console.WriteLine("Running on {0}", configuration.ServerUrl);
                var resetEvent = new ManualResetEvent(false);
                resetEvent.WaitOne();
            }
        }
    }
}
