using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public class HouseSumProfilesFromDetailedDatsProcessor : LoadTypeStepBase
    {
        [NotNull]
        private readonly IFileFactoryAndTracker _fft;

        public HouseSumProfilesFromDetailedDatsProcessor([NotNull] IFileFactoryAndTracker fft,
                                                         [NotNull] CalcDataRepository repository,
                                                         [NotNull] ICalculationProfiler profiler)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.HouseSumProfilesFromDetailedDats),
                profiler, "Individual Sum Profiles")
        {
            _fft = fft;
        }


        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            var dsc = new DateStampCreator(Repository.CalcParameters);
            var calcParameters = Repository.CalcParameters;
            CalcLoadTypeDto dstLoadType = p.LoadType;
            var householdKey = Constants.GeneralHouseholdKey;
            var sumfile = _fft.MakeFile<StreamWriter>("SumProfiles." + dstLoadType.Name + ".csv",
                "Summed up energy profile for all devices for " + dstLoadType.Name, true,
                ResultFileID.CSVSumProfile, householdKey, TargetDirectory.Results,
                calcParameters.InternalStepsize, CalcOption.HouseSumProfilesFromDetailedDats,
                dstLoadType.ConvertToLoadTypeInformation());
            sumfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() + "Sum [" +
                              dstLoadType.UnitOfSum + "]");
            foreach (var efr in p.EnergyFileRows)
            {
                if (!efr.Timestep.DisplayThisStep)
                {
                    continue;
                }

                var time = dsc.MakeTimeString(efr.Timestep);
                var sumstring = time + (efr.SumCached * dstLoadType.ConversionFactor).ToString(Config.CultureInfo);
                sumfile.WriteLine(sumstring);
            }
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {
            CalcOption.DetailedDatFiles
        };
    }
}