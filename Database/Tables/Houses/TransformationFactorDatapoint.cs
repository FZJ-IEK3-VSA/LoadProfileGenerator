using System.Collections.ObjectModel;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class TransformationFactorDatapoint : DBBase {
        public const string TableName = "tblTransformationFactor";
        private readonly double _factor;
        private readonly double _referenceValue;

        public TransformationFactorDatapoint([CanBeNull]int? pID, double referenceValue, double factor, int transformationDeviceID,
            [NotNull] string connectionString, StrGuid guid)
            : base(referenceValue + " - " + factor, TableName, connectionString, guid)
        {
            ID = pID;
            _referenceValue = referenceValue;
            _factor = factor;
            TransformationDeviceID = transformationDeviceID;
            TypeDescription = "Transformation Device Data Point";
        }

        [UsedImplicitly]
        public double Factor => _factor;

        public override string Name => _referenceValue + " - " + _factor;

        [UsedImplicitly]
        public double ReferenceValue => _referenceValue;

        public int TransformationDeviceID { get; }

        [NotNull]
        private static TransformationFactorDatapoint AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var factor = dr.GetDouble("Factor");
            var referenceValue = dr.GetDouble("ReferenceValue");
            var transformationDeviceID = dr.GetIntFromLong("TransformationDeviceID");
            var guid = GetGuid(dr, ignoreMissingFields);

            var tdlt = new TransformationFactorDatapoint(id, referenceValue, factor,
                transformationDeviceID, connectionString, guid);
            return tdlt;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TransformationFactorDatapoint> result,
            [NotNull] string connectionString, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("ReferenceValue", _referenceValue);
            cmd.AddParameter("Factor", Factor);
            cmd.AddParameter("TransformationDeviceID", TransformationDeviceID);
        }

        public override string ToString() => Name;
    }
}