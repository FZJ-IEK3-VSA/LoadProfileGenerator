using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public class IndividualHouseholdSumProfileProcessor : HouseholdLoadTypeStepBase {
        [NotNull] private readonly FileFactoryAndTracker _fft;

        public IndividualHouseholdSumProfileProcessor([NotNull] CalcDataRepository repository,
                                                      [NotNull] FileFactoryAndTracker fft,
                                                      [NotNull] ICalculationProfiler calculationProfiler) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.IndividualSumProfiles),
            calculationProfiler,
            "Individual Household Sum Profiles") =>
            _fft = fft;

        protected override void PerformActualStep(IStepParameters parameters)
        {
            HouseholdLoadtypeStepParameters p = (HouseholdLoadtypeStepParameters)parameters;

            if (p.Key.HouseholdKey == Constants.GeneralHouseholdKey) {
                return;
            }

            var efc = Repository.ReadEnergyFileColumns(p.Key.HouseholdKey);
            RunIndividualHouseholds(p.LoadType, p.EnergyFileRows, efc, p.Key.HouseholdKey);
        }

        private void RunIndividualHouseholds([NotNull] CalcLoadTypeDto dstLoadType,
                                             [NotNull] [ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
                                             [NotNull] EnergyFileColumns efc,
                                             [NotNull] HouseholdKey key)
        {
            DateStampCreator dsc = new DateStampCreator(Repository.CalcParameters);
            //var householdKeys = efc.ColumnEntriesByColumn[dstLoadType].Values.Select(entry => entry.HouseholdKey).Distinct().ToList();
            if (!efc.ColumnCountByLoadType.ContainsKey(dstLoadType)) {
                return;
            }

            var columns = efc.ColumnEntriesByColumn[dstLoadType].Values.Where(entry => entry.HouseholdKey == key).Select(entry => entry.Column)
                .ToList();
            var hhname = "." + key + ".";
            StreamWriter sumfile = null;
            StreamWriter normalfile = null;
            if (Repository.CalcParameters.IsSet(CalcOption.IndividualSumProfiles)) {
                sumfile = _fft.MakeFile<StreamWriter>("SumProfiles" + hhname + dstLoadType.Name + ".csv",
                    "Summed up energy profile for all devices for " + dstLoadType.Name + " for " + hhname,
                    true,
                    ResultFileID.SumProfileForHouseholds,
                    key,
                    TargetDirectory.Results,
                    Repository.CalcParameters.InternalStepsize,
                    dstLoadType.ConvertToLoadTypeInformation());
                sumfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() + "Sum [" + dstLoadType.UnitOfSum + "]");
            }

            if (Repository.CalcParameters.IsSet(CalcOption.DeviceProfiles)) {
                normalfile = _fft.MakeFile<StreamWriter>("DeviceProfiles" + hhname + dstLoadType.Name + ".csv",
                    "Energy use by each device in each Timestep for " + dstLoadType.Name + " for " + hhname,
                    true,
                    ResultFileID.DeviceProfileForHouseholds,
                    key,
                    TargetDirectory.Results,
                    Repository.CalcParameters.InternalStepsize,
                    dstLoadType.ConvertToLoadTypeInformation());
                normalfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() + efc.GetTotalHeaderString(dstLoadType, columns));
            }

            foreach (var efr in energyFileRows) {
                if (!efr.Timestep.DisplayThisStep) {
                    continue;
                }

                var time = dsc.MakeTimeString(efr.Timestep);
                if (Repository.CalcParameters.IsSet(CalcOption.DeviceProfiles)) {
                    var indidivdual = time + efr.GetEnergyEntriesAsString(true, dstLoadType, columns, Repository.CalcParameters.CSVCharacter);
                    if (normalfile == null) {
                        throw new LPGException("File is null, even though it shouldn't be. Please report.");
                    }

                    normalfile.WriteLine(indidivdual);
                }

                if (Repository.CalcParameters.IsSet(CalcOption.IndividualSumProfiles)) {
                    var sumstring = time + efr.GetSumForCertainCols(columns) * dstLoadType.ConversionFactor;
                    if (sumfile == null) {
                        throw new LPGException("File is null, even though it shouldn't be. Please report.");
                    }

                    sumfile.WriteLine(sumstring);
                }
            }

            if (Repository.CalcParameters.IsSet(CalcOption.DeviceProfiles)) {
                if (normalfile == null) {
                    throw new LPGException("File is null, even though it shouldn't be. Please report.");
                }

                normalfile.Flush();
            }
        }
    }
}