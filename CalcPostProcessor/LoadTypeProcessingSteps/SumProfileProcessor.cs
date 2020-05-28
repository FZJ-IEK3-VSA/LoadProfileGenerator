using System;
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
    public class JsonSumProfileProcessor : LoadTypeStepBase {
        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public JsonSumProfileProcessor([NotNull] CalcDataRepository repository,
                                       [NotNull] IFileFactoryAndTracker fft,
                                       [NotNull] ICalculationProfiler calculationProfiler)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.JsonSumFiles), calculationProfiler,
                "Json Sum Profile Creation") =>
            _fft = fft;

        protected override void PerformActualStep(IStepParameters parameters)
        {
            var ltstep = (LoadtypeStepParameters)parameters;
            Run(ltstep.LoadType, ltstep.EnergyFileRows);
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>();

        private void Run([NotNull] CalcLoadTypeDto dstLoadType,
                         [NotNull] [ItemNotNull] List<OnlineEnergyFileRow> energyFileRows)
        {
            var calcParameters = Repository.CalcParameters;
            if (!calcParameters.IsSet(CalcOption.JsonSumFiles)) {
                return;
            }

            var jrf = new JsonResultFile("Sum profile", calcParameters.InternalStepsize,
                calcParameters.OfficialStartTime, dstLoadType.Name, dstLoadType.UnitOfSum);
            foreach (var efr in energyFileRows) {
                if (!efr.Timestep.DisplayThisStep) {
                    continue;
                }

                jrf.Values.Add(efr.SumCached * dstLoadType.ConversionFactor);
            }

            var sumfile = _fft.MakeFile<StreamWriter>("Sum." + dstLoadType.FileName + ".json",
                "Summed up energy profile for all devices for " + dstLoadType.Name, true,
                ResultFileID.JsonSums, Constants.GeneralHouseholdKey, TargetDirectory.Results,
                calcParameters.InternalStepsize,CalcOption.JsonSumFiles,
                dstLoadType.ConvertToLoadTypeInformation());
            sumfile.Write(JsonConvert.SerializeObject(jrf, Formatting.Indented));
            sumfile.Flush();
        }
    }

    public class SumProfileProcessor : LoadTypeSumStepBase {
        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public SumProfileProcessor([NotNull] CalcDataRepository repository,
                                   [NotNull] IFileFactoryAndTracker fft,
                                   [NotNull] ICalculationProfiler calculationProfiler)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.OverallSum), calculationProfiler,
                "Sum Profile Creation") =>
            _fft = fft;

        public void ProcessSumFile([NotNull] CalcLoadTypeDto dstLoadType)
        {
            var dsc = new DateStampCreator(Repository.CalcParameters);
            if (!_fft.CheckForResultFileEntry(ResultFileID.OnlineSumActivationFiles, dstLoadType.Name,
                Constants.GeneralHouseholdKey, null, null)) {
                return;
            }

            var dstFile = _fft.MakeFile<StreamWriter>("Overall.SumProfiles." + dstLoadType.Name + ".csv",
                "Overall summed up energy profile for everything in the house/household for " + dstLoadType.Name, true,
                ResultFileID.OverallSumFile, Constants.GeneralHouseholdKey, TargetDirectory.Results,
                Repository.CalcParameters.InternalStepsize, CalcOption.OverallSum,  dstLoadType.ConvertToLoadTypeInformation());
            dstFile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() + "Sum [" +
                              dstLoadType.UnitOfSum + "]");
            var srcFile = _fft.GetResultFileEntry(ResultFileID.OnlineSumActivationFiles, dstLoadType.Name,
                Constants.GeneralHouseholdKey, null, null);
            if (srcFile.FullFileName == null) {
                throw new LPGException("Filename was null in resultfileentry");
            }
            var bytes = File.ReadAllBytes(srcFile.FullFileName);
            var values = new double[bytes.Length / 8];
            Buffer.BlockCopy(bytes, 0, values, 0, values.Length * 8);

            for (var i = 0; i < values.Length; i++) {
                var ts = new TimeStep(i, Repository.CalcParameters);
                if (!ts.DisplayThisStep) {
                    continue;
                }

                var rowsum = values[i] * dstLoadType.ConversionFactor;
                var line = dsc.MakeTimeString(ts) + rowsum.ToString(Config.CultureInfo) +
                           Repository.CalcParameters.CSVCharacter;
                dstFile.WriteLine(line);
            }
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            var ltstep = (LoadtypeSumStepParameters)parameters;
            ProcessSumFile(ltstep.LoadType);
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {
            CalcOption.OverallDats
        };
    }
}