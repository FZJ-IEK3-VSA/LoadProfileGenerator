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
                                  [ItemNotNull][CanBeNull] ObservableCollection<ChargingStationSet> chargingStationSets = null,
                                      [ItemNotNull][CanBeNull] ObservableCollection<LivingPatternTag> livingPatternTags = null)
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
            LivingPatternTags = livingPatternTags;
        }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TransportationDeviceSet> TransportationDeviceSets { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TravelRouteSet> TravelRouteSets { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<HouseholdTag> HouseholdTags { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<Variable> Variables { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<AffordanceTag> AffordanceTags { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<ChargingStationSet> ChargingStationSets { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<Affordance> Affordances { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<ModularHousehold> ModularHouseholds { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<DateBasedProfile> DateBasedProfiles { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<Desire> Desires { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<DeviceCategory> DeviceCategories { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<DeviceSelection> DeviceSelections { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<DeviceTag> DeviceTags { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TimeLimit> TimeLimits { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<EnergyStorage> EnergyStorages { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<Generator> Generators { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<GeographicLocation> GeographicLocations { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<Holiday> Holidays { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<HouseType> HouseTypes { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<House> Houses { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<HouseholdTrait> HouseholdTraits { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<VLoadType> LoadTypes { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<Location> Locations { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<Person> Persons { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<RealDevice> RealDevices { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<SubAffordance> SubAffordances { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TemperatureProfile> TemperatureProfiles { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TimeBasedProfile> TimeProfiles { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TransformationDevice> TransformationDevices { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<AffordanceTaggingSet> AffordanceTaggingSets { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<DeviceActionGroup> DeviceActionGroups { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<DeviceAction> DeviceActions { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TraitTag> TraitTags { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<Vacation> Vacations { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<HouseholdTemplate> HouseholdTemplates { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<Site> Sites { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TransportationDeviceCategory> TransportationDeviceCategories { get;  }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TravelRoute> TravelRoutes { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TransportationDevice> TransportationDevices { get; }

        public ObservableCollection<LivingPatternTag > LivingPatternTags { get; set; }
    }
}