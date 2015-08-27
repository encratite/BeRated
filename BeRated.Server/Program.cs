using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Ashod;
using BeRated.App;
using BeRated.Server;

namespace BeRated
{
    class Program
    {
        private static void CreateAppDomain()
        {
            Logger.Log("Creating new app domain");
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
			configuration.Validate();
            var instance = new RatingApp(configuration);
            instance.Initialize();
            using (var launcher = new WebAppLauncher(instance, configuration.ServerUrl))
            {
                launcher.Start();
                Logger.Log("Running on {0}", configuration.ServerUrl);
                Logger.Log("Hit enter to shut down the server");
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
