using Ashod;

namespace BeRated
{
	class Program
	{
        static void Main(string[] arguments)
        {
            var configuration = JsonFile.Read<Configuration>();
            using (var uploader = new Uploader(configuration.LogDirectory, configuration.ConnectionString))
			{
				uploader.Run();
			}
		}
	}
}
