using System;

namespace BeRated.App
{
	public class TimeConstraints
	{
		public DateTimeOffset? Start { get; private set; }
		public DateTimeOffset? End { get; private set; }

		public TimeConstraints()
		{
		}

		public TimeConstraints(DateTimeOffset? start, DateTimeOffset? end = null)
		{
            Start = GetMidnightOffset(start);
            End = GetMidnightOffset(end);
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

        private DateTimeOffset? GetMidnightOffset(DateTimeOffset? maybeOffset)
        {
            if (maybeOffset.HasValue)
            {
                var offset = maybeOffset.Value;
                maybeOffset = new DateTimeOffset(offset.Year, offset.Month, offset.Day, 0, 0, 0, offset.Offset);
            }
            return maybeOffset;
        }
	}
}
