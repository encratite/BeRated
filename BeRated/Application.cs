using Ashod;

namespace BeRated
{
	class Application
	{
		static void Main(string[] arguments)
		{
			var configuration = XmlFile.Read<Configuration>();
			var service = new Service(configuration);
			service.Run();
		}
	}
}
