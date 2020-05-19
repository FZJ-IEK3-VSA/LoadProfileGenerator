using System;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;


namespace CalcPostProcessor.GeneralHouseholdSteps {
    public class MakeBodilyActivityLevelStatistics : HouseholdStepBase
    {
        [NotNull] private readonly CalcParameters _calcParameters;

        [NotNull] private readonly ICalculationProfiler _calculationProfiler;

        [NotNull] private readonly IInputDataLogger _inputDataLogger;

        [NotNull] private readonly CalcDataRepository _repository;

        public MakeBodilyActivityLevelStatistics([NotNull] ICalculationProfiler calculationProfiler,
                                                 [NotNull] CalcDataRepository repository,
                                                 [NotNull] IInputDataLogger inputDataLogger) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.BodilyActivityStatistics),
            calculationProfiler,
            "Make Bodily Activity Level Statistics",1)
        {
            _calculationProfiler = calculationProfiler;
            _repository = repository;
            _inputDataLogger = inputDataLogger;
            _calcParameters = _repository.CalcParameters;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            var entry = hhp.Key;
            if (entry.KeyType != HouseholdKeyType.Household)
            {
                return;
            }

            var householdKey = entry.HouseholdKey;
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());


            Logger.Info("Starting to complete list of heat gains by human activity at each time step...");

            var singletimestepEntries = _repository.ReadSingleTimestepActionEntries(householdKey)
                .OrderBy(x => x.TimeStep).ThenBy(x => x.PersonGuid)
                .ToList();
            if (singletimestepEntries.Count == 0) {
                throw new LPGException("No file for actions each time step found");
            }
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
                bals.ActivityLevels.Add(level, new List<double>(new double[ _calcParameters.OfficalTimesteps]));
            }
            foreach (var actionEntry in singletimestepEntries)
            {
                var ae = actionEntryDict[actionEntry.ActionEntryGuid];
                var ts = actionEntry.TimeStep;
                bals.ActivityLevels[ae.BodilyActivityLevel][ts] += 1;
            }
            _inputDataLogger.Save(householdKey,bals);
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());

        }
    }
}