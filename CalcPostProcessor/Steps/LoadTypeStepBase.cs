using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;

namespace CalcPostProcessor.Steps
{
    public interface ILoadTypeStep
    {
        void Run([JetBrains.Annotations.NotNull] IStepParameters parameters);
        bool IsEnabled();
    }
    public abstract class LoadTypeStepBase : BasicPostProcessingStep, ILoadTypeStep
    {
        protected LoadTypeStepBase([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] List<CalcOption> options,
                                             [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                             [JetBrains.Annotations.NotNull] string stepName) : base(repository, options, calculationProfiler,
            stepName)
        {
        }
    }
}
