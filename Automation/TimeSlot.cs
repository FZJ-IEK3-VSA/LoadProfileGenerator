namespace Automation
{
    /// <summary>
    /// Defines for which day types a time slot applies
    /// </summary>
    public enum DayType
    {
        Weekday = 0, Weekend = 1, EveryDay = 2
    }

    /// <summary>
    /// Defines repeating time spans within a week, specified by the type of day and the start
    /// and end time.
    /// </summary>
    /// <param name="Start">Start of the time slot as seconds since midnight</param>
    /// <param name="End">End of the time slot (exclusive) as seconds since midnight</param>
    /// <param name="DayType">The type of days this time slot applies to</param>
    public record TimeSlot(int Start, int End, DayType DayType);
}
