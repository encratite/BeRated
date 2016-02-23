using System;
using System.Collections.Generic;
using System.Globalization;

namespace BeRated.App
{
    public class TimeConstraintPreset
    {
        public string InternalName { get; private set; }

        public string Description { get; private set; }

        public Func<TimeConstraints> GetConstraints { get; private set; }

        private TimeConstraintPreset(string internalName, string description, Func<TimeConstraints> getConstraints)
        {
            InternalName = internalName;
            Description = description;
            GetConstraints = getConstraints;
        }

        public static IEnumerable<TimeConstraintPreset> Presets = new List<TimeConstraintPreset>
        {
            new TimeConstraintPreset("all", "Show all stats", () => new TimeConstraints()),
            new TimeConstraintPreset("today", "Today", () => GetDayPreset()),
            new TimeConstraintPreset("yesterday", "Yesterday", () => GetDayPreset(1)),
            new TimeConstraintPreset("currentWeek", "Current week", () => GetWeekPreset()),
            new TimeConstraintPreset("previousWeek", "Previous week", () => GetWeekPreset(1)),
            new TimeConstraintPreset("currentMonth", "Current month", () => GetMonthPreset()),
            new TimeConstraintPreset("previousMonth", "Previous month", () => GetMonthPreset(1)),
        }.AsReadOnly();

        private static TimeConstraints GetDayPreset(int days = 0)
        {
            var start = DateTimeOffset.Now - TimeSpan.FromDays(days);
            var end = start + TimeSpan.FromDays(1);
            var constraints = new TimeConstraints(start, end);
            return constraints;
        }

        private static TimeConstraints GetWeekPreset(int weeks = 0)
        {
            var weekTimeSpan = TimeSpan.FromDays(7 * weeks);
            var start = DateTimeOffset.Now - weekTimeSpan;
            while (start.DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                start = start.AddDays(-1);
            var end = start + weekTimeSpan;
            var constraints = new TimeConstraints(start, end);
            return constraints;
        }

        private static TimeConstraints GetMonthPreset(int months = 0)
        {
            var start = DateTimeOffset.Now;
            start = start.AddDays(- start.Day + 1);
            start = start.AddMonths(-months);
            var end = start.AddMonths(1);
            var constraints = new TimeConstraints(start, end);
            return constraints;
        }
    }
}
