using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database;
using Database.Database;
using Database.Helpers;
using Database.Tables;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tables.Validation;
using Database.Tests;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.SpecialViews;
using NUnit.Framework;

#pragma warning disable 162

namespace ReleaseBuilder
{
    [TestFixture]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public class ReleaseBuilderTests : UnitTestBaseClass
    {
        private const bool ThrowOnMissingOutcomes = false;
        private const bool ThrowOnUnusedDesires = true;

        private static void CheckForDevicesWithoutCategory([NotNull] Simulator sim)
        {
            sim.DeviceCategories.DeviceCategoryNone.RefreshSubDevices();
            foreach (var device in sim.DeviceCategories.DeviceCategoryNone.SubDevicesWithoutRefresh)
            {
                Logger.Info(device.Name);
            }

            if (sim.DeviceCategories.DeviceCategoryNone.SubDevices.Count > 0)
            {
                throw new LPGException("There are devices in the none-category!");
            }
        }

        [SuppressMessage("ReSharper", "RedundantLogicalConditionalExpressionOperand")]
        private static void CheckForNewItems([NotNull] Simulator sim)
        {
            var unusedDesires = FindUnusedDesires(sim);
            foreach (var unusedDesire in unusedDesires)
            {
                Logger.Error("unused desire:" + unusedDesire);
            }
#pragma warning disable S2583 // Conditionally executed blocks should be reachable
#pragma warning disable S2589 // Boolean expressions should not be gratuitous
            if (ThrowOnUnusedDesires && unusedDesires.Count > 0)
            {
#pragma warning restore S2589 // Boolean expressions should not be gratuitous
#pragma warning restore S2583 // Conditionally executed blocks should be reachable
                throw new LPGException(unusedDesires.Count + " unused desires found!");
            }
            foreach (var category in sim.Categories)
            {
                var thisType = category.GetType();
                var pi = thisType.GetProperty("MyItems");
                if (pi != null)
                {
                    var collection = pi.GetValue(category, null);
                    dynamic collection2 = collection;
                    var list = new List<DBBase>(collection2);
                    foreach (var dbBase in list)
                    {
                        if (dbBase.Name.ToLower(CultureInfo.CurrentCulture).StartsWith("new ", StringComparison.Ordinal)
                        )
                        {
                            throw new LPGException("Forgotten a new item:" + dbBase.Name);
                        }
                        if (dbBase.Name.ToLower(CultureInfo.CurrentCulture).Contains("diss") &&
                            dbBase.TypeDescription != "Settlement Template")
                        {
                            throw new LPGException("Forgotten a diss item: " + Environment.NewLine +
                                                   dbBase.TypeDescription + ":" + Environment.NewLine +
                                                   dbBase.Name);
                        }
                    }
                }
            }
        }

        private void Copy([NotNull] string src, [NotNull] string dst, [NotNull] string filename,  [CanBeNull] string dstfilename = null)
        {
            if (File.Exists(Path.Combine(dst, filename)))
            {
                File.Delete(Path.Combine(dst, filename));
            }
            if (dstfilename == null)
            {
                File.Copy(Path.Combine(src, filename), Path.Combine(dst, filename));
            }
            else
            {
                File.Copy(Path.Combine(src, filename), Path.Combine(dst, dstfilename));
            }
            _programFiles.Add(filename);
        }

        [ItemNotNull] [NotNull] private readonly List<string> _programFiles = new List<string>();

        private void CopyFiles([NotNull] string src, [NotNull] string dst)
        {
            Copy(src, dst, "Autofac.dll");
            Copy(src, dst, "Automation.dll");
            Copy(src, dst, "CalcPostProcessor.dll");
            Copy(src, dst, "CalculationController.dll");
            Copy(src, dst, "CalculationEngine.dll");
            Copy(src, dst, "ChartCreator2.dll");
            Copy(src, dst, "ChartPDFCreator.dll");
            Copy(src, dst, "Common.dll");
            Copy(src, dst, "Database.dll");
            Copy(src, dst, "SimulationEngineLib.dll");
            Copy(src, dst, "System.Buffers.dll");
            Copy(src, dst, "System.Linq.Dynamic.Core.dll");
            Copy(src, dst, "System.Memory.dll");
            Copy(src, dst, "System.Numerics.Vectors.dll");
            Copy(src, dst, "System.Resources.Extensions.dll");
            Copy(src, dst, "System.Runtime.CompilerServices.Unsafe.dll");
            //Copy(src, dst, "MigraDoc.DocumentObjectModel-wpf.dll");
            //Copy(src, dst, "MigraDoc.DocumentObjectModel.dll");
            //Copy(src, dst, "MigraDoc.Rendering-wpf.dll");
            //Copy(src, dst, "MigraDoc.Rendering.dll");
            //Copy(src, dst, "MigraDoc.RtfRendering.dll");

            //Copy(desrc, dst, "MigraDoc.DocumentObjectModel.resources.dll");
            //
            Copy(src, dst, "MigraDoc.DocumentObjectModel-gdi.dll");
            string desrc = Path.Combine(src, "de");
            Copy(desrc, dst, "MigraDoc.DocumentObjectModel-gdi.resources.dll");
            Copy(desrc, dst, "MigraDoc.Rendering-gdi.resources.dll");
            Copy(desrc, dst, "MigraDoc.RtfRendering-gdi.resources.dll");
            Copy(desrc, dst, "PdfSharp-gdi.resources.dll");
            Copy(desrc, dst, "PdfSharp.Charting-gdi.resources.dll");
            Copy(src, dst, "MigraDoc.Rendering-gdi.dll");
            Copy(src, dst, "MigraDoc.RtfRendering-gdi.dll");
            Copy(src, dst, "PdfSharp-gdi.dll");
            Copy(src, dst, "PdfSharp.Charting-gdi.dll");
            //Copy(desrc, dst, "PdfSharp.Charting.resources.dll");
            //Copy(src, dst, "PdfSharp.Charting.dll");
            //Copy(desrc, dst, "PdfSharp.resources.dll");
            //Copy(src, dst, "PdfSharp.dll");
            Copy(src, dst, "Newtonsoft.Json.dll");
            Copy(src, dst, "OxyPlot.dll");
            Copy(src, dst, "OxyPlot.Pdf.dll");
            Copy(src, dst, "OxyPlot.Wpf.dll");
            //Copy(src, dst, "PdfSharp-wpf.dll");
            //Copy(src, dst, "PdfSharp.Charting-wpf.dll");
            //Copy(src, dst, "SettlementProcessing.dll");
            Copy(src, dst, "SQLite.Interop.dll");
            //Copy(src, dst, "sqlite3.dll");
            Copy(src, dst, "System.Data.SQLite.dll");
            Copy(src, dst, "LoadProfileGenerator.exe");
            Copy(src, dst, "SimulationEngine.exe");
            Copy(src, dst, "PowerArgs.dll");
            Copy(src, dst, "EntityFramework.dll");
            Copy(src, dst, "EntityFramework.SqlServer.dll");
            Copy(src, dst, "JetBrains.Annotations.dll");
            Copy(src, dst, "System.Data.SQLite.EF6.dll");
            Copy(src, dst, "System.Data.SQLite.Linq.dll");
            //string src64 = Path.Combine(src, "x64");
            //Copy(src64, dst, "sqlite3.dll");
            DirectoryInfo di = new DirectoryInfo(src);
            var fis = di.GetFiles("*.dll", SearchOption.AllDirectories);
            List<string> filesToIgnore = new List<string> {"Common.Tests.dll", "Database.Tests.dll", "nunit.framework.dll", "ReleaseBuilder.dll", "FluentAssertions.dll" };

            foreach (var fi in fis) {
                if (filesToIgnore.Contains(fi.Name)) {
                    continue;
                }
                if (!_programFiles.Contains(fi.Name)) {
                    throw new LPGException("Forgotten DLL:" + fi.Name);
                }
            }
            //Copy(src, dst, "netstandard.dll");
        }

        /* 
         * //TODO: Fix all unused traits
        private static void FindUnusedTraits(Simulator sim)
        {
            List<HouseholdTrait> usedTraits = new List<HouseholdTrait>();
            foreach (ModularHousehold chh in sim.ModularHouseholds.MyItems)
                foreach (ModularHouseholdTrait trait in chh.Traits)
                    usedTraits.Add(trait.HouseholdTrait);
            List<HouseholdTrait> unusedTraits = new List<HouseholdTrait>();

            foreach (HouseholdTrait hht in sim.HouseholdTraits.MyItems)
            {
                bool found = usedTraits.Contains(hht);

                if (!found)
                    unusedTraits.Add(hht);
            }
            foreach (var device in unusedTraits)
            {
                Logger.Error("Found unused trait: " + device.Name);

            }
            if (unusedTraits.Count > 0)
                throw new LPGException("Unused traits!");
        }
        */
        /*
        private static void DeleteOldCalcOutcomes([NotNull] DatabaseSetup db)
        {
            var sim = new Simulator(db.ConnectionString);
            var maxVersion = sim.CalculationOutcomes.It.Select(x => x.LPGVersion).Distinct()
                .OrderByDescending(x => x).First();
            var toDelete =
                sim.CalculationOutcomes.It.Where(x => x.LPGVersion != maxVersion).ToList();

            foreach (var simCalculationOutcome in toDelete)
            {
                simCalculationOutcome.DeleteFromDB();
            }
            Logger.Info("Deleted " + toDelete.Count + " items.");
        }
        */
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void FindUnusedAffordance([NotNull] Simulator sim)
        {
            var notFoundAffordances = new List<Affordance>();
            var affordances = new List<Affordance>(sim.Affordances.MyItems);
            var householdTraits = new List<HouseholdTrait>(sim.HouseholdTraits.MyItems);
            sim.DeviceCategories.MyItems.ToList().ForEach(dc => dc.RefreshSubDevices());
            // collect all household trait desires
            var hhtdesires = new Dictionary<HouseholdTrait, List<Desire>>();
            foreach (var householdTrait in householdTraits)
            {
                hhtdesires.Add(householdTrait, new List<Desire>());
                foreach (var desire in householdTrait.Desires)
                {
                    if (!hhtdesires[householdTrait].Contains(desire.Desire))
                    {
                        hhtdesires[householdTrait].Add(desire.Desire);
                    }
                }
            }
            foreach (var affordance in affordances)
            {
                var found = false;
                foreach (var householdTrait in sim.HouseholdTraits.MyItems)
                {
                    if (!found)
                    {
                        var isvalidinHousehold = false;
                        foreach (var affordanceDesire in affordance.AffordanceDesires)
                        {
                            if (hhtdesires[householdTrait].Contains(affordanceDesire.Desire))
                            {
                                isvalidinHousehold = true;
                            }
                        }
                        if (isvalidinHousehold)
                        {
                            foreach (var hhLocation in householdTrait.Locations)
                            {
                                if (hhLocation.AffordanceLocations.Any(x => x.Affordance == affordance))
                                {
                                    found = true;
                                }
                            }
                        }
                    }
                }
                if (!found)
                {
                    notFoundAffordances.Add(affordance);
                }
            }
            foreach (var notFoundAffordance in notFoundAffordances)
            {
                Logger.Error("Unused Affordance: " + notFoundAffordance);
            }
            if (notFoundAffordances.Count > 0)
            {
                throw new LPGException("too many unused affordances.");
            }
        }

        [ItemNotNull]
        [NotNull]
        private static List<Desire> FindUnusedDesires([NotNull] Simulator sim)
        {
            var desires = new List<Desire>();
            // collect all household trait desires
            foreach (var householdTrait in sim.HouseholdTraits.MyItems)
            {
                foreach (var desire in householdTrait.Desires)
                {
                    if (!desires.Contains(desire.Desire))
                    {
                        desires.Add(desire.Desire);
                    }
                }
            }
            var unusedDesires = new List<Desire>();
            foreach (var desire in sim.Desires.MyItems)
            {
                if (!desires.Contains(desire))
                {
                    unusedDesires.Add(desire);
                }
            }
            if (unusedDesires.Count > 0)
            {
                var desireNames = "";
                foreach (var unusedDesire in unusedDesires)
                {
                    desireNames = desireNames + Environment.NewLine + unusedDesire;
                }
                throw new LPGException("Unused desires: " + unusedDesires.Count + desireNames);
            }
            return unusedDesires;
        }

        private static void FindUnusedDevices([NotNull] Simulator sim)
        {
            var usedDevices = new List<IAssignableDevice>();
            foreach (var affordance in sim.Affordances.MyItems)
            {
                foreach (var affordanceDevice in affordance.AffordanceDevices)
                {
                    usedDevices.Add(affordanceDevice.Device);
                }
            }
            foreach (var location in sim.Locations.MyItems)
            {
                foreach (var locdev in location.LocationDevices)
                {
                    usedDevices.Add(locdev.Device);
                }
            }
            foreach (var action in sim.DeviceActions.It)
            {
                usedDevices.Add(action.Device);
            }
            foreach (var dev in sim.RealDevices.It)
            {
                usedDevices.Add(dev.DeviceCategory);
            }
            foreach (var hht in sim.HouseholdTraits.MyItems)
            {
                foreach (var autodev in hht.Autodevs)
                {
                    usedDevices.Add(autodev.Device);
                }
            }
            var devices = new List<IAssignableDevice>();
            devices.Clear();
            foreach (var rd in sim.RealDevices.MyItems)
            {
                var found = usedDevices.Contains(rd) || usedDevices.Contains(rd.DeviceCategory);
                if (!found)
                {
                    devices.Add(rd);
                }
            }
            foreach (var dc in sim.DeviceCategories.MyItems)
            {
                if (!usedDevices.Contains(dc) && dc.SubDevices.Count > 0)
                {
                    devices.Add(dc);
                }
            }
            foreach (var device in devices)
            {
                Logger.Error("Found unused device: " + device.Name);
            }
            if (devices.Count > 0)
            {
                throw new LPGException("Unused devices!");
            }
        }

        private void ReleaseCheck([NotNull] string filename)
        {
            var db = new DatabaseSetup("CheckForNewLeftovers", filename);
            var sim = new Simulator(db.ConnectionString);
            CheckForNewItems(sim);
            CheckForDevicesWithoutCategory(sim);
            FindUnusedAffordance(sim);
            FindUnusedDevices(sim);
            //TODO: Add the trait check back
            //FindUnusedTraits(sim);

            SimIntegrityChecker.Run(sim);
            db.Cleanup();
            CheckForCalculationOutcomeCompleteness();
        }

        [Test]
        [Category("BasicTest")]
        public void CheckForCalculationOutcomeCompleteness()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var count = CalculationOutcomesPresenter.CountMissingEntries(sim);
#pragma warning disable S2583 // Conditionally executed blocks should be reachable
            if (count != 0 && ThrowOnMissingOutcomes)
            {
#pragma warning restore S2583 // Conditionally executed blocks should be reachable
                throw new LPGException("Missing " + count + " calculation outcomes!");
            }
            db.Cleanup();
        }

        [Test]
        [Category("ReleaseMaker")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public void MakeRelease()
        {
            SkipEndCleaning = true;
            const string filename = "profilegenerator-latest.db3";
            const bool cleanDatabase = true;
            const bool makeZipAndSetup = true;
            const bool cleanCalcOutcomes = true;

            var releasename = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            releasename = releasename.Substring(0, 5);
            var dst = @"v:\Dropbox\LPGReleases\releases" + releasename;
            const string src = @"C:\Work\LPGDev\ReleaseBuilder\bin\Debug\net472";
            //const string srcsim = @"v:\Dropbox\LPG\SimulationEngine\bin\x64\Debug";

            if (Directory.Exists(dst))
            {
                try {
                    Directory.Delete(dst, true);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }

            Thread.Sleep(100);
            Directory.CreateDirectory(dst);
            Thread.Sleep(100);
           // CopyFiles(src, dst);
            ReleaseCheck(filename);
            CopyFiles(src, dst);
            //CopyFilesSimulationEngine(srcsim, dst);
            var db = new DatabaseSetup("Release", filename);

#pragma warning disable S2583 // Conditionally executed blocks should be reachable
            if (cleanDatabase)
            {
#pragma warning restore S2583 // Conditionally executed blocks should be reachable
                //DeleteOldCalcOutcomes(db);
                DissStuffDatabaseCleaner.Run(db.FileName);
            }
            if (cleanCalcOutcomes)
            {
                CalculationOutcome.ClearTable(db.ConnectionString);
            }
            var sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.ReasonableWithChartsAndPDF);
            sim.MyGeneralConfig.PerformCleanUpChecks = "True";
            sim.MyGeneralConfig.ShowSettlingPeriod = "False";
            sim.MyGeneralConfig.DestinationPath = "C:\\Work\\";
            sim.MyGeneralConfig.ImagePath = "C:\\Work\\";
            sim.MyGeneralConfig.RandomSeed = -1;
            sim.MyGeneralConfig.StartDateString = "01.01.2016";
            sim.MyGeneralConfig.EndDateString = "31.12.2016";
            SimIntegrityChecker.Run(sim);
            sim.MyGeneralConfig.PerformCleanUpChecks = "False";
            sim.MyGeneralConfig.CSVCharacter = ";";
            var forgottenUpdates = false;
            foreach (var trait in sim.HouseholdTraits.It)
            {
                switch (trait.Name)
                {
                    case "Cooking, average":
                    case "Cooking, maximum": continue;

                    default:

                        var count = trait.EstimatedTimeCount;
                        var tt = trait.EstimatedTimeType;
                        trait.CalculateEstimatedTimes();
                        if (Math.Abs(trait.EstimatedTimeCount - count) > 0.0000001 || trait.EstimatedTimeType != tt)
                        {
                            forgottenUpdates = true;
                            Logger.Error("seems you forgot to update the estimate for " + trait.PrettyName +
                                         Environment.NewLine + "Prev count: " + count + " curr: " +
                                         trait.EstimatedTimeCount + Environment.NewLine + "prev tt: " + tt +
                                         " curr tt: " + trait.EstimatedTimeType);
                        }
                        break;
                }
            }
            if (forgottenUpdates)
            {
                throw new LPGException("Forgotten updates!");
            }

            // get rid of all templated items
#pragma warning disable S2583 // Conditions should not unconditionally evaluate to "true" or to "false"
            if (cleanDatabase)
            {
#pragma warning restore S2583 // Conditions should not unconditionally evaluate to "true" or to "false" {
                var templatedItems = sim.FindAndDeleteAllTemplated();
                if (templatedItems > 0)
                {
                    throw new LPGException("Left templated items");
                }
            }

            File.Copy(db.FileName, Path.Combine(dst, "profilegenerator.db3"));
            Thread.Sleep(1000);

            //CopyFilesSimulationEngine(srcsim, dst);
            if (makeZipAndSetup) {
                MakeZipFile(releasename, dst);
                MakeSetup(dst, releasename);
            }
        }

        private void MakeSetup([NotNull] string dst, [NotNull] string releaseName)
        {
//make iss
            string dstFileName = dst + "\\lpgsetup.iss";
            using (var sw = new StreamWriter(dstFileName)) {
                const string top = @"V:\Dropbox\Development\LPGSetup\lpgsetup_start.iss";
                using (var sr = new StreamReader(top)) {
                    while (!sr.EndOfStream) {
                        var s = sr.ReadLine();
                        if (s == null) {
                            throw new LPGException("Readline failed");
                        }

                        if (s.StartsWith("AppVersion=", StringComparison.Ordinal)) {
                            sw.WriteLine("AppVersion=" + releaseName);
                        }
                        else {
                            sw.WriteLine(s);
                        }
                    }
                }

                //insert the files
                foreach (var programFile in _programFiles) {
                    sw.WriteLine("Source: \"" + programFile + "\"; DestDir: \"{app}\"");
                }

                //bottom of the file
                const string bottom = @"V:\Dropbox\Development\LPGSetup\lpgsetup_end.iss";
                using (var sr = new StreamReader(bottom)) {
                    while (!sr.EndOfStream) {
                        var s = sr.ReadLine();
                        if (s == null) {
                            throw new LPGException("Readline failed");
                        }

                        sw.WriteLine(s);
                    }
                }
            }

            Console.WriteLine("Currently open connections:" + Connection.ConnectionCount);
            //Thread.Sleep(3000);
            GC.WaitForPendingFinalizers();
            GC.Collect();
            //Thread.Sleep(3000);
            using (var process2 = new Process()) {
                // Configure the process using the StartInfo properties.
                process2.StartInfo.FileName = @"C:\Program Files (x86)\Inno Setup 6\Compil32.exe";
                process2.StartInfo.Arguments = "/cc lpgsetup.iss";
                Logger.Info(process2.StartInfo.FileName + " " + process2.StartInfo.Arguments);
                process2.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                process2.StartInfo.WorkingDirectory = dst;
                process2.Start();
                process2.WaitForExit(); // Waits here for the process to exit.
            }

            var fi = new FileInfo(Path.Combine(dst, "mysetup.exe"));
            var newsetupFileName = Path.Combine(dst, "Setup" + releaseName + ".exe");
            if (fi.Exists) {
                fi.MoveTo(newsetupFileName);
            }
        }

        private static void MakeZipFile([NotNull] string releaseName, [NotNull] string dst)
        {
            using (var process = new Process()) {
                // Configure the process using the StartInfo properties.
                process.StartInfo.FileName = @"C:\Program Files\7-Zip\7z.exe";
                process.StartInfo.Arguments = "a -tzip -mx9 LPG" + releaseName + ".zip  *.*";
                Logger.Info(process.StartInfo.FileName + " " + process.StartInfo.Arguments);
                process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                process.StartInfo.WorkingDirectory = dst;
                process.Start();
                process.WaitForExit(); // Waits here for the process to exit.
            }
        }
    }
}