/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChartPDFCreator;
using CommonDataWPF;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator.SettlementMergePlots
{
    internal class R3VacationCharts
    {
        private readonly int _fontsize = 24;

        private void MakeEnergyBarChart(List<VacationHousehold> values, string fileName)
        {
            PlotModel plotModel2 = MakePlotmodel("Energieverbrauch in kWh");
            plotModel2.LegendFontSize = _fontsize;
            OxyPalette p = OxyPalettes.Jet(4);
            List<Tuple<string, List<double>>> list = new List<Tuple<string, List<double>>>();
            var l1 = values.Select(x => x.EnergyUse).ToList();
            list.Add(new Tuple<string, List<double>>("1 Tag Urlaub", l1));
            var l7 = values.Select(x => x.Others[7].EnergyUse).ToList();
            list.Add(new Tuple<string, List<double>>("7 Tag Urlaub", l7));
            var l14 = values.Select(x => x.Others[14].EnergyUse).ToList();
            list.Add(new Tuple<string, List<double>>("14 Tage Urlaub", l14));
            var l21 = values.Select(x => x.Others[21].EnergyUse).ToList();
            list.Add(new Tuple<string, List<double>>("21 Tage Urlaub", l21));

            List<LineSeries> allLs = new List<LineSeries>();
            for (int i = 0; i < list.Count; i++)
            {
                Tuple<string, List<double>> tuple = list[i];
                var columnSeries2 = new ColumnSeries();
                columnSeries2.IsStacked = false;
                columnSeries2.StrokeThickness = 1;
                columnSeries2.Title = tuple.Item1;
                columnSeries2.LabelPlacement = LabelPlacement.Middle;
                columnSeries2.FillColor = p.Colors[i];
                columnSeries2.StrokeColor = OxyColors.White;
                double averageValue = list[i].Item2.Average();
                LineSeries ls = new LineSeries();
                ls.Color = p.Colors[i];
                ls.StrokeThickness = 2;
                ls.Title = "Durchschnitt " + list[i].Item1;
                allLs.Add(ls);
                int col = 0;
                for (int j = 0; j < list[i].Item2.Count; j++)
                {
                    columnSeries2.Items.Add(new ColumnItem(list[i].Item2[j]));
                    ls.Points.Add(new DataPoint(col, averageValue));
                    col++;
                }
                plotModel2.Series.Add(columnSeries2);
            }
            foreach (LineSeries series in allLs)
            {
                plotModel2.Series.Add(series);
            }
            OxyPDFCreator.Run(plotModel2, fileName);
        }

        private static void MakeHeatMap(List<VacationHousehold> hhs, string dstFileName)
        {
            Dictionary<DateTime, int> count1 = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> count7 = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> count14 = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> count21 = new Dictionary<DateTime, int>();
            DateTime start = new DateTime(2015, 1, 1);
            DateTime dt = start;
            DateTime end = new DateTime(2016, 1, 1);
            while (dt < end)
            {
                count1.Add(dt, 0);
                count7.Add(dt, 0);
                count14.Add(dt, 0);
                count21.Add(dt, 0);
                dt = dt.AddDays(1);
            }
            int totalday1 = 0;
            int totalday7 = 0;
            int totalday14 = 0;
            int totalday21 = 0;
            foreach (var household in hhs)
            {
                foreach (DateTime date in household.VacationDates)
                {
                    if (date >= start && date <= end)
                    {
                        totalday1++;
                        count1[date]++;
                    }
                }
                foreach (DateTime date in household.Others[7].VacationDates)
                {
                    if (date >= start && date <= end)
                    {
                        totalday7++;
                        count7[date]++;
                    }
                }
                foreach (DateTime date in household.Others[14].VacationDates)
                {
                    if (date >= start && date <= end)
                    {
                        totalday14++;
                        count14[date]++;
                    }
                }
                foreach (DateTime date in household.Others[21].VacationDates)
                {
                    if (date >= start && date <= end)
                    {
                        totalday21++;
                        count21[date]++;
                    }
                }
            }
            Logger.Info("1:" + totalday1);
            Logger.Info("7:" + totalday7);
            Logger.Info("14:" + totalday14);
            Logger.Info("21:" + totalday21);
            var pm = new PlotModel();
            OxyPalette p = OxyPalettes.Jet(100);
            p.Colors[0] = OxyColors.White;
            foreach (KeyValuePair<DateTime, int> pair in count1)
            {
                int day = pair.Key.DayOfYear - 1;
                // vacation days
                int row = 0;
                if (pair.Value > 0)
                {
                    IntervalBarSeries bridgeDays = new IntervalBarSeries();
                    bridgeDays.FontSize = 16;
                    IntervalBarItem ibsi = new IntervalBarItem(day, day + 1);
                    bridgeDays.Items.Add(ibsi);
                    pm.Series.Add(bridgeDays);
                    bridgeDays.FillColor = p.Colors[pair.Value];
                    bridgeDays.StrokeThickness = 0.1;
                    bridgeDays.StrokeColor = OxyColors.White;
                }
                row++;
                if (count7[pair.Key] > 0)
                {
                    IntervalBarSeries bridgeDays = new IntervalBarSeries();
                    bridgeDays.FontSize = 16;
                    for (int i = 0; i < row; i++)
                        bridgeDays.Items.Add(new IntervalBarItem(0, 0));
                    IntervalBarItem ibsi = new IntervalBarItem(day, day + 1);
                    bridgeDays.Items.Add(ibsi);
                    pm.Series.Add(bridgeDays);
                    bridgeDays.FillColor = p.Colors[count7[pair.Key]];
                    bridgeDays.StrokeThickness = 0.1;
                    bridgeDays.StrokeColor = OxyColors.White;
                }
                row++;
                if (count14[pair.Key] > 0)
                {
                    IntervalBarSeries bridgeDays = new IntervalBarSeries();
                    bridgeDays.FontSize = 16;
                    for (int i = 0; i < row; i++)
                        bridgeDays.Items.Add(new IntervalBarItem(0, 0));
                    IntervalBarItem ibsi = new IntervalBarItem(day, day + 1);
                    bridgeDays.Items.Add(ibsi);
                    pm.Series.Add(bridgeDays);
                    bridgeDays.FillColor = p.Colors[count14[pair.Key]];
                    bridgeDays.StrokeThickness = 0.1;
                    bridgeDays.StrokeColor = OxyColors.White;
                }
                row++;
                if (count21[pair.Key] > 0)
                {
                    IntervalBarSeries bridgeDays = new IntervalBarSeries();
                    bridgeDays.FontSize = 16;
                    for (int i = 0; i < row; i++)
                        bridgeDays.Items.Add(new IntervalBarItem(0, 0));
                    IntervalBarItem ibsi = new IntervalBarItem(day, day + 1);
                    bridgeDays.Items.Add(ibsi);
                    pm.Series.Add(bridgeDays);
                    bridgeDays.FillColor = p.Colors[count21[pair.Key]];
                    bridgeDays.StrokeThickness = 0.1;
                    bridgeDays.StrokeColor = OxyColors.White;
                }
            }
            pm.LegendFontSize = 26;
            pm.DefaultFontSize = 26;
            var colAx = new LinearColorAxis
            {
                Position = AxisPosition.Bottom,
                HighColor = OxyColors.Gray,
                LowColor = OxyColors.Black,
                Title = "Anteil der betroffenen Haushalte in Prozent"
            };
            OxyPalette p2 = OxyPalettes.Jet(100);
            p2.Colors[0] = OxyColors.White;
            OxyPalette p3 = p2.Reverse();
            colAx.Palette = p3;
            colAx.RenderAsImage = true;
            colAx.PositionTier = 3;
            colAx.FontSize = 22;
            colAx.EndPosition = 0.5;
            pm.Axes.Add(colAx);

            // adding half cellwidth/cellheight to bounding box coordinates
            var linearAxis1 = new LinearAxis();
            linearAxis1.Position = AxisPosition.Bottom;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MinorTickSize = 0;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.Minimum = 0;
            linearAxis1.Maximum = 365;
            // linearAxis1.
            linearAxis1.Title = "Tag im Jahr";
            linearAxis1.FontSize = 22;
            pm.Axes.Add(linearAxis1);
            var ca = new CategoryAxis();
            ca.Position = AxisPosition.Left;
            ca.Labels.Add("1 Tag Urlaub");
            ca.Labels.Add("7 Tag Urlaub");
            ca.Labels.Add("14 Tage Urlaub");
            ca.Labels.Add("21 Tage Urlaub");
            ca.GapWidth = 0.1;
            pm.Axes.Add(ca);
            OxyPDFCreator.Run(pm, dstFileName, OxyPDFCreator.HeightWidth.HeightWidth165);
        }

        private PlotModel MakePlotmodel(string yaxislabel)
        {
            PlotModel plotModel1 = new PlotModel();
            plotModel1.DefaultFontSize = _fontsize;
            plotModel1.LegendFontSize = 10;
            plotModel1.LegendBorderThickness = 0;
            plotModel1.LegendOrientation = LegendOrientation.Horizontal;
            plotModel1.LegendPlacement = LegendPlacement.Outside;
            plotModel1.LegendPosition = LegendPosition.BottomCenter;
            plotModel1.LegendSymbolMargin = 20;

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MajorStep = 10;
            categoryAxis1.Minimum = -0.5;
            categoryAxis1.MaximumPadding = 0.03;
            categoryAxis1.GapWidth = 0;

            plotModel1.Axes.Add(categoryAxis1);
            var linearAxis1 = new LinearAxis();
            linearAxis1.AbsoluteMinimum = 0;
            linearAxis1.MaximumPadding = 0.06;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.Title = yaxislabel;
            plotModel1.Axes.Add(linearAxis1);
            return plotModel1;
        }

        public void Run(List<VacationHousehold> hhs, string dstDir)
        {
            string heatMapFile = Path.Combine(dstDir, "VacationHeatmapFile.pdf");
            MakeHeatMap(hhs, heatMapFile);
            string filename = Path.Combine(dstDir, "VacationTotalEnergyUse.pdf");
            MakeEnergyBarChart(hhs, filename);
        }

        public class VacationHousehold
        {
            public VacationHousehold(string dayCount)
            {
                DayCount = dayCount;
            }

            private string DayCount { get; }

            //private List<Tuple<DateTime, DateTime>> Dates { get; } = new List<Tuple<DateTime, DateTime>>();

            public List<DateTime> VacationDates
            {
                get
                {
                    List<DateTime> dts = new List<DateTime>();
                    /*foreach (Tuple<DateTime, DateTime> tuple in Dates)
                    {
                        DateTime curr = tuple.Item1;
                        while (curr <= tuple.Item2)
                        {
                            dts.Add(curr);
                            curr = curr.AddDays(1);
                        }
                    }
                    return dts;
                }
            }

            public double EnergyUse { get; set; }
            private string Name { get;  }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
                 "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            private string ComparableName => CreateComparableName();

            private string CreateComparableName()
            {
                if (Name.Contains("DissR3Vacations"))
                {
                    string oldstr1 = "for DissR3Vacations14 for DissR3Vacations";
                    string newstr1 = "xx14 for DissR3Vacations";
                    string oldstr2 = "for DissR3Vacations21 for DissR3Vacations";
                    string newstr2 = "xx21 for DissR3Vacations";
                    string oldstr3 = "for DissR3Vacations7 for DissR3Vacations";
                    string newstr3 = "xx7 for DissR3Vacations";
                    string oldstr4 = "for DissR3Vacations1 for DissR3Vacations";
                    string newstr4 = "xx1 for DissR3Vacations";
                    string oldstr5 = "DissR3Vacations" + DayCount;
                    string newStr5 = "DissR3Vacations xx";
                    string s =
                        Name.Replace(oldstr1, newstr1)
                            .Replace(oldstr2, newstr2)
                            .Replace(oldstr3, newstr3)
                            .Replace(oldstr4, newstr4)
                            .Replace(oldstr5, newStr5)
                            .Trim();
                    if (s.EndsWith(" 1", StringComparison.Ordinal))
                        s = s.Substring(0, s.Length - 2);
                    return s;
                }
                throw new LPGException("Missing DissR3Vacations");
            }

            public Dictionary<int, VacationHousehold> Others { get; } = new Dictionary<int, VacationHousehold>();

            public override string ToString() => ComparableName;
        }
    }
}*/

