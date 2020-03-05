using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using MigraDoc.DocumentObjectModel;

namespace ChartCreator2.PDF {
    internal class TimeProfilesPages : ChartPageBase {
        public TimeProfilesPages():base("These files show which time profiles were used for each device and how often. The content looks like this:",
            "Time Profiles", "TimeProfiles.*.txt", "Time Profiles") {
            MyTargetDirectory = TargetDirectory.Debugging;
        }

        protected override string GetGraphTitle(string filename) => throw new LPGNotImplementedException();

        public void MakePage([NotNull] Document doc, [NotNull] string dstdir, bool requireAll, [NotNull] Section tocSection) {
            var di =
                new DirectoryInfo(Path.Combine(dstdir, DirectoryNames.CalculateTargetdirectory(MyTargetDirectory)));
            var fi = di.GetFiles("TimeProfiles.*.txt");
            if (fi.Length == 0) {
                if (requireAll) {
                    throw new LPGException("Missing files for " + SectionTitle);
                }
                return;
            }
            var sec = MakeDescriptionArea(doc, tocSection);
            foreach (var fileInfo in fi) {
                var strings = new List<string>();
                using (var sr = new StreamReader(fileInfo.FullName)) {
                    var row = 0;
                    while (!sr.EndOfStream && row < 20) {
                        var s = sr.ReadLine();
                        strings.Add(s);
                        row++;
                    }
                }
                var sb = new StringBuilder();
                foreach (var s in strings) {
                    sb.Append(s + Environment.NewLine);
                }
                var para = sec.AddParagraph();
                para.Format.Alignment = ParagraphAlignment.Justify;
                para.Format.Font.Name = "Times New Roman";
                para.Format.Font.Size = 10;
                para.Format.Font.Bold = false;
                para.Format.SpaceAfter = "0.25cm";
                para.Format.Font.Color = Colors.Black;

                para.AddText(fileInfo.Name);

                para = sec.AddParagraph();
                para.Format.Alignment = ParagraphAlignment.Left;
                para.Format.Font.Name = "Times New Roman";
                para.Format.Font.Size = 10;
                para.Format.Font.Bold = false;
                para.Format.SpaceAfter = "0.25cm";
                para.Format.Font.Color = Colors.Black;

                para.AddText(sb.ToString());
            }
        }
    }
}