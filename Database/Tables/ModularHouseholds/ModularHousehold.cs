//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IJsonSerializable<T> {
        [NotNull]
        T GetJson();
    }
    public class ModularHousehold : DBBaseElement, ICalcObject, IJsonSerializable<ModularHousehold.JsonModularHousehold>
    {
        [UsedImplicitly]
        public static void ImportObjectFromJson([NotNull] Simulator sim, [NotNull] [ItemNotNull] List<JsonModularHousehold> jsonHouseholds)
        {

            foreach (JsonModularHousehold jsonHH in jsonHouseholds)
            {
                ModularHousehold mhh = sim.ModularHouseholds.FindByGuid(jsonHH.Guid);
                if (mhh == null)
                {
                    Logger.Info(jsonHH.Name + " not found, creating...");
                    mhh = sim.ModularHouseholds.CreateNewItem(sim.ConnectionString);
                }

                mhh.ImportFromJsonTemplate(jsonHH, sim);
            }
        }
        public class JsonModularHousehold {
            public JsonModularHousehold([NotNull] string name, [CanBeNull] string description, StrGuid guid, CreationType creationType, [CanBeNull] JsonReference deviceSelection, EnergyIntensityType energyIntensityType, JsonReference vacation)
            {
                Name = name;
                Description = description;
                Guid = guid;
                CreationType = creationType;
                DeviceSelection = deviceSelection;
                EnergyIntensityType = energyIntensityType;
                Vacation = vacation;
            }

            /// <summary>
            /// no json
            /// </summary>
            [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
            [Obsolete("Only for json")]
            public JsonModularHousehold()
            {
            }
            public JsonReference Vacation { get; set; }
            [NotNull]
            public string Name { get; set; }

            [CanBeNull]
            public string Description { get; set; }

            public StrGuid Guid { get; set; }

            [NotNull]
            [ItemNotNull]
            public List<JsonReference> HouseholdTags { get; set; } = new List<JsonReference>();

            [NotNull]
            [ItemNotNull]
            public List<ModularHouseholdPerson.JsonModularHouseholdPerson> Persons { get; set; } = new List<ModularHouseholdPerson.JsonModularHouseholdPerson>();

            [NotNull]
            [ItemNotNull]
            public List<ModularHouseholdTrait.JsonModularHouseholdTrait> Traits { get; set; } = new List<ModularHouseholdTrait.JsonModularHouseholdTrait>();

            public CreationType CreationType { get; set; }
            [CanBeNull]
            public JsonReference DeviceSelection { get; set; }
            public EnergyIntensityType EnergyIntensityType { get; set; }
        }

        public JsonModularHousehold GetJson()
        {
            JsonModularHousehold     jec = new JsonModularHousehold(Name,
                Description,Guid,CreationType, DeviceSelection?.GetJsonReference(),
                EnergyIntensityType, Vacation?.GetJsonReference());

            foreach (var person in Persons) {
                jec.Persons.Add(person.GetJson());
            }

            foreach (var householdTrait in Traits) {
                jec.Traits.Add(householdTrait.GetJson());
            }

            foreach (var householdTag in ModularHouseholdTags) {
                jec.HouseholdTags.Add(householdTag.Tag.GetJsonReference());
            }
            return jec;

        }

        public void ImportFromJsonTemplate([NotNull] JsonModularHousehold jsonHH, [NotNull] Simulator sim)
        {
            Logger.Info("Adjusting values based on Json-Data for " + jsonHH.Name);
            Name = jsonHH.Name;
            Description = jsonHH.Description??"";
            CreationType = jsonHH.CreationType;
            Guid = jsonHH.Guid;
            if (jsonHH.DeviceSelection != null) {
                var devsel =sim.DeviceSelections.FindByGuid(jsonHH.DeviceSelection.Guid);
                DeviceSelection = devsel ?? throw new LPGException("Could not find the device selection with the guid " + jsonHH.DeviceSelection.Guid + " while importing households");
            }
            EnergyIntensityType = jsonHH.EnergyIntensityType;
            if (jsonHH.Vacation != null) {
                var vac = sim.Vacations.FindByGuid(jsonHH.Vacation.Guid);
                Vacation = vac;
            }
            //persons
            SynchronizeList(Persons, jsonHH.Persons, out var personsToCreate);
            foreach (var personJson in personsToCreate)
            {
                var person = sim.Persons.FindByGuid(personJson.Person.Guid);
                if (person == null)
                {
                    throw new LPGException("Person with the guid " + personJson.Person.Guid + " and the name " + personJson.Person.Name + " could not be found in the database.");
                }

                LivingPatternTag lptag = null;
                if(personJson.LivingPatternTag != null)
                {
                    lptag = sim.LivingPatternTags.FindByGuid(personJson.LivingPatternTag.Guid);
                }
                var hhp = AddPerson(person, lptag);
                if(hhp!= null) {
                    hhp.Guid = personJson.Guid;
                }
            }

            foreach (var person in Persons)
            {
                var jtp = jsonHH.Persons.Single(x => person.Guid == x.Guid);
                person.SynchronizeDataFromJson(jtp, sim);
            }

            //traits
            SynchronizeList(Traits, jsonHH.Traits, out var traitsToCreate);
            foreach (var traitJson in traitsToCreate)
            {
                var trait = sim.HouseholdTraits.FindByGuid(traitJson.HouseholdTrait?.Guid);
                if (trait == null)
                {
                    throw new LPGException("TraitTag " + traitJson.HouseholdTrait + " could not be found in the database.");
                }

                Person person = null;
                if (traitJson.DstPerson != null)
                {
                    person = sim.Persons.FindByGuid(traitJson.DstPerson.Guid);
                    if(person == null)
                    {
                        throw new LPGException("Person " +  traitJson.DstPerson + " was not found");
                    }
                }
                var t = AddTrait(trait,traitJson.AssignType, person);
                if (t == null) {
                    throw new LPGException("Could not add new trait");
                }
                t.Guid = traitJson.Guid;
            }

            foreach (var trait in Traits)
            {
                var jtp = jsonHH.Traits.Single(x => trait.Guid == x.Guid);
                trait.SynchronizeDataFromJson(jtp, sim);
            }

            //traits
            SynchronizeList(ModularHouseholdTags, jsonHH.HouseholdTags, out var tagsToCreate);
            foreach (var tagJsonRef in tagsToCreate)
            {
                var tag = sim.HouseholdTags.FindByGuid(tagJsonRef.Guid);
                if (tag == null)
                {
                    throw new LPGException("Tag " + tagJsonRef.Name + " could not be found in the database.");
                }
                AddHouseholdTag(tag);
            }

            foreach (var trait in Traits)
            {
                var jtp = jsonHH.Traits.Single(x => trait.Guid == x.Guid);
                trait.SynchronizeDataFromJson(jtp, sim);
            }
            SaveToDB();
        }

        public const string TableName = "tblModularHouseholds";
        private const string TableNameOld = "tblCombinedHouseholds";

        [ItemNotNull] [NotNull] private readonly ObservableCollection<ModularHouseholdTag> _modularHouseholdTags =
            new ObservableCollection<ModularHouseholdTag>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<ModularHouseholdPerson> _persons;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<PersonTimeEstimate> _personTimeEstimates =
            new ObservableCollection<PersonTimeEstimate>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<Person> _purePersons = new ObservableCollection<Person>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<ModularHouseholdTrait> _traits;
        private CreationType _creationType;
        [NotNull] private string _description;

        [CanBeNull] private DeviceSelection _deviceSelection;

        private EnergyIntensityType _energyIntensityType;
        [CanBeNull]
        private int? _generatorID;
        [NotNull] private string _source;

        [CanBeNull] private Vacation _vacation;

        public ModularHousehold([NotNull] string pName, [CanBeNull] int? id, [NotNull] string description, [NotNull] string connectionString,
            [CanBeNull] DeviceSelection deviceSelection, [NotNull] string source, [CanBeNull] int? generatorID, [CanBeNull] Vacation vacation,
            EnergyIntensityType energyIntensityType, CreationType creationType, StrGuid guid) : base(pName, TableName,
            connectionString, guid)
        {
            ID = id;
            _vacation = vacation;
            _energyIntensityType = energyIntensityType;
            _generatorID = generatorID;
            _source = source;
            _deviceSelection = deviceSelection;
            TypeDescription = "Modular Household";
            _description = description;
            _creationType = creationType;
            _traits = new ObservableCollection<ModularHouseholdTrait>();
            _persons = new ObservableCollection<ModularHouseholdPerson>();
        }

        [UsedImplicitly]
        public CreationType CreationType {
            get => _creationType;
            set => SetValueWithNotify(value, ref _creationType, nameof(CreationType));
        }
        public override bool IsValid(string filter)
        {
            if (filter == null)
            {
                throw new LPGException("isvalid failed, s = null");
            }
            if (PrettyName.ToUpperInvariant().Contains(filter.ToUpperInvariant()))
            {
                return true;
            }
            if (Description.ToUpperInvariant().Contains(filter.ToUpperInvariant())) {
                return true;
            }
            return false;
        }
        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [CanBeNull]
        [UsedImplicitly]
        public DeviceSelection DeviceSelection {
            get => _deviceSelection;
            set => SetValueWithNotify(value, ref _deviceSelection, false, nameof(DeviceSelection));
        }

        [UsedImplicitly]
        [CanBeNull]
        public int? GeneratorID {
            get => _generatorID;
            set => SetValueWithNotify(value, ref _generatorID, nameof(GeneratorID));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<ModularHouseholdTag> ModularHouseholdTags => _modularHouseholdTags;

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<ModularHouseholdPerson> Persons => _persons;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<PersonTimeEstimate> PersonTimeEstimates => _personTimeEstimates;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Person> PurePersons { get; } = new ObservableCollection<Person>();

        [NotNull]
        [UsedImplicitly]
        public string Source {
            get => _source;
            set => SetValueWithNotify(value, ref _source, nameof(Source));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<ModularHouseholdTrait> Traits => _traits;

        [UsedImplicitly]
        [CanBeNull]
        public Vacation Vacation {
            get => _vacation;
            set => SetValueWithNotify(value, ref _vacation,false, nameof(Vacation));
        }

        public ObservableCollection<Person> AllPersons {
            get {
                foreach (var modularHouseholdPerson in _persons) {
                    if (!_purePersons.Contains(modularHouseholdPerson.Person)) {
                        _purePersons.Add(modularHouseholdPerson.Person);
                    }
                }
                var todelete = new List<Person>();
                foreach (var purePerson in _purePersons) {
                    if (_persons.All(x => x.Person != purePerson)) {
                        todelete.Add(purePerson);
                    }
                }
                foreach (var person in todelete) {
                    _purePersons.Remove(person);
                }
                return _purePersons;
            }
        }

        public CalcObjectType CalcObjectType => CalcObjectType.ModularHousehold;

        public TimeSpan CalculateMaximumInternalTimeResolution() => new TimeSpan(0, 1, 0);

        public int CalculatePersonCount() => _persons.Count;

        public List<VLoadType> CollectLoadTypes(ObservableCollection<Affordance> affordances)
        {
            var vLoadTypes = new List<VLoadType>();
            foreach (var householdTrait in _traits) {
                var vlt = householdTrait.HouseholdTrait.GetLoadTypes(affordances);
                foreach (var loadType in vlt) {
                    if (!vLoadTypes.Contains(loadType)) {
                        vLoadTypes.Add(loadType);
                    }
                }
            }
            return vLoadTypes;
        }
        public GeographicLocation DefaultGeographicLocation => null;
        public TemperatureProfile DefaultTemperatureProfile => null;

        public EnergyIntensityType EnergyIntensityType {
            get => _energyIntensityType;
            set => SetValueWithNotify(value, ref _energyIntensityType, nameof(EnergyIntensityType));
        }


        public void AddHouseholdTag([NotNull] HouseholdTag householdTag)
        {
            if (_modularHouseholdTags.Any(x => x.Tag == householdTag)) {
                return;
            }
            var chhtag =
                new ModularHouseholdTag(null, householdTag, IntID, householdTag.Name, ConnectionString, System.Guid.NewGuid().ToStrGuid());
            chhtag.SaveToDB();
            _modularHouseholdTags.Add(chhtag);
        }

        [CanBeNull]
        public ModularHouseholdPerson AddPerson([NotNull] Person person,  LivingPatternTag lptag)
        {
            if (_persons.Any(x => x.Person == person)) {
                return null;
            }
            var chp = new ModularHouseholdPerson(null, IntID, person.Name, ConnectionString, person, lptag, System.Guid.NewGuid().ToStrGuid());
            PurePersons.Add(person);
            chp.SaveToDB();
            _persons.Add(chp);
            return chp;
        }

        [CanBeNull]
        public ModularHouseholdTrait AddTrait([NotNull] HouseholdTrait hht,
            ModularHouseholdTrait.ModularHouseholdTraitAssignType modularHouseholdTraitAssignType,
            [CanBeNull] Person person)
        {
            foreach (var modularHouseholdTrait in _traits) {
                if (modularHouseholdTrait.HouseholdTrait == hht &&
                    modularHouseholdTrait.AssignType == modularHouseholdTraitAssignType &&
                    modularHouseholdTrait.DstPerson == person) {
                    return null;
                }
            }

            var chht = new ModularHouseholdTrait(null, IntID, hht.Name, ConnectionString, hht, person,
                modularHouseholdTraitAssignType, System.Guid.NewGuid().ToStrGuid());
            _traits.Add(chht);
            _traits.Sort();
            SaveToDB();
            return chht;
        }

        [NotNull]
        private static ModularHousehold AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var householdID =  dr.GetIntFromLong("ID");
            var name = dr.GetString("Name","no name");
            var description = dr.GetString("Description", false);
            var source = dr.GetString("Source", false, "Manually created", ignoreMissingFields);
            var generatorID = dr.GetNullableIntFromLong("GeneratorID", false, ignoreMissingFields);
            var creationType = (CreationType) dr.GetIntFromLong("CreationType", false, ignoreMissingFields);
            var deviceSelectionID = dr.GetIntFromLong("DeviceSelectionID", false, ignoreMissingFields, -1);
            var deviceSelection =
                aic.DeviceSelections.FirstOrDefault(mySelection => mySelection.ID == deviceSelectionID);
            var vacationID = dr.GetIntFromLong("VacationID", false, ignoreMissingFields, -1);
            var vac = aic.Vacations.FirstOrDefault(x => x.ID == vacationID);
            var eit =
                (EnergyIntensityType) dr.GetIntFromLong("EnergyIntensity", false, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new ModularHousehold(name, householdID, description, connectionString,
                deviceSelection, source, generatorID, vac, eit, creationType, guid);
        }

        [ItemNotNull]
        [NotNull]
        public List<IAutonomousDevice> CollectAutonomousDevices()
        {
            var devices = new List<IAutonomousDevice>();
            foreach (var modularHouseholdTrait in _traits) {
                var autonomousDevices =
                    modularHouseholdTrait.HouseholdTrait.GetAllAutodevs();
                foreach (var autonomousDevice in autonomousDevices) {
                    var found = false;
                    foreach (var device in devices) {
                        if (device.Device == autonomousDevice.Device &&
                            device.Location == autonomousDevice.Location) {
                            found = true;
                        }
                    }
                    if (!found) {
                        devices.Add(autonomousDevice);
                    }
                }
            }
            return devices;
        }

        [ItemNotNull]
        [NotNull]
        public List<DeviceActionGroup> CollectDeviceActionGroups()
        {
            var dcs = new List<DeviceActionGroup>();
            foreach (var trait in _traits) {
                dcs.AddRange(trait.HouseholdTrait.CollectDeviceActionGroups());
            }
            var dcs2 = new List<DeviceActionGroup>();
            foreach (var deviceCategory in dcs) {
                if (!dcs2.Contains(deviceCategory)) {
                    dcs2.Add(deviceCategory);
                }
            }
            return dcs2;
        }

        [ItemNotNull]
        [NotNull]
        public List<DeviceCategory> CollectDeviceCategories()
        {
            var dcs = new List<DeviceCategory>();
            foreach (var modularHouseholdTrait in _traits) {
                dcs.AddRange(modularHouseholdTrait.HouseholdTrait.CollectDeviceCategories());
            }
            var dcs2 = new List<DeviceCategory>();
            foreach (var deviceCategory in dcs) {
                if (!dcs2.Contains(deviceCategory)) {
                    dcs2.Add(deviceCategory);
                }
            }
            return dcs2;
        }

        [ItemNotNull]
        [NotNull]
        public List<Location> CollectLocations()
        {
            var locations = new List<Location>();
            foreach (var householdTrait in _traits) {
                foreach (var location in householdTrait.HouseholdTrait.GetAllLocations()) {
                    if (!locations.Contains(location.Location)) {
                        locations.Add(location.Location);
                    }
                }
            }
            return locations;
        }

        public class PersonTraitDesireEntry {
            public PersonTraitDesireEntry(ModularHouseholdTrait.ModularHouseholdTraitAssignType assignType,[CanBeNull] Person person, [NotNull] HHTDesire hhtDesire, [NotNull] HouseholdTrait srcTrait)
            {
                AssignType = assignType;
                Person = person;
                HHTDesire = hhtDesire;
                SrcTrait = srcTrait;
            }

            public ModularHouseholdTrait.ModularHouseholdTraitAssignType AssignType { get; }
            [CanBeNull]
            public Person Person { get; }
            [NotNull]
            public HHTDesire HHTDesire { get; }

            [NotNull]
            public HouseholdTrait SrcTrait { get; }
        }

        [ItemNotNull]
        [NotNull]
        public List<PersonTraitDesireEntry> CollectTraitDesires()
        {
            var tuples =
                new List<PersonTraitDesireEntry>();
            foreach (var modularHouseholdTrait in _traits) {
                var desires = modularHouseholdTrait.HouseholdTrait.GetAllDesires();
                foreach (var hhtDesire in desires) {
                    var item =
                        new PersonTraitDesireEntry(modularHouseholdTrait.AssignType, modularHouseholdTrait.DstPerson, hhtDesire,
                        modularHouseholdTrait.HouseholdTrait);
                    tuples.Add(item);
                }
            }
            return tuples;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new
            ModularHousehold(FindNewName(isNameTaken, "New Modular Household "), null, "(no description yet)",
                connectionString, null, "Manually created", null, null, EnergyIntensityType.Random,
                CreationType.ManuallyCreated, System.Guid.NewGuid().ToStrGuid());

        public override void DeleteFromDB()
        {
            DeleteAllForOneParent(IntID, ModularHouseholdTrait.ParentIDField, ModularHouseholdTrait.TableName,
                ConnectionString);
            DeleteAllForOneParent(IntID, ModularHouseholdPerson.ParentIDField, ModularHouseholdPerson.TableName,
                ConnectionString);
            DeleteAllForOneParent(IntID, ModularHouseholdTag.ParentIDField, ModularHouseholdTag.TableName,
                ConnectionString);
            base.DeleteFromDB();
        }

        public void DeleteTag([NotNull] ModularHouseholdTag tag)
        {
            _modularHouseholdTags.Remove(tag);
            tag.DeleteFromDB();
        }

        public void DeleteTraitFromDB([NotNull] ModularHouseholdTrait chht)
        {
            chht.DeleteFromDB();
            _traits.Remove(chht);
        }

        [NotNull]
        public List<AffordanceWithTimeLimit> GetAllAffordancesForLocation([NotNull] Location loc)
        {
            var affordances = new List<AffordanceWithTimeLimit>();
            foreach (var trait in _traits) {
                string assignedTo = "";
                if (trait.AssignType == ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name && trait.DstPerson!=null) {
                    assignedTo = trait.DstPerson.PrettyName;
                }
                var affs = trait.HouseholdTrait.CollectAffordancesForLocation(loc,assignedTo,trait.DstPerson);

                foreach (var aff in affs) {
                    if (!affordances.Any(x => x.Equals(aff))) {
                        affordances.Add(aff);
                    }
                }
            }
            return affordances;
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((ModularHousehold)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var usedIns = new List<UsedIn>();
            foreach (var house in sim.Houses.Items) {
                if (house.Households.Any(x => x.CalcObject == this)) {
                    usedIns.Add(new UsedIn(house, "House"));
                }
            }
            foreach (var settlement in sim.Settlements.Items) {
                if (settlement.Households.Any(x => x.CalcObject == this)) {
                    usedIns.Add(new UsedIn(settlement, "Settlement"));
                }
            }
            return usedIns;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] ModularHousehold item,  [NotNull] Simulator dstSim)
        {
            DeviceSelection ds = null;
            Vacation vac = null;
            if (item._deviceSelection != null) {
                ds = GetItemFromListByName(dstSim.DeviceSelections.Items, item._deviceSelection.Name);
            }
            if (item._vacation != null) {
                vac = GetItemFromListByName(dstSim.Vacations.Items, item._vacation.Name);
            }
            var chh = new ModularHousehold(item.Name, null, item.Description, dstSim.ConnectionString, ds,
                item.Source, item.GeneratorID, vac, item.EnergyIntensityType, item._creationType, item.Guid);
            chh.SaveToDB();
            foreach (var trait in item.Traits) {
                var newtrait =
                    GetItemFromListByName(dstSim.HouseholdTraits.Items, trait.HouseholdTrait.Name);
                if (trait.DstPerson != null && newtrait != null) {
                    var p = GetItemFromListByName(dstSim.Persons.Items, trait.DstPerson.Name);
                    if (p == null) {
                        Logger.Error("While importing, could not find a person. Skipping.");
                        continue;
                    }
                    chh.AddTrait(newtrait, trait.AssignType, p);
                }
            }
            foreach (var modularHouseholdPerson in item.Persons) {
                var p = GetItemFromListByName(dstSim.Persons.Items, modularHouseholdPerson.Person.Name);
                if (p == null) {
                    Logger.Error("While importing, could not find a person. Skipping.");
                    continue;
                }
                var lpTag = GetItemFromListByName(dstSim.LivingPatternTags.Items, modularHouseholdPerson.LivingPatternTag?.Name);
                chh.AddPerson(p, lpTag);
            }
            foreach (var householdTag in item.ModularHouseholdTags) {
                var tag = GetItemFromListByName(dstSim.HouseholdTags.Items, householdTag.Tag.Name);
                if (tag == null) {
                    Logger.Error("While importing, could not find a tag. Skipping.");
                    continue;
                }

                chh.AddHouseholdTag(tag);
            }
            chh.SaveToDB();
            return chh;
        }

        public void ImportModularHousehold([NotNull] ModularHousehold other)
        {
            Description = other.Description;
            DeviceSelection = other.DeviceSelection;
            Vacation = other.Vacation;
            Source = other.Source;
            EnergyIntensityType = other.EnergyIntensityType;
            SaveToDB();
            foreach (var chhp in other.Persons) {
                AddPerson(chhp.Person,  chhp.LivingPatternTag);
            }
            foreach (var trait in other.Traits) {
                AddTrait(trait.HouseholdTrait, trait.AssignType, trait.DstPerson);
            }
            foreach (var modularHouseholdTag in other.ModularHouseholdTags) {
                AddHouseholdTag(modularHouseholdTag.Tag);
            }
            SaveToDB();
        }

        private static bool IsCorrectModularHouseholdPersonParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var chhp = (ModularHouseholdPerson) child;
            if (parent.ID == chhp.ModularHouseholdID) {
                var chh = (ModularHousehold) parent;
                chh.Persons.Add(chhp);
                chh.PurePersons.Add(chhp.Person);
                return true;
            }
            return false;
        }

        private static bool IsCorrectModularHouseholdTagParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var chht = (ModularHouseholdTag) child;
            if (parent.ID == chht.ModularHouseholdID) {
                var chh = (ModularHousehold) parent;
                chh.ModularHouseholdTags.Add(chht);
                return true;
            }
            return false;
        }

        private static bool IsCorrectModularHouseholdTraitParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var chht = (ModularHouseholdTrait) child;
            if (parent.ID == chht.ModularHouseholdID) {
                var chh = (ModularHousehold) parent;
                chh.Traits.Add(chht);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<ModularHousehold> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTrait> householdTraits,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceSelection> deviceSelections, bool ignoreMissingTables,
            [ItemNotNull] [NotNull] ObservableCollection<Person> persons, [ItemNotNull] [NotNull] ObservableCollection<Vacation> allVacations,
            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTag> hhTags, [ItemNotNull] [NotNull] ObservableCollection<TraitTag> traitTags,
                                            [ItemNotNull][NotNull] ObservableCollection<LivingPatternTag> livingPatternTags)
        {
            var aic = new AllItemCollections(householdTraits: householdTraits,
                deviceSelections: deviceSelections, persons: persons, vacations: allVacations);
            var loadResult = LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic,
                ignoreMissingTables, true);
            if (ignoreMissingTables && loadResult == LoadResults.TableNotFound) {
                LoadAllFromDatabase(result, connectionString, TableNameOld, AssignFields, aic, ignoreMissingTables,
                    true);
            }
            // traits
            var modularHouseholdTraits =
                new ObservableCollection<ModularHouseholdTrait>();
            ModularHouseholdTrait.LoadFromDatabase(modularHouseholdTraits, connectionString, householdTraits,
                ignoreMissingTables, persons);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(modularHouseholdTraits),
                IsCorrectModularHouseholdTraitParent, ignoreMissingTables);

            var modularHouseholdPersons =
                new ObservableCollection<ModularHouseholdPerson>();
            ModularHouseholdPerson.LoadFromDatabase(modularHouseholdPersons, connectionString, ignoreMissingTables,
                persons, traitTags, livingPatternTags:livingPatternTags);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(modularHouseholdPersons),
                IsCorrectModularHouseholdPersonParent, ignoreMissingTables);

            var modularHouseholdTags =
                new ObservableCollection<ModularHouseholdTag>();
            ModularHouseholdTag.LoadFromDatabase(modularHouseholdTags, connectionString, hhTags, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(modularHouseholdTags),
                IsCorrectModularHouseholdTagParent, ignoreMissingTables);

            // sort
            foreach (var chht in result) {
                chht.Traits.Sort();
            }
            // cleanup
            result.Sort();
        }

        public void RefreshPersonTimeEstimates()
        {
            _personTimeEstimates.Clear();
            var persons = PurePersons.ToList();
            if (persons.Count == 0) {
                throw new DataIntegrityException("No persons in " + PrettyName);
            }
            double vacationTime = 0;
            if (_vacation != null) {
                vacationTime = _vacation.DurationInH;
            }
            foreach (var person in persons) {
                var traitsForPerson = Traits.Where(x => x.DstPerson == person).ToList();
                var timesum = traitsForPerson.Sum(x => x.HouseholdTrait.EstimatedTimePerYearInH);
                var traitsWithoputEstimate = traitsForPerson
                    .Where(x => x.HouseholdTrait.EstimateType == EstimateType.Theoretical)
                    .ToList();
                var traitNames = "";
                foreach (var trait in traitsWithoputEstimate) {
                    traitNames += trait.PrettyName + ", ";
                }
                if (traitNames.Length > 2) {
                    traitNames = traitNames.Substring(0, traitNames.Length - 2);
                }
                var pte = new PersonTimeEstimate(person.PrettyName, timesum, vacationTime,
                    traitsWithoputEstimate.Count, traitNames);
                _personTimeEstimates.Add(pte);
            }
        }

        public void RemovePerson([NotNull] ModularHouseholdPerson chp)
        {
            _persons.Remove(chp);
            chp.DeleteFromDB();
            PurePersons.Remove(chp.Person);
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var trait in _traits) {
                trait.SaveToDB();
            }
            foreach (var tag in _modularHouseholdTags) {
                tag.SaveToDB();
            }
            foreach (var modularHouseholdPerson in _persons) {
                modularHouseholdPerson.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", "@Description", _description);
            cmd.AddParameter("Source", _source);
            cmd.AddParameter("CreationType", (int) _creationType);
            if (_generatorID != null) {
                cmd.AddParameter("GeneratorID", _generatorID);
            }
            cmd.AddParameter("EnergyIntensity", _energyIntensityType);
            if (_deviceSelection != null) {
                cmd.AddParameter("DeviceSelectionID", _deviceSelection.IntID);
            }
            if (_vacation != null) {
                cmd.AddParameter("VacationID", _vacation.IntID);
            }
        }

        public void SwapPersons([NotNull] ModularHouseholdPerson srcPerson,
                                [NotNull] Person dstPerson,  LivingPatternTag lptag)
        {
            var traits2Delete = Traits.Where(x => x.DstPerson == srcPerson.Person).ToList();
            foreach (var trait in traits2Delete) {
                DeleteTraitFromDB(trait);
            }

            RemovePerson(srcPerson);
            AddPerson(dstPerson,  lptag);
            foreach (var trait in traits2Delete) {
                AddTrait(trait.HouseholdTrait, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name,
                    dstPerson);
            }
        }
    }
}