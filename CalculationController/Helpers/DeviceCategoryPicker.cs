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
using CalculationController.DtoFactories;
using Common;
using Common.CalcDto;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

#endregion

namespace CalculationController.Helpers {
    public interface IDeviceCategoryPicker
    {
        [JetBrains.Annotations.NotNull]
        RealDevice PickDeviceFromCategory([JetBrains.Annotations.NotNull] DeviceCategory deviceCategory, EnergyIntensityType energyIntensity,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions);

        [JetBrains.Annotations.NotNull]
        RealDevice GetAutoDeviceDeviceFromDeviceCategoryOrDevice([CanBeNull] IAssignableDevice dev,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<IAssignableDevice> alldevices, EnergyIntensityType energyIntensity,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions, int locationID);

        //DeviceAction GetDeviceActionFromGroup(IAssignableDevice dev, IEnumerable<CalcDevice> calcDevices,ObservableCollection<DeviceAction> allDeviceActions);

        [JetBrains.Annotations.NotNull]
        DeviceAction GetAutoDeviceActionFromGroup([JetBrains.Annotations.NotNull] IAssignableDevice dev,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<IAssignableDevice> otherDevicesAtLocation, EnergyIntensityType energyIntensity,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> allDeviceActions, int locationID);

        [CanBeNull]
        RealDevice GetOrPickDevice([JetBrains.Annotations.NotNull] IAssignableDevice dev, [JetBrains.Annotations.NotNull] Location devLocation,
            EnergyIntensityType energyIntensity, [JetBrains.Annotations.NotNull][ItemNotNull] List<IAssignableDevice> allDevLocations,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> allDeviceActions);

        [CanBeNull]
        RealDevice GetDeviceDtoForAffordance([JetBrains.Annotations.NotNull] IAssignableDevice dev, [JetBrains.Annotations.NotNull][ItemNotNull] IEnumerable<CalcDeviceDto> calcDevices,
            int locationID,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<DeviceAction> allDeviceActions,
            [ItemNotNull] [JetBrains.Annotations.NotNull] List<DeviceCategoryDto> deviceCategories);

        [CanBeNull]
        DeviceAction GetDeviceActionFromGroup([JetBrains.Annotations.NotNull] IAssignableDevice dev, [JetBrains.Annotations.NotNull][ItemNotNull] IEnumerable<CalcDeviceDto> calcDevices,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> allDeviceActions);
    }
    public class DeviceCategoryPicker: IDeviceCategoryPicker
    {
        [CanBeNull] private readonly DeviceSelection _deviceSelection;
        [JetBrains.Annotations.NotNull]
        private readonly Dictionary<LocationDeviceTuple, DeviceAction> _pickedDeviceActions;
        [JetBrains.Annotations.NotNull]
        private readonly Dictionary<LocationDeviceTuple, RealDevice> _pickedDevices;
        [JetBrains.Annotations.NotNull]
        private readonly Random _random;

        public DeviceCategoryPicker([JetBrains.Annotations.NotNull] Random random, [CanBeNull] DeviceSelection deviceSelection)
        {
            _pickedDevices = new Dictionary<LocationDeviceTuple, RealDevice>();
            _pickedDeviceActions = new Dictionary<LocationDeviceTuple, DeviceAction>();
            _random = random;
            _deviceSelection = deviceSelection;
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public DeviceAction GetAutoDeviceActionFromGroup(IAssignableDevice dev,
                                                         List<IAssignableDevice> otherDevicesAtLocation, EnergyIntensityType energyIntensity,
                                                         ObservableCollection<DeviceAction> allDeviceActions, int locationID)
        {
            if(dev == null) {
                throw new LPGException("Device was null");
            }
            switch (dev.AssignableDeviceType) {
                case AssignableDeviceType.DeviceAction:
                    return (DeviceAction) dev;
                case AssignableDeviceType.DeviceActionGroup:
                    // schauen, ob schon zugeordnet
                    var deviceActionGroup = (DeviceActionGroup) dev;
                    var ldt = new LocationDeviceTuple(deviceActionGroup, locationID);
                    if (_pickedDeviceActions.ContainsKey(ldt)) {
                        return _pickedDeviceActions[ldt];
                    }

                    // pruefen, ob eins der anderen devices fuer diese group zutrifft
                    var myActions = deviceActionGroup.GetDeviceActions(allDeviceActions);
                    foreach (var deviceAction in myActions) {
                        if (otherDevicesAtLocation.Contains(deviceAction)) {
                            var action =
                                (DeviceAction) otherDevicesAtLocation.First(anondev => anondev == deviceAction);
                            _pickedDevices.Add(ldt, action.Device);
                            return action;
                        }
                    }
                    // see if another device from the same device category was already picked
                    // einfach eins aussuchen
                    var pickdevAction = PickDeviceFromGroup(deviceActionGroup, energyIntensity, allDeviceActions,
                        locationID);
                    _pickedDeviceActions.Add(ldt, pickdevAction);
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // ReSharper disable once HeuristicUnreachableCode
                    if (pickdevAction == null) {
                        // ReSharper disable once HeuristicUnreachableCode
                        throw new LPGException("Picked device was null");
                    }
                    return pickdevAction;
                default:
                    throw new LPGException("Forgotten AssignableDeviceType in GetAutoDeviceDevice. Please Report!");
            }
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public RealDevice GetAutoDeviceDeviceFromDeviceCategoryOrDevice(IAssignableDevice dev,
                                                                        List<IAssignableDevice> alldevices, EnergyIntensityType energyIntensity,
                                                                        ObservableCollection<DeviceAction> deviceActions, int locationID)
        {
            if(dev == null) {
                throw new LPGException("Device was null");
            }

            // bereits ein realDevice
            switch (dev.AssignableDeviceType) {
                case AssignableDeviceType.Device:
                    return (RealDevice) dev;
                case AssignableDeviceType.DeviceCategory:
                    // schauen, ob schon zugeordnet
                    var dc = (DeviceCategory) dev;
                    var ldt = new LocationDeviceTuple(dc, locationID);
                    if (_pickedDevices.ContainsKey(ldt)) {
                        return _pickedDevices[ldt];
                    }

                    // pruefen, ob eins der anderen devices fuer diese category zutrifft
                    foreach (var assignableDevice in alldevices) {
                        if (assignableDevice.AssignableDeviceType == AssignableDeviceType.Device) {
                            var rd = (RealDevice) assignableDevice;
                            if (rd.DeviceCategory == dc) {
                                _pickedDevices.Add(ldt, rd);
                                return rd;
                            }
                        }
                    }

                    // einfach eins aussuchen
                    var pickdev = PickDeviceFromCategory(dc, energyIntensity, deviceActions);
                    _pickedDevices.Add(ldt, pickdev);
                    return pickdev;
                default:
                    throw new LPGException("Forgotten Assignable Device Type! Please Report.");
            }
        }

        public DeviceAction GetDeviceActionFromGroup(IAssignableDevice dev, IEnumerable<CalcDeviceDto> calcDevices,
            ObservableCollection<DeviceAction> allDeviceActions)
        {
            var deviceActionGroup = (DeviceActionGroup) dev;
            var deviceActions = deviceActionGroup.GetDeviceActions(allDeviceActions);
            foreach (var calcDevice in calcDevices) {
                foreach (var deviceAction in deviceActions) {
                    if (deviceAction.Device== null)
                    {
                        throw new LPGException("Device was null");
                    }
                    if (calcDevice.Name == deviceAction.Device.Name) {
                        return deviceAction;
                    }
                }
            }
            return null;
        }

        // this only checks for an affordance, if the desired device category is fullfilled by the devices
        // this selects a device from the list of previously selected and created devices
        public RealDevice GetDeviceDtoForAffordance(IAssignableDevice dev, IEnumerable<CalcDeviceDto> calcDevices,
            int locationID, ObservableCollection<DeviceAction> allDeviceActions, List<DeviceCategoryDto> deviceCategories)
        {
            switch (dev.AssignableDeviceType) {
                case AssignableDeviceType.Device:
                    return (RealDevice) dev;
                case AssignableDeviceType.DeviceCategory:
                    var dc = (DeviceCategory) dev;
                    var deviceCategoryDto = deviceCategories.Single(x => x.FullCategoryName == dc.FullPath);
                    var ldt = new LocationDeviceTuple(dc, locationID);
                    if (_pickedDevices.ContainsKey(ldt) && _pickedDevices[ldt] != null) {
                        return _pickedDevices[ldt];
                    }
                    var existingDevicesFromCategory = new List<CalcDeviceDto>();
                    foreach (var calcDevice in calcDevices) {
                        if (calcDevice.DeviceCategoryGuid == deviceCategoryDto.Guid) {
                            existingDevicesFromCategory.Add(calcDevice);
                        }
                    }
                    if (existingDevicesFromCategory.Count > 0) {
                        dc.RefreshSubDevices();
                        foreach (var realDevice in dc.SubDevices) {
                            if (realDevice.Name == existingDevicesFromCategory[0].Name) {
                                return realDevice;
                            }
                        }
                        throw new LPGException("One device should have been picked!");
                    }
                    return null;
                case AssignableDeviceType.DeviceAction: {
                    var da = (DeviceAction) dev;
                    return da.Device;
                }
                case AssignableDeviceType.DeviceActionGroup: {
                    var da = GetDeviceActionFromGroup(dev, calcDevices, allDeviceActions);
                    return da?.Device;
                }
                default:
                    throw new LPGException("Missed a AssignableDeviceType");
            }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private static List<RealDevice> GetMeasuredDevicesFromRealDevices(
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions, [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> rds)
        {
            var devices = new List<RealDevice>();
            foreach (var rd in rds) {
                foreach (var da in deviceActions) {
                    if (da.Device == rd && da.AbsoluteProfileCount > 0 && !devices.Contains(rd)) {
                        devices.Add(rd);
                    }
                }
            }
            if (devices.Count == 0) // if nothing is found, just use all devices
            {
                devices.AddRange(rds);
            }
            return devices;
        }

        // this tries to double check if it's a device selection that's already been done
        public RealDevice GetOrPickDevice(IAssignableDevice dev, Location devLocation,
            EnergyIntensityType energyIntensity, List<IAssignableDevice> allDevLocations,
            ObservableCollection<DeviceAction> allDeviceActions)
        {
            // if it's already a device, we are done
            switch (dev.AssignableDeviceType) {
                case AssignableDeviceType.Device:
                    return (RealDevice) dev;
                case AssignableDeviceType.DeviceCategory: {
                    // check if we already dealt with this category at this Location
                    var dc = (DeviceCategory) dev;
                    var ldt = new LocationDeviceTuple(dc, devLocation.IntID);
                    if (_pickedDevices.ContainsKey(ldt)) // && _pickedDevices[ldt] != null)
                    {
                        return _pickedDevices[ldt];
                    }
                    // pick a new one
                    var result = PickRealDeviceFromCategoryAtHHLocation(dc, allDevLocations, energyIntensity,
                        devLocation, allDeviceActions);
                    if (result != null) {
                        _pickedDevices.Add(ldt, result);
                    }
                    return result;
                }
                case AssignableDeviceType.DeviceAction:
                    return ((DeviceAction) dev).Device;
                case AssignableDeviceType.DeviceActionGroup: {
                    // check if we already dealt with this group at this Location
                    var deviceActionGroup = (DeviceActionGroup) dev;
                    var ldt = new LocationDeviceTuple(deviceActionGroup, devLocation.IntID);
                    if (_pickedDeviceActions.ContainsKey(ldt)) // && _pickedDevices[ldt] != null)
                    {
                        return _pickedDeviceActions[ldt].Device;
                    }
                    var deviceActions = deviceActionGroup.GetDeviceActions(allDeviceActions);
                    if (!deviceActions.Any())
                    {
                        throw new LPGException("There were no DeviceActions in the DeviceActionGroup");
                    }
                    var firstDeviceAction = deviceActions.First();
                    if (firstDeviceAction.Device== null)
                    {
                        throw new LPGException("Device was null");
                    }
                    var usedDeviceCategory = firstDeviceAction.Device.DeviceCategory;
                        if (usedDeviceCategory== null)
                    {
                        throw new LPGException("used device category was null");
                    }
                    var ldt2 = new LocationDeviceTuple(usedDeviceCategory, devLocation.IntID);
                    if (_pickedDevices.ContainsKey(ldt2)) {
                        // find the device picked earlier
                        var rd = _pickedDevices[ldt2];
                        return rd;
                    }

                    // pick a new one
                    var result = PickDeviceActionFromGroupAtHHLocation(deviceActionGroup, allDevLocations,
                        energyIntensity, devLocation, allDeviceActions);
                    _pickedDeviceActions.Add(ldt, result);
                    if (result == null) {
                        throw new LPGException("Device was null");
                    }
                    _pickedDevices.Add(ldt2, result.Device);
                    return result.Device;
                }
                default:
                    throw new LPGException("Forgotten AssignableDeviceType. Please report!");
            }
        }

        [CanBeNull]
        private DeviceAction PickDeviceActionFromGroupAtHHLocation([JetBrains.Annotations.NotNull] DeviceActionGroup deviceActionGroup,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<IAssignableDevice> allDevLocations, EnergyIntensityType energyIntensity, [JetBrains.Annotations.NotNull] Location targetLoc,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> allDeviceActions)
        {
            // check if there is already such a device
            // first get all device actions in the group
            var deviceActions = deviceActionGroup.GetDeviceActions(allDeviceActions);
            //then get all devices in all device actions
            var devices = deviceActions.Select(da => da.Device).ToList();
            //then find if any of the locations already has a device for any of the device actions
            foreach (var device in allDevLocations) {
                if (devices.Contains(device)) {
                    var devac = deviceActions.Single(x => x.Device == device);
                    Logger.Debug(
                        "Picked " + devac.Name + " from " + targetLoc.Name + " since the device for it was already there.");
                    return devac;
                }
            }
            return PickDeviceFromGroup(deviceActionGroup, energyIntensity, allDeviceActions, targetLoc.IntID);
        }

        // the real picking function that selects a device
        public RealDevice PickDeviceFromCategory(DeviceCategory deviceCategory, EnergyIntensityType energyIntensity,
            ObservableCollection<DeviceAction> deviceActions)
        {
            deviceCategory.RefreshSubDevices();
            var rds = deviceCategory.SubDevices;

            if (rds.Count == 0) {
                throw new DataIntegrityException("The device category " + deviceCategory.Name +
                                                 " has no devices but it is used. A device could not be picked from it.");
            }
            RealDevice pickedDevice = null;
            // look for this device category in the device selection
            if (_deviceSelection != null && _deviceSelection.Items.Count > 0) {
                foreach (var item in _deviceSelection.Items) {
                    if (item.DeviceCategory == deviceCategory) {
                        pickedDevice = item.Device;
                    }
                }
            }
            if (pickedDevice == null) {
                switch (energyIntensity) {
                    case EnergyIntensityType.Random:
                        var dstval = _random.Next(rds.Count);
                        pickedDevice = rds[dstval];
                        break;
                    case EnergyIntensityType.EnergySaving:
                        pickedDevice = rds[0];
                        foreach (var t in rds) {
                            if (pickedDevice.WeightedEnergyIntensity > t.WeightedEnergyIntensity) {
                                pickedDevice = t;
                            }
                        }
                        break;
                    case EnergyIntensityType.EnergySavingPreferMeasured:
                        var devices = GetMeasuredDevicesFromRealDevices(deviceActions, rds);
                        pickedDevice = devices[0];
                        foreach (var t in devices) {
                            if (pickedDevice.WeightedEnergyIntensity > t.WeightedEnergyIntensity) {
                                pickedDevice = t;
                            }
                        }
                        break;
                    case EnergyIntensityType.EnergyIntensive:
                        pickedDevice = rds[0];
                        foreach (var t in rds) {
                            if (pickedDevice.WeightedEnergyIntensity < t.WeightedEnergyIntensity) {
                                pickedDevice = t;
                            }
                        }
                        break;
                    case EnergyIntensityType.EnergyIntensivePreferMeasured:
                        var rdevices = GetMeasuredDevicesFromRealDevices(deviceActions, rds);
                        pickedDevice = rdevices[0];
                        foreach (var t in rdevices) {
                            if (pickedDevice.WeightedEnergyIntensity < t.WeightedEnergyIntensity) {
                                pickedDevice = t;
                            }
                        }
                        break;
                    case EnergyIntensityType.AsOriginal:
                        throw new DataIntegrityException("Not permitted energy intensity type here: As Original");
                    default:
                        throw new DataIntegrityException("Unknown EnergyIntensityType");
                }
            }

            Logger.Debug("Picked " + pickedDevice + " from " + rds.Count + " devices.");
            return pickedDevice;
        }

        // the real picking function that selects a device
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        [JetBrains.Annotations.NotNull]
        private DeviceAction PickDeviceFromGroup([JetBrains.Annotations.NotNull] DeviceActionGroup deviceGroup, EnergyIntensityType energyIntensity,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> allDeviceActions, int locationID)
        {
            var deviceActions = deviceGroup.GetDeviceActions(allDeviceActions);
            if (deviceActions.Count == 0) {
                throw new DataIntegrityException("The device action group " + deviceGroup.Name +
                                                 " has no devices but it is used. A device could not be picked from it.");
            }
            if (deviceActions[0].Device == null)
            {
                throw new LPGException("Device was null");
            }
            var usedDeviceCategory = deviceActions[0].Device.DeviceCategory;
            if (usedDeviceCategory == null)
            {
                throw new LPGException("usedDeviceCategory was null");
            }
            var ldt2 = new LocationDeviceTuple(usedDeviceCategory, locationID);
            if (_pickedDevices.ContainsKey(ldt2)) {
                // find the device picked earlier
                var rd = _pickedDevices[ldt2];
                deviceActions = deviceActions.Where(x => x.Device == rd).ToList();
                if (deviceActions.Count == 0) {
                    throw new DataIntegrityException(
                        "The device action group " + deviceGroup.Name + " has no entry for the device " +
                        rd.PrettyName +
                        ". Please fix.", rd);
                }
            }
            DeviceAction pickedDevice = null;
            // look for this device group in the device selection
            if (_deviceSelection != null && _deviceSelection.Actions.Count > 0) {
                foreach (var item in _deviceSelection.Actions) {
                    if (item.DeviceActionGroup == deviceGroup) {
                        pickedDevice = item.DeviceAction;
                    }
                }
            }
            if (pickedDevice == null) {
                switch (energyIntensity) {
                    case EnergyIntensityType.Random:
                        var dstval = _random.Next(deviceActions.Count);
                        pickedDevice = deviceActions[dstval];
                        break;
                    case EnergyIntensityType.EnergySaving: {
                        var pickeDeviceAction = deviceActions[0];
                        foreach (var deviceAction in deviceActions) {
                            if (pickeDeviceAction.CalculateWeightedEnergyUse() >
                                deviceAction.CalculateWeightedEnergyUse()) {
                                pickeDeviceAction = deviceAction;
                            }
                        }
                        pickedDevice = pickeDeviceAction;
                    }
                        break;
                    case EnergyIntensityType.EnergySavingPreferMeasured: {
                        var actions = TryToGetMeasured(deviceActions);
                        var pickeDeviceAction = actions[0];
                        foreach (var deviceAction in actions) {
                            if (pickeDeviceAction.CalculateWeightedEnergyUse() >
                                deviceAction.CalculateWeightedEnergyUse()) {
                                pickeDeviceAction = deviceAction;
                            }
                        }
                        pickedDevice = pickeDeviceAction;
                    }
                        break;
                    case EnergyIntensityType.EnergyIntensive: {
                        var pickeDeviceAction = deviceActions[0];
                        foreach (var deviceAction in deviceActions) {
                            if (pickeDeviceAction.CalculateWeightedEnergyUse() <
                                deviceAction.CalculateWeightedEnergyUse()) {
                                pickeDeviceAction = deviceAction;
                            }
                        }
                        pickedDevice = pickeDeviceAction;
                    }
                        break;
                    case EnergyIntensityType.EnergyIntensivePreferMeasured: {
                        var actions = TryToGetMeasured(deviceActions);
                        var pickeDeviceAction = actions[0];
                        foreach (var deviceAction in deviceActions) {
                            if (pickeDeviceAction.CalculateWeightedEnergyUse() <
                                deviceAction.CalculateWeightedEnergyUse()) {
                                pickeDeviceAction = deviceAction;
                            }
                        }
                        pickedDevice = pickeDeviceAction;
                    }
                        break;
                    default:
                        throw new LPGException("Unknown EnergyIntensityType");
                }
            }
            Logger.Debug(
                "Picked " + pickedDevice.Name + " from " + deviceActions.Count + " device actions in the group.");
            if(pickedDevice == null) {
                throw new LPGException("Picked device was null");
            }
            return pickedDevice;
        }

        [CanBeNull]
        private RealDevice PickRealDeviceFromCategoryAtHHLocation([JetBrains.Annotations.NotNull] DeviceCategory deviceCategory,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<IAssignableDevice> allDevicesAtLocations, EnergyIntensityType energyIntensity, [JetBrains.Annotations.NotNull] Location targetLoc,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions)
        {
            // check if there is already such a device
            foreach (var device in allDevicesAtLocations) {
                if (device.AssignableDeviceType == AssignableDeviceType.Device) {
                    var rd = (RealDevice) device;
                    if (rd.DeviceCategory == deviceCategory) {
                        Logger
                            .Debug(
                                "Picked " + rd.Name + " from " + targetLoc.Name + " since it was already there.");
                        return null;
                    }
                }
            }

            return PickDeviceFromCategory(deviceCategory, energyIntensity, deviceActions);
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private static List<DeviceAction> TryToGetMeasured([JetBrains.Annotations.NotNull][ItemNotNull] List<DeviceAction> deviceActions)
        {
            var actions = new List<DeviceAction>();
            foreach (var deviceAction in deviceActions) {
                if (deviceAction.AbsoluteProfileCount > 0) {
                    actions.Add(deviceAction);
                }
            }
            if (actions.Count == 0) // if there is no measured one, then just take the normal ones
            {
                actions.AddRange(deviceActions);
            }
            return actions;
        }

        #region Nested type: LocationDeviceTuple

        private record LocationDeviceTuple : IEquatable<LocationDeviceTuple> {
            //public bool Equals(LocationDeviceTuple other) => Category.Equals(other.Category) && LocationID == other.LocationID;

            //public override bool Equals(object obj) => obj is LocationDeviceTuple other && Equals(other);

            //public override int GetHashCode()
            //{
            //    unchecked {
            //        return (Category.GetHashCode() * 397) ^ LocationID;
            //    }
            //}

            //public static bool operator ==(LocationDeviceTuple left, LocationDeviceTuple right) => left.Equals(right);

            //public static bool operator !=(LocationDeviceTuple left, LocationDeviceTuple right) => !left.Equals(right);

            public LocationDeviceTuple([JetBrains.Annotations.NotNull] IAssignableDevice category, int locationID)
            {
                Category = category;
                LocationID = locationID;
            }

            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            public IAssignableDevice Category { get; }

            [UsedImplicitly]
            public int LocationID { get; }

            [JetBrains.Annotations.NotNull]
            public override string ToString() => "LocID:" + LocationID + ", " + Category;
        }

        #endregion
    }
}