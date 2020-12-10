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
using CalculationEngine.HouseholdElements;
using Common.CalcDto;
using JetBrains.Annotations;

namespace CalculationController.CalcFactories {
    public class CalcLocationFactory
    {
        [NotNull]
        private readonly CalcLoadTypeDictionary _calcLoadTypeDict;

        private readonly CalcRepo _calcRepo;

        public CalcLocationFactory( [NotNull] CalcLoadTypeDictionary calcLoadTypeDict,CalcRepo calcRepo
            )
        {
            _calcLoadTypeDict = calcLoadTypeDict;
            _calcRepo = calcRepo;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcLocation> MakeCalcLocations([NotNull][ItemNotNull] List<CalcLocationDto> locations,
                                                    [NotNull] DtoCalcLocationDict locdict, CalcRepo calcRepo) {
            var locs = new List<CalcLocation>();
            foreach (var t in locations) {
                // loc anlegen
                var cloc = new CalcLocation(t.Name, t.Guid);
                foreach (var locdev in t.LightDevices) {
                    var deviceLoads = CalcDeviceFactory.MakeCalcDeviceLoads(locdev,_calcLoadTypeDict);
                    var deviceName = CalcAffordanceFactory.FixAffordanceName(locdev.Name, _calcRepo.CalcParameters.CSVCharacter);
                    locdev.Name = deviceName;
                    var clightdevice = new CalcDevice(  deviceLoads,
                         cloc, locdev,calcRepo);
                    cloc.AddLightDevice(clightdevice);
                }
                //deviceLocationDict.Add(cloc, new List<IAssignableDevice>());
                locs.Add(cloc);
                locdict.LocationDtoDict.Add(t, cloc);
            }
            return locs;
        }
    }
}