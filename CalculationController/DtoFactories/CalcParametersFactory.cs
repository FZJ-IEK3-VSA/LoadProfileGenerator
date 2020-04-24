using System;
using Automation;
using CalculationController.Queue;
using Common.JSON;
using Database.Tables.Houses;
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
            cp.SetShowSettlingPeriod(csps.ShowSettlingPeriod);
            cp.SetRandomSeed(csps.SelectedRandomSeed, forceRandom);
            if (csps.CalcTarget.GetType() == typeof(Settlement))
            {
                cp.Enable(CalcOption.OverallDats);
            }

            cp.SetCsvCharacter(csps.CsvCharacter);
            cp.SetStartDate(csps.OfficialSimulationStartTime);
            cp.SetEndDate(csps.OfficialSimulationEndTime.AddDays(1));
            cp.SetInternalTimeResolution(csps.InternalTimeResolution);
            cp.SetExternalTimeResolution(csps.ExternalTimeResolution);
            cp.SetLoadTypePriority(csps.LoadTypePriority);
            cp.SetSettlingDays(csps.SettlingDays);
            cp.SetAffordanceRepetitionCount(csps.AffordanceRepetitionCount);
            cp.DeviceProfileHeaderMode = csps.DeviceProfileHeaderMode;
            cp.IgnorePreviousActivitesWhenNeeded = csps.IgnorePreviousActivitiesWhenNeeded;
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
