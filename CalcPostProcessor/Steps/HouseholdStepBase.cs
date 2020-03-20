using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;

namespace CalcPostProcessor.Steps
{
    public interface IGeneralHouseholdStep
    {
        void Run([JetBrains.Annotations.NotNull] IStepParameters parameters);
        bool IsEnabled();
    }
    public abstract class HouseholdStepBase : BasicPostProcessingStep, IGeneralHouseholdStep
    {
        protected HouseholdStepBase([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] List<CalcOption> options,
                                             [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                             [JetBrains.Annotations.NotNull] string stepName) : base(repository, options, calculationProfiler,
            stepName)
        {
        }
    }
}
