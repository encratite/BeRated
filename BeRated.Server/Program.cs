using Ashod;
using BeRated.Server;
using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace BeRated
{
    class Program
    {
        private static void CreateAppDomain()
        {
            Console.WriteLine("Creating new app domain");
            var setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var current = AppDomain.CurrentDomain;
            var permissionSet = new PermissionSet(PermissionState.Unrestricted);
            Evidence securityInfo = null;
            var domain = AppDomain.CreateDomain("BeRatedDomain", securityInfo, setup, permissionSet);
            var assembly = Assembly.GetExecutingAssembly();
            var exitCode = domain.ExecuteAssembly(assembly.Location);
            AppDomain.Unload(domain);
        }

        private static void RunServer()
        {
            var configuration = JsonFile.Read<Configuration>();
            var instance = new ServerInstance(configuration);
            instance.Initialize();
            using (var server = new WebServer(instance, configuration.ServerUrl))
            {
                server.Start();
                Console.WriteLine("Running on {0}", configuration.ServerUrl);
                Console.WriteLine("Hit enter to shut down the server");
                Console.ReadLine();
            }
        }

        private static void Main(string[] arguments)
        {
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                CreateAppDomain();
            else
                RunServer();
        }
    }
}
