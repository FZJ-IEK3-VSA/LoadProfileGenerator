using System.Collections.Generic;
using Automation;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.Steps
{
    public interface IGeneralHouseholdStep
    {
        void Run([NotNull] IStepParameters parameters);
        bool IsEnabled();
    }
    public abstract class HouseholdStepBase : BasicPostProcessingStep, IGeneralHouseholdStep
    {
        protected HouseholdStepBase([NotNull] CalcDataRepository repository, [NotNull] List<CalcOption> options,
                                             [NotNull] ICalculationProfiler calculationProfiler,
                                             [NotNull] string stepName) : base(repository, options, calculationProfiler,
            stepName)
        {
        }
    }
}
