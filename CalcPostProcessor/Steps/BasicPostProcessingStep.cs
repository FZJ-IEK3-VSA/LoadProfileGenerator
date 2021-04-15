using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.Steps
{
#pragma warning disable CA1040 // Avoid empty interfaces
    public interface IStepParameters {
#pragma warning restore CA1040 // Avoid empty interfaces
    }
    public class GeneralStepParameters:IStepParameters {
    }
    public class HouseholdStepParameters : IStepParameters
    {
        public HouseholdStepParameters([NotNull] HouseholdKeyEntry key) => Key = key;

        [NotNull]
        public HouseholdKeyEntry Key { get;  }
    }

    public class HouseholdLoadtypeStepParameters : IStepParameters
    {
        public HouseholdLoadtypeStepParameters([NotNull] HouseholdKeyEntry key, [NotNull] CalcLoadTypeDto loadType,
                                                      [NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows)
        {
            Key = key;
            LoadType = loadType;
            EnergyFileRows = energyFileRows;
        }

        [NotNull]
        public HouseholdKeyEntry Key { get; }
        [NotNull]
        public CalcLoadTypeDto LoadType { get; }
        [NotNull]
        [ItemNotNull]
        public List<OnlineEnergyFileRow> EnergyFileRows { get; }
    }

    public class LoadtypeStepParameters : IStepParameters
    {
        [NotNull]
        public CalcLoadTypeDto LoadType { get; }
        [NotNull]
        [ItemNotNull]
        public List<OnlineEnergyFileRow> EnergyFileRows { get; }

        public LoadtypeStepParameters([NotNull] CalcLoadTypeDto loadType,
                                            [NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows)
        {
            LoadType = loadType;
            EnergyFileRows = energyFileRows;
        }
    }

    public class LoadtypeSumStepParameters : IStepParameters
    {
        [NotNull]
        public CalcLoadTypeDto LoadType { get; }
        public LoadtypeSumStepParameters([NotNull] CalcLoadTypeDto loadType)
        {
            LoadType = loadType;
        }
    }

    public interface IRequireOptions {
        public List<CalcOption> NeededOptions { get; }
        List<CalcOption> Options { get; }
        public string StepName { get; }
    }
    public  abstract class BasicPostProcessingStep: IRequireOptions
    {

        public int Priority { get; }
        [NotNull] private readonly List<CalcOption> _options;

        protected BasicPostProcessingStep([NotNull] CalcDataRepository repository, [NotNull] List<CalcOption> options,
                                             [NotNull] ICalculationProfiler calculationProfiler,
                                             [NotNull] string stepName, int priority)
        {
            Repository = repository;
            _options = options;
            _calculationProfiler = calculationProfiler;
            _stepName = stepName;
            Priority = priority;
        }
        public void Run([NotNull] IStepParameters parameters)
        {
            if (!IsEnabled()) {
                return;
            }

            try {
                _calculationProfiler.StartPart(StepName);
                PerformActualStep(parameters);
            }
            finally {
                _calculationProfiler.StopPart(StepName);
            }
        }

        protected abstract void PerformActualStep([NotNull] IStepParameters parameters);

        [NotNull]
        protected CalcDataRepository Repository { get; }

        [NotNull]
        public List<CalcOption> Options => _options;

        [NotNull]
        private readonly ICalculationProfiler _calculationProfiler;
        [NotNull]
        private readonly string _stepName;

        public bool IsEnabled()
        {
            if (_options.Any(x=> Repository.CalcParameters.IsSet(x)))
            {
                return true;
            }
            return false;
        }

        public  abstract List<CalcOption> NeededOptions { get; }

        [NotNull]
        public string StepName => _stepName;
    }
}
