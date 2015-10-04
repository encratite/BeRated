using System;
using Ashod.Database;

namespace BeRated.App
{
	class TimeConstraints
	{
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }

		public CommandParameter StartParameter
		{
			get
			{
				return new CommandParameter("time_start", Start);
			}
		}

		public CommandParameter EndParameter
		{
			get
			{
				return new CommandParameter("time_end", End);
			}
		}

		public TimeConstraints(int? days)
		{
			Start = null;
			End = null;
			if (days != null)
			{
				var then = DateTime.Now.AddDays((double)-days);
				Start = new DateTime(then.Year, then.Month, then.Day);
				End = null;
			}
		}
	}
}
