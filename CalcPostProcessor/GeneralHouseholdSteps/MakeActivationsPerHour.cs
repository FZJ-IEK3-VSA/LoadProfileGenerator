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
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace CalcPostProcessor.GeneralHouseholdSteps {
    internal class MakeActivationsPerHour : HouseholdStepBase {

        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public MakeActivationsPerHour([NotNull] CalcDataRepository repository,
                                      [NotNull] ICalculationProfiler profiler,
                                      [NotNull] IFileFactoryAndTracker fft) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.ActivationsPerHour),
            profiler,
            "Activiations per Hour",0)
        {
            _fft = fft;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            var entry = hhp.Key;
            if (entry.KeyType != HouseholdKeyType.Household) {
                return;
            }

            BuildActivationsPerHours(_fft, entry.HouseholdKey, Repository.AffordanceTaggingSets, Repository.ReadActionEntries(entry.HouseholdKey));
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>();

        private void BuildActivationsPerHours([NotNull] IFileFactoryAndTracker fft,
                                              [NotNull] HouseholdKey householdKey,
                                              [NotNull] [ItemNotNull] List<CalcAffordanceTaggingSetDto> taggingSets,
                                              [NotNull] [ItemNotNull] List<ActionEntry> actionEntries)
        {
            var calcParameters = Repository.CalcParameters;
            var activitiesPerHour = new Dictionary<string, Dictionary<string, int[]>>();
            var activitiesPerWeekday = new Dictionary<string, Dictionary<string, int[]>>();
            foreach (ActionEntry ase in actionEntries) {
                if (!activitiesPerHour.ContainsKey(ase.PersonName)) {
                    activitiesPerHour.Add(ase.PersonName, new Dictionary<string, int[]>());
                    activitiesPerWeekday.Add(ase.PersonName, new Dictionary<string, int[]>());
                }

                if (!activitiesPerHour[ase.PersonName].ContainsKey(ase.AffordanceName)) {
                    activitiesPerHour[ase.PersonName].Add(ase.AffordanceName, new int[24]);
                    activitiesPerWeekday[ase.PersonName].Add(ase.AffordanceName, new int[7]);
                }

                var hour = ase.DateTime.Hour;
                var weekday = (int)ase.DateTime.DayOfWeek;
                activitiesPerHour[ase.PersonName][ase.AffordanceName][hour]++;
                activitiesPerWeekday[ase.PersonName][ase.AffordanceName][weekday]++;
            }

            using (var sw = fft.MakeFile<StreamWriter>("ActivationsPerHour." + householdKey + ".csv",
                "Activations per hour",
                true,
                ResultFileID.ActivationsPerHour,
                householdKey,
                TargetDirectory.Reports,
                calcParameters.InternalStepsize, CalcOption.ActivationsPerHour)) {
                foreach (var person in activitiesPerHour) {
                    sw.WriteLine(person.Key);
                    var s = new StringBuilder();
                    s.Append("Hour").Append(calcParameters.CSVCharacter);
                    for (var i = 0; i < 24; i++) {
                        s.Append(i).Append(calcParameters.CSVCharacter);
                    }

                    sw.WriteLine(s);
                    var sb = new StringBuilder();
                    foreach (var device in person.Value) {
                        sb.Clear();
                        sb.Append(device.Key);
                        for (var i = 0; i < 24; i++) {
                            sb.Append(calcParameters.CSVCharacter);
                            sb.Append(device.Value[i].ToString(CultureInfo.InvariantCulture));
                        }

                        sw.WriteLine(sb);
                    }
                }
            }

            using (var sw = fft.MakeFile<StreamWriter>("ExecutedActionsOverviewCount." + householdKey + ".csv",
                "Overview of the executed actions",
                true,
                ResultFileID.ExecutedActionsOverview,
                householdKey,
                TargetDirectory.Reports,
                calcParameters.InternalStepsize, CalcOption.ActivationsPerHour)) {
                foreach (var person in activitiesPerHour) {
                    sw.WriteLine("-----");
                    sw.WriteLine(person.Key);
                    var header = new StringBuilder();
                    header.Append("Actions").Append(calcParameters.CSVCharacter).Append("Times of execution in the simulation period")
                        .Append(calcParameters.CSVCharacter);
                    foreach (var weekday in Enum.GetValues(typeof(DayOfWeek))) {
                        header.Append(weekday).Append(calcParameters.CSVCharacter);
                    }

                    foreach (var set in taggingSets) {
                        header.Append(set.Name).Append(calcParameters.CSVCharacter);
                    }

                    sw.WriteLine(header);
                    var alllines = new List<List<string>>();
                    foreach (var device in person.Value) {
                        var elements = new List<string> {
                            device.Key
                        };
                        var total = 0;
                        for (var i = 0; i < 24; i++) {
                            total += device.Value[i];
                        }

                        elements.Add(total.ToString(Config.CultureInfo));
                        foreach (var weekday in Enum.GetValues(typeof(DayOfWeek))) {
                            elements.Add(activitiesPerWeekday[person.Key][device.Key][(int)weekday].ToString(Config.CultureInfo));
                        }

                        foreach (var set in taggingSets) {
                            elements.Add(set.AffordanceToTagDict[device.Key]);
                        }

                        alllines.Add(elements);
                    }

                    alllines.Sort(Comparison);
                    foreach (var allline in alllines) {
                        var builder = new StringBuilder();
                        foreach (var s1 in allline) {
                            builder.Append(s1).Append(calcParameters.CSVCharacter);
                        }

                        sw.WriteLine(builder);
                    }
                }
            }
        }

        private static int Comparison([NotNull] [ItemNotNull] List<string> list, [NotNull] [ItemNotNull] List<string> list1)
        {
            var result = 0;
            if (list.Count > 9) {
                result = string.Compare(list[9], list1[9], StringComparison.Ordinal);
            }

            if (result != 0) {
                return result;
            }

            return string.Compare(list[0], list1[0], StringComparison.Ordinal);
        }
    }
}