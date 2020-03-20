using System;
using System.Collections.Generic;
using System.Linq;
using Automation;
using CalcPostProcessor.Steps;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;

namespace CalcPostProcessor.GeneralHouseholdSteps {
    public class TransportationStatisticsWriter : HouseholdStepBase {
        //private int _maxTime;
        [JetBrains.Annotations.NotNull] private readonly IInputDataLogger _logger;

        public TransportationStatisticsWriter([JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                              [JetBrains.Annotations.NotNull] ICalculationProfiler profiler, [JetBrains.Annotations.NotNull] IInputDataLogger logger)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.TransportationStatistics), profiler, "Transportation Statistics Use") =>
            _logger = logger;

        protected override void PerformActualStep(IStepParameters parameters)
        {
            var hsp = (HouseholdStepParameters)parameters;
            if (hsp.Key.HouseholdKey == Constants.GeneralHouseholdKey) {
                return;
            }

            if (!Repository.CalcParameters.IsInTranportMode(hsp.Key.HouseholdKey)) {
                return;
            }

            var deviceActivations = Repository.LoadTransportationDeviceStates(hsp.Key.HouseholdKey);
            var devices = deviceActivations.Select(x => x.TransportationDeviceGuid).Distinct().ToList();
            List<TransportationDeviceStatisticsEntry> statistics = new List<TransportationDeviceStatisticsEntry>();
            foreach (string device in devices) {
                List<TransportationDeviceStateEntry> activations =
                    deviceActivations.Where(x => x.TransportationDeviceGuid == device).ToList();
                TransportationDeviceStatisticsEntry e = new TransportationDeviceStatisticsEntry(device,
                    activations[0].TransportationDeviceName,
                    hsp.Key.HouseholdKey);
                statistics.Add(e);
                e.ProcessOneState(activations[0]);
                for (int i = 1; i < activations.Count; i++) {
                    var cur = activations[i];
                    var prev = activations[i - 1];
                    double distanceTraveled = cur.CurrentRange - prev.CurrentRange;
                    if (distanceTraveled < 0) {
                        e.TotalDistanceTraveled += distanceTraveled;
                    }
                    else {
                        e.TotalDistanceCharged += distanceTraveled;
                    }

                    e.ProcessOneState(cur);
                }
            }

            var transportationEvents = Repository.LoadTransportationEvents(hsp.Key.HouseholdKey);
            Dictionary<string, TransportationDeviceEventStatistics> transportationDevice =
                new Dictionary<string, TransportationDeviceEventStatistics>();
            Dictionary<string, TransportationRouteStatistics> routeStatistics =
                new Dictionary<string, TransportationRouteStatistics>();
            foreach (TransportationEventEntry entry in transportationEvents) {
                {
                    string deviceKey = entry.TransportationDevice;
                    TransportationDeviceEventStatistics transportationDeviceStatistics;
                    if (transportationDevice.ContainsKey(deviceKey)) {
                        transportationDeviceStatistics = transportationDevice[deviceKey];
                    }
                    else {
                        transportationDeviceStatistics =
                            new TransportationDeviceEventStatistics(hsp.Key.HouseholdKey, deviceKey);
                        transportationDevice.Add(deviceKey, transportationDeviceStatistics);
                    }

                    transportationDeviceStatistics.TotalDistance += entry.TotalDistance;
                    transportationDeviceStatistics.TotalTimeSteps += entry.TotalDuration;
                    transportationDeviceStatistics.NumberOfEvents++;
                    //route
                }

                string routeKey;
                if (String.CompareOrdinal(entry.SrcSite, entry.DstSite) < 0) {
                    routeKey = entry.SrcSite + " - " + entry.DstSite + " - " + entry.TransportationDevice;
                }
                else {
                    routeKey = entry.DstSite + " - " + entry.SrcSite + " - " + entry.TransportationDevice;
                }

                TransportationRouteStatistics singleRouteStatistics;
                if (routeStatistics.ContainsKey(routeKey)) {
                    singleRouteStatistics = routeStatistics[routeKey];
                }
                else {
                    singleRouteStatistics =
                        new TransportationRouteStatistics(hsp.Key.HouseholdKey, entry.TransportationDevice, routeKey);
                    routeStatistics.Add(routeKey, singleRouteStatistics);
                }

                singleRouteStatistics.TotalDistance += entry.TotalDistance;
                singleRouteStatistics.TotalTimeSteps += entry.TotalDuration;
                singleRouteStatistics.NumberOfEvents++;
            }

            _logger.Save(hsp.Key.HouseholdKey, statistics);
            _logger.Save(hsp.Key.HouseholdKey, transportationDevice.Values.ToList());
            _logger.Save(hsp.Key.HouseholdKey, routeStatistics.Values.ToList());
        }
    }
}