using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;

namespace CalcPostProcessor.Steps
{
    public interface IHouseholdLoadTypeStep
    {
        void Run([JetBrains.Annotations.NotNull] IStepParameters parameters);
        bool IsEnabled();
    }
    public abstract class HouseholdLoadTypeStepBase : BasicPostProcessingStep, IHouseholdLoadTypeStep
    {
        protected HouseholdLoadTypeStepBase([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] List<CalcOption> options,
                                             [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                             [JetBrains.Annotations.NotNull] string stepName) : base(repository, options, calculationProfiler,
            stepName,0)
        {
        }
    }
}
