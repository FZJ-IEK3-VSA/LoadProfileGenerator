using System;
using Autofac;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using CalculationController.Queue;
using ChartCreator2;
using Common;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class CalcParametersFactory
    {
        [NotNull]
        public CalcParameters MakeCalculationParametersFromConfig([NotNull] CalcStartParameterSet csps, bool forceRandom)
        {
            CalcParameters cp = CalcParameters.GetNew();
            cp.LoadtypesToPostprocess = csps.LoadTypesToProcess;
            cp.SetDeleteDatFiles(csps.DeleteDatFiles);
            cp.SetWriteExcelColumn (csps.WriteExcelColumn);
            cp.SetManyOptionsWithClear(csps.CalcOptions);
            FileFactoryAndTrackerDummy fftd = new FileFactoryAndTrackerDummy();
            ChartProcessorManager.ChartingFunctionDependencySetter(csps.ResultPath,csps.CalculationProfiler,fftd,cp.Options);

            var container =PostProcessingManager.RegisterEverything(csps.ResultPath,csps.CalculationProfiler, fftd);
            using (var scope = container.BeginLifetimeScope())
            {
                var odm = scope.Resolve<OptionDependencyManager>();
                odm.EnableRequiredOptions(cp.Options);
            }
            cp.SetShowSettlingPeriod(csps.ShowSettlingPeriod);
            cp.SetRandomSeed(csps.SelectedRandomSeed, forceRandom);
            cp.SetCsvCharacter(csps.CsvCharacter);
            cp.SetStartDate(csps.OfficialSimulationStartTime);
            cp.SetEndDate(csps.OfficialSimulationEndTime.AddDays(1));
            cp.SetInternalTimeResolution(csps.InternalTimeResolution);
            cp.SetExternalTimeResolution(csps.ExternalTimeResolution);
            cp.SetLoadTypePriority(csps.LoadTypePriority);
            cp.SetSettlingDays(csps.SettlingDays);
            cp.SetAffordanceRepetitionCount(csps.AffordanceRepetitionCount);
            cp.EnableIdlemode = csps.EnableIdlemode;
            cp.DeviceProfileHeaderMode = csps.DeviceProfileHeaderMode;
            cp.IgnorePreviousActivitesWhenNeeded = csps.IgnorePreviousActivitiesWhenNeeded;
            cp.TransportationEnabled = csps.TransportationEnabled;
            //if (cp.TransportationEnabled && csps.CalcTarget.CalcObjectType == CalcObjectType.House) {
            //    if (csps.ChargingStationSet != null) {
            //        throw new LPGException("trying to set transportation options on a house. that won't work.");
            //    }
            //    if (csps.TransportationDeviceSet != null)
            //    {
            //        throw new LPGException("trying to set transportation options on a house. that won't work.");
            //    }
            //    if (csps.TravelRouteSet != null)
            //    {
            //        throw new LPGException("trying to set transportation options on a house. that won't work.");
            //    }
            //}
            if (cp.TransportationEnabled && csps.CalcTarget.CalcObjectType == CalcObjectType.ModularHousehold)
            {
                if (csps.ChargingStationSet == null)
                {
                    throw new LPGException("No charging station set, but transportation enabled. That won't work.");
                }
                if (csps.TransportationDeviceSet == null)
                {
                    throw new LPGException("No transportation device set, but transportation enabled. That won't work.");
                }
                if (csps.TravelRouteSet == null)
                {
                    throw new LPGException("No travel route set, but transportation enabled. That won't work.");
                }
            }
            cp.CheckSettings();
            //transport mode
            /*if (csps.TransportationDeviceSet != null || csps.TravelRouteSet != null) {
                cp.SetTransportMode(true);
                if (csps.TransportationDeviceSet == null ) {
                    throw new DataIntegrityException("Only a transportation device set was defined, but not a travel route set. This can't work.");
                }
                if (csps.TravelRouteSet == null)
                {
                    throw new DataIntegrityException("Only a travel route set was defined, but not a transportation device set. This can't work.");
                }
            }*/
            return cp;
        }

        [NotNull]
        public static CalcParameters MakeGoodDefaults()
        {
            CalcParameters cp = CalcParameters.GetNew();
            cp.SetStartDate(2018, 1, 1).SetEndDate(2018, 12, 31);
            cp.SetInternalTimeResolution(new TimeSpan(0, 1, 0)).SetExternalTimeResolution(new TimeSpan(0, 1, 0));
            cp.SetCsvCharacter(";");
            cp.SetLoadTypePriority(LoadTypePriority.RecommendedForHouses);
            cp.DisableShowSettlingPeriod();
            cp.SetSettlingDays(3);
            cp.SetRandomSeed(17,false);
            cp.SetWriteExcelColumn(false);
            cp.SetAffordanceRepetitionCount(3);
            cp.CheckSettings();
            return cp;
        }
    }
}
