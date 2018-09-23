using System;
using System.Collections.Generic;
using System.Linq;

namespace BaldurToolkit.Cron
{
    public class CronExpression
    {
        protected const int MinutesIndex = 0;
        protected const int HoursIndex = 1;
        protected const int DaysIndex = 2;
        protected const int MonthsIndex = 3;
        protected const int DaysOfWeekIndex = 4;

        private static readonly Dictionary<string, string> Replacements = new Dictionary<string, string>()
        {
            { "Sun", "0" }, { "Mon", "1" }, { "Tue", "2" }, { "Wed", "3" }, { "Thu", "4" }, { "Fri", "5" }, { "Sat", "6" },
            { "Jan", "1" }, { "Feb", "2" }, { "Mar", "3" }, { "Apr", "4" }, { "May", "5" }, { "Jun", "6" }, { "Jul", "7" }, { "Aug", "8" }, { "Sep", "9" }, { "Oct", "10" }, { "Nov", "11" }, { "Dec", "12" },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CronExpression"/> class.
        /// </summary>
        /// <remarks>
        /// Cron expression format:
        /// <code>
        /// * * * * *
        /// - - - - -
        /// | | | | |
        /// | | | | ----- Day of week (0 - 7 or Sun - Sat) (Sunday is 0 or 7)
        /// | | | ------- Month (1 - 12 or Jan - Dec)
        /// | | --------- Day of month (1 - 31)
        /// | ----------- Hour (0 - 23)
        /// ------------- Minute (0 - 59)
        /// </code>
        /// Each field can be in one of two formats: <code>*[/interval]</code> or <code>start[-end[/interval]]</code>.
        /// In one field you can specify multiple comma-separated entries (eg <code>12-15,19,22</code>).
        /// </remarks>
        /// <param name="expression">The expression text.</param>
        public CronExpression(string expression)
        {
            foreach (var item in Replacements)
            {
                expression = expression.Replace(item.Key, item.Value);
            }

            var fields = expression.Split(' ', '\t');

            if (fields.Length != 5)
            {
                throw new Exception("Wrong fields count in cron rule.");
            }

            fields[DaysOfWeekIndex] = fields[DaysOfWeekIndex].Replace('7', '0'); // Days of week: Sun = 7 = 0

            this.Minutes = this.ParseField(fields[MinutesIndex], 0, 59);
            this.Hours = this.ParseField(fields[HoursIndex], 0, 23);
            this.Days = this.ParseField(fields[DaysIndex], 1, 31);
            this.Months = this.ParseField(fields[MonthsIndex], 1, 12);
            this.DaysOfWeek = this.ParseField(fields[DaysOfWeekIndex], 0, 6);
        }

        public IEnumerable<int> Minutes { get; protected set; }

        public IEnumerable<int> Hours { get; protected set; }

        public IEnumerable<int> Days { get; protected set; }

        public IEnumerable<int> Months { get; protected set; }

        public IEnumerable<int> DaysOfWeek { get; protected set; }

        public bool Check(DateTime time)
        {
            return this.Minutes.Contains(time.Minute) &&
                    this.Hours.Contains(time.Hour) &&
                    this.Days.Contains(time.Day) &&
                    this.Months.Contains(time.Month) &&
                    this.DaysOfWeek.Contains((int)time.DayOfWeek);
        }

        protected IEnumerable<int> ParseField(string line, int minValue, int maxValue)
        {
            var list = line.Split(',');

            foreach (var entry in list)
            {
                int start, end, interval;
                var parts = entry.Split('-', '/');

                if (parts[0].Equals("*"))
                {
                    // Format is *[/interval]
                    start = minValue;
                    end = maxValue;
                    interval = (parts.Length > 1) ? int.Parse(parts[1]) : 1;
                }
                else
                {
                    // Format is start[-end[/interval]]
                    start = int.Parse(parts[0]);
                    end = (parts.Length > 1) ? int.Parse(parts[1]) : start;
                    interval = (parts.Length > 2) ? int.Parse(parts[2]) : 1;
                }

                for (int i = start; i <= end; i += interval)
                {
                    yield return i;
                }
            }
        }
    }
}
