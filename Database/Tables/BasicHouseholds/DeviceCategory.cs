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

#region de

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    public sealed class DeviceCategory : DBBaseElement, IAssignableDevice, IComparable<DeviceCategory> // IComparable
    {
        public const string TableName = "tblDeviceCategories";

        [NotNull] private static readonly Dictionary<string, ObservableCollection<RealDevice>> _allDevicesDict =
            new Dictionary<string, ObservableCollection<RealDevice>>();

        [NotNull] private static readonly Dictionary<string, DeviceCategory> _dcNoneCategory =
            new Dictionary<string, DeviceCategory>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<RealDevice> _allDevices;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<DeviceCategory> _children;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<RealDevice> _subDevices;
        private bool _ignoreInRealDeviceViews;

        [CanBeNull] private DeviceCategory _parentCategory;

        private int _parentID;

        public DeviceCategory([NotNull] string pName, int parentID, [NotNull] string connectionString, bool ignoreInRealDeviceViews,
            [ItemNotNull] [NotNull] ObservableCollection<RealDevice> devices, StrGuid guid,
                              [CanBeNull]int? pID = null, bool isEmptyconnectionStringOk = false)
            : base(pName, TableName, connectionString, guid)
        {
            _allDevices = devices;
            if (_allDevices == null) {
                throw new LPGException("the device list should never be null. please report.");
            }
            if (connectionString.Length == 0 && !isEmptyconnectionStringOk) {
                throw new LPGException("Empty Connectionstring!");
            }
            ID = pID;
            TypeDescription = "Device Category";
            _parentID = parentID;
            _ignoreInRealDeviceViews = ignoreInRealDeviceViews;
            _children = new ObservableCollection<DeviceCategory>();
            _subDevices = new ObservableCollection<RealDevice>();
            RefreshSubDevices();
            SortChildren();
            RefreshNames();
        }

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<DeviceCategory> Children => _children;

        [NotNull]
        [UsedImplicitly]
        public string FullPath {
            get {
                var s = string.Empty;
                var dc = this;
                var i = 0;
                s = dc.ShortName + " / " + s;
                dc = dc._parentCategory;
                while (dc != null && dc != _dcNoneCategory[ConnectionString] && i < 10) {
                    s = dc.ShortName + " / " + s;
                    dc = dc._parentCategory;
                    i++;
                }
                if (s.EndsWith(" / ", StringComparison.Ordinal)) {
                    s = s.Substring(0, s.Length - 2);
                }
                return s.Trim();
            }
        }

        public bool IsRootCategory {
            get {
                if (_parentID == -1) {
                    return true;
                }
                return false;
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public DeviceCategory ParentCategory {
            get {
                if (_parentCategory == null) {
                    return _dcNoneCategory[ConnectionString];
                }
                return _parentCategory;
            }
            set {
                if (_parentCategory == value) {
                    return;
                }
                if (value == null) {
                    return;
                }
                if (value == this) {
                    return;
                }
                if (IsAChild(value)) {
                    Logger.Error("You can't make a category a child of itself!");
                    return;
                }

                if (_parentCategory != null && _parentCategory.Children.Contains(this)) {
                    _parentCategory.Children.Remove(this);
                }
                _parentCategory = value;
                _parentID = _parentCategory.IntID;
                if (_parentID != -1) {
                    _parentCategory.Children.Add(this);
                }
                SortChildren();
                OnPropertyChanged(nameof(ParentCategory));
                OnPropertyChanged(nameof(FullPath));
                OnPropertyChanged(nameof(Children));
                OnPropertyChanged(nameof(Name));
                ParentCategory?.OnPropertyChanged(nameof(Children));
                RefreshNames();
            }
        }

        [NotNull]
        public string ShortName {
            get => base.Name;
            set => base.Name = value;
        }

        public int CompareTo([CanBeNull] DeviceCategory other)
        {
            if (other == null) {
                return 0;
            }
            return string.Compare(FullPath, other.FullPath, StringComparison.Ordinal);
        }

        [NotNull]
        private static DeviceCategory AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id =  dr.GetIntFromLong("ID");
            var parentID = -1;
            if (!(dr["ParentID"] is DBNull)) {
                parentID =  dr.GetInt("ParentID");
            }
            var ignoreInRealDeviceViews = dr.GetBool("IgnoreInRealDeviceViews", false, false, ignoreMissingFields);
            var name =dr.GetString("Name","(no name)");
            var guid = GetGuid(dr, ignoreMissingFields);
            var db = new DeviceCategory(name, parentID, connectionString, ignoreInRealDeviceViews,
                aic.RealDevices,guid, id);

            return db;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
        {
            var dc = new DeviceCategory(FindNewName(isNameTaken, "New Device Category "), -1,
                connectionString, false, _allDevicesDict[connectionString], System.Guid.NewGuid().ToStrGuid());
            return dc;
        }

        [NotNull]
        public string GetAllName(int level)
        {
            _children.Sort();
            var s = Name;
            var pad = string.Empty.PadLeft(level);
            if (level == int.MaxValue) {
                throw new LPGException("Level too deep");
            }
            var builder = new StringBuilder();
            builder.Append(s);
            foreach (var deviceCategory in _children) {
                builder.Append(Environment.NewLine).Append(pad).Append(deviceCategory.GetAllName(level + 1));
            }
            return builder.ToString();
        }

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public List<UsedIn> GetHouseholdsForDeviceCategory(
            [ItemNotNull] [NotNull] IEnumerable<Affordance> affordances, [ItemNotNull] [NotNull] IEnumerable<Location> locations,
            [ItemNotNull] [NotNull] IEnumerable<HouseholdTrait> householdTraits, [ItemNotNull] [NotNull] IEnumerable<HouseType> houseTypes)
        {
            var used = new List<UsedIn>();
            foreach (var affordance in affordances) {
                foreach (var affordanceDevice in affordance.AffordanceDevices) {
                    if (affordanceDevice.Device == this) {
                        used.Add(new UsedIn(affordance, "Affordance"));
                    }
                }
            }
            foreach (var location in locations) {
                foreach (var locationDevice in location.LocationDevices) {
                    if (locationDevice.Device == this) {
                        used.Add(new UsedIn(location, "Location - Lightdevice"));
                    }
                }
            }

            foreach (var hht in householdTraits) {
                foreach (var autodev in hht.Autodevs) {
                    if (autodev.Device == this) {
                        used.Add(new UsedIn(hht, "Autonomous device"));
                    }
                }
            }

            foreach (var housetype in houseTypes) {
                foreach (var hhdev in housetype.HouseDevices) {
                    if (hhdev.Device == this) {
                        used.Add(new UsedIn(housetype, "House Type"));
                    }
                }
            }
            return used;
        }

        [NotNull]
        [UsedImplicitly]
        public static DeviceCategory ImportFromItem([NotNull] DeviceCategory toImport,[NotNull] Simulator dstSim)
        {
            var dc2 = GetItemFromListByName(dstSim.DeviceCategories.Items, toImport.ParentCategory?.Name);
            var parentID = -1;
            if (dc2 != null) {
                parentID = dc2.IntID;
            }
            else {
                Logger.Error("Parent device category for " + toImport.Name + " missing.");
                if (ThrowExceptionOnImportWithMissingParent) {
                    throw new LPGException("Missing Parent for " + toImport.Name);
                }
            }
            var dc = new DeviceCategory(toImport.ShortName, parentID, dstSim.ConnectionString,
                toImport.IgnoreInRealDeviceViews, dstSim.RealDevices.Items, toImport.Guid)
            {
                ParentCategory = dc2
            };
            dc.SaveToDB();

            return dc;
        }

        private void InitParentCategory([ItemNotNull] [NotNull] IEnumerable<DeviceCategory> allCategories)
        {
            foreach (var deviceCategory in allCategories) {
                _parentCategory = _dcNoneCategory[ConnectionString];
                if (deviceCategory.ID == _parentID && deviceCategory.ID > -1) {
                    _parentCategory = deviceCategory;
                    SortChildren();
                    break;
                }
            }
        }

        private bool IsAChild([NotNull] DeviceCategory dc)
        {
            if (Children.Contains(dc)) {
                return true;
            }

            foreach (var child in Children) {
                if (child.IsAChild(dc)) {
                    return true;
                }
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DeviceCategory> result,
            [NotNull] out DeviceCategory deviceCategoryNone, [NotNull] string connectionString, [ItemNotNull] [NotNull] ObservableCollection<RealDevice> devices,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(realDevices: devices);
            _allDevicesDict[connectionString] = devices;
            deviceCategoryNone = new DeviceCategory("(none)", -1, connectionString, false,
                devices, "04290EB6-4FC1-4D4D-92B8-E7E12A104C0F".ToStrGuid(), -1);
            _dcNoneCategory[connectionString] = deviceCategoryNone;
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            result.Add(_dcNoneCategory[connectionString]);
            foreach (var deviceCategory in result) {
                if (deviceCategory._parentID != -1) {
                    foreach (var other in result) {
                        if (deviceCategory._parentID == other.ID) {
                            other.Children.Add(deviceCategory);
                        }
                    }
                }
                deviceCategory.InitParentCategory(result);
                deviceCategory.Children.Sort();
            }
            result.Sort();
        }

        private void RefreshNames()
        {
            foreach (var category in Children) {
                category.RefreshNames();
            }
            OnPropertyChangedNoUpdate(nameof(ParentCategory));
            OnPropertyChangedNoUpdate(nameof(FullPath));
            OnPropertyChangedNoUpdate(nameof(Children));
            OnPropertyChangedNoUpdate(nameof(Name));
        }

        public void RefreshSubDevices()
        {
            //var hasFinished = false;
            Action a = () => {
                _subDevices.Clear();
                foreach (var observableDevice in _allDevices) {
                    if (observableDevice.DeviceCategory == this) {
                        _subDevices.Add(observableDevice);
                    }
                }
                _subDevices.Sort();
                //hasFinished = true;
            };
            Logger.Get().SafeExecuteWithWait(a);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", ShortName);
            cmd.AddParameter("ParentID", "@ParentID", _parentID);
            cmd.AddParameter("IgnoreInRealDeviceViews", _ignoreInRealDeviceViews);
        }

        private void SortChildren()
        {
            _children.Sort();
        }

        public override string ToString() => FullPath;

        #region IAssignableDevice Members

        [UsedImplicitly]
        public bool IgnoreInRealDeviceViews {
            get => _ignoreInRealDeviceViews;
            set => SetValueWithNotify(value, ref _ignoreInRealDeviceViews, nameof(IgnoreInRealDeviceViews));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<RealDevice> SubDevices {
            get {
                RefreshSubDevices();
                return _subDevices;
            }
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<RealDevice> SubDevicesWithoutRefresh => _subDevices;

        public AssignableDeviceType AssignableDeviceType => AssignableDeviceType.DeviceCategory;

        public override string Name => FullPath;

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static bool ThrowExceptionOnImportWithMissingParent { get; set; }

        public bool IsOrContainsStandbyDevice(ObservableCollection<DeviceAction> allActions)
        {
            foreach (var device in SubDevicesWithoutRefresh) {
                if (device.IsStandbyDevice) {
                    return true;
                }
            }
            return false;
        }

        public List<RealDevice> GetRealDevices(ObservableCollection<DeviceAction> allActions) => GetRealDevices();

        [ItemNotNull]
        [NotNull]
        public List<RealDevice> GetRealDevices()
        {
            var devices = new List<RealDevice>();
            foreach (var device in _subDevices) {
                devices.Add(device);
            }
            return devices;
        }

        public List<Tuple<VLoadType, double>> CalculateAverageEnergyUse(VLoadType dstLoadType,
         ObservableCollection<DeviceAction> allActions, TimeBasedProfile timeProfile, double multiplier,
            double probability)
        {
            if (SubDevices.Count == 0) {
                return new List<Tuple<VLoadType, double>>();
            }
            var sums = new Dictionary<VLoadType, double>();
            var counts = new Dictionary<VLoadType, int>();
            foreach (var realDevice in SubDevices) {
                var result = realDevice.CalculateAverageEnergyUse(dstLoadType, allActions,
                    timeProfile, multiplier, probability);
                foreach (var tuple in result) {
                    if (!sums.ContainsKey(tuple.Item1)) {
                        sums.Add(tuple.Item1, 0);
                        counts.Add(tuple.Item1, 0);
                    }
                    sums[tuple.Item1] += tuple.Item2;
                    counts[tuple.Item1]++;
                }
            }
            var results = new List<Tuple<VLoadType, double>>();
            foreach (var pair in sums) {
                var count = counts[pair.Key];
                var avg = pair.Value / count;
                var result = new Tuple<VLoadType, double>(pair.Key, avg);
                results.Add(result);
            }
            return results;
        }

        #endregion

        public override DBBase ImportFromGenericItem(DBBase toImport,  Simulator dstSim)
            => ImportFromItem( (DeviceCategory)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}