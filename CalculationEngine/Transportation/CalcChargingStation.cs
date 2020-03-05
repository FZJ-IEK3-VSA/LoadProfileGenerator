using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineLogging;
using Common;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation {
    public class CalcChargingStation {
        [NotNull] private readonly IOnlineLoggingData _onlineLoggingData;
        [NotNull] private readonly HouseholdKey _householdKey;

        public CalcChargingStation([NotNull] CalcTransportationDeviceCategory deviceCategory, [NotNull] CalcLoadType gridChargingLoadType, double maxChargingPower,
                                   [NotNull] IOnlineLoggingData onlineLoggingData,
                                   [NotNull] string chargingStationName, [NotNull] string chargingStationGuid,
                                   [NotNull] HouseholdKey householdKey,
                                   [NotNull] CalcLoadType carChargingLoadType)
        {
            _onlineLoggingData = onlineLoggingData;
            _householdKey = householdKey;
            CarChargingLoadType = carChargingLoadType;
            DeviceCategory = deviceCategory;
            GridChargingLoadType = gridChargingLoadType;
            MaxChargingPower = maxChargingPower;
            ChargingStationName = chargingStationName;
            ChargingStationGuid = chargingStationGuid;
            IsAvailable = true;
        }
        //TODO: time limit for availability
        //TODO: do the entire "requested power, real active power" thing
        //TODO: multiple charging points at one charging station
        //TODO: power limits across multiple charging stations
        [NotNull]
        public CalcTransportationDeviceCategory DeviceCategory { get; }
        [NotNull]
        public CalcLoadType GridChargingLoadType { get; }
        [NotNull]
        public CalcLoadType CarChargingLoadType { get; }

        public double MaxChargingPower { get; }
        [NotNull]
        public string ChargingStationName { get; }
        [NotNull]
        public string ChargingStationGuid { get; }
        public bool IsAvailable { get; private set; }
        [CanBeNull] private CalcTransportationDevice _connectedCar;
        public void SetConnectedCar([NotNull] CalcTransportationDevice device)
        {
            _connectedCar = device;
            IsAvailable = false;
        }

        public void DisconnectCar()
        {
            _connectedCar = null;
            IsAvailable = true;
        }
        public void ProcessRequests([NotNull] TimeStep timestep)
        {
            //TODO: do the entire thing with requested power / real power
            ChargingStationState state  = new ChargingStationState(ChargingStationName,ChargingStationGuid,
                IsAvailable,timestep,_householdKey,_connectedCar?.Name,_connectedCar?.Guid,
                _connectedCar?.LastChargingPower??0);
            _onlineLoggingData.AddChargingStationState(state);
        }
    }
}