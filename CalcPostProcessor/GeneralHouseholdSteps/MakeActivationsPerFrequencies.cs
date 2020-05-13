//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace CalcPostProcessor.GeneralHouseholdSteps {
    internal class MakeActivationsPerFrequencies : HouseholdStepBase {
        [NotNull] private readonly CalcParameters _calcParameters;

        [NotNull] private readonly IInputDataLogger _dataLogger;

        [NotNull] private readonly FileFactoryAndTracker _fft;

        public MakeActivationsPerFrequencies([NotNull] FileFactoryAndTracker fft,
                                             [NotNull] CalcDataRepository repository,
                                             [NotNull] ICalculationProfiler profiler,
                                             [NotNull] IInputDataLogger logger) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.ActivationFrequencies),
            profiler,
            "Activation Frequency Analysis",0)
        {
            _dataLogger = logger;
            _calcParameters = Repository.CalcParameters;
            _fft = fft;
        }

        [NotNull]
        public static Dictionary<string, Dictionary<string, Dictionary<string, int>>> AffordanceTaggingSetByPersonByTag { get; private set; } =
            new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

        [NotNull]
        public static Dictionary<string, Dictionary<string, Dictionary<string, int>>> AffordanceTaggingSetByPersonByTagExecutioncount {
            get;
            private set;
        } = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

        protected override void PerformActualStep([NotNull] IStepParameters parameters)
        {
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            var entry = hhp.Key;
            if (entry.KeyType != HouseholdKeyType.Household) {
                return;
            }

            BuildActivitiesPerMinute(entry.HouseholdKey, Repository.AffordanceTaggingSets, Repository.GetPersons(entry.HouseholdKey));
        }

        private void BuildActivitiesPerMinute([NotNull] HouseholdKey householdKey,
                                              [NotNull] [ItemNotNull] List<CalcAffordanceTaggingSetDto> taggingSets,
                                              [NotNull] [ItemNotNull] List<CalcPersonDto> persons)
        {
            int timesteps;

            if (_calcParameters.ShowSettlingPeriodTime) {
                var ts = _calcParameters.InternalEndTime - _calcParameters.InternalStartTime;
                timesteps = (int)ts.TotalMinutes;
            }
            else {
                var ts = _calcParameters.InternalEndTime - _calcParameters.OfficialStartTime;
                timesteps = (int)ts.TotalMinutes;
            }

            var activitiesByPerson = new Dictionary<string, List<ActionEntry>>();
            foreach (var ae in Repository.ReadActionEntries(householdKey)) {
                if (!activitiesByPerson.ContainsKey(ae.PersonName)) {
                    activitiesByPerson.Add(ae.PersonName, new List<ActionEntry>());
                }

                activitiesByPerson[ae.PersonName].Add(ae);
            }

            var categoryMinutes = new Dictionary<string, Dictionary<string, int>>();
            var frequencies = new Dictionary<string, Dictionary<string, int[]>>();
            // taggingset,Person, tag, minutes
            var affordanceTaggingSets = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            AffordanceTaggingSetByPersonByTagExecutioncount = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            foreach (var affordanceTaggingSet in taggingSets) {
                affordanceTaggingSets.Add(affordanceTaggingSet.Name, new Dictionary<string, Dictionary<string, int>>());
                AffordanceTaggingSetByPersonByTagExecutioncount.Add(affordanceTaggingSet.Name, new Dictionary<string, Dictionary<string, int>>());
            }

            foreach (var person in activitiesByPerson) {
                if (!frequencies.ContainsKey(person.Key)) {
                    frequencies.Add(person.Key, new Dictionary<string, int[]>());
                    categoryMinutes.Add(person.Key, new Dictionary<string, int>());
                }

                var times = new TimeActionTuple[timesteps];
                for (var i = 0; i < times.Length; i++) {
                    times[i] = new TimeActionTuple();
                }

                var entries = person.Value;
                SetTimeDictionary(times);
                var currentaction = entries[0];
                const int currentActionIndex = 0;
                MakeActionTimeDictionary(times, currentActionIndex, entries, currentaction);
                GenerateActivityFrequencies(times, frequencies, person);
                // verbrauchsstatistiken
                GenerateUsageStatistics(times, categoryMinutes, person);
                foreach (var affordanceTaggingSet in taggingSets) {
                    var tagMinutesDictionary = new Dictionary<string, int>();
                    CalculateTaggingSet(times, tagMinutesDictionary, affordanceTaggingSet);
                    affordanceTaggingSets[affordanceTaggingSet.Name].Add(person.Key, tagMinutesDictionary);
                    if (!AffordanceTaggingSetByPersonByTagExecutioncount[affordanceTaggingSet.Name].ContainsKey(person.Key)) {
                        AffordanceTaggingSetByPersonByTagExecutioncount[affordanceTaggingSet.Name].Add(person.Key, new Dictionary<string, int>());
                    }

                    var activityDict = AffordanceTaggingSetByPersonByTagExecutioncount[affordanceTaggingSet.Name][person.Key];
                    foreach (var entry in person.Value) {
                        if (!activityDict.ContainsKey(entry.AffordanceName)) {
                            activityDict.Add(entry.AffordanceName, 0);
                        }

                        activityDict[entry.AffordanceName]++;
                    }
                }
            }

            AffordanceTaggingSetByPersonByTag = affordanceTaggingSets;
            MakeActivityTaggingSetFile(householdKey, affordanceTaggingSets, taggingSets, persons);
            MakeActivityPercentageFile(householdKey, categoryMinutes);
            MakeActivityFrequenciesFile(householdKey, frequencies);
            MakeAffordanceTimeUseFile(householdKey, frequencies, taggingSets);
        }

        private static void CalculateTaggingSet([NotNull] [ItemNotNull] TimeActionTuple[] times,
                                                [NotNull] Dictionary<string, int> tagMinutesDictionary,
                                                [NotNull] CalcAffordanceTaggingSetDto taggingSet)
        {
            foreach (var time in times) {
                if (time.ActionEntry == null) {
                    throw new LPGException("ae was null");
                }

                var action = time.ActionEntry.AffordanceName;
                /*if(action == null) {
                    throw new LPGException("Action was null");
                }*/
                var dstTag = taggingSet.AffordanceToTagDict[action];
                if (!tagMinutesDictionary.ContainsKey(dstTag)) {
                    tagMinutesDictionary.Add(dstTag, 0);
                }

                tagMinutesDictionary[dstTag]++;
            }
        }

        private static void GenerateActivityFrequencies([NotNull] [ItemNotNull] TimeActionTuple[] times,
                                                        [NotNull] Dictionary<string, Dictionary<string, int[]>> frequencies,
                                                        KeyValuePair<string, List<ActionEntry>> person)
        {
            for (var i = 0; i < times.Length; i++) {
                if (times[i].ActionEntry == null) {
                    throw new LPGException("AE was null");
                }

                /*if (times[i].ActionEntry.AffordanceName == null) {
                    throw new LPGException("Action was null");
                }*/
                if (!frequencies[person.Key].ContainsKey(times[i].ActionEntry.AffordanceName)) {
                    frequencies[person.Key].Add(times[i].ActionEntry.AffordanceName, new int[1440]);
                }

                var sp = new TimeSpan(times[i].DateTime.Hour, times[i].DateTime.Minute, 0);
                var destinationMinutes = (int)sp.TotalMinutes;
                var tmpdict = frequencies[person.Key];
                tmpdict[times[i].ActionEntry.AffordanceName][destinationMinutes]++;
            }
        }

        private static void GenerateUsageStatistics([NotNull] [ItemNotNull] TimeActionTuple[] times,
                                                    [NotNull] Dictionary<string, Dictionary<string, int>> categoryMinutes,
                                                    KeyValuePair<string, List<ActionEntry>> person)
        {
            for (var i = 0; i < times.Length; i++) {
                if (times[i].ActionEntry == null) {
                    throw new LPGException("ae was null");
                }

                /*if (times[i].ActionEntry.Category == null) {
                    throw new LPGException("AffCategory was null");
                }*/
                if (!categoryMinutes[person.Key].ContainsKey(times[i].ActionEntry.Category)) {
                    categoryMinutes[person.Key].Add(times[i].ActionEntry.Category, 0);
                }

                var tmpdict = categoryMinutes[person.Key];
                tmpdict[times[i].ActionEntry.Category]++;
            }
        }

        private static void MakeActionTimeDictionary([NotNull] [ItemNotNull] TimeActionTuple[] times,
                                                     int currentActionIndex,
                                                     [NotNull] [ItemNotNull] List<ActionEntry> entries,
                                                     [NotNull] ActionEntry action)
        {
            var currentaction = action;
            for (var i = 0; i < times.Length; i++) {
                if (currentActionIndex < entries.Count - 1) {
                    if (times[i].DateTime > entries[currentActionIndex + 1].DateTime) {
                        currentActionIndex++;
                        currentaction = entries[currentActionIndex];
                    }
                }
                else {
                    currentaction = entries[entries.Count - 1];
                }

                times[i].ActionEntry = currentaction;
            }
        }

        private void MakeActivityFrequenciesFile([NotNull] HouseholdKey householdKey,
                                                 [NotNull] Dictionary<string, Dictionary<string, int[]>> frequencies)
        {
            if (_calcParameters.IsSet(CalcOption.ActivationFrequencies)) {
                using var sw = _fft.MakeFile<StreamWriter>("ActivityFrequenciesPerMinute." + householdKey + ".csv",
                    "Activity frequencies ",
                    true,
                    ResultFileID.ActivationFrequencies,
                    householdKey,
                    TargetDirectory.Reports,
                    _calcParameters.InternalStepsize);
                foreach (var person in frequencies) {
                    sw.WriteLine(person.Key);

                    var s = new StringBuilder();
                    s.Append("Minute").Append(_calcParameters.CSVCharacter);
                    for (var i = 0; i < 1440; i++) {
                        s.Append(i).Append(_calcParameters.CSVCharacter);
                    }

                    sw.WriteLine(s);
                    var sb = new StringBuilder();
                    foreach (var device in person.Value) {
                        sb.Clear();
                        sb.Append(device.Key);
                        for (var i = 0; i < device.Value.Length; i++) {
                            sb.Append(_calcParameters.CSVCharacter);
                            sb.Append(device.Value[i].ToString(CultureInfo.InvariantCulture));
                        }

                        sw.WriteLine(sb);
                    }
                }
            }
        }

        private void MakeActivityPercentageFile([NotNull] HouseholdKey householdKey,
                                                [NotNull] Dictionary<string, Dictionary<string, int>> categoryMinutes)
        {
            if (_calcParameters.IsSet(CalcOption.ActivationFrequencies)) {
                using (var activityPercentage = _fft.MakeFile<StreamWriter>("ActivityPercentage." + householdKey + ".csv",
                    "Time used by each Person per affordance category ",
                    true,
                    ResultFileID.ActivityPercentages,
                    householdKey,
                    TargetDirectory.Reports,
                    _calcParameters.InternalStepsize)) {
                    foreach (var person in categoryMinutes) {
                        activityPercentage.WriteLine(person.Key);
                        activityPercentage.WriteLine("Activity" + _calcParameters.CSVCharacter + "Time Used [" +
                                                     _calcParameters.InternalStepsize.TotalSeconds + " seconds]" + _calcParameters.CSVCharacter +
                                                     "Percentage");
                        var totalminutes = 0;
                        foreach (var defcategory in person.Value) {
                            totalminutes += defcategory.Value;
                        }

                        foreach (var defcategory in person.Value) {
                            activityPercentage.WriteLine(defcategory.Key + _calcParameters.CSVCharacter + defcategory.Value +
                                                         _calcParameters.CSVCharacter +
                                                         (defcategory.Value / (double)totalminutes).ToString("0.00", Config.CultureInfo));
                        }
                    }
                }
            }
        }

        private void MakeActivityTaggingSetFile([NotNull] HouseholdKey householdKey,
                                                [NotNull] Dictionary<string, Dictionary<string, Dictionary<string, int>>> affordanceTaggingSets,
                                                [NotNull] [ItemNotNull] List<CalcAffordanceTaggingSetDto> taggingSets,
                                                [NotNull] [ItemNotNull] List<CalcPersonDto> persons)
        {
            // taggingset,Person, tag, minutes

            foreach (var taggingSetDict in affordanceTaggingSets) {
                var taggingSetName = taggingSetDict.Key;
                var ts = taggingSets.FirstOrDefault(x => x.Name == taggingSetName);
                if (ts == null) {
                    throw new LPGException("Missing calc affordance tagging set.");
                }

                var personDictionary = taggingSetDict.Value;
                var activityPercentage = _fft.MakeFile<StreamWriter>("AffordanceTaggingSet." + taggingSetName + "." + householdKey + ".csv",
                    "Time used per tag in the tagging set " + taggingSetName,
                    true,
                    ResultFileID.AffordanceTaggingSetFiles,
                    householdKey,
                    TargetDirectory.Reports,
                    _calcParameters.InternalStepsize,
                    null,
                    null,
                    taggingSetName);
                var header = new StringBuilder();
                header.Append("Tag + Time Used [").Append(_calcParameters.InternalStepsize.TotalSeconds).Append(" seconds]")
                    .Append(_calcParameters.CSVCharacter);
                if (ts.HasReferenceEntries) {
                    foreach (var tagsByPerson in personDictionary) {
                        header.Append(tagsByPerson.Key).Append(_calcParameters.CSVCharacter).Append("Reference").Append(_calcParameters.CSVCharacter);
                    }
                }
                else {
                    foreach (var tagsByPerson in personDictionary) {
                        header.Append(tagsByPerson.Key).Append(_calcParameters.CSVCharacter);
                    }
                }

                activityPercentage.WriteLine(header);
                // make tag list
                var tags = new List<string>();
                foreach (var tagsByPerson in personDictionary) {
                    foreach (var pair in tagsByPerson.Value) {
                        if (!tags.Contains(pair.Key)) {
                            tags.Add(pair.Key);
                        }
                    }
                }

                if (ts.HasReferenceEntries) {
                    foreach (var reftag in ts.MakeReferenceTags()) {
                        if (!tags.Contains(reftag)) {
                            tags.Add(reftag);
                        }
                    }
                }

                // build lines
                foreach (var tag in tags) {
                    var line = new StringBuilder();
                    line.Append(tag).Append(_calcParameters.CSVCharacter);
                    foreach (var tagsByPerson in personDictionary) {
                        var value = 0;
                        if (tagsByPerson.Value.ContainsKey(tag)) {
                            value = tagsByPerson.Value[tag];
                        }

                        if (ts.HasReferenceEntries) {
                            var person = persons.First(x => x.Name == tagsByPerson.Key);

                            var refValue = ts.LookupReferenceValue(tag, person.Gender, person.Age);
                            var referenceSteps = _calcParameters.OfficalTimesteps * refValue;
                            line.Append(value).Append(_calcParameters.CSVCharacter).Append(referenceSteps).Append(_calcParameters.CSVCharacter);
                        }
                        else {
                            line.Append(value).Append(_calcParameters.CSVCharacter);
                        }
                    }

                    activityPercentage.WriteLine(line);
                }

                activityPercentage.Close();
            }
        }

        private void MakeAffordanceTimeUseFile([NotNull] HouseholdKey householdKey,
                                               [NotNull] Dictionary<string, Dictionary<string, int[]>> frequencies,
                                               [NotNull] [ItemNotNull] List<CalcAffordanceTaggingSetDto> taggingSets)
        {
            //CSV File
            using (var sw = _fft.MakeFile<StreamWriter>("AffordanceTimeUse." + householdKey + ".csv",
                "Time used by each Affordance ",
                true,
                ResultFileID.AffordanceTimeUse,
                householdKey,
                TargetDirectory.Reports,
                _calcParameters.InternalStepsize)) {
                var coma = _calcParameters.CSVCharacter;
                foreach (var person in frequencies) {
                    sw.WriteLine("------");
                    var header = new StringBuilder();
                    header.Append(person.Key).Append(coma).Append("Time").Append(coma);
                    for (var i = 0; i < taggingSets.Count; i++) {
                        header.Append(taggingSets[i].Name).Append(coma);
                    }

                    sw.WriteLine(header);
                    var sb = new StringBuilder();
                    foreach (var affordance in person.Value) {
                        sb.Clear();
                        sb.Append(affordance.Key);
                        sb.Append(_calcParameters.CSVCharacter);
                        double sum = 0;
                        for (var i = 0; i < affordance.Value.Length; i++) {
                            sum += affordance.Value[i];
                        }

                        sb.Append(sum);
                        for (var i = 0; i < taggingSets.Count; i++) {
                            sb.Append(coma).Append(taggingSets[i].AffordanceToTagDict[affordance.Key]);
                        }

                        sw.WriteLine(sb);
                    }
                }
            }

            /*//JSON File
            using (
                var sw = _fft.MakeFile<StreamWriter>("AffordanceTimeUse." + householdKey + ".json",
                    "Time used by each Affordance ", false, ResultFileID.AffordanceTimeUseJson, householdKey,
                    TargetDirectory.Reports, _calcParameters.InternalStepsize)) {*/
            List<PersonAffordanceInformation> atuisList = new List<PersonAffordanceInformation>();
            foreach (var person in frequencies) {
                var atui = new PersonAffordanceInformation(person.Key, householdKey);
                atuisList.Add(atui);
                foreach (var affordance in person.Value) {
                    var ai = new AffordanceInformation(affordance.Key, affordance.Value.Sum());
                    atui.Affordances.Add(ai);
                    for (var i = 0; i < taggingSets.Count; i++) {
                        ai.AffordanceTags.Add(taggingSets[i].Name, taggingSets[i].AffordanceToTagDict[affordance.Key]);
                    }
                }
            }

            _dataLogger.SaveList(atuisList.ConvertAll(x => (IHouseholdKey)x));
        }


        /*
        private  void ReadActionsFromFile(FileFactoryAndTracker fft, string householdKey,
            Dictionary<string, List<ActionEntry>> activities)
        {
            var actionsFileName = fft.GetResultFileEntry(ResultFileID.Actions, null, householdKey, null, null)
                .FullFileName;
            using (var affordanceReader = new StreamReader(actionsFileName)) {
                affordanceReader.ReadLine();
                while (!affordanceReader.EndOfStream) {
                    var line = affordanceReader.ReadLine();
                    if (line == null) {
                        throw new LPGException("Readline failed.");
                    }
                    var ase = new ActionEntry(line,_calcParameters);
                    if (ase.StrPerson == null) {
                        throw new LPGException("Person was null");
                    }
                    if (!activities.ContainsKey(ase.StrPerson)) {
                        activities.Add(ase.StrPerson, new List<ActionEntry>());
                    }
                    activities[ase.StrPerson].Add(ase);
                }
            }
        }*/

        private void SetTimeDictionary([NotNull] [ItemNotNull] TimeActionTuple[] times)
        {
            if (_calcParameters.ShowSettlingPeriodTime) {
                times[0].DateTime = _calcParameters.InternalStartTime;
            }
            else {
                times[0].DateTime = _calcParameters.OfficialStartTime;
            }

            for (var i = 1; i < times.Length; i++) {
                times[i].DateTime = times[i - 1].DateTime + _calcParameters.InternalStepsize;
            }
        }

        #region Nested type: TimeActionTuple

        private class TimeActionTuple {
            [CanBeNull]
            public ActionEntry ActionEntry { get; set; }

            public DateTime DateTime { get; set; }
        }

        #endregion
    }
}