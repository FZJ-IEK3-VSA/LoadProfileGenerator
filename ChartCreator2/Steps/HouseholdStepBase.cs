using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;

namespace ChartCreator2.Steps
{
    public interface IGeneralHouseholdStep : IRequireOptions
    {
        void Run([JetBrains.Annotations.NotNull] IStepParameters parameters);
        bool IsEnabled();

        int Priority { get; }
    }
    public abstract class HouseholdStepBase : BasicChartProcessingStep, IGeneralHouseholdStep
    {
        protected HouseholdStepBase([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] List<CalcOption> options,
                                             [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                             [JetBrains.Annotations.NotNull] string stepName, int priority) : base(repository, options, calculationProfiler,
            stepName, priority)
        {
        }
    }
}
