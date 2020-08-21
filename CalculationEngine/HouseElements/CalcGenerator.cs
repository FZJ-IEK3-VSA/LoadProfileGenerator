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
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseElements {
    public class CalcGenerator : CalcBase {
        private readonly OefcKey _devProcessorKey;

        [NotNull] private readonly CalcLoadType _loadType;

        [NotNull] private readonly IOnlineDeviceActivationProcessor _odap;

        [NotNull] private readonly List<double> _values;

        public CalcGenerator(  [NotNull] IOnlineDeviceActivationProcessor odap,
                             [NotNull] CalcLoadType loadType,
                             [NotNull] List<double> values,
                             [NotNull] CalcParameters calcParameters,  [NotNull] CalcDeviceDto calcDeviceDto) : base(calcDeviceDto.Name, calcDeviceDto.Guid)
        {
            //_devProcessorKey = new OefcKey(householdKey, OefcDeviceType.Generator, guid, "-1", loadType.Guid, "Generator");
            _odap = odap;
            _loadType = loadType;
            _values = values;
            _devProcessorKey= _odap.RegisterDevice( _loadType.ConvertToDto(), calcDeviceDto);
            if (values.Count != calcParameters.InternalTimesteps) {
                throw new LPGException("Not enough values in the generator to fill the entire period!");
            }
        }

        public bool ProcessOneTimestep([NotNull] [ItemNotNull] List<OnlineEnergyFileRow> fileRows, [NotNull] TimeStep timeStep,
                                       [ItemNotNull] List<string>? log)
        {
            var madeChanges = false;
            foreach (var fileRow in fileRows) {
                if (fileRow.LoadType == _loadType.ConvertToDto()) {
                    var column = _odap.Oefc.GetColumnNumber(_loadType.ConvertToDto(), _devProcessorKey);
                    if (Math.Abs(fileRow.EnergyEntries[column] - _values[timeStep.InternalStep]) > Constants.Ebsilon) {
                        fileRow.EnergyEntries[column] = _values[timeStep.InternalStep];
                        log?.Add(Name + " set " + _loadType.Name + " to " + _values[timeStep.InternalStep]);
                        madeChanges = true;
                    }
                }
            }

            return madeChanges;
        }
    }
}