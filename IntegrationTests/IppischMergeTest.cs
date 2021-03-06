﻿using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database;
using Database.DatabaseMerger;
using Database.Tests;

using Xunit;

namespace IntegrationTests {
    public class IppischMergeTest {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void TestImport()
        {
            const string importPath = @"e:\Haushalt3.db3";

            var fi = new FileInfo(importPath);
            if (!File.Exists(importPath))
            {
                throw new LPGException("Missing file: " + fi.FullName);
            }
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var mainSim = new Simulator(db.ConnectionString);
                var hhNames = mainSim.ModularHouseholds.Items.Select(x => x.Name).ToList();
                var dbm = new DatabaseMerger(mainSim);

                dbm.RunFindItems(importPath, null);
                dbm.RunImport(null);
                var newHHs = mainSim.ModularHouseholds.Items.Where(x => !hhNames.Contains(x.Name)).ToList();
                foreach (var newHH in newHHs)
                {
                    if (newHH.Persons.Count == 0)
                    {
                        throw new LPGException("No persons were imported.");
                    }
                    Logger.Info(newHH.Name);
                    foreach (var hhPerson in newHH.Persons)
                    {
                        Logger.Info("\t" + hhPerson.Name);
                    }
                }
                db.Cleanup();
            }
            CleanTestBase.RunAutomatically(true);
        }
    }
}