using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class HouseholdPlan : DBBaseElement {
        public const string TableName = "tblHouseholdPlans";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HouseholdPlanEntry> _entries;

        [CanBeNull] private ICalcObject _calcObject;

        [NotNull] private string _description;

        [CanBeNull] private AffordanceTaggingSet _taggingSet;

        public HouseholdPlan([NotNull] string name, [CanBeNull] AffordanceTaggingSet taggingSet,
            [CanBeNull] ICalcObject calcObject,
                             [NotNull] string description,
                             [NotNull] string connectionString, [NotNull] StrGuid guid,
                             [CanBeNull]int? pID = null) : base(
            name, TableName, connectionString, guid)
        {
            _entries = new ObservableCollection<HouseholdPlanEntry>();
            ID = pID;
            TypeDescription = "Household Plan";
            _description = description;
            _taggingSet = taggingSet;
            _calcObject = calcObject;
        }

        [CanBeNull]
        [UsedImplicitly]
        public AffordanceTaggingSet AffordanceTaggingSet {
            get => _taggingSet;
            set => SetValueWithNotify(value, ref _taggingSet);
        }

        [CanBeNull]
        [UsedImplicitly]
        public ICalcObject CalcObject {
            get => _calcObject;
            set => SetValueWithNotify(value, ref _calcObject);
        }

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdPlanEntry> Entries => _entries;

        [UsedImplicitly]
        public static bool FailOnIncorrectImport { get; set; }

        [UsedImplicitly]
        public void AddNewEntry([NotNull] Person person, [NotNull] AffordanceTag tag, double times, double timecount, TimeType timeType)
        {
            //if (tag != null) {
                var tagname = tag.Name;
            //}
            var name = person.Name + "-" + tagname;
            foreach (var entry in _entries) {
                if (entry.Name == name) {
                    return;
                }
            }
            var at = new HouseholdPlanEntry(name, IntID, tag, person, times, timecount, timeType,
                ConnectionString, null, _calcObject, System.Guid.NewGuid().ToStrGuid());
            Logger.Get().SafeExecuteWithWait(() => {
                _entries.Add(at);
                _entries.Sort();
                SaveToDB();
            });
        }

        [NotNull]
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        private static HouseholdPlan AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var description = dr.GetString("Description");
            var calcObjectID = dr.GetIntFromLong("CalcObjectID", false, ignoreMissingFields);
            //var calcObjectType = dr.GetIntFromLong("CalcObjectType", false, ignoreMissingFields);
            var affTaggingSetID = dr.GetIntFromLong("AffordanceTaggingSetID", false, ignoreMissingFields);
            //CalcObjectType cot = (CalcObjectType) calcObjectType;
            ICalcObject calcObject = aic.ModularHouseholds.FirstOrDefault(ho => ho.ID == calcObjectID);
            var afftagset =
                aic.AffordanceTaggingSets.FirstOrDefault(affts => affts.ID == affTaggingSetID);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new HouseholdPlan(name, afftagset, calcObject, description, connectionString,guid, id);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
            => new HouseholdPlan(FindNewName(isNameTaken, "New Household Plan "), null, null, "(no description)",
                connectionString, System.Guid.NewGuid().ToStrGuid());

        public void DeleteEntry([NotNull] HouseholdPlanEntry at)
        {
            at.DeleteFromDB();
            Logger.Get().SafeExecuteWithWait(() => _entries.Remove(at));
        }

        public override void DeleteFromDB()
        {
            base.DeleteFromDB();
            foreach (var value in _entries) {
                value.DeleteFromDB();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public static HouseholdPlan ImportFromItem([NotNull] HouseholdPlan toImport, [NotNull] Simulator dstSim)
        {
            ICalcObject calcObject = null;
            if (toImport.CalcObject != null) {
                calcObject = GetICalcObjectFromList(dstSim.ModularHouseholds.MyItems, null, null, toImport.CalcObject);
            }
            if (toImport.AffordanceTaggingSet == null)
            {
                throw new LPGException("Affordance tagging set was null");
            }
            var dsttaggingSet =
                GetItemFromListByName(dstSim.AffordanceTaggingSets.MyItems, toImport.AffordanceTaggingSet.Name);
            var hd = new HouseholdPlan(toImport.Name, dsttaggingSet, calcObject, toImport.Description,
                dstSim.ConnectionString,toImport.Guid);
            hd.SaveToDB();

            foreach (var hpe in toImport._entries) {
                if (hpe.Person != null && hpe.Tag != null) {
                    var dstPerson = GetItemFromListByName(dstSim.Persons.MyItems, hpe.Person.Name);
                    AffordanceTag tag = null;
                    if (dsttaggingSet != null) {
                        tag = GetItemFromListByName(dsttaggingSet.Tags, hpe.Tag.Name);
                    }
                    if (tag == null) {
                        Logger.Warning("Could not find a tag for import. Skipping.");
                        continue;
                    }
                    if (dstPerson == null) {
                        Logger.Warning("Could not find a person for import. Skipping.");
                        continue;
                    }
                    hd.AddNewEntry(dstPerson, tag, hpe.Times, hpe.TimeCount, hpe.TimeType);
                }
            }
            hd.SaveToDB();
            if (FailOnIncorrectImport) {
                var src = toImport.MakeHash();
                var dst = toImport.MakeHash();
                if (src != dst) {
                    throw new LPGException("FailOnIncorrectImport: Not equal after import.");
                }
            }
            return hd;
        }

        private static bool IsCorrectEntryParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var entry = (HouseholdPlanEntry) child;

            if (parent.ID == entry.HouseholdPlanID) {
                var ats = (HouseholdPlan) parent;
                ats.Entries.Add(entry);
                entry.CalcObject = ats.CalcObject;
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HouseholdPlan> result, [NotNull] string connectionString,
            bool ignoreMissingTables, [NotNull] [ItemNotNull] ObservableCollection<Person> persons,
            [ItemNotNull] [NotNull] ObservableCollection<AffordanceTaggingSet> taggingSets,
            [NotNull] [ItemNotNull] ObservableCollection<ModularHousehold> modularHouseholds)
        {
            var aic = new AllItemCollections(affordanceTaggingSets: taggingSets,
                modularHouseholds: modularHouseholds);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);

            var allTags = new ObservableCollection<AffordanceTag>();
            foreach (var set in taggingSets) {
                foreach (var tag in set.Tags) {
                    allTags.Add(tag);
                }
            }

            var entries = new ObservableCollection<HouseholdPlanEntry>();
            HouseholdPlanEntry.LoadFromDatabase(entries, connectionString, ignoreMissingTables, allTags, persons);
            var items2Delete = new List<HouseholdPlanEntry>();
            foreach (var entry in entries) {
                if (entry.Person == null || entry.Tag == null) {
                    items2Delete.Add(entry);
                }
            }
            foreach (var entry in items2Delete) {
                entry.DeleteFromDB();
                entries.Remove(entry);
            }
            SetSubitems(new List<DBBase>(result), new List<DBBase>(entries), IsCorrectEntryParent, ignoreMissingTables);
        }

        [NotNull]
        private string MakeHash()
        {
            var s = Name + "#";
            if (_calcObject != null) {
                s += _calcObject.Name + "#";
            }
            s += Description + "#";
            if (_taggingSet != null) {
                s += _taggingSet.Name + "#";
            }
            var builder = new StringBuilder();
            builder.Append(s);
            foreach (var householdPlanEntry in Entries) {
                builder.Append(householdPlanEntry.MakeHash()).Append("#");
            }
            return builder.ToString();
        }

        public void Refresh([CanBeNull] Action<int> reportProgressAction)
        {
            if (_taggingSet == null) {
                return;
            }
            if (CalcObject == null) {
                return;
            }
            var newEntries = new List<Tuple<Person, AffordanceTag>>();
            if (_taggingSet.Tags.Count == 0) {
                Logger.Error("The selected tagging set has no tags.");
            }
            if (CalcObject.AllPersons.Count == 0) {
                Logger.Error("The selected household has no persons.");
            }
            var count = 0;
            var entriescopy = _entries.ToList(); // to avoid collection changed
            foreach (var person in CalcObject.AllPersons) {
                foreach (var affordanceTag in _taggingSet.Tags) {
                    var found = entriescopy.Any(x => x.Person == person && x.Tag == affordanceTag);

                    count++;
                    reportProgressAction?.Invoke(count);
                    if (!found) {
                        newEntries.Add(new Tuple<Person, AffordanceTag>(person, affordanceTag));
                    }
                }
            }
            foreach (var entry in newEntries) {
                AddNewEntry(entry.Item1, entry.Item2, 0, 1, TimeType.Day);
            }
            var items2Delete = new List<HouseholdPlanEntry>();
            if (_taggingSet == null) {
                return;
            }
            foreach (var entry in _entries) {
                if (CalcObject== null)
                {
                    throw new LPGException("CalcObject was null");
                }
                if (entry.Tag == null || entry.Person == null || !CalcObject.AllPersons.Contains(entry.Person) ||
                    !_taggingSet.Tags.Contains(entry.Tag)) {
                    items2Delete.Add(entry);
                }
            }
            foreach (var entry in items2Delete) {
                DeleteEntry(entry);
            }
            foreach (var entry in _entries) {
                entry.Ats = AffordanceTaggingSet;
            }
            Logger.Get().SafeExecuteWithWait(_entries.Sort);
        }

        public void RefreshFromTraits()
        {
            if (!(CalcObject is ModularHousehold)) {
                return;
            }
            foreach (var entry in Entries) {
                if (entry.FirstExistingTrait != null) {
                    entry.TimeCount = entry.FirstExistingTrait.EstimatedTimeCount;
                    entry.TimeType = entry.FirstExistingTrait.EstimatedTimeType;
                    entry.Times = entry.FirstExistingTrait.EstimatedTimes;
                }
            }
        }

        public void RefreshTagCategories([ItemNotNull] [NotNull] ObservableCollection<Affordance> affordances)
        {
            foreach (var entry in _entries) {
                if (entry.Tag== null)
                {
                    throw new LPGException("Tag was null");
                }
                var aff = affordances.FirstOrDefault(x => x.Name == entry.Tag.Name);

                if (aff != null) {
                    entry.AffordanceCategory = aff.AffCategory;
                }
            }
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var entry in _entries) {
                entry.SaveToDB();
            }
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", Description);
            if (_taggingSet != null) {
                cmd.AddParameter("AffordanceTaggingSetID", _taggingSet.IntID);
            }
            if (_calcObject != null) {
                cmd.AddParameter("CalcObjectID", _calcObject.IntID);
                cmd.AddParameter("CalcObjectType", _calcObject.CalcObjectType);
            }
        }

        public override string ToString() => Name;
        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((HouseholdPlan)toImport,dstSim);

        [ItemNotNull]
        [NotNull]
        public override List<UsedIn> CalculateUsedIns([NotNull] Simulator sim) => throw new NotImplementedException();
    }
}