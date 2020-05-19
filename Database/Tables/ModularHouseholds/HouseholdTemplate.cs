#region header

/*
//  ProfileGenerator DatabaseIO changed: 2015 04 23 21:21
*/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Templating;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public enum TemplateVacationType {
        FromList,
        RandomlyGenerated
    }


    public interface IRelevantGuidProvider {
        StrGuid RelevantGuid { get; }
    }

    public interface IJSonSubElement<T>: IJsonSerializable<T>, IGuidObject, IRelevantGuidProvider {

        void SynchronizeDataFromJson([NotNull] T json, [NotNull] Simulator sim);
    }

    public class HouseholdTemplate : DBBaseElement, IJsonSerializable<HouseholdTemplate.JsonDto> {
        public const string TableName = "tblHouseholdTemplates";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTemplateEntry> _entries = new ObservableCollection<HHTemplateEntry>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTemplatePerson> _persons = new ObservableCollection<HHTemplatePerson>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTemplateTag> _templateTags = new ObservableCollection<HHTemplateTag>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTemplateVacation> _vacations = new ObservableCollection<HHTemplateVacation>();

        private int _averageVacationDuration;

        private int _count;

        [CanBeNull] private string _description;
        private int _maxNumberOfVacations;
        private int _maxTotalVacationDays;
        private int _minNumberOfVacations;
        private int _minTotalVacationDays;
        [CanBeNull] private string _newHHName;
        private TemplateVacationType _templateVacationType;

        [CanBeNull] private DateBasedProfile _timeProfileForVacations;

        public HouseholdTemplate([NotNull] string pName, [CanBeNull] int? id, [CanBeNull] string description, [NotNull] string connectionString, [CanBeNull] string newName, int count,
                                 [CanBeNull] DateBasedProfile timeProfileForVacations, TemplateVacationType templateVacationType, int minNumberOfVacations, int maxNumberOfVacations,
                                 int averageVacationDuration, int minTotalVacationDays, int maxTotalVacationDays, StrGuid guid) : base(pName, TableName, connectionString, guid)
        {
            _templateVacationType = templateVacationType;
            _minNumberOfVacations = minNumberOfVacations;
            _maxNumberOfVacations = maxNumberOfVacations;
            _averageVacationDuration = averageVacationDuration;
            _minTotalVacationDays = minTotalVacationDays;
            _maxTotalVacationDays = maxTotalVacationDays;
            _timeProfileForVacations = timeProfileForVacations;
            ID = id;
            _count = count;
            _newHHName = newName;
            TypeDescription = "Household Template";
            _description = description;
        }

        public int AverageVacationDuration {
            get => _averageVacationDuration;
            set => SetValueWithNotify(value, ref _averageVacationDuration, nameof(AverageVacationDuration));
        }

        public int Count {
            get => _count;
            set => SetValueWithNotify(value, ref _count, nameof(Count));
        }

        [UsedImplicitly]
        [CanBeNull]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [UsedImplicitly]
        public EnergyIntensityType EnergyIntensityType { get; set; } = EnergyIntensityType.Random;

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HHTemplateEntry> Entries => _entries;

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<ModularHousehold> GeneratedHouseholds { get; } = new ObservableCollection<ModularHousehold>();

        public int MaxNumberOfVacations {
            get => _maxNumberOfVacations;
            set => SetValueWithNotify(value, ref _maxNumberOfVacations, nameof(MaxNumberOfVacations));
        }

        public int MaxTotalVacationDays {
            get => _maxTotalVacationDays;
            set => SetValueWithNotify(value, ref _maxTotalVacationDays, nameof(MaxTotalVacationDays));
        }

        public int MinNumberOfVacations {
            get => _minNumberOfVacations;
            set => SetValueWithNotify(value, ref _minNumberOfVacations, nameof(MinNumberOfVacations));
        }

        public int MinTotalVacationDays {
            get => _minTotalVacationDays;
            set => SetValueWithNotify(value, ref _minTotalVacationDays, nameof(MinTotalVacationDays));
        }

        [CanBeNull]
        [UsedImplicitly]
        public string NewHHName {
            get => _newHHName;
            set => SetValueWithNotify(value, ref _newHHName, nameof(NewHHName));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HHTemplatePerson> Persons => _persons;

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HHTemplateTag> TemplateTags => _templateTags;

        public TemplateVacationType TemplateVacationType {
            get => _templateVacationType;
            set => SetValueWithNotify(value, ref _templateVacationType, nameof(TemplateVacationType));
        }

        [CanBeNull]
        public DateBasedProfile TimeProfileForVacations {
            get => _timeProfileForVacations;
            set => SetValueWithNotify(value, ref _timeProfileForVacations, false, nameof(TimeProfileForVacations));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HHTemplateVacation> Vacations => _vacations;

        public JsonDto GetJson()
        {
            JsonDto jec = new JsonDto(Name, Description, NewHHName, Count, TimeProfileForVacations?.GetJsonReference(), TemplateVacationType, MinNumberOfVacations,
                MaxNumberOfVacations, AverageVacationDuration, MinTotalVacationDays, MaxTotalVacationDays, Guid) {TraitEntries = new List<HHTemplateEntry.JsonDto>()};
            foreach (HHTemplateEntry entry in _entries) {
                jec.TraitEntries.Add(entry.GetJson());
            }

            jec.TemplatePersons = new List<HHTemplatePerson.JsonDto>();
            foreach (HHTemplatePerson person in _persons) {
                jec.TemplatePersons.Add(person.GetJson());
            }

            jec.HouseholdTags = new List<JsonReference>();
            foreach (HHTemplateTag hhTemplateTag in _templateTags) {
                jec.HouseholdTags.Add(hhTemplateTag.Tag.GetJsonReference());
            }

            jec.Vacations = new List<JsonReference>();
            foreach (HHTemplateVacation vacation in Vacations) {
                jec.Vacations.Add(vacation.Vacation.GetJsonReference());
            }

            return jec;
        }
        [NotNull]
        public HHTemplatePerson AddPersonFromJson([NotNull]HHTemplatePerson.JsonDto jto, [NotNull] Simulator sim)
        {
            var person = sim.Persons.FindByGuid(jto.PersonReference?.Guid) ?? throw new LPGException("Could not find the person " + jto.PersonReference);
            var livingPattern = sim.TraitTags.FindByGuid(jto.LivingPatternTraitTagReference?.Guid);
            var p = AddPerson(person, livingPattern) ?? throw new LPGException("Could not add person " + jto.PersonReference);
            p.Name = jto.Name;
            p.Guid = jto.Guid;
            p.SaveToDB();
            return p;
        }

        [NotNull]
        public HHTemplateEntry AddEntryFromJson([NotNull] HHTemplateEntry.JsonDto jto, [NotNull] Simulator sim)
        {

            var tag = sim.TraitTags.FindByGuid(jto.TraitTagReference?.Guid)??throw new LPGException("could not find trait tag " + jto.TraitTagReference);
            var entry = AddEntry(tag, jto.TraitCountMin, jto.TraitCountMax);
            entry.Guid = jto.Guid;
            entry.SaveToDB();
            foreach (JsonReference personRef in jto.Persons)
            {
                var person = sim.Persons.FindByGuid(personRef.Guid) ?? throw new LPGException("Person not found");
                entry.AddPerson(person);
            }
            entry.Persons.Sort();
            return entry;
        }
        [NotNull]
        public HHTemplateEntry AddEntry([NotNull] TraitTag tag, int min, int max)
        {
            var entry = new HHTemplateEntry(null, IntID, "newEntry", ConnectionString, tag, min, max, System.Guid.NewGuid().ToStrGuid());
            _entries.Add(entry);
            entry.SaveToDB();
            _entries.Sort();
            return entry;
        }

        [NotNull]
        public HHTemplateEntry AddEntry([NotNull] TraitTag tag, int min, int max, [ItemNotNull] [NotNull] List<Person> persons)
        {
            persons.Sort();
            for (var index = 0; index < _entries.Count; index++) {
                var hhgEntry = _entries[index];
                var entrypersons = hhgEntry.Persons.Select(x => x.Person).ToList();
                entrypersons.Sort();

                if (entrypersons.SequenceEqual(persons) && hhgEntry.TraitTag == tag) {
                    DeleteEntryFromDB(hhgEntry);
                    index = 0;
                }
            }

            var entry = new HHTemplateEntry(null, IntID, "newEntry", ConnectionString, tag, min, max, System.Guid.NewGuid().ToStrGuid());

            _entries.Add(entry);
            entry.SaveToDB();
            foreach (var person in persons) {
                entry.AddPerson(person);
            }

            _entries.Sort();
            return entry;
        }

        [CanBeNull]
        public HHTemplatePerson AddPerson([NotNull] Person p, [CanBeNull] TraitTag tag)
        {
            foreach (var hhTemplatePerson in _persons) {
                if (hhTemplatePerson.Person == p) {
                    Logger.Error("The person " + p.PrettyName + " was already added.");
                    return null;
                }
            }

            var entry = new HHTemplatePerson(null, p, IntID, "...", ConnectionString, tag, System.Guid.NewGuid().ToStrGuid());
            _persons.Add(entry);
            entry.SaveToDB();
            _persons.Sort();
            return entry;
        }
        [CanBeNull]
        public HHTemplateTag AddTemplateTagFromJson([NotNull] JsonReference myReference, [NotNull] Simulator sim)
        {
            var tag = sim.HouseholdTags.FindByJsonReference(myReference) ?? throw new LPGException("Tag not found : " + myReference);

            return AddTemplateTag(tag);
        }
        [CanBeNull]
        public HHTemplateTag AddTemplateTag([NotNull] HouseholdTag tag)
        {
            var existingTags = _templateTags.Where(x => x.Tag == tag);
            if (existingTags.Any()) {
                return null;
            }

            var entry = new HHTemplateTag(null, tag,
                IntID, tag.Name, ConnectionString, System.Guid.NewGuid().ToStrGuid());
            entry.SaveToDB();
            _templateTags.Add(entry);
            _templateTags.Sort((x, y) => {
                if (x == null || y == null) {
                    return 0;
                }

                var val = string.Compare(x.Tag.Name, y.Tag.Name, StringComparison.Ordinal);
                return val;
            });
            return entry;
        }

        [CanBeNull]
        public HHTemplateVacation AddVacationFromJson([NotNull] JsonReference myReference, [NotNull] Simulator sim)
        {
            var vacation = sim.Vacations.FindByJsonReference(myReference)??throw new LPGException("Vacation not found : " + myReference);
            return AddVacation(vacation);
        }
        [CanBeNull]
        public HHTemplateVacation AddVacation([NotNull] Vacation v)
        {
            foreach (var vac in _vacations) {
                if (vac.Vacation == v) {
                    Logger.Error("The vacation " + v.PrettyName + " was already added.");
                    return null;
                }
            }

            var entry = new HHTemplateVacation(null, v, IntID, "...", ConnectionString, System.Guid.NewGuid().ToStrGuid());
            _vacations.Add(entry);
            entry.SaveToDB();
            _vacations.Sort();
            return entry;
        }

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new HouseholdTemplate(FindNewName(isNameTaken, "New  Household Template"),
            null, "(no description yet)", connectionString, "Automatically generated Household", 10, null, TemplateVacationType.FromList, 1, 3, 7, 10, 14, System.Guid.NewGuid().ToStrGuid());

        public void DeleteEntryFromDB([NotNull] HHTemplateEntry entry)
        {
            entry.DeleteFromDB();
            _entries.Remove(entry);
        }

        public override void DeleteFromDB()
        {
            Config.ShowDeleteMessages = false;
            foreach (var entry in _entries) {
                entry.DeleteFromDB();
            }

            foreach (var person in _persons) {
                person.DeleteFromDB();
            }

            foreach (var vacation in _vacations) {
                vacation.DeleteFromDB();
            }

            foreach (var hhTemplateTag in _templateTags) {
                hhTemplateTag.DeleteFromDB();
            }

            Config.ShowDeleteMessages = true;
            base.DeleteFromDB();
        }

        public void DeletePersonFromDB([NotNull] HHTemplatePerson entry)
        {
            entry.DeleteFromDB();
            _persons.Remove(entry);
        }

        public void DeleteTagFromDB([NotNull] HHTemplateTag tag)
        {
            tag.DeleteFromDB();
            _templateTags.Remove(tag);
        }

        public void DeleteVacationFromDB([NotNull] HHTemplateVacation entry)
        {
            entry.DeleteFromDB();
            _vacations.Remove(entry);
        }

        [ItemNotNull]
        [NotNull]
        public List<ModularHousehold> GenerateHouseholds([NotNull] Simulator sim, bool generateSettlement, [ItemNotNull] [NotNull] List<STTraitLimit> limits) =>
            HouseholdTemplateExecutor.GenerateHouseholds(sim, generateSettlement, limits, this);

        public void ImportExistingModularHouseholds([NotNull] ModularHousehold chh)
        {
            var traitcounts = new Dictionary<Person, Dictionary<TraitTag, int>>();
            foreach (var trait in chh.Traits) {
                var traitDstPerson = trait.DstPerson;
                if (traitDstPerson == null) {
                    throw new LPGException("DstPerson was null");
                }

                if (!traitcounts.ContainsKey(traitDstPerson)) {
                    traitcounts.Add(traitDstPerson, new Dictionary<TraitTag, int>());
                }

                var traitHouseholdTrait = trait.HouseholdTrait;
                var dic = traitcounts[traitDstPerson];
                Logger.Info("Found Trait:" + trait.HouseholdTrait.Name + " for " + trait.DstPerson);

                //add all the tags to the trait tag count dictionary
                foreach (var tag in traitHouseholdTrait.Tags) {
                    if (tag.Name.ToLower().StartsWith("living pattern")) {
                        continue;
                    }

                    if (tag.Name.ToLower().StartsWith("web /")) {
                        continue;
                    }

                    if (!dic.ContainsKey(tag.Tag)) {
                        dic.Add(tag.Tag, 0);
                    }

                    dic[tag.Tag]++;
                }
            }

            foreach (var persondict in traitcounts) {
                var p = persondict.Key;
                var ps = new List<Person> {
                    p
                };
                foreach (var tagdict in persondict.Value) {
                    AddEntry(tagdict.Key, tagdict.Value, tagdict.Value, ps);
                }
            }

            // add mandatory traits
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim) => ImportFromItem((HouseholdTemplate)toImport, dstSim);

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] HouseholdTemplate item, [NotNull] Simulator dstSim)
        {
            DateBasedProfile dbp = null;
            if (item.TimeProfileForVacations != null) {
                dbp = GetItemFromListByName(dstSim.DateBasedProfiles.It, item.TimeProfileForVacations.Name);
            }

            var hhg = new HouseholdTemplate(item.Name, null, item.Description, dstSim.ConnectionString, item.NewHHName, item.Count, dbp, item.TemplateVacationType, item.MinNumberOfVacations,
                item.MaxNumberOfVacations, item.AverageVacationDuration, item.MinTotalVacationDays, item.MaxTotalVacationDays, item.Guid);
            hhg.SaveToDB();
            foreach (var entry in item.Entries) {
                {
                    var tag = GetItemFromListByName(dstSim.TraitTags.It, entry.TraitTag.Name);
                    if (tag == null) {
                        Logger.Error("Found a missing tag. Skipping.");
                        continue;
                    }

                    var newEntry = new HHTemplateEntry(null, hhg.IntID, "no name", dstSim.ConnectionString, tag, entry.TraitCountMin, entry.TraitCountMax, entry.Guid);
                    newEntry.SaveToDB();
                    foreach (var person in entry.Persons) {
                        var p = GetItemFromListByName(dstSim.Persons.It, person.Person.Name);
                        if (p == null) {
                            Logger.Error("Found a missing person. Skipping.");
                            continue;
                        }

                        newEntry.AddPerson(p);
                    }

                    hhg.Entries.Add(newEntry);
                }
            }

            foreach (var person in item.Persons) {
                var p = GetItemFromListByName(dstSim.Persons.It, person.Person.Name);

                if (p == null) {
                    Logger.Error("Found a missing person. Skipping.");
                    continue;
                }

                TraitTag traittag = null;
                if (person.LivingPattern != null) {
                    traittag = GetItemFromListByName(dstSim.TraitTags.It, person.LivingPattern.Name);
                }

                hhg.AddPerson(p, traittag);
            }

            foreach (var vacation in item.Vacations) {
                var p = GetItemFromListByName(dstSim.Vacations.It, vacation.Name);
                if (p == null) {
                    Logger.Error("Found a missing Vacation. Skipping.");
                    continue;
                }

                hhg.AddVacation(p);
            }

            foreach (var hhTemplateTag in item.TemplateTags) {
                var tag = GetItemFromListByName(dstSim.HouseholdTags.It, hhTemplateTag.Tag.Name);
                if (tag == null) {
                    Logger.Error("Found a missing tag. Skipping.");
                    continue;
                }

                hhg.AddTemplateTag(tag);
            }

            hhg.SaveToDB();
            return hhg;
        }

        public void ImportFromJsonTemplate([NotNull] JsonDto jsonTemplate, [NotNull] Simulator sim)
        {
            DateBasedProfile dbp = null;
            if (jsonTemplate.TimeProfileForVacations != null) {
                dbp = GetItemFromListByJsonReference(sim.DateBasedProfiles.It, jsonTemplate.TimeProfileForVacations);
            }

            Name = jsonTemplate.Name;
            Description = jsonTemplate.Description;
            NewHHName = jsonTemplate.NewHouseholdName;
            Count = jsonTemplate.Count;
            TimeProfileForVacations = dbp;
            TemplateVacationType = jsonTemplate.TemplateVacationType;
            MinNumberOfVacations = jsonTemplate.MinNumberOfVacations;
            MaxNumberOfVacations = jsonTemplate.MaxNumberOfVacations;
            AverageVacationDuration = jsonTemplate.AverageVacationDuration;
            MinTotalVacationDays = jsonTemplate.MinTotalVacationDays;
            MaxTotalVacationDays = jsonTemplate.MaxTotalVacationDays;
            Guid = jsonTemplate.Guid;
            SaveToDB();
            SynchronizeListWithCreation(Persons, jsonTemplate.TemplatePersons, AddPersonFromJson,sim);
            SynchronizeListWithCreation(Entries, jsonTemplate.TraitEntries,   AddEntryFromJson,sim);
            SynchronizeListWithCreation(Vacations, jsonTemplate.Vacations, AddVacationFromJson, sim);
            SynchronizeListWithCreation(TemplateTags, jsonTemplate.HouseholdTags, AddTemplateTagFromJson, sim);

            SaveToDB();
        }

        [UsedImplicitly]
        public static void ImportObjectFromJson([NotNull] Simulator sim, [NotNull] [ItemNotNull] List<JsonDto> jsonHouseholdTemplates)
        {
            foreach (JsonDto jsontemplate in jsonHouseholdTemplates) {
                HouseholdTemplate hhtemplate = sim.HouseholdTemplates.FindByGuid(jsontemplate.Guid);
                if (hhtemplate == null) {
                    hhtemplate = sim.HouseholdTemplates.CreateNewItem(sim.ConnectionString);
                }

                hhtemplate.ImportFromJsonTemplate(jsontemplate, sim);
            }
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HouseholdTemplate> result, [NotNull] string connectionString,
                                            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTrait> householdTraits, bool ignoreMissingTables,
                                            [ItemNotNull] [NotNull] ObservableCollection<Person> persons, [ItemNotNull] [NotNull] ObservableCollection<TraitTag> traitTags,
                                            [ItemNotNull] [NotNull] ObservableCollection<Vacation> vacations, [ItemNotNull] [NotNull] ObservableCollection<HouseholdTag> templateTags,
                                            [ItemNotNull] [NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles)
        {
            var aic = new AllItemCollections(householdTraits: householdTraits, persons: persons, dateBasedProfiles: dateBasedProfiles);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);

            var entries = new ObservableCollection<HHTemplateEntry>();
            HHTemplateEntry.LoadFromDatabase(entries, connectionString, traitTags, ignoreMissingTables, persons);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(entries), IsCorrectHHTemplateEntryParent, ignoreMissingTables);

            var hhTemplatePersons = new ObservableCollection<HHTemplatePerson>();
            HHTemplatePerson.LoadFromDatabase(hhTemplatePersons, connectionString, persons, ignoreMissingTables, traitTags);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hhTemplatePersons), IsCorrectHHTemplatePersonParent, ignoreMissingTables);

            var hhTemplateVacations = new ObservableCollection<HHTemplateVacation>();
            HHTemplateVacation.LoadFromDatabase(hhTemplateVacations, connectionString, vacations, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hhTemplateVacations), IsCorrectHHTemplateVacationParent, ignoreMissingTables);

            var hhTemplateTags = new ObservableCollection<HHTemplateTag>();
            HHTemplateTag.LoadFromDatabase(hhTemplateTags, connectionString, templateTags, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hhTemplateTags), IsCorrectHHTemplateTagParent, ignoreMissingTables);
            // sort
            foreach (var hhg in result) {
                hhg.Entries.Sort();
                hhg.Persons.Sort();
            }

            // cleanup
            result.Sort();
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var entry in _entries) {
                entry.SaveToDB();
            }

            foreach (var person in _persons) {
                person.SaveToDB();
            }

            foreach (var vacation in _vacations) {
                vacation.SaveToDB();
            }

            foreach (var tag in _templateTags) {
                tag.SaveToDB();
            }
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
            cmd.AddParameter("Count", _count);
            if (_description != null) {
                cmd.AddParameter("Description", _description);
            }

            if (_newHHName != null) {
                cmd.AddParameter("NewHHName", _newHHName);
            }

            if (_timeProfileForVacations != null) {
                cmd.AddParameter("ProfileForVacationsID", _timeProfileForVacations.IntID);
            }

            cmd.AddParameter("TemplateVacationType", (int)_templateVacationType);
            cmd.AddParameter("MaxNumberOfVacations", MaxNumberOfVacations);
            cmd.AddParameter("MinNumberOfVacations", _minNumberOfVacations);
            cmd.AddParameter("AverageVacationDuration", AverageVacationDuration);
            cmd.AddParameter("MinTotalVacationDays", MinTotalVacationDays);
            cmd.AddParameter("MaxTotalVacationDays", MaxTotalVacationDays);
        }

        [NotNull]
        private static HouseholdTemplate AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var hhid = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name", "no name");
            var description = dr.GetString("Description", false);
            var newhhname = dr.GetString("NewHHName", false);
            var count = dr.GetIntFromLong("Count", false);
            var profileForVacations = dr.GetIntFromLong("ProfileForVacationsID", false, ignoreMissingFields, -1);
            var dbp = aic.DateBasedProfiles.FirstOrDefault(x => x.IntID == profileForVacations);
            var templateVacationType = (TemplateVacationType)dr.GetIntFromLong("TemplateVacationType", false, ignoreMissingFields);
            var minNumberOfVacations = dr.GetIntFromLong("MinNumberOfVacations", false, ignoreMissingFields);
            var maxNumberOfVacations = dr.GetIntFromLong("MaxNumberOfVacations", false, ignoreMissingFields);
            var averageVacationDuration = dr.GetIntFromLong("AverageVacationDuration", false, ignoreMissingFields);
            var minTotalVacationDays = dr.GetIntFromLong("MinTotalVacationDays", false, ignoreMissingFields);
            var maxTotalVacationDays = dr.GetIntFromLong("MaxTotalVacationDays", false, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var chh = new HouseholdTemplate(name, hhid, description, connectionString, newhhname, count, dbp, templateVacationType, minNumberOfVacations, maxNumberOfVacations, averageVacationDuration,
                minTotalVacationDays, maxTotalVacationDays, guid);
            return chh;
        }

        private static bool IsCorrectHHTemplateEntryParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hhgEntry = (HHTemplateEntry)child;
            if (parent.ID == hhgEntry.HouseholdTemplateID) {
                var chh = (HouseholdTemplate)parent;
                chh.Entries.Add(hhgEntry);
                return true;
            }

            return false;
        }

        private static bool IsCorrectHHTemplatePersonParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hhgEntry = (HHTemplatePerson)child;
            if (parent.ID == hhgEntry.HHTemplateID) {
                var chh = (HouseholdTemplate)parent;
                chh.Persons.Add(hhgEntry);
                return true;
            }

            return false;
        }

        private static bool IsCorrectHHTemplateTagParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hhTemplateTag = (HHTemplateTag)child;
            if (parent.ID == hhTemplateTag.TemplateID) {
                var householdTemplate = (HouseholdTemplate)parent;
                householdTemplate.TemplateTags.Add(hhTemplateTag);
                return true;
            }

            return false;
        }

        private static bool IsCorrectHHTemplateVacationParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hhgEntry = (HHTemplateVacation)child;
            if (parent.ID == hhgEntry.HHTemplateID) {
                var chh = (HouseholdTemplate)parent;
                chh.Vacations.Add(hhgEntry);
                return true;
            }

            return false;
        }


        public class JsonDto {
            public JsonDto([NotNull] string name, [CanBeNull] string description, [CanBeNull] string newHouseholdName, int count, [CanBeNull] JsonReference timeProfileForVacations,
                           TemplateVacationType templateVacationType, int minNumberOfVacations, int maxNumberOfVacations, int averageVacationDuration, int minTotalVacationDays,
                           int maxTotalVacationDays, StrGuid guid)
            {
                Name = name;
                Description = description;
                NewHouseholdName = newHouseholdName;
                Count = count;
                TimeProfileForVacations = timeProfileForVacations;
                TemplateVacationType = templateVacationType;
                MinNumberOfVacations = minNumberOfVacations;
                MaxNumberOfVacations = maxNumberOfVacations;
                AverageVacationDuration = averageVacationDuration;
                MinTotalVacationDays = minTotalVacationDays;
                MaxTotalVacationDays = maxTotalVacationDays;
                Guid = guid;
            }

            /// <summary>
            ///     for json only
            /// </summary>
            [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
            public JsonDto()
            {
            }

            public int AverageVacationDuration { get; set; }
            public int Count { get; set; }

            [CanBeNull]
            public string Description { get; set; }

            public StrGuid Guid { get; set; }

            [NotNull]
            [ItemNotNull]
            public List<JsonReference> HouseholdTags { get; set; } = new List<JsonReference>();

            public int MaxNumberOfVacations { get; set; }
            public int MaxTotalVacationDays { get; set; }
            public int MinNumberOfVacations { get; set; }
            public int MinTotalVacationDays { get; set; }

            [NotNull]
            public string Name { get; set; }

            [CanBeNull]
            public string NewHouseholdName { get; set; }

            [NotNull]
            [ItemNotNull]
            public List<HHTemplatePerson.JsonDto> TemplatePersons { get; set; } = new List<HHTemplatePerson.JsonDto>();

            public TemplateVacationType TemplateVacationType { get; set; }

            [CanBeNull]
            public JsonReference TimeProfileForVacations { get; set; }

            [NotNull]
            [ItemNotNull]
            public List<HHTemplateEntry.JsonDto> TraitEntries { get; set; } = new List<HHTemplateEntry.JsonDto>();

            [NotNull]
            [ItemNotNull]
            public List<JsonReference> Vacations { get; set; } = new List<JsonReference>();
        }
    }
}