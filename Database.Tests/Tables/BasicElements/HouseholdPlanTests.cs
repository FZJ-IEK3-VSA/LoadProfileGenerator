using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using NUnit.Framework;

namespace Database.Tests.Tables.BasicElements {
    [TestFixture]
    public class HouseholdPlanTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.LongTest5)]
        public void HouseholdPlanEntryAverageTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                foreach (var dc in sim.DeviceCategories.MyItems)
                {
                    dc.RefreshSubDevices();
                }
                foreach (var hhp in sim.HouseholdPlans.MyItems)
                {
                    hhp.Refresh(null);
                    Logger.Info("---------------");
                    Logger.Info(hhp.Name);

                    foreach (var entry in hhp.Entries)
                    {
                        Logger.Info(entry.Name);
                        Logger.Info("time :" + entry.AverageTimePerActivation);
                    }
                }
                db.Cleanup();
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test]
        [Category(UnitTestCategories.LongTest5)]
        public void HouseholdPlanEntryRefreshTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseholdPlan.TableName);
                db.ClearTable(DeviceTag.TableName);
                var timeBasedProfiles = db.LoadTimeBasedProfiles();
                var desires = db.LoadDesires();
                var persons = db.LoadPersons();

                var realDevices =
                    db.LoadRealDevices(out var deviceCategories, out _, out var loadTypes, timeBasedProfiles);

                var locations = db.LoadLocations(realDevices, deviceCategories, loadTypes);
                var dateBasedProfiles = db.LoadDateBasedProfiles();
                var timeLimits = db.LoadTimeLimits(dateBasedProfiles);
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions =
                    db.LoadDeviceActions(timeBasedProfiles, realDevices, loadTypes, deviceActionGroups);
                var variables = db.LoadVariables();
                var affordances = db.LoadAffordances(timeBasedProfiles, out _,
                    deviceCategories, realDevices, desires, loadTypes, timeLimits, deviceActions, deviceActionGroups,
                    locations, variables);
                var sets = db.LoadAffordanceTaggingSets(affordances, loadTypes);
                var traitTags = db.LoadTraitTags();
                var traits = db.LoadHouseholdTraits(locations, affordances, realDevices,
                    deviceCategories, timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, deviceActionGroups,
                    traitTags, variables);
                var selections = db.LoadDeviceSelections(deviceCategories, realDevices,
                    deviceActions, deviceActionGroups);
                var vacations = db.LoadVacations();
                var hhTags = db.LoadHouseholdTags();
                var modularHouseholds = db.LoadModularHouseholds(traits, selections,
                    persons, vacations, hhTags, traitTags);
                var plans = new ObservableCollection<HouseholdPlan>();
                HouseholdPlan.LoadFromDatabase(plans, db.ConnectionString, false, persons, sets,
                    modularHouseholds);
                var hh = modularHouseholds[0];
                var set = sets[0];
                var hhp = new HouseholdPlan("test", set, hh, "blub", db.ConnectionString, System.Guid.NewGuid().ToStrGuid());
                hhp.SaveToDB();
                hhp.Refresh(null);
                hhp.SaveToDB();
                plans.Clear();
                HouseholdPlan.LoadFromDatabase(plans, db.ConnectionString, false, persons, sets, modularHouseholds);
                hhp = plans[0];
                foreach (var entry in hhp.Entries)
                {
                    Logger.Info(entry.Name);
                }
                Assert.AreEqual(hh.Persons.Count * set.Tags.Count, hhp.Entries.Count);
                db.Cleanup();
            }
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseholdPlanEntryTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseholdPlan.TableName);
                db.ClearTable(HouseholdPlanEntry.TableName);
                var timeBasedProfiles = db.LoadTimeBasedProfiles();
                var desires = db.LoadDesires();
                var persons = db.LoadPersons();

                var realDevices =
                    db.LoadRealDevices(out var deviceCategories, out _, out var loadTypes, timeBasedProfiles);
                var locations = db.LoadLocations(realDevices, deviceCategories, loadTypes);
                var dateBasedProfiles = db.LoadDateBasedProfiles();
                var timeLimits = db.LoadTimeLimits(dateBasedProfiles);

                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions =
                    db.LoadDeviceActions(timeBasedProfiles, realDevices, loadTypes, deviceActionGroups);
                var variables = db.LoadVariables();
                var affordances = db.LoadAffordances(timeBasedProfiles, out _,
                    deviceCategories, realDevices, desires, loadTypes,
                    timeLimits, deviceActions, deviceActionGroups, locations, variables);

                var sets = db.LoadAffordanceTaggingSets(affordances, loadTypes);
                var tags = db.LoadTraitTags();
                var traits = db.LoadHouseholdTraits(locations, affordances, realDevices,
                    deviceCategories, timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, deviceActionGroups,
                    tags, variables);
                var selections = db.LoadDeviceSelections(deviceCategories, realDevices,
                    deviceActions, deviceActionGroups);
                var vacations = db.LoadVacations();
                var hhTags = db.LoadHouseholdTags();
                var modularHouseholds = db.LoadModularHouseholds(traits, selections,
                    persons, vacations, hhTags, tags);
                var plans = new ObservableCollection<HouseholdPlan>();
                HouseholdPlan.LoadFromDatabase(plans, db.ConnectionString, false, persons, sets,
                    modularHouseholds);
                var hh = modularHouseholds[0];
                var set = sets[0];
                var hhp = new HouseholdPlan("test", set, hh, "blub", db.ConnectionString, System.Guid.NewGuid().ToStrGuid());
                hhp.SaveToDB();
                hhp.AddNewEntry(hh.Persons[0].Person, set.Tags[0], 1, 1, TimeType.Day);
                hhp.SaveToDB();
                plans.Clear();
                HouseholdPlan.LoadFromDatabase(plans, db.ConnectionString, false, persons, sets,
                    modularHouseholds);
                hhp = plans[0];
                Assert.AreEqual(hhp.CalcObject, hh);
                Assert.AreEqual(hhp.AffordanceTaggingSet, set);
                Assert.AreEqual(1, hhp.Entries.Count);
                hhp.DeleteEntry(hhp.Entries[0]);
                hhp.DeleteFromDB();
                plans.Clear();
                HouseholdPlan.LoadFromDatabase(plans, db.ConnectionString, false, persons, sets,
                    modularHouseholds);
                Assert.AreEqual(0, plans.Count);
                db.Cleanup();
            }
        }
    }
}