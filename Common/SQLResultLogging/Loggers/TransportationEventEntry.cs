using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.SQLResultLogging.Loggers
{
    public enum CurrentActivity
    {
        InTransport,
        InAffordance
    }

    public class TransportationEventEntry:IHouseholdKey
    {
        public HouseholdKey HouseholdKey {get; }
        public TransportationEventEntry(
            [NotNull] HouseholdKey key,
            [NotNull] string person, [NotNull] TimeStep timestep, [NotNull] string srcSite, [NotNull] string dstSite, CurrentActivity activity,
                                        [NotNull] string description, [NotNull] string transportationDevice,
            int totalDuration, double totalDistance)
        {
            HouseholdKey = key;
            PersonName = person;
            Timestep = timestep;
            SrcSite = srcSite;
            DstSite = dstSite;
            Activity = activity;
            Description = description;
            TransportationDevice = transportationDevice;
            TotalDuration = totalDuration;
            TotalDistance = totalDistance;
        }

        [NotNull]
        public string TransportationDevice { get; }

        public int TotalDuration { get; }
        public double TotalDistance { get; }

        public CurrentActivity Activity { get; }
        [NotNull]
        public string Description { get; }
        [NotNull]
        public string PersonName { get; }
        [NotNull]
        public string SrcSite { get; }
        [NotNull]
        public string DstSite { get; }
        [NotNull]
        public TimeStep Timestep { get; }
    }

    public class TransportationStatus:IHouseholdKey {
        public TransportationStatus([NotNull] TimeStep timestep,[NotNull] HouseholdKey householdKey, [NotNull] string statusMessage)
        {
            Timestep = timestep;
            HouseholdKey = householdKey;
            StatusMessage = statusMessage;
        }

        [NotNull]
        public TimeStep Timestep { get; }

        [UsedImplicitly]
        public HouseholdKey HouseholdKey { get; set; }
        [NotNull]
        [UsedImplicitly]
        public string StatusMessage { get; set; }
    }
    /*
    public class TransportationEntry
    {
        public TransportationEntry([NotNull] string person) => Person = person;

        [UsedImplicitly]
        public CurrentActivity CurrentActivity { get; set; }
        [CanBeNull]
        public string CurrentSite { get; set; }
        [CanBeNull]
        public string Description { get; set; }
        public int IsInAffordance { get; set; }
        public int IsInTransport { get; set; }
        [NotNull]
        public string Person { get; }
        [CanBeNull]
        public string TransportationDevice { get; set; }
    }*/
}
