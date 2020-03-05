using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation.ResultFiles;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public enum TransformationConditionType {
        MinMaxValue,
        StorageContent
    }

    public class TransformationDeviceCondition : DBBase {
        public const string TableName = "tblTransformationDeviceConditions";

        [CanBeNull] private readonly VLoadType _conditionLoadType;

        private readonly TransformationConditionType _conditionType;
        private readonly double _maxValue;
        private readonly double _minValue;

        [CanBeNull] private readonly EnergyStorage _storage;

        [CanBeNull] private Dictionary<TransformationConditionType, string> _names;

        public TransformationDeviceCondition([CanBeNull]int? pID, TransformationConditionType conditionType,
            [CanBeNull] VLoadType conditionLoadType, double minValue, double maxValue,
            [CanBeNull] EnergyStorage storage, int transformationDeviceID, [NotNull] string connectionString, [NotNull] string name,
                                             [NotNull] string guid) : base(
            name, TableName, connectionString, guid)
        {
            ID = pID;
            _conditionType = conditionType;
            _conditionLoadType = conditionLoadType;
            _minValue = minValue;
            _maxValue = maxValue;
            _storage = storage;
            TransformationDeviceID = transformationDeviceID;
            TypeDescription = "Transformation Device Load Type";
        }
        [CanBeNull]
        public VLoadType ConditionLoadType => _conditionLoadType;

        public TransformationConditionType ConditionType => _conditionType;

        public double MaxValue => _maxValue;

        public double MinValue => _minValue;

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        [UsedImplicitly]
        [CanBeNull]
        public string SelectedConditionTypeStr {
            get {
                if (_names == null) {
                    throw new LPGException("Unexpected names was null.");
                }
                return _names[_conditionType];
            }
        }

        [CanBeNull]
        public EnergyStorage Storage => _storage;

        [NotNull]
        [UsedImplicitly]
        public string TextDescription => GetTextDescription();

        public int TransformationDeviceID { get; }

        [NotNull]
        private static TransformationDeviceCondition AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var transformationDeviceID = dr.GetIntFromLong("TransformationDeviceID");
            var conditionType =
                (TransformationConditionType) dr.GetIntFromLong("ConditionType", false, ignoreMissingFields);
            var conditionLoadTypeID = dr.GetIntFromLong("ConditionLoadTypeID", false, ignoreMissingFields, -1);
            var conditionLoadType = aic.LoadTypes.FirstOrDefault(mylt => mylt.ID == conditionLoadTypeID);
            var minValue = dr.GetDouble("MinValue", false, 0, ignoreMissingFields);
            var maxValue = dr.GetDouble("MaxValue", false, 10000, ignoreMissingFields);
            var storageID = dr.GetIntFromLong("StorageID", false, ignoreMissingFields);
            var storage = aic.EnergyStorages.FirstOrDefault(myStor => myStor.IntID == storageID);
            var name = "no name";
            if (conditionLoadType != null) {
                name = conditionLoadType.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var tdlt = new TransformationDeviceCondition(id, conditionType, conditionLoadType,
                minValue, maxValue, storage, transformationDeviceID, connectionString, name, guid);
            return tdlt;
        }

        [NotNull]
        private string GetTextDescription()
        {
            var loadtypename = "(no load type set)";
            if (_conditionLoadType != null) {
                loadtypename = _conditionLoadType.Name;
            }
            var storageName = "(no storage set)";
            if (_storage != null) {
                storageName = _storage.Name;
            }
            switch (_conditionType) {
                case TransformationConditionType.MinMaxValue:
                    return "Value for " + loadtypename + " between " + _minValue + " and " + _maxValue;
                case TransformationConditionType.StorageContent:
                    return "The storage " + storageName + " has to be between " + _minValue + "% and " + _maxValue +
                           "%";
                default: throw new LPGException("Unknown Conditiontype");
            }
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            if (_conditionLoadType == null && _storage == null) {
                message = "Energy storage not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TransformationDeviceCondition> result,
            [NotNull] string connectionString, [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes,
            [ItemNotNull] [NotNull] ObservableCollection<EnergyStorage> energyStorages, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(loadTypes: loadTypes, energyStorages: energyStorages);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        public void SetNameDictionary([CanBeNull] Dictionary<TransformationConditionType, string> names)
        {
            _names = names;
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("TransformationDeviceID", TransformationDeviceID);
            cmd.AddParameter("ConditionType", _conditionType);
            if (_conditionLoadType != null) {
                cmd.AddParameter("ConditionLoadTypeID", _conditionLoadType.IntID);
            }
            cmd.AddParameter("MinValue", _minValue);
            cmd.AddParameter("MaxValue", _maxValue);
            if (_storage != null) {
                cmd.AddParameter("StorageID", _storage.IntID);
            }
        }

        public override string ToString() => Name;
    }
}