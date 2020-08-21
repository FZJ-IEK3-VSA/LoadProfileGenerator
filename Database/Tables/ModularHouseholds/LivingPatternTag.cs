using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {

    public class LivingPatternTag : DBBaseElement {
        public const string TableName = "tblLivingPatternTags";
        public LivingPatternTag([NotNull] string pName, [NotNull] string connectionString,
                        StrGuid guid, [CanBeNull]int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            TypeDescription = "Living Pattern Tag";
        }


        [NotNull]
        private static LivingPatternTag AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
                                                     [NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name","No name");
            var id = dr.GetIntFromLong("ID");
            var guid = GetGuid(dr, ignoreMissingFields);
            var d = new LivingPatternTag(name, connectionString,guid, id);
            return d;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new LivingPatternTag(
            FindNewName(isNameTaken, "New Living Pattern Tag "), connectionString, System.Guid.NewGuid().ToStrGuid());

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((TraitTag)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var used = new List<UsedIn>();

            //foreach (var t in sim.HouseholdTraits.Items) {
            //    foreach (var hhtTag in t.Tags) {
            //        if (hhtTag.Tag == this) {
            //            used.Add(new UsedIn(t, "Household Trait"));
            //        }
            //    }
            //}
            //foreach (ModularHousehold mhh in sim.ModularHouseholds.Items) {
            //    foreach (ModularHouseholdPerson mhhPerson in mhh.Persons) {
            //        if (mhhPerson.TraitTag == this) {
            //            used.Add(new UsedIn(mhh,"Person Description"));
            //        }
            //    }
            //}
            return used;
        }

        [NotNull]
        [UsedImplicitly]
#pragma warning disable RCS1163 // Unused parameter.
        public static DBBase ImportFromItem([NotNull] TraitTag item, [NotNull] Simulator dstSim)
#pragma warning restore RCS1163 // Unused parameter.
        {
            //TODO: finish this
            var tt = new LivingPatternTag(item.Name,dstSim.ConnectionString,
                 item.Guid);
            tt.SaveToDB();
            return tt;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<LivingPatternTag> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
        }
    }
}