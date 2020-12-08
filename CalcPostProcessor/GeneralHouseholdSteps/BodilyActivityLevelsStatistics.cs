using System;
using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.Enums;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using Newtonsoft.Json;


namespace CalcPostProcessor.GeneralHouseholdSteps {

    public class BodilyActivityLevelsStatistics : HouseholdStepBase
    {
        [JetBrains.Annotations.NotNull] private readonly ICalculationProfiler _calculationProfiler;

        private readonly IFileFactoryAndTracker _fft;

        [JetBrains.Annotations.NotNull] private readonly CalcDataRepository _repository;

        public BodilyActivityLevelsStatistics([JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                              [JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                              IFileFactoryAndTracker fft) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.BodilyActivityStatistics), calculationProfiler,
            "Make Bodily Activity Level Counts",10)
        {
            _repository = repository;
            _fft = fft;
            _calculationProfiler = calculationProfiler;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            var entry = hhp.Key;
            if (entry.KeyType != HouseholdKeyType.Household)
            {
                return;
            }

            var householdKey = entry.HHKey;
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());


            Logger.Info("Starting to complete list of actions for each person at each time step for statistical analysis...");

            var activitiesPerPerson = ReadActivities(householdKey);
            //var fileIdx = 0;
            if (activitiesPerPerson.Count == 0)
            {
                throw new LPGException("There were no activities for any person in the household " + householdKey.Key);
            }

            var affs = ReadActivities(householdKey);
            var sactionEntries = _repository.ReadSingleTimestepActionEntries(householdKey);
            Dictionary<BodilyActivityLevel, List<int>> personCountByActivity = new Dictionary<BodilyActivityLevel, List<int>>();
            foreach (BodilyActivityLevel bal in Enum.GetValues(typeof(BodilyActivityLevel))) {
                personCountByActivity.Add(bal,new List<int>(new int[_repository.CalcParameters.OfficalTimesteps]));
            }
            foreach (var singleTimestepActionEntry in sactionEntries) {
                var aff = affs[singleTimestepActionEntry.ActionEntryGuid];
                personCountByActivity[aff.BodilyActivityLevel][singleTimestepActionEntry.TimeStep]++;
            }

            foreach (var pair in personCountByActivity) {
                JsonSumProfile jsp = new JsonSumProfile("Person Count for Activity Level " + pair.Key.ToString() + " " + householdKey.Key,
                    Repository.CalcParameters.InternalStepsize, Repository.CalcParameters.OfficialStartTime,
                    "Person Count for  - " + pair.Key.ToString(), "",
                    null, hhp.Key);
                foreach (var val in pair.Value)
                {
                    jsp.Values.Add(val);
                }

                var sumfile = _fft.MakeFile<StreamWriter>("BodilyActivityLevel." + pair.Key + "." + householdKey.Key + ".json",
                    "Bodily Activity Level for " + pair.Key + " in the household " + householdKey.Key,
                    true, ResultFileID.BodilyActivityJsons, householdKey,
                    TargetDirectory.Results, Repository.CalcParameters.InternalStepsize, CalcOption.BodilyActivityStatistics, null, null,
                    pair.Key.ToString());
                sumfile.Write(JsonConvert.SerializeObject(jsp, Formatting.Indented));
                sumfile.Flush();
            }

            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());

        }

        [JetBrains.Annotations.NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.ActionEntries, CalcOption.HouseholdContents, CalcOption.ActionsEachTimestep };

        [JetBrains.Annotations.NotNull]
        private Dictionary<StrGuid, ActionEntry> ReadActivities([JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            List<ActionEntry> actionEntries = _repository.ReadActionEntries(householdKey);

            Dictionary<StrGuid, ActionEntry> dict = new Dictionary<StrGuid, ActionEntry>();
            foreach (ActionEntry entry in actionEntries)
            {
                    dict.Add(entry.ActionEntryGuid, entry);
            }
            return dict;
        }

    }
}