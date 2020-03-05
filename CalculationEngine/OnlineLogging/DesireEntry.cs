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

using System.Text;
using CalculationEngine.HouseholdElements;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.OnlineLogging {
    public class DesireEntry {
        [NotNull]
        public CalcPerson CPerson { get; }

        [NotNull]
        private readonly decimal[] _desirevalues;
        [NotNull]
        private readonly DesiresLogFile _lf;
        [NotNull]
        private readonly CalcParameters _calcParameters;
        [NotNull] private TimeStep _timestep;

        public DesireEntry([NotNull] CalcPerson pPerson, [NotNull] TimeStep pTimestep,
                           [NotNull] CalcPersonDesires pDesires, [NotNull]DesiresLogFile dlf,
                           [NotNull] CalcParameters calcParameters ) {
            CPerson = pPerson;
            _timestep = pTimestep;
            _lf = dlf;
            _calcParameters = calcParameters;
            _desirevalues = new decimal[dlf.DesireColumn.Count];
            foreach (var calcDesire in pDesires.Desires.Values) {
                _desirevalues[dlf.DesireColumn[calcDesire.Name]] = calcDesire.Value;
            }
            _dsc = new DateStampCreator(calcParameters);
        }

        [NotNull]
        private readonly DateStampCreator _dsc;

        [NotNull]
        public string PersonName => CPerson.Name;

        [NotNull]
        [UsedImplicitly]
        public TimeStep Timestep {
            get => _timestep;
            set => _timestep = value;
        }

        [NotNull]
        public string GenerateHeader() {
            var desirestring = string.Empty;
            foreach (var keyValuePair in _lf.DesireColumn) {
                desirestring = desirestring + _calcParameters.CSVCharacter + keyValuePair.Key;
            }
            return desirestring;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            _dsc.GenerateDateStampForTimestep(_timestep, sb);
                sb.Append(CPerson.Name);
            sb.Append(_calcParameters.CSVCharacter);

            foreach (var desirevalue in _desirevalues) {
                sb.Append(desirevalue.ToString("0.0000", Config.CultureInfo));
                sb.Append(_calcParameters.CSVCharacter);
            }
            return sb.ToString();
        }
    }
}