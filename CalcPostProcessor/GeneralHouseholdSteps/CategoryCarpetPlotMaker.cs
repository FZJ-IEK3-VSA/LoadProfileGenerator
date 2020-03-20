//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using Automation.ResultFiles;
//using CalcPostProcessor.Helper;
//using Common;
//using Common.JSON;
//using JetBrains.Annotations;
//using Brush = System.Windows.Media.Brush;
//using Brushes = System.Windows.Media.Brushes;
//using Color = System.Windows.Media.Color;
//using Pen = System.Windows.Media.Pen;
//using Point = System.Windows.Point;

//namespace CalcPostProcessor.GeneralHouseholdSteps
//{
//    public class CarpetCategoryEntry {
//        public CarpetCategoryEntry([JetBrains.Annotations.NotNull] TimeStep time, [CanBeNull] string category, Color color, bool addSicknessMarker)
//        {
//            Time = time;
//            Category = category;
//            Color = color;
//            AddSicknessMarker = addSicknessMarker;
//        }
//        public bool AddSicknessMarker { get; }
//        [JetBrains.Annotations.NotNull]
//        public TimeStep Time { get; }
//        [CanBeNull]
//        public string Category { get; }
//        public Color Color { get; }
//    }
//    public class CategoryCarpetPlotMaker {
//        [JetBrains.Annotations.NotNull] private readonly CalcParameters _calcParameters;

//        public CategoryCarpetPlotMaker([JetBrains.Annotations.NotNull] CalcParameters calcParameters)
//        {
//            _calcParameters = calcParameters;
//        }

//        [JetBrains.Annotations.NotNull]
//        private RenderTargetBitmap AddAxisTexts([JetBrains.Annotations.NotNull] BitmapSource bmp, int numberofStepsPerDay, int colWidth)
//        {
//            const int fontsize = 22;
//            var bmpSource = bmp;
//            var drawingVisual = new DrawingVisual();
//            var drawingContext = drawingVisual.RenderOpen();
//            const int xOffset = 70;
//            const int ySpace = 150;

//            var targetwidth = (int)bmp.Width + xOffset;
//            var targetheight = (int)bmp.Height + ySpace;
//            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, targetwidth, targetheight));
//            drawingContext.DrawImage(bmpSource, new Rect(xOffset, 0, bmp.Width, bmp.Height));
//            var c = Color.FromRgb(0, 0, 0);
//            Brush b = new SolidColorBrush(c);
//            var tfarr = new Typeface[Fonts.SystemTypefaces.Count];
//            Fonts.SystemTypefaces.CopyTo(tfarr, 0);
//            var typeFace = tfarr.First(x => x.FontFamily.Source == "Arial");
//            var oneHour = numberofStepsPerDay / 24.0;
//            for (var i = 0; i < 24; i += 2)
//            {
//                var text = new FormattedText(i + ":00", new CultureInfo("en-us"), FlowDirection.LeftToRight,
//                    typeFace, fontsize, b, 1);
//                drawingContext.DrawText(text, new Point(2, oneHour * i));
//            }

//            var starttime = _calcParameters.OfficialStartTime;
//            var endTime = _calcParameters.OfficialEndTime;
//            var rotateTransform = new RotateTransform
//            {
//                Angle = -90
//            };
//            drawingContext.PushTransform(rotateTransform);
//            var dateCount = 0;
//            //double textwidth = 0;
//            while (starttime < endTime)
//            {
//                var text = new FormattedText(starttime.ToShortDateString(), new CultureInfo("en-us"),
//                    FlowDirection.LeftToRight, typeFace, fontsize, b, 1);
//                //textwidth = text.Width;
//                double normalxPos = xOffset + dateCount * colWidth;
//                var normalyPos = bmp.Height + text.Width + 10;
//                var newxPos = normalyPos * -1;
//                var newypos = normalxPos;
//                drawingContext.DrawText(text, new Point(newxPos, newypos));
//                dateCount += (int)(starttime.AddMonths(1) - starttime).TotalDays;
//                starttime = starttime.AddMonths(1);
//            }

//            drawingContext.Pop();
//            //var square = Convert.ToChar(9632).ToString();
//            /*if (taggingSet != null)
//            {
//                double xpos = xOffset;
//                var ypos = bmp.Height + textwidth + 15;
//                if (!taggingSet.Colors.ContainsKey("Vacation"))
//                {
//                    taggingSet.Colors.Add("Vacation", Color.FromRgb(2, 200, 200));
//                }

//                var colorList = taggingSet.Colors.Where(x => x.Key != "none").Select(x => {
//                    var transText = ChartLocalizer.Get().GetTranslation(x.Key);
//                    return new Tuple<string, Color>(transText, x.Value);
//                }).ToList();
//                colorList.Sort((x, y) => string.Compare(x.Item1, y.Item1, StringComparison.Ordinal));
//                foreach (var color in colorList)
//                {
//                    Brush colorBrush = new SolidColorBrush(color.Item2);
//                    var squaretext = new FormattedText(square, new CultureInfo("en-us"),
//                        FlowDirection.LeftToRight, typeFace, 75, colorBrush, 1);
//                    drawingContext.DrawText(squaretext, new Point(xpos, ypos - 25));
//                    xpos += squaretext.Width + 5;
//                    var text = new FormattedText(color.Item1, new CultureInfo("en-us"),
//                        FlowDirection.LeftToRight, typeFace, fontsize, b, 1);
//                    drawingContext.DrawText(text, new Point(xpos, ypos + 15));
//                    xpos += text.Width + 15;
//                }
//            }*/

//            drawingContext.Close();
//            var rendertargetbmp = new RenderTargetBitmap(targetwidth, targetheight, 96, 96,
//                PixelFormats.Pbgra32);
//            rendertargetbmp.Render(drawingVisual);
//            return rendertargetbmp;
//        }

//        public void MakeCarpet([JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
//                                [JetBrains.Annotations.NotNull] Dictionary<int, CarpetCategoryEntry> carpetCategoryEntries,
//                                 TimeSpan officialSimulationDuration, [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
//                                int colwidth, [JetBrains.Annotations.NotNull] string dstFilename,
//                               [JetBrains.Annotations.NotNull] string fileDescription, int resolutionMultiplier,
//                               [JetBrains.Annotations.NotNull] string legendFilename, [JetBrains.Annotations.NotNull] string legendDescription,
//                               [JetBrains.Annotations.NotNull] string chartIndex)
//        {
//            var numberofStepsPerDay =
//                (int)
//                (new TimeSpan(1, 0, 0, 0).TotalSeconds / _calcParameters.InternalStepsize
//                     .TotalSeconds);
//            if (numberofStepsPerDay > 40000)
//            {
//                Logger.Error("No carpet plots for time resolutions with more than 40.000 steps per day can be created!");
//                return;
//            }

//            var row = 0;
//            var col = 0;
//            Logger.Info("Setting the pixels for " + dstFilename + "...");

//            //var descriptionDict = new Dictionary<Point, string>();
//                using (var bmp = new BitmapPreparer((int)(officialSimulationDuration.TotalDays + 1) * colwidth, numberofStepsPerDay)) {
//                    for (int i = 0; i < _calcParameters.OfficalTimesteps; i++) {
//                        row++;
//                        if (row == numberofStepsPerDay)
//                        {
//                            row = 0;
//                            col++;
//                        }

//                        Color c = Colors.Black;
//                        if (carpetCategoryEntries.ContainsKey(i)) {
//                            var entry = carpetCategoryEntries[i];
//                            c = entry.Color;
//                            if (entry.AddSicknessMarker)
//                            {
//                                // add little white line for sickness
//                                c = Color.FromRgb(255, 255, 255);
//                                bmp.SetPixel(col * colwidth + 0, row, c);
//                                bmp.SetPixel(col * colwidth + 1, row, c);
//                                bmp.SetPixel(col * colwidth + colwidth - 1, row, c);
//                                bmp.SetPixel(col * colwidth + colwidth, row, c);
//                            }
//                            //descriptionDict.Add(new Point(col * colwidth, row), time.Ae.AffordanceName);
//                        }
//                        for (var colwidthIdx = 0; colwidthIdx < colwidth; colwidthIdx++)
//                        {
//                            bmp.SetPixel(col * colwidth + colwidthIdx, row, c);
//                        }
//                    }

//                    var renderbmp = AddAxisTexts(bmp.GetBitmap(), numberofStepsPerDay, colwidth);
//                    Logger.Info("Saving the carpet plot for " + dstFilename + "...");
//                    using (
//                        var fs =
//                            fft.MakeFile<Stream>(
//                                dstFilename, fileDescription, true,
//                                ResultFileID.CarpetPlots, householdKey, TargetDirectory.Charts,
//                                _calcParameters.InternalStepsize, null, null,chartIndex))
//                    {
//                        var encoder = new PngBitmapEncoder();
//                        encoder.Frames.Add(BitmapFrame.Create(renderbmp));
//                        encoder.Save(fs);
//                    }
//                }

//            var legendEntries = MakeLegendEntries(carpetCategoryEntries);
//            if(legendEntries.Count > 0) {
//                MakeLegend(fft,legendEntries,householdKey,resolutionMultiplier,legendFilename,legendDescription,
//                    chartIndex);
//            }
//        }

//        [ItemNotNull]
//        [JetBrains.Annotations.NotNull]
//        private List<Tuple<string, Color>> MakeLegendEntries([JetBrains.Annotations.NotNull] Dictionary<int, CarpetCategoryEntry> carpetCategoryEntries)
//        {
//            List<Tuple<string, Color>> legendEntries = new List<Tuple<string, Color>>();
//            Dictionary<string, Color> colorsByDesc = new Dictionary<string, Color>();
//            foreach (var pair in carpetCategoryEntries) {
//                if (pair.Value.Category != null &&
//                    !colorsByDesc.ContainsKey(pair.Value.Category)) {
//                    colorsByDesc.Add(pair.Value.Category,pair.Value.Color);
//                }
//            }

//            foreach (KeyValuePair<string, Color> pair in colorsByDesc) {
//                legendEntries.Add(new Tuple<string, Color>(pair.Key,pair.Value));
//            }
//            return legendEntries;
//        }

//        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
//        private void MakeLegend([JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
//                                [ItemNotNull] [JetBrains.Annotations.NotNull] List<Tuple<string, Color>> entries,
//                               [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
//                               int resolutionMultiplier, [JetBrains.Annotations.NotNull] string legendfilename,
//                                [JetBrains.Annotations.NotNull] string legendDescription,
//                                [JetBrains.Annotations.NotNull] string additionalFileIndex)
//        {
//            var tfarr = new Typeface[Fonts.SystemTypefaces.Count];
//            Fonts.SystemTypefaces.CopyTo(tfarr, 0);

//            var tf = tfarr.First(x => x.FontFamily.Source == "Arial");
//            var drawingVisual = new DrawingVisual();
//            var drawingContext = drawingVisual.RenderOpen();
//            var legendrow = 2;

//            entries.Sort(Comparison);
//            Brush white = Brushes.White;
//            var whitepen = new Pen(white, 0);
//            foreach (var nameColor in entries)
//            {
//                Brush b = new SolidColorBrush(nameColor.Item2);
//                var square = Convert.ToChar(9632).ToString();

//                var squaretext = new FormattedText(square, new CultureInfo("en-us"), FlowDirection.LeftToRight,
//                    tf, 50, b, 1);
//                var text = new FormattedText(nameColor.Item1, new CultureInfo("en-us"),
//                    FlowDirection.LeftToRight, tf, 28.0, Brushes.Black, 1);
//                drawingContext.DrawRectangle(white, whitepen, new Rect(0, legendrow, 1200, text.Height));
//                drawingContext.DrawText(squaretext, new Point(2, legendrow - 20));
//                drawingContext.DrawText(text, new Point(40, legendrow));
//                legendrow += 40;
//            }

//            drawingContext.Close();
//            var bmp = new RenderTargetBitmap(1200 * resolutionMultiplier,
//                entries.Count * 40 * resolutionMultiplier, 96 * resolutionMultiplier,
//                96 * resolutionMultiplier,
//                PixelFormats.Pbgra32);
//            bmp.Render(drawingVisual);
//            using (
//                var fslegend =
//                    fft.MakeFile<Stream>(legendfilename,
//                        legendDescription, true,
//                        ResultFileID.CarpetPlotsLegend, householdKey, TargetDirectory.Charts,
//                        _calcParameters.InternalStepsize,additionalFileIndex: additionalFileIndex))
//            {
//                var encoderLegend = new PngBitmapEncoder();
//                encoderLegend.Frames.Add(BitmapFrame.Create(bmp));
//                encoderLegend.Save(fslegend);
//            }
//        }

//        private static int Comparison([JetBrains.Annotations.NotNull] Tuple<string, Color> t1, [JetBrains.Annotations.NotNull] Tuple<string, Color> t2)
//        {
//            var b1 = 0.2126 * t1.Item2.R + 0.7152 * t1.Item2.G + 0.0722 * t1.Item2.B;
//            var b2 = 0.2126 * t2.Item2.R + 0.7152 * t2.Item2.G + 0.0722 * t2.Item2.B;
//            return b1.CompareTo(b2);
//        }
//    }
//}
