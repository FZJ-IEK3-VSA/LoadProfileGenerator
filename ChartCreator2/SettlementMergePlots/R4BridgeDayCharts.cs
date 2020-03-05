/*using System.Collections.Generic;
using System.Linq;
using ChartPDFCreator;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator.SettlementMergePlots
{
    internal class R4RetiredCharts
    {
        private readonly int _fontsize = 24;

        public void MakeGeneralBarChart(List<double> values, string fileName)
        {
            PlotModel plotModel2 = MakePlotmodel("Energieverbrauch in kWh");
            plotModel2.LegendFontSize = _fontsize;
            var columnSeries2 = new ColumnSeries();
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
            OxyPDFCreator.Run(plotModel2, fileName);
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
    }
}*/

