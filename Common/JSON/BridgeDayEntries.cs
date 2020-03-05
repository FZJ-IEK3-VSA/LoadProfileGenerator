using System;
using System.Collections.Generic;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.JSON
{
    using SQLResultLogging.Loggers;

    public class BridgeDayEntries :IHouseholdKey
    {
        public BridgeDayEntries([NotNull] HouseholdKey householdKey, [NotNull] List<DateTime> dateTimes)
        {
            HouseholdKey = householdKey;
            Entries.AddRange(dateTimes);
        }

        [NotNull]
        public List<DateTime> Entries { get; } = new List<DateTime>();
        public HouseholdKey HouseholdKey { get; }
    }
}
