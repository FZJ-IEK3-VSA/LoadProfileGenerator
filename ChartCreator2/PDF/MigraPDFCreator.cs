using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace ChartCreator2.PDF {
    public enum FileCreationMode {
        Household,
        Settlement
    }

    public class MigraPDFCreator {
        [NotNull] private readonly ICalculationProfiler _calculationProfiler;
        public MigraPDFCreator([NotNull] ICalculationProfiler calculationProfiler)
        {
            _calculationProfiler = calculationProfiler;
        }

        [NotNull]
        private static Document CreateDocument([NotNull] string destinationDirectory, bool requireAll, [NotNull] string csvCharacter) {
            // Create a new MigraDoc document
            var document = new Document {Info = {Title = "LoadProfileGenerator", Subject = "Overview", Author = "Noah Pflugradt"}};
            var di = new DirectoryInfo(destinationDirectory);
            var fis = di.GetFiles("Results.General.sqlite"); // TotalsPerLoadtype.csv
            if (fis.Length > 0) {
                ProcessHousehold(document, destinationDirectory, requireAll, csvCharacter); // household
            }
            else if (File.Exists(Path.Combine(di.FullName, "Information.txt"))) {
                // settlement
                var subdirs = di.GetDirectories();
                CoverPage.DefineSettlementCover(document, destinationDirectory);
                TotalsPage.MakePage(document, destinationDirectory, requireAll, FileCreationMode.Settlement,
                    null,
                    csvCharacter);
                foreach (var directoryInfo in subdirs) {
                    ProcessHousehold(document, directoryInfo.FullName, requireAll, csvCharacter); // household
                }
            }
            else {
                throw new LPGException("Couldn't find any files for the PDF generation?!?");
            }
            return document;
        }

        public void MakeDocument([NotNull] string pdfDstPath, [NotNull] string calcObjectName, bool startpdf,
            bool requireAll, [NotNull] string csvCharacter, [NotNull] FileFactoryAndTracker fft) {
            // Create a MigraDoc document
             _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - PDF Layout");
            var document = CreateDocument(pdfDstPath, requireAll, csvCharacter);
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - PDF Layout");
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - PDF Render");
#pragma warning disable 618
            var renderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always)
#pragma warning restore 618
            {
                Document = document
            };
            Logger.ImportantInfo("Rendering the PDF. This will take a really long time without any progress report...");

            renderer.RenderDocument();
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - PDF Render");
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - PDF Cleanup");
            // Save the document...
            if (!Directory.Exists(pdfDstPath)) {
                Directory.CreateDirectory(pdfDstPath);
            }
            var filename = "Overview." + AutomationUtili.CleanFileName(calcObjectName) + ".pdf";
            var dstFullName = Path.Combine(pdfDstPath, filename);
            if (File.Exists(dstFullName)) {
                File.Delete(dstFullName);
            }
            Logger.ImportantInfo("----");
            Logger.ImportantInfo(dstFullName);
            Logger.ImportantInfo("----");
            Logger.ImportantInfo("Saving the PDF...");
            renderer.PdfDocument.Save(dstFullName);
            GC.WaitForPendingFinalizers();
            GC.Collect();
            // ...and start a viewer.
            if (startpdf) {
                Process.Start(dstFullName);
            }
            fft.RegisterFile(filename,"Overview of all results",true,ResultFileID.PDF,Constants.GeneralHouseholdKey,TargetDirectory.Root);
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - PDF Cleanup");
            //return new ResultFileEntry("Overview", filename, dstFullName, true);
        }

        [SuppressMessage("ReSharper", "ReplaceWithSingleAssignment.True")]
        private static void ProcessHousehold([NotNull] Document document, [NotNull] string path, bool requireAll, [NotNull] string csvCharacter) {
            var di = new DirectoryInfo(path);
            var fis = di.GetFiles("*.png", SearchOption.AllDirectories);
            var pngfiles = fis.Select(x => x.Name).ToList();

            var cp = new CoverPage();
            cp.MakePage(document, path, requireAll);
            var tocSection = SetTocSection(document);
            TotalsPage.MakePage(document, path, requireAll, FileCreationMode.Household, tocSection, csvCharacter);
            var pp = new PersonPage();
            pp.MakePage(document, path, requireAll, pngfiles, tocSection);
            // How they spend their time

            var afp = new ActivityFrequencyPages();
            afp.MakePage(document, path, requireAll, pngfiles, tocSection);
            var app = new ActivityPercentagePages();
            app.MakePage(document, path, requireAll, pngfiles, tocSection);
            var atu = new AffordanceTimeUsePages();
            atu.MakePage(document, path, requireAll, pngfiles, tocSection);
            var affordanceEnergyUsePerPersonPages =
                new AffordanceEnergyUsePerPersonPages();
            affordanceEnergyUsePerPersonPages.MakePage(document, path, requireAll, pngfiles, tocSection);

            var ats = new AffordanceTaggingSetPages();
            ats.MakePage(document, path, requireAll, pngfiles, tocSection);
            var cpp = new CarpetplotPages();
            cpp.MakePage(document, path, requireAll,  tocSection);

            var eaop = new ExecutedActionsOverviewPages();
            eaop.MakePage(document, path, requireAll, pngfiles, tocSection);

            var timeOfUseProfilesPages = new TimeOfUseProfilesPages();
            timeOfUseProfilesPages.MakePage(document, path, requireAll, pngfiles, tocSection);

            // energy use
            var aeu = new AffordanceEnergyUse();
            aeu.MakePage(document, path, requireAll, pngfiles, tocSection);
            var dsp = new DeviceSumsPages();
            dsp.MakePage(document, path, requireAll, pngfiles, tocSection);

            // energy quality
            var deviceDurationCurvesPages = new DeviceDurationCurvesPages();
            deviceDurationCurvesPages.MakePage(document, path, requireAll, pngfiles, tocSection);
            var dcp = new DurationCurvePages();
            dcp.MakePage(document, path, requireAll, pngfiles, tocSection);
            var deviceTaggingSetPages = new DeviceTaggingSetPages();
            deviceTaggingSetPages.MakePage(document, path, requireAll, pngfiles, tocSection);
            var energyCarpetplotPages = new EnergyCarpetplotPages();
            energyCarpetplotPages.MakePage(document, path, requireAll, pngfiles, tocSection);

            var dpp = new DeviceProfilePages();
            dpp.MakePage(document, path, requireAll, pngfiles, tocSection);
            var timeOfUseEnergyProfilePages = new TimeOfUseEnergyProfilePages();
            timeOfUseEnergyProfilePages.MakePage(document, path, requireAll, pngfiles, tocSection);
            var wpp = new WeekdayProfilesPages();
            wpp.MakePage(document, path, requireAll, pngfiles, tocSection);

            // location
            var lsp = new LocationStatisticsPages();
            lsp.MakePage(document, path, requireAll, pngfiles, tocSection);

            // other
            var dp = new DesirePages();
            dp.MakePage(document, path, requireAll, pngfiles, tocSection);
            var tep = new TemperaturePages();
            tep.MakePage(document, path, requireAll, pngfiles, tocSection);
            var actionsPages = new ActionsPages();
            actionsPages.MakePage(document, path, requireAll, tocSection);
            var spp = new SumProfilePages();
            spp.MakePage(document, path, requireAll, pngfiles, tocSection);
            var sumProfilePagesExternal = new SumProfilePagesExternal();
            sumProfilePagesExternal.MakePage(document, path, requireAll, pngfiles, tocSection);
            var thoughtsp = new ThoughtsPages();
            thoughtsp.MakePage(document, path, requireAll, tocSection);
            var daylight = new DaylightTimePages();
            daylight.MakePage(document, path, requireAll, tocSection);
            var criticalPage = new CriticalThresholdViolationsPage();
            criticalPage.MakePage(document, path, requireAll, pngfiles, tocSection);
            var dpExternalPages = new DeviceProfileExternalPages();
            dpExternalPages.MakePage(document, path, requireAll, pngfiles, tocSection);
            var tpp = new TimeProfilesPages();
            tpp.MakePage(document, path, requireAll, tocSection);
            var vp = new VariablePages();
            vp.MakePage(document, path, requireAll, pngfiles, tocSection);
            var filtered = new List<string>();
            foreach (var pngfile in pngfiles) {
                var isok = true;
                if (pngfile.StartsWith("HouseholdPlan.", StringComparison.Ordinal)) {
                    isok = false;
                }
                if (pngfile.StartsWith("CarpetplotLegend.", StringComparison.Ordinal)) {
                    isok = false;
                }
                if (pngfile.StartsWith("Carpetplot.", StringComparison.Ordinal)) {
                    isok = false;
                }
                if (pngfile.StartsWith("CarpetplotLabeled.", StringComparison.Ordinal)) {
                    isok = false;
                }
                if (pngfile.StartsWith("EnergyCarpetplot.", StringComparison.Ordinal)) {
                    isok = false;
                }
                if (pngfile.StartsWith("DeviceSums_Monthly.", StringComparison.Ordinal)) {
                    isok = false;
                }
                if (pngfile.StartsWith("LocationCarpetPlot", StringComparison.Ordinal))
                {
                    isok = false;
                }
                if (pngfile.StartsWith("TransportationStateCarpetPlot", StringComparison.Ordinal))
                {
                    isok = false;
                }
                if (pngfile.StartsWith("TransportationDeviceUserCarpetPlot", StringComparison.Ordinal))
                {
                    isok = false;
                }
                if (pngfile.StartsWith("TransportationDeviceSiteCarpetPlot", StringComparison.Ordinal))
                {
                    isok = false;
                }
                if (pngfile.StartsWith("CalculationDurationFlameChart", StringComparison.Ordinal))
                {
                    isok = false;
                }
                if (isok) {
                    filtered.Add(pngfile);
                }
            }
            if (filtered.Count > 0) {
                foreach (var pngfile in filtered) {
                    Logger.Info(pngfile);
                }
                throw new LPGException("Forgotten PNGs:" + filtered.Count);
            }
        }

        [NotNull]
        private static Section SetTocSection([NotNull] Document document) {
            var section = document.AddSection();

            section.AddPageBreak();
            var paragraph = section.AddParagraph("Table of Contents");
            paragraph.Format.Font.Size = 20;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.SpaceAfter = 24;
            paragraph.Format.OutlineLevel = OutlineLevel.Level1;
            var style = document.Styles.AddStyle("TOC", "Normal");
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);
            style.ParagraphFormat.Font.Color = Colors.Blue;
            style.ParagraphFormat.SpaceBefore = 8;
            style.ParagraphFormat.SpaceAfter = 8;
            style.ParagraphFormat.Font.Size = 12;

            return section;
        }
    }
}