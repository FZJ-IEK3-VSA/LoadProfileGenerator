using System;
using System.Collections.ObjectModel;
using System.Linq;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HHTTrait : DBBase {
        public const string ParentIDFieldName = "ParentTraitID";
        public const string TableName = "tblHHTTraits";

        [CanBeNull] private readonly HouseholdTrait _thisTrait;

        public HHTTrait([CanBeNull]int? pID, [CanBeNull] int? parentTraitID, [CanBeNull] HouseholdTrait thisTrait, [NotNull] string connectionString,
            [NotNull] string name, [NotNull] string guid)
            : base(name, TableName, connectionString, guid)
        {
            ID = pID;
            _thisTrait = thisTrait;
            ParentTraitID = parentTraitID;
            TypeDescription = "Household Trait Subtrait";
        }

        [NotNull]
        [UsedImplicitly]
        public new string Name => ThisTrait.Name;

        [CanBeNull]
        public int? ParentTraitID { get; }

        [NotNull]
        public HouseholdTrait ThisTrait => _thisTrait ?? throw new InvalidOperationException();

        [NotNull]
        private static HHTTrait AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var hhtDesireID = dr.GetIntFromLong("ID");
            var parentTraitID = dr.GetIntFromLong("ParentTraitID");
            var thisTraitID = dr.GetIntFromLong("ThisTraitID");
            var subhht = aic.HouseholdTraits.FirstOrDefault(mytrait => mytrait.ID == thisTraitID);
            var name = "(no name)";
            if (subhht != null) {
                name = subhht.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var hhtsub = new HHTTrait(hhtDesireID, parentTraitID, subhht, connectionString, name, guid);
            return hhtsub;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            if (_thisTrait == null) {
                message = "Trait not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HHTTrait> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTrait> traits, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(householdTraits: traits);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_thisTrait != null) {
                cmd.AddParameter("ThisTraitID", ThisTrait.IntID);
            }
            if (ParentTraitID != null) {
                cmd.AddParameter("ParentTraitID", ParentTraitID);
            }
        }

        public override string ToString() => ThisTrait.Name;
    }
}