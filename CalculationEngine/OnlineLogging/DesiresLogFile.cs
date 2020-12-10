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
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.OnlineLogging {
    public class DesiresLogFile : LogfileBase, IDisposable {
        [NotNull]
        private readonly Dictionary<string, StreamWriter> _desireFiles;
        [NotNull]
        private readonly CalcParameters _calcParameters;
        [NotNull]
        private readonly FileFactoryAndTracker _fft;
        [NotNull]
        private readonly Dictionary<string, bool> _writeDesiresHeader;

        public DesiresLogFile([NotNull] FileFactoryAndTracker fft, [NotNull] CalcParameters calcParameters)
        {
            _fft = fft;
            _calcParameters = calcParameters;
            _desireFiles = new Dictionary<string, StreamWriter>();
            _writeDesiresHeader = new Dictionary<string, bool>();
            _dsc = new DateStampCreator(calcParameters);
        }

        [NotNull]
        private readonly DateStampCreator _dsc;

        [NotNull]
        public Dictionary<string, int> DesireColumn { get; } = new Dictionary<string, int>();

        public void Dispose()
        {
            foreach (var writer in _desireFiles.Values) {
                writer?.Close();
            }
        }

        [NotNull]
        private static string GetKey([NotNull] DesireEntry e, [NotNull] HouseholdKey householdKey) => householdKey + "###" + e.PersonName;

        public void RegisterDesires([NotNull][ItemNotNull] IEnumerable<CalcDesire> desires)
        {
            var i = DesireColumn.Count;
            foreach (var calcDesire in desires) {
                if (!DesireColumn.ContainsKey(calcDesire.Name)) {
                    DesireColumn.Add(calcDesire.Name, i);
                    i++;
                }
            }
        }

        public void WriteEntry([NotNull] DesireEntry entry, [NotNull] HouseholdKey householdKey)
        {
            if (!_writeDesiresHeader.ContainsKey(GetKey(entry, householdKey))) {
                _desireFiles.Add(GetKey(entry, householdKey),
                    _fft.MakeFile<StreamWriter>("Desires." + householdKey + "." + entry.PersonName + ".csv",
                        "Desire values for " + entry.PersonName + " for household #" + householdKey, true,
                        ResultFileID.DesireFiles, householdKey, TargetDirectory.Reports,
                        _calcParameters.InternalStepsize,CalcOption.DesiresLogfile, null, entry.CPerson.MakePersonInformation()));
                var header = _dsc.GenerateDateStampHeader();
                _desireFiles[GetKey(entry, householdKey)].Write(header + "Person");
                _desireFiles[GetKey(entry, householdKey)].WriteLine(entry.GenerateHeader());
                _writeDesiresHeader.Add(GetKey(entry, householdKey), true);
            }
            if (!entry.Timestep.DisplayThisStep) {
                return;
            }
            _desireFiles[GetKey(entry, householdKey)].WriteLine(entry.ToString());
        }
    }
}