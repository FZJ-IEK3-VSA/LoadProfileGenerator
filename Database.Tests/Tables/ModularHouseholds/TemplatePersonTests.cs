using System.Collections.ObjectModel;
using Common;
using Common.Tests;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using NUnit.Framework;

namespace Database.Tests.Tables.ModularHouseholds {
    [TestFixture]
    public class TemplatePersonTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void TemplatePersonTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            db.ClearTable(TemplatePerson.TableName);
            db.ClearTable(TemplatePersonTrait.TableName);
            var cat = new CategoryDBBase<TemplatePerson>("TemplatePersons");
            var timeBasedProfiles = db.LoadTimeBasedProfiles();
            var devices = db.LoadRealDevices(out ObservableCollection<DeviceCategory> deviceCategories, out ObservableCollection<VLoadType> loadTypes,
                timeBasedProfiles);
            var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
            var desires = db.LoadDesires();
            var dateBasedProfiles = db.LoadDateBasedProfiles();
            var timeLimits = db.LoadTimeLimits(dateBasedProfiles);
            var deviceActionGroups = db.LoadDeviceActionGroups();
            var deviceActions = db.LoadDeviceActions(timeBasedProfiles, devices,
                loadTypes, deviceActionGroups);
            var variables = db.LoadVariables();
            var affordances = db.LoadAffordances(timeBasedProfiles, out _,
                deviceCategories, devices, desires, loadTypes, timeLimits, deviceActions, deviceActionGroups, locations,
                variables);
            var traitTags = db.LoadTraitTags();
            var traits = db.LoadHouseholdTraits(locations, affordances, devices,
                deviceCategories, timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, deviceActionGroups,
                traitTags, variables);
            var selections = db.LoadDeviceSelections(deviceCategories, devices,
                deviceActions, deviceActionGroups);
            var persons = db.LoadPersons();
            var vacations = db.LoadVacations();
            var tags = db.LoadHouseholdTags();
            var chhs = db.LoadModularHouseholds(traits, selections, persons,vacations, tags,traitTags);

            TemplatePerson.LoadFromDatabase(cat.It, db.ConnectionString, traits, false, chhs, persons);
            Assert.AreEqual(0, cat.MyItems.Count);
            cat.CreateNewItem(db.ConnectionString);
            cat.SaveToDB();
            var templatePerson = new ObservableCollection<TemplatePerson>();
            TemplatePerson.LoadFromDatabase(templatePerson, db.ConnectionString, traits, false, chhs, persons);
            Assert.AreEqual(1, templatePerson.Count);
            db.Cleanup();
        }
    }
}