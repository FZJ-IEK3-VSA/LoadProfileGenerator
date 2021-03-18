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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.Houses {
    public class HouseTypeDevice : DBBase, IAutonomousDevice, IDeviceSelectionWithVariable {
        public const string TableName = "tblHouseTypeDevices";

        [CanBeNull] private readonly IAssignableDevice _device;

        [CanBeNull] private readonly VLoadType _loadType;

        [CanBeNull] private readonly Location _location;

        [CanBeNull] private readonly TimeBasedProfile _profile;

        private readonly double _timeStandardDeviation;

        [CanBeNull] private readonly Variable _variable;

        private readonly VariableCondition _variableCondition;
        private readonly double _variableValue;

        public HouseTypeDevice([CanBeNull]int? pID, [CanBeNull] IAssignableDevice adev, [CanBeNull] TimeBasedProfile profile,
            int houseID,
            [CanBeNull] TimeLimit timeLimit, double timeStandardDeviation, [CanBeNull] VLoadType loadType,
            [JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] string name,
            [CanBeNull] Location loc, double variableValue, VariableCondition variableCondition,
            [CanBeNull] Variable variable, [JetBrains.Annotations.NotNull] StrGuid guid)
            : base(name, TableName, connectionString, guid)
        {
            ID = pID;
            _location = loc;
            _device = adev;
            _profile = profile;
            HouseID = houseID;
            TimeLimit = timeLimit;
            _timeStandardDeviation = timeStandardDeviation;
            _loadType = loadType;
            _variableValue = variableValue;
            TypeDescription = "House Type Device";
            _variableCondition = variableCondition;
            _variable = variable;
        }

        public int HouseID { get; }

        [UsedImplicitly]
        [CanBeNull]
        public TimeBasedProfile Profile => _profile;
        public IAssignableDevice Device => _device;
        public VLoadType LoadType => _loadType;
        public Location Location => _location;

        [JetBrains.Annotations.NotNull]
        public override string Name {
            get {
                if (_device != null && Profile != null) {
                    return _device.Name + " - " + Profile.Name;
                }
                if (_device != null) {
                    return _device.Name + " - (no time profile)";
                }
                if (_profile != null) {
                    return _profile.Name;
                }
                return "(no name)";
            }
        }

        [UsedImplicitly]
        public TimeLimit TimeLimit { get; }

        public TimeBasedProfile TimeProfile => _profile;

        [UsedImplicitly]
        public decimal TimeStandardDeviation => (decimal) _timeStandardDeviation;

        public Variable Variable => _variable;

        public VariableCondition VariableCondition => _variableCondition;

        public double VariableValue => _variableValue;

        public double Probability => 0;

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static HouseTypeDevice AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var deviceID =  dr.GetInt("DeviceID");
            var assignableDeviceType = (AssignableDeviceType) dr.GetInt("AssignableDeviceType");

            IAssignableDevice device;
            switch (assignableDeviceType) {
                case AssignableDeviceType.Device:
                    device = aic.RealDevices.FirstOrDefault(mydevice => mydevice.ID == deviceID);
                    break;
                case AssignableDeviceType.DeviceCategory:
                    device = aic.DeviceCategories.FirstOrDefault(mydeviceCat => mydeviceCat.ID == deviceID);
                    break;
                case AssignableDeviceType.DeviceAction:
                    device = aic.DeviceActions.FirstOrDefault(mydeviceCat => mydeviceCat.ID == deviceID);
                    break;
                case AssignableDeviceType.DeviceActionGroup:
                    device = aic.DeviceActionGroups.FirstOrDefault(mydeviceCat => mydeviceCat.ID == deviceID);
                    break;
                default:
                    throw new LPGException("Unknown AssignableDeviceType. This is a bug. Please report!");
            }

            var timeProfileID = dr.GetNullableIntFromLongOrInt("TimeProfileID", false, ignoreMissingFields);
            TimeBasedProfile tp = null;
            if (timeProfileID != null) {
                tp = aic.TimeProfiles.FirstOrDefault(myTimeProfiles => myTimeProfiles.ID == timeProfileID);
            }
            var houseID = dr.GetInt("HouseID");
            var dtID = dr.GetIntFromLong("TimeLimitID", false, ignoreMissingFields);
            var standardDeviation = dr.GetDouble("StandardDeviation");
            var timeLimit = aic.TimeLimits.FirstOrDefault(myDateTime => myDateTime.ID == dtID);
            if (timeLimit == null && aic.TimeLimits.Count > 0) {
                timeLimit = aic.TimeLimits.First();
            }
            var locationID = dr.GetIntFromLong("LocationID", false, ignoreMissingFields, -1);
            var loc = aic.Locations.FirstOrDefault(vl => vl.ID == locationID);

            var loadtypeID = dr.GetNullableIntFromLong("VLoadTypeID", false);
            VLoadType vlt = null;
            if (loadtypeID != null) {
                vlt = aic.LoadTypes.FirstOrDefault(vl => vl.ID == loadtypeID);
            }
            var name = "(no name)";
            if (device != null) {
                name = device.Name;
            }
            if (timeLimit != null) {
                name += "[" + timeLimit.Name + "]";
            }

            var variableValue = dr.GetDouble("VariableValue", false, 0, ignoreMissingFields);
            var tc =
                (VariableCondition) dr.GetIntFromLong("VariableCondition", false, ignoreMissingFields);
            var variableID = dr.GetIntFromLong("VariableID", false, ignoreMissingFields, -1);
            var variable = aic.Variables.FirstOrDefault(x => x.ID == variableID);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new HouseTypeDevice(id, device, tp, houseID, timeLimit, standardDeviation, vlt, connectionString,
                name, loc,
                variableValue, tc, variable, guid);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_device == null) {
                message = "Device not found";
                return false;
            }
            if (TimeLimit == null) {
                message = "Time limit not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HouseTypeDevice> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<RealDevice> pallDevices, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceCategory> pallDeviceCategories,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeBasedProfile> pallTimeBasedProfiles, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeLimit> timeLimits,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> vLoadTypes, bool ignoreMissingTables,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Location> allLocations, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceAction> deviceActions,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Variable> variables)
        {
            var aic = new AllItemCollections(realDevices: pallDevices,
                deviceCategories: pallDeviceCategories, timeProfiles: pallTimeBasedProfiles, timeLimits: timeLimits,
                loadTypes: vLoadTypes, locations: allLocations, deviceActions: deviceActions,
                deviceActionGroups: deviceActionGroups, variables: variables);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (TimeLimit != null) {
                cmd.AddParameter("TimeLimitID", "@TimeLimitID", TimeLimit.IntID);
            }
            if (_device != null) {
                cmd.AddParameter("DeviceID", _device.IntID);
                cmd.AddParameter("AssignableDeviceType", _device.AssignableDeviceType);
            }
            if (_profile != null) {
                cmd.AddParameter("TimeProfileID", "@TimeProfileID", _profile.IntID);
            }
            cmd.AddParameter("StandardDeviation", "@StandardDeviation", _timeStandardDeviation);
            cmd.AddParameter("HouseID", "@HouseID", HouseID);
            if (_loadType != null) {
                cmd.AddParameter("VLoadTypeID", _loadType.IntID);
            }
            if (_location != null) {
                cmd.AddParameter("LocationID", _location.IntID);
            }
            if (_variable != null) {
                cmd.AddParameter("VariableID", _variable.IntID);
            }
            cmd.AddParameter("VariableValue", _variableValue);
            cmd.AddParameter("VariableCondition", _variableCondition);
        }

        public override string ToString() => Name;
    }
}