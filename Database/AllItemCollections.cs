using System.Collections.ObjectModel;
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
                                  [ItemNotNull] [CanBeNull] ObservableCollection<DeviceTag> deviceTags = null,
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
                                  [ItemNotNull][CanBeNull] ObservableCollection<ChargingStationSet> chargingStationSets = null)
        {
            AffordanceTags = affordanceTags;
            Affordances = affordances;
            ModularHouseholds = modularHouseholds;
            DateBasedProfiles = dateBasedProfiles;
            Desires = desires;
            DeviceCategories = deviceCategories;
            DeviceSelections = deviceSelections;
            DeviceTags = deviceTags;
            TimeLimits = timeLimits;
            EnergyStorages = energyStorages;
            Generators = generators;
            GeographicLocations = geographicLocations;
            Holidays = holidays;
            HouseTypes = houseTypes;
            Houses = houses;
            HouseholdTraits = householdTraits;
            LoadTypes = loadTypes;
            Locations = locations;
            Persons = persons;
            RealDevices = realDevices;
            SubAffordances = subAffordances;
            TemperatureProfiles = temperatureProfiles;
            TimeProfiles = timeProfiles;
            TransformationDevices = transformationDevices;
            AffordanceTaggingSets = affordanceTaggingSets;
            DeviceActionGroups = deviceActionGroups;
            DeviceActions = deviceActions;
            TraitTags = traitTags;
            Vacations = vacations;
            HouseholdTemplates = householdTemplates;
            Variables = variables;
            HouseholdTags = householdTags;
            Sites = sites;
            TransportationDeviceCategories = transportationDeviceCategories;
            TravelRoutes = travelRoutes;
            TransportationDevices = transportationDevices;
            TravelRouteSets = travelRouteSets;
            TransportationDeviceSets = transportationDeviceSets;
            ChargingStationSets = chargingStationSets;
        }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TransportationDeviceSet> TransportationDeviceSets { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TravelRouteSet> TravelRouteSets { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HouseholdTag> HouseholdTags { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Variable> Variables { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<AffordanceTag> AffordanceTags { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<ChargingStationSet> ChargingStationSets { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Affordance> Affordances { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<ModularHousehold> ModularHouseholds { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DateBasedProfile> DateBasedProfiles { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Desire> Desires { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DeviceCategory> DeviceCategories { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DeviceSelection> DeviceSelections { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DeviceTag> DeviceTags { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TimeLimit> TimeLimits { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<EnergyStorage> EnergyStorages { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Generator> Generators { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<GeographicLocation> GeographicLocations { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Holiday> Holidays { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HouseType> HouseTypes { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<House> Houses { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HouseholdTrait> HouseholdTraits { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<VLoadType> LoadTypes { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Location> Locations { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Person> Persons { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<RealDevice> RealDevices { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<SubAffordance> SubAffordances { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TemperatureProfile> TemperatureProfiles { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TimeBasedProfile> TimeProfiles { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TransformationDevice> TransformationDevices { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<AffordanceTaggingSet> AffordanceTaggingSets { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DeviceActionGroup> DeviceActionGroups { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DeviceAction> DeviceActions { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TraitTag> TraitTags { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Vacation> Vacations { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HouseholdTemplate> HouseholdTemplates { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Site> Sites { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TransportationDeviceCategory> TransportationDeviceCategories { get;  }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TravelRoute> TravelRoutes { get; }
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TransportationDevice> TransportationDevices { get; }
    }
}