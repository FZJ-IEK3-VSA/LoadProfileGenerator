#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    public class AffordanceDevice : DBBase, IComparable<AffordanceDevice>, IDeviceSelection {
        public const string TableName = "tblAffordanceDeviceTimeprofile";
        [CanBeNull]
        private readonly int? _affordanceID;

        [CanBeNull] private readonly IAssignableDevice _assignableDevice;

        [CanBeNull] private readonly VLoadType _loadType;

        private readonly double _probability;
        private readonly decimal _timeOffset;

        [CanBeNull] private readonly TimeBasedProfile _timeProfile;

        public AffordanceDevice([CanBeNull] IAssignableDevice assignableDevice,
            [CanBeNull] TimeBasedProfile timeProfile, [CanBeNull] int? id, decimal timeOffset, [CanBeNull] int? affordanceID,
            [ItemNotNull] [NotNull] ObservableCollection<RealDevice> simdevices, [ItemNotNull] [NotNull] ObservableCollection<DeviceCategory> simdevcategories,
            [NotNull] string deviceName, [CanBeNull] VLoadType loadType, [NotNull] string connectionString, double probability, StrGuid guid) : base(
            deviceName, TableName, connectionString, guid) {
            _assignableDevice = assignableDevice;
            _timeProfile = timeProfile;
            ID = id;
            _timeOffset = timeOffset;
            _affordanceID = affordanceID;

            _loadType = loadType;
            if (!Config.IsInUnitTesting) {
                simdevices.CollectionChanged += SimdevicesOnCollectionChanged;
            }
            if (!Config.IsInUnitTesting) {
                simdevcategories.CollectionChanged += SimdevicesOnCollectionChanged;
            }
            TypeDescription = "Affordance Device";
            if (_timeOffset < 0) {
                _timeOffset = 0;
            }
            _probability = probability;
        }

        [CanBeNull]
        [UsedImplicitly]
        public Affordance ParentAffordance { get; set; }

        [UsedImplicitly]
        public decimal TimeOffset => _timeOffset;

        [NotNull]
        [UsedImplicitly]
        public string TimeOffsetString => _timeOffset.ToString("0.00", CultureInfo.CurrentCulture);

        public int CompareTo([CanBeNull] AffordanceDevice other) {
            if (other == null) {
                return 0;
            }
            if (TimeOffset != other.TimeOffset) {
                return decimal.Compare(TimeOffset, other.TimeOffset);
            }
            if (other.Name != Name) {
                return string.CompareOrdinal(Name, other.Name);
            }
            if (TimeProfile != other.TimeProfile && TimeProfile != null && other.TimeProfile != null) {
                return TimeProfile.CompareTo(other.TimeProfile);
            }
            if (LoadType != null && other.LoadType != null && LoadType != other.LoadType) {
                return LoadType.CompareTo(other.LoadType);
            }

            return _probability.CompareTo(other.Probability);
        }

        [UsedImplicitly]
        public IAssignableDevice Device => _assignableDevice;

        public VLoadType LoadType => _loadType;

        public double Probability => _probability;

        [UsedImplicitly]
        public TimeBasedProfile TimeProfile => _timeProfile;

        [NotNull]
        private static AffordanceDevice AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var affordanceID = dr.GetInt("AffordanceID", false, -1, ignoreMissingFields);
            var deviceID =  dr.GetInt("DeviceID");

            var timeprofileID = dr.GetNullableIntFromLongOrInt("TimeprofileID", false, ignoreMissingFields);
            var timeOffset = dr.GetDecimal("TimeOffset");
            var adt =
                (AssignableDeviceType) dr.GetInt("AssignableDeviceType", false, (int) AssignableDeviceType.Device,
                    true);
            var vloadtypeID = dr.GetNullableIntFromLong("VLoadTypeID", false, ignoreMissingFields);
            var vlt = aic.LoadTypes.FirstOrDefault(lt => lt.ID == vloadtypeID);
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
                default: throw new LPGException("unknown device type");
            }
            TimeBasedProfile tp = null;
            if (timeprofileID != null) {
                tp = aic.TimeProfiles.FirstOrDefault(tpt => tpt.ID == timeprofileID);
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
            var probability = dr.GetDouble("Probability", false, 1, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var tup = new AffordanceDevice(device, tp, id, timeOffset, affid, aic.RealDevices,
                aic.DeviceCategories, deviceName, vlt, connectionString, probability, guid)
            {
                ParentAffordance = aff
            };
            return tup;
        }

        public override int CompareTo(BasicElement other) {
            if (_assignableDevice == null) {
                return 0;
            }
            if (other is AffordanceDevice otheraff)
            {
                return _assignableDevice.CompareTo(otheraff._assignableDevice);
            }
            return base.CompareTo(other);
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_assignableDevice == null) {
                message = "Device not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<AffordanceDevice> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceCategory> deviceCategories, [ItemNotNull] [NotNull] ObservableCollection<RealDevice> realDevices,
            [ItemNotNull] [NotNull] ObservableCollection<TimeBasedProfile> timeBasedProfiles, [ItemNotNull] [NotNull] ObservableCollection<Affordance> affordances,
            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes, [ItemNotNull] [NotNull] ObservableCollection<DeviceAction> deviceActions,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups, bool ignoreMissingTables) {
            var aic = new AllItemCollections(deviceCategories: deviceCategories,
                realDevices: realDevices, timeProfiles: timeBasedProfiles, affordances: affordances,
                loadTypes: loadTypes, deviceActions: deviceActions, deviceActionGroups: deviceActionGroups);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_affordanceID != null) {
                cmd.AddParameter("AffordanceID", "@AffordanceID", _affordanceID);
            }
            if (_assignableDevice != null) {
                cmd.AddParameter("DeviceID", "@DeviceID", _assignableDevice.IntID);
                cmd.AddParameter("AssignableDeviceType", "@AssignableDeviceType",
                    _assignableDevice.AssignableDeviceType);
            }
            if (_timeProfile != null) {
                cmd.AddParameter("TimeprofileID", "@TimeprofileID", _timeProfile.IntID);
            }
            cmd.AddParameter("TimeOffset", "@TimeOffset", _timeOffset);
            if (_loadType != null) {
                cmd.AddParameter("VLoadTypeID", _loadType.IntID);
            }
            cmd.AddParameter("Probability", _probability);
        }

        private void SimdevicesOnCollectionChanged([NotNull] object sender,
            [NotNull] NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove &&
                notifyCollectionChangedEventArgs.OldItems != null &&
                notifyCollectionChangedEventArgs.OldItems[0] == _assignableDevice) {
                ParentAffordance?.DeleteDeviceFromDB(this);
            }
        }
    }
}