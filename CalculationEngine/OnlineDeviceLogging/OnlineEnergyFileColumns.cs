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
using Automation;
using Automation.ResultFiles;

namespace CalculationEngine.OnlineDeviceLogging {
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Common.CalcDto;
    using Common.JSON;
    using OnlineLogging;

    public readonly struct ZeroEntryKey:IEquatable<ZeroEntryKey> {
        public ZeroEntryKey([JetBrains.Annotations.NotNull] string locationGuid, [JetBrains.Annotations.NotNull] string deviceGuid, [JetBrains.Annotations.NotNull] HouseholdKey householdKey, OefcDeviceType deviceType)
        {
            _locationGuid = locationGuid;
            _deviceGuid = deviceGuid;
            _householdKey = householdKey;
            _deviceType = deviceType;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = _locationGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ _deviceGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ _householdKey.GetHashCode();
                return (hashCode * 397) ^ (int) _deviceType;
            }
        }

        public ZeroEntryKey([JetBrains.Annotations.NotNull] HouseholdKey householdKey, OefcDeviceType deviceType, [JetBrains.Annotations.NotNull] string deviceID, [JetBrains.Annotations.NotNull] string locationID) {
            _householdKey = householdKey;
            _deviceType = deviceType;
            _deviceGuid = deviceID;
            _locationGuid = locationID;
        }

        public override bool Equals(object? obj) {
            if (obj is null) {
                return false;
            }
            return obj is ZeroEntryKey key && Equals(key);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool Equals(ZeroEntryKey other) => _locationGuid == other._locationGuid && _deviceGuid == other._deviceGuid &&
                                                  _householdKey == other._householdKey &&
                                                  _deviceType == other._deviceType;

        public static bool operator ==(ZeroEntryKey point1, ZeroEntryKey point2) => point1.Equals(point2);

        public static bool operator !=(ZeroEntryKey point1, ZeroEntryKey point2) => !point1.Equals(point2);

        [JetBrains.Annotations.NotNull] private readonly string _locationGuid;
        [JetBrains.Annotations.NotNull] private readonly string _deviceGuid;
        [JetBrains.Annotations.NotNull]
        private readonly HouseholdKey _householdKey;
        private readonly OefcDeviceType _deviceType;
    }

    public class OnlineEnergyFileColumns {
        private readonly IOnlineLoggingData _old;

        [JetBrains.Annotations.NotNull]
        private readonly Dictionary<CalcLoadTypeDto, Dictionary<OefcKey, ColumnEntry>> _columnEntriesByLoadTypeByDeviceKey;
        public OnlineEnergyFileColumns(IOnlineLoggingData old) {
            _old = old;
            _columnEntriesByLoadTypeByDeviceKey = new Dictionary<CalcLoadTypeDto, Dictionary<OefcKey, ColumnEntry>>();
            ColumnCountByLoadType = new Dictionary<CalcLoadTypeDto, int>();
            _columnEntriesByColumn = new Dictionary<CalcLoadTypeDto, Dictionary<int, ColumnEntry>>();
        }

        [JetBrains.Annotations.NotNull]
        public Dictionary<CalcLoadTypeDto, int> ColumnCountByLoadType { get; }
        [JetBrains.Annotations.NotNull]
        private readonly Dictionary<CalcLoadTypeDto, Dictionary<int, ColumnEntry>> _columnEntriesByColumn;
        //[JetBrains.Annotations.NotNull] public Dictionary<CalcLoadTypeDto, Dictionary<int, ColumnEntry>> ColumnEntriesByColumn => _columnEntriesByColumn;
        //private readonly Dictionary<string, string> _registeredDevices = new Dictionary<string, string>();
        public bool IsDeviceRegistered([JetBrains.Annotations.NotNull] CalcLoadTypeDto loadtype, OefcKey key)
        {
            //var tmpkey = key.HouseholdKey + "#" + key.LocationGuid + "#" + key.DeviceName;
          //  if (!_registeredDevices.ContainsKey(tmpkey)) {
                //_registeredDevices.Add(tmpkey, key.DeviceGuid.StrVal);
            //}
            //if (  key.DeviceGuid.StrVal != _registeredDevices[tmpkey]) {
//                throw new LPGException("device registered twice: " + tmpkey);
  //          }

            if (_columnEntriesByLoadTypeByDeviceKey.ContainsKey(loadtype) &&
                ColumnEntriesByLoadTypeByDeviceKey[loadtype].ContainsKey(key)) {
                return true;
            }
            throw new LPGException("Forgotten device registration: " + key);
        }

        [JetBrains.Annotations.NotNull]
        public Dictionary<CalcLoadTypeDto, Dictionary<OefcKey, ColumnEntry>> ColumnEntriesByLoadTypeByDeviceKey
            => _columnEntriesByLoadTypeByDeviceKey;
        public void AddColumnEntry([JetBrains.Annotations.NotNull] string name, OefcKey key,
                                   [JetBrains.Annotations.NotNull] string locationName, [JetBrains.Annotations.NotNull] CalcLoadTypeDto lt, StrGuid deviceGuid,
            [JetBrains.Annotations.NotNull] HouseholdKey householdKey, [JetBrains.Annotations.NotNull] string deviceCategory, [JetBrains.Annotations.NotNull] CalcDeviceDto calcDeviceDto) {
            if (!ColumnCountByLoadType.ContainsKey(lt)) {
                ColumnCountByLoadType.Add(lt, 0);
            }
            var dstcolum = ColumnCountByLoadType[lt];

            var ce = new ColumnEntry(name, dstcolum, locationName, deviceGuid,
                householdKey, lt,key.ToString(), deviceCategory, calcDeviceDto);
            if (!_columnEntriesByLoadTypeByDeviceKey.ContainsKey(lt)) {
                _columnEntriesByLoadTypeByDeviceKey.Add(lt, new Dictionary<OefcKey, ColumnEntry>());
                _columnEntriesByColumn.Add(lt, new Dictionary<int, ColumnEntry>());
            }
            if (!_columnEntriesByLoadTypeByDeviceKey[lt].ContainsKey(key)) {
                _columnEntriesByLoadTypeByDeviceKey[lt].Add(key, ce);
                _columnEntriesByColumn[lt].Add(dstcolum, ce);
                ColumnCountByLoadType[lt]++;
                _old.AddColumnEntry(ce);
            }
        }

        public int GetColumnNumber([JetBrains.Annotations.NotNull] CalcLoadTypeDto loadType,  OefcKey deviceKey) =>
            _columnEntriesByLoadTypeByDeviceKey[loadType][deviceKey].Column;
    }
}