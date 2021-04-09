/*using System;
using System.Collections.Generic;
using System.Linq;
using ChartPDFCreator;
using CommonDataWPF;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator.SettlementMergePlots {
    internal class HouseholdTagDistributionChart {
        public void Run(string classification, List<Tuple<string, int>> originalEntries, string newFileName) {
            var entries =
                originalEntries
                    .Select(x => new Tuple<string, int>(ChartLocalizer.Get().GetTranslation(x.Item1), x.Item2))
                    .ToList();
            entries.Sort((x, y) => string.Compare(y.Item1, x.Item1, StringComparison.Ordinal));
            var pm = new PlotModel();
            pm.LegendFontSize = 50;
            pm.DefaultFontSize = 50;
            var max = entries.Select(x => x.Item2).Max();
            var ca = new CategoryAxis();
            ca.Position = AxisPosition.Left;
            ca.TickStyle = TickStyle.None;
            ca.Title = ChartLocalizer.Get().GetTranslation(classification);
            pm.Axes.Add(ca);
            var la = new LinearAxis();
            la.Position = AxisPosition.Bottom;
            la.MinorTickSize = 0;
            la.Minimum = 0;
            la.MinimumPadding = 0;
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
            var step = max / 5;
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
            la.MajorStep = step;
            la.MaximumPadding = 0.03;
            la.Title = "Anzahl Haushalte";
            pm.Axes.Add(la);
            var bs = new BarSeries();
            bs.FillColor = OxyColors.Blue;
            foreach (var keyValuePair in entries) {
                var lbl = keyValuePair.Item1;
                ca.Labels.Add(lbl);
                bs.Items.Add(new BarItem(keyValuePair.Item2));
            }
            pm.Series.Add(bs);
            OxyPDFCreator.Run(pm, newFileName);
        }
    }
}*/