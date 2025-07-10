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

namespace Database
{
    public class AllItemCollections(ObservableCollection<AffordanceTag>? affordanceTags = null,
                              ObservableCollection<Affordance>? affordances = null,
                              ObservableCollection<ModularHousehold>? modularHouseholds = null,
                              ObservableCollection<DateBasedProfile>? dateBasedProfiles = null,
                              ObservableCollection<Desire>? desires = null,
                              ObservableCollection<DeviceCategory>? deviceCategories = null,
                              ObservableCollection<DeviceSelection>? deviceSelections = null,
                              ObservableCollection<DeviceTag>? deviceTags = null,
                              ObservableCollection<TimeLimit>? timeLimits = null,
                              ObservableCollection<EnergyStorage>? energyStorages = null,
                              ObservableCollection<Generator>? generators = null,
                              ObservableCollection<GeographicLocation>? geographicLocations = null,
                              ObservableCollection<Holiday>? holidays = null,
                              ObservableCollection<HouseType>? houseTypes = null,
                              ObservableCollection<House>? houses = null,
                              ObservableCollection<HouseholdTrait>? householdTraits = null,
                              ObservableCollection<VLoadType>? loadTypes = null,
                              ObservableCollection<Location>? locations = null,
                              ObservableCollection<Person>? persons = null,
                              ObservableCollection<RealDevice>? realDevices = null,
                              ObservableCollection<SubAffordance>? subAffordances = null,
                              ObservableCollection<TemperatureProfile>? temperatureProfiles = null,
                              ObservableCollection<TimeBasedProfile>? timeProfiles = null,
                              ObservableCollection<TransformationDevice>? transformationDevices = null,
                              ObservableCollection<AffordanceTaggingSet>? affordanceTaggingSets = null,
                              ObservableCollection<DeviceActionGroup>? deviceActionGroups = null,
                              ObservableCollection<DeviceAction>? deviceActions = null,
                              ObservableCollection<TraitTag>? traitTags = null,
                              ObservableCollection<Vacation>? vacations = null,
                              ObservableCollection<HouseholdTemplate>? householdTemplates = null,
                              ObservableCollection<Variable>? variables = null,
                              ObservableCollection<HouseholdTag>? householdTags = null,
                              ObservableCollection<TransportationDeviceCategory>? transportationDeviceCategories = null,
                              ObservableCollection<TravelRoute>? travelRoutes = null,
                              ObservableCollection<TransportationDevice>? transportationDevices = null,
                              ObservableCollection<Site>? sites = null,
                              ObservableCollection<TravelRouteSet>? travelRouteSets = null,
                              ObservableCollection<TransportationDeviceSet>? transportationDeviceSets = null,
                              ObservableCollection<ChargingStationSet>? chargingStationSets = null,
                              ObservableCollection<LivingPatternTag>? livingPatternTags = null)
    {

        /// <summary>
        /// Turns an ObservableColleciton into an ObservableCollectionWithMap, or if the input was null also returns null.
        /// </summary>
        /// <typeparam name="T">type parameter for the collection</typeparam>
        /// <param name="items">the original collection to transform</param>
        /// <returns>the collection as an ObservableCollectionWithMap</returns>
        private static ObservableCollectionWithMap<T>? CreateCollectionWithMap<T>(ObservableCollection<T>? items) where T : DBBase => items is null ? null : new(items);


        public ObservableCollectionWithMap<TransportationDeviceSet>? TransportationDeviceSets { get; } = CreateCollectionWithMap(transportationDeviceSets);
        public ObservableCollectionWithMap<TravelRouteSet>? TravelRouteSets { get; } = CreateCollectionWithMap(travelRouteSets);
        public ObservableCollectionWithMap<HouseholdTag>? HouseholdTags { get; } = CreateCollectionWithMap(householdTags);
        public ObservableCollectionWithMap<Variable>? Variables { get; } = CreateCollectionWithMap(variables);
        public ObservableCollectionWithMap<AffordanceTag>? AffordanceTags { get; } = CreateCollectionWithMap(affordanceTags);
        public ObservableCollectionWithMap<ChargingStationSet>? ChargingStationSets { get; } = CreateCollectionWithMap(chargingStationSets);
        public ObservableCollectionWithMap<Affordance>? Affordances { get; } = CreateCollectionWithMap(affordances);
        public ObservableCollectionWithMap<ModularHousehold>? ModularHouseholds { get; } = CreateCollectionWithMap(modularHouseholds);
        public ObservableCollectionWithMap<DateBasedProfile>? DateBasedProfiles { get; } = CreateCollectionWithMap(dateBasedProfiles);
        public ObservableCollectionWithMap<Desire>? Desires { get; } = CreateCollectionWithMap(desires);
        public ObservableCollectionWithMap<DeviceCategory>? DeviceCategories { get; } = CreateCollectionWithMap(deviceCategories);
        public ObservableCollectionWithMap<DeviceSelection>? DeviceSelections { get; } = CreateCollectionWithMap(deviceSelections);
        public ObservableCollectionWithMap<DeviceTag>? DeviceTags { get; } = CreateCollectionWithMap(deviceTags);
        public ObservableCollectionWithMap<TimeLimit>? TimeLimits { get; } = CreateCollectionWithMap(timeLimits);
        public ObservableCollectionWithMap<EnergyStorage>? EnergyStorages { get; } = CreateCollectionWithMap(energyStorages);
        public ObservableCollectionWithMap<Generator>? Generators { get; } = CreateCollectionWithMap(generators);
        public ObservableCollectionWithMap<GeographicLocation>? GeographicLocations { get; } = CreateCollectionWithMap(geographicLocations);
        public ObservableCollectionWithMap<Holiday>? Holidays { get; } = CreateCollectionWithMap(holidays);
        public ObservableCollectionWithMap<HouseType>? HouseTypes { get; } = CreateCollectionWithMap(houseTypes);
        public ObservableCollectionWithMap<House>? Houses { get; } = CreateCollectionWithMap(houses);
        public ObservableCollectionWithMap<HouseholdTrait>? HouseholdTraits { get; } = CreateCollectionWithMap(householdTraits);
        public ObservableCollectionWithMap<VLoadType>? LoadTypes { get; } = CreateCollectionWithMap(loadTypes);
        public ObservableCollectionWithMap<Location>? Locations { get; } = CreateCollectionWithMap(locations);
        public ObservableCollectionWithMap<Person>? Persons { get; } = CreateCollectionWithMap(persons);
        public ObservableCollectionWithMap<RealDevice>? RealDevices { get; } = CreateCollectionWithMap(realDevices);
        public ObservableCollectionWithMap<SubAffordance>? SubAffordances { get; } = CreateCollectionWithMap(subAffordances);
        public ObservableCollectionWithMap<TemperatureProfile>? TemperatureProfiles { get; } = CreateCollectionWithMap(temperatureProfiles);
        public ObservableCollectionWithMap<TimeBasedProfile>? TimeProfiles { get; } = CreateCollectionWithMap(timeProfiles);
        public ObservableCollectionWithMap<TransformationDevice>? TransformationDevices { get; } = CreateCollectionWithMap(transformationDevices);
        public ObservableCollectionWithMap<AffordanceTaggingSet>? AffordanceTaggingSets { get; } = CreateCollectionWithMap(affordanceTaggingSets);
        public ObservableCollectionWithMap<DeviceActionGroup>? DeviceActionGroups { get; } = CreateCollectionWithMap(deviceActionGroups);
        public ObservableCollectionWithMap<DeviceAction>? DeviceActions { get; } = CreateCollectionWithMap(deviceActions);
        public ObservableCollectionWithMap<TraitTag>? TraitTags { get; } = CreateCollectionWithMap(traitTags);
        public ObservableCollectionWithMap<Vacation>? Vacations { get; } = CreateCollectionWithMap(vacations);
        public ObservableCollectionWithMap<HouseholdTemplate>? HouseholdTemplates { get; } = CreateCollectionWithMap(householdTemplates);
        public ObservableCollectionWithMap<Site>? Sites { get; } = CreateCollectionWithMap(sites);
        public ObservableCollectionWithMap<TransportationDeviceCategory>? TransportationDeviceCategories { get; } = CreateCollectionWithMap(transportationDeviceCategories);
        public ObservableCollectionWithMap<TravelRoute>? TravelRoutes { get; } = CreateCollectionWithMap(travelRoutes);
        public ObservableCollectionWithMap<TransportationDevice>? TransportationDevices { get; } = CreateCollectionWithMap(transportationDevices);
        public ObservableCollectionWithMap<LivingPatternTag>? LivingPatternTags { get; } = CreateCollectionWithMap(livingPatternTags);
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