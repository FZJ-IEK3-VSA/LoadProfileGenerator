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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Automation.ResultFiles;
using Common.CalcDto;
using JetBrains.Annotations;

namespace Common.JSON {
    public class OnlineEnergyFileRow {
        [NotNull]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        private readonly List<double> _energyEntries;
        [NotNull]
        private readonly CalcLoadTypeDto _loadType;
        [CanBeNull]
        private double? _sum;

        public OnlineEnergyFileRow([NotNull] TimeStep timestep, [NotNull] List<double> energyEntries, [NotNull] CalcLoadTypeDto loadType) {
            Timestep = timestep;
            _sum = null;
            _energyEntries = energyEntries;
            _loadType = loadType;
        }

        public OnlineEnergyFileRow([NotNull] OnlineEnergyFileRow other) {
            Timestep = other.Timestep;
            var otherEntries = new List<double>(other._energyEntries);
            _energyEntries = otherEntries;
            _loadType = other.LoadType;
            _sum = null;
        }

        [NotNull]
        public List<double> EnergyEntries => _energyEntries;

        public int EntryLengthInByte => sizeof(int) * 2 + sizeof(double) * _energyEntries.Count;

        [NotNull]
        public CalcLoadTypeDto LoadType => _loadType;

        public double SumCached {
            get {
                if (_sum != null) {
                    return (double) _sum;
                }
                double d = 0;
                for (var i = 0; i < _energyEntries.Count; i++) {
                    d += _energyEntries[i];
                }
                _sum = d;
                return d;
            }
        }

        public double SumFresh() {
                double d = 0;
                for (var i = 0; i < _energyEntries.Count; i++) {
                    d += _energyEntries[i];
                }
                _sum = d;
                return d;
        }
        public double SumFresh(int excludedIdx)
        {
            double d = 0;
            for (var i = 0; i < _energyEntries.Count; i++)
            {
                if (i == excludedIdx) {
                    continue;
                }
                d += _energyEntries[i];
            }
            _sum = d;
            return d;
        }

        [NotNull]
        public TimeStep Timestep { get; }

        public void AddValues([NotNull] OnlineEnergyFileRow efr) {
            _sum = null;
            if (efr._energyEntries.Count != _energyEntries.Count) {
                throw new LPGException("Row lengths are inconsistent");
            }
            for (var i = 0; i < _energyEntries.Count; i++) {
                _energyEntries[i] += efr._energyEntries[i];
            }
        }

        [NotNull]
        public StringBuilder GetEnergyEntriesAsString(bool useUnitConverter, [NotNull] CalcLoadTypeDto dstLoadType,
            [CanBeNull] List<int> usedColumns, [NotNull] string csvChar) {
            var sb = new StringBuilder();
            for (var i = 0; i < _energyEntries.Count; i++) {
                if (usedColumns == null || usedColumns.Count == 0 || usedColumns.Contains(i)) {
                    var d = _energyEntries[i];
                    if (useUnitConverter) {
                        d *= dstLoadType.ConversionFactor;
                    }
                    sb.Append(d.ToString(Config.CultureInfo));
                    sb.Append(csvChar);
                }
            }
            if (sb[sb.Length-1] == csvChar[0]) {
                sb.Length--;
            }
            return sb;
        }

        public double GetSumForCertainCols([NotNull] List<int> usedColumns) {
            double colSum = 0;
            if (usedColumns.Count == 0) {
                for (var i = 0; i < _energyEntries.Count; i++) {
                    colSum += _energyEntries[i];
                }
                return colSum;
            }
            foreach (var i in usedColumns) {
                colSum += _energyEntries[i];
            }
            return colSum;
        }

        public double GetValueForSingleCols(int column)
        {
            return _energyEntries[column];
        }

        [NotNull]
        public static OnlineEnergyFileRow Read([NotNull] BinaryReader br, [NotNull] CalcLoadTypeDto lt,
                                               [NotNull] CalcParameters parameters) {
            if (parameters == null) {
                throw new ArgumentNullException(nameof(parameters));
            }

            var timestep = new TimeStep( br.ReadInt32(),parameters);
            var length = br.ReadInt32();
            var valuearr = new List<double>(new double[length]);
            for (var i = 0; i < length; i++) {
                valuearr[i] = br.ReadDouble();
            }
            var efr = new OnlineEnergyFileRow(timestep, valuearr, lt);
            return efr;
        }

        public void Save([NotNull] BinaryWriter bw) {
            bw.Write(Timestep.InternalStep);
            bw.Write(_energyEntries.Count);
            for (var i = 0; i < _energyEntries.Count; i++) {
                bw.Write(_energyEntries[i]);
            }
        }

        public void SaveSum([NotNull] BinaryWriter bw) {
            bw.Write(SumFresh());
        }

        [NotNull]
        public override string ToString() => "Timestep: " + Timestep + " Entries:" + _energyEntries.Count + " Sum:" +
                                             SumFresh() + " LoadType:" + _loadType.Name;
    }
}