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

        private double _weight;

        public const double DefaultWeight = 1.0;

        public LivingPatternTag([JetBrains.Annotations.NotNull] string pName, double weight, [JetBrains.Annotations.NotNull] string connectionString,
                        [NotNull] StrGuid guid, [CanBeNull]int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            _weight = weight;
            ID = pID;
            TypeDescription = "Living Pattern Tag";
        }

        /// <summary>
        /// Weight for varying LivingPatternTags when creating households from templates
        /// </summary>
        public double Weight
        {
            get => _weight;
            [UsedImplicitly]
            set => SetValueWithNotify(value, ref _weight, nameof(Weight));
        }


        [JetBrains.Annotations.NotNull]
        private static LivingPatternTag AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
                                                     [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name","No name");
            var weight = dr.GetDouble("Weight", defaultvalue: DefaultWeight);
            var id = dr.GetIntFromLong("ID");
            var guid = GetGuid(dr, ignoreMissingFields);
            var d = new LivingPatternTag(name, weight, connectionString,guid, id);
            return d;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new LivingPatternTag(
            FindNewName(isNameTaken, "New Living Pattern Tag "), DefaultWeight, connectionString, System.Guid.NewGuid().ToStrGuid());

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((LivingPatternTag)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var used = new List<UsedIn>();
            foreach (var t in sim.HouseholdTraits.Items) {
                foreach (var hhtTag in t.LivingPatternTags) {
                    if (hhtTag.Tag == this) {
                        used.Add(new UsedIn(t, "Household Trait"));
                    }
                }
            }
            foreach (ModularHousehold mhh in sim.ModularHouseholds.Items) {
                foreach (ModularHouseholdPerson mhhPerson in mhh.Persons) {
                    if (mhhPerson.LivingPatternTag == this) {
                        used.Add(new UsedIn(mhh,"Person Description"));
                   }
                }
            }
            foreach (var template in sim.HouseholdTemplates.Items)
            {
                foreach (var mhhPerson in template.Persons)
                {
                    if (mhhPerson.LivingPatternTag == this)
                    {
                        used.Add(new UsedIn(template, "Person Description"));
                    }
                }
            }
            return used;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([JetBrains.Annotations.NotNull] LivingPatternTag item, [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            //TODO: finish this
            var tt = new LivingPatternTag(item.Name, item._weight, dstSim.ConnectionString,
                 item.Guid);
            tt.SaveToDB();
            return tt;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<LivingPatternTag> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
            cmd.AddParameter("Weight", _weight);
        }
    }
}