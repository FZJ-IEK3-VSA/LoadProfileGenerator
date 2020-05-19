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
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class DeviceSelectionItem : DBBase {
        public const string TableName = "tblDeviceSelectionItems";

        [CanBeNull] private readonly RealDevice _device;

        [CanBeNull] private readonly DeviceCategory _deviceCategory;
        [CanBeNull]
        private readonly int? _deviceSelectionID;

        public DeviceSelectionItem([CanBeNull]int? pID, [CanBeNull] int? deviceSelectionID, [CanBeNull] DeviceCategory deviceCategory,
            [CanBeNull] RealDevice device,
            [NotNull] string connectionString, [NotNull] string name, StrGuid guid)
            : base(name, TableName, connectionString, guid) {
            ID = pID;
            _deviceCategory = deviceCategory;
            _device = device;
            _deviceSelectionID = deviceSelectionID;
            TypeDescription = "Device Selection Item";
        }

        [NotNull]
        public RealDevice Device => _device ?? throw new InvalidOperationException();
        [NotNull]
        public DeviceCategory DeviceCategory => _deviceCategory ?? throw new InvalidOperationException();
        [CanBeNull]
        public int? DeviceSelectionID => _deviceSelectionID;

        [NotNull]
        private static DeviceSelectionItem AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var deviceSelectionItemID = dr.GetIntFromLong("ID");
            var deviceSelectionID = dr.GetIntFromLong("deviceSelectionID", false, ignoreMissingFields, -1);
            var deviceCategoryID = dr.GetIntFromLong("deviceCategoryID", false, ignoreMissingFields, -1);
            var deviceID = dr.GetIntFromLong("DeviceID", false, ignoreMissingFields, -1);
            var rd = aic.RealDevices.FirstOrDefault(myDevice => myDevice.ID == deviceID);
            var dc =
                aic.DeviceCategories.FirstOrDefault(myDeviceCategory => myDeviceCategory.ID == deviceCategoryID);
            var name = "(no name)";
            if (dc != null) {
                name = dc.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var dsi = new DeviceSelectionItem(deviceSelectionItemID, deviceSelectionID, dc, rd,
                connectionString, name, guid);
            return dsi;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_device == null) {
                message = "Device not found";
                return false;
            }
            if (_deviceCategory == null) {
                message = "Device category not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DeviceSelectionItem> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceCategory> deviceCategories, [ItemNotNull] [NotNull] ObservableCollection<RealDevice> devices,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections(deviceCategories: deviceCategories, realDevices: devices);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_deviceSelectionID != null) {
                cmd.AddParameter("deviceSelectionID", _deviceSelectionID);
            }
            if (_device != null) {
                cmd.AddParameter("DeviceID", Device.IntID);
            }
            if (_deviceCategory != null) {
                cmd.AddParameter("deviceCategoryID", DeviceCategory.IntID);
            }
        }

        public override string ToString() {
            if (_device != null) {
                return DeviceCategory.Name;
            }
            return "unknown";
        }
    }
}