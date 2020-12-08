using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database.Database;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STHouseholdTemplate : DBBase {
        public const string TableName = "tblSTHouseholdTemplates";

        [CanBeNull] private readonly HouseholdTemplate _householdTemplate;

        private readonly int _settlementTemplateID;

        public STHouseholdTemplate([CanBeNull]int? pID, [JetBrains.Annotations.NotNull] string connectionString, int settlementTemplateID, [JetBrains.Annotations.NotNull] string name,
            [CanBeNull] HouseholdTemplate householdTemplate, StrGuid guid) : base(name, TableName, connectionString, guid) {
            TypeDescription = "Settlement Template Household Templates";
            ID = pID;
            _householdTemplate = householdTemplate;
            _settlementTemplateID = settlementTemplateID;
        }

        [CanBeNull]
        public HouseholdTemplate HouseholdTemplate => _householdTemplate;

        public int SettlementTemplateID => _settlementTemplateID;

        [JetBrains.Annotations.NotNull]
        private static STHouseholdTemplate AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var settlementtemplateID = dr.GetIntFromLong("SettlementTemplateID", false, ignoreMissingFields, -1);
            var householdTemplateID = dr.GetIntFromLong("HouseholdTemplateID", false);
            var ht = aic.HouseholdTemplates.FirstOrDefault(x => x.IntID == householdTemplateID);
            var name = "unknown";
            if (ht != null) {
                name = ht.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var shh = new STHouseholdTemplate(id, connectionString, settlementtemplateID, name, ht, guid);
            return shh;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (HouseholdTemplate == null) {
                message = "Household template not found";
                return false;
            }
            message = "";
            return true;
        }

        /// <param name="result">todo: describe result parameter on LoadFromDatabase</param>
        /// <param name="connectionString">todo: describe connectionString parameter on LoadFromDatabase</param>
        /// <param name="ignoreMissingTables">todo: describe ignoreMissingTables parameter on LoadFromDatabase</param>
        /// <param name="templates">todo: describe templates parameter on LoadFromDatabase</param>
        /// <exception cref="LPGException">tried to call needs update even though it's not allowed right now</exception>
        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<STHouseholdTemplate> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HouseholdTemplate> templates) {
            var aic = new AllItemCollections(householdTemplates: templates);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_householdTemplate != null) {
                cmd.AddParameter("HouseholdTemplateID", _householdTemplate.IntID);
            }
            cmd.AddParameter("SettlementTemplateID", _settlementTemplateID);
        }

        public override string ToString() {
            if (HouseholdTemplate != null) {
                return HouseholdTemplate.Name;
            }

            return "Unknown";
        }
    }
}