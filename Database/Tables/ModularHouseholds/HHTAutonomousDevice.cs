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

using System;
using System.Collections.Generic;
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

namespace Database.Tables.ModularHouseholds {
    public class HHTAutonomousDevice : DBBase, IAutonomousDevice, IDeviceSelectionWithVariable, IJSonSubElement<HHTAutonomousDevice.JsonDto>
    {
        public class JsonDto :IGuidObject {

            [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
            [Obsolete("Json Only")]
            public JsonDto()
            {
            }

            public JsonDto(StrGuid guid, JsonReference device, JsonReference location, JsonReference timeLimit, JsonReference timeProfile,
                           decimal standardDeviation, JsonReference variable, VariableCondition variableCondition, double variableValue, JsonReference loadType)
            {
                Guid = guid;
                Device = device;
                Location = location;
                TimeLimit = timeLimit;
                TimeProfile = timeProfile;
                StandardDeviation = standardDeviation;
                Variable = variable;
                VariableCondition = variableCondition;
                VariableValue = variableValue;
                LoadType = loadType;
            }

            public StrGuid Guid { get; set; }

            public JsonReference Device { get; set; }

            public JsonReference Location { get; set; }

            public JsonReference TimeLimit { get; set; }
            public JsonReference TimeProfile { get; set; }
            public decimal StandardDeviation { get; set; }
            public JsonReference Variable { get; set; }
            public VariableCondition VariableCondition { get; set; }
            public double VariableValue { get; set; }
            public JsonReference LoadType { get; set; }
        }

        public const string ParentIDFieldName = "HouseholdTraitID";
        public const string TableName = "tblHHTAutonomousDevices";

        [CanBeNull] private IAssignableDevice _device;

        private readonly int _householdTraitID;

        [CanBeNull] private  Location _location;

        [CanBeNull] private TimeLimit _timeLimit;

        [CanBeNull] private TimeBasedProfile _timeprofile;

        private decimal _timeStandardDeviation;

        [CanBeNull] private Variable _variable;

        private VariableCondition _variableCondition;
        private double _variableValue;

        [CanBeNull] private VLoadType _vLoadType;

        public HHTAutonomousDevice([CanBeNull]int? pID, [CanBeNull] IAssignableDevice device,
            [CanBeNull] TimeBasedProfile timeprofile, int householdTraitID, decimal timeStandardDeviation,
            [CanBeNull] VLoadType vLoadType, [CanBeNull] TimeLimit timeLimit, [JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] string name,
            [CanBeNull] Location location, double variableValue, VariableCondition variableCondition,
            [CanBeNull] Variable variable, [JetBrains.Annotations.NotNull] StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Household Trait Autonomous Device";
            ID = pID;
            _device = device;
            _timeprofile = timeprofile;
            _householdTraitID = householdTraitID;
            _timeStandardDeviation = timeStandardDeviation;
            _vLoadType = vLoadType;
            _timeLimit = timeLimit;
            _location = location;
            _variableValue = variableValue;
            _variableCondition = variableCondition;
            _variable = variable;
        }

        [UsedImplicitly]
        [CanBeNull]
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public DeviceCategory DeviceCategory {
            get {
                if (_device == null) {
                    return null;
                }
                switch (_device.AssignableDeviceType) {
                    case AssignableDeviceType.Device:
                        var rd = (RealDevice) _device;
                        return rd.DeviceCategory;
                    case AssignableDeviceType.DeviceCategory: return (DeviceCategory) _device;
                }
                return null;
            }
        }

        [UsedImplicitly]
        [CanBeNull]
        public int? HouseholdTraitID => _householdTraitID;

        public IAssignableDevice Device => _device;

        public VLoadType LoadType => _vLoadType;
        public Location Location => _location;

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public new string Name {
            get {
                if(_device!= null) {
                    return _device.Name;
                }
                return "(no name)";
            }
        }

        public TimeLimit TimeLimit => _timeLimit ;

        public TimeBasedProfile TimeProfile => _timeprofile;

        public decimal TimeStandardDeviation => _timeStandardDeviation;
        public Variable Variable => _variable;

        public VariableCondition VariableCondition => _variableCondition;

        public double VariableValue => _variableValue;

        public double Probability => 0;

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static HHTAutonomousDevice AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var hhadID = dr.GetIntFromLong("ID");
            var deviceID = dr.GetIntFromLong("DeviceID");
            var timeProfileID = dr.GetNullableIntFromLong("TimeProfileID", false, ignoreMissingFields);
            var householdID = dr.GetIntFromLong("HouseholdTraitID");
            var locationID = dr.GetIntFromLong("LocationID", false, ignoreMissingFields, -1);
            var loc = aic.Locations.FirstOrDefault(lt => lt.ID == locationID);

            var vLoadTypeID = dr.GetNullableIntFromLong("VLoadTypeID", false, ignoreMissingFields);

            VLoadType newloadType = null;
            if (vLoadTypeID != null) {
                newloadType = aic.LoadTypes.FirstOrDefault(lt => lt.ID == vLoadTypeID);
            }
            var timeLimitID = dr.GetNullableIntFromLong("TimeLimitID", false, ignoreMissingFields);
            if (timeLimitID == null && ignoreMissingFields) {
                timeLimitID = dr.GetNullableIntFromLong("DeviceTimeID", false, ignoreMissingFields);
            }
            TimeLimit newTimeLimit = null;
            if (timeLimitID != null) {
                newTimeLimit = aic.TimeLimits.FirstOrDefault(dt => dt.ID == timeLimitID);
            }
            var deviceType =
                (AssignableDeviceType) dr.GetIntFromLong("AssignableDeviceType", false, ignoreMissingFields);
            var timeStandardDeviation = dr.GetDecimal("TimeStandardDeviation", false, 0.1m, ignoreMissingFields);
            IAssignableDevice device;
            switch (deviceType) {
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
                    throw new LPGException("Forgotten assignable device type in HHTAutonomousDevice. Please report!");
            }
            TimeBasedProfile tp = null;
            if (timeProfileID != null) {
                tp = aic.TimeProfiles.FirstOrDefault(myTimeProfiles => myTimeProfiles.ID == timeProfileID);
            }
            var name = "(no name)";
            if (device != null) {
                name = device.Name;
            }

            var variableValue = dr.GetDouble("VariableValue", false, 0, ignoreMissingFields);
            var condition =
                (VariableCondition) dr.GetIntFromLong("VariableCondition", false, ignoreMissingFields);
            var variableID = dr.GetIntFromLong("VariableID", false, ignoreMissingFields, -1);
            var variable = aic.Variables.FirstOrDefault(x => x.ID == variableID);
            var guid = GetGuid(dr, ignoreMissingFields);

            var hhad = new HHTAutonomousDevice(hhadID, device, tp, householdID, timeStandardDeviation,
                newloadType, newTimeLimit, connectionString, name, loc, variableValue,
                condition, variable, guid);

            return hhad;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_device == null) {
                message = "Device not found";
                return false;
            }
            if (_device.AssignableDeviceType == AssignableDeviceType.Device ||
                _device.AssignableDeviceType == AssignableDeviceType.DeviceCategory) {
                if (_timeprofile == null) {
                    message = "Time profile not found";
                    return false;
                }
                if (_vLoadType == null) {
                    message = "Load type not found";
                    return false;
                }
            }
            if (_location == null) {
                message = "Location not found";
                return false;
            }
            if (_timeLimit == null) {
                message = "Time limit not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HHTAutonomousDevice> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeBasedProfile> timeBasedProfiles, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<RealDevice> realDevices,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceCategory> deviceCategories, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadTypes,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeLimit> timeLimits, bool ignoreMissingTables,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Location> locations, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceAction> deviceActions,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Variable> variables)
        {
            var aic = new AllItemCollections(timeProfiles: timeBasedProfiles, realDevices: realDevices,
                deviceCategories: deviceCategories, loadTypes: loadTypes, locations: locations, timeLimits: timeLimits,
                deviceActions: deviceActions, deviceActionGroups: deviceActionGroups, variables: variables);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new ObservableCollection<HHTAutonomousDevice>();
            foreach (var hhAutonomousDevice in result) {
                if (hhAutonomousDevice.Device == null) {
                    items2Delete.Add(hhAutonomousDevice);
                }
            }
            foreach (var hhAutonomousDevice in items2Delete) {
                hhAutonomousDevice.DeleteFromDB();
                result.Remove(hhAutonomousDevice);
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_device != null) {
                cmd.AddParameter("DeviceID", _device.IntID);
                cmd.AddParameter("AssignableDeviceType", _device.AssignableDeviceType);
            }
            cmd.AddParameter("TimeStandardDeviation", _timeStandardDeviation);
            if (_timeprofile != null) {
                cmd.AddParameter("TimeProfileID", _timeprofile.IntID);
            }

            cmd.AddParameter("HouseholdTraitID", _householdTraitID);
            if (_vLoadType != null) {
                cmd.AddParameter("VLoadTypeID", _vLoadType.IntID);
            }
            if (_timeLimit != null) {
                cmd.AddParameter("TimeLimitID", _timeLimit.IntID);
            }
            if (_location != null) {
                cmd.AddParameter("LocationID", _location.IntID);
            }

            cmd.AddParameter("VariableValue", _variableValue);
            cmd.AddParameter("VariableCondition", _variableCondition);
            if (_variable != null) {
                cmd.AddParameter("VariableID", _variable.IntID);
            }
        }

        public override string ToString()
        {
            if (_device != null) {
                return _device.Name;
            }
            return "(no name)";
        }

        public JsonDto GetJson()
        {
            return new JsonDto(Guid,Device?.GetJsonReference(),
                Location?.GetJsonReference(),
                TimeLimit?.GetJsonReference(),
                TimeProfile?.GetJsonReference(),
                TimeStandardDeviation,Variable?.GetJsonReference(),
                VariableCondition,
                VariableValue,
                LoadType?.GetJsonReference()
                );
        }
        public void SynchronizeDataFromJson(JsonDto jto, Simulator sim)
        {
            var checkedProperties = new List<string>();
            ValidateAndUpdateValueAsNeeded(nameof(Device),
                checkedProperties,
            _device?.GetJsonReference().Guid , jto.Device.Guid,
                ()=>_device = sim.GetAssignableDeviceByGuid(jto.Device?.Guid) ??
                          throw new LPGException("Could not find a device with for  " + jto.Device));
            ValidateAndUpdateValueAsNeeded(nameof(Location),
                checkedProperties, Location?.Guid , jto.Location.Guid,
            ()=>
                _location = sim.Locations.FindByGuid(jto.Location?.Guid) );
            ValidateAndUpdateValueAsNeeded(nameof(TimeLimit),
                checkedProperties, TimeLimit?.Guid, jto.TimeLimit.Guid,
            ()=>
                _timeLimit = sim.TimeLimits.FindByGuid(jto.TimeLimit?.Guid));
            ValidateAndUpdateValueAsNeeded(nameof(TimeProfile),
                checkedProperties,
                 TimeProfile?.Guid, jto.TimeProfile?.Guid,
                () => _timeprofile = sim.Timeprofiles.FindByJsonReference(jto.TimeProfile));
            if(TimeStandardDeviation != jto.StandardDeviation) {
                _timeStandardDeviation = jto.StandardDeviation;
                NeedsUpdate = true;
            }
            if (Variable?.Guid != jto.Variable?.Guid)
            {
                _variable = sim.Variables.FindByGuid(jto.Variable?.Guid);
                NeedsUpdate = true;
            }
            if (VariableCondition != jto.VariableCondition)
            {
                _variableCondition = jto.VariableCondition;
                NeedsUpdate = true;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (VariableValue != jto.VariableValue)
            {
                _variableValue = jto.VariableValue;
                NeedsUpdate = true;
            }
            if (LoadType?.Guid != jto.LoadType?.Guid)
            {
                _vLoadType = sim.LoadTypes.FindByGuid(jto.Variable?.Guid);
                NeedsUpdate = true;
            }
            ValidateAndUpdateValueAsNeeded(nameof(Guid),
                checkedProperties,
                Guid, jto.Guid,
                () => Guid = jto.Guid);
            CheckIfAllPropertiesWereCovered(checkedProperties, this);
            SaveToDB();
        }

        public StrGuid RelevantGuid => Guid;
    }
}