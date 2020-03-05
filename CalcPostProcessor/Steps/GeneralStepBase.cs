using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.Steps
{
    public interface IGeneralStep
    {
        void Run([NotNull] IStepParameters parameters);
        bool IsEnabled();
    }
    public abstract class GeneralStepBase : BasicPostProcessingStep, IGeneralStep
    {
        public GeneralStepBase([NotNull] CalcDataRepository repository,[NotNull] List< CalcOption> option,
                                             [NotNull] ICalculationProfiler calculationProfiler,
                                             [NotNull] string stepName) : base(repository, option, calculationProfiler,
            stepName)
        {
        }
    }
}
