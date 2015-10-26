using System;

namespace BeRated.Server
{
	class CacheEntry
	{
		public DateTime Time { get; private set; }

		public string Markup { get; private set; }

		public CacheEntry(string markup)
		{
			Time = DateTime.Now;
			Markup = markup;
		}
	}
}
