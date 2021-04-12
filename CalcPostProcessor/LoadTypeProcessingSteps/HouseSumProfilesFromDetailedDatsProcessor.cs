using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging;

namespace CalcPostProcessor.LoadTypeProcessingSteps {
    public class HouseSumProfilesFromDetailedDatsProcessor : LoadTypeStepBase
    {
        [JetBrains.Annotations.NotNull]
        private readonly IFileFactoryAndTracker _fft;

        public HouseSumProfilesFromDetailedDatsProcessor([JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft,
                                                         [JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                                         [JetBrains.Annotations.NotNull] ICalculationProfiler profiler)
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
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = Repository.CalcParameters.DecimalSeperator;
            foreach (var efr in p.EnergyFileRows)
            {
                if (!efr.Timestep.DisplayThisStep)
                {
                    continue;
                }

                var time = dsc.MakeTimeString(efr.Timestep);
                double val = (efr.SumCached * dstLoadType.ConversionFactor);
                var sumstring = time +val.ToString(nfi);
                sumfile.WriteLine(sumstring);
            }
        }

        [JetBrains.Annotations.NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {
            CalcOption.DetailedDatFiles
        };
    }
}