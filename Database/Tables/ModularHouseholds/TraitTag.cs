using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum TraitLimitType {
        NoLimit,
        OnePerPerson,
        OnePerHousehold
    }

    public enum TraitPriority {
        Mandatory,
        Recommended,
        Optional,
        ForExperts,
        All
    }

    public static class TraitPriorityHelper {
#pragma warning disable S3887 // Mutable, non-private fields should not be "readonly"
        //public static Dictionary<TraitPriority, string> TraitPriorityDictionaryEnumDictionary { get; }=
        //    new Dictionary<TraitPriority, string> {
        //        {TraitPriority.Mandatory, "Mandatory"},
        //        {TraitPriority.Optional, "Optional"},
        //        {TraitPriority.Recommended, "Recommended"}
        //    };
#pragma warning restore S3887 // Mutable, non-private fields should not be "readonly"

        [NotNull] [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
#pragma warning disable S3887 // Mutable, non-private fields should not be "readonly"
#pragma warning disable S2386 // Mutable fields should not be "public static"
        public static readonly Dictionary<TraitPriority, string> TraitPriorityDictionaryEnumDictionaryComplete =
            new Dictionary<TraitPriority, string>
            {
                [TraitPriority.Mandatory] = "Mandatory",
                [TraitPriority.Optional] = "Optional",
                [TraitPriority.Recommended] = "Recommended",
                [TraitPriority.All] = "All",
                [TraitPriority.ForExperts] = "For Experts"
            };
#pragma warning restore S2386 // Mutable fields should not be "public static"
#pragma warning restore S3887 // Mutable, non-private fields should not be "readonly"

        [NotNull] [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
#pragma warning disable S3887 // Mutable, non-private fields should not be "readonly"
#pragma warning disable S2386 // Mutable fields should not be "public static"
        public static readonly Dictionary<TraitPriority, string> TraitPriorityDictionaryEnumDictionaryWithAll =
            new Dictionary<TraitPriority, string>
            {
                [TraitPriority.Mandatory] = "Mandatory",
                [TraitPriority.Optional] = "Optional",
                [TraitPriority.Recommended] = "Recommended",
                [TraitPriority.All] = "All"
            };
#pragma warning restore S2386 // Mutable fields should not be "public static"
#pragma warning restore S3887 // Mutable, non-private fields should not be "readonly"
    }

    public class TraitTag : DBBaseElement {
        public const string TableName = "tblTraitTags";
        private TraitLimitType _traitLimitType;
        private TraitPriority _traitPriority;
        public TraitTag([NotNull] string pName, [NotNull] string connectionString, TraitLimitType traitLimitType, TraitPriority tp,
                        [NotNull] string guid, [CanBeNull]int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            _traitPriority = tp;
            TypeDescription = "Trait Tag";
            _traitLimitType = traitLimitType;
        }

        [UsedImplicitly]
        public TraitLimitType TraitLimitType {
            get => _traitLimitType;
            set => SetValueWithNotify(value, ref _traitLimitType, nameof(TraitLimitType));
        }

        [UsedImplicitly]
        public TraitPriority TraitPriority {
            get => _traitPriority;
            set => SetValueWithNotify(value, ref _traitPriority, nameof(TraitPriority));
        }

        [NotNull]
        private static TraitTag AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name","No name");
            var id = dr.GetIntFromLong("ID");
            var traitLimitType =
                (TraitLimitType) dr.GetIntFromLong("TraitLimitType", false, ignoreMissingFields);
            var traitPriority =
                (TraitPriority)
                dr.GetIntFromLong("TraitPriority", false, ignoreMissingFields, (int) TraitPriority.ForExperts);
            var guid = GetGuid(dr, ignoreMissingFields);
            var d = new TraitTag(name, connectionString, traitLimitType, traitPriority,guid, id);
            return d;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new TraitTag(
            FindNewName(isNameTaken, "New Trait Tag "), connectionString, TraitLimitType.NoLimit,
            TraitPriority.Recommended, System.Guid.NewGuid().ToString());

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((TraitTag)toImport,dstSim);

        [NotNull]
        public override List<UsedIn> CalculateUsedIns([NotNull] Simulator sim)
        {
            var used = new List<UsedIn>();

            foreach (var t in sim.HouseholdTraits.It) {
                foreach (var hhtTag in t.Tags) {
                    if (hhtTag.Tag == this) {
                        used.Add(new UsedIn(t, "Household Trait"));
                    }
                }
            }
            foreach (ModularHousehold mhh in sim.ModularHouseholds.It) {
                foreach (ModularHouseholdPerson mhhPerson in mhh.Persons) {
                    if (mhhPerson.TraitTag == this) {
                        used.Add(new UsedIn(mhh,"Person Description"));
                    }
                }
            }
            return used;
        }

        [NotNull]
        [UsedImplicitly]
#pragma warning disable RCS1163 // Unused parameter.
        public static DBBase ImportFromItem([NotNull] TraitTag item, [NotNull] Simulator dstSim)
#pragma warning restore RCS1163 // Unused parameter.
        {
            //TODO: finish this
            var tt = new TraitTag(item.Name,dstSim.ConnectionString,
                item.TraitLimitType, item._traitPriority, item.Guid);
            tt.SaveToDB();
            return tt;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TraitTag> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", Name);
            cmd.AddParameter("TraitLimitType", _traitLimitType);
            cmd.AddParameter("TraitPriority", _traitPriority);
        }
    }
}