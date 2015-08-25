using System;

namespace BeRated
{
	class TimeConstraints
	{
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }

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
