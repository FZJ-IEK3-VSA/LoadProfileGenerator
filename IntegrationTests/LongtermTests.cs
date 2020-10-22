using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database;
using Database.Database;
using Database.DatabaseMerger;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace IntegrationTests {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class Comparer {
        /*private static bool CompareObjects(this object obj, [CanBeNull] object another, int level, string path,
            out bool doPropertyComparison)
        {
            if (obj is int) {
                doPropertyComparison = false;
                if (obj.Equals(another)) {
                    return true;
                }
                Logger.Info(path + ": The int values were different.");
                return false;
            }


            if (obj is System.Collections.IEnumerable) {
                var enumerA = (System.Collections.IEnumerable) obj;
                var enumerB = (System.Collections.IEnumerable)obj;
                enumerA.Zip
            }

            if (obj.GetType().Name.StartsWith("ObservableCollection")) {

                var types = obj.GetType().GetInterfaces()
                    .Where(x => x.IsGenericType
                                && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .ToArray();
                // Only support collections that implement IEnumerable<T> once.
                var result = types.Length == 1 ? types[0].GetGenericArguments()[0] : null;

                Type innertype = obj.GetType().GenericTypeArguments[0];
                var mycol = obj.
            }
            doPropertyComparison = true;
            return true;
        }*/
        /*
        public static bool DeepCompare(this object obj, [CanBeNull] object another, int level, string path)
        {
            if (ReferenceEquals(obj, another)) {
                return true;
            }
            if (obj == null || another == null) {
                Logger.Info("Both objects were null");
                return false;
            }
            //Compare two object's class, return false if they are difference
            if (obj.GetType() != another.GetType()) {
                Logger.Info("The objects were of different type");
                return false;
            }
            Logger.Info("Comparing " + path);
            bool doPropertyComparison;
            var isEqual = CompareObjects(obj, another, level, path, out doPropertyComparison);
            if (doPropertyComparison) {
                var result = true;
                path += obj.GetType().Name + "/";
                foreach (var property in obj.GetType().GetProperties()) {
                    var objValue = property.GetValue(obj);
                    var anotherValue = property.GetValue(another);
                    if (!objValue.DeepCompare(anotherValue, level + 1, path)) {
                        Logger.Info("The objects did not match");
                        result = false;
                    }
                }
                return result;
            }
            return isEqual;
        }
        */
        /*
        public static bool CompareEx(this object obj, object another)
        {
            if (ReferenceEquals(obj, another)) {
                return true;
            }
            if ((obj == null) || (another == null)) {
                return false;
            }
            if (obj.GetType() != another.GetType()) {
                return false;
            }

            //properties: int, double, DateTime, etc, not class
            if (!obj.GetType().IsClass) {
                return obj.Equals(another);
            }

            var result = true;
            foreach (var property in obj.GetType().GetProperties())
            {
                var objValue = property.GetValue(obj);
                var anotherValue = property.GetValue(another);
                //Recursion
                if (!objValue.DeepCompare(anotherValue,)) {
                    result = false;
                }
            }
            return result;
        }*/
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class LongtermTests : UnitTestBaseClass
    {
        private static void ClearAllTables([JetBrains.Annotations.NotNull] DatabaseSetup db)
        {
            using (var con = new Connection(db.ConnectionString)) {
                con.Open();
                var tables = new List<string>();
                using (var cmd = new Command(con)) {
                    var dr = cmd.ExecuteReader("SELECT * from SQLITE_Master WHERE type = 'table'");
                    while (dr.Read()) {
                        var s = dr.GetString("name").ToLowerInvariant();
                        if (s.StartsWith("tbl", StringComparison.Ordinal)) {
                            if (s.Equals("tbllpgversion", StringComparison.Ordinal)) {
                                continue;
                            }
                            tables.Add(s);
                            Logger.Info("Clearing Table " + s);
                        }
                    }
                    dr.Dispose();
                    foreach (var table in tables) {
                        cmd.ExecuteNonQuery("DELETE FROM " + table);
                    }
                }
            }
        }

        private static void DeleteOneElementFromAllTables([JetBrains.Annotations.NotNull] DatabaseSetup db)
        {
            using (var con = new Connection(db.ConnectionString))
            {
                con.Open();
                var tables = new List<string>();
                using (var cmd = new Command(con))
                {
                    var dr = cmd.ExecuteReader("SELECT * from SQLITE_Master WHERE type = 'table'");
                    while (dr.Read())
                    {
                        var s = dr.GetString("name");
                        if (s.ToLower(CultureInfo.CurrentCulture).StartsWith("tbl", StringComparison.Ordinal))
                        {
                            tables.Add(s);
                            Logger.Info("Clearing Table " + s);
                        }
                    }
                    dr.Dispose();
                    foreach (var table in tables)
                    {
                        Logger.Info(table);
                        string sql = "DELETE FROM " + table + " where ID in (select id from " + table + " limit 1)";
                        Logger.Info(sql);
                        cmd.ExecuteNonQuery(sql);
                    }
                }
            }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public static List<string> GetTableList([JetBrains.Annotations.NotNull] DatabaseSetup db)
        {
            var tables = new List<string>();
            using (var con = new Connection(db.ConnectionString)) {
                con.Open();
                using (var cmd = new Command(con)) {
                    var dr = cmd.ExecuteReader("SELECT * from SQLITE_Master WHERE type = 'table'");
                    while (dr.Read()) {
                        var s = dr.GetString("name");
                        if (s.ToUpperInvariant().StartsWith("TBL", StringComparison.Ordinal)) {
                            tables.Add(s);
                        }
                    }
                    dr.Dispose();
                }
            }
            return tables;
        }

        [JetBrains.Annotations.NotNull]
        private static Dictionary<string, int> GetTableRowCounts([JetBrains.Annotations.NotNull] string connectionstring)
        {
            var tablerowcounts = new Dictionary<string, int>();
            using (var con = new Connection(connectionstring)) {
                con.Open();
                var tables = new List<string>();
                using (var cmd = new Command(con)) {
                    var dr = cmd.ExecuteReader("SELECT * from SQLITE_Master WHERE type = 'table'");
                    while (dr.Read()) {
                        var s = dr.GetString("name");
                        if (s.ToLower(CultureInfo.CurrentCulture).StartsWith("tbl", StringComparison.Ordinal)) {
                            tables.Add(s);
                            Logger.Info("Counting Table " + s);
                        }
                    }
                    dr.Dispose();
                    foreach (var table in tables) {
                        var o = cmd.ExecuteScalar("SELECT count(*) FROM " + table);
                        var i = (int) (long) o;
                        tablerowcounts.Add(table, i);
                    }
                }
            }
            return tablerowcounts;
        }

        private enum ClearMode {
            ClearTable,
            DeleteOnlyOneRow,
            NoClearing
        }

        private static void TestImport([JetBrains.Annotations.NotNull] string path, bool clearDstFolder, ClearMode cleartablemode,
            bool verifyRowCount = false)
        {
            TestImport(path, out _, out _, cleartablemode, verifyRowCount, out _, clearDstFolder);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void TestImport([JetBrains.Annotations.NotNull] string srcfilename, [JetBrains.Annotations.NotNull] out Simulator mainsim,
            [JetBrains.Annotations.NotNull] out DatabaseMerger dbm, ClearMode clearTablesMode,
            bool checkrowcounts, [JetBrains.Annotations.NotNull] out DatabaseSetup db, bool clearDstFolder)
        {
            var acceptedUneven = new List<string>
            {
                "tblSettings",
                "tblOptions",
                "tblHHTTraits", // this is messed up since it would need a sorting of the householdtraits by subtraits
                "tblLPGVersion", // settings are not imported
                "tblHouseholdPlanEntries" //nobody cares about the household plans anymore
            };
            if (clearDstFolder) {
                CleanTestBase.RunAutomatically(false);
            }
            DirectoryInfo di = new DirectoryInfo(".");
            Logger.Info("Current directory:" + di.FullName);
            var fis = di.GetFiles();
            Logger.Info("Files in this directory:");
            foreach (var fileInfo in fis) {
                Logger.Info(fileInfo.Name);
            }

            var fi = FindImportFile(srcfilename);
            db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());

            var oldSim = new Simulator(db.ConnectionString);
            Dictionary<string, int> srctablerowcounts = null;
            switch (clearTablesMode) {
                case ClearMode.ClearTable: {
                    var connectionstrOld = "Data Source=" + fi.FullName;
                    srctablerowcounts = GetTableRowCounts(connectionstrOld);
                    ClearAllTables(db);
                }
                    break;
                case ClearMode.DeleteOnlyOneRow: {
                    var connectionstrOld = "Data Source=" + fi.FullName;
                    srctablerowcounts = GetTableRowCounts(connectionstrOld);
                    DeleteOneElementFromAllTables(db);
                }
                    break;
                case ClearMode.NoClearing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clearTablesMode), clearTablesMode, null);
            }
            var mainSim = new Simulator(db.ConnectionString);
            dbm = new DatabaseMerger(mainSim);

            dbm.RunFindItems(fi.FullName, null);
            dbm.RunImport(null);
            foreach (var oldAff in oldSim.Affordances.Items) {
                foreach (var newAff in mainSim.Affordances.Items) {
                    if (newAff.Name == oldAff.Name) {
                        foreach (var oldDes in oldAff.AffordanceDesires) {
                            var foundDesire = false;
                            foreach (var newDes in newAff.AffordanceDesires) {
                                if (oldDes.Desire.Name == newDes.Desire.Name) {
                                    foundDesire = true;
                                }
                            }
                            if (!foundDesire) {
                                Logger.Info("Missing affordance Desire in " + oldAff.Name + " " + oldDes.Desire.Name);
                            }
                        }
                    }
                }
            }
            foreach (var hhg in oldSim.HouseholdTemplates.Items) {
                Logger.Info("HHG 1:" + hhg.Name + " " + hhg.Entries.Count);
            }
            foreach (var hhg in mainSim.HouseholdTemplates.Items) {
                Logger.Info("HHG 2:" + hhg.Name + " " + hhg.Entries.Count);
            }
            if (checkrowcounts && srctablerowcounts != null) {
                var dsttablerowcounts = GetTableRowCounts(db.ConnectionString);
                var errors = new List<string>();

                foreach (var srctablerowcount in srctablerowcounts) {
                    if (!acceptedUneven.Contains(srctablerowcount.Key)) {
                        if (!dsttablerowcounts.ContainsKey(srctablerowcount.Key)) {
                            errors.Add(srctablerowcount.Key + " is missing in the dst");
                        }
                        if (dsttablerowcounts[srctablerowcount.Key] != srctablerowcount.Value) {
                            errors.Add(srctablerowcount.Key + " is missing rows: dst = " +
                                       dsttablerowcounts[srctablerowcount.Key] + " src = " + srctablerowcount.Value);
                        }
                    }
                }
                var s = string.Empty;

                foreach (var error in errors) {
                    Logger.Error(error);
                    s += error + Environment.NewLine;
                }
                if (s.Length > 0) {
                    throw new LPGException("Errors happened in " + s);
                }
            }

            mainsim = mainSim;
            // load again to see if anything gets deleted on loading
            Logger.Threshold = Severity.Debug;
            var oldSim2 = new Simulator(db.ConnectionString);
            foreach (var hhg in oldSim2.HouseholdTemplates.Items) {
                Logger.Info("HHG 3:" + hhg.Name + " " + hhg.Entries.Count);
            }
            foreach (var hhg in mainSim.HouseholdTemplates.Items) {
                Logger.Info("HHG 4:" + hhg.Name + " " + hhg.Entries.Count);
            }
            if (checkrowcounts) {
                var dsttablerowcounts = GetTableRowCounts(db.ConnectionString);
                var errors = new List<string>();

                if (srctablerowcounts == null) {
                    throw new LPGException("srctablerowcounts was null");
                }
                foreach (var srctablerowcount in srctablerowcounts) {
                    if (!acceptedUneven.Contains(srctablerowcount.Key)) {
                        if (!dsttablerowcounts.ContainsKey(srctablerowcount.Key)) {
                            errors.Add(srctablerowcount.Key + " is missing in the dst");
                        }
                        if (dsttablerowcounts[srctablerowcount.Key] != srctablerowcount.Value) {
                            errors.Add(srctablerowcount.Key + " is missing rows: dst = " +
                                       dsttablerowcounts[srctablerowcount.Key] + " src = " + srctablerowcount.Value);
                        }
                    }
                }
                var s = string.Empty;

                foreach (var error in errors) {
                    Logger.Error(error);
#pragma warning disable CC0039 // Don't concatenate strings in loops
                    s += error + Environment.NewLine;
#pragma warning restore CC0039 // Don't concatenate strings in loops
                }
                if (s.Length > 0) {
                    throw new LPGException("Errors happened in " + s);
                }
            }
            db.Cleanup();
            if (clearDstFolder) {
                CleanTestBase.RunAutomatically(true);
            }
        }

        [JetBrains.Annotations.NotNull]
        private static FileInfo FindImportFile([JetBrains.Annotations.NotNull] string srcfilename)
        {
            const string teamcityrelativePath = @"..\..\..\Importfiles\";
            const string jenkinsrelativePath = "Importfiles\\";
            FileInfo fi = new FileInfo(Path.Combine(teamcityrelativePath, srcfilename));
            if (fi.Exists) {
                return fi;
            }
            Logger.Info("file not found: " + fi.FullName + ", trying jenkins path next");
            fi = new FileInfo(Path.Combine(jenkinsrelativePath, srcfilename));
            if (fi.Exists) {
                return fi;
            }
            Logger.Info("file not found: " + fi.FullName + ", trying jenkins path next");
            var assemblyInfo = Assembly.GetExecutingAssembly().Location;
            var assemblfi = new FileInfo(assemblyInfo);
            var currPath = assemblfi.Directory;
            fi = new FileInfo(Path.Combine(currPath?.FullName??"",srcfilename));
            if (fi.Exists) {
                return fi;
            }

            Logger.Info("not found: " + fi.FullName);
            fi = new FileInfo(Path.Combine(currPath?.FullName??"", jenkinsrelativePath, srcfilename));
            if (fi.Exists) {
                return fi;
            }

            throw new LPGException("Missing file: " + fi.FullName + "\n Current Directory:" + Directory.GetCurrentDirectory());
    }

        /*
        [Fact]
        public void ComparisonTest()
        {
            var importedDBString = "Data Source=e:\\importeddb.db3";
            var testdb = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var original = new Simulator(testdb.ConnectionString);
            var imported = new Simulator(importedDBString);
           // original.DeepCompare(imported, 0, "");
        }
        */
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "oldSim")]
        [Fact]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTermMerge)]
        public void LoadEmptyDatabase()
        {
            CleanTestBase.RunAutomatically(false);
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                ClearAllTables(db);
#pragma warning disable S1854 // Dead stores should be removed

#pragma warning disable S1481 // Unused local variables should be removed
#pragma warning disable IDE0059 // Value assigned to symbol is never used
                var oldSim = new Simulator(db.ConnectionString); // need to load it for testing
#pragma warning restore IDE0059 // Value assigned to symbol is never used
#pragma warning restore S1481 // Unused local variables should be removed
#pragma warning restore S1854 // Dead stores should be removed
                db.Cleanup();
            }
            CleanTestBase.RunAutomatically(true);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTermMerge)]
        public void RunTest124WithClear() => TestImport("profilegenerator124.db3", true,ClearMode.NoClearing);

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTermMerge)]
        public void RunTest240SMAWithClear() => TestImport("profilegenerator240SMA.db3", true,ClearMode.NoClearing);

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTermMerge)]
        public void RunTest520SimonWithClear() => TestImport("profilegenerator520_simon.db3",
            true,ClearMode.NoClearing);

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTermMerge)]
        public void RunTestCurrentVersionIdentical()
        {
            // tests with the current database without any changes. this should find nothing to import
            HouseholdPlan.FailOnIncorrectImport = true;
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                var path = Path.Combine(wd.WorkingDirectory, "profilegeneratorNothingImportTest.db3");
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Thread.Sleep(3000);
                }
                File.Copy(DatabaseSetup.GetSourcepath(null), path);
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var mainSim = new Simulator(db.ConnectionString);
                    var dbm = new DatabaseMerger(mainSim);
                    dbm.RunFindItems(path, null);
                    Logger.Info("Found " + dbm.ItemsToImport.Count + " items.");
                    if (dbm.ItemsToImport.Count != 0)
                    {
                        throw new LPGException("This should not import anything, since its the same database.");
                    }
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTermMerge)]
        public void RunTestCurrentVersionWithClear()
        {
            Logger.Threshold = Severity.Error;
            HouseholdPlan.FailOnIncorrectImport = true;
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                var path = Path.Combine(wd.WorkingDirectory, "profilegeneratorFullImportTest.db3");
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Thread.Sleep(3000);
                }
                Thread.Sleep(3000);
                File.Copy(DatabaseSetup.GetSourcepath(null), path);
                TestImport(path, false, ClearMode.ClearTable, true);
                wd.CleanUp(1);
            }
        }
        /*
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTermMerge)]
        public void RunTestCurrentVersionWithClearFast()
        {
            HouseholdPlan.FailOnIncorrectImport = true;
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());

            var path = Path.Combine(wd.WorkingDirectory, "profilegeneratorFullImportTest.db3");
            if (File.Exists(path))
            {
                File.Delete(path);
                Thread.Sleep(3000);
            }
            Thread.Sleep(3000);
            File.Copy(DatabaseSetup.GetSourcepath(null, DatabaseSetup.TestPackage.LongTermMerger), path);
            TestImport(path, false, ClearMode.DeleteOnlyOneRow, true);
            wd.CleanUp(1);
        }*/

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTermMerge)]
        public void RunTestSMA381()
        {
            DeviceCategory.ThrowExceptionOnImportWithMissingParent = true;
            TestImport(DatabaseSetup.GetImportFileFullPath("Profilegenerator381SMA.db3"), true,ClearMode.NoClearing);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTermMerge)]
        public void Version520TimeLimitImport()
        {
            string sourcefilepath = DatabaseSetup.GetImportFileFullPath("profilegenerator520_simon.db3");
            CleanTestBase.RunAutomatically(false);
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                var fi = new FileInfo(sourcefilepath);
                if (!File.Exists(sourcefilepath))
                {
                    throw new LPGException("Missing file: " + fi.FullName);
                }
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var mainSim = new Simulator(db.ConnectionString);
                    var dbm = new DatabaseMerger(mainSim);
                    Logger.Info("Full Source: " + fi.FullName);
                    dbm.RunFindItems(sourcefilepath, null);
                    dbm.RunImport(null);
                    var timelimits = dbm.ItemsToImport.Count(x => x.Entry.GetType() == typeof(TimeLimit));
                    timelimits.Should().BeGreaterThan(0);
                    // ReSharper disable once UnusedVariable
#pragma warning disable S1481 // Unused local variables should be removed
                    var mainSim2 = new Simulator(db.ConnectionString);
                }
#pragma warning restore S1481 // Unused local variables should be removed
                /*mainSim2.MyGeneralConfig.PerformCleanUpChecks = "false";
                var toDelete = new List<DeviceActionGroup>();
                foreach (DeviceActionGroup deviceActionGroup in mainSim2.DeviceActionGroups.It) {
                    var actions = deviceActionGroup.GetDeviceActions(mainSim2.DeviceActions.It);
                    if(actions.Count == 0)
                        toDelete.Add(deviceActionGroup);
                }
                foreach (DeviceActionGroup deviceActionGroup in toDelete) {
                    mainSim2.DeviceActionGroups.DeleteItem(deviceActionGroup);
                }
                var dsttag = mainSim2.TraitTags.It.First(x => x.Name.StartsWith("Living "));
                foreach (ModularHousehold modularHousehold in mainSim2.ModularHouseholds.It) {
                    var toSet = new List<ModularHouseholdPerson>();
                    foreach (ModularHouseholdPerson modularHouseholdPerson in modularHousehold.Persons) {
                        if (modularHouseholdPerson.TraitTag == null) {
                            toSet.Add(modularHouseholdPerson);
                        }
                    }
                    foreach (ModularHouseholdPerson person in toSet) {
                        modularHousehold.SwapPersons(person, person.Person, dsttag);
                    }
                    modularHousehold.SaveToDB();
                }
                SimIntegrityChecker.Run(mainSim2);*/
                wd.CleanUp();
            }
            CleanTestBase.RunAutomatically(true);
        }

        public LongtermTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}