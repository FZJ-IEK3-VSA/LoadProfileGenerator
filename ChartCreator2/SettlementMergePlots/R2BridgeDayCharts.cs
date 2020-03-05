/*using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ChartPDFCreator;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator.SettlementMergePlots
{
    internal class R2BridgeDayCharts
    {
        private readonly int _fontsize = 24;

        private void MakeGeneralBarChart(List<double> values, string fileName, string yAxisLabel)
        {
            PlotModel plotModel2 = MakePlotmodel(yAxisLabel);
            plotModel2.LegendFontSize = _fontsize;
            ColumnSeries columnSeries2 = new ColumnSeries();
            columnSeries2.IsStacked = true;
            columnSeries2.StackGroup = "1";
            columnSeries2.StrokeThickness = 1;
            columnSeries2.Title = string.Empty;
            columnSeries2.LabelPlacement = LabelPlacement.Middle;
            columnSeries2.FillColor = OxyColors.LightBlue;
            columnSeries2.StrokeColor = OxyColors.White;
            double averageValue = values.Average();
            LineSeries ls = new LineSeries();
            ls.Color = OxyColors.Red;
            ls.Title = "Durchschnitt";
            for (int i = 0; i < values.Count; i++)
            {
                ls.Points.Add(new DataPoint(i, averageValue));
                columnSeries2.Items.Add(new ColumnItem(values[i]));
            }
            plotModel2.Series.Add(columnSeries2);
            plotModel2.Series.Add(ls);
            OxyPDFCreator.Run(plotModel2, fileName, OxyPDFCreator.HeightWidth.HeightWidth167);
        }

        private static List<DateTime> InitFeiertagDates()
        {
            string[] strdates =
            {
                "03.04.2015", "06.04.2015", "01.05.2015", "14.05.2015", "25.05.2015", "03.10.2015",
                "31.10.2015", "01.01.2015", "24.12.2015", "25.12.2015", "26.12.2015", "31.12.2015"
            };
            List<DateTime> dt = new List<DateTime>();
            foreach (string s in strdates)
                dt.Add(Convert.ToDateTime(s, CultureInfo.CurrentCulture));
            return dt;
        }

        private static void MakeHeatMap(List<BridgeDayHousehold> hhs, string dstFileName)
        {
            Dictionary<DateTime, int> counts = new Dictionary<DateTime, int>();
            DateTime dt = new DateTime(2015, 1, 1);
            DateTime end = new DateTime(2016, 1, 1);
            while (dt < end)
            {
                counts.Add(dt, 0);
                dt = dt.AddDays(1);
            }
            foreach (BridgeDayHousehold bridgeDayHousehold in hhs)
                foreach (DateTime date in bridgeDayHousehold.Dates)
                    counts[date]++;
            PlotModel pm = new PlotModel();
            List<DateTime> holidays = InitFeiertagDates();
            OxyPalette p = OxyPalettes.Jet(100);
            p.Colors[0] = OxyColors.White;

            foreach (KeyValuePair<DateTime, int> pair in counts)
            {
                int day = pair.Key.DayOfYear - 1;
                // bridge days
                int valueToSet = 0;
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
                    valueToSet = pair.Value;
                }
                if ((pair.Key.DayOfWeek == DayOfWeek.Saturday) || (pair.Key.DayOfWeek == DayOfWeek.Sunday))
                {
                    IntervalBarSeries bridgeDays = new IntervalBarSeries();
                    bridgeDays.Items.Add(new IntervalBarItem(0, 0));
                    bridgeDays.Items.Add(new IntervalBarItem(day, day + 1));
                    pm.Series.Add(bridgeDays);
                    bridgeDays.FillColor = p.Colors[99];
                    bridgeDays.StrokeThickness = 0.1;
                    bridgeDays.StrokeColor = OxyColors.White;
                    valueToSet = 99;
                }
                if (holidays.Contains(pair.Key))
                {
                    IntervalBarSeries bridgeDays = new IntervalBarSeries();
                    bridgeDays.Items.Add(new IntervalBarItem(0, 0));
                    bridgeDays.Items.Add(new IntervalBarItem(0, 0));
                    bridgeDays.Items.Add(new IntervalBarItem(day, day + 1));
                    pm.Series.Add(bridgeDays);
                    bridgeDays.FillColor = p.Colors[99];
                    bridgeDays.StrokeThickness = 0.1;
                    bridgeDays.StrokeColor = OxyColors.White;
                    valueToSet = 99;
                }
                if (valueToSet > 0)
                {
                    IntervalBarSeries bridgeDays = new IntervalBarSeries();
                    bridgeDays.Items.Add(new IntervalBarItem(0, 0));
                    bridgeDays.Items.Add(new IntervalBarItem(0, 0));
                    bridgeDays.Items.Add(new IntervalBarItem(0, 0));
                    bridgeDays.Items.Add(new IntervalBarItem(day, day + 1));
                    pm.Series.Add(bridgeDays);
                    bridgeDays.FillColor = p.Colors[valueToSet];
                    bridgeDays.StrokeThickness = 0.1;
                    bridgeDays.StrokeColor = OxyColors.White;
                }
            }
            pm.LegendFontSize = 26;
            pm.DefaultFontSize = 26;
            LinearColorAxis colAx = new LinearColorAxis
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
            LinearAxis linearAxis1 = new LinearAxis();
            linearAxis1.Position = AxisPosition.Bottom;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MinorTickSize = 0;
            linearAxis1.MaximumPadding = 0;
            // linearAxis1.
            linearAxis1.Title = "Tag im Jahr";
            linearAxis1.FontSize = 22;
            pm.Axes.Add(linearAxis1);
            CategoryAxis ca = new CategoryAxis();
            ca.Position = AxisPosition.Left;
            ca.Labels.Add("Brückentage");
            ca.Labels.Add("Wochenenden");
            ca.Labels.Add("Feiertage");
            ca.Labels.Add("Kombiniert");
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

            CategoryAxis categoryAxis1 = new CategoryAxis();
            categoryAxis1.MajorStep = 10;
            categoryAxis1.Minimum = -0.5;
            categoryAxis1.MaximumPadding = 0.03;
            categoryAxis1.GapWidth = 0;

            plotModel1.Axes.Add(categoryAxis1);
            LinearAxis linearAxis1 = new LinearAxis();
            linearAxis1.AbsoluteMinimum = 0;
            linearAxis1.MaximumPadding = 0.06;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.Title = yaxislabel;
            plotModel1.Axes.Add(linearAxis1);
            return plotModel1;
        }

        public void Run(List<BridgeDayHousehold> hhs, string dstDir)
        {
            string heatMapFile = Path.Combine(dstDir, "BridgeDayHeatmapFile.pdf");
            MakeHeatMap(hhs, heatMapFile);
            string filename = Path.Combine(dstDir, "BridgeTotalEnergyUse.pdf");
            MakeGeneralBarChart(hhs.Select(x => x.EnergyUse).ToList(), filename, "Energieverbrauch in kWh");
            filename = Path.Combine(dstDir, "BridgeEnergyDifference.pdf");
            MakeGeneralBarChart(hhs.Select(x => x.EnergyDifference).ToList(), filename, "Energieverbrauch in kWh");
            filename = Path.Combine(dstDir, "BridgeDayCount.pdf");
            MakeGeneralBarChart(hhs.Select(x => (double) x.BridgeDayCount).ToList(), filename, "Anzahl der Brückentage");
        }

        internal class BridgeDayHousehold
        {
            public int BridgeDayCount { get; set; }
            public List<DateTime> Dates { get; } = new List<DateTime>();
            public double EnergyDifference { get; set; }
            public double EnergyUse { get; set; }
            public string Name { get; set; }
        }
    }
}*/

