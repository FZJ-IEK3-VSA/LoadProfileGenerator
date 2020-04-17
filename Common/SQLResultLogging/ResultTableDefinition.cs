using System;
using JetBrains.Annotations;

namespace Common.SQLResultLogging
{
    public enum ResultTableID {
        CalcParameters,
        AffordanceEnergyUse,
        DaylightTimes,
        Temperatures,
        BridgeDayEntries,
        AffordanceDefinitions,
        AffordanceTaggingSets,
        AutonomousDeviceDefinitions,
        LoadTypeDefinitions,
        CalcObjectInformation,
        PersonDefinitions,
        SiteDefinitions,
        TransportationDeviceDefinitions,
        TravelRouteDefinitions,
        VariableDefinitions,
        ChargingeStationState,
        VariableValues,
        BinaryTempFileColumnDescriptions,
        DeviceTaggingSetInformation,
        HouseDefinition,
        HouseholdDefinitions,
        HouseholdKeys,
        ResultFileEntries,
        TransportationDeviceStates,
        PerformedActions,
        DeviceActivationEntries,
        LocationEntries,
        LogMessages,
        PersonStatus,
        TotalsPerLoadtype,
        TransportationEvents,
        TransportationStatuses,
        DeviceDefinitions,
        CalcStartParameters,
        TransportationDeviceStatistics,
        TransportationRouteStatistics,
        TransportationDeviceEventStatistics,
        PersonAffordanceInformation,
        SingleTimeStepActionEntry,
        TotalsPerDevice
    }
    public class ResultTableDefinition: IEquatable<ResultTableDefinition>
    {
        public ResultTableDefinition([NotNull] string tableName, ResultTableID resultTableID, [NotNull] string description)
        {
            TableName = tableName;
            ResultTableID = resultTableID;
            Description = description;
        }

        public static bool operator ==([CanBeNull] ResultTableDefinition obj1, [CanBeNull] ResultTableDefinition obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if (obj1 is null)
            {
                return false;
            }
            if (obj2 is null)
            {
                return false;
            }

            return string.Equals(obj1.TableName, obj2.TableName) && obj1.ResultTableID == obj2.ResultTableID && string.Equals(obj1.Description, obj2.Description);
        }

        // this is second one '!='
        public static bool operator !=([CanBeNull] ResultTableDefinition obj1, [CanBeNull] ResultTableDefinition obj2)
        {
            return !(obj1 == obj2);
        }

        public bool Equals(ResultTableDefinition other)
        {
            if (other is null) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(TableName, other.TableName) && ResultTableID == other.ResultTableID && string.Equals(Description, other.Description);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ResultTableDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = (TableName.GetHashCode());
                hashCode = (hashCode * 397) ^ (int)ResultTableID;
                return (hashCode * 397) ^ ( Description.GetHashCode());
            }
        }

        [NotNull]
        public string TableName { get; }
        public ResultTableID ResultTableID { get; }
        [NotNull]
        public string Description { get; }
    }
}
