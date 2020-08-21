//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.IO;
////using System.Windows.Media.Imaging;
//using Automation.ResultFiles;
//using Common;
//using JetBrains.Annotations;
//using MigraDoc.DocumentObjectModel;

//namespace ChartCreator2.PDF {
//    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
//    internal abstract class ChartPageBase : IPageCreatorToc {
//        protected ChartPageBase([CanBeNull] string description, [CanBeNull] string fileName, [JetBrains.Annotations.NotNull] string pattern, [JetBrains.Annotations.NotNull] string sectionTitle)
//        {
//            Description = description;
//            FileName = fileName;
//            Pattern = pattern;
//            SectionTitle = sectionTitle;
//        }

//        [CanBeNull]
//        protected string Description { private get; set; }
//        [CanBeNull]
//        protected string FileName { private get; set; }
//        protected TargetDirectory MyTargetDirectory { get; set; } = TargetDirectory.Undefined;
//        [JetBrains.Annotations.NotNull]
//        protected string Pattern { get; set; }
//        [JetBrains.Annotations.NotNull]
//        protected string SectionTitle { get; set; }

//        public void MakePage(Document doc, string dstdir, bool requireAll, List<string> pngFiles, Section tocSection) {
//            if (MyTargetDirectory == TargetDirectory.Undefined) {
//                throw new LPGException("Undefined Target Directory in " + SectionTitle);
//            }
//            var di =
//                new DirectoryInfo(Path.Combine(dstdir, DirectoryNames.CalculateTargetdirectory(MyTargetDirectory)));
//            var files = di.GetFiles(Pattern);
//            if (files.Length == 0) {
//                if (requireAll) {
//                    throw new LPGException("Missing files for " + SectionTitle);
//                }
//                return;
//            }
//            var sec = MakeDescriptionArea(doc, tocSection);

//            foreach (var file in files) {
//                pngFiles.Remove(file.Name);
//                AddImageToSection(sec, file);
//                // img.WrapFormat
//            }
//        }

//        protected void AddImageToSection([JetBrains.Annotations.NotNull] Section sec, [JetBrains.Annotations.NotNull] FileInfo file, [CanBeNull] string title = "") {
//            var mytitle = GetGraphTitle(file.Name);
//            if (!string.IsNullOrEmpty(title)) {
//                mytitle = title;
//            }
//            var imgtitle = sec.AddParagraph(mytitle);
//            imgtitle.Format.Font.Size = 12;
//            imgtitle.Format.KeepWithNext = true;
//            imgtitle.Format.SpaceAfter = "0.5cm";
//            imgtitle.Format.SpaceBefore = "0.5cm";
//            imgtitle.Format.Font.Color = Colors.Blue;
//            Logger.Debug("Adding Image to pdf:" + file.FullName);
//            var img = sec.AddImage(file.FullName);
//            var size = GetDimensions(file.FullName);

//            if (size.Height > size.Width) {
//                img.Height = "10cm";
//            }
//            else {
//                img.Width = "16cm";
//            }
//        }

//        [JetBrains.Annotations.NotNull]
//        protected static Size GetDimensions([JetBrains.Annotations.NotNull] string fileName) {
//            //using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
//                //var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
//                //var width = bitmapFrame.PixelWidth;
//                //var height = bitmapFrame.PixelHeight;
//                //todo: properly look at the file size
//                return new Size(500, 500);
//            //}
//        }

//        [JetBrains.Annotations.NotNull]
//        protected abstract string GetGraphTitle([JetBrains.Annotations.NotNull] string filename);

//        [JetBrains.Annotations.NotNull]
//        protected Section MakeDescriptionArea([JetBrains.Annotations.NotNull] Document doc, [JetBrains.Annotations.NotNull] Section tocSection) {
//            var sec = doc.AddSection();
//            // Add a single paragraph with some text and format information.
//            var para = sec.AddParagraph();
//            para.Format.Alignment = ParagraphAlignment.Justify;
//            para.Format.Font.Name = "Times New Roman";
//            para.Format.Font.Size = 20;
//            para.Format.Font.Bold = true;
//            para.Format.SpaceAfter = "0.5cm";
//            para.Format.Font.Color = Colors.Black;
//            para.AddText(SectionTitle);
//            MakeTocEntry(tocSection, SectionTitle);
//            para.AddBookmark(SectionTitle);
//            para = sec.AddParagraph();
//            para.Format.Alignment = ParagraphAlignment.Justify;
//            para.Format.Font.Name = "Times New Roman";
//            para.Format.Font.Size = 12;
//            para.Format.Font.Bold = true;
//            para.Format.SpaceAfter = "0.25cm";
//            para.Format.Font.Color = Colors.Black;
//            para.AddText("This is made from the files starting with: " + FileName);

//            para = sec.AddParagraph();
//            para.Format.Alignment = ParagraphAlignment.Justify;
//            para.Format.Font.Name = "Times New Roman";
//            para.Format.Font.Size = 12;
//            para.Format.Font.Bold = false;
//            para.Format.SpaceAfter = "0.25cm";
//            para.Format.Font.Color = Colors.Black;
//            para.AddText(Description);
//            return sec;
//        }

//        public static void MakeTocEntry([CanBeNull] Section section, [JetBrains.Annotations.NotNull] string name) {
//            if (section == null) {
//                return;
//            }
//            var paragraph = section.AddParagraph();
//            paragraph.Style = "TOC";
//            var hyperlink = paragraph.AddHyperlink(name);
//            hyperlink.AddText(name + "\t");
//            hyperlink.AddPageRefField(name);
//        }

//        protected class Size {
//            public Size(double height, double width) {
//                Height = height;
//                Width = width;
//            }

//            public double Height { get; }
//            public double Width { get; }
//        }
//    }
//}