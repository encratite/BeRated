using System;
using System.Threading;
using Ashod;
using BeRated.Server;

namespace BeRated
{
    class Program
    {
        static void Main(string[] arguments)
        {
            var configuration = JsonFile.Read<Configuration>();
            var ratingServer = new ServerInstance(configuration.ConnectionString);
            using (var server = new WebServer(ratingServer, configuration.ServerUrl))
            {
                server.Start();
                Console.WriteLine("Running on {0}", configuration.ServerUrl);
                var resetEvent = new ManualResetEvent(false);
                resetEvent.WaitOne();
            }
        }
    }
}
