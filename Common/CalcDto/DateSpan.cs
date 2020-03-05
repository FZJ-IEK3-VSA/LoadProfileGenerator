using System;

namespace Common.CalcDto {
    public class DateSpan {
        public DateSpan(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; }
        public DateTime End { get; }
    }
}