using System.Collections.ObjectModel;
using System.Globalization;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STHouseSize : DBBase {
        public const string TableName = "tblSTHouseSizes";
        private readonly int _maximumHouseSize;
        private readonly int _minimumHouseSize;
        private readonly double _percentage;
        private readonly int _settlementTemplateID;

        public STHouseSize([CanBeNull]int? pID, [JetBrains.Annotations.NotNull] string connectionString, int settlementTemplateID, [JetBrains.Annotations.NotNull] string name,
                           int minimumSize,
            int maximumSize, double percentage, [NotNull] StrGuid guid) : base(name, TableName, connectionString, guid)
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

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string PrettyPercentage => (_percentage * 100).ToString("N2", CultureInfo.CurrentCulture) + "%";

        public int SettlementTemplateID => _settlementTemplateID;

        [JetBrains.Annotations.NotNull]
        private static STHouseSize AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
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

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<STHouseSize> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("SettlementTemplateID", _settlementTemplateID);
            cmd.AddParameter("MinimumHouseSize", _minimumHouseSize);
            cmd.AddParameter("MaximumHouseSize", _maximumHouseSize);
            cmd.AddParameter("Percentage", _percentage);
        }

        public override string ToString() => "Unknown";
    }
}