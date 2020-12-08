using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum TraitLimitType
    {
        NoLimit,
        OnePerPerson,
        OnePerHousehold
    }

    public enum TraitPriority
    {
        Mandatory,
        Recommended,
        Optional,
        ForExperts,
        All
    }

    public static class TraitPriorityHelper
    {
#pragma warning disable S3887 // Mutable, non-private fields should not be "readonly"
        //public static Dictionary<TraitPriority, string> TraitPriorityDictionaryEnumDictionary { get; }=
        //    new Dictionary<TraitPriority, string> {
        //        {TraitPriority.Mandatory, "Mandatory"},
        //        {TraitPriority.Optional, "Optional"},
        //        {TraitPriority.Recommended, "Recommended"}
        //    };
#pragma warning restore S3887 // Mutable, non-private fields should not be "readonly"

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
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

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
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

    public class TraitTag : DBBaseElement
    {
        public const string TableName = "tblTraitTags";
        private TraitLimitType _traitLimitType;
        private TraitPriority _traitPriority;
        public TraitTag([JetBrains.Annotations.NotNull] string pName, [JetBrains.Annotations.NotNull] string connectionString, TraitLimitType traitLimitType, TraitPriority tp,
                        StrGuid guid, [CanBeNull] int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            _traitPriority = tp;
            TypeDescription = "Trait Tag";
            _traitLimitType = traitLimitType;
        }

        [UsedImplicitly]
        public TraitLimitType TraitLimitType
        {
            get => _traitLimitType;
            set => SetValueWithNotify(value, ref _traitLimitType, nameof(TraitLimitType));
        }

        [UsedImplicitly]
        public TraitPriority TraitPriority
        {
            get => _traitPriority;
            set => SetValueWithNotify(value, ref _traitPriority, nameof(TraitPriority));
        }

        [JetBrains.Annotations.NotNull]
        private static TraitTag AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", "No name");
            var id = dr.GetIntFromLong("ID");
            var traitLimitType =
                (TraitLimitType)dr.GetIntFromLong("TraitLimitType", false, ignoreMissingFields);
            var traitPriority =
                (TraitPriority)
                dr.GetIntFromLong("TraitPriority", false, ignoreMissingFields, (int)TraitPriority.ForExperts);
            var guid = GetGuid(dr, ignoreMissingFields);
            var d = new TraitTag(name, connectionString, traitLimitType, traitPriority, guid, id);
            return d;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new TraitTag(
            FindNewName(isNameTaken, "New Trait Tag "), connectionString, TraitLimitType.NoLimit,
            TraitPriority.Recommended, System.Guid.NewGuid().ToStrGuid());

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((TraitTag)toImport, dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var used = new List<UsedIn>();

            foreach (var t in sim.HouseholdTraits.Items)
            {
                foreach (var hhtTag in t.Tags)
                {
                    if (hhtTag.Tag == this)
                    {
                        used.Add(new UsedIn(t, "Household Trait"));
                    }
                }
            }
            return used;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
#pragma warning disable RCS1163 // Unused parameter.
        public static DBBase ImportFromItem([JetBrains.Annotations.NotNull] TraitTag item, [JetBrains.Annotations.NotNull] Simulator dstSim)
#pragma warning restore RCS1163 // Unused parameter.
        {
            //TODO: finish this
            var tt = new TraitTag(item.Name, dstSim.ConnectionString,
                item.TraitLimitType, item._traitPriority, item.Guid);
            tt.SaveToDB();
            return tt;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<TraitTag> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
            cmd.AddParameter("TraitLimitType", _traitLimitType);
            cmd.AddParameter("TraitPriority", _traitPriority);
        }
    }
}