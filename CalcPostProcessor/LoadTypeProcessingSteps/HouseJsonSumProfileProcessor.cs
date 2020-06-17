using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CalcPostProcessor.LoadTypeProcessingSteps {
    public class HouseJsonSumProfileProcessor : LoadTypeStepBase {
        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public HouseJsonSumProfileProcessor([NotNull] CalcDataRepository repository,
                                       [NotNull] IFileFactoryAndTracker fft,
                                       [NotNull] ICalculationProfiler calculationProfiler)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.JsonHouseSumFiles), calculationProfiler,
                "Json Sum Profile Creation") =>
            _fft = fft;

        protected override void PerformActualStep(IStepParameters parameters)
        {
            var ltstep = (LoadtypeStepParameters)parameters;
            Run(ltstep.LoadType, ltstep.EnergyFileRows);
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() { CalcOption.DetailedDatFiles};

        private void Run([NotNull] CalcLoadTypeDto dstLoadType,
                         [NotNull] [ItemNotNull] List<OnlineEnergyFileRow> energyFileRows)
        {
            var calcParameters = Repository.CalcParameters;
            if (!calcParameters.IsSet(CalcOption.JsonHouseSumFiles)) {
                return;
            }
            var he = new HouseholdKeyEntry(Constants.GeneralHouseholdKey,"Sum of the entire House",HouseholdKeyType.General,
                "Sum",Repository.CalcObjectInformation.CalcObjectName,"");
            var jrf = new JsonSumProfile("Sum profile", calcParameters.InternalStepsize,
                calcParameters.OfficialStartTime, dstLoadType.Name, dstLoadType.UnitOfSum,
                dstLoadType.ConvertToLoadTypeInformation(), he);
            foreach (var efr in energyFileRows) {
                if (!efr.Timestep.DisplayThisStep) {
                    continue;
                }

                jrf.Values.Add(efr.SumCached * dstLoadType.ConversionFactor);
            }

            var sumfile = _fft.MakeFile<StreamWriter>("Sum." + dstLoadType.FileName + ".json",
                "Summed up energy profile for all devices for " + dstLoadType.Name, true,
                ResultFileID.JsonHouseSums, Constants.GeneralHouseholdKey, TargetDirectory.Results,
                calcParameters.InternalStepsize,CalcOption.JsonHouseSumFiles,
                dstLoadType.ConvertToLoadTypeInformation());
            sumfile.Write(JsonConvert.SerializeObject(jrf, Formatting.Indented));
            sumfile.Flush();
        }
    }
}