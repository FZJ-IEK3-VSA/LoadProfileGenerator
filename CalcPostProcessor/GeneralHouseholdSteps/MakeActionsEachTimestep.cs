using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;


namespace CalcPostProcessor.GeneralHouseholdSteps {

    public class MakeActionsEachTimestep : HouseholdStepBase
    {
        [NotNull] private readonly ICalculationProfiler _calculationProfiler;

        [NotNull] private readonly IInputDataLogger _inputDataLogger;

        [NotNull] private readonly CalcDataRepository _repository;

        public MakeActionsEachTimestep([NotNull] ICalculationProfiler calculationProfiler,
                                [NotNull] CalcDataRepository repository,
                                [NotNull] IInputDataLogger inputDataLogger) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.ActionsEachTimestep), calculationProfiler,
            "Make Actions Each Timestep List",0)
        {
            _repository = repository;
            _inputDataLogger = inputDataLogger;
            _calculationProfiler = calculationProfiler;
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


            Logger.Info("Starting to complete list of actions for each person at each time step for statistical analysis...");
            var displayNegativeTime = calcParameters.ShowSettlingPeriodTime;

            var activitiesPerPerson = ReadActivities(householdKey);
            //var fileIdx = 0;
            if (activitiesPerPerson.Count == 0)
            {
                throw new LPGException("There were no activities for any person in the household " + householdKey.Key);
            }

            List<SingleTimestepActionEntry> allSteps = new List<SingleTimestepActionEntry>();
            foreach (var personActivities in activitiesPerPerson)
            {
                Logger.Info("Starting to generate the carpet plot for " + personActivities.Key.Name + "...");
                var times = MakeTimeArray(displayNegativeTime, personActivities.Value);
                allSteps.AddRange(times);
            }
            _inputDataLogger.SaveList< SingleTimestepActionEntry>(allSteps.ConvertAll(x => (IHouseholdKey)x));
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());

        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.ActionEntries, CalcOption.HouseholdContents};

        [NotNull]
        [ItemNotNull]
        private List<SingleTimestepActionEntry> MakeTimeArray(bool displayNegativeTime,
                                                [NotNull] [ItemNotNull] List<ActionEntry> actionsForPerson)
        {
            var calcParameters = _repository.CalcParameters;
            List<SingleTimestepActionEntry> timestepActionEntries = new List<SingleTimestepActionEntry>();
            DateTime currentTime;
            TimeSpan calculationTimeSpan;
            if (displayNegativeTime)
            {
                currentTime = calcParameters.InternalStartTime;
                calculationTimeSpan = calcParameters.InternalEndTime -
                                      calcParameters.InternalStartTime;
            }
            else
            {
                currentTime = calcParameters.OfficialStartTime;
                calculationTimeSpan = calcParameters.InternalEndTime - calcParameters.OfficialStartTime;
            }
            var timesteps = (int)(calculationTimeSpan.TotalMinutes / calcParameters.InternalStepsize.TotalMinutes);
            var currentAction = actionsForPerson[0];
            var currentActionIndex = 0;
            for (var timeIdx = 0; timeIdx < timesteps; timeIdx++)
            {
                if (currentActionIndex < actionsForPerson.Count - 1)
                {
                    if (currentTime > actionsForPerson[currentActionIndex + 1].DateTime)
                    {
                        currentActionIndex++;
                        currentAction = actionsForPerson[currentActionIndex];
                    }
                }
                else
                {
                    currentAction = actionsForPerson[actionsForPerson.Count - 1];
                }
                SingleTimestepActionEntry stae = new SingleTimestepActionEntry(
                    currentAction.HouseholdKey, timeIdx, currentTime,
                    currentAction.PersonGuid, currentAction.ActionEntryGuid);
                timestepActionEntries.Add(stae);
                currentTime += calcParameters.InternalStepsize;
            }

            return timestepActionEntries;
        }

        [NotNull]
        private Dictionary<CalcPersonDto, List<ActionEntry>> ReadActivities([NotNull] HouseholdKey householdKey)
        {
            List<ActionEntry> actionEntries = _repository.ReadActionEntries(householdKey);
            var persons = _repository.GetPersons(householdKey);
            Dictionary<StrGuid, CalcPersonDto> personsByGuid = new Dictionary<StrGuid, CalcPersonDto>();
            foreach (CalcPersonDto person in persons)
            {
                personsByGuid.Add(person.Guid, person);
            }

            Dictionary<CalcPersonDto, List<ActionEntry>> dict = new Dictionary<CalcPersonDto, List<ActionEntry>>();
            foreach (ActionEntry entry in actionEntries)
            {
                var person = personsByGuid[entry.PersonGuid];
                if (!dict.ContainsKey(person))
                {
                    dict.Add(person, new List<ActionEntry>());
                }

                dict[person].Add(entry);
            }

            return dict;
        }
    }
}