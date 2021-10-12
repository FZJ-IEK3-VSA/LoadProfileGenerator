using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.SQLResultLogging;

namespace CalcPostProcessor.GeneralSteps
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class DatFileDeletor: GeneralStepBase
    {
        [JetBrains.Annotations.NotNull]
        private readonly IFileFactoryAndTracker _fft;

        public DatFileDeletor([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                    [JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.HouseholdContents), calculationProfiler, "Delete .dat files", 0)
        {
            _fft = fft;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            // insert deleting function here
        }

        [JetBrains.Annotations.NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.DeleteDatFiles};
    }
}
