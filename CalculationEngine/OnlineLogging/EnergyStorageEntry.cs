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

using System.Text;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalculationEngine.OnlineLogging {
    public class EnergyStorageEntry {
        [NotNull]
        private readonly EnergyStorageLogfile _lf;
        [NotNull]
        private readonly string _csvChar;
        [NotNull]
        private readonly double[] _storageValues;
        [NotNull]
        private readonly DateStampCreator _dsc;

        public EnergyStorageEntry([NotNull] TimeStep pTimestep, [NotNull] EnergyStorageLogfile dlf, [NotNull] string csvChar, [NotNull] DateStampCreator dsc) {
            Timestep = pTimestep;
            _lf = dlf;
            _csvChar = csvChar;
            _storageValues = new double[dlf.EnergyStorageColumnDict.Count];
            _dsc = dsc;
        }

        [NotNull]
        public TimeStep Timestep { get; set; }

        public void AddValue([NotNull] string name, double value) {
            var column = _lf.EnergyStorageColumnDict[name];
            _storageValues[column] = value;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            _dsc.GenerateDateStampForTimestep(Timestep, sb);

            foreach (var desirevalue in _storageValues) {
                sb.Append(desirevalue.ToString("0.0000", Config.CultureInfo));
                sb.Append(_csvChar);
            }
            return sb.ToString();
        }
    }
}