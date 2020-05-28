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

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.HouseholdElements {
    public class CalcPersonDesires {
        private readonly CalcRepo _calcRepo;

        [NotNull]
        private static readonly Dictionary<Tuple<string, HouseholdKey>, int> _persons =
            new Dictionary<Tuple<string, HouseholdKey>, int>();
        [ItemNotNull]
        [NotNull]
        private readonly List<string> _lastAffordances = new List<string>();
        [NotNull]
        private readonly DateStampCreator _dsc;
        [CanBeNull] private StreamWriter _sw;

        public CalcPersonDesires(CalcRepo calcRepo) {
            _calcRepo = calcRepo;
            Desires = new Dictionary<int, CalcDesire>();
            _persons.Clear();
            _dsc = new DateStampCreator(_calcRepo.CalcParameters);
        }

        [NotNull]
        public Dictionary<int, CalcDesire> Desires { get; }

        public void AddDesires([NotNull] CalcDesire cd) {
            if (!Desires.ContainsKey(cd.DesireID)) {
                Desires.Add(cd.DesireID, cd);
            }
        }

        public void ApplyAffordanceEffect([NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues, bool randomEffect,
            [NotNull] string affordance) {
            while (_lastAffordances.Count > 10) {
                _lastAffordances.RemoveAt(0);
            }
            _lastAffordances.Add(affordance);
            foreach (var satisfactionvalue in satisfactionvalues) {
                if (Desires.ContainsKey(satisfactionvalue.DesireID)) {
                    Desires[satisfactionvalue.DesireID].Value += satisfactionvalue.Value;
                    if (Desires[satisfactionvalue.DesireID].Value > 1) {
                        Desires[satisfactionvalue.DesireID].Value = 1;
                    }
                }
            }
            if (randomEffect) {
                var usedDesires = new Dictionary<CalcDesire, bool>();
                foreach (var satisfactionvalue in satisfactionvalues) {
                    usedDesires.Add(satisfactionvalue, true);
                }
                var desiresArray = new CalcDesire[Desires.Count];
                Desires.Values.CopyTo(desiresArray, 0);
                var affectedCount = _calcRepo.Rnd.Next(Desires.Count - usedDesires.Count + 1);
                for (var i = 0; i < affectedCount; i++) {
                    CalcDesire d = null;
                    var loopcount = 0;
                    while (d == null) {
                        var selectedkey = _calcRepo.Rnd.Next(Desires.Count);
                        d = desiresArray[selectedkey];
                        loopcount++;
                        if (usedDesires.ContainsKey(d)) {
                            d = null;
                        }
                        if (loopcount > 500) {
                            throw new LPGException("Random result failed after 500 tries...");
                        }
                    }
                    d.Value += (decimal)_calcRepo.Rnd.NextDouble();
                    if (d.Value > 1) {
                        d.Value = 1;
                    }
                }
            }
        }

        public void ApplyDecay([NotNull] TimeStep timestep) {
            foreach (var calcDesire in Desires.Values) {
                calcDesire.ApplyDecay(timestep);
            }
        }

        public decimal CalcEffect([NotNull][ItemNotNull] IEnumerable<CalcDesire> satisfactionvalues, [NotNull] out string thoughtstring,
            [NotNull] string affordanceName) {
            // calc decay
            foreach (var calcDesire in Desires.Values) {
                calcDesire.TempValue = calcDesire.Value;
            }
            decimal modifier = 1;
            if (_lastAffordances.Contains(affordanceName)) {
                var index = _lastAffordances.IndexOf(affordanceName);
                for (var i = index; i >= 0; i--) {
                    modifier *= 0.9m;
                }
            }
            // add value
            foreach (var satisfactionvalue in satisfactionvalues) {
                if (Desires.ContainsKey(satisfactionvalue.DesireID)) {
                    var desire = Desires[satisfactionvalue.DesireID];
                    if (desire.TempValue + satisfactionvalue.Value > 1) {
                        desire.TempValue += satisfactionvalue.Value / modifier;
                    }
                    else {
                        desire.TempValue += satisfactionvalue.Value * modifier;
                    }
                }
            }
            // get results
            return CalcTotalDeviation(out thoughtstring);
        }

        private decimal CalcTotalDeviation([CanBeNull] out string thoughtstring) {
            decimal totalDeviation = 0;
            StringBuilder sb = null;
            var makeThoughts = _calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile);
            if (makeThoughts) {
                sb = new StringBuilder(_calcRepo.CalcParameters.CSVCharacter);
            }

            foreach (var calcDesire in Desires.Values) {
                if (calcDesire.TempValue < calcDesire.Threshold || calcDesire.TempValue > 1) {
                    var deviation = (1 - calcDesire.TempValue) * 100;
                    var desirevalue = deviation * deviation * calcDesire.Weight;
                    totalDeviation += desirevalue;
                    if (makeThoughts) {
                        sb.Append(calcDesire.Name);
                        sb.Append(_calcRepo.CalcParameters.CSVCharacter).Append("'");
                        sb.Append(deviation.ToString("0#.#", Config.CultureInfo));
                        sb.Append("*");
                        sb.Append(deviation.ToString("0#.#", Config.CultureInfo));
                        sb.Append("*");
                        sb.Append(calcDesire.Weight.ToString("0#.#", Config.CultureInfo));
                        sb.Append(_calcRepo.CalcParameters.CSVCharacter);
                        sb.Append(desirevalue.ToString("0#.#", Config.CultureInfo));
                        sb.Append(_calcRepo.CalcParameters.CSVCharacter).Append(" ");
                    }
                }
            }
            if (makeThoughts) {
                thoughtstring = sb.ToString();
            }
            else {
                thoughtstring = null;
            }
            return totalDeviation;
        }

        public void CheckForCriticalThreshold([NotNull] CalcPerson person, [NotNull] TimeStep time, [NotNull] FileFactoryAndTracker fft,
                                              [NotNull] HouseholdKey householdKey) {
            if ( time.ExternalStep < 0 &&
                !time.ShowSettling) {
                return;
            }

            var builder = new StringBuilder();
            foreach (var calcDesire in Desires) {
                if (calcDesire.Value.CriticalThreshold > 0) {
                    if (calcDesire.Value.Value < calcDesire.Value.CriticalThreshold) {
                        builder.Append("1");
                    }
                    else {
                        builder.Append("0");
                    }
                    builder.Append(_calcRepo.CalcParameters.CSVCharacter);
                }
            }
            if (builder.Length > 0) {
                var sb = new StringBuilder();
                _dsc.GenerateDateStampForTimestep(time, sb);

                if (_sw == null) {
                    var personNumber = _persons.Count;
                    _persons.Add(new Tuple<string, HouseholdKey>(person.Name, householdKey), personNumber);
                    _sw = fft.MakeFile<StreamWriter>(
                        "CriticalThresholdViolations." + householdKey + "." + person + ".csv",
                        "Lists the critical threshold violations for " + person, true,
                        ResultFileID.CriticalThresholdViolations, householdKey,
                        TargetDirectory.Debugging, _calcRepo.CalcParameters.InternalStepsize, CalcOption.CriticalViolations, null,person.MakePersonInformation());
                    var header = _dsc.GenerateDateStampHeader();
                    foreach (var calcDesire in Desires) {
                        if (calcDesire.Value.CriticalThreshold > 0) {
#pragma warning disable CC0039 // Don't concatenate strings in loops
                            header += calcDesire.Value.Name;
                            header += _calcRepo.CalcParameters.CSVCharacter;
#pragma warning restore CC0039 // Don't concatenate strings in loops
                        }
                    }
                    if(_sw==null) {
                        throw new LPGException("SW was null");
                    }
                    _sw.WriteLine(header);
                }
                sb.Append(builder);
                _sw.WriteLine(sb);
            }
        }

        public void CopyOtherDesires([NotNull] CalcPersonDesires otherdesires) {
            foreach (var calcDesire in Desires.Values) {
                if (calcDesire.DecayTime < 100) {
                    calcDesire.Value = 1;
                }
                if (otherdesires.Desires.ContainsKey(calcDesire.DesireID)) {
                    var tmpdes = otherdesires.Desires[calcDesire.DesireID];
                    calcDesire.Value = tmpdes.Value;
                }
            }
            if (_sw == null && otherdesires._sw != null) {
                _sw = otherdesires._sw;
            }
        }

        public bool HasAtLeastOneDesireBelowThreshold([NotNull] ICalcAffordanceBase aff) {
            foreach (var desire in aff.Satisfactionvalues) {
                if (Desires.ContainsKey(desire.DesireID)) {
                    if (Desires[desire.DesireID].Value < Desires[desire.DesireID].Threshold) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}