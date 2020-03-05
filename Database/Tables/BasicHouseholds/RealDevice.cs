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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    public class RealDevice : DBBaseElement, IAssignableDevice {
        public const string TableName = "tblDevices";

        [CanBeNull] private static DeviceCategory _noneCategory;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<RealDeviceLoadType> _loads;
        [NotNull] private string _description;

        [CanBeNull] private DeviceCategory _deviceCategory;

        private bool _forceAllLoadTypesToBeSet;
        private bool _isStandbyDevice;
        [NotNull] private string _picture;
        private int _year;

        public RealDevice([NotNull] string pName, int pYear, [NotNull] string pPicture, [CanBeNull] DeviceCategory deviceCategory,
            [NotNull] string description,
            bool forceAllLoadTypesToBeSet, bool isStandbyDevice, [NotNull] string connectionString, [NotNull] string guid, [CanBeNull]int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            _year = pYear;
            _isStandbyDevice = isStandbyDevice;
            _picture = pPicture;
            _deviceCategory = deviceCategory;
            _description = description;
            TypeDescription = "Device";
            _forceAllLoadTypesToBeSet = forceAllLoadTypesToBeSet;
            _loads = new ObservableCollection<RealDeviceLoadType>();
            PropertyChanged += OnPropertyChanged;
        }

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [CanBeNull]
        [UsedImplicitly]
        public DeviceCategory DeviceCategory {
            get => _deviceCategory;

            set {
                SetValueWithNotify(value, ref _deviceCategory, nameof(DeviceCategory));
                if (_deviceCategory == null) {
                    throw new LPGException("Device category was null");
                }
                _deviceCategory.RefreshSubDevices();
            }
        }

        [UsedImplicitly]
        public bool ForceAllLoadTypesToBeSet {
            get => _forceAllLoadTypesToBeSet;
            set => SetValueWithNotify(value, ref _forceAllLoadTypesToBeSet, nameof(ForceAllLoadTypesToBeSet));
        }

        [UsedImplicitly]
        public bool IsStandbyDevice {
            get => _isStandbyDevice;
            set => SetValueWithNotify(value, ref _isStandbyDevice, nameof(IsStandbyDevice));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<RealDeviceLoadType> Loads => _loads;

        [NotNull]
        [UsedImplicitly]
        public string NameWithEnergyIntensity => Name + " (Energyintensity: " + WeightedEnergyIntensity + ")";

        [NotNull]
        [UsedImplicitly]
        public string Picture {
            get => _picture;
            set => SetValueWithNotify(value, ref _picture, nameof(Picture));
        }

        [UsedImplicitly]
        public decimal WeightedEnergyIntensity {
            get {
                double total = 0;
                foreach (var realDeviceLoadType in Loads) {
                    if(realDeviceLoadType.LoadType != null) {
                        total += realDeviceLoadType.LoadType.LoadTypeWeight * realDeviceLoadType.MaxPower;
                    }
                }
                return (decimal) total;
            }
        }

        [UsedImplicitly]
        public int Year {
            get => _year;
            set => SetValueWithNotify(value, ref _year, nameof(Year));
        }

        public AssignableDeviceType AssignableDeviceType => AssignableDeviceType.Device;

        public List<Tuple<VLoadType, double>> CalculateAverageEnergyUse([CanBeNull]VLoadType dstLoadType,
            ObservableCollection<DeviceAction> allActions,[CanBeNull] TimeBasedProfile timeProfile, double multiplier,
            double probability)
        {
            var result = new List<Tuple<VLoadType, double>>();
            if (timeProfile == null) {
                throw new LPGException("Time profile was null");
            }
            if (dstLoadType == null)
            {
                throw new LPGException("dstLoadType was null");
            }
            var load = _loads.FirstOrDefault(myload => myload.LoadType == dstLoadType);
            if (load != null) {
                switch (timeProfile.TimeProfileType) {
                    case TimeProfileType.Relative: {
                        var energy = load.MaxPower * timeProfile.CalculateSecondsPercent() *
                                     dstLoadType.ConversionFaktorPowerToSum * multiplier * probability;
                        result.Add(new Tuple<VLoadType, double>(load.LoadType, energy));
                    }
                        break;
                    case TimeProfileType.Absolute: {
                        var energy = timeProfile.CalculateSecondsSum() * dstLoadType.ConversionFaktorPowerToSum *
                                     multiplier * probability;
                        result.Add(new Tuple<VLoadType, double>(load.LoadType, energy));
                    }
                        break;
                    default:
                        throw new LPGException("Forgotten TimeProfileType. Please report!");
                }
            }
            return result;
        }

        public List<RealDevice> GetRealDevices(ObservableCollection<DeviceAction> allActions)
        {
            var devices = new List<RealDevice>
            {
                this
            };
            return devices;
        }

        public bool IsOrContainsStandbyDevice(ObservableCollection<DeviceAction> allActions) => IsStandbyDevice;

        public void AddLoad([NotNull] VLoadType loadType, double maxpower, double standardDeviation,
            double averageYearlyConsumption)
        {
            // delete old values for the same load type for the update function
            for (var i = 0; i < _loads.Count; i++) {
                if (_loads[i].LoadType == loadType) {
                    _loads[i].DeleteFromDB();
                    var i1 = i;
                    Logger.Get().SafeExecuteWithWait(() => _loads.RemoveAt(i1));
                    i = _loads.Count + 1;
                }
            }
            var load = new RealDeviceLoadType(loadType.Name, IntID, loadType.IntID, maxpower, loadType,
                standardDeviation, averageYearlyConsumption, ConnectionString, System.Guid.NewGuid().ToString());
            Logger.Get().SafeExecuteWithWait(() => {
                _loads.Add(load);
                load.SaveToDB();
                OnPropertyChanged(nameof(WeightedEnergyIntensity));
            });
        }

        [NotNull]
        private static RealDevice AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name","(no name)");
            var year = dr.GetInt("year", false, 0);
            var picture = dr.GetString("Picture", false);
            var deviceCategoryID = dr.GetInt("DeviceCategoryID", false);
            var forceAllLoadTypesToBeSet = dr.GetBool("ForceAllLoadTypesToBeSet", false, true, ignoreMissingFields);
            var isStandbyDevice = dr.GetBool("IsStandbyDevice", false, false, ignoreMissingFields);
            var description = dr.GetString("Description", false, string.Empty, ignoreMissingFields);
            var dc = aic.DeviceCategories.FirstOrDefault(category => category.ID == deviceCategoryID);
            if (dc == null) {
                dc = _noneCategory;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var db = new RealDevice(name, year, picture, dc, description, forceAllLoadTypesToBeSet,
                isStandbyDevice, connectionString, guid,id);
            return db;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
        {
            const string s = "New Device ";
            var i = 1;
            while (isNameTaken(s + i)) {
                i++;
            }
            var db = new RealDevice(s + i, DateTime.Now.Year, string.Empty, null, string.Empty, true, false,
                connectionString, System.Guid.NewGuid().ToString());
            return db;
        }

        public override void DeleteFromDB()
        {
            foreach (var realDeviceLoadType in _loads) {
                realDeviceLoadType.DeleteFromDB();
            }
            base.DeleteFromDB();
        }

        public void DeleteLoad([NotNull] RealDeviceLoadType rdlt)
        {
            _loads.Remove(rdlt);
            rdlt.DeleteFromDB();
            OnPropertyChanged(nameof(WeightedEnergyIntensity));
        }

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport,  [NotNull] Simulator dstSim)
            => ImportFromItem((RealDevice)toImport,dstSim);

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override List<UsedIn> CalculateUsedIns([NotNull] Simulator sim)
        {
            List<UsedIn> usedIns = new List<UsedIn>();
            foreach (var affordance in sim.Affordances.It) {
                foreach (var dev in affordance.AffordanceDevices) {
                    if (dev.Device == this) {
                        usedIns.Add(new UsedIn(affordance,
                            "Affordance, Loadtype:" + dev.LoadType + ", Time Profile:" + dev.TimeProfile));
                    }
                    if (dev.Device == DeviceCategory) {
                        usedIns.Add(new UsedIn(affordance,
                            "Affordance indirect, Device Category " + DeviceCategory?.Name + " Loadtype:" +
                            dev.LoadType +
                            ", Time Profile:" + dev.TimeProfile));
                    }
                }
            }
            foreach (var trait in sim.HouseholdTraits.It) {
                foreach (var autodev in trait.Autodevs) {
                    if (autodev.Device == this) {
                        usedIns.Add(new UsedIn(trait, "Trait autonomous device"));
                    }
                    if (autodev.Device == DeviceCategory) {
                        usedIns.Add(new UsedIn(trait,
                            "Trait autonomous device indirect, Device Category " + DeviceCategory?.Name));
                    }
                }
            }

            foreach (var ht in sim.HouseTypes.It) {
                foreach (var autodev in ht.HouseDevices) {
                    if (autodev.Device == this) {
                        usedIns.Add(new UsedIn(ht, "Housetype autonomous device"));
                    }
                    if (autodev.Device == DeviceCategory) {
                        usedIns.Add(new UsedIn(ht,
                            "Housetype autonomous device indirect, Device Category " + DeviceCategory?.Name));
                    }
                }
            }
            foreach (var da in sim.DeviceActions.It) {
                if (da.Device == this) {
                    usedIns.Add(new UsedIn(da, "Device Action"));
                    var list = da.CalculateUsedIns(sim);
                    foreach (var usedIn in list) {
                        usedIns.Add(new UsedIn(usedIn.Item, "By Device Action - " + usedIn.Item));
                    }
                }
            }

            return usedIns;
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var outcome in Loads)
            {
                outcome.SaveToDB();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] RealDevice item,[NotNull] Simulator dstSim)
        {
            var dstdc = GetItemFromListByName(dstSim.DeviceCategories.MyItems, item.DeviceCategory?.Name);
            var rd = new RealDevice(item.Name, item.Year, item.Picture, dstdc, item.Description,
                item.ForceAllLoadTypesToBeSet, item.IsStandbyDevice,
                dstSim.ConnectionString, item.Guid);
            rd.SaveToDB();
            foreach (var rdlt in item.Loads) {
                var newlt = GetItemFromListByName(dstSim.LoadTypes.MyItems, rdlt.LoadType?.Name);
                if (newlt == null) {
                    Logger.Error("Could not find a load type while importing. Skipping");
                    continue;
                }
                rd.AddLoad(newlt, rdlt.MaxPower, rdlt.StandardDeviation, rdlt.AverageYearlyConsumption);
            }
            return rd;
        }

        public void ImportFromOtherDevice([CanBeNull] RealDevice other)
        {
            if (other == null) {
                return;
            }
            Year = other.Year;
            Picture = other.Picture;
            DeviceCategory = other.DeviceCategory;
            Description = other.Description;
            ForceAllLoadTypesToBeSet = other.ForceAllLoadTypesToBeSet;
            IsStandbyDevice = other.IsStandbyDevice;
            foreach (var load in other.Loads) {
                if(load.LoadType != null) {
                    AddLoad(load.LoadType, load.MaxPower, load.StandardDeviation, load.AverageYearlyConsumption);
                }
            }
        }

        private static bool IsCorrectParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (RealDeviceLoadType) child;
            if (parent.ID == hd.RealDeviceID) {
                var realDevice = (RealDevice) parent;
                realDevice.Loads.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<RealDevice> result,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceCategory> deviceCategories, [NotNull] DeviceCategory noneCategory, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes, [ItemNotNull] [NotNull] ObservableCollection<TimeBasedProfile> timeBasedProfiles,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(deviceCategories: deviceCategories, loadTypes: loadTypes,
                timeProfiles: timeBasedProfiles);
            _noneCategory = noneCategory;
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var realDeviceLoadTypes =
                new ObservableCollection<RealDeviceLoadType>();
            RealDeviceLoadType.LoadFromDatabase(realDeviceLoadTypes, connectionString, loadTypes, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(realDeviceLoadTypes), IsCorrectParent,
                ignoreMissingTables);
        }

        [NotNull]
        public DeviceAction MakeDeviceAction([NotNull] Simulator sim)
        {
            var action = sim.DeviceActions.CreateNewItem(sim.ConnectionString);
            action.Device = this;
            action.Description = Name;
            action.Name = "run " + Name;
            action.SaveToDB();
            var tp = sim.Timeprofiles.FindByName("placeholder", FindMode.Partial);
            if (tp == null) {
                Logger.Error(
                    "The placeholder time profile could not be found. The action could not be created correctly. Please fix.");
                return action;
            }
            foreach (var load in Loads) {
                if(load.LoadType != null) {
                    action.AddDeviceProfile(tp, 0, load.LoadType, 1);
                }
            }
            return action;
        }

        private void OnPropertyChanged([NotNull] object sender, [NotNull] PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Name) ||
                propertyChangedEventArgs.PropertyName == nameof(WeightedEnergyIntensity)) {
                OnPropertyChanged(nameof(NameWithEnergyIntensity));
            }
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Year", "@Year", Year);
            cmd.AddParameter("Picture", "@Picture", Picture);
            cmd.AddParameter("Description", _description);
            cmd.AddParameter("ForceAllLoadTypesToBeSet", _forceAllLoadTypesToBeSet);
            cmd.AddParameter("IsStandbyDevice", _isStandbyDevice);
            if (_deviceCategory != null) {
                cmd.AddParameter("DeviceCategoryID", "@DeviceCategoryID", _deviceCategory.IntID);
            }
        }
    }
}