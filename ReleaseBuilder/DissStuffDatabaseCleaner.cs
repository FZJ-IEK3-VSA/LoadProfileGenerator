using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ReleaseBuilder {
    public class DissStuffDatabaseCleaner {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static void Run([NotNull] string filePath)
        {
            var start = DateTime.Now;
            var connectionString = "Data Source=" + filePath;
            var sim = new Simulator(connectionString);
            Config.ShowDeleteMessages = false;
            var i = 0;
            const int interval = 100;
            var comHHToDelete = sim.ModularHouseholds.It.Where(x => x.Name.StartsWith("x ", StringComparison.Ordinal)).ToList();
            Logger.Info("Modular households to be deleted: " + comHHToDelete.Count);

            foreach (var modularHousehold in comHHToDelete) {
                sim.ModularHouseholds.DeleteItem(modularHousehold);
                i++;
                if (i % interval == 0) {
                    Console.WriteLine(i);
                }
            }

            i = 0;
            var houseToDelete = sim.Houses.It.Where(x => x.Name.StartsWith("(", StringComparison.Ordinal) || x.Name.Contains("Diss")).ToList();
            Logger.Info("houses to be deleted: " + houseToDelete.Count);
            foreach (var house in houseToDelete) {
                sim.Houses.DeleteItem(house);
                i++;
                if (i % interval == 0) {
                    Console.WriteLine(i);
                }
            }

            i = 0;
            var vacationsToDelete = sim.Vacations.It.Where(x => x.Name.StartsWith("Diss", StringComparison.Ordinal)).ToList();
            Logger.Info("vacations to be deleted: " + vacationsToDelete.Count);
            foreach (var vaca in vacationsToDelete) {
                sim.Vacations.DeleteItem(vaca);
                i++;
                if (i % interval == 0) {
                    Console.WriteLine(i);
                }
            }

            i = 0;
            var settlementsToDelete = sim.Settlements.It.Where(x => x.Name.StartsWith("Dis", StringComparison.Ordinal)).ToList();
            var settlementsToDelete2 = sim.Settlements.It.Where(x => x.HouseholdCount == 0).ToList();
            settlementsToDelete.AddRange(settlementsToDelete2);
            Logger.Info("settlements to be deleted: " + settlementsToDelete.Count);
            foreach (var vaca in settlementsToDelete) {
                sim.Settlements.DeleteItem(vaca);
                i++;
                if (i % interval == 0) {
                    Console.WriteLine(i);
                }
            }

            i = 0;
            var settlementsTempsToDelete = sim.SettlementTemplates.It.Where(x => x.Name.StartsWith("Diss", StringComparison.Ordinal)).ToList();
            Logger.Info("settlement templates to delete:" + settlementsTempsToDelete.Count);
            foreach (var vaca in settlementsTempsToDelete) {
                sim.SettlementTemplates.DeleteItem(vaca);
                i++;
                if (i % interval == 0) {
                    Console.WriteLine(i);
                }
            }

            i = 0;
            var datebased = sim.DateBasedProfiles.It.Where(x =>
                x.Name.StartsWith("Chemnitz, Germany, Total Solar Radition 2013 15 min resolution", StringComparison.Ordinal)).ToList();
            Logger.Info("date based to delete:" + datebased.Count);
            var radiation = sim.DateBasedProfiles.FindByName("Chemnitz, Germany, Total Solar Radiation");
            if (datebased.Count > 0) {
                var dbp = datebased[0];
                foreach (var vaca in datebased) {
                    sim.DateBasedProfiles.DeleteItem(vaca);
                    i++;
                    if (i % interval == 0) {
                        Logger.Info(i.ToString(CultureInfo.CurrentCulture));
                    }
                }

                var limitsToFix = new List<TimeLimit> {
                    sim.TimeLimits.FindByName("Below 50W solar radation")
                };
                foreach (var timeLimit in limitsToFix) {
                    foreach (var boolEntry in timeLimit.TimeLimitEntries) {
                        if (boolEntry.RepeaterType == PermissionMode.ControlledByDateProfile &&
                            (boolEntry.DateBasedProfile == null || boolEntry.DateBasedProfile == dbp)) {
                            boolEntry.DateBasedProfile = radiation;
                            boolEntry.SaveToDB();
                        }
                    }

                    timeLimit.SaveToDB();
                }
            }

            var gen = sim.Generators.FindByName("Photovoltaic System 50m2");
            if (gen == null) {
                throw new LPGException("generator was null");
            }

            gen.DateBasedProfile = radiation;
            gen.SaveToDB();
            var end = DateTime.Now;
            Logger.Info("Duration:" + (end - start));

            using (var con = new Connection(connectionString)) {
                using (var cmd = new Command(con)) {
                    con.Open();
                    cmd.ExecuteNonQuery("vacuum;");
                }
            }

            Config.ShowDeleteMessages = true;
            var sim2 = new Simulator(connectionString);
            if (sim2.Affordances.AllAffordanceCategories.Count > 0) {
                Logger.Info("sim2.Affordances.AllAffordanceCategories.Count > 0");
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Test]
        [Category("QuickChart")]
        public void RunTest()
        {
            const string srcpath = @"e:\profilegenerator.db3";
            const string tmpPath = @"e:\temp\profGen.db3";
            File.Copy(srcpath, tmpPath, true);
            Run(tmpPath);
        }
    }
}