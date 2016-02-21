using System;

namespace BeRated.App
{
	class TimeConstraints
	{
		public DateTimeOffset? Start { get; private set; }
		public DateTimeOffset? End { get; private set; }

		public TimeConstraints()
		{
		}

		public TimeConstraints(int? days)
		{
            Start = null;
			if (days != null)
			{
				var then = DateTimeOffset.Now.AddDays((double)-days);
				Start = new DateTimeOffset(then.Year, then.Month, then.Day, 0, 0, 0, then.Offset);
			}
            End = null;
		}

        public bool Match(DateTimeOffset time)
        {
            return
                (Start == null || Start.Value <= time) &&
                (End == null || time <= End.Value);
        }

        public bool Match(DateTime time)
        {
            var dateTimeOffset = new DateTimeOffset(time, DateTimeOffset.Now.Offset);
            return Match(dateTimeOffset);
        }
	}
}
