using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public class IndividualHouseholdSumProfileProcessor : HouseholdLoadTypeStepBase {
        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public IndividualHouseholdSumProfileProcessor([NotNull] CalcDataRepository repository,
                                                      [NotNull] IFileFactoryAndTracker fft,
                                                      [NotNull] ICalculationProfiler calculationProfiler) : base(
            repository,
            AutomationUtili.GetOptionList(CalcOption.HouseholdSumProfilesFromDetailedDats),
            calculationProfiler,
            "Individual Household Sum Profiles") =>
            _fft = fft;

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption> {CalcOption.DetailedDatFiles};

        protected override void PerformActualStep(IStepParameters parameters)
        {
            var p = (HouseholdLoadtypeStepParameters)parameters;

            if (p.Key.HouseholdKey == Constants.GeneralHouseholdKey) {
                return;
            }

            var efc = Repository.ReadEnergyFileColumns(p.Key.HouseholdKey);
            var dsc = new DateStampCreator(Repository.CalcParameters);
            var dstLoadType = p.LoadType;

            if (!efc.ColumnCountByLoadType.ContainsKey(dstLoadType)) {
                return;
            }

            var columns = efc.ColumnEntriesByColumn[dstLoadType].Values
                .Where(entry => entry.HouseholdKey == p.Key.HouseholdKey)
                .Select(entry => entry.Column)
                .ToList();
            var hhname = "." + p.Key.HouseholdKey + ".";
            var sumfile = _fft.MakeFile<StreamWriter>("SumProfiles" + hhname + dstLoadType.Name + ".csv",
                "Summed up energy profile for all devices for " + dstLoadType.Name + " for " + hhname,
                true,
                ResultFileID.SumProfileForHouseholds,
                p.Key.HouseholdKey,
                TargetDirectory.Results,
                Repository.CalcParameters.InternalStepsize, CalcOption.HouseSumProfilesFromDetailedDats,
                dstLoadType.ConvertToLoadTypeInformation());
            sumfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() + "Sum [" + dstLoadType.UnitOfSum +
                              "]");


            foreach (var efr in p.EnergyFileRows) {
                if (!efr.Timestep.DisplayThisStep) {
                    continue;
                }

                var time = dsc.MakeTimeString(efr.Timestep);
                var sumstring = time + efr.GetSumForCertainCols(columns) * dstLoadType.ConversionFactor;
                sumfile.WriteLine(sumstring);
            }

            sumfile.Flush();
        }
    }
}