using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;

namespace ChartCreator2.Steps
{
    public interface ILoadTypeStep: IRequireOptions
    {
        void Run([JetBrains.Annotations.NotNull] IStepParameters parameters);
        bool IsEnabled();
    }

    public interface ILoadTypeSumStep: IRequireOptions
    {
        void Run([JetBrains.Annotations.NotNull] IStepParameters parameters);
        bool IsEnabled();
    }

    public abstract class LoadTypeSumStepBase : BasicPostProcessingStep, ILoadTypeSumStep
    {
        protected LoadTypeSumStepBase([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] List<CalcOption> options,
                                      [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                      [JetBrains.Annotations.NotNull] string stepName) : base(repository, options, calculationProfiler,
            stepName,0)
        {
        }
    }
    public abstract class LoadTypeStepBase : BasicPostProcessingStep, ILoadTypeStep
    {
        protected LoadTypeStepBase([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] List<CalcOption> options,
                                             [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                             [JetBrains.Annotations.NotNull] string stepName) : base(repository, options, calculationProfiler,
            stepName,0)
        {
        }
    }
}
