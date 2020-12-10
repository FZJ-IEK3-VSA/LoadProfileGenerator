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
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.CalcFactories {
    public class CalcLoadTypeDictionary
    {
        public CalcLoadTypeDictionary([NotNull] Dictionary<CalcLoadTypeDto, CalcLoadType> dtoCalcDict)
        {
            DtoCalcDict = dtoCalcDict;
            _calcLoadTypes = dtoCalcDict.Values.ToList();
            _guids = new HashSet<StrGuid>(_calcLoadTypes.Select(x => x.Guid));
        }

        [ItemNotNull]
        [NotNull]
        private readonly List<CalcLoadType> _calcLoadTypes;

        [NotNull]
        private Dictionary<CalcLoadTypeDto, CalcLoadType> DtoCalcDict { get; }

        [NotNull]
        private readonly HashSet<StrGuid> _guids;

        [NotNull]
        public CalcLoadType GetLoadtypeByGuid(StrGuid loadtypeGuid)
        {
            return _calcLoadTypes.Single(x => x.Guid == loadtypeGuid);
        }

        public bool SimulateLoadtype(StrGuid guid)
        {
            return _guids.Select(x=> x.StrVal).Contains(guid.StrVal);
        }
        /*
        public bool SimulateLoadtype([JetBrains.Annotations.NotNull] CalcLoadTypeDto lt)
        {
            if (DtoCalcDict.ContainsKey(lt))
            {
                return true;
            }

            return false;
        }*/

        [NotNull]
        public CalcLoadType GetCalcLoadTypeByLoadtype([NotNull] CalcLoadTypeDto lt)
        {
            return DtoCalcDict[lt];
        }

        /*
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcLoadType> GetAllCalcLoadtypes()
        {
            return DtoCalcDict.Values.ToList();
        }*/
    }

    public static class CalcLoadTypeDtoFactory {
        [NotNull]
        public static CalcLoadTypeDtoDictionary MakeLoadTypes(
            [NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes, TimeSpan internalTimeResolution,
            LoadTypePriority priority)
        {
            var ltDtoDict = new Dictionary<VLoadType, CalcLoadTypeDto>();
            foreach (var lt in loadTypes) {
                // ReSharper disable once ReplaceWithSingleAssignment.True
                if (lt.Priority <= priority) {
                    var guid = Guid.NewGuid().ToStrGuid();
                    var calcLoadTypeDto = new CalcLoadTypeDto(lt.Name,  lt.UnitOfPower, lt.UnitOfSum,
                        lt.ConvertPowerValueWithTime(1, internalTimeResolution), lt.ShowInCharts, guid);
                    ltDtoDict.Add(lt, calcLoadTypeDto);
                }
            }
            return new CalcLoadTypeDtoDictionary(ltDtoDict);
        }
    }

    public static class CalcLoadTypeFactory {
        [NotNull]
        public static CalcLoadTypeDictionary MakeLoadTypes(
            [NotNull] CalcLoadTypeDtoDictionary dtoDict) {
            var ltDict = new Dictionary<CalcLoadTypeDto, CalcLoadType>();
            foreach (var dto in dtoDict.Ltdtodict.Values) {
                    var calcLoadType = new CalcLoadType(dto.Name,  dto.UnitOfPower, dto.UnitOfSum,
                       dto.ConversionFactor, dto.ShowInCharts,dto.Guid);
                    ltDict.Add(dto, calcLoadType);
            }
            return new CalcLoadTypeDictionary(ltDict);
        }
    }
}