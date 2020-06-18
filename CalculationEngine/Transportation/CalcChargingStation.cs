using System.Collections;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation {
    public class CalcChargingStation {
        [NotNull] private readonly HouseholdKey _householdKey;
        private readonly CalcRepo _calcRepo;

        public CalcChargingStation([NotNull] CalcTransportationDeviceCategory deviceCategory, [NotNull] CalcLoadType gridChargingLoadType,
                                   double maxChargingPower,
                                   [NotNull] string chargingStationName, StrGuid chargingStationGuid,
                                   [NotNull] HouseholdKey householdKey,
                                   [NotNull] CalcLoadType carChargingLoadType, CalcRepo calcRepo, BitArray timeLimit)
        {
            _householdKey = householdKey;
            _calcRepo = calcRepo;
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
        public StrGuid ChargingStationGuid { get; }
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
            _calcRepo.OnlineLoggingData.AddChargingStationState(state);
        }
    }
}