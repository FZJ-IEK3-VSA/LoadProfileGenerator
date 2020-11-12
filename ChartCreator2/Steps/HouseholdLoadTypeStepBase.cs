using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;

namespace ChartCreator2.Steps
{
    public interface IHouseholdLoadTypeStep : IRequireOptions
    {
        void Run([JetBrains.Annotations.NotNull] IStepParameters parameters);
        bool IsEnabled();
    }
    public abstract class HouseholdLoadTypeStepBase : BasicChartProcessingStep, IHouseholdLoadTypeStep
    {
        protected HouseholdLoadTypeStepBase([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] List<CalcOption> options,
                                             [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                             [JetBrains.Annotations.NotNull] string stepName) : base(repository, options, calculationProfiler,
            stepName,0)
        {
        }
    }
}
