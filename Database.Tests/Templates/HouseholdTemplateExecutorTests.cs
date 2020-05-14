using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using Database.Templating;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Templates {
    [TestFixture]
    public class HouseholdTemplateExecutorTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunMakeProbabilityArrayTest() {
            //tests the make probability array function
            // the function finds the probabiltiies in a date profile, normalizes them and then generates a sorted 1000 member array to pick the proper
            // probability
            var dataPoints = new ObservableCollection<DateProfileDataPoint>
            {
                new DateProfileDataPoint(new DateTime(2017, 01, 01), 0.3, null, -1, "", Guid.NewGuid().ToStrGuid()),
                new DateProfileDataPoint(new DateTime(2017, 01, 02), 0.2, null, -1, "", Guid.NewGuid().ToStrGuid()),
                new DateProfileDataPoint(new DateTime(2017, 01, 03), 0.5, null, -1, "", Guid.NewGuid().ToStrGuid()),
                new DateProfileDataPoint(new DateTime(2017, 01, 03), 0.3, null, -1, "", Guid.NewGuid().ToStrGuid())
            };
            var result = HouseholdTemplateExecutor.VacationMakeProbabilityArray(dataPoints);
            var counts = new Dictionary<double, int>();
            for (var i = 0; i < result.Count; i++) {
                if (!counts.ContainsKey(result[i])) {
                    counts.Add(result[i], 0);
                }
                counts[result[i]]++;
            }
            foreach (var pair in counts) {
                Logger.Info(pair.Key + ":" + pair.Value);
            }
            for (var i = 0; i < result.Count; i++) {
                Logger.Info(result[i].ToString(CultureInfo.InvariantCulture));
            }
            Assert.AreEqual(counts[0.5], 500);
            Assert.AreEqual(counts[0.2], 200);
            Assert.AreEqual(counts[0.3], 300);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunVacationGetProbabilityRangesTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var dbp = sim.DateBasedProfiles.CreateNewItem(sim.ConnectionString);
                var year = DateTime.Now.Year;
                dbp.AddNewDatePoint(new DateTime(year, 1, 1), 0.3);
                dbp.AddNewDatePoint(new DateTime(year, 3, 1), 0.7);
                dbp.AddNewDatePoint(new DateTime(year, 3, 2), 0.6);
                dbp.AddNewDatePoint(new DateTime(year, 8, 1), 0);

                var probabilityarr = dbp.GetValueArray(new DateTime(year, 1, 1), new DateTime(year + 1, 1, 1),
                    new TimeSpan(1, 0, 0, 0));
                var result = HouseholdTemplateExecutor.VacationGetProbabilityRanges(probabilityarr, year);
                foreach (var range in result)
                {
                    Logger.Info("Range: " + range.Start + " - " + range.End + " : " + range.Probability);
                }
                Assert.AreEqual(dbp.Datapoints.Count - 1, result.Count);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunVacGenerationTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var dbp = sim.DateBasedProfiles.CreateNewItem(sim.ConnectionString);

                dbp.AddNewDatePoint(new DateTime(2017, 1, 1), 0.3);
                dbp.AddNewDatePoint(new DateTime(2017, 3, 1), 0.7);
                dbp.AddNewDatePoint(new DateTime(2017, 6, 1), 0);
                var ht = sim.HouseholdTemplates.CreateNewItem(sim.ConnectionString);
                ht.MinTotalVacationDays = 30;
                ht.MaxTotalVacationDays = 40;
                ht.MinNumberOfVacations = 1;
                ht.MaxNumberOfVacations = 5;
                ht.AverageVacationDuration = 7;
                ht.TemplateVacationType = TemplateVacationType.RandomlyGenerated;
                ht.TimeProfileForVacations = dbp;
                var r = new Random();
                var vac = HouseholdTemplateExecutor.GenerateVacation(ht, r, sim, "VacTest");
                var vacdays = vac.DurationInH / 24;
                Logger.Info("Total days taken: " + vacdays);
                foreach (var vacVacationTime in vac.VacationTimes)
                {
                    Logger.Info(vacVacationTime.ToString());
                }
                db.Cleanup();
            }
        }

        public HouseholdTemplateExecutorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}