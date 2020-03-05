using System;
using System.Collections.ObjectModel;
using System.Linq;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class DeviceSelectionDeviceAction : DBBase {
        public const string TableName = "tblDeviceSelectionDeviceActions";

        [CanBeNull] private readonly DeviceAction _deviceAction;

        [CanBeNull] private readonly DeviceActionGroup _deviceActionGroup;
        [CanBeNull]
        private readonly int? _deviceSelectionID;

        public DeviceSelectionDeviceAction([CanBeNull]int? pID, [CanBeNull]int? deviceSelectionID, [CanBeNull] DeviceActionGroup group,
            [CanBeNull] DeviceAction action, [NotNull] string connectionString, [NotNull] string name,
                                           [NotNull] string guid) : base(name, TableName,
            connectionString, guid) {
            ID = pID;
            _deviceActionGroup = group;
            _deviceAction = action;
            _deviceSelectionID = deviceSelectionID;
            TypeDescription = "Device Selection Action Item";
        }

        [NotNull]
        public DeviceAction DeviceAction => _deviceAction ?? throw new InvalidOperationException();
        [NotNull]
        public DeviceActionGroup DeviceActionGroup => _deviceActionGroup ?? throw new InvalidOperationException();
        [CanBeNull]
        public int? DeviceSelectionID => _deviceSelectionID;

        [NotNull]
        private static DeviceSelectionDeviceAction AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic) {
            var deviceSelectionItemID = dr.GetIntFromLong("ID");
            var deviceSelectionID = dr.GetIntFromLong("deviceSelectionID", false, ignoreMissingFields, -1);
            var deviceActionGroupID = dr.GetIntFromLong("DeviceActionGroupID", false, ignoreMissingFields, -1);
            var deviceActionID = dr.GetIntFromLong("DeviceActionID", false, ignoreMissingFields, -1);
            var da = aic.DeviceActions.FirstOrDefault(myDeviceAction => myDeviceAction.ID == deviceActionID);
            var dag =
                aic.DeviceActionGroups.FirstOrDefault(
                    myDeviceActionGroup => myDeviceActionGroup.ID == deviceActionGroupID);
            var name = "(no name)";
            if (dag != null) {
                name = dag.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var dsi = new DeviceSelectionDeviceAction(deviceSelectionItemID, deviceSelectionID,
                dag, da, connectionString, name, guid);
            return dsi;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_deviceAction == null) {
                message = "Device action not found";
                return false;
            }
            if (_deviceActionGroup == null) {
                message = "Device action group not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DeviceSelectionDeviceAction> result,
            [NotNull] string connectionString, [ItemNotNull] [NotNull] ObservableCollection<DeviceAction> deviceActions,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups, bool ignoreMissingTables) {
            var aic = new AllItemCollections(deviceActions: deviceActions,
                deviceActionGroups: deviceActionGroups);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_deviceSelectionID != null) {
                cmd.AddParameter("deviceSelectionID", _deviceSelectionID);
            }
            if (_deviceAction != null) {
                cmd.AddParameter("DeviceActionID", DeviceAction.IntID);
            }
            if (_deviceActionGroup != null) {
                cmd.AddParameter("DeviceActionGroupID", DeviceActionGroup.IntID);
            }
        }

        public override string ToString() {
            if (_deviceActionGroup != null) {
                return DeviceActionGroup.Name;
            }
            return "unknown";
        }
    }
}