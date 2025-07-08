using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Database.Tables;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace Database
{
    public class AllItemCollections
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability",
            "CA1506:AvoidExcessiveClassCoupling")]
        public AllItemCollections([CanBeNull][ItemNotNull] ObservableCollection<AffordanceTag> affordanceTags = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<Affordance> affordances = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<ModularHousehold> modularHouseholds = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<DateBasedProfile> dateBasedProfiles = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<Desire> desires = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<DeviceCategory> deviceCategories = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<DeviceSelection> deviceSelections = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<DeviceTag> deviceTags = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TimeLimit> timeLimits = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<EnergyStorage> energyStorages = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<Generator> generators = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<GeographicLocation> geographicLocations = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<Holiday> holidays = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<HouseType> houseTypes = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<House> houses = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<HouseholdTrait> householdTraits = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<VLoadType> loadTypes = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<Location> locations = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<Person> persons = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<RealDevice> realDevices = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<SubAffordance> subAffordances = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TemperatureProfile> temperatureProfiles = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TimeBasedProfile> timeProfiles = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TransformationDevice> transformationDevices = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<AffordanceTaggingSet> affordanceTaggingSets = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<DeviceActionGroup> deviceActionGroups = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<DeviceAction> deviceActions = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TraitTag> traitTags = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<Vacation> vacations = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<HouseholdTemplate> householdTemplates = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<Variable> variables = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<HouseholdTag> householdTags = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TransportationDeviceCategory> transportationDeviceCategories = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TravelRoute> travelRoutes = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TransportationDevice> transportationDevices = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<Site> sites = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TravelRouteSet> travelRouteSets = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<TransportationDeviceSet> transportationDeviceSets = null,
                                  [ItemNotNull][CanBeNull] ObservableCollection<ChargingStationSet> chargingStationSets = null,
                                      [ItemNotNull][CanBeNull] ObservableCollection<LivingPatternTag> livingPatternTags = null)
        {
            AffordanceTags = CreateCollectionWithMap(affordanceTags);
            Affordances = CreateCollectionWithMap(affordances);
            ModularHouseholds = CreateCollectionWithMap(modularHouseholds);
            DateBasedProfiles = CreateCollectionWithMap(dateBasedProfiles);
            Desires = CreateCollectionWithMap(desires);
            DeviceCategories = CreateCollectionWithMap(deviceCategories);
            DeviceSelections = CreateCollectionWithMap(deviceSelections);
            DeviceTags = CreateCollectionWithMap(deviceTags);
            TimeLimits = CreateCollectionWithMap(timeLimits);
            EnergyStorages = CreateCollectionWithMap(energyStorages);
            Generators = CreateCollectionWithMap(generators);
            GeographicLocations = CreateCollectionWithMap(geographicLocations);
            Holidays = CreateCollectionWithMap(holidays);
            HouseTypes = CreateCollectionWithMap(houseTypes);
            Houses = CreateCollectionWithMap(houses);
            HouseholdTraits = CreateCollectionWithMap(householdTraits);
            LoadTypes = CreateCollectionWithMap(loadTypes);
            Locations = CreateCollectionWithMap(locations);
            Persons = CreateCollectionWithMap(persons);
            RealDevices = CreateCollectionWithMap(realDevices);
            SubAffordances = CreateCollectionWithMap(subAffordances);
            TemperatureProfiles = CreateCollectionWithMap(temperatureProfiles);
            TimeProfiles = CreateCollectionWithMap(timeProfiles);
            TransformationDevices = CreateCollectionWithMap(transformationDevices);
            AffordanceTaggingSets = CreateCollectionWithMap(affordanceTaggingSets);
            DeviceActionGroups = CreateCollectionWithMap(deviceActionGroups);
            DeviceActions = CreateCollectionWithMap(deviceActions);
            TraitTags = CreateCollectionWithMap(traitTags);
            Vacations = CreateCollectionWithMap(vacations);
            HouseholdTemplates = CreateCollectionWithMap(householdTemplates);
            Variables = CreateCollectionWithMap(variables);
            HouseholdTags = CreateCollectionWithMap(householdTags);
            Sites = CreateCollectionWithMap(sites);
            TransportationDeviceCategories = CreateCollectionWithMap(transportationDeviceCategories);
            TravelRoutes = CreateCollectionWithMap(travelRoutes);
            TransportationDevices = CreateCollectionWithMap(transportationDevices);
            TravelRouteSets = CreateCollectionWithMap(travelRouteSets);
            TransportationDeviceSets = CreateCollectionWithMap(transportationDeviceSets);
            ChargingStationSets = CreateCollectionWithMap(chargingStationSets);
            LivingPatternTags = CreateCollectionWithMap(livingPatternTags);
        }

        /// <summary>
        /// Turns an ObservableColleciton into an ObservableCollectionWithMap, or if the input was null also returns null.
        /// </summary>
        /// <typeparam name="T">type parameter for the collection</typeparam>
        /// <param name="items">the original collection to transform</param>
        /// <returns>the collection as an ObservableCollectionWithMap</returns>
        private static ObservableCollectionWithMap<T>? CreateCollectionWithMap<T>(ObservableCollection<T>? items) where T : DBBase => items is null ? null : new(items);

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TransportationDeviceSet> TransportationDeviceSets { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TravelRouteSet> TravelRouteSets { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<HouseholdTag> HouseholdTags { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<Variable> Variables { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<AffordanceTag> AffordanceTags { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<ChargingStationSet> ChargingStationSets { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<Affordance> Affordances { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<ModularHousehold> ModularHouseholds { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<DateBasedProfile> DateBasedProfiles { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<Desire> Desires { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<DeviceCategory> DeviceCategories { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<DeviceSelection> DeviceSelections { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<DeviceTag> DeviceTags { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TimeLimit> TimeLimits { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<EnergyStorage> EnergyStorages { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<Generator> Generators { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<GeographicLocation> GeographicLocations { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<Holiday> Holidays { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<HouseType> HouseTypes { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<House> Houses { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<HouseholdTrait> HouseholdTraits { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<VLoadType> LoadTypes { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<Location> Locations { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<Person> Persons { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<RealDevice> RealDevices { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<SubAffordance> SubAffordances { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TemperatureProfile> TemperatureProfiles { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TimeBasedProfile> TimeProfiles { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TransformationDevice> TransformationDevices { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<AffordanceTaggingSet> AffordanceTaggingSets { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<DeviceActionGroup> DeviceActionGroups { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<DeviceAction> DeviceActions { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TraitTag> TraitTags { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<Vacation> Vacations { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<HouseholdTemplate> HouseholdTemplates { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<Site> Sites { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TransportationDeviceCategory> TransportationDeviceCategories { get;  }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TravelRoute> TravelRoutes { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollectionWithMap<TransportationDevice> TransportationDevices { get; }

        public ObservableCollectionWithMap<LivingPatternTag > LivingPatternTags { get; }
    }

    /// <summary>
    /// Behaves just like ObservableMapping, but offers an additional method that allows looking up items by their Id.
    /// For this purpose, on the first call of this method a Dictionary containing all items and their IDs is built.
    /// This has the advantage that it only needs to iterate all objects once, and subsequent calls can reuse the already
    /// generated map, improving database loading performance.
    /// The reason to implement this as a subclass of ObservableCollection was that it could be used as a drop-in replacement in
    /// the AllItemCollections class without having to change any other classes.
    /// </summary>
    /// <typeparam name="T">type parameter for the collection</typeparam>
    public class ObservableCollectionWithMap<T> : ObservableCollection<T> where T : DBBase
    {
        /// <summary>
        /// Maps all items by their IDs. Is only instantiated when FindById is called.
        /// </summary>
        private readonly Lazy<Dictionary<int, T>> itemsById;

        /// <summary>
        /// Creates a new ObservableCollectionWithMap from the specified collection, using the copy constructor.
        /// </summary>
        /// <param name="items">a normal collection, which will be used to initialize this collection</param>
        public ObservableCollectionWithMap(ObservableCollection<T> items) : base(items)
        {
            itemsById = new(BuildIdMap);
        }

        /// <summary>
        /// Builds the ID map that maps each item ID to the corresponding object.
        /// </summary>
        /// <returns>a dictionary mapping item IDs to objects</returns>
        private Dictionary<int, T> BuildIdMap() => this.ToDictionary(x => x.IntID, x => x);

        /// <summary>
        /// Returns the item with the specified ID, or null.
        /// The first call is O(N), all subsequent calls are O(1).
        /// </summary>
        /// <param name="id">the ID of the requested item</param>
        /// <returns>the requested item</returns>
        public T? FindById(int id) => itemsById.Value.GetValueOrDefault(id);
    }
}