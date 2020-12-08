using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class DeviceTaggingSetLoadType : DBBase {
        public const string TableName = "tblDeviceTaggingSetLoadType";
        private readonly int _taggingSetID;

        [CanBeNull] private VLoadType _loadType;

        public DeviceTaggingSetLoadType([JetBrains.Annotations.NotNull] string name, int taggingSetID, [CanBeNull] VLoadType loadType,
            [JetBrains.Annotations.NotNull] string connectionString, [CanBeNull]int? pID, StrGuid guid) : base(name, pID, TableName,
            connectionString, guid)
        {
            _taggingSetID = taggingSetID;
            _loadType = loadType;
            TypeDescription = "Affordance Tagging Set Loadtype";
        }

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType LoadType {
            get => _loadType;
            set => SetValueWithNotify(value, ref _loadType, false, nameof(LoadType));
        }

        public int TaggingSetID => _taggingSetID;

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceTaggingSetLoadType> result,
            [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadTypes)
        {
            var aic = new AllItemCollections(loadTypes: loadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        public override string ToString() => Name;

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_loadType == null) {
                message = "Loadtype was not found when loading " + TypeDescription;
                return false;
            }

            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("DeviceTaggingSetID", _taggingSetID);
            if (_loadType != null) {
                cmd.AddParameter("LoadTypeID", _loadType.IntID);
            }
        }

        [JetBrains.Annotations.NotNull]
        private static DeviceTaggingSetLoadType AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var taggingSetID = dr.GetIntFromLong("DeviceTaggingSetID");
            var loadtypeID = dr.GetIntFromLong("LoadTypeID");
            var loadType = aic.LoadTypes.FirstOrDefault(lt => lt.ID == loadtypeID);
            var name = "(no name)";
            if (loadType != null) {
                name = loadType.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            return new DeviceTaggingSetLoadType(name, taggingSetID,
                loadType, connectionString, id, guid);
        }
    }
}