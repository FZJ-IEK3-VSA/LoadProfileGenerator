using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using MigraDoc.DocumentObjectModel;

namespace ChartCreator2.PDF {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    internal class CoverPage : IPageCreator {
        [CanBeNull] private string _endDate;

        [CanBeNull] private string _energyIntensity;

        [CanBeNull] private string _householdName;

        [CanBeNull] private string _seed;

        [CanBeNull] private string _startdate;

        [CanBeNull] private string _version;

        public void MakePage(Document doc, string dstdir, bool requireAll) {
            if (!ReadHouseholdName(dstdir)) {
                if (requireAll) {
                    throw new LPGException("Missing Household Name File");
                }
                return;
            }

            var section = doc.AddSection();

            section.AddParagraph();
            var paragraph = section.AddParagraph("Overview of the results of the household"+ Environment.NewLine + _householdName);

            paragraph.Format.Font.Size = 16;
            paragraph.Format.Font.Color = Colors.DarkRed;
            paragraph.Format.SpaceBefore = "8cm";
            paragraph.Format.SpaceAfter = "3cm";

            paragraph = section.AddParagraph("Calculation Time"+ Environment.NewLine + _startdate + " - " + _endDate);
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Color = Colors.Black;
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.Format.SpaceAfter = "1cm";

            paragraph = section.AddParagraph(_energyIntensity);
            paragraph.Format.Font.Size = 10;
            paragraph.Format.Font.Color = Colors.Black;
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.Format.SpaceAfter = "1cm";

            if (_seed != null) {
                paragraph = section.AddParagraph(_seed);
                paragraph.Format.Font.Size = 12;
                paragraph.Format.Font.Color = Colors.Black;
                paragraph.Format.SpaceBefore = "1cm";
                paragraph.Format.SpaceAfter = "1cm";
            }

            paragraph = section.AddParagraph("LoadProfileGenerator " + _version);
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Color = Colors.Black;
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.Format.SpaceAfter = "1cm";
            paragraph = section.AddParagraph("by Noah Pflugradt");
            paragraph.Format.Font.Size = 10;
            paragraph.Format.Font.Color = Colors.Black;
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.Format.SpaceAfter = "1cm";

            paragraph = section.AddParagraph("http://www.loadprofilegenerator.de");
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Color = Colors.Blue;
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.Format.SpaceAfter = "1cm";

            paragraph = section.AddParagraph("Rendering date:");
            paragraph.AddDateField();
        }

        public static void DefineSettlementCover([JetBrains.Annotations.NotNull] Document document, [JetBrains.Annotations.NotNull] string dstDir) {
            var strings = GetSettlementStrings(dstDir);
            var section = document.AddSection();
            section.AddParagraph();
            var paragraph = section.AddParagraph("Overview of the results of the settlement"+ Environment.NewLine + strings[0]);
            paragraph.Format.Font.Size = 16;
            paragraph.Format.Font.Color = Colors.DarkRed;
            paragraph.Format.SpaceBefore = "8cm";
            paragraph.Format.SpaceAfter = "3cm";

            for (var i = 1; i < strings.Count; i++) {
                paragraph = section.AddParagraph(strings[i]);
                if (strings.Count < 10) {
                    paragraph.Format.Font.Size = 12;
                }
                else {
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
                    var fontsize = 12 - strings.Count / 10;
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
                    if (fontsize < 3) {
                        fontsize = 3;
                    }
                    paragraph.Format.Font.Size = fontsize;
                }
                paragraph.Format.Font.Color = Colors.Black;
                paragraph.Format.SpaceBefore = "0cm";
                paragraph.Format.SpaceAfter = "0cm";
            }

            paragraph = section.AddParagraph("http://www.loadprofilegenerator.de");
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Color = Colors.Blue;
            paragraph.Format.SpaceBefore = "1cm";
            paragraph.Format.SpaceAfter = "1cm";

            paragraph = section.AddParagraph("Rendering date:");
            paragraph.AddDateField();
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private static List<string> GetSettlementStrings([JetBrains.Annotations.NotNull] string destinationDirectory) {
            var filename = Path.Combine(destinationDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Root), "Information.txt");
            var strings = new List<string>();
            if (!File.Exists(filename)) {
                return strings;
            }

            using (var sr = new StreamReader(filename)) {
                while (!sr.EndOfStream) {
                    strings.Add(sr.ReadLine());
                }
            }
            return strings;
        }

        private bool ReadHouseholdName([JetBrains.Annotations.NotNull] string destinationDirectory) {
            var destinationFullName = Path.Combine(destinationDirectory, "HouseholdName.txt");
            if (File.Exists(destinationFullName)) {
                using (var sr = new StreamReader(destinationFullName)) {
                    _householdName = sr.ReadLine();
                    _startdate = sr.ReadLine();
                    _endDate = sr.ReadLine();
                    _version = sr.ReadLine();
                    _seed = sr.ReadLine();
                    _energyIntensity = sr.ReadLine();
                }
                return true;
            }
            return false;
        }
    }
}