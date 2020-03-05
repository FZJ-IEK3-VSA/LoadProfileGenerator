using System.Collections.ObjectModel;
using System.Globalization;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STHouseSize : DBBase {
        public const string TableName = "tblSTHouseSizes";
        private readonly int _maximumHouseSize;
        private readonly int _minimumHouseSize;
        private readonly double _percentage;
        private readonly int _settlementTemplateID;

        public STHouseSize([CanBeNull]int? pID, [NotNull] string connectionString, int settlementTemplateID, [NotNull] string name,
                           int minimumSize,
            int maximumSize, double percentage, [NotNull] string guid) : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Settlement Template House Size";
            ID = pID;
            _minimumHouseSize = minimumSize;
            _maximumHouseSize = maximumSize;
            _percentage = percentage;
            _settlementTemplateID = settlementTemplateID;
        }

        // for the household generation
        public int HouseCount { get; set; }

        public int MaximumHouseholdCount => HouseCount * _maximumHouseSize;

        public int MaximumHouseSize => _maximumHouseSize;

        public int MinimumHouseholdCount => HouseCount * _minimumHouseSize;

        public int MinimumHouseSize => _minimumHouseSize;

        public double Percentage => _percentage;

        [NotNull]
        [UsedImplicitly]
        public string PrettyPercentage => (_percentage * 100).ToString("N2", CultureInfo.CurrentCulture) + "%";

        public int SettlementTemplateID => _settlementTemplateID;

        [NotNull]
        private static STHouseSize AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var settlementtemplateID = dr.GetIntFromLong("SettlementTemplateID", false, ignoreMissingFields, -1);
            var minimumHouseSize = dr.GetIntFromLong("MinimumHouseSize", false);
            var maximumHouseSize = dr.GetIntFromLong("MaximumHouseSize", false);
            var percentage = dr.GetDouble("Percentage", false);
            const string name = "unknown";
            var guid = GetGuid(dr, ignoreMissingFields);

            var shh = new STHouseSize(id, connectionString, settlementtemplateID, name, minimumHouseSize,
                maximumHouseSize, percentage, guid);
            return shh;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([NotNull] [ItemNotNull] ObservableCollection<STHouseSize> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("SettlementTemplateID", _settlementTemplateID);
            cmd.AddParameter("MinimumHouseSize", _minimumHouseSize);
            cmd.AddParameter("MaximumHouseSize", _maximumHouseSize);
            cmd.AddParameter("Percentage", _percentage);
        }

        public override string ToString() => "Unknown";
    }
}