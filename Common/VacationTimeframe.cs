using System;

namespace Common
{
    public class VacationTimeframe
    {
        public VacationTimeframe(DateTime start, DateTime end, bool mapToOtherYears = true)
        {
            StartDate = start;
            EndDate = end;
            MapToOtherYears = mapToOtherYears;
        }

        public DateTime StartDate { get; }

        public DateTime EndDate { get; }

        public bool MapToOtherYears { get; }
    }
}