using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Common;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public class DeviceActionGroup : DBBaseElement, IAssignableDevice, IComparable<DeviceActionGroup> {
        public const string TableName = "tblDeviceActionGroups";
        [NotNull] private string _description;

        public DeviceActionGroup([NotNull] string pName, [NotNull] string connectionString, [NotNull] string description, StrGuid guid, [CanBeNull]int? pID = null)
            : base(pName, pID, TableName, connectionString, guid) {
            _description = description;
            TypeDescription = "Device Action Group";
        }

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        public AssignableDeviceType AssignableDeviceType => AssignableDeviceType.DeviceActionGroup;

        public List<Tuple<VLoadType, double>> CalculateAverageEnergyUse(VLoadType dstLoadType,
            ObservableCollection<DeviceAction> allActions, TimeBasedProfile timeProfile, double multiplier,
            double probability) {
            // time profile is not needed here
            var actions = GetDeviceActions(allActions);
            if (actions.Count == 0) {
                return new List<Tuple<VLoadType, double>>();
            }
            var sums = new Dictionary<VLoadType, double>();
            var counts = new Dictionary<VLoadType, int>();
            foreach (var deviceAction in actions) {
                var result = deviceAction.CalculateAverageEnergyUse(dstLoadType, allActions,
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

        public List<RealDevice> GetRealDevices(ObservableCollection<DeviceAction> allActions) {
            var actions = GetDeviceActions(allActions);
            var devices = new List<RealDevice>();
            foreach (var action in actions) {
                devices.Add(action.Device);
            }
            return devices;
        }

        public bool IsOrContainsStandbyDevice(ObservableCollection<DeviceAction> allActions) {
            var list = GetDeviceActions(allActions);
            foreach (var action in list) {
                if (action.IsOrContainsStandbyDevice(allActions)) {
                    return true;
                }
            }
            return false;
        }

        public int CompareTo([CanBeNull] DeviceActionGroup other) {
            if (other == null) {
                return 0;
            }
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        [NotNull]
        private static DeviceActionGroup AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLongOrInt("ID");
            var description = dr.GetString("Description");
            var name = dr.GetString("Name");
            var guid = GetGuid(dr, ignoreMissingFields);
            var db = new DeviceActionGroup(name, connectionString, description,guid, id);
            return db;
        }

        internal static TimeSpan CalculateMinimumTimespan() {
            // List<DeviceAction> deviceActions = GetDeviceActions(das);
            var ts = TimeSpan.MaxValue;
            //foreach (DeviceAction action in deviceActions)
            if (DeviceAction.CalculateMinimumTimespan() < ts) {
                ts = DeviceAction.CalculateMinimumTimespan();
            }
            return ts;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) {
            var dc = new DeviceActionGroup(FindNewName(isNameTaken, "New Device Action Group "),
                connectionString, "(no description)", System.Guid.NewGuid().ToStrGuid());
            return dc;
        }

        [ItemNotNull]
        [NotNull]
        public List<DeviceAction> GetDeviceActions([ItemNotNull] [CanBeNull] ObservableCollection<DeviceAction> allActions) {
            if (allActions == null) {
                return new List<DeviceAction>();
            }
            var result = allActions.Where(a => a.DeviceActionGroup == this);
            var da = new List<DeviceAction>(result);
            return da;
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((DeviceActionGroup)toImport,dstSim);

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override List<UsedIn> CalculateUsedIns(Simulator sim) {
            var used = new List<UsedIn>();
            foreach (var deviceAction in sim.DeviceActions.Items) {
                if (deviceAction.DeviceActionGroup == this) {
                    List<TimeSpan?> timespans = deviceAction.Profiles.Select(x => x.Timeprofile?.Duration).ToList();
                    var information = string.Empty;
                    if (timespans.Count > 0) {
                        var min = timespans.Min();
                        var max = timespans.Max();
                        information = min + " - " + max;
                    }
                    used.Add(new UsedIn(deviceAction, "Device Action", information));
                }
            }
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
                foreach (var autodev in hht.Autodevs) {
                    if (autodev.Device == this) {
                        used.Add(new UsedIn(hht, "Autonomous device in Trait"));
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
            return used;
        }

        [NotNull]
        [UsedImplicitly]
        public static DeviceActionGroup ImportFromItem([NotNull] DeviceActionGroup toImport, [NotNull] Simulator dstSim)
        {
            var dc = new DeviceActionGroup(toImport.Name, dstSim.ConnectionString,
                toImport.Description, toImport.Guid);
            dc.SaveToDB();
            return dc;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            message = "";
            return true;
        }

        internal static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DeviceActionGroup> result, [NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            result.Sort();
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", _description);
        }
    }
}