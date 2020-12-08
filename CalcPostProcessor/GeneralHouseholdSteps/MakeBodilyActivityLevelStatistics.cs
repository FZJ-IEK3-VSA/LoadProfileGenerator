using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.Enums;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;


namespace CalcPostProcessor.GeneralHouseholdSteps {
    public class MakeBodilyActivityLevelStatistics : HouseholdStepBase
    {
        [JetBrains.Annotations.NotNull] private readonly ICalculationProfiler _calculationProfiler;

        [JetBrains.Annotations.NotNull] private readonly IInputDataLogger _inputDataLogger;

        [JetBrains.Annotations.NotNull] private readonly CalcDataRepository _repository;

        public MakeBodilyActivityLevelStatistics([JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                                 [JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                                 [JetBrains.Annotations.NotNull] IInputDataLogger inputDataLogger) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.BodilyActivityStatistics),
            calculationProfiler,
            "Make Bodily Activity Level Statistics",1)
        {
            _calculationProfiler = calculationProfiler;
            _repository = repository;
            _inputDataLogger = inputDataLogger;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            var calcParameters = _repository.CalcParameters;
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            var entry = hhp.Key;
            if (entry.KeyType != HouseholdKeyType.Household)
            {
                return;
            }

            var householdKey = entry.HHKey;
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());


            Logger.Info("Starting to complete list of heat gains by human activity at each time step...");

            var actionEntries = _repository.ReadActionEntries(householdKey);
            if (actionEntries.Count == 0) {
                throw new LPGException("");
            }
            var actionEntryDict = new Dictionary<StrGuid, ActionEntry>();
            foreach (var ae in actionEntries)
            {
                actionEntryDict.Add(ae.ActionEntryGuid, ae);
            }

            BodilyActivityLevelStatistics bals = new BodilyActivityLevelStatistics(householdKey);
            foreach (BodilyActivityLevel level in Enum.GetValues(typeof(BodilyActivityLevel)))
            {

                bals.ActivityLevels.Add(level, new List<double>(new double[ calcParameters.OfficalTimesteps]));
            }
            var singletimestepEntries = _repository.ReadSingleTimestepActionEntries(householdKey);
            foreach (var actionEntry in singletimestepEntries)
            {
                var ae = actionEntryDict[actionEntry.ActionEntryGuid];
                var ts = actionEntry.TimeStep;
                bals.ActivityLevels[ae.BodilyActivityLevel][ts] += 1;
            }
            _inputDataLogger.Save(householdKey,bals);
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());

        }

        [JetBrains.Annotations.NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.ActionsEachTimestep};
    }
}