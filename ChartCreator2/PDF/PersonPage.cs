using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation.ResultFiles;
using JetBrains.Annotations;
using MigraDoc.DocumentObjectModel;

namespace ChartCreator2.PDF {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    internal class PersonPage : IPageCreatorToc {
        public void MakePage(Document doc, string dstdir, bool requireAll, List<string> pngFiles, Section tocSection) {
            var di = new DirectoryInfo(dstdir);
            var files = di.GetFiles("Persons.*.txt");
            if (files.Length == 0) {
                if (requireAll) {
                    throw new LPGException("Missing Persons files");
                }
                return;
            }
            var sec = doc.AddSection();
            // Add a single paragraph with some text and format information.
            var para = sec.AddParagraph();
            para.Format.Alignment = ParagraphAlignment.Justify;
            para.Format.Font.Name = "Times New Roman";
            para.Format.Font.Size = 20;
            para.Format.Font.Bold = true;
            para.Format.SpaceAfter = "1cm";
            para.Format.SpaceBefore = "1cm";
            para.Format.Font.Color = Colors.DarkGray;
            para.AddText("Persons");
            ChartPageBase.MakeTocEntry(tocSection, "Persons");
            para.AddBookmark("Persons");
            var format1 = new ParagraphFormat {Font = {Size = 14}, SpaceBefore = "0.2cm", SpaceAfter = "0.2cm", ListInfo = {ListType = ListType.BulletList1}};
            var format2 = new ParagraphFormat {Font = {Size = 12}, ListInfo = {ListType = ListType.BulletList2}, LeftIndent = "3cm"};
            foreach (var file in files) {
                var household = file.Name.Substring(file.Name.IndexOf(".", StringComparison.Ordinal) + 1);
                household = household.Substring(0, household.IndexOf(".", StringComparison.Ordinal));
                var para1 = sec.AddParagraph();
                para1.Format.Alignment = ParagraphAlignment.Left;
                para1.Format = format1.Clone();
                para1.AddText(household);
                var persons = PersonsReader(file.FullName);
                foreach (var person in persons) {
                    var para2 = sec.AddParagraph();
                    para2.Format.Alignment = ParagraphAlignment.Left;
                    para2.Format = format2.Clone();
                    para2.AddText(person);
                }
            }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private static List<string> PersonsReader([JetBrains.Annotations.NotNull] string filename) {
            var persons = new List<string>();
            using (var sr = new StreamReader(filename)) {
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    persons.Add(s);
                }
            }
            return persons;
        }
    }
}