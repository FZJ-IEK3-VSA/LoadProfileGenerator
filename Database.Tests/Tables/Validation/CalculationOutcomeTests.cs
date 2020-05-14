using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.Validation;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Tables.Validation {
    [TestFixture]
    public class CalculationOutcomeTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.LongTest1)]
        public void CalculationOutcomeTest()
        {
            Config.ShowDeleteMessages = false;
            Logger.Threshold = Severity.Error;
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(CalculationOutcome.TableName);
                db.ClearTable(LoadtypeOutcome.TableName);
                var ca = new CalculationOutcome("1", "2", "3", "4", "5", "6", "7", "8", TimeSpan.FromHours(1),
                    DateTime.Today, DateTime.Today,
                    db.ConnectionString, 0, 0, null, Guid.NewGuid().ToStrGuid());
                ca.SaveToDB();
                ca.AddLoadType("bla", 10);
                ca.AddAffordanceTimeUse("bla", "blub", 1, 1);
                var cas = new ObservableCollection<CalculationOutcome>();
                CalculationOutcome.LoadFromDatabase(cas, db.ConnectionString, false);
                Assert.AreEqual(1, cas.Count);
                Assert.AreEqual(1, cas[0].Entries.Count);
                Assert.AreEqual(1, cas[0].AffordanceTimeUses.Count);
                db.Cleanup();
            }
        }

        [Fact]
        [Category(UnitTestCategories.LongTest1)]
        public void CalculationOutcomeTest2()
        {
            Config.ShowDeleteMessages = false;
            Logger.Threshold = Severity.Error;

            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Logger.Threshold = Severity.Warning;
                db.ClearTable(CalculationOutcome.TableName);
                db.ClearTable(LoadtypeOutcome.TableName);
                var sim = new Simulator(db.ConnectionString);

                var co = sim.CalculationOutcomes.CreateNewItem(sim.ConnectionString);
                co.AddLoadType("lt", 10);
                co.SaveToDB();

                var sim2 = new Simulator(db.ConnectionString);

                Assert.AreEqual(1, sim2.CalculationOutcomes.It.Count);
                Assert.AreEqual(1, sim2.CalculationOutcomes[0].Entries.Count);

                db.Cleanup();
            }
        }

        public CalculationOutcomeTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}