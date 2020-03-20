using System;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging;

namespace CalcPostProcessor.LoadTypeProcessingSteps
{
    public class SumProfileProcessor: LoadTypeStepBase
    {
        [JetBrains.Annotations.NotNull]
        private readonly FileFactoryAndTracker _fft;

        public void ProcessSumFile([JetBrains.Annotations.NotNull] CalcLoadTypeDto dstLoadType)
        {
            var dsc = new DateStampCreator(Repository.CalcParameters);
            if (!_fft.CheckForResultFileEntry(ResultFileID.OnlineSumActivationFiles, dstLoadType.Name,
                Constants.GeneralHouseholdKey, null, null))
            {
                return;
            }
            var dstFile = _fft.MakeFile<StreamWriter>("Overall.SumProfiles." + dstLoadType.Name + ".csv",
                "Overall summed up energy profile for everything in the house/household for " + dstLoadType.Name, true,
                ResultFileID.OverallSumFile, Constants.GeneralHouseholdKey, TargetDirectory.Results,
                Repository.CalcParameters.InternalStepsize, dstLoadType.ConvertToLoadTypeInformation());
            dstFile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() + "Sum [" +
                              dstLoadType.UnitOfSum + "]");
            var srcFile = _fft.GetResultFileEntry(ResultFileID.OnlineSumActivationFiles, dstLoadType.Name,
                Constants.GeneralHouseholdKey, null, null);
            var bytes = File.ReadAllBytes(srcFile.FullFileName);
            var values = new double[bytes.Length / 8];
            Buffer.BlockCopy(bytes, 0, values, 0, values.Length * 8);

            for (var i = 0; i < values.Length; i++)
            {
                TimeStep ts = new TimeStep(i, Repository.CalcParameters);
                if (!ts.DisplayThisStep) {
                    continue;
                }
                var rowsum = values[i] * dstLoadType.ConversionFactor;
                var line = dsc.MakeTimeString(ts) + rowsum.ToString(Config.CultureInfo) +
                           Repository.CalcParameters.CSVCharacter;
                dstFile.WriteLine(line);
            }
        }

        public SumProfileProcessor([JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                   [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.IndividualSumProfiles),calculationProfiler,"Sum Profile Creation")
        {
            _fft = fft;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters ltstep = (LoadtypeStepParameters)parameters;
            ProcessSumFile(ltstep.LoadType);
        }
    }
}
