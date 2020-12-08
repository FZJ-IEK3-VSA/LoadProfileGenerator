using System;
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
    public class HouseSumProfileFromOverallDatProcessor : LoadTypeSumStepBase {
        [JetBrains.Annotations.NotNull] private readonly IFileFactoryAndTracker _fft;

        public HouseSumProfileFromOverallDatProcessor([JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                   [JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft,
                                   [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.OverallSum), calculationProfiler,
                "Sum Profile Creation") =>
            _fft = fft;

        public void ProcessSumFile([JetBrains.Annotations.NotNull] CalcLoadTypeDto dstLoadType)
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
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = Repository.CalcParameters.DecimalSeperator;
            for (var i = 0; i < values.Length; i++) {
                var ts = new TimeStep(i, Repository.CalcParameters);
                if (!ts.DisplayThisStep) {
                    continue;
                }

                var rowsum = values[i] * dstLoadType.ConversionFactor;
                var line = dsc.MakeTimeString(ts) + rowsum.ToString(nfi) +
                           Repository.CalcParameters.CSVCharacter;
                dstFile.WriteLine(line);
            }
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            var ltstep = (LoadtypeSumStepParameters)parameters;
            ProcessSumFile(ltstep.LoadType);
        }

        [JetBrains.Annotations.NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {
            CalcOption.OverallDats
        };
    }
}