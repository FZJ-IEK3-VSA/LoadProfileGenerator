using System.Collections;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.SQLResultLogging.InputLoggers;

namespace CalculationEngine.Transportation {
    public class CalcChargingStation {
        [JetBrains.Annotations.NotNull] private readonly HouseholdKey _householdKey;
        private readonly CalcRepo _calcRepo;

        public CalcChargingStation([JetBrains.Annotations.NotNull] CalcTransportationDeviceCategory deviceCategory, [JetBrains.Annotations.NotNull] CalcLoadType gridChargingLoadType,
                                   double maxChargingPower,
                                   [JetBrains.Annotations.NotNull] string chargingStationName, StrGuid chargingStationGuid,
                                   [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                   // ReSharper disable once UnusedParameter.Local
                                   //todo: put in isbusy
                                   [JetBrains.Annotations.NotNull] CalcLoadType carChargingLoadType, CalcRepo calcRepo, BitArray isBusyArray)
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
        [JetBrains.Annotations.NotNull]
        public CalcTransportationDeviceCategory DeviceCategory { get; }
        [JetBrains.Annotations.NotNull]
        public CalcLoadType GridChargingLoadType { get; }
        [JetBrains.Annotations.NotNull]
        public CalcLoadType CarChargingLoadType { get; }

        public double MaxChargingPower { get; }
        [JetBrains.Annotations.NotNull]
        public string ChargingStationName { get; }
        public StrGuid ChargingStationGuid { get; }
        public bool IsAvailable { get; private set; }
        private CalcTransportationDevice? _connectedCar;
        public void SetConnectedCar([JetBrains.Annotations.NotNull] CalcTransportationDevice device)
        {
            _connectedCar = device;
            IsAvailable = false;
        }

        public void DisconnectCar()
        {
            _connectedCar = null;
            IsAvailable = true;
        }
        public void ProcessRequests([JetBrains.Annotations.NotNull] TimeStep timestep)
        {
            //TODO: do the entire thing with requested power / real power
            ChargingStationState state  = new ChargingStationState(ChargingStationName,ChargingStationGuid,
                IsAvailable,timestep,_householdKey,_connectedCar?.Name,_connectedCar?.Guid,
                _connectedCar?.LastChargingPower??0);
            _calcRepo.OnlineLoggingData.AddChargingStationState(state);
        }
    }
}