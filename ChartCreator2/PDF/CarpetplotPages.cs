using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation.ResultFiles;
using Common;
using MigraDoc.DocumentObjectModel;

namespace ChartCreator2.PDF {
    internal class CarpetplotPages : ChartPageBase {
        public CarpetplotPages():base("The carpet plots show the activities of the people and give a quick overview " +
                                      " if everything is working correctly.", "Carpetplot", "Carpetplot.*.7.png",
            "Carpet plot of the activities per person"
            )
        {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [JetBrains.Annotations.NotNull]
        protected override string GetGraphTitle([JetBrains.Annotations.NotNull] string filename) {
            var str = filename.Split('.');
            var hh = str[1];
            var taggingset = str[2];
            return hh + " -  " + taggingset;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void MakePage([JetBrains.Annotations.NotNull] Document doc, [JetBrains.Annotations.NotNull] string dstdir, bool requireAll, [JetBrains.Annotations.NotNull] Section tocSection) {
            var di =
                new DirectoryInfo(Path.Combine(dstdir, DirectoryNames.CalculateTargetdirectory(MyTargetDirectory)));
            var files = di.GetFiles(Pattern);
            if (files.Length == 0) {
                if (requireAll) {
                    throw new LPGException("Missing Carpet plots");
                }
                return;
            }
            var sec = MakeDescriptionArea(doc, tocSection);

            foreach (var file in files) {
                AddImageToSection(sec, file);
                if (file.Name.Contains("Carpetplot.")) {
                    var legendFileName = file.Name.Replace("Carpetplot.", "CarpetplotLegend.").Replace(".7.", ".");
                    if(file.DirectoryName == null) {
                        throw new LPGException("Directory Name was null");
                    }

                    var legendFile = new FileInfo(Path.Combine(file.DirectoryName, legendFileName));
                    if (legendFile.Exists) {
                        AddImageToSection(sec, legendFile, "Legend for the previous carpet plot");
                    }
                }
                // img.WrapFormat
            }
        }
    }
}