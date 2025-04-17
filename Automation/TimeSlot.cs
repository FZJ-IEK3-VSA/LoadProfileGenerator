using System;
using System.Collections.Generic;

namespace Automation
{
    /// <summary>
    /// Defines repeating time spans within a week, specified by the type of day and the start
    /// and end time.
    /// </summary>
    /// <param name="Start">Start of the time slot as seconds since midnight</param>
    /// <param name="End">End of the time slot (exclusive) as seconds since midnight</param>
    /// <param name="WeekDays">The weekdays this time slot applies to</param>
    public record TimeSlot(int Start, int End, HashSet<DayOfWeek> WeekDays);
}
