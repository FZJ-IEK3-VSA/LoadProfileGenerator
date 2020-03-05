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
using Automation.ResultFiles;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.OnlineLogging {
    public interface IThoughtsLogFile {
        void Close();
        void WriteEntry([NotNull] ThoughtEntry entry, [NotNull] HouseholdKey householdKey);
    }

    public class ThoughtsLogFile : LogfileBase, IThoughtsLogFile {
        [NotNull]
        private readonly FileFactoryAndTracker _fft;
        [NotNull]
        private readonly CalcParameters _calcParameters;
        [NotNull]
        private readonly Dictionary<Tuple<HouseholdKey, string>, StreamWriter> _thoughtsFiles =
            new Dictionary<Tuple<HouseholdKey, string>, StreamWriter>();

        public ThoughtsLogFile([NotNull] FileFactoryAndTracker fft, [NotNull] CalcParameters calcParameters)
        {
            _fft = fft;
            _calcParameters = calcParameters;
        }

        public void Close() {
            foreach (var thoughtsFile in _thoughtsFiles) {
                thoughtsFile.Value.Close();
            }
        }

        public void WriteEntry([NotNull] ThoughtEntry entry, [NotNull] HouseholdKey householdKey) {
            if (entry.Timestep.ExternalStep < 0 && !_calcParameters.ShowSettlingPeriodTime) {
                return;
            }
            if (entry.Person == null) {
                throw new DataIntegrityException("Empty Person name found in thoughts log file!");
            }
            var name = entry.Person.Name;
            var filekey = new Tuple<HouseholdKey, string>(householdKey, name);
            if (!_thoughtsFiles.ContainsKey(filekey)) {
                MakeNewFile(entry.Person.MakePersonInformation(), householdKey);
            }
            var line = new StringBuilder();
            line.Append(entry.Timestep);
            line.Append(_calcParameters.CSVCharacter);
            DateStampCreator dsc = new DateStampCreator(_calcParameters);
            line.Append(dsc.MakeDateStringFromTimeStep(entry.Timestep, out string weekday));
            line.Append(_calcParameters.CSVCharacter);
            line.Append(weekday).Append(_calcParameters.CSVCharacter);
            line.Append(name).Append(_calcParameters.CSVCharacter);
            line.Append(entry.Thought);
            _thoughtsFiles[filekey].WriteLine(line);
        }

        private void MakeNewFile([NotNull] PersonInformation pi, [NotNull] HouseholdKey householdKey) {
            var thoughts =
                _fft.MakeFile<StreamWriter>("Thoughts." + householdKey + "." + pi.Name + ".csv",
                    "Thoughts by " + pi.Name, true, ResultFileID.ThoughtsPerPerson, householdKey, TargetDirectory.Reports,
                    _calcParameters.InternalStepsize,null,pi);
            thoughts.WriteLine("Timestep" + _calcParameters.CSVCharacter + "Calender time" +
                               _calcParameters.CSVCharacter + "Weekday" +
                               _calcParameters.CSVCharacter + "Person" +
                               _calcParameters.CSVCharacter + "Thought");
            _thoughtsFiles.Add(new Tuple<HouseholdKey, string>(householdKey, pi.Name), thoughts);
        }
    }
}