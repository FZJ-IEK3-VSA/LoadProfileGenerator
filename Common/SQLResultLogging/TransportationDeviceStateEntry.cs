using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.SQLResultLogging
{
    public enum TransportationDeviceState {
        Undefined,
        Driving,
        ParkingAndCharging,
        ParkingAndFullyCharged,
        ParkingAndNoChargingAvailableHere,
        ParkingAndWaitingForCharging
    }
    public class TransportationDeviceStateEntry:IHouseholdKey
    {
        [NotNull]
        public HouseholdKey HouseholdKey { get; set; }
        public TransportationDeviceStateEntry([NotNull]string transportationDeviceName,
                                              [NotNull] StrGuid transportationDeviceGuid, [NotNull] TimeStep timeStep,
                                              TransportationDeviceState transportationDeviceStateEnum,
                                              double currentSOC, [NotNull] HouseholdKey householdKey,
                                              double currentRange, [CanBeNull] string currentSite,
                                              [CanBeNull] string currentUser,
            [NotNull] string dateTime)
        {
            TransportationDeviceName = transportationDeviceName;
            TransportationDeviceGuid = transportationDeviceGuid;
            TimeStep = timeStep;
            TransportationDeviceStateEnum = transportationDeviceStateEnum;
            TransportationDeviceState = transportationDeviceStateEnum.ToString();
            CurrentSOC = currentSOC;
            HouseholdKey = householdKey;
            CurrentRange = currentRange;
            CurrentSite = currentSite;
            CurrentUser = currentUser;
            DateTime = dateTime;
        }

        [CanBeNull]
        public string CurrentSite { get; set; }
        [CanBeNull]
        public string CurrentUser { get; set; }

        [NotNull]
        public string DateTime { get; }

        [NotNull]
        public string TransportationDeviceName { get; set; }
        [NotNull]
        public StrGuid TransportationDeviceGuid { get; set; }
        [NotNull]
        public TimeStep TimeStep { get; set; }
        [NotNull]
        public string TransportationDeviceState { get; set; }
        public TransportationDeviceState TransportationDeviceStateEnum { get; set; }
        public double CurrentSOC { get; set; }
        public double CurrentRange { get; set; }
    }
}
