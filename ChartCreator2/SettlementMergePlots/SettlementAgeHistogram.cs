/*using System.Collections.Generic;
using ChartPDFCreator;
using DatabaseIO.Tables.Houses;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator.SettlementMergePlots {
    internal class SettlementAgeHistogram {
        public void Run(string outputPath, List<Settlement.AgeEntry> ageEntries) {
            var plotModel2 = new PlotModel();
            plotModel2.LegendFontSize = 30;
            plotModel2.DefaultFontSize = 26;
            // axes
            var cate = new CategoryAxis();
            cate.AbsoluteMinimum = 0;
            cate.MinimumPadding = 0;
            cate.GapWidth = 0;
            cate.IsTickCentered = true;
            cate.MajorStep = 1;
            cate.Title = "Alter";
            plotModel2.Axes.Add(cate);

            var linearAxis2 = new LinearAxis();
            linearAxis2.AbsoluteMinimum = 0;
            linearAxis2.MaximumPadding = 0.1;
            linearAxis2.MinimumPadding = 0;
            linearAxis2.MinorTickSize = 0;
            linearAxis2.Title = "Anzahl der Personen";
            plotModel2.Axes.Add(linearAxis2);
            var columnSeries2 = new ColumnSeries();
            columnSeries2.StrokeThickness = 1;
            columnSeries2.StrokeColor = OxyColors.White;
            columnSeries2.LabelFormatString = "{0}";
            for (var i = 1; i < ageEntries.Count; i++) {
                var item = new ColumnItem(ageEntries[i].Count);
                columnSeries2.Items.Add(item);
                cate.Labels.Add(ageEntries[i].AgeRange.Replace(" ", string.Empty));
            }
            plotModel2.Series.Add(columnSeries2);
            OxyPDFCreator.Run(plotModel2, outputPath, OxyPDFCreator.HeightWidth.HeightWidth165);
        }
    }
}*/