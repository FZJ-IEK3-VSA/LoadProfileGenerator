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
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.Houses {
    public class House : DBBaseElement, ICalcObject {
        internal const string TableName = "tblHouses";

        [NotNull] [ItemNotNull] private readonly ObservableCollection<HouseHousehold> _houseHouseholds = new ObservableCollection<HouseHousehold>();

        private CreationType _creationType;

        [CanBeNull] private string _description;
        private EnergyIntensityType _energyIntensityType;

        [CanBeNull] private GeographicLocation _geographicLocation;

        [CanBeNull] private HouseType _houseType;

        [CanBeNull] private string _source;

        [CanBeNull] private TemperatureProfile _temperatureProfile;


        public House([NotNull] string pName, [CanBeNull] string description, [CanBeNull] TemperatureProfile temperatureProfile, [CanBeNull] GeographicLocation geographicLocation,
                     [CanBeNull] HouseType houseType, [NotNull] string connectionString, EnergyIntensityType energyIntensity, [CanBeNull] string source, CreationType creationType,
                     StrGuid guid,[CanBeNull] int? pID = null) : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            TypeDescription = "House";
            _description = description;
            _energyIntensityType = energyIntensity;
            _temperatureProfile = temperatureProfile;
            _geographicLocation = geographicLocation;
            _houseType = houseType;
            _source = source;
            _creationType = creationType;
        }

        [UsedImplicitly]
        public CreationType CreationType {
            get => _creationType;
            set => SetValueWithNotify(value, ref _creationType, nameof(CreationType));
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [NotNull]
        [UsedImplicitly]
        public string EnergyIntensityTypeString {
            get => _energyIntensityType.ToString();
            set {
                _energyIntensityType = EnergyIntensityConverter.GetEnergyIntensityTypeFromString(value);
                OnPropertyChanged(nameof(EnergyIntensityType));
                OnPropertyChanged(nameof(EnergyIntensityTypeString));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public GeographicLocation GeographicLocation {
            get => _geographicLocation;
            set => SetValueWithNotify(value, ref _geographicLocation, false, nameof(GeographicLocation));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HouseHousehold> Households => _houseHouseholds;

        [UsedImplicitly]
        [CanBeNull]
        public HouseType HouseType {
            get => _houseType;
            set => SetValueWithNotify(value, ref _houseType, false, nameof(HouseType));
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Source {
            get => _source;
            set => SetValueWithNotify(value, ref _source, nameof(Source));
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
                foreach (var houseHousehold in _houseHouseholds) {
                    if (houseHousehold.CalcObject == null) {
                        throw new LPGException("Household was null");
                    }

                    foreach (var person in houseHousehold.CalcObject.AllPersons) {
                        persons.Add(person);
                    }
                }

                return persons;
            }
        }

        public CalcObjectType CalcObjectType => CalcObjectType.House;

        public TimeSpan CalculateMaximumInternalTimeResolution() => new TimeSpan(0, 1, 0);

        public int CalculatePersonCount()
        {
            var i = 0;
            foreach (var houseHousehold in _houseHouseholds) {
                if (houseHousehold.CalcObject == null) {
                    throw new LPGException("Household was null");
                }

                i += houseHousehold.CalcObject.CalculatePersonCount();
            }

            return i;
        }

        public List<VLoadType> CollectLoadTypes(ObservableCollection<Affordance> affordances)
        {
            var loadTypes = new List<VLoadType>();
            foreach (var houseHousehold in _houseHouseholds) {
                if (houseHousehold.CalcObject == null) {
                    throw new LPGException("Household was null");
                }

                var tmpList = houseHousehold.CalcObject.CollectLoadTypes(affordances);
                foreach (var vLoadType in tmpList) {
                    if (!loadTypes.Contains(vLoadType)) {
                        loadTypes.Add(vLoadType);
                    }
                }
            }

            return loadTypes;
        }

        public GeographicLocation DefaultGeographicLocation => GeographicLocation;

        public TemperatureProfile DefaultTemperatureProfile => TemperatureProfile;

        [UsedImplicitly]
        public EnergyIntensityType EnergyIntensityType {
            get => _energyIntensityType;
            set {
                _energyIntensityType = value;
                OnPropertyChanged(nameof(EnergyIntensityType));
                OnPropertyChanged(nameof(EnergyIntensityTypeString));
            }
        }

        public void AddHousehold([NotNull] ICalcObject hh,
                                 [CanBeNull] ChargingStationSet chargingstations,
                                 [CanBeNull] TravelRouteSet travelrouteset, [CanBeNull] TransportationDeviceSet transportationDeviceSet)
        {
            if (hh.ConnectionString != ConnectionString) {
                throw new LPGException("Adding house from another database. This is a bug! Please report.");
            }

            var hd = new HouseHousehold(
                null, IntID, hh, ConnectionString, hh.Name,
                System.Guid.NewGuid().ToStrGuid(), transportationDeviceSet, chargingstations, travelrouteset);
            _houseHouseholds.Add(hd);
            _houseHouseholds.Sort();
            hd.SaveToDB();
        }

        [NotNull]
        public HouseData MakeHouseData()
        {
            HouseData hd = new HouseData(System.Guid.NewGuid().ToStrGuid(), HouseType?.HouseTypeCode,
                HouseType?.HeatingYearlyTotal, HouseType?.CoolingYearlyTotal, Name);
            int householdIdx = 1;
            foreach (var houseHousehold in Households)
            {
                string householdName = "House " + Name + " - Household " + householdIdx + " - " + houseHousehold.CalcObject?.Name;
                ModularHousehold household = (ModularHousehold)houseHousehold.CalcObject;
                if (household == null)
                {
                    throw new LPGException("Household was null");
                }
                var hhd = new HouseholdData(householdName, houseHousehold.Name, houseHousehold.ChargingStationSet?.GetJsonReference(),
                    houseHousehold.TransportationDeviceSet?.GetJsonReference(),
                    houseHousehold.TravelRouteSet?.GetJsonReference(), null, HouseholdDataSpecificationType.ByHouseholdName);
                hhd.HouseholdNameSpec = new HouseholdNameSpecification(household.GetJsonReference());
                hd.Households.Add(hhd);
                householdIdx++;
            }

            return hd;
        }

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
        {
            var house = new House(FindNewName(isNameTaken, "New House "), "New house description", null, null, null, connectionString, EnergyIntensityType.Random, "Manually Created",
                CreationType.ManuallyCreated, System.Guid.NewGuid().ToStrGuid());
            return house;
        }

        public override void DeleteFromDB()
        {
            foreach (var houseHousehold in _houseHouseholds) {
                houseHousehold.DeleteFromDB();
            }

            base.DeleteFromDB();
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim) => ImportFromItem((House)toImport, dstSim);
        /*
        public void ImportFromExisting(House other)
        {
            _description = other.Description;
            _energyIntensityType = other._energyIntensityType;
            _geographicLocation = other.GeographicLocation;
            _houseType = other.HouseType;
            _temperatureProfile = other.TemperatureProfile;
            _source = other.Source;
            SaveToDB();
            foreach (var houseHousehold in _houseHouseholds) {
                AddHousehold(houseHousehold.CalcObject);
            }
        }*/

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] House item, [NotNull] Simulator dstSim)
        {
            TemperatureProfile tp = null;
            if (item.TemperatureProfile != null) {
                tp = GetItemFromListByName(dstSim.TemperatureProfiles.MyItems, item.TemperatureProfile.Name);
            }

            if (tp != null && tp.ConnectionString != dstSim.ConnectionString) {
                throw new LPGException("imported for the wrong db!");
            }

            GeographicLocation geographic = null;
            if (item.GeographicLocation != null) {
                geographic = GetItemFromListByName(dstSim.GeographicLocations.MyItems, item.GeographicLocation.Name);
            }

            HouseType ht = null;
            if (item.HouseType != null) {
                ht = GetItemFromListByName(dstSim.HouseTypes.MyItems, item.HouseType.Name);
            }

            var house = new House(item.Name, item.Description, tp, geographic, ht, dstSim.ConnectionString, EnergyIntensityType.Random, item._source, item._creationType,
                System.Guid.NewGuid().ToStrGuid());
            house.SaveToDB();
            foreach (var hhh in item.Households) {
                if (hhh.CalcObject == null) {
                    continue;
                }

                var hh = GetICalcObjectFromList(dstSim.ModularHouseholds.MyItems, null, null, hhh.CalcObject);
                if (hh == null) {
                    Logger.Error("While importing a settlement, a house was null. Skipping.");
                    continue;
                }

                ChargingStationSet chargingStations = null;
                if (hhh.ChargingStationSet != null) {
                    chargingStations = GetItemFromListByName(dstSim.ChargingStationSets.It, hhh.ChargingStationSet.Name);
                }

                TravelRouteSet travelRouteSets = null;
                if (hhh.TravelRouteSet != null) {
                    travelRouteSets = GetItemFromListByName(dstSim.TravelRouteSets.It, hhh.TravelRouteSet.Name);
                }

                TransportationDeviceSet transportationDevices = null;
                if (hhh.TravelRouteSet != null) {
                    transportationDevices = GetItemFromListByName(dstSim.TransportationDeviceSets.It, hhh.TravelRouteSet.Name);
                }

                house.AddHousehold(hh,  chargingStations, travelRouteSets, transportationDevices);
            }

            return house;
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var hhh in Households) {
                hhh.SaveToDB();
            }
        }

        public override string ToString() => Name;

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            if (_description != null) {
                cmd.AddParameter("Description", "@description", _description);
            }

            if (TemperatureProfile != null) {
                cmd.AddParameter("TemperatureProfileID", TemperatureProfile.IntID);
            }

            if (GeographicLocation != null) {
                cmd.AddParameter("GeographicLocationID", GeographicLocation.IntID);
            }

            if (_houseType != null) {
                cmd.AddParameter("HouseTypeID", _houseType.IntID);
            }

            cmd.AddParameter("EnergyIntensityType", "@EnergyIntensityType", EnergyIntensityTypeString);
            if (_source != null) {
                cmd.AddParameter("Source", _source);
            }

            cmd.AddParameter("CreationType", (int)_creationType);
        }

        public void DeleteHouseholdFromDB([NotNull] HouseHousehold hhh)
        {
            _houseHouseholds.Remove(hhh);
            hhh.DeleteFromDB();
        }

        internal static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<House> result, [NotNull] string connectionString,
                                              [ItemNotNull] [NotNull] ObservableCollection<TemperatureProfile> temperaturProfiles,
                                              [ItemNotNull] [NotNull] ObservableCollection<GeographicLocation> geographicLocations, [ItemNotNull] [NotNull] ObservableCollection<HouseType> houseTypes,
                                              [ItemNotNull] [NotNull] ObservableCollection<ModularHousehold> modularHouseholds, [NotNull] [ItemNotNull] ObservableCollection<ChargingStationSet> chargingStationSets,
                                              [NotNull] [ItemNotNull] ObservableCollection<TransportationDeviceSet> transportationDeviceSets, [NotNull] [ItemNotNull] ObservableCollection<TravelRouteSet> travelRouteSets, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(temperatureProfiles: temperaturProfiles, geographicLocations: geographicLocations, houseTypes: houseTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var hhs = new ObservableCollection<HouseHousehold>();
            HouseHousehold.LoadFromDatabase(hhs, connectionString, modularHouseholds, chargingStationSets, transportationDeviceSets, travelRouteSets, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hhs), IsCorrectParentHouseHousehold, ignoreMissingTables);
        }

        [NotNull]
        private static House AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", "");
            var id = dr.GetIntFromLong("ID");
            var description = dr.GetString("Description", "");
            var temperatureprofileID = dr.GetNullableIntFromLong("TemperatureProfileID", false);
            TemperatureProfile tp = null;
            if (temperatureprofileID != null) {
                tp = aic.TemperatureProfiles.FirstOrDefault(tp1 => tp1.ID == temperatureprofileID);
            }

            var geographicLocationID = dr.GetNullableIntFromLong("GeographicLocationID", false);
            GeographicLocation geoloc = null;
            if (geographicLocationID != null) {
                geoloc = aic.GeographicLocations.FirstOrDefault(geo1 => geo1.ID == geographicLocationID);
            }

            var houseTypeID = dr.GetNullableIntFromLong("HouseTypeID", false);
            HouseType houseType = null;
            if (houseTypeID != null) {
                houseType = aic.HouseTypes.FirstOrDefault(housetype => housetype.ID == houseTypeID);
            }

            var energyintensityID = dr.GetString("EnergyIntensityType", false, EnergyIntensityType.Random.ToString(), ignoreMissingFields);
            var energyintensity = EnergyIntensityConverter.GetEnergyIntensityTypeFromString(energyintensityID);
            var source = dr.GetString("Source", false, "Manually Created", ignoreMissingFields);
            var creationType = (CreationType)dr.GetIntFromLong("CreationType", false, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new House(name, description, tp, geoloc, houseType,
                connectionString, energyintensity, source, creationType, guid, id);
        }

        private static bool IsCorrectParentHouseHousehold([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (HouseHousehold)child;
            if (parent.ID == hd.HouseID) {
                var house = (House)parent;
                house.Households.Add(hd);
                return true;
            }

            return false;
        }
    }
}