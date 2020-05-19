using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    internal class AffordanceEnergyUsePerPerson : ChartBaseSqlStep {
        public AffordanceEnergyUsePerPerson([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                            [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                            [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                            [JetBrains.Annotations.NotNull] CalcDataRepository calcDataRepository) : base(parameters,
            fft,
            calculationProfiler,
            new List<ResultTableID> {ResultTableID.AffordanceEnergyUse},
            "Affordance Energy Use Per Person",
            FileProcessingResult.ShouldCreateFiles,
            calcDataRepository)
        {
        }

        protected override FileProcessingResult MakeOnePlot(HouseholdKeyEntry hhkey)
        {
            CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var entries = CalcDataRepository.LoadAffordanceEnergyUses(hhkey.HouseholdKey);
            var usedLoadtypes = entries.Select(x => x.LoadTypeGuid).Distinct().ToList();
            var loadTypeInfos = CalcDataRepository.LoadTypes;
            foreach (StrGuid loadtypeGuid in usedLoadtypes) {
                var lti = loadTypeInfos.Single(x => x.Guid == loadtypeGuid);

                List<AffordanceEnergyUseEntry> filteredEntries = entries.Where(x => x.LoadTypeGuid == loadtypeGuid).ToList();
                var persons = filteredEntries.Select(x => x.PersonName).Distinct().ToList();
                PrepareData(filteredEntries, out var energyUsesPerPersonByAffordance, persons);
                DrawChart(hhkey, energyUsesPerPersonByAffordance, persons, lti);
            }

            CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }

        private void DrawChart([JetBrains.Annotations.NotNull] HouseholdKeyEntry hhkey,
                               [JetBrains.Annotations.NotNull] Dictionary<string, List<double>> energyUsesPerPersonByAfforcance,
                               [ItemNotNull] [JetBrains.Annotations.NotNull] List<string> persons,
                               [JetBrains.Annotations.NotNull] CalcLoadTypeDto lti)
        {
            string plotName = "Affordance Energy Use Per Person " + hhkey.HouseholdKey.Key + " " + lti.Name;
            var plotModel1 = new PlotModel {
                // general
                LegendBorderThickness = 0,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendMargin = 10,
                LegendItemAlignment = HorizontalAlignment.Left
            };
            var labelfontsize = 14;
            if (Config.MakePDFCharts) {
                plotModel1.DefaultFontSize = Parameters.PDFFontSize;
                plotModel1.LegendFontSize = 16;
                labelfontsize = 18;
            }

            if (Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }

            // axes
            var categoryAxis1 = new CategoryAxis {
                MinorStep = 1,
                GapWidth = 1,
                Position = AxisPosition.Left
            };
            OxyPalette p;
            if (energyUsesPerPersonByAfforcance.Count > 1) {
                p = OxyPalettes.HueDistinct(energyUsesPerPersonByAfforcance.Count);
            }
            else {
                p = OxyPalettes.Hue64;
            }

            foreach (var personName in persons) {
                categoryAxis1.Labels.Add(ChartLocalizer.Get().GetTranslation(personName));
            }

            plotModel1.Axes.Add(categoryAxis1);
            string s2 = lti.Name + " in " + lti.UnitOfSum;

            var linearAxis1 = new LinearAxis {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.03,
                MinimumPadding = 0,
                MinorTickSize = 0,
                Position = AxisPosition.Bottom,
                Title = ChartLocalizer.Get().GetTranslation(s2)
            };
            plotModel1.Axes.Add(linearAxis1);
            // generate plot
            var count = 0;
            Dictionary<int, double> colSums2 = new Dictionary<int, double>();
            Dictionary<int, double> colSums = new Dictionary<int, double>();
            for (int i = 0; i < persons.Count; i++) {
                colSums.Add(i, 0);
                colSums2.Add(i, 0);
            }

            foreach (var pair in energyUsesPerPersonByAfforcance) {
                // main columns
                var columnSeries2 = new BarSeries {
                    IsStacked = true,
                    StackGroup = "1",
                    StrokeThickness = 0.1,
                    StrokeColor = OxyColors.White,
                    Title = pair.Key, //affordance name
                    LabelPlacement = LabelPlacement.Middle,
                    FillColor = p.Colors[count++]
                };
                var col = 0;
                foreach (var d in pair.Value) {
                    //energy use values
                    var coli = new BarItem(d);
                    colSums2[col] += d;
                    columnSeries2.Items.Add(coli);
                    col++;
                }

                plotModel1.Series.Add(columnSeries2);
            }

            foreach (var pair in energyUsesPerPersonByAfforcance) {
                var col = 0;
                foreach (var d in pair.Value) {
                    if (d / colSums2[col] > 0.2) {
                        {
                            var textAnnotation1 = new RectangleAnnotation();
                            var shortendName = pair.Key;
                            if (shortendName.Length > 30) {
                                shortendName = shortendName.Substring(0, 30) + "...";
                            }

                            textAnnotation1.Text = shortendName;
                            textAnnotation1.MinimumX = colSums[col];
                            textAnnotation1.MaximumX = colSums[col] + d;
                            textAnnotation1.MinimumY = col + 0.35;
                            textAnnotation1.MaximumY = col + 0.45;
                            textAnnotation1.TextHorizontalAlignment = HorizontalAlignment.Left;
                            textAnnotation1.TextVerticalAlignment = VerticalAlignment.Middle;
                            textAnnotation1.FontSize = labelfontsize;
                            textAnnotation1.StrokeThickness = 0;
                            textAnnotation1.Fill = OxyColors.White;
                            plotModel1.Annotations.Add(textAnnotation1);
                        }
                        {
                            var textAnnotation2 = new RectangleAnnotation {
                                Text = d.ToString("N1", CultureInfo.CurrentCulture) + " " + lti.UnitOfSum,
                                TextHorizontalAlignment = HorizontalAlignment.Left,
                                TextVerticalAlignment = VerticalAlignment.Middle,
                                MinimumX = colSums[col],
                                MaximumX = colSums[col] + d,
                                MinimumY = col + 0.25,
                                MaximumY = col + 0.35,
                                Fill = OxyColors.White,
                                FontSize = labelfontsize,
                                StrokeThickness = 0
                            };

                            plotModel1.Annotations.Add(textAnnotation2);
                        }
                    }

                    colSums[col] += d;
                    col++;
                }
            }

            Save(plotModel1, plotName, "AffordanceEnergyUsePerPerson." + hhkey.HouseholdKey + "." + lti.FileName + ".png", Parameters.BaseDirectory);
        }

        private static void PrepareData([ItemNotNull] [JetBrains.Annotations.NotNull] List<AffordanceEnergyUseEntry> filteredEntries,
                                        [JetBrains.Annotations.NotNull] out Dictionary<string, List<double>> energyUsesPerPersonByAfforcance,
                                        [JetBrains.Annotations.NotNull] [ItemNotNull] List<string> personNames)
        {
            var affordances = filteredEntries.Select(x => x.AffordanceName).Distinct().ToList();
            energyUsesPerPersonByAfforcance = new Dictionary<string, List<double>>();
            affordances.Sort();
            foreach (string affordance in affordances) {
                energyUsesPerPersonByAfforcance.Add(affordance, new List<double>(new double[personNames.Count]));
            }

            foreach (var energyUseEntry in filteredEntries) {
                string affName = energyUseEntry.AffordanceName;
                int personColIdx = personNames.IndexOf(energyUseEntry.PersonName);
                energyUsesPerPersonByAfforcance[affName][personColIdx] += energyUseEntry.EnergySum;
            }
        }
    }
}