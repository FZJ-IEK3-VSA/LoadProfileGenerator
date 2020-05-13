using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class TemplatePersonTrait : DBBase, IComparable<TemplatePersonTrait> {
        public const string TableName = "tblTemplatePersonTrait";
        [CanBeNull]
        private readonly int? _templatePersonID;

        [CanBeNull] private readonly HouseholdTrait _trait;

        public TemplatePersonTrait([CanBeNull]int? pID,
                                   [CanBeNull]int? templatePersonID,
                                   [NotNull] string name,
                                   [NotNull] string connectionString,
            [CanBeNull] HouseholdTrait trait, [NotNull] StrGuid guid) : base(name, TableName, connectionString, guid) {
            _trait = trait;
            _templatePersonID = templatePersonID;
            ID = pID;
            TypeDescription = "Template Person Trait";
        }
        [CanBeNull]
        public int? TemplatePersonID => _templatePersonID;

        [NotNull]
        public HouseholdTrait Trait
        {
            get {
                if (_trait != null) {
                    return _trait;
                }
                throw new InvalidOperationException();
            }
        }

        public int CompareTo([CanBeNull] TemplatePersonTrait other) {
            if (_trait != null && other?.Trait != null) {
                return string.Compare(_trait.Name, other.Trait.Name, StringComparison.Ordinal);
            }
            return 0;
        }

        [NotNull]
        private static TemplatePersonTrait AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var templatePersonID = dr.GetIntFromLong("TemplatePersonID");
            var traitID = dr.GetNullableIntFromLong("HouseholdTraitID", false, ignoreMissingFields);

            var trait = aic.HouseholdTraits.FirstOrDefault(mytrait => mytrait.ID == traitID);
            var name = "(no name)";

            if (trait != null) {
                name = trait.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var chht = new TemplatePersonTrait(id, templatePersonID, name, connectionString, trait,
                guid);
            return chht;
        }

        public override int CompareTo([CanBeNull] BasicElement other) {
            if (!(other is TemplatePersonTrait othr))
            {
                throw new LPGException("Other was null.");
            }
            if (_trait != null && othr._trait != null) {
                return string.Compare(_trait.Name, othr._trait.Name, StringComparison.Ordinal);
            }
            return 0;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_trait == null) {
                message = "Trait not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TemplatePersonTrait> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTrait> traits, bool ignoreMissingTables) {
            var aic = new AllItemCollections(householdTraits: traits);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_trait != null) {
                cmd.AddParameter("HouseholdTraitID", _trait.IntID);
            }
            if (_templatePersonID != null) {
                cmd.AddParameter("TemplatePersonID", _templatePersonID);
            }
        }

        public override string ToString() {
            if (_trait != null) {
                return _trait.Name;
            }
            return "(no name)";
        }
    }
}