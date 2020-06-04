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
    public class IndividualHouseholdDeviceProfileProcessor : HouseholdLoadTypeStepBase {
        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public IndividualHouseholdDeviceProfileProcessor([NotNull] CalcDataRepository repository,
                                                         [NotNull] IFileFactoryAndTracker fft,
                                                         [NotNull] ICalculationProfiler calculationProfiler) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.DeviceProfilesIndividualHouseholds),
            calculationProfiler,
            "Individual Household Device Profiles") =>
            _fft = fft;

        protected override void PerformActualStep(IStepParameters parameters)
        {
            HouseholdLoadtypeStepParameters p = (HouseholdLoadtypeStepParameters)parameters;

            if (p.Key.HouseholdKey == Constants.GeneralHouseholdKey) {
                return;
            }

            var efc = Repository.ReadEnergyFileColumns(p.Key.HouseholdKey);

            DateStampCreator dsc = new DateStampCreator(Repository.CalcParameters);
            //var householdKeys = efc.ColumnEntriesByColumn[dstLoadType].Values.Select(entry => entry.HouseholdKey).Distinct().ToList();
            if (!efc.ColumnCountByLoadType.ContainsKey(p.LoadType))
            {
                return;
            }

            var key = p.Key.HouseholdKey;
            var columns = efc.ColumnEntriesByColumn[p.LoadType].Values.Where(entry => entry.HouseholdKey == key).Select(entry => entry.Column)
                .ToList();
            var hhname = "." + key + ".";

            var normalfile = _fft.MakeFile<StreamWriter>("DeviceProfiles" + hhname + p.LoadType.Name + ".csv",
                "Energy use by each device in each Timestep for " + p.LoadType.Name + " for " + hhname,
                true,
                ResultFileID.DeviceProfileForHouseholds,
                key,
                TargetDirectory.Results,
                Repository.CalcParameters.InternalStepsize, CalcOption.DeviceProfilesIndividualHouseholds,
                p.LoadType.ConvertToLoadTypeInformation());
            normalfile.WriteLine(p.LoadType.Name + "." + dsc.GenerateDateStampHeader() + efc.GetTotalHeaderString(p.LoadType, columns));

            foreach (var efr in p.EnergyFileRows)
            {
                if (!efr.Timestep.DisplayThisStep)
                {
                    continue;
                }

                var time = dsc.MakeTimeString(efr.Timestep);
                var indidivdual = time + efr.GetEnergyEntriesAsString(true, p.LoadType, columns, Repository.CalcParameters.CSVCharacter);
                normalfile.WriteLine(indidivdual);

            }

            normalfile.Flush();
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.DetailedDatFiles};

    }
}