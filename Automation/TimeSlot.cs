using System;
using System.Collections.Generic;
using System.Linq;

namespace Automation
{
    /// <summary>
    /// Defines repeating time spans within a week, specified by the type of day and the start
    /// and end time.
    /// </summary>
    /// <param name="Start">Start of the time slot as seconds since midnight</param>
    /// <param name="End">End of the time slot (exclusive) as seconds since midnight</param>
    /// <param name="WeekDays">The weekdays this time slot applies to</param>
    public record TimeSlot(int Start, int End, HashSet<DayOfWeek> WeekDays)
    {
        /// <summary>
        /// Return an unambiguous string identifying this time slot.
        /// </summary>
        /// <returns>string representation of this time slot</returns>
        public override string ToString()
        {
            // order according to the enum definition to make the string unambiguous
            var orderedWeekdays = WeekDays.Order();
            string weekdayString = string.Join(", ", orderedWeekdays);
            return $"time slot {Start} to {End} on {weekdayString}";
        }
    }
}
