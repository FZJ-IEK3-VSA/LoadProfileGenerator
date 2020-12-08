using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Automation;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public class DeviceActionProfile : DBBase, IComparable<DeviceActionProfile> {
        public const string TableName = "tblDeviceActionDevices";
        private readonly int _deviceActionID;
        private readonly double _multiplier;
        private readonly decimal _timeOffset;

        [CanBeNull] private readonly TimeBasedProfile _timeprofile;

        [CanBeNull] private readonly VLoadType _vLoadType;

        public DeviceActionProfile([CanBeNull] TimeBasedProfile timeprofile, [CanBeNull] int? id, decimal timeOffset,
            int deviceActionID,
            [JetBrains.Annotations.NotNull] string deviceName, [CanBeNull] VLoadType vLoadType, [JetBrains.Annotations.NotNull] string connectionString, double multiplier, StrGuid guid)
            : base(deviceName, TableName, connectionString, guid) {
            _deviceActionID = deviceActionID;
            _timeprofile = timeprofile;
            ID = id;
            _timeOffset = timeOffset;

            _vLoadType = vLoadType;
            TypeDescription = "Device Action Device";
            if (_timeOffset < 0) {
                _timeOffset = 0;
            }
            _multiplier = multiplier;
        }

        [UsedImplicitly]
        public int DeviceActionID => _deviceActionID;

        public double Multiplier => _multiplier;

        [UsedImplicitly]
        public decimal TimeOffset => _timeOffset;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string TimeOffsetString => _timeOffset.ToString("0.00", CultureInfo.CurrentCulture);

        [UsedImplicitly]
        [CanBeNull]
        public TimeBasedProfile Timeprofile => _timeprofile;
        [CanBeNull]
        public VLoadType VLoadType => _vLoadType;

        public int CompareTo([CanBeNull] DeviceActionProfile other) {
            if (other == null) {
                return 0;
            }
            if (TimeOffset != other.TimeOffset) {
                return decimal.Compare(TimeOffset, other.TimeOffset);
            }
            if (other.Name != Name) {
                return string.CompareOrdinal(Name, other.Name);
            }
            if (Timeprofile != other.Timeprofile && Timeprofile != null && other.Timeprofile != null) {
                return Timeprofile.CompareTo(other.Timeprofile);
            }
            if (VLoadType != null && other.VLoadType != null && VLoadType != other.VLoadType) {
                return VLoadType.CompareTo(other.VLoadType);
            }
            return 0;
        }

        [JetBrains.Annotations.NotNull]
        private static DeviceActionProfile AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var deviceActionID = dr.GetIntFromLong("DeviceActionID", false, ignoreMissingFields, -1);
            var timeprofileID = dr.GetIntFromLong("TimeprofileID");
            var timeOffset = (decimal) dr.GetDouble("TimeOffset");
            var vloadtypeID = dr.GetNullableIntFromLong("LoadTypeID", false, ignoreMissingFields);
            var vlt = aic.LoadTypes.FirstOrDefault(lt => lt.ID == vloadtypeID);
            var tp = aic.TimeProfiles.FirstOrDefault(tpt => tpt.ID == timeprofileID);
            var multiplier = dr.GetDouble("Multiplier", false, 1, ignoreMissingFields);
            var deviceName = "(no name)";
            if (vlt != null) {
                deviceName = vlt.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var tup = new DeviceActionProfile(tp, id, timeOffset, deviceActionID, deviceName, vlt,
                connectionString, multiplier, guid);
            return tup;
        }

        public override int CompareTo(BasicElement other) {
            if (other is DeviceActionProfile otheraff && _vLoadType != null && otheraff._vLoadType != null)
            {
                return _vLoadType.CompareTo(otheraff._vLoadType);
            }
            return base.CompareTo(other);
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_timeprofile == null) {
                message = "Time profile not found.";
                return false;
            }
            if (_vLoadType == null) {
                message = "Load type not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceActionProfile> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceAction> deviceActions, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeBasedProfile> timeBasedProfiles,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadTypes, bool ignoreMissingTables) {
            var aic = new AllItemCollections(timeProfiles: timeBasedProfiles, loadTypes: loadTypes,
                deviceActions: deviceActions);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("DeviceActionID", _deviceActionID);
            if (_timeprofile != null) {
                cmd.AddParameter("TimeProfileID", _timeprofile.IntID);
            }
            cmd.AddParameter("TimeOffset", "@TimeOffset", _timeOffset);
            if (_vLoadType != null) {
                cmd.AddParameter("LoadTypeID", _vLoadType.IntID);
            }
            cmd.AddParameter("Multiplier", _multiplier);
        }
    }
}