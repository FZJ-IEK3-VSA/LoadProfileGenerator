using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;

namespace ChartCreator2.Steps
{
    public interface IGeneralStep: IRequireOptions
    {
        void Run([JetBrains.Annotations.NotNull] IStepParameters parameters);
        bool IsEnabled();
    }
    public abstract class GeneralStepBase : BasicPostProcessingStep, IGeneralStep
    {
        protected GeneralStepBase([JetBrains.Annotations.NotNull] CalcDataRepository repository,[JetBrains.Annotations.NotNull] List< CalcOption> option,
                                  [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                  [JetBrains.Annotations.NotNull] string stepName, int priority) : base(
            repository, option, calculationProfiler,
            stepName,priority)
        {
        }
    }
}
