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

        public HouseholdTag([JetBrains.Annotations.NotNull] string pName, [JetBrains.Annotations.NotNull] string connectionString, [CanBeNull] string classification,StrGuid guid, [CanBeNull] int? pID = null)
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

        [JetBrains.Annotations.NotNull]
        private static HouseholdTag AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var name =  dr.GetString("Name","no name");
            var id = dr.GetIntFromLong("ID");
            var classification = dr.GetString("Classification", false, string.Empty, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var d = new HouseholdTag(name, connectionString, classification,guid, id);
            return d;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new HouseholdTag(
            FindNewName(isNameTaken, "New Template Tag "), connectionString, string.Empty, System.Guid.NewGuid().ToStrGuid());

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((HouseholdTag)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var used = new List<UsedIn>();

            foreach (var t in sim.HouseholdTemplates.Items) {
                foreach (var hhtTag in t.TemplateTags) {
                    if (hhtTag.Tag == this) {
                        used.Add(new UsedIn(t, "Household Template"));
                    }
                }
            }
            return used;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
#pragma warning disable RCS1163 // Unused parameter.
        public static DBBase ImportFromItem([JetBrains.Annotations.NotNull] HouseholdTag item, [JetBrains.Annotations.NotNull] Simulator dstSim)
#pragma warning restore RCS1163 // Unused parameter.
        {
            var tt = new HouseholdTag(item.Name, dstSim.ConnectionString, item.Classification,item.Guid);
            tt.SaveToDB();
            return tt;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HouseholdTag> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
            if(_classification != null) {
                cmd.AddParameter("Classification", _classification);
            }
        }

        public StrGuid RelevantGuid => Guid;
    }
}