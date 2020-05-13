using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public class AffordanceStandby : DBBase, IComparable<AffordanceStandby>, IDeviceSelection {
        public const string TableName = "tblAffordanceStandby";
        [CanBeNull]
        private readonly int? _affordanceID;

        [CanBeNull] private readonly IAssignableDevice _assignableDevice;

        public AffordanceStandby([CanBeNull] IAssignableDevice assignableDevice, [CanBeNull]int? id, [CanBeNull]int? affordanceID,
            [NotNull] string connectionString,
            [NotNull] string deviceName, [NotNull] StrGuid guid) : base(deviceName, TableName, connectionString, guid) {
            _assignableDevice = assignableDevice;
            ID = id;
            _affordanceID = affordanceID;
            TypeDescription = "Affordance Standby Device";
        }
        [CanBeNull]
        public int? AffordanceID => _affordanceID;

        [UsedImplicitly]
        public AssignableDeviceType AssignableDeviceType {
            get {
                if (Device != null) {
                    return Device.AssignableDeviceType;
                }
                return AssignableDeviceType.Device;
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public Affordance ParentAffordance { get; set; }

        [NotNull]
        [UsedImplicitly]
        public string StrKey => Name;

        public int CompareTo([CanBeNull] AffordanceStandby other) {
            if (Device == null || other?.Device == null) {
                return 0;
            }
            if (other.Name != Name) {
                return string.CompareOrdinal(Name, other.Name);
            }
            return string.Compare(Device.Name, other.Device.Name, StringComparison.Ordinal);
        }

        [UsedImplicitly]
        [CanBeNull]
        public IAssignableDevice Device => _assignableDevice;

        [CanBeNull]
        public VLoadType LoadType => null;

        public double Probability => 0;

        [CanBeNull]
        public TimeBasedProfile TimeProfile => null;

        [NotNull]
        private static AffordanceStandby AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID", true, ignoreMissingFields, -1);
            var affordanceID = dr.GetIntFromLong("AffordanceID", false, ignoreMissingFields, -1);
            var deviceID = dr.GetIntFromLong("DeviceID", false, ignoreMissingFields, -1);
            var adt =
                (AssignableDeviceType) dr.GetIntFromLong("AssignableDeviceType", false, ignoreMissingFields);

            IAssignableDevice device;
            switch (adt) {
                case AssignableDeviceType.DeviceCategory:
                    device = aic.DeviceCategories.FirstOrDefault(dc => dc.ID == deviceID);
                    break;
                case AssignableDeviceType.Device:
                    device = aic.RealDevices.FirstOrDefault(rd => rd.ID == deviceID);
                    break;
                case AssignableDeviceType.DeviceAction:
                    device = aic.DeviceActions.FirstOrDefault(rd => rd.ID == deviceID);
                    break;
                case AssignableDeviceType.DeviceActionGroup:
                    device = aic.DeviceActionGroups.FirstOrDefault(rd => rd.ID == deviceID);
                    break;
                default:
                    throw new LPGException("unknown device type");
            }

            var aff = aic.Affordances.FirstOrDefault(affordance => affordance.ID == affordanceID);
            var deviceName = "(no device)";
            if (device != null) {
                deviceName = device.Name;
            }
            int? affid = null;
            if (aff != null) {
                affid = aff.IntID;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var tup = new AffordanceStandby(device, id, affid, connectionString, deviceName, guid)
            {
                ParentAffordance = aff
            };
            return tup;
        }

        public override int CompareTo([CanBeNull] BasicElement other) {
            if (_assignableDevice == null) {
                return 0;
            }
            if (other is AffordanceStandby otheraff)
            {
                return _assignableDevice.CompareTo(otheraff._assignableDevice);
            }
            return base.CompareTo(other);
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_assignableDevice == null) {
                message = "Device not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<AffordanceStandby> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceCategory> deviceCategories, [ItemNotNull] [NotNull] ObservableCollection<RealDevice> realDevices,
            [ItemNotNull] [NotNull] ObservableCollection<Affordance> affordances, [ItemNotNull] [NotNull] ObservableCollection<DeviceAction> deviceActions,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups, bool ignoreMissingTables) {
            var aic = new AllItemCollections(deviceCategories: deviceCategories, realDevices: realDevices,
                affordances: affordances, deviceActions: deviceActions, deviceActionGroups: deviceActionGroups);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            if (_affordanceID != null) {
                cmd.AddParameter("AffordanceID", "@AffordanceID", _affordanceID);
            }
            if (_assignableDevice != null) {
                cmd.AddParameter("DeviceID", "@DeviceID", _assignableDevice.IntID);
                cmd.AddParameter("AssignableDeviceType", "@AssignableDeviceType",
                    _assignableDevice.AssignableDeviceType);
            }
        }
    }
}