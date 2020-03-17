using System;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Common;
using Common.Tests;
using Database;
using Database.Database;
using Database.Tables;
using Database.Tests;
using NUnit.Framework;
using Logger = Common.Logger;

namespace IntegrationTests
{
    [TestFixture]
    public class CleanEachTable
    {
        [Test]
        [Category(UnitTestCategories.CleanEachTable)]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void TryCleaningEachTable()
        {
            CleanTestBase.RunAutomatically(false);
            var db1 = new DatabaseSetup("TryCleaningEach", DatabaseSetup.TestPackage.LongTermMerger);
            var alltables = LongtermTests.GetTableList(db1);
            db1.Cleanup();
            for (var index = 0; index < alltables.Count; index++)
            {
                Logger.Info("processing table " + index + " out of " + alltables.Count);
                var table = alltables[index];
                if (table == "tblLPGVersion") {
                    continue;
                }
                var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.LongTermMerger);
                DBBase.HideDeleteMessages();
                Command.HideDeleteMessages();

                db.ClearTable(table);
                var oldSim = new Simulator(db.ConnectionString); // need to load it for testing
                Console.WriteLine(oldSim.ModularHouseholds.It.Count);
                db.Cleanup();
            }
            CleanTestBase.RunAutomatically(true);
        }
    }
}
