using System;

namespace BeRated
{
    public class Configuration
    {
        public string ServerUrl { get; set; }

        public string ConnectionString { get; set; }

		public string LogDirectory { get; set; }

        public string ViewPath { get; set; }

		/// <summary>
		/// Approximate size of HTTP cache, in MiB.
		/// </summary>
		public int? CacheSize { get; set; }

        public void Validate()
		{
			if (ServerUrl == null || ConnectionString == null || LogDirectory == null || ViewPath == null || !CacheSize.HasValue || CacheSize.Value <= 0)
				throw new ApplicationException("Invalid configuration.");
		}
    }
}
