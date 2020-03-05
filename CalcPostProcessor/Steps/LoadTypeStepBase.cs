using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.Steps
{
    public interface ILoadTypeStep
    {
        void Run([NotNull] IStepParameters parameters);
        bool IsEnabled();
    }
    public abstract class LoadTypeStepBase : BasicPostProcessingStep, ILoadTypeStep
    {
        protected LoadTypeStepBase([NotNull] CalcDataRepository repository, [NotNull] List<CalcOption> options,
                                             [NotNull] ICalculationProfiler calculationProfiler,
                                             [NotNull] string stepName) : base(repository, options, calculationProfiler,
            stepName)
        {
        }
    }
}
