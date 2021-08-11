using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Automation;
using Automation.ResultFiles;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database.Tables.Houses;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.Houses {

    public class SettlementTemplateTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SettlementTemplatePreviewTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var st = sim.SettlementTemplates.CreateNewItem(sim.ConnectionString);
                st.AddHouseSize(1, 2, 0.50);
                st.AddHouseSize(1, 2, 0.50);
                st.AddHouseType(sim.HouseTypes[0]);
                st.TemperatureProfile = sim.TemperatureProfiles[0];
                st.GeographicLocation = sim.GeographicLocations[0];
                st.DesiredHHCount = 100;
                var limittrait = sim.HouseholdTraits.FindFirstByName("Showering with electric Air Heater");
                if (limittrait == null)
                {
                    throw new LPGException("Trait not found");
                }
                st.AddTraitLimit(limittrait, 1);
                st.AddHouseholdDistribution(1, 1, 0.95, EnergyIntensityType.EnergyIntensive);
                st.AddHouseholdDistribution(1, 2, 0.05, EnergyIntensityType.EnergySaving);
                st.AddHouseType(sim.HouseTypes[0]);
                st.AddChargingStationSet(sim.ChargingStationSets[0]);
                st.AddTransportationDeviceSet(sim.TransportationDeviceSets[0]);
                st.AddTravelRouteSet(sim.TravelRouteSets[0]);
                foreach (var template in sim.HouseholdTemplates.Items)
                {
                    st.AddHouseholdTemplate(template);
                }

                st.GenerateSettlementPreview(sim);
                var i = 1;
                var traitCounts = new Dictionary<string, int>();
                foreach (var housePreviewEntry in st.HousePreviewEntries)
                {
                    Logger.Info("-------------------");
                    Logger.Info("HousePreviewEntry " + i++);
                    Logger.Info("Households:" + housePreviewEntry.Households.Count + " Housetype:" +
                                housePreviewEntry.HouseType?.Name + " First Housesize:" + housePreviewEntry.HouseSize);
                    var j = 1;
                    foreach (var calcObject in housePreviewEntry.Households)
                    {
                        Logger.Info("#" + j++ + " Persons: " + calcObject.Household.CalculatePersonCount() + ", " +
                                    calcObject.Household.Name + ", " + calcObject.Household.EnergyIntensityType);
                            foreach (var trait in calcObject.Household.Traits)
                            {
                                var name = trait.HouseholdTrait.Name;
                                if (!traitCounts.ContainsKey(name))
                                {
                                    traitCounts.Add(name, 1);
                                }
                                else
                                {
                                    traitCounts[name] += 1;
                                }
                            }
                    }
                }
                st.CreateSettlementFromPreview(sim);
                SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                db.Cleanup();
                foreach (var pair in traitCounts)
                {
                    Logger.Info(pair.Key + ": " + pair.Value);
                }
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SettlementTemplateTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sts = new ObservableCollection<SettlementTemplate>();

                var affordances = db.LoadAffordances(out var timeBasedProfiles, out _, out var deviceCategories,
                    out var devices, out _, out var loadTypes, out var timeLimits, out var deviceActions,
                    out var deviceActionGroups, out var locations, out var variables, out var dateBasedProfiles);
                var affordanceTaggingSets = db.LoadAffordanceTaggingSets(affordances, loadTypes);
                var templates = db.LoadHouseholdTemplates(out var realDevices,
                    out _, out  _, out  _, out  _, out  _,
                    out  _, out var traits);
                var energyStorages = db.LoadEnergyStorages(loadTypes, variables);
                var transformationDevices = db.LoadTransformationDevices(loadTypes,
                    variables);
                var generators = db.LoadGenerators(loadTypes, dateBasedProfiles);
                var allLocations = db.LoadLocations(realDevices, deviceCategories, loadTypes);
                var houseTypes = db.LoadHouseTypes(realDevices, deviceCategories,
                    timeBasedProfiles, timeLimits, loadTypes, transformationDevices, energyStorages, generators,
                    allLocations, deviceActions, deviceActionGroups, variables);
                var tempprofiles = db.LoadTemperatureProfiles();
                var geolocs = db.LoadGeographicLocations(out _, timeLimits);
                var householdTags = db.LoadHouseholdTags();
                db.LoadTransportation(allLocations, out var transportationDeviceSets,
                    out var travelRouteSets, out _,out _,
                    loadTypes,out var chargingStationSets, affordanceTaggingSets);
                db.ClearTable(SettlementTemplate.TableName);
                db.ClearTable(STHouseholdDistribution.TableName);
                db.ClearTable(STHouseholdTemplate.TableName);
                db.ClearTable(STHouseSize.TableName);
                db.ClearTable(STHouseType.TableName);
                db.ClearTable(STTraitLimit.TableName);
                var st = new SettlementTemplate("bla", null, "desc", db.ConnectionString, 100, "TestName",
                    null, null, Guid.NewGuid().ToStrGuid());
                st.SaveToDB();
                st.IntID.Should().NotBe(-1);
                st.AddHouseholdDistribution(10, 100, 0.2, EnergyIntensityType.AsOriginal);
                st.AddHouseholdTemplate(templates[0]);
                st.AddHouseSize(10, 100, 0.2);
                st.AddHouseType(houseTypes[0]);
                st.AddTraitLimit(traits[0], 10);
                SettlementTemplate.LoadFromDatabase(sts, db.ConnectionString, templates, houseTypes, false, tempprofiles,
                    geolocs, householdTags, traits,chargingStationSets,travelRouteSets,transportationDeviceSets);
                (sts.Count).Should().Be(1);
                (sts[0].HouseholdDistributions.Count).Should().Be(1);
                (sts[0].HouseholdTemplates.Count).Should().Be(1);

                (sts[0].HouseTypes.Count).Should().Be(1);
                (sts[0].HouseSizes.Count).Should().Be(1);
                st = sts[0];
                st.DeleteHouseholdDistribution(st.HouseholdDistributions[0]);
                st.DeleteHouseholdTemplate(st.HouseholdTemplates[0]);
                st.DeleteHouseType(st.HouseTypes[0]);
                st.DeleteHouseSize(st.HouseSizes[0]);
                st.DeleteTraitLimit(st.TraitLimits[0]);
                sts.Clear();
                SettlementTemplate.LoadFromDatabase(sts, db.ConnectionString,
                    templates, houseTypes, false, tempprofiles,
                    geolocs, householdTags, traits,
                    chargingStationSets,travelRouteSets,transportationDeviceSets);
                (sts.Count).Should().Be(1);
                (sts[0].HouseholdDistributions.Count).Should().Be(0);
                (sts[0].HouseholdTemplates.Count).Should().Be(0);

                (sts[0].HouseTypes.Count).Should().Be(0);
                (sts[0].HouseSizes.Count).Should().Be(0);
                db.Cleanup();
            }
        }

        public SettlementTemplateTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}