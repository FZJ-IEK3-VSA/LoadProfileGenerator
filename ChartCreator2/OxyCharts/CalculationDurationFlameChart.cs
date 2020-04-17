using System.Collections.Generic;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using TextAnnotation = OxyPlot.Annotations.TextAnnotation;

namespace ChartCreator2.OxyCharts {
    public class CalculationDurationFlameChart {
        //private static int _compressedItems;

        //private static int _maxLevel;

        [NotNull] private readonly Dictionary<int, double> _textOffsets = new Dictionary<int, double>();

        [NotNull] [ItemNotNull] private readonly List<CalculationProfiler.ProgramPart> _parts =new List<CalculationProfiler.ProgramPart>();

        private void AddBars([NotNull] CalculationProfiler.ProgramPart part, int row, double offset, int fontsize,
            [NotNull] Dictionary<int, IntervalBarSeries> itemsByLevel, [NotNull] OxyPalette palette, [NotNull] PlotModel pm)
        {
            var runningsum = offset;
            for (var i = 0; i < part.Children.Count; i++) {
                var programPart = part.Children[i];
                AddBars(programPart, row + 1, runningsum, fontsize, itemsByLevel, palette, pm);
                runningsum += programPart.Duration2;
            }

            //bar
            var item = new IntervalBarItem(offset, offset + part.Duration2)
            {
                Color = palette.Colors[_parts.IndexOf(part)]
            };

            if (!itemsByLevel.ContainsKey(1)) {
                var series = new IntervalBarSeries
                {
                    FontSize = fontsize
                };
                itemsByLevel.Add(1, series);
            }
            var ibs = new IntervalBarSeries();

            for (var i = 0; i < row; i++) {
                ibs.Items.Add(new IntervalBarItem(0, 0, ""));
            }
            ibs.StrokeThickness = 0.1;
            ibs.Items.Add(item);
            pm.Series.Add(ibs);
            //  item.Title = name;

            //annotation
            if (string.IsNullOrWhiteSpace(part.Key)) {
                throw new LPGException("Empty profiler key");
            }
            var name = part.Key;
            if (name.Length > 100) {
                name = name.Substring(0, 97) + "...";
            }
            var textAnnotation1 = new TextAnnotation
            {
                StrokeThickness = 0,
                FontSize = 6,
                Padding = new OxyThickness(10, 0, 10, 0)
            };
            var txtValue = name + " - " + part.Duration2.ToString("N1") + "s";

            textAnnotation1.Text = txtValue;

            textAnnotation1.TextHorizontalAlignment = HorizontalAlignment.Left;
            textAnnotation1.TextPosition = new DataPoint(offset, row + GetOffset(row));

            pm.Annotations.Add(textAnnotation1);
        }

        private void InitPartsList([NotNull] CalculationProfiler.ProgramPart part)
        {
            if (_parts.Contains(part))
            {
                throw new LPGException("Fail! Part already in list");
            }
            _parts.Add(part);

            foreach (var child in part.Children)
            {
                InitPartsList(child);
            }
        }

        /*
        private int CountLevels(CalculationProfiler.ProgramPart part, int level)
        {
            if (_parts.Contains(part)) {
                throw new LPGException("Fail! Part already in list");
            }
            _parts.Add(part);
            if (level > _maxLevel) {
                _maxLevel = level;
            }
            foreach (var child in part.Children) {
                CountLevels(child, level + 1);
            }
            return _maxLevel;
        }*/
        /*
        private int GetFontsize(CalculationProfiler.ProgramPart mainPart)
        {
            var maxlevel = CountLevels(mainPart, 0);
            var fontsize = 48;
            if (maxlevel <= 20) {
                fontsize = 22;
            }
            if (maxlevel <= 30) {
                fontsize = 16;
            }
            if (maxlevel <= 40) {
                fontsize = 14;
            }
            if (maxlevel <= 50) {
                fontsize = 12;
            }
            if (maxlevel > 50) {
                fontsize = 10;
            }
            return fontsize;
        }*/

        private double GetOffset(int row)
        {
            if (!_textOffsets.ContainsKey(row)) {
                _textOffsets.Add(row, -0.4);
            }
            var value = _textOffsets[row];
            if (value < 0.4) {
                _textOffsets[row] += 0.1;
            }
            else {
                _textOffsets[row] = -0.4;
            }
            return value;
        }

        //convert all durations to double for later adding
        private static void InitializeDuration2([NotNull] CalculationProfiler.ProgramPart part)
        {
            part.Duration2 = part.Duration.TotalSeconds;
            foreach (var child in part.Children) {
                InitializeDuration2(child);
            }
        }

        /// <summary>
        ///     Merges the children of a program part to make the plot slightly less confusing.
        /// </summary>
        /// <param name="part">the program part</param>
        private static void MergeAndCompress([NotNull] CalculationProfiler.ProgramPart part)
        {
            //part.Children.Sort((x,y)=> String.Compare(x.Key, y.Key, StringComparison.Ordinal));
            for (var i = 0; i < part.Children.Count; i++) {
                var first = part.Children[i];
                for (var j = i + 1; j < part.Children.Count; j++) {
                    if (first.Key == part.Children[j].Key) {
                        //merge
                        //_compressedItems++;
                        var second = part.Children[j];
                        first.Duration2 += second.Duration2;
                        first.Children.AddRange(second.Children);
                        part.Children.Remove(second);
                        i = -1;
                        break;
                    }
                }
            }
            foreach (var child in part.Children) {
                MergeAndCompress(child);
            }
        }

        public void Run([NotNull] CalculationProfiler cp, [NotNull] string outputDirectory, [NotNull] string source)
        {
            //var cp =  CalculationProfiler.Read(@"C:\work\CalculationBenchmarks.ActionCarpetPlotTest\");

            InitializeDuration2(cp.MainPart);
            MergeAndCompress(cp.MainPart);
            InitPartsList(cp.MainPart);
            const int fontsize = 6;// = GetFontsize(cp.MainPart);
            //const string xaxislabel = "Time Consumption in CPUSeconds";

            OxyPalette p;
            if (_parts.Count > 1) {
                p = OxyPalettes.HueDistinct(_parts.Count);
            }
            else {
                p = OxyPalettes.Hue64;
            }
            var plotModel1 = new PlotModel
            {
                LegendBorderThickness = 0,
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,
                PlotAreaBorderColor = OxyColors.White,
                LegendFontSize = fontsize,
                LegendSymbolMargin = 25,
                DefaultFontSize = fontsize
            };

            var ca = new CategoryAxis
            {
                Position = AxisPosition.Left,
                GapWidth = 0,
                MaximumPadding = 0.03,
                MajorTickSize = 0
            };
            plotModel1.Axes.Add(ca);
           /* var la = new LinearAxis
            {
                Minimum = 0,
                MinimumPadding = 0,
                Title = ChartLocalizer.Get().GetTranslation(xaxislabel),
                Position = AxisPosition.Bottom,
                MinorTickSize = 0
            };*/
            /*  plotModel1.Axes.Add(la);
              var caSub = new CategoryAxis();
              caSub.StartPosition = 0.5;
              caSub.EndPosition = 1;
              caSub.Position = AxisPosition.Left;
              caSub.Key = "Sub";
              caSub.GapWidth = 0.3;
              caSub.MajorTickSize = 0;
              caSub.MinorTickSize = 0;
              plotModel1.Axes.Add(caSub);*/
            //const double runningSum = 0;
            //var row = 0;

            // var allBarSeries = new Dictionary<string, IntervalBarSeries>();
            //var ba = new BarSeries();
            //ba.YAxisKey = "Sub";
            //ba.LabelFormatString = "{0:N1} %";
            /*  foreach (var s in taggingSet.Categories)
              {
                  caSub.Labels.Add(ChartLocalizer.Get().GetTranslation(s));
                  var ibs = new IntervalBarSeries();
                  // ibs.Title =
                  var coloridx = taggingSet.GetCategoryIndexOfCategory(s);
                  ibs.FillColor = p.Colors[coloridx];
                  ibs.StrokeThickness = 0;
                  ibs.FontSize = fontsize;
                  allBarSeries.Add(s, ibs);
                  double categorysum = 0;
                  foreach (var tuple in consumption)
                  {
                      if (taggingSet.AffordanceToCategories[tuple.Item1] == s)
                      {
                          categorysum += tuple.Item2;
                      }
                  }
                  var percent = categorysum / sum * 100;
                  var bai = new BarItem(percent);
                  bai.Color = p.Colors[coloridx];
                  ba.Items.Add(bai);
              }*/
            //   plotModel1.Series.Add(ba);

            var itemsByLevel = new Dictionary<int, IntervalBarSeries>();
            _textOffsets.Clear();
            AddBars(cp.MainPart, 0, 0, fontsize, itemsByLevel, p, plotModel1);
            //        foreach (IntervalBarSeries series in itemsByLevel.Values) {
            //          plotModel1.Series.Add(series);
            //    }
            string dstFileName = Path.Combine(outputDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Charts), "CalculationDurationFlameChart."+ source+".Png");
            PngExporter.Export(plotModel1, dstFileName, 3200, 2000, OxyColor.FromArgb(255, 255, 255, 255),
                144);
            //Save(plotModel1, plotName, srcEntry.FullFileName + newFileNameSuffix, basisPath); // ".interval"
        }
    }
}