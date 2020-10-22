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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Helpers;
using Database.Tables;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using Database.Tables.Validation;
using JetBrains.Annotations;

#region house

#endregion

[assembly: InternalsVisibleTo("LoadProfileGenerator")]
[assembly: InternalsVisibleTo("Database.Tests")]

namespace Database {
    [SuppressMessage("ReSharper", "CatchAllClause")]
    [SuppressMessage("ReSharper", "ThrowingSystemException")]
    public sealed class Simulator // : INotifyPropertyChanged
    {

        [CanBeNull]
        public IAssignableDevice GetAssignableDeviceByGuid([CanBeNull] StrGuid? guid)
        {
            if (guid == null) {
                return null;
            }
            var dev = RealDevices.FindByGuid(guid);
            if (dev != null) {
                return dev;
            }
            var devc = DeviceCategories.FindByGuid(guid);
            if (devc != null)
            {
                return devc;
            }
            var devAc = DeviceActions.FindByGuid(guid);
            if (devAc != null)
            {
                return devAc;
            }
            var deviceActionGroup = DeviceActionGroups.FindByGuid(guid);
            if (deviceActionGroup != null)
            {
                return deviceActionGroup;
            }
            return null;
        }
        [CanBeNull] private DeviceCategory _dcnone;

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public Simulator([NotNull] string connectionString, bool ignoreMissingTables = false) {
            ConnectionString = connectionString;
            MyGeneralConfig = new GeneralConfig(ConnectionString);
            CategoryOrDevice.Add("Device");
            CategoryOrDevice.Add("Device Category");

            Categories = new ObservableCollection<object>();
            LoadTypes = new CategoryDBBase<VLoadType>("Load types");
            Categories.Add(LoadTypes);
            Holidays = new CategoryDBBase<Holiday>("Holidays");
            Categories.Add(Holidays);
            GeographicLocations = new CategoryDBBase<GeographicLocation>("Geographic Locations / Cities");
            Categories.Add(GeographicLocations);
            TemperatureProfiles = new CategoryDBBase<TemperatureProfile>("Temperature Profiles");
            Categories.Add(TemperatureProfiles);
            DateBasedProfiles = new CategoryDBBase<DateBasedProfile>("Date Based Profiles");
            Categories.Add(DateBasedProfiles);
            Vacations = new CategoryDBBase<Vacation>("Vacations");
            Categories.Add(Vacations);
            Desires = new CategoryDBBase<Desire>("Desires");
            Categories.Add(Desires);
            Locations = new CategoryDBBase<Location>("Locations");
            Categories.Add(Locations);
            Persons = new CategoryDBBase<Person>("Persons");
            Categories.Add(Persons);
            DeviceCategories = new CategoryDeviceCategory();
            Categories.Add(DeviceCategories);
            RealDevices = new CategoryDBBase<RealDevice>("Devices");
            Categories.Add(RealDevices);
            DeviceActions = new CategoryDBBase<DeviceAction>("Device Actions");
            Categories.Add(DeviceActions);
            DeviceActionGroups = new CategoryDBBase<DeviceActionGroup>("Device Action Groups");
            Categories.Add(DeviceActionGroups);
            DeviceTaggingSets = new CategoryDBBase<DeviceTaggingSet>("Device Tagging Set");
            Categories.Add(DeviceTaggingSets);
            Timeprofiles = new CategoryDBBase<TimeBasedProfile>("Time Profiles");
            Categories.Add(Timeprofiles);
            TimeLimits = new CategoryDBBase<TimeLimit>("Time Limits");
            Categories.Add(TimeLimits);
            Variables = new CategoryDBBase<Variable>("Variables");
            Categories.Add(Variables);
            Affordances = new CategoryAffordance("Affordances");
            Categories.Add(Affordances);
            SubAffordances = new CategoryDBBase<SubAffordance>("Sub-Affordances");
            Categories.Add(SubAffordances);
            AffordanceTaggingSets = new CategoryDBBase<AffordanceTaggingSet>("Affordance Tagging Set");
            Categories.Add(AffordanceTaggingSets);
            TraitTags = new CategoryDBBase<TraitTag>("Household Trait Tags");
            Categories.Add(TraitTags);
            LivingPatternTags = new CategoryDBBase<LivingPatternTag>("Living Pattern Tags");
            Categories.Add(LivingPatternTags);
            HouseholdTraits = new CategoryDBBase<HouseholdTrait>("Household Traits");
            Categories.Add(HouseholdTraits);
            HouseholdTags = new CategoryDBBase<HouseholdTag>("Household Template Tags");
            Categories.Add(HouseholdTags);
            HouseholdTemplates = new CategoryDBBase<HouseholdTemplate>("Household Templates");
            Categories.Add(HouseholdTemplates);
            DeviceSelections = new CategoryDBBase<DeviceSelection>("Device Selections");
            Categories.Add(DeviceSelections);
            //TemplatePersons = new CategoryDBBase<TemplatePerson>("Template Persons");
            //Categories.Add(TemplatePersons);
            ModularHouseholds = new CategoryDBBase<ModularHousehold>("Modular Households");
            Categories.Add(ModularHouseholds);
            TransformationDevices = new CategoryDBBase<TransformationDevice>("Transformation Devices");
            Categories.Add(TransformationDevices);
            EnergyStorages = new CategoryDBBase<EnergyStorage>("Energy Storages");
            Categories.Add(EnergyStorages);
            Generators = new CategoryDBBase<Generator>("Externally Controlled Generators");
            Categories.Add(Generators);
            HouseTypes = new CategoryDBBase<HouseType>("House Types");
            Categories.Add(HouseTypes);
            Houses = new CategoryDBBase<House>("Houses");
            Categories.Add(Houses);
            HouseholdPlans = new CategoryDBBase<HouseholdPlan>("Household Plans");
            Categories.Add(HouseholdPlans);
            SettlementTemplates = new CategoryDBBase<SettlementTemplate>("Settlement Templates");
            Categories.Add(SettlementTemplates);
            Settlements = new CategorySettlement();
            Categories.Add(Settlements);
            Sites = new CategoryDBBase<Site>("Sites");
            Categories.Add(Sites);

            TransportationDeviceCategories = new CategoryDBBase<TransportationDeviceCategory>("Transportation Device Categories");
            Categories.Add(TransportationDeviceCategories);

            TransportationDevices = new CategoryDBBase<TransportationDevice>("Transportation Devices");
            Categories.Add(TransportationDevices);

            TransportationDeviceSets = new CategoryDBBase<TransportationDeviceSet>("Transportation Devices Sets");
            Categories.Add(TransportationDeviceSets);

            TravelRoutes = new CategoryDBBase<TravelRoute>("Travel Routes");
            Categories.Add(TravelRoutes);

            TravelRouteSets = new CategoryDBBase<TravelRouteSet>("Travel Route Sets");
            Categories.Add(TravelRouteSets);

            ChargingStationSets = new CategoryDBBase<ChargingStationSet>("Charging Station Sets");
            Categories.Add(ChargingStationSets);

            Categories.Add(new OtherCategory("Calculation"));
            Categories.Add(new OtherCategory("Settings"));
            CalculationOutcomes = new CategoryOutcome();
            Categories.Add(CalculationOutcomes);
                try {
                if (!ignoreMissingTables)
                {
                    DatabaseVersionChecker.CheckVersion(ConnectionString);
                }
                LoadFromDB(ignoreMissingTables);
                }
                catch (Exception e) {
                    if (Config.IsInUnitTesting) {
                        Logger.Exception(e);
                    }

                    throw;
                }
            Logger.Info("Loaded the Database");
            foreach (dynamic category in Categories) {
                if (category.LoadingNumber == -1) {
                    throw new LPGException("Loading of the database failed due to invalid loading number for " +
                                           category.Name + " of " + category.LoadingNumber + ".");
                }
            }
        }

        [NotNull]
        public CategoryAffordance Affordances { get; }

        [NotNull]
        public CategoryDBBase<AffordanceTaggingSet> AffordanceTaggingSets { get; }

        [NotNull]
        public CategoryOutcome CalculationOutcomes { get; }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<object> Categories { get; }

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<string> CategoryOrDevice { get; } = new ObservableCollection<string>();

        [NotNull]
        public string ConnectionString { get; }

        [NotNull]
        public CategoryDBBase<DateBasedProfile> DateBasedProfiles { get; }

        [NotNull]
        public CategoryDBBase<Desire> Desires { get; }

        [NotNull]
        public CategoryDBBase<DeviceActionGroup> DeviceActionGroups { get; }

        [NotNull]
        public CategoryDBBase<DeviceAction> DeviceActions { get; }

        [NotNull]
        public CategoryDeviceCategory DeviceCategories { get; }

        [NotNull]
        public CategoryDBBase<DeviceSelection> DeviceSelections { get; }

        [NotNull]
        public CategoryDBBase<DeviceTaggingSet> DeviceTaggingSets { get; }

        [NotNull]
        public CategoryDBBase<EnergyStorage> EnergyStorages { get; }

        [NotNull]
        public CategoryDBBase<Generator> Generators { get; }

        [NotNull]
        public CategoryDBBase<GeographicLocation> GeographicLocations { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<Holiday> Holidays { get; }

        [NotNull]
        public CategoryDBBase<HouseholdPlan> HouseholdPlans { get; }

        [NotNull]
        public CategoryDBBase<HouseholdTag> HouseholdTags { get; }

        [NotNull]
        public CategoryDBBase<HouseholdTemplate> HouseholdTemplates { get; }

        [NotNull]
        public CategoryDBBase<HouseholdTrait> HouseholdTraits { get; }

        [NotNull]
        public CategoryDBBase<House> Houses { get; }

        [NotNull]
        public CategoryDBBase<HouseType> HouseTypes { get; }

        [NotNull]
        [UsedImplicitly]
        public CategoryDBBase<VLoadType> LoadTypes { get; }

        [NotNull]
        public CategoryDBBase<Location> Locations { get; }

        [NotNull]
        public CategoryDBBase<ModularHousehold> ModularHouseholds { get; }

        [NotNull]
        public GeneralConfig MyGeneralConfig { get; private set; }

        [NotNull]
        public CategoryDBBase<Person> Persons { get; }

        [NotNull]
        public CategoryDBBase<RealDevice> RealDevices { get; }

        [NotNull]
        public CategorySettlement Settlements { get; }

        [NotNull]
        public CategoryDBBase<SettlementTemplate> SettlementTemplates { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<Site> Sites { get; }

        [NotNull]
        public CategoryDBBase<SubAffordance> SubAffordances { get; }

        [NotNull]
        public CategoryDBBase<TemperatureProfile> TemperatureProfiles { get; }

        //[NotNull]
        //public CategoryDBBase<TemplatePerson> TemplatePersons { get; }

        [NotNull]
        public CategoryDBBase<TimeLimit> TimeLimits { get; }

        [NotNull]
        public CategoryDBBase<TimeBasedProfile> Timeprofiles { get; }

        [NotNull]
        public CategoryDBBase<TraitTag> TraitTags { get; }

        [NotNull]
        public CategoryDBBase<TransformationDevice> TransformationDevices { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<TransportationDeviceCategory> TransportationDeviceCategories { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<TransportationDevice> TransportationDevices { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<TransportationDeviceSet> TransportationDeviceSets { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<TravelRoute> TravelRoutes { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<ChargingStationSet> ChargingStationSets { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<TravelRouteSet> TravelRouteSets { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<Vacation> Vacations { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<Variable> Variables { get; }

        [UsedImplicitly]
        [NotNull]
        public CategoryDBBase<LivingPatternTag> LivingPatternTags { get; }


        [SuppressMessage("Microsoft.Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht")]
        public void DeleteAllTemplatedItems(bool noConfirmation) {
            if (!noConfirmation) {
                var mbr = MessageWindowHandler.Mw.ShowYesNoMessage("Delete all templated items?", "Delete?");
                if (mbr != LPGMsgBoxResult.Yes) {
                    return;
                }
            }
            FindAndDeleteAllTemplated();
        }

        public int FindAndDeleteAllTemplated() {
            var settlements2Delete = new List<Settlement>();
            //settlements
            foreach (var settlement in Settlements.Items) {
                if (settlement.CreationType == CreationType.TemplateCreated) {
                    settlements2Delete.Add(settlement);
                }
            }
            var totalCount = settlements2Delete.Count;

            foreach (var dbBase in settlements2Delete) {
                Settlements.DeleteItemNoWait(dbBase);
            }

            //houses
            var housesToDelete = new List<House>();
            foreach (var house in Houses.Items) {
                if (house.CreationType == CreationType.TemplateCreated) {
                    housesToDelete.Add(house);
                }
            }
            totalCount += housesToDelete.Count;
            var t2 = new Thread(() => {
                foreach (var dbBase in housesToDelete) {
                    Houses.DeleteItemNoWait(dbBase);
                }
            });
            t2.Start();
            t2.Join();
            //modularhosueholds

            var mhhToDelete = new List<ModularHousehold>();
            foreach (var household in ModularHouseholds.Items) {
                if (household.CreationType == CreationType.TemplateCreated) {
                    mhhToDelete.Add(household);
                }
            }
            totalCount += mhhToDelete.Count;

            var t3 = new Thread(() => {
                foreach (var dbBase in mhhToDelete) {
                    ModularHouseholds.DeleteItemNoWait(dbBase);
                }
            });
            t3.Start();
            t3.Join();

            //vacations

            var vacationsToDelete = new List<Vacation>();
            foreach (var vacation in Vacations.Items) {
                if (vacation.CreationType == CreationType.TemplateCreated) {
                    vacationsToDelete.Add(vacation);
                }
            }
            totalCount += mhhToDelete.Count;

            var t4 = new Thread(() => {
                foreach (var dbBase in vacationsToDelete) {
                    Vacations.DeleteItemNoWait(dbBase);
                }
            });
            t4.Start();
            t4.Join();

            Logger.Info("Finished deleting all templated items.");
            return totalCount;
        }

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private List<LoadingEntry> GetLoadingActions(bool ignoreMissingTables) {
            var actions = new List<LoadingEntry>
            {
                new LoadingEntry("Loadtypes",
                () => VLoadType.LoadFromDatabase(LoadTypes.Items, ConnectionString, ignoreMissingTables),
                LoadTypes),
                new LoadingEntry("Holidays",
                () => Holiday.LoadFromDatabase(Holidays.Items, ConnectionString, ignoreMissingTables), Holidays),
                new LoadingEntry("Variables",
                () => Variable.LoadFromDatabase(Variables.Items, ConnectionString, ignoreMissingTables), Variables),

                new LoadingEntry("Date Based Profiles",
                () => DateBasedProfile.LoadFromDatabase(DateBasedProfiles.Items, ConnectionString,
                    ignoreMissingTables), DateBasedProfiles),
                new LoadingEntry("Vacations",
                () => Vacation.LoadFromDatabase(Vacations.Items, ConnectionString, ignoreMissingTables),
                Vacations),
                new LoadingEntry("Desires",
                () => Desire.LoadFromDatabase(Desires.Items, ConnectionString, ignoreMissingTables), Desires),
                new LoadingEntry("Time Profiles",
                () => TimeBasedProfile.LoadFromDatabase(Timeprofiles.Items, ConnectionString, ignoreMissingTables),
                Timeprofiles),
                new LoadingEntry("Temperature Profiles",
                () => TemperatureProfile.LoadFromDatabase(TemperatureProfiles.Items, ConnectionString,
                    ignoreMissingTables), TemperatureProfiles),
                new LoadingEntry("Generators",
                () => Generator.LoadFromDatabase(Generators.Items, ConnectionString, LoadTypes.Items,
                    DateBasedProfiles.Items, ignoreMissingTables), Generators),
                new LoadingEntry("Energy Storages",
                () => EnergyStorage.LoadFromDatabase(EnergyStorages.Items, ConnectionString, LoadTypes.Items,Variables.Items,
                    ignoreMissingTables), EnergyStorages),
                new LoadingEntry("Transformation Devices",
                () => TransformationDevice.LoadFromDatabase(TransformationDevices.Items, ConnectionString,
                    LoadTypes.Items, Variables.Items, ignoreMissingTables), TransformationDevices),
                new LoadingEntry("Device Categories", () =>
                {
                    DeviceCategory.LoadFromDatabase(DeviceCategories.Items, out _dcnone, ConnectionString,
                        RealDevices.Items, ignoreMissingTables);
                    DeviceCategories.DeviceCategoryNone = _dcnone;
                }, DeviceCategories),

                new LoadingEntry("Real Devices",
                () => RealDevice.LoadFromDatabase(RealDevices.Items, DeviceCategories.Items,
                    DeviceCategories.DeviceCategoryNone, ConnectionString, LoadTypes.Items, Timeprofiles.Items,
                    ignoreMissingTables), RealDevices),
                new LoadingEntry("Device Action Groups",
                () => DeviceActionGroup.LoadFromDatabase(DeviceActionGroups.Items, ConnectionString,
                    ignoreMissingTables), DeviceActionGroups),
                new LoadingEntry("Device Actions",
                () => DeviceAction.LoadFromDatabase(DeviceActions.Items, ConnectionString, Timeprofiles.Items,
                    RealDevices.Items, LoadTypes.Items, DeviceActionGroups.Items, ignoreMissingTables),
                DeviceActions),
                new LoadingEntry("Device Tagging Sets",
                () => DeviceTaggingSet.LoadFromDatabase(DeviceTaggingSets.Items, ConnectionString,
                    ignoreMissingTables, RealDevices.Items, LoadTypes.Items), DeviceTaggingSets),
                new LoadingEntry("Persons",
                () => Person.LoadFromDatabase(Persons.Items, ConnectionString,
                    ignoreMissingTables), Persons),
                new LoadingEntry("Locations",
                () => Location.LoadFromDatabase(Locations.Items, ConnectionString, RealDevices.Items,
                    DeviceCategories.Items, LoadTypes.Items, ignoreMissingTables), Locations),
                new LoadingEntry("Time Limits",
                () => TimeLimit.LoadFromDatabase(TimeLimits.Items, DateBasedProfiles.Items, ConnectionString,
                    ignoreMissingTables), TimeLimits),
                new LoadingEntry("Geographic Locations",
                () => GeographicLocation.LoadFromDatabase(GeographicLocations.Items, ConnectionString,
                    Holidays.Items, TimeLimits.Items, ignoreMissingTables), GeographicLocations),
                new LoadingEntry("Subaffordances",
                () => SubAffordance.LoadFromDatabase(SubAffordances.Items, ConnectionString, Desires.Items,
                    ignoreMissingTables, Locations.Items, Variables.Items), SubAffordances),
                new LoadingEntry("Affordances",
                () => Affordance.LoadFromDatabase(Affordances.Items, ConnectionString, Timeprofiles.Items,
                    DeviceCategories.Items, RealDevices.Items, Desires.Items, SubAffordances.Items,
                    LoadTypes.Items, TimeLimits.Items, DeviceActions.Items, DeviceActionGroups.Items,
                    Locations.Items, ignoreMissingTables, Variables.Items), Affordances),
                new LoadingEntry("Affordance Tagging Sets",
                () => AffordanceTaggingSet.LoadFromDatabase(AffordanceTaggingSets.Items, ConnectionString,
                    ignoreMissingTables, Affordances.Items, LoadTypes.Items), AffordanceTaggingSets),
                new LoadingEntry("Trait Tags",
                () => TraitTag.LoadFromDatabase(TraitTags.Items, ConnectionString, ignoreMissingTables),
                TraitTags),
                new LoadingEntry("Living Pattern Tags",
                    () => LivingPatternTag.LoadFromDatabase(LivingPatternTags.Items, ConnectionString, ignoreMissingTables),
                    LivingPatternTags),
                new LoadingEntry("Household Traits",
                () => HouseholdTrait.LoadFromDatabase(HouseholdTraits.Items, ConnectionString, Locations.Items,
                    Affordances.Items, RealDevices.Items, DeviceCategories.Items, Timeprofiles.Items,
                    LoadTypes.Items, TimeLimits.Items, Desires.Items, DeviceActions.Items, DeviceActionGroups.Items,
                    TraitTags.Items, LivingPatternTags.Items, ignoreMissingTables, Variables.Items), HouseholdTraits),
                new LoadingEntry("Device Selections",
                () => DeviceSelection.LoadFromDatabase(DeviceSelections.Items, ConnectionString,
                    DeviceCategories.Items, RealDevices.Items, DeviceActions.Items, DeviceActionGroups.Items,
                    ignoreMissingTables), DeviceSelections),
                new LoadingEntry("Household Tags",
                () => HouseholdTag.LoadFromDatabase(HouseholdTags.Items, ConnectionString, ignoreMissingTables),
                HouseholdTags),
                new LoadingEntry("Modular Households",
                () => ModularHousehold.LoadFromDatabase(ModularHouseholds.Items, ConnectionString,
                    HouseholdTraits.Items, DeviceSelections.Items, ignoreMissingTables, Persons.Items,
                    Vacations.Items, HouseholdTags.Items, TraitTags.Items, LivingPatternTags.Items), ModularHouseholds),
                new LoadingEntry("Household Templates",
                () => HouseholdTemplate.LoadFromDatabase(HouseholdTemplates.Items, ConnectionString,
                    HouseholdTraits.Items, ignoreMissingTables, Persons.Items, TraitTags.Items, Vacations.Items,
                    HouseholdTags.Items, DateBasedProfiles.Items, LivingPatternTags.Items), HouseholdTemplates),
                //new LoadingEntry("Template Persons",
                //() => TemplatePerson.LoadFromDatabase(TemplatePersons.Items, ConnectionString, HouseholdTraits.Items,
                //    ignoreMissingTables, ModularHouseholds.Items, Persons.Items), TemplatePersons),
                new LoadingEntry("Household Plans",
                () => HouseholdPlan.LoadFromDatabase(HouseholdPlans.Items, ConnectionString, ignoreMissingTables,
                    Persons.Items, AffordanceTaggingSets.Items, ModularHouseholds.Items),
                HouseholdPlans),
                new LoadingEntry("House Types",
                () => HouseType.LoadFromDatabase(HouseTypes.Items, ConnectionString, RealDevices.Items,
                    DeviceCategories.Items, Timeprofiles.Items, TimeLimits.Items, LoadTypes.Items,
                    TransformationDevices.Items, EnergyStorages.Items, Generators.Items, ignoreMissingTables,
                    Locations.Items, DeviceActions.Items, DeviceActionGroups.Items, Variables.Items), HouseTypes),
                new LoadingEntry("Transportation Device Categories",
                    () => TransportationDeviceCategory.LoadFromDatabase(TransportationDeviceCategories.Items, ConnectionString,
                        ignoreMissingTables), TransportationDeviceCategories),
                new LoadingEntry("Sites",
                    () => Site.LoadFromDatabase(Sites.Items,
                        ConnectionString, ignoreMissingTables,
                        Locations.Items), Sites),
                new LoadingEntry("Transportation Devices",
                    () => TransportationDevice.LoadFromDatabase(TransportationDevices.Items, ConnectionString, ignoreMissingTables,
                        TransportationDeviceCategories.Items,LoadTypes.Items), TransportationDevices),

                new LoadingEntry("Transportation Device Sets",
                    () => TransportationDeviceSet.LoadFromDatabase(TransportationDeviceSets.Items, ConnectionString,
                        ignoreMissingTables,TransportationDevices.Items), TransportationDeviceSets),

                new LoadingEntry("Travel Routes",
                    () => TravelRoute.LoadFromDatabase(TravelRoutes.Items, ConnectionString, ignoreMissingTables,
                        TransportationDeviceCategories.Items, Sites.Items), TravelRoutes),

                new LoadingEntry("Travel Route Sets",
                    () => TravelRouteSet.LoadFromDatabase(TravelRouteSets.Items, ConnectionString, ignoreMissingTables,
                        TravelRoutes.Items), TravelRouteSets),
                new LoadingEntry("Charging Station Sets",
                    () => ChargingStationSet.LoadFromDatabase(ChargingStationSets.Items,
                        ConnectionString, ignoreMissingTables,
                        LoadTypes.Items, TransportationDeviceCategories.Items,Sites.Items), ChargingStationSets),

                new LoadingEntry("Houses",
                () => House.LoadFromDatabase(Houses.Items, ConnectionString, TemperatureProfiles.Items,
                    GeographicLocations.Items, HouseTypes.Items,
                    ModularHouseholds.Items,ChargingStationSets.Items,
                    TransportationDeviceSets.Items,TravelRouteSets.Items,
                    ignoreMissingTables), Houses),
                new LoadingEntry("Settlements",
                () => Settlement.LoadFromDatabase(Settlements.Items, ConnectionString,
                    TemperatureProfiles.Items, GeographicLocations.Items, ModularHouseholds.Items, Houses.Items,
                    ignoreMissingTables), Settlements),
                new LoadingEntry("Settlement Templates",
                () => SettlementTemplate.LoadFromDatabase(SettlementTemplates.Items, ConnectionString,
                    HouseholdTemplates.Items, HouseTypes.Items, ignoreMissingTables, TemperatureProfiles.Items,
                    GeographicLocations.Items, HouseholdTags.Items, HouseholdTraits.Items,
                    ChargingStationSets.Items,TravelRouteSets.Items,TransportationDeviceSets.Items), SettlementTemplates),
                new LoadingEntry("Settings",
                () => MyGeneralConfig = GeneralConfig.LoadFromDatabase(ConnectionString, ignoreMissingTables), null),
                new LoadingEntry("Calculation Outcomes",
                () => CalculationOutcome.LoadFromDatabase(CalculationOutcomes.Items, ConnectionString,
                    ignoreMissingTables), CalculationOutcomes)
            };
            return actions;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void LoadFromDB(bool ignoreMissingTables) {
            DBBase.IsLoading = true;
            DBBase.TypesThatMadeGuids.Clear();
            DBBase.GuidCreationCount = 0;
            var step = 1;
            Logger.Info("Starting the database from " + ConnectionString);
            var start = DateTime.Now;
            var startLoading = DateTime.Now;
            var list = GetLoadingActions(ignoreMissingTables);
            foreach (var loadingAction in list) {
                DataReader.TotalReads = 0;
                var prevguidCreationCount = DBBase.GuidCreationCount;
                loadingAction.Action.Invoke();
                var afterGuidCreationcount = DBBase.GuidCreationCount;
                int newGuids = afterGuidCreationcount - prevguidCreationCount;
                var guidscreatedstring = "";
                if(newGuids > 0) {
                    guidscreatedstring = ", " + newGuids + " Guids created";
                }

                LogLoadingProgress(ref start, ref step,
                    loadingAction.Name + " (" + DataReader.TotalReads + " database reads"+guidscreatedstring+") ");
                if (loadingAction.CategoryDBBase != null) {
                    loadingAction.CategoryDBBase.LoadingNumber = step;
                }
                else {
                    if (loadingAction.Name != "Settings") {
                        throw new LPGException("No loading number assigned to " + loadingAction.Name);
                    }
                }
            }
            Logger.Info("Total Loading Time:" +
                        (DateTime.Now - startLoading).TotalSeconds.ToString("0.000", CultureInfo.CurrentCulture) +
                        " seconds");
            if (!ignoreMissingTables) {
                foreach (string typesThatMadeGuid in DBBase.TypesThatMadeGuids) {
                    Logger.Info("Made Guids in: " + typesThatMadeGuid);
                    if (typesThatMadeGuid.Contains("SingleSetting.LoadFromDatabase") ||
                        typesThatMadeGuid.Contains("SingleOption.LoadFromDatabase")) {
                        MyGeneralConfig.SaveEverything();
                    }
                }
            }

            foreach (var category in Categories) {
                var thisType = category.GetType();
                if (thisType.Name.Contains("CategoryDBBase") || thisType.Name.Contains("CategoryDeviceCategory") ||
                    thisType.Name.Contains("CategorySettlement") || thisType.Name.Contains("CategoryAffordance")) {
                    dynamic d = category;
                    var saveToDB = !ignoreMissingTables;
                    d.CheckForDuplicateNames(saveToDB);
                    if (!ignoreMissingTables && DBBase.GuidCreationCount > 0) {
                        d.SaveEverything();
                    }
                }else if (thisType.Name.Contains("CategoryOutcome")) {
                    if (!ignoreMissingTables && DBBase.GuidCreationCount > 0)
                    {
                        dynamic d = category;
                        d.SaveEverything();
                    }
                }
                else {
                    Logger.Info(category.ToString());
                }
            }
            DBBase.IsLoading = false;
        }

        private static void LogLoadingProgress(ref DateTime lasttime, ref int step, [NotNull] string description) {
            var now = DateTime.Now;
            Logger.Debug("Loaded " + description + " (Part " + step + ")  in " +
                         (now - lasttime).TotalMilliseconds.ToString("0", CultureInfo.CurrentCulture) +
                         " milliseconds");
            step++;
            lasttime = now;
        }

        private class LoadingEntry {
            public LoadingEntry([NotNull] string name, [NotNull] Action action, [CanBeNull] dynamic categoryDBBase) {
                Action = action;
                CategoryDBBase = categoryDBBase;
                Name = name;
            }

            [NotNull]
            public Action Action { get; }
            [CanBeNull]
            public dynamic CategoryDBBase { get; }
            [NotNull]
            public string Name { get; }
        }
    }
}