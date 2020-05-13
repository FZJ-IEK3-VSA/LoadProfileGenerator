using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HouseholdTag : DBBaseElement, IRelevantGuidProvider {

        public const string TableName = "tblTemplateTags";
        [CanBeNull]
        private string _classification;

        public HouseholdTag([NotNull] string pName, [NotNull] string connectionString, [CanBeNull] string classification,[NotNull] StrGuid guid, [CanBeNull] int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            _classification = classification;
            TypeDescription = "Household Template Tag";
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Classification {
            get => _classification;
            set => SetValueWithNotify(value, ref _classification, nameof(Classification));
        }

        [NotNull]
        private static HouseholdTag AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var name =  dr.GetString("Name","no name");
            var id = dr.GetIntFromLong("ID");
            var classification = dr.GetString("Classification", false, string.Empty, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var d = new HouseholdTag(name, connectionString, classification,guid, id);
            return d;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new HouseholdTag(
            FindNewName(isNameTaken, "New Template Tag "), connectionString, string.Empty, System.Guid.NewGuid().ToStrGuid());

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((HouseholdTag)toImport,dstSim);

        [NotNull]
        public override List<UsedIn> CalculateUsedIns([NotNull] Simulator sim)
        {
            var used = new List<UsedIn>();

            foreach (var t in sim.HouseholdTemplates.It) {
                foreach (var hhtTag in t.TemplateTags) {
                    if (hhtTag.Tag == this) {
                        used.Add(new UsedIn(t, "Household Template"));
                    }
                }
            }
            return used;
        }

        [NotNull]
        [UsedImplicitly]
#pragma warning disable RCS1163 // Unused parameter.
        public static DBBase ImportFromItem([NotNull] HouseholdTag item, [NotNull] Simulator dstSim)
#pragma warning restore RCS1163 // Unused parameter.
        {
            var tt = new HouseholdTag(item.Name, dstSim.ConnectionString, item.Classification,item.Guid);
            tt.SaveToDB();
            return tt;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HouseholdTag> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", Name);
            if(_classification != null) {
                cmd.AddParameter("Classification", _classification);
            }
        }

        public StrGuid RelevantGuid => Guid;
    }
}