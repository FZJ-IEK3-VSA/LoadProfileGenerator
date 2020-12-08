using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database.Database;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STTraitLimit : DBBase {
        public const string TableName = "tblSTTraitLimits";
        private readonly int _maximum;
        private readonly int _settlementTemplateID;

        [CanBeNull] private readonly HouseholdTrait _trait;

        public STTraitLimit([CanBeNull]int? pID, [JetBrains.Annotations.NotNull] string connectionString, int settlementTemplateID, [JetBrains.Annotations.NotNull] string name,
            [CanBeNull] HouseholdTrait trait, int maximum, StrGuid guid) : base(name, TableName, connectionString, guid) {
            TypeDescription = "Settlement Template Household Trait Limit";
            ID = pID;
            _trait = trait;
            _maximum = maximum;
            _settlementTemplateID = settlementTemplateID;
        }

        [UsedImplicitly]
        public int LeftoverPermittedTraits => PermittedCount - UsedCount;

        public int Maximum => _maximum;

        [UsedImplicitly]
        public int PermittedCount { get; set; }

        public int SettlementTemplateID => _settlementTemplateID;

        [CanBeNull]
        public HouseholdTrait Trait => _trait;

        [UsedImplicitly]
        public int UsedCount { get; set; }

        [JetBrains.Annotations.NotNull]
        private static STTraitLimit AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var settlementtemplateID = dr.GetIntFromLong("SettlementTemplateID", false, ignoreMissingFields, -1);
            var traitID = dr.GetIntFromLong("TraitID", false);
            var maximum = dr.GetIntFromLong("Maximum", false);
            var ht = aic.HouseholdTraits.FirstOrDefault(x => x.IntID == traitID);
            var name = "unknown";
            if (ht != null) {
                name = ht.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var shh = new STTraitLimit(id, connectionString, settlementtemplateID, name, ht, maximum, guid);
            return shh;
        }

        public void Init(int totalCount) {
            PermittedCount = (int) (_maximum / 100.0 * totalCount);
            UsedCount = 0;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_trait == null) {
                message = "Trait not found";
                return false;
            }
            message = "";
            return true;
        }

        public bool IsPermitted([JetBrains.Annotations.NotNull] ModularHousehold chh) {
            var traits =
                chh.Traits.Where(x => x.HouseholdTrait == _trait).Select(x => x.HouseholdTrait).ToList();
            if (traits.Count <= PermittedCount - UsedCount) {
                return true;
            }
            return false;
        }

        public bool IsPermitted([JetBrains.Annotations.NotNull] HouseholdTrait trait) {
            if (trait != _trait) {
                return true;
            }
            if (UsedCount < PermittedCount) {
                return true;
            }
            return false;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<STTraitLimit> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HouseholdTrait> traits) {
            var aic = new AllItemCollections(householdTraits: traits);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        public void RegisterMHH([JetBrains.Annotations.NotNull] ModularHousehold chh) {
            var traits =
                chh.Traits.Where(x => x.HouseholdTrait == _trait).Select(x => x.HouseholdTrait).ToList();
            UsedCount += traits.Count;
            if (UsedCount > PermittedCount) {
                throw new LPGException("too many traits assigned!");
            }
        }

        public void RegisterTrait([JetBrains.Annotations.NotNull] HouseholdTrait trait) {
            if (_trait != trait) {
                return;
            }
            UsedCount++;
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_trait != null) {
                cmd.AddParameter("TraitID", _trait.IntID);
            }
            cmd.AddParameter("Maximum", _maximum);
            cmd.AddParameter("SettlementTemplateID", _settlementTemplateID);
        }

        public override string ToString() {
            if (_trait != null) {
                return _trait.Name;
            }

            return "Unknown";
        }
    }
}