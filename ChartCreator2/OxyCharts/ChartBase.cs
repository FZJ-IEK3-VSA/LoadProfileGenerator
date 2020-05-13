using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using ChartPDFCreator;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using BarSeries = OxyPlot.Series.BarSeries;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using TextAnnotation = OxyPlot.Annotations.TextAnnotation;

namespace ChartCreator2.OxyCharts {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class IntervallBarMaker {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void MakeIntervalBars([JetBrains.Annotations.NotNull] ResultFileEntry srcResultFileEntry, [JetBrains.Annotations.NotNull] string plotName, [JetBrains.Annotations.NotNull] DirectoryInfo basisPath,
            [ItemNotNull] [JetBrains.Annotations.NotNull] List<Tuple<string, double>> consumption,
                                     [JetBrains.Annotations.NotNull] ChartTaggingSet taggingSet,
                                     [JetBrains.Annotations.NotNull] string newFileNameSuffix,
                                     bool showTitle,
                                     [JetBrains.Annotations.NotNull] GenericChartBase gcb)
        {
            var fontsize = 48;
            if (consumption.Count <= 20)
            {
                fontsize = 22;
            }
            if (consumption.Count > 20 && consumption.Count <= 30)
            {
                fontsize = 16;
            }
            if (consumption.Count > 30 && consumption.Count <= 40)
            {
                fontsize = 14;
            }
            if (consumption.Count > 40 && consumption.Count <= 50)
            {
                fontsize = 12;
            }
            if (consumption.Count > 50)
            {
                fontsize = 10;
            }
            if (!Config.MakePDFCharts)
            {
                fontsize = (int)(fontsize * 0.8);
            }
            var unit = "min";
            var xaxislabel = "Time Consumption in Percent";
            if (srcResultFileEntry.LoadTypeInformation != null)
            {
                var lti = srcResultFileEntry.LoadTypeInformation;
                if (!lti.ShowInCharts)
                {
                    return;
                }
                unit = lti.UnitOfSum;
                xaxislabel = lti.Name + " in Percent";
            }
            consumption.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            OxyPalette p;
            if (consumption.Count > 1)
            {
                if (taggingSet.Categories.Count > 1)
                {
                    p = OxyPalettes.HueDistinct(taggingSet.Categories.Count);
                }
                else
                {
                    p = OxyPalettes.Hue64;
                }
            }
            else
            {
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
                LegendSymbolMargin = 25
            };
            if (showTitle)
            {
                plotModel1.Title = plotName;
            }
            if (Config.MakePDFCharts)
            {
                plotModel1.DefaultFontSize = fontsize;
            }
            var ca = new CategoryAxis
            {
                Position = AxisPosition.Left,
                GapWidth = 0,
                MaximumPadding = 0.03,
                MajorTickSize = 0
            };
            plotModel1.Axes.Add(ca);
            var la = new LinearAxis
            {
                Minimum = 0,
                MinimumPadding = 0,
                Title = ChartLocalizer.Get().GetTranslation(xaxislabel),
                Position = AxisPosition.Bottom,
                MinorTickSize = 0
            };
            plotModel1.Axes.Add(la);
            var caSub = new CategoryAxis
            {
                StartPosition = 0.5,
                EndPosition = 1,
                Position = AxisPosition.Left,
                Key = "Sub",
                GapWidth = 0.3,
                MajorTickSize = 0,
                MinorTickSize = 0
            };
            plotModel1.Axes.Add(caSub);
            double runningSum = 0;
            var row = 0;
            var sum = consumption.Select(x => x.Item2).Sum();
            var allBarSeries = new Dictionary<string, IntervalBarSeries>();
            var ba = new BarSeries
            {
                YAxisKey = "Sub",
                LabelFormatString = "{0:N1} %"
            };
            foreach (var s in taggingSet.Categories)
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
                var bai = new BarItem(percent)
                {
                    Color = p.Colors[coloridx]
                };
                ba.Items.Add(bai);
            }
            plotModel1.Series.Add(ba);
            foreach (var tuple in consumption)
            {
                var percentage = tuple.Item2 / sum * 100;
                var name = ChartLocalizer.Get().GetTranslation(tuple.Item1.Trim());
                if (name.Length > 100)
                {
                    name = name.Substring(0, 97) + "...";
                }
                var textAnnotation1 = new TextAnnotation
                {
                    StrokeThickness = 0,
                    FontSize = fontsize,
                    Padding = new OxyThickness(10, 0, 10, 0)
                };
                var txtValue = tuple.Item2.ToString("N1", CultureInfo.CurrentCulture);
                if (srcResultFileEntry.LoadTypeInformation == null)
                {
                    var ts = TimeSpan.FromMinutes(tuple.Item2);
                    txtValue = ts.ToString();
                }
                textAnnotation1.Text = " " + name + " (" + txtValue + " " + unit + ", " +
                                       (tuple.Item2 / sum * 100).ToString("N1", CultureInfo.CurrentCulture) + " %)   ";
                if (runningSum < 50)
                {
                    textAnnotation1.TextHorizontalAlignment = HorizontalAlignment.Left;
                    textAnnotation1.TextPosition = new DataPoint(runningSum + percentage, row - 0.6);
                }
                else
                {
                    textAnnotation1.TextPosition = new DataPoint(runningSum, row - 0.5);
                    textAnnotation1.TextHorizontalAlignment = HorizontalAlignment.Right;
                }
                plotModel1.Annotations.Add(textAnnotation1);
                var item = new IntervalBarItem(runningSum, runningSum + percentage);
                var category = taggingSet.AffordanceToCategories[tuple.Item1];
                allBarSeries[category].Items.Add(item);
                foreach (var pair in allBarSeries)
                {
                    if (pair.Key != category)
                    {
                        pair.Value.Items.Add(new IntervalBarItem(0, 0));
                    }
                }
                ca.Labels.Add(string.Empty);
                runningSum += percentage;
                row++;
            }
            foreach (var pair in allBarSeries)
            {
                plotModel1.Series.Add(pair.Value);
            }
            gcb.Save(plotModel1, plotName, srcResultFileEntry.FullFileName + newFileNameSuffix, basisPath); // ".interval"
        }
    }
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public abstract class GenericChartBase  {
        protected GenericChartBase([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                   [JetBrains.Annotations.NotNull] string stepName,
                                   FileProcessingResult shouldHaveProducedFiles,
                                   [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft)
        {
            _parameters = parameters;
            StepName = stepName;
            ShouldHaveProducedFiles = shouldHaveProducedFiles;
            FFT = fft;
        }

        [JetBrains.Annotations.NotNull]
        public string StepName { get; }
        public FileProcessingResult ShouldHaveProducedFiles { get; }
        [JetBrains.Annotations.NotNull]
        protected FileFactoryAndTracker FFT { get; }

        [NotNull]
        protected ChartCreationParameters Parameters => _parameters;

        [JetBrains.Annotations.NotNull] private readonly ChartCreationParameters _parameters;
        public void Save([JetBrains.Annotations.NotNull] PlotModel plotModel1, [JetBrains.Annotations.NotNull] string plotName, [JetBrains.Annotations.NotNull] string csvFullFileName, [JetBrains.Annotations.NotNull] DirectoryInfo basisPath,
           [CanBeNull] string newDstFileName = null, bool makePng = true)
        {
            if (plotName.Contains("\\") || plotName.Trim().Length == 0 || plotName.Contains("."))
            {
                throw new LPGException("Plotname is messed up. Please report!");
            }
            var fiCsv = new FileInfo(csvFullFileName.Replace("/", " "));
            var destinationFileName = fiCsv.Name;
            if (newDstFileName != null)
            {
                destinationFileName = newDstFileName;
            }
            destinationFileName = destinationFileName.Replace(".csv", string.Empty);
            destinationFileName += ".png";
            destinationFileName = AutomationUtili.CleanFileName(destinationFileName);
            if (fiCsv.DirectoryName == null)
            {
                throw new LPGException("Directory name was null. This is a bug. Please report.");
            }
            var fi = new FileInfo(Path.Combine(basisPath.FullName, "Charts", destinationFileName));
            if (makePng)
            {
                if (fi.Exists)
                {
                    throw new LPGException("File already exists?!? " + fi.FullName);
                }
                FFT.RegisterFile(fi.Name, "Plot for " + plotName, true, ResultFileID.Chart, Constants.GeneralHouseholdKey, TargetDirectory.Charts, fi.Name);
                PngExporter.Export(plotModel1, fi.FullName, _parameters.Width,
                    _parameters.Height, OxyColor.FromArgb(255, 255, 255, 255),
                    _parameters.Dpi);
            }
            if (Config.MakePDFCharts)
            {
                var pdfChartName = fi.FullName.Substring(0, fi.FullName.Length - 4);
                pdfChartName += ".pdf";
                FFT.RegisterFile(pdfChartName, "PDF Plot for " + plotName, true, ResultFileID.Chart, Constants.GeneralHouseholdKey, TargetDirectory.Charts, pdfChartName);
                OxyPDFCreator.Run(plotModel1, pdfChartName);
            }
        }
    }
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public abstract class ChartBaseFileStep: GenericChartBase, IChartMakerStep
    {
        [JetBrains.Annotations.NotNull] private readonly ICalculationProfiler _calculationProfiler;
        protected ChartBaseFileStep([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                            [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                            [JetBrains.Annotations.NotNull] List<ResultFileID> validResultFileIds, [JetBrains.Annotations.NotNull] string stepName,
                            FileProcessingResult shouldHaveProducedFiles):base(parameters,stepName,shouldHaveProducedFiles,fft)
        {
            _calculationProfiler = calculationProfiler;
            ResultFileIDs = validResultFileIds;
        }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        protected abstract FileProcessingResult MakeOnePlot([JetBrains.Annotations.NotNull] ResultFileEntry srcResultFileEntry);

        public FileProcessingResult MakePlot([JetBrains.Annotations.NotNull] ResultFileEntry srcResultFileEntry)
        {
            _calculationProfiler.StartPart(StepName);
            var result = MakeOnePlot(srcResultFileEntry);
            _calculationProfiler.StopPart(StepName);
            return result;
        }

        [JetBrains.Annotations.NotNull]
        public List<ResultFileID> ResultFileIDs { get; }

        [NotNull]
        protected ICalculationProfiler Profiler => _calculationProfiler;

        public bool IsEnabled([JetBrains.Annotations.NotNull] ResultFileEntry resultFileEntry)
        {
            if (ResultFileIDs.Contains(resultFileEntry.ResultFileID)) {
                return true;
            }
            return false;
        }
    }
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public abstract class ChartBaseSqlStep : GenericChartBase, ISqlChartMakerStep
    {
        [JetBrains.Annotations.NotNull] private readonly ICalculationProfiler _calculationProfiler;
        [JetBrains.Annotations.NotNull]
        public List<ResultTableID> ValidResultTableIds { get; }
        [JetBrains.Annotations.NotNull]
        public CalcDataRepository CalcDataRepository { get; }

        [NotNull]
        public ICalculationProfiler CalculationProfiler => _calculationProfiler;

        protected ChartBaseSqlStep([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                   [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                   [JetBrains.Annotations.NotNull] List<ResultTableID> validResultTableIds, [JetBrains.Annotations.NotNull] string stepName,
                                   FileProcessingResult shouldHaveProducedFiles,
                                   [JetBrains.Annotations.NotNull] CalcDataRepository calcDataRepository) : base(parameters, stepName, shouldHaveProducedFiles, fft)
        {
            _calculationProfiler = calculationProfiler;
            ValidResultTableIds = validResultTableIds;
            CalcDataRepository = calcDataRepository;
        }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        protected abstract FileProcessingResult MakeOnePlot([JetBrains.Annotations.NotNull] HouseholdKeyEntry hhkey);

        public FileProcessingResult MakePlot([JetBrains.Annotations.NotNull] HouseholdKeyEntry hhkey)
        {
            _calculationProfiler.StartPart(StepName);
            var result = MakeOnePlot(hhkey);
            _calculationProfiler.StopPart(StepName);
            return result;
        }

        public bool IsEnabled([JetBrains.Annotations.NotNull] HouseholdKeyEntry householdKey, [JetBrains.Annotations.NotNull] ResultTableDefinition resultTableDefinition)
        {
            if (ValidResultTableIds.Contains(resultTableDefinition.ResultTableID))
            {
                return true;
            }
            return false;
        }
    }
}