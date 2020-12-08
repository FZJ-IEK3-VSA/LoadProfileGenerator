using System;
using System.Collections.Generic;
using Automation.ResultFiles;

namespace Common.JSON
{
    using SQLResultLogging.Loggers;

    public class BridgeDayEntries :IHouseholdKey
    {
        public BridgeDayEntries([JetBrains.Annotations.NotNull] HouseholdKey householdKey, [JetBrains.Annotations.NotNull] List<DateTime> dateTimes)
        {
            HouseholdKey = householdKey;
            Entries.AddRange(dateTimes);
        }

        [JetBrains.Annotations.NotNull]
        public List<DateTime> Entries { get; } = new List<DateTime>();
        public HouseholdKey HouseholdKey { get; }
    }
}
