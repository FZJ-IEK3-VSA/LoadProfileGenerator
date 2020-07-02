using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public class DeviceAction : DBBaseElement, IAssignableDevice {
        public const string TableName = "tblDeviceActions";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<DeviceActionProfile> _profiles;
        [NotNull] private string _description;

        [CanBeNull] private RealDevice _device;

        [CanBeNull] private DeviceActionGroup _deviceActionGroup;

        public DeviceAction([NotNull] string pName, [CanBeNull]int? id, [NotNull] string description, [NotNull] string connectionString,
            [CanBeNull] DeviceActionGroup deviceActionGroup, [CanBeNull] RealDevice device, StrGuid guid) : base(pName, TableName,
            connectionString, guid) {
            ID = id;
            _profiles = new ObservableCollection<DeviceActionProfile>();
            _description = description;
            TypeDescription = "Device Action";
            _deviceActionGroup = deviceActionGroup;
            _device = device;
        }

        public int AbsoluteProfileCount {
            get {
                var absoluteCount = 0;
                foreach (var dap in _profiles) {
                    if (dap.Timeprofile != null && dap.Timeprofile.TimeProfileType == TimeProfileType.Absolute) {
                        absoluteCount++;
                    }
                }
                return absoluteCount;
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [CanBeNull]
        [UsedImplicitly]
        public RealDevice Device {
            get => _device;
            set => SetValueWithNotify(value, ref _device,false, nameof(Device));
        }

        [CanBeNull]
        [UsedImplicitly]
        public DeviceActionGroup DeviceActionGroup {
            get => _deviceActionGroup;
            set => SetValueWithNotify(value, ref _deviceActionGroup,false, nameof(DeviceActionGroup));
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceActionProfile> Profiles => _profiles;

        public AssignableDeviceType AssignableDeviceType => AssignableDeviceType.DeviceAction;

        public List<Tuple<VLoadType, double>> CalculateAverageEnergyUse(VLoadType dstLoadType,
            ObservableCollection<DeviceAction> allActions, TimeBasedProfile timeProfile, double multiplier,
            double probability) {
            // timeprofile is not needed here
            var results = new List<Tuple<VLoadType, double>>();
            foreach (var deviceActionProfile in _profiles) {
                if (_device != null) {
                    results.AddRange(_device.CalculateAverageEnergyUse(deviceActionProfile.VLoadType, allActions,
                        deviceActionProfile.Timeprofile, deviceActionProfile.Multiplier * multiplier, probability));
                }
            }
            return results;
        }

        public List<RealDevice> GetRealDevices(ObservableCollection<DeviceAction> allActions) {
            return new List<RealDevice>
            {
                _device
            };
        }

        public bool IsOrContainsStandbyDevice(ObservableCollection<DeviceAction> allActions) {
            if (_device != null) {
                return _device.IsStandbyDevice;
            }
            return false;
        }

        public void AddDeviceProfile([NotNull] TimeBasedProfile tp, decimal timeoffset, [NotNull] VLoadType vLoadType, double multiplier) {
            var deviceName = vLoadType.Name;
            var dad = new DeviceActionProfile(tp, null, timeoffset, IntID, deviceName, vLoadType,
                ConnectionString, multiplier,System.Guid.NewGuid().ToStrGuid());
            Logger.Get().SafeExecuteWithWait(() => {
                _profiles.Add(dad);
                _profiles.Sort();
            });
            dad.SaveToDB();
        }

        [NotNull]
        private static DeviceAction AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var description = dr.GetString("Description", false, string.Empty, ignoreMissingFields);
            var deviceActionGroupID = dr.GetNullableIntFromLong("DeviceActionGroupID", false, ignoreMissingFields);
            DeviceActionGroup devActionGroup = null;
            if (deviceActionGroupID != null) {
                devActionGroup = aic.DeviceActionGroups.FirstOrDefault(devid => devid.ID == deviceActionGroupID);
            }

            var deviceID = dr.GetNullableIntFromLong("DeviceID", false, ignoreMissingFields);
            RealDevice realDevice = null;
            if (deviceID != null) {
                realDevice = aic.RealDevices.FirstOrDefault(devid => devid.ID == deviceID);
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var aff = new DeviceAction(name, id, description, connectionString, devActionGroup, realDevice, guid);
            return aff;
        }

        public static TimeSpan CalculateMinimumTimespan() {
            var ts = TimeSpan.MaxValue;
            // foreach (DeviceActionProfile actionProfile in _profiles)
            if (TimeBasedProfile.CalculateMinimumTimespan() < ts) {
                ts = TimeBasedProfile.CalculateMinimumTimespan();
            }
            return ts;
        }

        public double CalculateWeightedEnergyUse() {
            if (_device == null) {
                Logger.Error("Tried to calculate the weighted energy use for a device action without a device.");
                return 0;
            }
            double result = 0;
            foreach (var deviceActionProfile in _profiles) {
                var lt = deviceActionProfile.VLoadType;
                if(lt == null) {
                    throw new LPGException("loadtype was null");
                }
                var rdload = _device.Loads.FirstOrDefault(load => load.LoadType == lt);
                if (rdload == null) {
                    throw new DataIntegrityException(
                        "Device action " + Name + " has the load type " + lt.Name +
                        " set, even though the device has it not. Please fix.", this);
                }
                if(deviceActionProfile.Timeprofile == null) {
                    throw new LPGException("Timeprofile was null");
                }
                var totalfactor = deviceActionProfile.Timeprofile.CalculateSecondsPercent();
                result = lt.LoadTypeWeight * totalfactor;
            }
            return result;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) {
            var aff = new DeviceAction(FindNewName(isNameTaken, "New Device Action "), null, string.Empty,
                connectionString, null, null, System.Guid.NewGuid().ToStrGuid());
            return aff;
        }

        public override void DeleteFromDB() {
            if (ID != null) {
                foreach (var dad in _profiles) {
                    dad.DeleteFromDB();
                }
                base.DeleteFromDB();
            }
        }

        public void DeleteProfileFromDB([NotNull] DeviceActionProfile dad) {
            dad.DeleteFromDB();
            _profiles.Remove(dad);
        }

        /*
         *  public List<UsedIn> GetUsedIns(IEnumerable<Household> households,
           IEnumerable<HouseholdTrait> householdTraits,
            )
         * */

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((DeviceAction)toImport,dstSim);

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Performance", "CC0039:Don't concatenate strings in loops", Justification = "<Pending>")]
        public override List<UsedIn> CalculateUsedIns(Simulator sim) {
            var used = new List<UsedIn>();
            foreach (var affordance in sim.Affordances.Items) {
                foreach (var affordanceDevice in affordance.AffordanceDevices) {
                    if (affordanceDevice.Device == this) {
                        used.Add(new UsedIn(affordance, "Affordance"));
                    }
                }
                foreach (var standby in affordance.AffordanceStandbys) {
                    if (standby.Device == this) {
                        used.Add(new UsedIn(affordance, "Affordance - Standby Requirement"));
                    }
                }
            }

            foreach (var hht in sim.HouseholdTraits.Items) {
                foreach (var hhtloc in hht.Autodevs) {
                    if (hhtloc.Device == this) {
                        used.Add(new UsedIn(hht, "Household Trait"));
                    }
                }
            }
            foreach (var housetype in sim.HouseTypes.Items) {
                foreach (var hhdev in housetype.HouseDevices) {
                    if (hhdev.Device == this) {
                        used.Add(new UsedIn(housetype, "House Type"));
                    }
                }
            }
            if (_deviceActionGroup != null) {
                var others =
                    _deviceActionGroup.CalculateUsedIns(sim);
                foreach (var usedIn in others) {
                    if (usedIn.Item.GetType() != typeof(DeviceAction)) {
                        usedIn.TypeDescription += " (by group)";
                        used.Add(usedIn);
                    }
                }
            }
            return used;
        }

        [NotNull]
        [UsedImplicitly]
        public static DeviceAction ImportFromItem([NotNull] DeviceAction toImport,  [NotNull] Simulator dstSim) {
            var rd = GetItemFromListByName(dstSim.RealDevices.Items, toImport.Device?.Name);
            DeviceActionGroup group = null;
            if (toImport.DeviceActionGroup != null) {
                group = GetItemFromListByName(dstSim.DeviceActionGroups.Items, toImport.DeviceActionGroup.Name);
            }
            var da = new DeviceAction(toImport.Name, null, toImport.Description, dstSim.ConnectionString, group, rd, System.Guid.NewGuid().ToStrGuid());
            da.SaveToDB();

            foreach (var daProfile in toImport.Profiles) {
                var tp = GetItemFromListByName(dstSim.Timeprofiles.Items, daProfile.Timeprofile?.Name);
                var vlt = GetItemFromListByName(dstSim.LoadTypes.Items, daProfile.VLoadType?.Name);
                if (tp == null) {
                    Logger.Error("Could not find a time profile while importing. Skipping");
                    continue;
                }
                if (vlt == null) {
                    Logger.Error("Could not find a load tpye while importing. Skipping");
                    continue;
                }
                da.AddDeviceProfile(tp, daProfile.TimeOffset, vlt, daProfile.Multiplier);
            }
            return da;
        }

        private static bool IsCorrectParent([NotNull] DBBase parent, [NotNull] DBBase child) {
            var hd = (DeviceActionProfile) child;
            if (parent.ID == hd.DeviceActionID) {
                var aff = (DeviceAction) parent;
                aff.Profiles.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DeviceAction> result, [NotNull] string connectionString,
            [NotNull] [ItemNotNull] ObservableCollection<TimeBasedProfile> pTimeProfiles, [NotNull] [ItemNotNull] ObservableCollection<RealDevice> devices,
            [NotNull] [ItemNotNull] ObservableCollection<VLoadType> loadTypes, [NotNull] [ItemNotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections(timeProfiles: pTimeProfiles, realDevices: devices,
                loadTypes: loadTypes, deviceActionGroups: deviceActionGroups);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var profiles = new ObservableCollection<DeviceActionProfile>();
            DeviceActionProfile.LoadFromDatabase(profiles, connectionString, result, pTimeProfiles, loadTypes,
                ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(profiles), IsCorrectParent, ignoreMissingTables);

            foreach (var da in result) {
                da.Profiles.Sort();
            }
        }

        public override void SaveToDB() {
            base.SaveToDB();
            foreach (var profile in _profiles) {
                profile.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", _description);
            if (_device != null) {
                cmd.AddParameter("DeviceID", _device.IntID);
            }
            if (_deviceActionGroup != null) {
                cmd.AddParameter("DeviceActionGroupID", _deviceActionGroup.IntID);
            }
        }

        public override string ToString() {
            var devicename = string.Empty;
            if (_device != null) {
                devicename = " (Device " + _device.Name + ")";
            }
            return Name + devicename;
        }
    }
}