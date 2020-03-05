/*using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator.SettlementMergePlots
{
    public static class R1PVCharts
    {
        private const int Fontsize = 30;

        public static void MakeBatteryUsageChart(List<HHEntry> entries, string dstDir)
        {
            var plotModel1 = new PlotModel();
            plotModel1.DefaultFontSize = Fontsize;
            plotModel1.LegendBorderThickness = 0;
            plotModel1.LegendOrientation = LegendOrientation.Horizontal;
            plotModel1.LegendPlacement = LegendPlacement.Outside;
            plotModel1.LegendPosition = LegendPosition.BottomCenter;
            plotModel1.LegendFontSize = Fontsize;
            plotModel1.LegendSymbolMargin = 15;
            plotModel1.Title = string.Empty;
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1000;
            categoryAxis1.MajorStep = 1;
            categoryAxis1.GapWidth = 0;
            categoryAxis1.MaximumPadding = 0.02;
            categoryAxis1.Title = "Haushalt";
            plotModel1.Axes.Add(categoryAxis1);
            var linearAxis1 = new LinearAxis();
            linearAxis1.AbsoluteMinimum = 0;
            linearAxis1.MaximumPadding = 0.06;
            linearAxis1.MinimumPadding = 0.06;
            linearAxis1.MinorTickSize = 0;
            linearAxis1.Title = "Energie in kWh";
            plotModel1.Axes.Add(linearAxis1);
            var fromBattery = new ColumnSeries();
            fromBattery.IsStacked = true;
            fromBattery.StrokeThickness = 1;
            fromBattery.Title = "Ausspeisung Batterie";
            fromBattery.StrokeColor = OxyColors.White;
            var toBattery = new ColumnSeries();
            toBattery.IsStacked = true;
            toBattery.StrokeThickness = 1;
            toBattery.StrokeColor = OxyColors.White;
            toBattery.Title = "Einspeisung Batterie";
            for (int i = 0; i < entries.Count; i++)
            {
                if (i % 10 == 0)
                    categoryAxis1.Labels.Add((i + 1).ToString(CultureInfo.CurrentCulture)); // entries[i].Name
                else
                    categoryAxis1.Labels.Add(string.Empty); // entries[i].Name
                fromBattery.Items.Add(new ColumnItem(entries[i].SavedToBattery * -1));
                toBattery.Items.Add(new ColumnItem(entries[i].ConsumedFromBattery * -1));
            }
            plotModel1.Series.Add(fromBattery);
            plotModel1.Series.Add(toBattery);
            string dstFullName = Path.Combine(dstDir, "BatteryUsageChart.pdf");
            ChartPDFCreator.OxyPDFCreator.Run(plotModel1, dstFullName);
        }

        public static void MakeEnergyConsumptionChart(List<HHEntry> entries, string dstDir)
        {
            var plotModel1 = new PlotModel();
            plotModel1.DefaultFontSize = Fontsize;
            plotModel1.LegendBorderThickness = 0;
            plotModel1.LegendOrientation = LegendOrientation.Horizontal;
            plotModel1.LegendPlacement = LegendPlacement.Outside;
            plotModel1.LegendPosition = LegendPosition.BottomCenter;
            plotModel1.LegendFontSize = Fontsize;
            plotModel1.LegendSymbolMargin = 15;
            plotModel1.Title = string.Empty;
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1000;
            categoryAxis1.MajorStep = 10;
            categoryAxis1.GapWidth = 0;
            categoryAxis1.MaximumPadding = 0.02;
            categoryAxis1.Title = "Haushalt";
            plotModel1.Axes.Add(categoryAxis1);
            var linearAxis1 = new LinearAxis();
            linearAxis1.AbsoluteMinimum = 0;
            linearAxis1.MaximumPadding = 0.06;
            linearAxis1.MinimumPadding = 0.06;
            linearAxis1.MinorTickSize = 0;
            linearAxis1.Title = "Energie in kWh";
            plotModel1.Axes.Add(linearAxis1);
            OxyPalette p = OxyPalettes.HueDistinct(6);
            var selfConsumption = new ColumnSeries();
            selfConsumption.IsStacked = true;
            selfConsumption.StrokeThickness = 1;
            selfConsumption.Title = "Direkter Eigenverbrauch PV";
            selfConsumption.StrokeColor = OxyColors.White;
            selfConsumption.FillColor = p.Colors[0];

            var fromBattery = new ColumnSeries();
            fromBattery.IsStacked = true;
            fromBattery.StrokeThickness = 1;
            fromBattery.Title = "Ausspeisung Batterie";
            fromBattery.StrokeColor = OxyColors.White;
            fromBattery.FillColor = p.Colors[1];

            var gridload = new ColumnSeries();
            gridload.IsStacked = true;
            gridload.StrokeThickness = 1;
            gridload.StrokeColor = OxyColors.White;
            gridload.Title = "Restlast für das Netz";
            gridload.FillColor = p.Colors[2];

            var toBattery = new ColumnSeries();
            toBattery.IsStacked = true;
            toBattery.StrokeThickness = 1;
            toBattery.StrokeColor = OxyColors.White;
            toBattery.Title = "Einspeisung Batterie";
            toBattery.FillColor = p.Colors[3];

            var pvSelf = new ColumnSeries();
            pvSelf.IsStacked = true;
            pvSelf.StrokeThickness = 1;
            pvSelf.StrokeColor = OxyColors.White;
            pvSelf.Title = "direkter Eigenverbrauch";
            pvSelf.FillColor = p.Colors[4];

            var gridContrib = new ColumnSeries();
            gridContrib.IsStacked = true;
            gridContrib.StrokeThickness = 1;
            gridContrib.StrokeColor = OxyColors.White;
            gridContrib.Title = "Direkte Netzeinspeisung";
            gridContrib.FillColor = p.Colors[5];

            for (int i = 0; i < entries.Count; i++)
            {
                categoryAxis1.Labels.Add((i + 1).ToString(CultureInfo.CurrentCulture)); // entries[i].Name
                // last
                selfConsumption.Items.Add(new ColumnItem(entries[i].SelfConsumption));
                fromBattery.Items.Add(new ColumnItem(entries[i].ConsumedFromBattery * -1));
                gridload.Items.Add(new ColumnItem(entries[i].GridLoadWithBattery));
                // erzeugung
                toBattery.Items.Add(new ColumnItem(entries[i].ConsumedFromBattery));
                pvSelf.Items.Add(new ColumnItem(entries[i].SelfConsumption * -1));
                gridContrib.Items.Add(new ColumnItem(entries[i].GridEinspeisungWithBattery));
            }
            plotModel1.Series.Add(fromBattery);
            plotModel1.Series.Add(selfConsumption);
            plotModel1.Series.Add(gridload);
            // erzeugung
            plotModel1.Series.Add(toBattery);
            plotModel1.Series.Add(pvSelf);
            plotModel1.Series.Add(gridContrib);
            string dstFullName = Path.Combine(dstDir, "BatteryEnergyConsumption.pdf");
            ChartPDFCreator.OxyPDFCreator.Run(plotModel1, dstFullName);
        }
        /*
        public class HHEntry
        {
            public HHEntry(List<EigenverbrauchsCalculator.Entry> entries)
            {
                foreach (EigenverbrauchsCalculator.Entry entry in entries)
                {
                    PVRawSum += entry.PVRaw;
                    ElectricitySum += entry.Electricity;
                    PVFittedSum += entry.PhotovoltaikFitted;
                    SelfConsumption += entry.SelfConsumption;
                    GridEinspeisung += entry.GridEinspeisung;
                    GridLast += entry.GridLast;
                    ConsumedFromBattery += entry.ConsumedFromBattery;
                    SavedToBattery += entry.SavedToBattery;
                    SelfConsumptionWithBattery += entry.SelfConsumptionWithBattery;
                    GridEinspeisungWithBattery += entry.GridEinspeisungWithBattery;
                    GridLoadWithBattery += entry.GridLoadWithBattery;
                }
            }

            private double ElectricitySum { get; }
            private double GridEinspeisung { get; }
            private double GridLast { get; }
            private double PVFittedSum { get; }
            private double PVRawSum { get; }
            public double SelfConsumption { get; }
            public double ConsumedFromBattery { get; }
            public double SavedToBattery { get; }
            private double SelfConsumptionWithBattery { get;  }
            public double GridEinspeisungWithBattery { get; }
            public double GridLoadWithBattery { get; }
            public string Name { get; set; }

            public override string ToString()
            {
                string s = string.Empty;
                s += "PVRawSum: " + PVRawSum + Environment.NewLine;
                s += "ElectricitySum: " + ElectricitySum + Environment.NewLine;
                s += "PVFittedSum: " + PVFittedSum + Environment.NewLine;
                s += "SelfConsumption: " + SelfConsumption + Environment.NewLine;
                s += "GridEinspeisung: " + GridEinspeisung +Environment.NewLine;
                s += "GridLast: " + GridLast + Environment.NewLine;
                s += "ConsumedFromBattery: " + ConsumedFromBattery + Environment.NewLine;
                s += "SavedToBattery: " + SavedToBattery + Environment.NewLine;
                s += "SelfConsumptionWithBattery: " + SelfConsumptionWithBattery + Environment.NewLine;
                s += "GridEinspeisungWithBattery: " + GridEinspeisungWithBattery + Environment.NewLine;
                s += "GridLoadWithBattery: " + GridLoadWithBattery +Environment.NewLine;
                return s;
            }
        }
    }
}*/

