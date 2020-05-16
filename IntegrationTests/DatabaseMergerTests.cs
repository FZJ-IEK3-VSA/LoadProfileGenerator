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
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace IntegrationTests {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class DatabaseMergerTests : UnitTestBaseClass
    {
        private static void TestImport([JetBrains.Annotations.NotNull] string path) {
            TestImport(path, out _, out _);
        }

        private static void TestImport([JetBrains.Annotations.NotNull] string path, [JetBrains.Annotations.NotNull] out Simulator mainsim) {
            TestImport(path, out mainsim, out _);
        }

        private static void TestImport([JetBrains.Annotations.NotNull]string path, [JetBrains.Annotations.NotNull]out Simulator mainsim,
            [JetBrains.Annotations.NotNull]out Database.DatabaseMerger.DatabaseMerger dbm)
        {
            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), path));
            Logger.Debug(di.FullName);
            var fi = FindImportFiles(path);
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                var newpath = Path.Combine(wd.WorkingDirectory, "mergertest.db3");
                File.Copy(fi.FullName, newpath);
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var mainSim = new Simulator(db.ConnectionString);
                    dbm = new Database.DatabaseMerger.DatabaseMerger(mainSim);

                    dbm.RunFindItems(newpath, null);
                    dbm.RunImport(null);
                    mainsim = mainSim;
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        [JetBrains.Annotations.NotNull]
        private static FileInfo FindImportFiles([JetBrains.Annotations.NotNull] string path)
        {
            const string teamcityrelativePath = @"..\..\..\Importfiles\";
            FileInfo fi = new FileInfo(Path.Combine(teamcityrelativePath, path));
            if (fi.Exists) {
                return fi;
            }
            Logger.Info("file not found: " + fi.FullName + ", trying jenkins path next");
            const string jenkinsrelativePath = "Importfiles\\";
            fi = new FileInfo(Path.Combine(jenkinsrelativePath, path));
            if (!fi.Exists) {
            }
            const string dropboxpath = @"v:\dropbox\lpg\importfiles\";
            fi = new FileInfo(Path.Combine(dropboxpath, path));
            if (!fi.Exists) {
                return fi;
            }
            Logger.Info("file not found: " + fi.FullName + ", trying jenkins path next");
            throw new LPGException("Missing file: " + fi.FullName + "\n Current Directory:" + Directory.GetCurrentDirectory());
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void TestImportWithHouseholdTemplateDelete600()
        {
            const string srcFileName = "profilegenerator600.db3";
            string sourcepath = DatabaseSetup.GetImportFileFullPath(srcFileName);
            if (!File.Exists(sourcepath))
            {
                throw new LPGException("Missing file!");
            }

            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                var newpath = Path.Combine(wd.WorkingDirectory, "mergertest.db3");
                File.Copy(sourcepath, newpath);
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var mainSim = new Simulator(db.ConnectionString);
                    db.ClearTable(HouseholdTemplate.TableName);
                    Database.DatabaseMerger.DatabaseMerger dbm = new Database.DatabaseMerger.DatabaseMerger(mainSim);

                    dbm.RunFindItems(newpath, null);
                    dbm.RunImport(null);
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void TestImportWithHouseholdTemplateDelete880()
        {
            const string srcFileName = "profilegenerator880.db3";
            string sourcepath = DatabaseSetup.GetImportFileFullPath(srcFileName);
            if (!File.Exists(sourcepath))
            {
                throw new LPGException("Missing file!");
            }

            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                var newpath = Path.Combine(wd.WorkingDirectory, "mergertest.db3");
                File.Copy(sourcepath, newpath);
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var mainSim = new Simulator(db.ConnectionString);
                    db.ClearTable(HouseholdTemplate.TableName);
                    Database.DatabaseMerger.DatabaseMerger dbm = new Database.DatabaseMerger.DatabaseMerger(mainSim);

                    dbm.RunFindItems(newpath, null);
                    dbm.RunImport(null);
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunTest124() => TestImport("profilegenerator124.db3");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunTest160() => TestImport("profilegenerator160.db3");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunTest170() => TestImport("profilegenerator170.db3");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunTest171() {
            TestImport("profilegenerator171.db3", out var sim);
            var found = false;
            foreach (var householdTrait in sim.HouseholdTraits.MyItems) {
                if (householdTrait.Name == "import test") {
                    found = true;
                }
            }
            found.Should().BeTrue();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunTest201() => TestImport("profilegenerator201.db3");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunTest203() => TestImport("profilegenerator203.db3");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunTest210() => TestImport("profilegenerator210.db3");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunTest280Sarah() => TestImport("profilegenerator280SARAH.db3");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunTest520() => TestImport("profilegenerator520.db3");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunTestCurrentDeviceActions()
        {
            using (var dbOriginal = new DatabaseSetup("RunTestCurrentDeviceActionsOriginal"))
            {
                var originalSim = new Simulator(dbOriginal.ConnectionString);

                const string path = "profilegeneratorcopy.db3";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                File.Copy(DatabaseSetup.GetSourcepath(null), path);
                if (!File.Exists(path))
                {
                    throw new LPGException("Missing file!");
                }
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    db.ClearTable(DeviceAction.TableName);
                    db.ClearTable(DeviceActionGroup.TableName);
                    db.ClearTable(DeviceActionProfile.TableName);
                    var mainSim = new Simulator(db.ConnectionString);
                    var dbm = new Database.DatabaseMerger.DatabaseMerger(mainSim);
                    dbm.RunFindItems(path, null);

                    foreach (var dbBase in dbm.ItemsToImport)
                    {
                        Logger.Error(dbBase.Entry.Name + " " + dbBase.Import);
                        dbBase.Import = true;
                    }
                    dbm.RunImport(null);
                    var newActions = mainSim.DeviceActions.It;
                    var nullOldcount = 0;
                    foreach (var oldAction in originalSim.DeviceActions.It)
                    {
                        if (oldAction.DeviceActionGroup == null)
                        {
                            nullOldcount++;
                        }
                        var newAction = newActions.First(x => x.Name == oldAction.Name);
                        if (oldAction.DeviceActionGroup != null) {
                            oldAction.DeviceActionGroup.Name.Should().Be(newAction.DeviceActionGroup?.Name);
                        }
                    }
                    Logger.Info("oldAction total:" + originalSim.DeviceActions.It.Count + " null:" + nullOldcount);
                    dbOriginal.Cleanup();
                    db.Cleanup();
                }
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunTestCurrentDeviceCategory()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                const string path = "profilegeneratorcopy.db3";

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                string sourcefile = DatabaseSetup.GetSourcepath(null);
                File.Copy(sourcefile, path);
                if (!File.Exists(path))
                {
                    throw new LPGException("Missing file!");
                }
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    db.ClearTable(DeviceCategory.TableName);
                    db.ClearTable(AffordanceDevice.TableName);
                    var mainSim = new Simulator(db.ConnectionString);
                    var dbm = new Database.DatabaseMerger.DatabaseMerger(mainSim);
                    dbm.RunFindItems(path, null);

                    foreach (var dbBase in dbm.ItemsToImport)
                    {
                        Logger.Error(dbBase.Entry.Name + " " + dbBase.Import);
                        dbBase.Import = true;
                    }
                    dbm.RunImport(null);
                    var newCategories = mainSim.DeviceCategories.CollectAllDBBaseItems();
                    var oldCategories = dbm.OldSimulator.DeviceCategories.CollectAllDBBaseItems();
                    var newcats = new Dictionary<string, DeviceCategory>();
                    foreach (var newCategory in newCategories)
                    {
                        var cat = (DeviceCategory)newCategory;
                        newcats.Add(cat.ShortName, cat);
                    }
                    foreach (var oldCategory in oldCategories)
                    {
                        Logger.Debug("checking: " + oldCategory.Name);
                        var oldCat = (DeviceCategory)oldCategory;
                        newcats.ContainsKey(oldCat.ShortName).Should().BeTrue();
                        var newcat = newcats[oldCat.ShortName];
                        newcat.FullPath.Should().Be(oldCat.FullPath);
                        newcat.ParentCategory?.Name.Should().Be( oldCat.ParentCategory?.Name);
                    }
                    db.Cleanup();
                }
                wd.CleanUp(throwAllErrors: false);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunTestCurrentTimeLimits()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                var path = Path.Combine(wd.WorkingDirectory, "profilegeneratorcopy.db3");
                var sourcepath = DatabaseSetup.GetSourcepath(null);

                File.Copy(sourcepath, path);
                if (!File.Exists(path))
                {
                    throw new LPGException("Missing file!");
                }
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var mainSim = new Simulator(db.ConnectionString);
                    mainSim.TimeLimits.DeleteItem(mainSim.TimeLimits[0]);
                    mainSim.TimeLimits.DeleteItem(mainSim.TimeLimits[1]);
                    var dbm = new Database.DatabaseMerger.DatabaseMerger(mainSim);
                    dbm.RunFindItems(path, null);
                    Logger.Debug("importing:");
                    foreach (var dbBase in dbm.ItemsToImport)
                    {
                        Logger.Debug("importing:" + Environment.NewLine + dbBase.Entry.Name + " " + dbBase.Import);
                        dbBase.Import = true;
                    }
                    dbm.RunImport(null);
                    var allTimeLimits = new Dictionary<string, bool>();
                    foreach (var timeLimit in mainSim.TimeLimits.MyItems)
                    {
                        allTimeLimits.Add(timeLimit.CombineCompleteString(), true);
                    }
                    foreach (var timeLimit in dbm.OldSimulator.TimeLimits.MyItems) {
                        allTimeLimits.ContainsKey(timeLimit.CombineCompleteString()).Should().BeTrue();
                    }
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        public DatabaseMergerTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}