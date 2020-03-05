using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using MigraDoc.DocumentObjectModel;
using Table = MigraDoc.DocumentObjectModel.Tables.Table;

namespace ChartCreator2.PDF {
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal class TotalsPage : IPageCreator {
        public void MakePage(Document doc, string dstdir, bool requireAll) {
            throw new NotImplementedException();
        }

        public static void MakePage([NotNull] Document doc, [NotNull] string dstdir,
                                    bool requireAll, FileCreationMode fileCreationMode,
            [CanBeNull] Section tocSection, [NotNull] string csvCharacter) {
            string filename = null;
            if (fileCreationMode == FileCreationMode.Settlement && File.Exists(Path.Combine(dstdir, "totals.csv"))) {
                filename = Path.Combine(dstdir, DirectoryNames.CalculateTargetdirectory(TargetDirectory.Root),
                    "Totals.csv");
            }

            if (fileCreationMode == FileCreationMode.Household && File.Exists(Path.Combine(dstdir,
                    DirectoryNames.CalculateTargetdirectory(TargetDirectory.Reports), "TotalsPerLoadtype.csv"))) {
                filename = Path.Combine(dstdir, DirectoryNames.CalculateTargetdirectory(TargetDirectory.Reports),
                    "TotalsPerLoadtype.csv");
            }
            if (filename == null) {
                if (requireAll) {
                    throw new LPGException("Missing totals files.");
                }
                return;
            }

            var sec = doc.AddSection();

            var para = sec.AddParagraph();
            para.Format.Alignment = ParagraphAlignment.Justify;
            para.Format.Font.Name = "Times New Roman";
            para.Format.Font.Size = 20;
            para.Format.Font.Bold = true;
            para.Format.SpaceAfter = "0.5cm";
            para.Format.SpaceBefore = "1cm";
            para.Format.Font.Color = Colors.DarkGray;
            para.AddText("Totals");
            ChartPageBase.MakeTocEntry(tocSection, "Totals");
            para.AddBookmark("Totals");

            switch (fileCreationMode) {
                case FileCreationMode.Household:
                    var totals = TotalsReader(filename, false, csvCharacter);
                    SetHouseholdEntries(totals, doc, sec);
                    SetPerDayTable(totals, doc, sec);
                    SetMinMaxTable(totals, doc, sec);
                    SetPerPersonTable(totals, doc, sec);
                    SetPerPersonDayTable(totals, doc, sec);
                    return;
                case FileCreationMode.Settlement:
                    SetSettlementEntries(doc, sec, filename, csvCharacter);
                    return;
                default: throw new LPGException("Forgotten File Creation Mode");
            }
        }

        private static void MakeTable([NotNull] string headline, [NotNull] Section sec, [NotNull] out ParagraphFormat format, [NotNull] out Table table,
            TableColumnType tableColumnType) {
            var para = sec.AddParagraph();
            para.Format.Alignment = ParagraphAlignment.Justify;
            para.Format.Font.Name = "Times New Roman";
            para.Format.Font.Size = 16;
            para.Format.Font.Bold = true;
            para.Format.SpaceAfter = "0.5cm";
            para.Format.SpaceBefore = "1cm";
            para.Format.Font.Color = Colors.DarkGray;
            para.AddText(headline);

            format = new ParagraphFormat {Font = {Size = 12}, SpaceBefore = "0.2cm", SpaceAfter = "0.2cm"};
            table = new Table {Borders = {Width = 0.75}};
            int[] measurements;
            bool[] alignments;
            string[] headers;
            switch (tableColumnType) {
                case TableColumnType.Values:
                    alignments = new[] {false, true, false};
                    measurements = new[] {5, 5, 5};
                    headers = new[] {"Load Type", "Value", "Unit"};
                    break;
                case TableColumnType.SettlementValues:
                    measurements = new[] {4, 5, 3, 3};
                    alignments = new[] {false, false, true, false};
                    headers = new[] {"Household", "Load Type", "Value", "Unit"};
                    break;
                case TableColumnType.MinMax:
                    measurements = new[] {5, 3, 3, 3};
                    alignments = new[] {false, true, true, false};
                    headers = new[] {"Household", "Minimum", "Maximum", "Unit"};
                    break;
                default: throw new LPGException("Forgotten TableColumnType");
            }

            for (var i = 0; i < measurements.Length; i++) {
                var column0 = table.AddColumn(Unit.FromCentimeter(measurements[i]));
                if (alignments[i]) {
                    column0.Format.Alignment = ParagraphAlignment.Right;
                }
                else {
                    column0.Format.Alignment = ParagraphAlignment.Left;
                }
            }

            var row = table.AddRow();
            row.Shading.Color = Colors.AliceBlue;
            for (var i = 0; i < headers.Length; i++) {
                var cell = row.Cells[i];
                cell.AddParagraph(headers[i]);
                cell.Format = format.Clone();
            }
        }

        private static void SetHouseholdEntries([ItemNotNull] [NotNull] List<TotalEntry> totals, [NotNull] Document doc, [NotNull] Section sec) {
            MakeTable("Totals for each Loadtype", sec, out ParagraphFormat format, out var table, TableColumnType.Values);

            foreach (var entry in totals) {
                string[] values = {entry.Name, entry.Value.ToString("F2", CultureInfo.CurrentCulture), entry.UnitOfSum};
                SetOneRow(format, table, values);
            }
            doc.LastSection.Add(table);
        }

        private static void SetMinMaxTable([ItemNotNull] [NotNull] List<TotalEntry> totals, [NotNull] Document doc, [NotNull] Section sec) {
            MakeTable("Minimum and Maximum for each Loadtype", sec, out ParagraphFormat format, out Table table, TableColumnType.MinMax);

            foreach (var entry in totals) {
                string[] values = {
                    entry.Name, entry.Minimum.ToString("F2", CultureInfo.CurrentCulture),
                    entry.Maximum.ToString("F2", CultureInfo.CurrentCulture), entry.UnitOfPower
                };
                SetOneRow(format, table, values);
            }
            doc.LastSection.Add(table);
        }

        private static void SetOneRow([NotNull] ParagraphFormat format, [NotNull] Table table, [ItemNotNull] [NotNull] string[] values) {
            var row = table.AddRow();
            for (var i = 0; i < values.Length; i++) {
                var cell = row.Cells[i];
                cell.AddParagraph(values[i]);
                cell.Format = format.Clone();
            }
        }

        private static void SetPerDayTable([ItemNotNull] [NotNull] List<TotalEntry> totals, [NotNull] Document doc, [NotNull] Section sec) {
            MakeTable("Totals for each Loadtype per Day", sec, out ParagraphFormat format, out Table table, TableColumnType.Values);

            foreach (var entry in totals) {
                string[] values =
                    {entry.Name, entry.Perday.ToString("F2", CultureInfo.CurrentCulture), entry.UnitOfSum};
                SetOneRow(format, table, values);
            }
            doc.LastSection.Add(table);
        }

        private static void SetPerPersonDayTable([ItemNotNull] [NotNull] List<TotalEntry> totals, [NotNull] Document doc, [NotNull] Section sec) {
            MakeTable("Totals for each Loadtype per Person per Day", sec, out ParagraphFormat format, out Table table,
                TableColumnType.Values);

            foreach (var entry in totals) {
                string[] values =
                    {entry.Name, entry.PerPersonPerDay.ToString("F2", CultureInfo.CurrentCulture), entry.UnitOfSum};
                SetOneRow(format, table, values);
            }
            doc.LastSection.Add(table);
        }

        private static void SetPerPersonTable([ItemNotNull] [NotNull] List<TotalEntry> totals, [NotNull] Document doc, [NotNull] Section sec) {
            MakeTable("Totals for each Loadtype per Person", sec, out ParagraphFormat format, out Table table, TableColumnType.Values);

            foreach (var entry in totals) {
                string[] values =
                    {entry.Name, entry.PerPerson.ToString("F2", CultureInfo.CurrentCulture), entry.UnitOfSum};
                SetOneRow(format, table, values);
            }
            doc.LastSection.Add(table);
        }

        private static void SetSettlementEntries([NotNull] Document doc, [NotNull] Section sec, [NotNull] string filename, [NotNull] string csvCharacter) {
            MakeTable("Totals for each Loadtype", sec, out ParagraphFormat format, out Table table, TableColumnType.SettlementValues);

            var totals = TotalsReader(filename, true, csvCharacter);
            totals.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCulture));

            foreach (var entry in totals) {
                string[] values = {
                    entry.Household, entry.Name, entry.Value.ToString("F2", CultureInfo.CurrentCulture), entry.UnitOfSum
                };
                SetOneRow(format, table, values);
            }
            doc.LastSection.Add(table);
        }

        [ItemNotNull]
        [NotNull]
        private static List<TotalEntry> TotalsReader([NotNull] string filename, bool settlement, [NotNull] string csvCharacter) {
            var totals = new List<TotalEntry>();
            using (var sr = new StreamReader(filename)) {
                sr.ReadLine();
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("line from file was null");
                    }
                    var te = new TotalEntry(s, settlement, csvCharacter);
                    if (te.Name.ToUpperInvariant() != "NONE") {
                        totals.Add(te);
                    }
                }
            }
            return totals;
        }

        private enum TableColumnType {
            Values,
            SettlementValues,
            MinMax
        }

        private class TotalEntry {
            public TotalEntry([NotNull] string s, bool settlement, [NotNull] string csvCharacter) {
                string[] csvArr = {csvCharacter};
                var strarr = s.Split(csvArr, StringSplitOptions.None);
                if (strarr.Length > 3) {
                    if (settlement) {
                        Household = strarr[0];
                        Name = strarr[1];
                        Value = Convert.ToDouble(strarr[4], CultureInfo.CurrentCulture);
                        UnitOfSum = strarr[5];
                        Perday = Convert.ToDouble(strarr[6], CultureInfo.CurrentCulture);
                    }
                    else {
                        Name = strarr[0];
                        Value = Convert.ToDouble(strarr[3], CultureInfo.CurrentCulture);
                        UnitOfSum = strarr[4];
                        Perday = Convert.ToDouble(strarr[5], CultureInfo.CurrentCulture);
                        Minimum = Convert.ToDouble(strarr[7], CultureInfo.CurrentCulture);
                        Maximum = Convert.ToDouble(strarr[9], CultureInfo.CurrentCulture);
                        PerPerson = Convert.ToDouble(strarr[11], CultureInfo.CurrentCulture);
                        PerPersonPerDay = Convert.ToDouble(strarr[13], CultureInfo.CurrentCulture);
                        UnitOfPower = strarr[8];
                    }
                }
            }

            [NotNull]
            public string Household { get; } = "";
            public double Maximum { get; }
            public double Minimum { get; }
            [NotNull]
            public string Name { get; } = "";
            public double Perday { get; }
            public double PerPerson { get; }
            public double PerPersonPerDay { get; }
            [NotNull]
            public string UnitOfPower { get; } = "";
            [NotNull]
            public string UnitOfSum { get; } = "";

            public double Value { get; }
        }
    }
}