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

using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public class LocationDevice : DBBase, IDeviceSelection {
        public const string TableName = "tblLocationDevices";

        [CanBeNull] private readonly IAssignableDevice _device;

        private readonly int _locationID;

        public LocationDevice([CanBeNull]int? pID, [CanBeNull] IAssignableDevice adev, int locationID, [JetBrains.Annotations.NotNull] string connectionString,
            [JetBrains.Annotations.NotNull] string name, [NotNull] StrGuid guid)
            : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Location Device";
            ID = pID;
            _device = adev;
            _locationID = locationID;
            LoadType = null;
        }

        [UsedImplicitly]
        [CanBeNull]
        public DeviceCategory DeviceCategory {
            get {
                if (_device?.AssignableDeviceType == AssignableDeviceType.DeviceCategory) {
                    return (DeviceCategory)_device;
                }
                var rd = (RealDevice) Device;
                return rd?.DeviceCategory;
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string DeviceName {
            get {
                if (_device != null) {
                    return _device.Name;
                }
                return "(no name)";
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string DeviceOrCategory {
            get {
                if (_device == null) {
                    return "(no device selected)";
                }
                if (_device.AssignableDeviceType == AssignableDeviceType.Device) {
                    return "Device";
                }
                return "Device Category";
            }
        }

        [UsedImplicitly]
        public int LocationID => _locationID;

        public override string Name {
            get {
                if (_device != null) {
                    return _device.Name;
                }
                return "(no name)";
            }
        }

        [UsedImplicitly]
        [CanBeNull]
        public RealDevice RealDevice {
            get {
                if (_device == null) {
                    return null;
                }
                if (_device.AssignableDeviceType == AssignableDeviceType.Device) {
                    return (RealDevice) _device;
                }
                return null;
            }
        }

        [UsedImplicitly]
        public IAssignableDevice Device => _device;

        public VLoadType LoadType { get; }

        public double Probability => 0;
        public TimeBasedProfile TimeProfile => null;

        [JetBrains.Annotations.NotNull]
        private static LocationDevice AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var locdevID = dr.GetIntFromLong("ID");
            var locID = dr.GetIntFromLong("LocationID");
            var deviceID = dr.GetIntFromLong("DeviceID");
            var assignableDeviceTypeInt = dr.GetIntFromLong("AssignableDeviceType", false);
            var assignableDeviceType = (AssignableDeviceType) assignableDeviceTypeInt;
            IAssignableDevice device;
            if (assignableDeviceType == AssignableDeviceType.Device) {
                device = aic.RealDevices.FirstOrDefault(mydevice => mydevice.ID == deviceID);
            }
            else {
                device = aic.DeviceCategories.FirstOrDefault(mydeviceCat => mydeviceCat.ID == deviceID);
            }
            var name = "(no name)";
            if (device != null) {
                name = device.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var locdev = new LocationDevice(locdevID, device, locID, connectionString, name, guid);
            return locdev;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_device == null) {
                message = "Device not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<LocationDevice> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<RealDevice> realDevices, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceCategory> deviceCategories,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadTypes, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(realDevices: realDevices, deviceCategories: deviceCategories,
                loadTypes: loadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_device != null) {
                cmd.AddParameter("DeviceID", "@DeviceID", _device.IntID);
                cmd.AddParameter("AssignableDeviceType", "@AssignableDeviceType", _device.AssignableDeviceType);
            }
            cmd.AddParameter("LocationID", "@LocationID", _locationID);
        }

        public override string ToString() => Name;
    }
}