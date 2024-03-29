﻿//-----------------------------------------------------------------------

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
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Common.JSON;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Exception = System.Exception;

#endregion

namespace Database.Tables.Houses {
    public class Settlement : DBBaseElement, ICalcObject {
        public const string TableName = "tblSettlement";
        [JetBrains.Annotations.NotNull] [ItemNotNull] private readonly ObservableCollection<SettlementHH> _households;
        [JetBrains.Annotations.NotNull] private string _buildingType;
        [JetBrains.Annotations.NotNull] private JsonCalcSpecification _calcSpecification;
        [JetBrains.Annotations.NotNull] private string _character;
        private CreationType _creationType;

        [JetBrains.Annotations.NotNull] private string _description;
        //[JetBrains.Annotations.NotNull] private string _dstPath;

        [CanBeNull] private GeographicLocation _geographicLocation;

        [JetBrains.Annotations.NotNull] private string _location;
        [JetBrains.Annotations.NotNull] private string _popularWith;
        [JetBrains.Annotations.NotNull] private string _source;
        [CanBeNull] private TemperatureProfile _temperatureProfile;

        public Settlement([JetBrains.Annotations.NotNull] string pName,
                          [CanBeNull] int? id,
                          //[JetBrains.Annotations.NotNull] string dstPath,
                          [JetBrains.Annotations.NotNull] string character,
                          [JetBrains.Annotations.NotNull] string location,
                          [JetBrains.Annotations.NotNull] string popularWith,
                          [JetBrains.Annotations.NotNull] string buildingType,
                          [JetBrains.Annotations.NotNull] string description,
                          [JetBrains.Annotations.NotNull] string connectionString,
                          [CanBeNull] GeographicLocation geoLoc,
                          [CanBeNull] TemperatureProfile temperatureProfile,
                          [JetBrains.Annotations.NotNull] string source,
                          CreationType creationType,
                          [JetBrains.Annotations.NotNull] StrGuid guid,
                          [CanBeNull] JsonCalcSpecification calcSpecification) : base(pName, TableName, connectionString, guid)
        {
            _calcSpecification = calcSpecification ?? JsonCalcSpecification.MakeDefaultsForProduction();
            _geographicLocation = geoLoc;
            _temperatureProfile = temperatureProfile;
            ID = id;
            _character = character;
            _location = location;
            _popularWith = popularWith;
            _buildingType = buildingType;
            _description = description;
            _households = new ObservableCollection<SettlementHH>();
            TypeDescription = "Settlement";
            _households.CollectionChanged += HouseholdsCollectionChanged;
            _source = source;
            _creationType = creationType;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string BuildingType {
            get => _buildingType;

            set {
                if (_buildingType == value) {
                    return;
                }

                _buildingType = value;
                OnPropertyChanged(nameof(BuildingType));
            }
        }

        [CanBeNull]
        public IReadOnlyCollection<CalcOption> CalcOptions => _calcSpecification.CalcOptions?.AsReadOnly();

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string Character {
            get => _character;

            set {
                if (_character == value) {
                    return;
                }

                _character = value;
                OnPropertyChanged(nameof(Character));
            }
        }

        [UsedImplicitly]
        public int Citizens {
            get {
                var personcount = 0;
                foreach (var settlementHH in _households) {
                    if (settlementHH.CalcObject != null) {
                        personcount += settlementHH.CalcObject.CalculatePersonCount() * settlementHH.Count;
                    }
                }

                return personcount;
            }
        }

        [UsedImplicitly]
        public CreationType CreationType {
            get => _creationType;
            set => SetValueWithNotify(value, ref _creationType, nameof(CreationType));
        }
        /*
        [JetBrains.Annotations.NotNull]
        public string DstPath {
            get => _dstPath;
            [UsedImplicitly]
            set {
                if (_dstPath == value) {
                    return;
                }
                _dstPath = value;
                OnPropertyChanged(nameof(DstPath));
            }
        }*/

        public bool DeleteSqliteFiles {
            get => _calcSpecification.DeleteSqlite;
            [UsedImplicitly]
            set {
                if (_calcSpecification.DeleteSqlite == value) {
                    return;
                }

                _calcSpecification.DeleteSqlite = value;
                OnPropertyChanged(nameof(DeleteSqliteFiles));
            }
        }

        public bool EnableTransportation
        {
            get => _calcSpecification.EnableTransportation;
            [UsedImplicitly]
            set
            {
                if (_calcSpecification.EnableTransportation == value)
                {
                    return;
                }

                _calcSpecification.EnableTransportation = value;
                OnPropertyChanged(nameof(EnableTransportation));
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set {
                if (_description == value) {
                    return;
                }

                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ReadOnlyCollection<string> EnabledLoadtypes {
            get {
                if (_calcSpecification.LoadtypesForPostprocessing == null) {
                    _calcSpecification.LoadtypesForPostprocessing = new List<string>();
                    OnPropertyChanged(nameof(EnabledLoadtypes));
                }

                return _calcSpecification.LoadtypesForPostprocessing.AsReadOnly();
            }
        }

        [CanBeNull]
        public DateTime? EndDate {
            get => _calcSpecification.EndDate ?? new DateTime(DateTime.Now.Year, 12, 31);
            [UsedImplicitly]
            set {
                if (_calcSpecification.EndDate == value) {
                    return;
                }

                _calcSpecification.EndDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        [CanBeNull]
        public string ExternalTimeResolution {
            get => _calcSpecification.ExternalTimeResolution;
            set {
                if (_calcSpecification.ExternalTimeResolution == value) {
                    return;
                }

                _calcSpecification.ExternalTimeResolution = value;
                OnPropertyChanged(nameof(ExternalTimeResolution));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public GeographicLocation GeographicLocation {
            get => _geographicLocation;
            set => SetValueWithNotify(value, ref _geographicLocation, false, nameof(GeographicLocation));
        }

        [UsedImplicitly]
        public int HouseholdCount {
            get {
                var householdcount = 0;
                foreach (var settlementHH in _households) {
                    householdcount += settlementHH.Count;
                }

                return householdcount;
            }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<SettlementHH> Households => _households;

        [UsedImplicitly]
        public int HouseholdTypeCount => _households.Count;

        [CanBeNull]
        public string InternalTimeResolution {
            get => _calcSpecification.InternalTimeResolution;
            set {
                if (_calcSpecification.InternalTimeResolution == value) {
                    return;
                }

                _calcSpecification.InternalTimeResolution = value;
                OnPropertyChanged(nameof(InternalTimeResolution));
            }
        }

        public LoadTypePriority LoadTypePriority {
            get => _calcSpecification.LoadTypePriority;
            set {
                _calcSpecification.LoadTypePriority = value;
                OnPropertyChanged(nameof(LoadTypePriority));
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string Location {
            get => _location;

            set {
                if (_location == value) {
                    return;
                }

                _location = value;
                OnPropertyChanged(nameof(Location));
            }
        }

        [CanBeNull]
        public string OutputDirectory {
            get => _calcSpecification.OutputDirectory;
            set {
                _calcSpecification.OutputDirectory = value;
                OnPropertyChanged(nameof(OutputDirectory));
            }
        }

        public OutputFileDefault OutputFileDefault {
            [UsedImplicitly] get => _calcSpecification.DefaultForOutputFiles;
            set {
                _calcSpecification.DefaultForOutputFiles = value;
                OnPropertyChanged(nameof(OutputFileDefault));
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string PopularWith {
            get => _popularWith;

            set {
                if (_popularWith == value) {
                    return;
                }

                _popularWith = value;
                OnPropertyChanged(nameof(PopularWith));
            }
        }

        [JetBrains.Annotations.NotNull]
        public string Source {
            get => _source;
            set => SetValueWithNotify(value, ref _source, nameof(Source));
        }

        [UsedImplicitly]
        [CanBeNull]
        public DateTime? StartDate {
            get => _calcSpecification.StartDate;

            set {
                if (_calcSpecification.StartDate == value) {
                    return;
                }

                _calcSpecification.StartDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public TemperatureProfile TemperatureProfile {
            get => _temperatureProfile;
            set => SetValueWithNotify(value, ref _temperatureProfile, nameof(TemperatureProfile));
        }


        public ObservableCollection<Person> AllPersons {
            get {
                var persons = new ObservableCollection<Person>();
                foreach (var hh in _households) {
                    if (hh.CalcObject != null) {
                        foreach (var person in hh.CalcObject.AllPersons) {
                            persons.Add(person);
                        }
                    }
                }

                return persons;
            }
        }

        public CalcObjectType CalcObjectType => CalcObjectType.Settlement;

        public TimeSpan CalculateMaximumInternalTimeResolution() => new TimeSpan(0, 1, 0);

        public int CalculatePersonCount() => Citizens;

        public List<VLoadType> CollectLoadTypes(ObservableCollection<Affordance> affordances)
        {
            var loadTypes = new List<VLoadType>();
            foreach (var houseHousehold in _households) {
                var tmpList = houseHousehold.CalcObject?.CollectLoadTypes(affordances);
                if (tmpList != null) {
                    foreach (var vLoadType in tmpList) {
                        if (!loadTypes.Contains(vLoadType)) {
                            loadTypes.Add(vLoadType);
                        }
                    }
                }
            }

            return loadTypes;
        }

        public GeographicLocation DefaultGeographicLocation => GeographicLocation;

        public TemperatureProfile DefaultTemperatureProfile => TemperatureProfile;

        public EnergyIntensityType EnergyIntensityType {
            get => _calcSpecification.EnergyIntensityType;
            set {
                _calcSpecification.EnergyIntensityType = value;
                OnPropertyChanged(nameof(EnergyIntensityType));
            }
        }

        public void AddCalcOption(CalcOption option)
        {
            if (_calcSpecification.CalcOptions == null) {
                _calcSpecification.CalcOptions = new List<CalcOption>();
                OnPropertyChanged(nameof(CalcOptions));
            }

            if (_calcSpecification.CalcOptions.Contains(option)) {
                return;
            }

            _calcSpecification.CalcOptions.Add(option);
            OnPropertyChanged(nameof(CalcOptions));
        }

        [JetBrains.Annotations.NotNull]
        public SettlementHH AddHousehold([JetBrains.Annotations.NotNull] ICalcObject hh, int count)
        {
            SettlementHH item2Delete = null;
            foreach (var settlementHH in _households) {
                if (settlementHH.CalcObject == hh) {
                    item2Delete = settlementHH;
                }
            }

            if (item2Delete != null) {
                DeleteSettlementHHFromDB(item2Delete);
            }

            var shh = new SettlementHH(null, hh, count, ID, ConnectionString, hh.Name, System.Guid.NewGuid().ToStrGuid());
            _households.Add(shh);
            shh.SaveToDB();
            return shh;
        }

        public void AddLoadtypeForPostProcessing([JetBrains.Annotations.NotNull] string lt)
        {
            if (_calcSpecification.LoadtypesForPostprocessing == null) {
                _calcSpecification.LoadtypesForPostprocessing = new List<string>();
                OnPropertyChanged(nameof(EnabledLoadtypes));
            }

            if (!_calcSpecification.LoadtypesForPostprocessing.Contains(lt)) {
                _calcSpecification.LoadtypesForPostprocessing.Add(lt);
                OnPropertyChanged(nameof(EnabledLoadtypes));
            }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<AgeEntry> CalculateAgeEntries()
        {
            var ageEntries = new List<AgeEntry>();
            for (var i = 0; i < 100; i += 5) {
                ageEntries.Add(new AgeEntry(i, i + 4));
            }

            var allpersons = AllPersons;
            foreach (var person in allpersons) {
                var age = person.Age;
                var ageentry = ageEntries.First(x => age >= x.MinAge && age <= x.MaxAge);
                ageentry.Count++;
            }

            return ageEntries;
        }

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public Dictionary<HouseholdTag, int> CalculateHouseholdTagCounts()
        {
            var dict = new Dictionary<HouseholdTag, int>();
            foreach (var settlementHH in Households) {
                switch (settlementHH.CalcObjectType) {
                    case CalcObjectType.House:
                        var house = (House)settlementHH.CalcObject;
                        if (house != null) {
                            foreach (var household in house.Households) {
                                if (household.CalcObjectType == CalcObjectType.ModularHousehold) {
                                    var chh = (ModularHousehold)household.CalcObject;
                                    if (chh == null) {
                                        throw new LPGException("Household was null");
                                    }

                                    ProcessMHHForStatistics(chh, dict);
                                }
                                else {
                                    throw new LPGException("Unknown CalcObjectType");
                                }
                            }
                        }

                        break;
                    case CalcObjectType.ModularHousehold: {
                        var chh = (ModularHousehold)settlementHH.CalcObject;
                        if (chh != null) {
                            ProcessMHHForStatistics(chh, dict);
                        }
                    }
                        break;
                    default: throw new LPGException("Forgotten Calcobjecttype");
                }
            }

            return dict;
        }

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public Dictionary<LivingPatternTag, int> CalculateLivingPatternCounts()
        {
            List<ModularHousehold> households = new List<ModularHousehold>();
            foreach (var settlementHH in Households) {
                switch (settlementHH.CalcObjectType) {
                    case CalcObjectType.House:
                        var house = (House)settlementHH.CalcObject;
                        if (house != null) {
                            foreach (var household in house.Households) {
                                if (household.CalcObjectType == CalcObjectType.ModularHousehold) {
                                    households.Add((ModularHousehold)household.CalcObject);
                                }
                                else {
                                    throw new LPGException("Unknown CalcObjectType");
                                }
                            }
                        }

                        break;
                    case CalcObjectType.ModularHousehold: {
                        households.Add((ModularHousehold)settlementHH.CalcObject);
                    }
                        break;
                    default: throw new LPGException("Forgotten Calcobjecttype");
                }
            }

            var dict = new Dictionary<LivingPatternTag, int>();
            foreach (var household in households) {
                foreach (var person in household.Persons) {
                    if (person.LivingPatternTag != null) {
                        if (!dict.ContainsKey(person.LivingPatternTag)) {
                            dict.Add(person.LivingPatternTag, 0);
                        }

                        dict[person.LivingPatternTag]++;
                    }
                }
            }

            return dict;
        }

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString)
        {
            var newname = FindNewName(isNameTaken, "New Settlement ");
            var startDate = new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = new DateTime(DateTime.Now.Year, 12, 31);
            JsonCalcSpecification jcs = new JsonCalcSpecification(
                false,
                null,
                endDate,
                null,
                null,
                null,
                LoadTypePriority.All,
                "Results",
                false,
                startDate,
                null, true,false);
            return new Settlement(newname,
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                connectionString,
                null,
                null,
                "Manually Created",
                CreationType.ManuallyCreated,
                System.Guid.NewGuid().ToStrGuid(),
                jcs);
        }

        public override void DeleteFromDB()
        {
            while (_households.Count > 0) {
                var shh = _households[0];
                DeleteSettlementHHFromDB(shh);
            }

            base.DeleteFromDB();
        }

        public void DeleteSettlementHHFromDB([JetBrains.Annotations.NotNull] SettlementHH shh)
        {
            if (shh.ID != null) {
                shh.DeleteFromDB();
            }

            Logger.Get().SafeExecuteWithWait(() => _households.Remove(shh));
        }

        public void ImportFromExisting([JetBrains.Annotations.NotNull] Settlement settlement)
        {
            BuildingType = settlement.BuildingType;
            Source = string.Empty;
            Character = settlement.Character;
            Description = settlement.Description;
            Location = settlement.Location;
            PopularWith = settlement.PopularWith;
            EnergyIntensityType = settlement.EnergyIntensityType;
            CreationType = settlement.CreationType;
            _calcSpecification = settlement._calcSpecification;
            foreach (var settlementHH in settlement.Households) {
                if (settlementHH.CalcObject != null) {
                    AddHousehold(settlementHH.CalcObject, settlementHH.Count);
                }
            }

            SaveToDB();
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim) =>
            ImportFromItem((Settlement)toImport, dstSim);

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([JetBrains.Annotations.NotNull] Settlement item, [JetBrains.Annotations.NotNull] Simulator dstsim)
        {
            GeographicLocation geoloc = null;
            if (item.GeographicLocation != null) {
                geoloc = GetItemFromListByName(dstsim.GeographicLocations.Items, item.GeographicLocation.Name);
            }

            TemperatureProfile temperatureProfile = null;
            if (item.TemperatureProfile != null) {
                temperatureProfile = GetItemFromListByName(dstsim.TemperatureProfiles.Items, item.TemperatureProfile.Name);
            }

            var settlement = new Settlement(item.Name,
                null,
                item.Character,
                item.Location,
                item.PopularWith,
                item.BuildingType,
                item.Description,
                dstsim.ConnectionString,
                geoloc,
                temperatureProfile,
                item.Source,
                item.CreationType,
                item.Guid,
                item._calcSpecification);
            settlement.SaveToDB();
            foreach (var settlementHH in item.Households) {
                if (settlementHH.CalcObject != null) {
                    var hh = GetICalcObjectFromList(dstsim.ModularHouseholds.Items, dstsim.Houses.Items, null, settlementHH.CalcObject);
                    if (hh == null) {
                        Logger.Error("While importing a settlement, could not find a house. Skipping.");
                        continue;
                    }

                    settlement.AddHousehold(hh, settlementHH.Count);
                }
            }

            return settlement;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Settlement> result,
                                            [JetBrains.Annotations.NotNull] string connectionString,
                                            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TemperatureProfile> temperatureProfiles,
                                            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<GeographicLocation> geographicLocations,
                                            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<ModularHousehold> modularHouseholds,
                                            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<House> houses,
                                            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(temperatureProfiles: temperatureProfiles, geographicLocations: geographicLocations);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var settlementHhs = new ObservableCollection<SettlementHH>();
            SettlementHH.LoadFromDatabase(settlementHhs, connectionString, modularHouseholds, houses, ignoreMissingTables);
            var items2Delete = new List<SettlementHH>();
            foreach (var settlementHh in settlementHhs) {
                if (settlementHh.CalcObject == null) {
                    items2Delete.Add(settlementHh);
                }
            }

            foreach (var settlementHH in items2Delete) {
                settlementHH.DeleteFromDB();
                settlementHhs.Remove(settlementHH);
            }

            SetSubitems(new List<DBBase>(result), new List<DBBase>(settlementHhs), IsCorrectParent, ignoreMissingTables);
            // sort

            foreach (var settlement in result) {
                settlement.Households.Sort();
                foreach (var settlementHH in settlement.Households) {
                    if (settlementHH.CalcObject != null) {
                        settlementHH.CalcObject.FunctionsToCallAfterDelete.Add(settlement.OnHouseholdDelete);
                    }
                }
            }
        }
        /*
        public void MakeFullCopyWithRandomVacations(Simulator sim)
        {
            List<int> vacationDurations = new List<int>();
            vacationDurations.Add(1);
            vacationDurations.Add(7);
            vacationDurations.Add(14);
            vacationDurations.Add(21);
            int number = 0;
            Random r = new Random();
            foreach (int vacationduration in vacationDurations)
            {
                Settlement other = null;
                Logger.Get().SafeExecuteWithWait(() => other = sim.Settlements.CreateNewItem(sim.ConnectionString));
                other.SaveToDB();
                other.ImportFromExisting(this);
                other.Name = "DissR3Vacations" + vacationduration;
                other.SaveToDB();
                List<House> newHouses = new List<House>();
                int housecount = 1;
                foreach (SettlementHH settlementHH in other.Households)
                {
                    Logger.Info("Processing item " + housecount++ + " out of " + other.Households.Count);
                    House house = settlementHH.CalcObject as House;
                    if (house != null)
                    {
                        House newHouse = null;
                        Logger.Get()
                            .SafeExecuteWithWait(() => newHouse = sim.Houses.CreateNewItem(sim.ConnectionString));

                        newHouse.ImportFromExisting(house);
                        newHouse.Name = "Diss " + house.Name + " for " + other.Name;
                        List<ModularHousehold> newModularHouseholds = new List<ModularHousehold>();
                        foreach (HouseHousehold houseHouseHold in house.Households)
                        {
                            Thread.Sleep(150);
                            ModularHousehold chh = (ModularHousehold) houseHouseHold.CalcObject;
                            ModularHousehold newChh = null;
                            Logger.Get()
                                .SafeExecuteWithWait(() => newChh =
                                    sim.ModularHouseholds.CreateNewItem(sim.ConnectionString));

                            newChh.ImportModularHousehold(chh);
                            Vacation newVacation = null;
                            Logger.Get()
                                .SafeExecuteWithWait(() => newVacation =
                                    sim.Vacations.CreateNewItem(sim.ConnectionString));
                            newVacation.SaveToDB();
                            newVacation.MinimumAge = 1;
                            newVacation.MaximumAge = 99;
                            DateTime startVac = new DateTime(2015, 6, 15);
                            int offset = r.Next(60 - vacationduration);
                            startVac = startVac.AddDays(offset);
                            DateTime endVac = startVac.AddDays(vacationduration);
                            newVacation.AddVacationTime(startVac, endVac,);
                            string prefix = string.Empty;

                            if (!chh.Name.StartsWith("x ", StringComparison.Ordinal))
                                prefix = "x ";
                            newChh.Name = prefix + chh.Name + " for " + other.Name + "(Diss) " + number++;
                            newVacation.Name = "Diss Randomly Created for " + newChh.Name;
                            newVacation.SaveToDB();
                            newChh.Vacation = newVacation;
                            newModularHouseholds.Add(newChh);
                            newChh.SaveToDB();
                        }
                        while (newHouse.Households.Count > 0)
                            newHouse.DeleteHouseholdFromDB(newHouse.Households[0]);
                        foreach (ModularHousehold chh in newModularHouseholds)
                            newHouse.AddHousehold(chh);
                        newHouse.SaveToDB();
                        newHouses.Add(newHouse);
                    }
                }
                while (other._households.Count > 0)
                    other.DeleteSettlementHHFromDB(other._households[0]);
                foreach (House newHouse in newHouses)
                    other.AddHousehold(newHouse, 1);
                other.SaveToDB();
            }
        }*/

        public void MakeTraitStatistics([JetBrains.Annotations.NotNull] string traitfilename, [JetBrains.Annotations.NotNull] string affFileName, [JetBrains.Annotations.NotNull] Simulator sim)
        {
            var chhs = new List<ModularHousehold>();
            foreach (var settlementHH in _households) {
                if (settlementHH.CalcObjectType == CalcObjectType.ModularHousehold) {
                    var chh = (ModularHousehold)settlementHH.CalcObject;
                    chhs.Add(chh);
                }

                if (settlementHH.CalcObjectType == CalcObjectType.House) {
                    var house = (House)settlementHH.CalcObject;
                    if (house != null) {
                        foreach (var household in house.Households) {
                            if (household.CalcObjectType == CalcObjectType.ModularHousehold) {
                                var chh = (ModularHousehold)household.CalcObject;
                                chhs.Add(chh);
                            }
                        }
                    }
                }
            }

            var traitCounts = new Dictionary<HouseholdTrait, int>();
            var affordanceCounts = new Dictionary<Affordance, int>();
            var persons = 0;

            foreach (var chh in chhs) {
                foreach (var modularHouseholdTrait in chh.Traits) {
                    if (!traitCounts.ContainsKey(modularHouseholdTrait.HouseholdTrait)) {
                        traitCounts.Add(modularHouseholdTrait.HouseholdTrait, 0);
                    }

                    traitCounts[modularHouseholdTrait.HouseholdTrait]++;
                    foreach (var affordance in modularHouseholdTrait.HouseholdTrait.CollectAffordances(true)) {
                        if (!affordanceCounts.ContainsKey(affordance)) {
                            affordanceCounts.Add(affordance, 0);
                        }

                        affordanceCounts[affordance]++;
                    }
                }

                persons += chh.CalculatePersonCount();
            }

            var csv = sim.MyGeneralConfig.CSVCharacter;
            using (var sw = new StreamWriter(traitfilename)) {
                sw.WriteLine("Name" + csv + "Count" + csv + "Percentage of Households" + csv + "Percentage of Persons");
                foreach (var pair in traitCounts) {
                    var hhpercentage = pair.Value / (double)chhs.Count;
                    var ppPercentage = pair.Value / (double)persons;
                    var s = pair.Key.PrettyName + csv + pair.Value + csv + hhpercentage + csv + ppPercentage;
                    sw.WriteLine(s);
                }

                sw.Close();
            }

            using (var sw = new StreamWriter(affFileName)) {
                sw.WriteLine("Name" + csv + "Count" + csv + "Percentage of Households" + csv + "Percentage of Persons");
                foreach (var pair in affordanceCounts) {
                    var hhpercentage = pair.Value / (double)chhs.Count;
                    var ppPercentage = pair.Value / (double)persons;
                    var s = pair.Key.PrettyName + csv + pair.Value + csv + hhpercentage + csv + ppPercentage;
                    sw.WriteLine(s);
                }

                sw.Close();
            }
        }

        public void RemoveLoadtypeForPostProcessing([JetBrains.Annotations.NotNull] string lt)
        {
            if (_calcSpecification.LoadtypesForPostprocessing == null) {
                _calcSpecification.LoadtypesForPostprocessing = new List<string>();
                OnPropertyChanged(nameof(EnabledLoadtypes));
                return;
            }

            if (_calcSpecification.LoadtypesForPostprocessing.Contains(lt)) {
                _calcSpecification.LoadtypesForPostprocessing.Remove(lt);
                OnPropertyChanged(nameof(EnabledLoadtypes));
            }
        }

        public void RemoveOption(CalcOption calcOption)
        {
            if (_calcSpecification.CalcOptions == null) {
                _calcSpecification.CalcOptions = new List<CalcOption>();
            }

            if (_calcSpecification.CalcOptions.Contains(calcOption)) {
                _calcSpecification.CalcOptions.Remove(calcOption);
            }

            OnPropertyChanged(nameof(CalcOptions));
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var settlementHH in _households) {
                settlementHH.SaveToDB();
            }
        }

        public override string ToString() => Name;

        public void WriteJsonCalculationSpecs([JetBrains.Annotations.NotNull] string dstDirectory, [JetBrains.Annotations.NotNull] string simulationEnginePath)
        {
            List<string> generatedPaths = new List<string>();
            List<string> outputFiles = new List<string>();
            if (!Directory.Exists(dstDirectory)) {
                Directory.CreateDirectory(dstDirectory);
                Thread.Sleep(500);
            }

            string db3Srcdir = ConnectionString.Replace("Data Source=", "");
            string db3DstDir = Path.Combine(dstDirectory, "profilegenerator.db3");
            File.Copy(db3Srcdir, db3DstDir, true);
            int houseidx = 1;
            foreach (SettlementHH settlementHH in Households) {

                if (settlementHH.CalcObjectType != CalcObjectType.House) {
                    throw new LPGException("This feature can only be used on settlements that only contain houses. The element " + settlementHH.CalcObject?.Name + " is not a house, it is an " + settlementHH.CalcObjectType);
                }

                var house = (House)settlementHH.CalcObject;
                if (house == null) {
                    throw new LPGException("House was null");
                }

                if (StartDate == null) {
                    throw new LPGException("No startdate was set.");
                }
                HouseCreationAndCalculationJob housejob = new HouseCreationAndCalculationJob(PrettyName,
                    StartDate.Value.Year.ToString(CultureInfo.InvariantCulture),null, HouseDefinitionType.HouseData);
                var calcSettings = new JsonCalcSpecification(_calcSpecification) {
                    GeographicLocation = GeographicLocation?.GetJsonReference(),
                    TemperatureProfile = TemperatureProfile?.GetJsonReference(),
                    DefaultForOutputFiles = OutputFileDefault.NoFiles
                };


                string name = house.PrettyName + " " + houseidx;

                calcSettings.OutputDirectory = AutomationUtili.CleanFileName(name);
                housejob.CalcSpec = calcSettings;
                if (generatedPaths.Contains(calcSettings.OutputDirectory)) {
                    throw new LPGException("The directory " + calcSettings.OutputDirectory +
                                           " is in two houses. This is not very useful. Please fix. Aborting.");
                }

                generatedPaths.Add(calcSettings.OutputDirectory);

                housejob.House = house.MakeHouseData();
                housejob.PathToDatabase = "profilegenerator.db3";
                calcSettings.CalculationName = name;
                string calcJsonFilename = Path.Combine(dstDirectory, AutomationUtili.CleanFileName(name) + ".json");
                if (outputFiles.Contains(calcJsonFilename)) {
                    throw new LPGException("There are two houses that are generating a file for " + calcJsonFilename +
                                           ". this is not possible. Please fix. Aborting.");
                }

                outputFiles.Add(calcJsonFilename);
                 HouseJobSerializer.WriteJsonToFile(calcJsonFilename, housejob);
                 houseidx++;
            }

            string batchPath1 = Path.Combine(dstDirectory, "SimulateEverythingSequentially.cmd");
            StreamWriter sw = new StreamWriter(batchPath1);
            foreach (var outputFile in outputFiles) {
                FileInfo fi = new FileInfo(outputFile);
                sw.WriteLine("\"" + simulationEnginePath + "\" ProcessHouseJob -j \"" + fi.Name + "\"");
            }

            sw.WriteLine("pause");
            sw.Close();
            string batchPath2 = Path.Combine(dstDirectory, "SimulateEverythingInParallel.cmd");
            StreamWriter sw2 = new StreamWriter(batchPath2);
            sw2.WriteLine("\"" + simulationEnginePath + "\" LaunchJsonParallel -NumberOfCores 4 -JsonDirectory .");
            sw2.WriteLine("pause");
            sw2.Close();
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Character", "@Character", _character);
            cmd.AddParameter("Location", "@Location", _location);
            cmd.AddParameter("PopularWith", "@PopularWith", _popularWith);
            cmd.AddParameter("BuildingType", "@BuildingType", _buildingType);
            cmd.AddParameter("Description", "@Description", _description);
            cmd.AddParameter("Source", _source);
            if (_geographicLocation != null) {
                cmd.AddParameter("GeographicLocationID", _geographicLocation.IntID);
            }

            if (_temperatureProfile != null) {
                cmd.AddParameter("TemperatureProfileID", _temperatureProfile.IntID);
            }

            string str = JsonConvert.SerializeObject(_calcSpecification);
            cmd.AddParameter("JsonCalcSpecification", str);

            cmd.AddParameter("CreationType", (int)_creationType);
        }

        [JetBrains.Annotations.NotNull]
        private static Settlement AssignFields([JetBrains.Annotations.NotNull] DataReader dr,
                                               [JetBrains.Annotations.NotNull] string connectionString,
                                               bool ignoreMissingFields,
                                               [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var hhid = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var character = dr.GetString("Character");
            var location = dr.GetString("Location");
            var popularWith = dr.GetString("PopularWith");
            var buildingType = dr.GetString("BuildingType");
            var description = dr.GetString("Description", false);
            var temperatureProfileID = dr.GetNullableIntFromLong("TemperatureProfileID", false, ignoreMissingFields);
            TemperatureProfile tp = null;
            if (temperatureProfileID != null) {
                tp = aic.TemperatureProfiles.FirstOrDefault(tp1 => tp1.ID == temperatureProfileID);
            }

            var geographicLocationID = dr.GetNullableIntFromLong("GeographicLocationID", false, ignoreMissingFields);
            GeographicLocation geoloc = null;
            if (geographicLocationID != null) {
                geoloc = aic.GeographicLocations.FirstOrDefault(geo1 => geo1.ID == geographicLocationID);
            }

            //var loadtypePrio = (LoadTypePriority)dr.GetIntFromLong("LoadTypePriority", false, ignoreMissingFields, (int)Automation.LoadTypePriority.RecommendedForHouses);
            var source = dr.GetString("Source", false, "Manually Created", ignoreMissingFields);
            var creationType = (CreationType)dr.GetIntFromLong("CreationType", false, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var jsonCalcspecStr = dr.GetString("JsonCalcSpecification", false, "", ignoreMissingFields);
            JsonCalcSpecification jcs = null;

            if (!string.IsNullOrWhiteSpace(jsonCalcspecStr)) {
                try {
                    jcs = JsonConvert.DeserializeObject<JsonCalcSpecification>(jsonCalcspecStr);
                }
                catch (Exception ex) {
                    Logger.Info("Resetting calculation settings for " +name+ " due to changes from the previous version. Message: " + ex.Message);
                    jcs = JsonCalcSpecification.MakeDefaultsForProduction();
                }
            }

            return new Settlement(name,
                hhid,
                character,
                location,
                popularWith,
                buildingType,
                description,
                connectionString,
                geoloc,
                tp,
                source,
                creationType,
                guid,
                jcs);
        }

        private void HouseholdsCollectionChanged([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChangedNoUpdate(nameof(Citizens));
            OnPropertyChangedNoUpdate(nameof(HouseholdCount));
        }

        private static bool IsCorrectParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var settlementHh = (SettlementHH)child;
            if (parent.ID == settlementHh.SettlementID) {
                var settlement = (Settlement)parent;
                settlement._households.Add(settlementHh);
                return true;
            }

            return false;
        }

        private void OnHouseholdDelete([CanBeNull] DBBase item)
        {
            var items2Delete = new List<SettlementHH>();
            foreach (var settlementHH in Households) {
                if (settlementHH.CalcObject == item) {
                    items2Delete.Add(settlementHH);
                }
            }

            foreach (var settlementHH in items2Delete) {
                DeleteSettlementHHFromDB(settlementHH);
            }
        }

        private static void ProcessMHHForStatistics([JetBrains.Annotations.NotNull] ModularHousehold chh, [JetBrains.Annotations.NotNull] Dictionary<HouseholdTag, int> counts)
        {
            foreach (var tag in chh.ModularHouseholdTags) {
                if (!counts.ContainsKey(tag.Tag)) {
                    counts.Add(tag.Tag, 0);
                }

                counts[tag.Tag]++;
            }
        }

        public class AgeEntry : INotifyPropertyChanged, IComparable, IEquatable<AgeEntry> {
            private int _count;

            public bool Equals(AgeEntry other)
            {
                if (ReferenceEquals(null, other)) {
                    return false;
                }

                if (ReferenceEquals(this, other)) {
                    return true;
                }

                return _count == other._count && MaxAge == other.MaxAge && MinAge == other.MinAge;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != GetType()) {
                    return false;
                }

                return Equals((AgeEntry)obj);
            }

            public AgeEntry(int minAge, int maxAge)
            {
                MinAge = minAge;
                MaxAge = maxAge;
            }

            [JetBrains.Annotations.NotNull]
            [UsedImplicitly]
            public string AgeRange => MinAge + " - " + MaxAge;

            public int Count {
                get => _count;
                set {
                    if (value == _count) {
                        return;
                    }

                    _count = value;
                    OnPropertyChanged();
                }
            }

            public int MaxAge { get; }

            public int MinAge { get; }

            public int CompareTo([CanBeNull] object obj)
            {
                if (obj == null) {
                    return 0;
                }

                if (!(obj is AgeEntry ae)) {
                    return 0;
                }

                return string.CompareOrdinal(AgeRange, ae.AgeRange);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public override int GetHashCode()
            {
                unchecked {
                    // ReSharper disable once NonReadonlyMemberInGetHashCode
                    var hashCode = _count;
                    hashCode = (hashCode * 397) ^ MaxAge;
                    hashCode = (hashCode * 397) ^ MinAge;
                    return hashCode;
                }
            }

            public static bool operator ==([CanBeNull] AgeEntry left, [CanBeNull] AgeEntry right)
            {
                if (ReferenceEquals(left, null))
                {
                    return ReferenceEquals(right, null);
                }

                return left.Equals(right);
            }

            public static bool operator !=([CanBeNull] AgeEntry left, [CanBeNull] AgeEntry right)
            {
                return !(left == right);
            }

            public static bool operator <([CanBeNull] AgeEntry left, [CanBeNull] AgeEntry right)
            {
                return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
            }

            public static bool operator <=([CanBeNull] AgeEntry left, AgeEntry right)
            {
                return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
            }

            public static bool operator >([CanBeNull] AgeEntry left, AgeEntry right)
            {
                return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
            }

            public static bool operator >=([CanBeNull] AgeEntry left, [CanBeNull] AgeEntry right)
            {
                return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
            }
        }
    }
}