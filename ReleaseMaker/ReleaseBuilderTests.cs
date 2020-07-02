#nullable enable
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

#pragma warning disable 162

namespace ReleaseMaker
{
    public class CopierBase {
        protected static void Copy(List<string> programFiles, [NotNull] string src, [NotNull] string dst, [NotNull] string filename, string? dstfilename = null)
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
            Logger.Info("Copied " + filename);
            programFiles.Add(filename);
        }
        protected static void CheckIfFilesAreCompletelyCopied(string src, List<string> programFiles)
        {
            DirectoryInfo di = new DirectoryInfo(src);
            var fis = di.GetFiles("*.*", SearchOption.AllDirectories);
            if (fis.Length == 0) {
                throw new Exception("Not a single file in " + src);
            }
            var filesToComplain = new List<string>();
            var filesToIgnore = new List<string>();
            filesToIgnore.Add("Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll");
            filesToIgnore.Add("Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.dll");
            filesToIgnore.Add("Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.Interface.dll");
            filesToIgnore.Add("calcspec.json");
            filesToIgnore.Add("Log.CommandlineCalculation.txt");
            foreach (var fi in fis)
            {
                if (fi.Name.EndsWith(".pdb"))
                {
                    continue;
                }

                if (fi.Name.EndsWith(".db3"))
                {
                    continue;
                }

                if (fi.Name.EndsWith(".dat"))
                {
                    continue;
                }

                if (fi.Name.EndsWith(".sqlite"))
                {
                    continue;
                }

                if (filesToIgnore.Contains(fi.Name))
                {
                    continue;
                }

                if (!programFiles.Contains(fi.Name))
                {
                    filesToComplain.Add(fi.Name);
                }
            }

            if (filesToComplain.Count > 0)
            {
                string s1 = "";
                foreach (var fn in filesToComplain)
                {
                    s1 += "Copy(programFiles, src, dst, \"" + fn + "\");\n";
                }

                throw new LPGException("Forgotten Files in " + Utili.GetCallingMethodAndClass() + " :\n" + s1);
            }
        }
    }

    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public class ReleaseBuilderTests
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


//        private void CopyFiles([NotNull] string src, [NotNull] string dst)
//        {
//            Copy(programFiles, src, dst, "Autofac.dll");
//            Copy(programFiles, src, dst, "Automation.dll");
//            Copy(programFiles, src, dst, "CalcPostProcessor.dll");
//            Copy(programFiles, src, dst, "CalculationController.dll");
//            Copy(programFiles, src, dst, "CalculationEngine.dll");
//            Copy(programFiles, src, dst, "ChartCreator2.dll");
//            Copy(programFiles, src, dst, "ChartPDFCreator.dll");
//            Copy(programFiles, src, dst, "Common.dll");
//            Copy(programFiles, src, dst, "Database.dll");
//            Copy(programFiles, src, dst, "SimulationEngineLib.dll");
//            Copy(programFiles, src, dst, "System.Buffers.dll");
//            Copy(programFiles, src, dst, "System.Memory.dll");
//            Copy(programFiles, src, dst, "System.Numerics.Vectors.dll");
//            Copy(programFiles, src, dst, "System.Resources.Extensions.dll");
//            Copy(programFiles, src, dst, "System.Runtime.CompilerServices.Unsafe.dll");
//            //Copy(programFiles, src, dst, "MigraDoc.DocumentObjectModel-wpf.dll");
//            //Copy(programFiles, src, dst, "MigraDoc.DocumentObjectModel.dll");
//            //Copy(programFiles, src, dst, "MigraDoc.Rendering-wpf.dll");
//            //Copy(programFiles, src, dst, "MigraDoc.Rendering.dll");
//            //Copy(programFiles, src, dst, "MigraDoc.RtfRendering.dll");

//            //Copy(desrc, dst, "MigraDoc.DocumentObjectModel.resources.dll");
//            //
//            Copy(programFiles, src, dst, "MigraDoc.DocumentObjectModel-gdi.dll");
//            string desrc = Path.Combine(src, "de");
//            Copy(desrc, dst, "MigraDoc.DocumentObjectModel-gdi.resources.dll");
//            Copy(desrc, dst, "MigraDoc.Rendering-gdi.resources.dll");
//            Copy(desrc, dst, "MigraDoc.RtfRendering-gdi.resources.dll");
//            Copy(desrc, dst, "PdfSharp-gdi.resources.dll");
//            Copy(desrc, dst, "PdfSharp.Charting-gdi.resources.dll");
//            Copy(programFiles, src, dst, "MigraDoc.Rendering-gdi.dll");
//            Copy(programFiles, src, dst, "MigraDoc.RtfRendering-gdi.dll");
//            Copy(programFiles, src, dst, "PdfSharp-gdi.dll");
//            Copy(programFiles, src, dst, "PdfSharp.Charting-gdi.dll");
//            //Copy(desrc, dst, "PdfSharp.Charting.resources.dll");
//            //Copy(programFiles, src, dst, "PdfSharp.Charting.dll");
//            //Copy(desrc, dst, "PdfSharp.resources.dll");
//            //Copy(programFiles, src, dst, "PdfSharp.dll");
//            Copy(programFiles, src, dst, "Newtonsoft.Json.dll");
//            Copy(programFiles, src, dst, "OxyPlot.dll");
//            Copy(programFiles, src, dst, "OxyPlot.Pdf.dll");
//            Copy(programFiles, src, dst, "OxyPlot.Wpf.dll");
//            //Copy(programFiles, src, dst, "PdfSharp-wpf.dll");
//            //Copy(programFiles, src, dst, "PdfSharp.Charting-wpf.dll");
//            //Copy(programFiles, src, dst, "SettlementProcessing.dll");
//            Copy(programFiles, src, dst, "SQLite.Interop.dll");
//            //Copy(programFiles, src, dst, "sqlite3.dll");
//            Copy(programFiles, src, dst, "System.Data.SQLite.dll");
//            Copy(programFiles, src, dst, "LoadProfileGenerator.exe");
//            Copy(programFiles, src, dst, "SimulationEngine.exe");
//            Copy(programFiles, src, dst, "PowerArgs.dll");
//            Copy(programFiles, src, dst, "EntityFramework.dll");
//            Copy(programFiles, src, dst, "EntityFramework.SqlServer.dll");
//            Copy(programFiles, src, dst, "JetBrains.Annotations.dll");
//            Copy(programFiles, src, dst, "System.Data.SQLite.EF6.dll");
//            Copy(programFiles, src, dst, "System.Data.SQLite.Linq.dll");
//            Copy(programFiles, src, dst, "Microsoft.Bcl.AsyncInterfaces.dll");
//            //Copy(programFiles, src, dst, "Humanizer.dll");

//            Copy(programFiles, src, dst, "EPPlus.dll");
//            //Copy(programFiles, src, dst, "System.Collections.Immutable.dll");
//            //Copy(programFiles, src, dst, "System.Composition.AttributedModel.dll");
//            Copy(programFiles, src, dst, "System.Composition.Convention.dll");
//            Copy(programFiles, src, dst, "System.Composition.Hosting.dll");
//            Copy(programFiles, src, dst, "System.Composition.Runtime.dll");
//            Copy(programFiles, src, dst, "System.Composition.TypedParts.dll");
//            Copy(programFiles, src, dst, "System.Reflection.Metadata.dll");
//            Copy(programFiles, src, dst, "System.Text.Encoding.CodePages.dll");
//            Copy(programFiles, src, dst, "System.Threading.Tasks.Extensions.dll");
//            Copy(programFiles, src, dst, "xunit.abstractions.dll");
//            //string src64 = Path.Combine(src, "x64");
//            //Copy(programFiles, src64, dst, "sqlite3.dll");
//            DirectoryInfo di = new DirectoryInfo(src);
//            var fis = di.GetFiles("*.dll", SearchOption.AllDirectories);
//            List<string> filesToIgnore = new List<string> {
//                "Common.Tests.dll", "Database.Tests.dll", "nunit.framework.dll", "ReleaseBuilder.dll", "FluentAssertions.dll",
//                "Microsoft.VisualStudio.CodeCoverage.Shim.dll", "Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll",
//                "Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.dll",
//                "Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.Interface.dll",
//                "Microsoft.VisualStudio.TestPlatform.TestFramework.dll",
//                "Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll",
//                "Microsoft.CodeAnalysis.CSharp.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.dll",
//"Microsoft.CodeAnalysis.dll",
//"Microsoft.CodeAnalysis.VisualBasic.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll",
//"Microsoft.CodeAnalysis.Workspaces.dll",
//"xunit.assert.dll",
//"xunit.core.dll",
//"xunit.execution.desktop.dll",
//"xunit.runner.reporters.net452.dll",
//"xunit.runner.utility.net452.dll",
//"xunit.runner.visualstudio.testadapter.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.resources.dll",
//"Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll",
//"Microsoft.CodeAnalysis.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.resources.dll",
//"Microsoft.CodeAnalysis.VisualBasic.Workspaces.resources.dll"
//            };
//            var filesToComplain = new List<string>();
//            foreach (var fi in fis) {
//                if (filesToIgnore.Contains(fi.Name)) {
//                    continue;
//                }
//                if (!programFiles.Contains(fi.Name)) {
//                    filesToComplain.Add(fi.Name);
//                }
//            }

//            if (filesToComplain.Count > 0) {
//                throw new LPGException("Forgotten Files:" + string.Join("\",\n",filesToComplain));
//            }
//            //Copy(programFiles, src, dst, "netstandard.dll");
//        }


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
            var affordances = new List<Affordance>(sim.Affordances.Items);
            var householdTraits = new List<HouseholdTrait>(sim.HouseholdTraits.Items);
            sim.DeviceCategories.Items.ToList().ForEach(dc => dc.RefreshSubDevices());
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
                foreach (var householdTrait in sim.HouseholdTraits.Items)
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
            foreach (var householdTrait in sim.HouseholdTraits.Items)
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
            foreach (var desire in sim.Desires.Items)
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
            foreach (var affordance in sim.Affordances.Items)
            {
                foreach (var affordanceDevice in affordance.AffordanceDevices)
                {
                    usedDevices.Add(affordanceDevice.Device);
                }
            }
            foreach (var location in sim.Locations.Items)
            {
                foreach (var locdev in location.LocationDevices)
                {
                    usedDevices.Add(locdev.Device);
                }
            }
            foreach (var action in sim.DeviceActions.Items)
            {
                usedDevices.Add(action.Device);
            }
            foreach (var dev in sim.RealDevices.Items)
            {
                usedDevices.Add(dev.DeviceCategory);
            }
            foreach (var hht in sim.HouseholdTraits.Items)
            {
                foreach (var autodev in hht.Autodevs)
                {
                    usedDevices.Add(autodev.Device);
                }
            }
            var devices = new List<IAssignableDevice>();
            devices.Clear();
            foreach (var rd in sim.RealDevices.Items)
            {
                var found = usedDevices.Contains(rd) || usedDevices.Contains(rd.DeviceCategory);
                if (!found)
                {
                    devices.Add(rd);
                }
            }
            foreach (var dc in sim.DeviceCategories.Items)
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
            using (var db = new DatabaseSetup("CheckForNewLeftovers", filename))
            {
                var sim = new Simulator(db.ConnectionString);
                CheckForNewItems(sim);
                CheckForDevicesWithoutCategory(sim);
                FindUnusedAffordance(sim);
                FindUnusedDevices(sim);
                //TODO: Add the trait check back
                //FindUnusedTraits(sim);
                sim.MyGeneralConfig.PerformCleanUpChecks = "false";
                SimIntegrityChecker.Run(sim);
                db.Cleanup();
            }
            CheckForCalculationOutcomeCompleteness();
        }

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public void CheckForCalculationOutcomeCompleteness()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Logger.Info("Using file " + db.FileName);
                var sim = new Simulator(db.ConnectionString);
                var count = CalculationOutcomesPresenter.CountMissingEntries(sim);
#pragma warning disable S2583 // Conditionally executed blocks should be reachable
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (count != 0 && ThrowOnMissingOutcomes)
                    // ReSharper disable once HeuristicUnreachableCode
                {
#pragma warning restore S2583 // Conditionally executed blocks should be reachable
                    throw new LPGException("Missing " + count + " calculation outcomes!");
                }
                db.Cleanup();
            }
        }


//        [Test]
        //[Fact]
        //      [Trait(UnitTestCategories.Category,"ReleaseMaker")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public void MakeRelease()
        {
            const string filename = "profilegenerator-latest.db3";
            const bool cleanDatabase = true;
            const bool makeZipAndSetup = true;
            const bool cleanCalcOutcomes = true;
            Logger.Info("### Starting Release");
            var releasename = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            releasename = releasename.Substring(0, 5);
            Logger.Info("Release name: " + releasename);
            //return;
            var dstWin = @"v:\Dropbox\LPGReleases\releases" + releasename + "\\net48";
            var dstLinux = @"v:\Dropbox\LPGReleases\releases" + releasename + "\\linux";
            var dstWinCore = @"v:\Dropbox\LPGReleases\releases" + releasename + "\\netCore";
            //const string srcsim = @"v:\Dropbox\LPG\SimulationEngine\bin\x64\Debug";

            PrepareDirectory(dstWin);
            PrepareDirectory(dstLinux);
            PrepareDirectory(dstWinCore);
            const string srclpg = @"C:\Work\LPGDev\WpfApplication1\bin\Debug\net48";
            Logger.Info("### Copying win lpg files");
            var filesForSetup = WinLpgCopier.CopyLpgFiles(srclpg, dstWin);
            const string srcsim = @"C:\Work\LPGDev\SimulationEngine\bin\Debug\net48";
            var filesForSetup2 = SimEngineCopier.CopySimEngineFiles(srcsim, dstWin);

            const string srcsim2 = @"C:\Work\LPGDev\SimEngine2\bin\Release\netcoreapp3.1\win10-x64";
            SimEngine2Copier.CopySimEngine2Files(srcsim2, dstWinCore);
            const string srcsimLinux = @"C:\Work\LPGDev\SimEngine2\bin\Release\netcoreapp3.1\linux-x64";
            LinuxFileCopier.CopySimEngineLinuxFiles(srcsimLinux, dstLinux);
            Logger.Info("### Finished copying lpg files");
            // CopyFiles(src, dst);
            Logger.Info("### Performing release checks");
            ReleaseCheck(filename);
            //CopyFilesSimulationEngine(srcsim, dst);
            using (var db = new DatabaseSetup("Release", filename))
            {
                Logger.Info("Using database " + filename);
#pragma warning disable S2583 // Conditionally executed blocks should be reachable
                if (cleanDatabase)
                {
#pragma warning restore S2583 // Conditionally executed blocks should be reachable
                    //DeleteOldCalcOutcomes(db);
                    Logger.Info("### cleaning database");
                    DissStuffDatabaseCleaner.Run(db.FileName);
                }
                if (cleanCalcOutcomes)
                {
                    Logger.Info("### cleaning calc outcomes");
                    CalculationOutcome.ClearTable(db.ConnectionString);
                }
                Logger.Info("### integrity check");
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
                Logger.Info("### updating estimates");
                foreach (var trait in sim.HouseholdTraits.Items)
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
                    throw new LPGException("Forgotten updates!\n" + Logger.Get().ReturnAllLoggedErrors());
                }

                // get rid of all templated items
#pragma warning disable S2583 // Conditions should not unconditionally evaluate to "true" or to "false"
                if (cleanDatabase)
                {
                    Logger.Info("### deleting all templated items");
#pragma warning restore S2583 // Conditions should not unconditionally evaluate to "true" or to "false" {
                    sim.FindAndDeleteAllTemplated();
                    var templatedItems = sim.FindAndDeleteAllTemplated();
                    if (templatedItems > 0)
                    {
                        throw new LPGException("Left templated items");
                    }
                }

                File.Copy(db.FileName, Path.Combine(dstWin, "profilegenerator.db3"));
                File.Copy(db.FileName, Path.Combine(dstWinCore, "profilegenerator.db3"));
                File.Copy(db.FileName, Path.Combine(dstLinux, "profilegenerator.db3"));
            }
            Thread.Sleep(1000);
            Logger.Info("### Finished copying all files");
            //CopyFilesSimulationEngine(srcsim, dst);
            if (makeZipAndSetup)
            {
                List<FileInfo> fileForUpload = new List<FileInfo>();
                fileForUpload.Add(MakeZipFile(releasename, dstWin));
                fileForUpload.Add(MakeZipFile(releasename +"_core", dstWinCore));
                fileForUpload.Add(MakeZipFile(releasename + "_linux", dstLinux));

                var allSetupFiles = filesForSetup.ToList();
                allSetupFiles.AddRange(filesForSetup2);
                allSetupFiles = allSetupFiles.Distinct().ToList();

                fileForUpload.Add(MakeSetup(dstWin, releasename, allSetupFiles));
                var dstUpload = @"v:\Dropbox\LPGReleases\releases" + releasename + "\\upload";
                PrepareDirectory(dstUpload);
                foreach (FileInfo fi in fileForUpload) {
                    string dstName = Path.Combine(dstUpload, fi.Name);
                    fi.CopyTo(dstName,true);
                }
            }
        }

        private static void PrepareDirectory(string dstWin)
        {
            if (Directory.Exists(dstWin)) {
                try {
                    Directory.Delete(dstWin, true);
                }
                catch (Exception ex) {
                    Logger.Info(ex.Message);
                }

                Thread.Sleep(250);
            }

            Directory.CreateDirectory(dstWin);
            Thread.Sleep(250);
        }

        private static FileInfo MakeSetup([NotNull] string dst, [NotNull] string releaseName, List<string> programFiles)
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
                foreach (var programFile in programFiles) {
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

            Logger.Info("Currently open connections:" + Connection.ConnectionCount);
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
            return new FileInfo(newsetupFileName);
        }

        private static FileInfo MakeZipFile([NotNull] string releaseName, [NotNull] string dst)
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
            return new FileInfo( Path.Combine( dst, "LPG"+releaseName + ".zip"));
        }

 //       public ReleaseBuilderTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper){}
    }
}