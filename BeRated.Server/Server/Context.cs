using System;
using System.Collections.Generic;

namespace BeRated.Server
{
	public class Context
	{
		[ThreadStatic]
		public static Context Current = null;

		public Dictionary<string, string> Cookies { get; set; }
	}
}
