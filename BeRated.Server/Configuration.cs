using System;

namespace BeRated
{
    public class Configuration
    {
        public string ServerUrl { get; set; }

        public string ConnectionString { get; set; }

        public string ViewPath { get; set; }

        public void Validate()
		{
			if (ServerUrl == null || ConnectionString == null || ViewPath == null)
				throw new ApplicationException("Invalid configuration");
		}
    }
}
