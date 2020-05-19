using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class ChargingStationState:IHouseholdKey
    {
        public ChargingStationState([NotNull] string chargingStationName, StrGuid chargingStationGuid, bool isAvailable,
                                    [NotNull] TimeStep timeStep, [NotNull] HouseholdKey householdKey,
                                    [CanBeNull] string connectedCarName, [CanBeNull] StrGuid? connectedCarGuid,
                                    double chargingPower)
        {
            ChargingStationName = chargingStationName;
            ChargingStationGuid = chargingStationGuid;
            IsAvailable = isAvailable;
            TimeStep = timeStep;
            HouseholdKey = householdKey;
            ConnectedCarName = connectedCarName;
            ConnectedCarGuid = connectedCarGuid;
            ChargingPower = chargingPower;
        }
        [JsonProperty]
        [NotNull]
        public TimeStep TimeStep { get; private set; }
        [NotNull]
        [JsonProperty]
        public string ChargingStationName { get; private set; }
        [JsonProperty]
        public StrGuid ChargingStationGuid { get; private set; }
        [JsonProperty]
        public bool IsAvailable { get; private set; }
        [JsonProperty]
        public HouseholdKey HouseholdKey { get; private  set; }
        [CanBeNull]
        [JsonProperty]
        public string ConnectedCarName { get; private set; }
        [CanBeNull]
        [JsonProperty]
        public StrGuid? ConnectedCarGuid { get; private set; }
        [JsonProperty]
        public double ChargingPower { get; private set; }
    }
}
