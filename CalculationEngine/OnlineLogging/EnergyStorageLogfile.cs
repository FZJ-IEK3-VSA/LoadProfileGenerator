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

using System.Collections.Generic;
using System.IO;
using System.Text;
using Automation.ResultFiles;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalculationEngine.OnlineLogging {
    public class EnergyStorageLogfile : LogfileBase {
        [NotNull] private readonly CalcParameters _calcParameters;

        [NotNull] private readonly Dictionary<string, int> _energyStorageColumns = new Dictionary<string, int>();

        [NotNull] private readonly Dictionary<string, EnergyStorageHeaderEntry> _energyStorageHeaders =
            new Dictionary<string, EnergyStorageHeaderEntry>();

        [NotNull] private readonly FileFactoryAndTracker _fft;

        [CanBeNull] private EnergyStorageEntry _currentEntry;

        [NotNull] private TimeStep _currentTimeStep = new TimeStep(-1,0,false);

        [CanBeNull] private StreamWriter _energyStoragesSw;

        private bool _writeHeader;

        [NotNull] private readonly DateStampCreator _dsc;

        [UsedImplicitly]
        public EnergyStorageLogfile([NotNull] CalcParameters calcParameters, [NotNull] FileFactoryAndTracker fft)
        {
            _fft = fft;
            _calcParameters = calcParameters;
            _dsc = new DateStampCreator(calcParameters);
            _writeHeader = true;
        }
        /*
        public EnergyStorageLogfile([NotNull] FileFactoryAndTracker fft, bool displayNegativeTime,
                                    CalcParameters calcParameters)
        {
            _fft = fft;
            _displayNegativeTime = displayNegativeTime;
            _writeHeader = true;
            _calcParameters = calcParameters;
        }*/

        [NotNull]
        public Dictionary<string, int> EnergyStorageColumnDict => _energyStorageColumns;

        public void Close()
        {
            _energyStoragesSw?.Close();
        }

        public void RegisterStorage([NotNull] string name, [NotNull] EnergyStorageHeaderEntry eslf)
        {
            var i = EnergyStorageColumnDict.Count;
            if (!EnergyStorageColumnDict.ContainsKey(name)) {
                _energyStorageColumns.Add(name, i);
                _energyStorageHeaders.Add(name, eslf);
            }
        }

        public void SetValue([NotNull] string name, double value, [NotNull] TimeStep timestep, [NotNull] HouseholdKey householdKey,
                             [NotNull] CalcLoadTypeDto loadType)
        {
            if (timestep != _currentTimeStep) {
                if (_currentEntry != null) {
                    WriteEntry(_currentEntry, householdKey);
                }

                _currentEntry = new EnergyStorageEntry(timestep, this, _calcParameters.CSVCharacter, _dsc);
                _currentTimeStep = timestep;
            }

            if (_currentEntry == null) {
                throw new LPGException("Currententry was null");
            }

            _currentEntry.AddValue(name, value * loadType.ConversionFactor);
        }

        [NotNull]
        private string GetHeader()
        {
            var s = new StringBuilder();
            foreach (var esheader in _energyStorageHeaders.Values) {
                s.Append(_calcParameters.CSVCharacter).Append(esheader.TotalHeader);
            }

            return s.ToString();
        }

        private void WriteEntry([NotNull] EnergyStorageEntry e, [NotNull] HouseholdKey householdKey)
        {
            if (_writeHeader) {
                _energyStoragesSw = _fft.MakeFile<StreamWriter>("EnergyStorages.csv", "Energy storage values", true,
                    ResultFileID.EnergyStorages, householdKey, TargetDirectory.Results,
                    _calcParameters.InternalStepsize);
                _energyStoragesSw.Write(_dsc.GenerateDateStampHeader());
                _energyStoragesSw.WriteLine(GetHeader());
                _writeHeader = false;
            }

            if (!e.Timestep.DisplayThisStep) {
                return;
            }

            if (_energyStoragesSw == null) {
                throw new LPGException("energystoragesw was null");
            }

            _energyStoragesSw.WriteLine(e.ToString());
        }
    }
}