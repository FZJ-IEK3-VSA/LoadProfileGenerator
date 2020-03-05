/*using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChartPDFCreator;
using CommonDataWPF;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator.SettlementMergePlots {
    internal class EigenverbrauchsCharts {
        public enum BarChartMode {
            Quota,
            Energy
        }

        private readonly int _fontsize = 30;

        public void MakeBarChart(List<EigenverbrauchsColumn> columns, string dstDir, string fileName,
            BarChartMode barChartMode) {
            var plotModel1 = new PlotModel();
            plotModel1.DefaultFontSize = _fontsize;
            plotModel1.LegendBorderThickness = 0;
            plotModel1.LegendOrientation = LegendOrientation.Horizontal;
            plotModel1.LegendPlacement = LegendPlacement.Outside;
            plotModel1.LegendPosition = LegendPosition.BottomCenter;
            plotModel1.LegendFontSize = _fontsize;
            plotModel1.LegendSymbolMargin = 15;
            plotModel1.Title = string.Empty;
            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.GapWidth = 0;
            categoryAxis1.MaximumPadding = 0.01;
            categoryAxis1.Title = "Haushalt";
            plotModel1.Axes.Add(categoryAxis1);
            var linearAxis1 = new LinearAxis();
            linearAxis1.AbsoluteMinimum = 0;
            linearAxis1.MaximumPadding = 0.05;
            linearAxis1.MinimumPadding = 0.0;
            linearAxis1.MinorTickSize = 0;
            if (barChartMode == BarChartMode.Quota) {
                linearAxis1.Title = "Eigenverbrauchsquote in Prozent";
            }
            if (barChartMode == BarChartMode.Energy) {
                linearAxis1.Title = "Stromverbrauch in kWh";
            }

            plotModel1.Axes.Add(linearAxis1);
            OxyPalette p;

            switch (columns.Count) {
                case 1: {
                    var oc = new List<OxyColor>();
                    oc.Add(OxyColors.LightBlue);
                    oc.Add(OxyColors.Red);
                    p = new OxyPalette(oc);
                }
                    break;
                case 2: {
                    var oc = new List<OxyColor>();
                    oc.Add(OxyColors.DarkBlue);
                    oc.Add(OxyColors.LightBlue);
                    oc.Add(OxyColors.DarkOrange);
                    oc.Add(OxyColors.Orange);
                    p = new OxyPalette(oc);
                }
                    break;
                default:
                    p = OxyPalettes.HueDistinct(columns.Count * 2);
                    break;
            }
            var coloridx = 0;
            var allLineSeries = new List<LineSeries>();
            for (var i = 0; i < columns.Count; i++) {
                var eigenverbrauchsColumn = columns[i];
                var selfConsumption = new ColumnSeries();
                selfConsumption.StrokeThickness = 1;
                selfConsumption.StrokeColor = OxyColors.White;
                selfConsumption.FillColor = p.Colors[coloridx++];
                selfConsumption.Title = eigenverbrauchsColumn.Name;
                var col = 0;
                var ls = new LineSeries();
                ls.Color = p.Colors[coloridx++];
                double average;
                List<Tuple<string, double>> listToUse;
                string unit;
                double faktor;
                switch (barChartMode) {
                    case BarChartMode.Quota:
                        average = eigenverbrauchsColumn.AverageQuota;
                        listToUse = eigenverbrauchsColumn.Quotas;
                        faktor = 100;
                        unit = "%";
                        break;
                    case BarChartMode.Energy:
                        average = eigenverbrauchsColumn.AverageEnergy;
                        listToUse = eigenverbrauchsColumn.EnergyUse;
                        faktor = 1;
                        unit = "kWh";
                        break;
                    default:
                        throw new LPGException("Forgotten bar chart mode");
                }
                ls.Title = eigenverbrauchsColumn.AvgName + "(" + average.ToString("N1", CultureInfo.CurrentCulture) +
                           " " + unit + ")";
                ls.StrokeThickness = 3;
                foreach (var value in listToUse) {
                    ls.Points.Add(new DataPoint(col, average));
                    selfConsumption.Items.Add(new ColumnItem(value.Item2 * faktor));
                    if (i == 0) {
                        if ((col + 1) % 10 == 0) {
                            categoryAxis1.ActualLabels.Add((col + 1).ToString(CultureInfo.CurrentCulture));
                        }
                        else {
                            categoryAxis1.ActualLabels.Add(" ");
                        }
                    }
                    col++;
                }
                plotModel1.Series.Add(selfConsumption);
                allLineSeries.Add(ls);
            }
            foreach (var lineSeries in allLineSeries) {
                plotModel1.Series.Add(lineSeries);
            }
            var dstFullName = Path.Combine(dstDir, fileName);
            OxyPDFCreator.Run(plotModel1, dstFullName);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public EigenverbrauchsColumn ReadEigenverbrauchsColumn(string directory, string name, string avgName) {
            var evcol = new EigenverbrauchsColumn(name, avgName);
            var di = new DirectoryInfo(directory);
            var sufos = di.GetDirectories();
            var tasks = new List<Task>();
            var task = 0;
            var start = DateTime.Now;
            sufos = sufos.OrderBy(x => x.Name).ToArray();
            foreach (var sufo in sufos) {
#pragma warning disable CC0022 // Should dispose object
                var myTask = new Task(() => {
                    var tasknumber = task;
                    var taskStart = DateTime.Now;
                    var ts = DateTime.Now - start;
                    Logger.Info("Executing:" + task++ + " at " + ts);
                    var mySufo = sufo;
                    var fis = mySufo.GetFiles("DeviceProfiles.PV Electricity Balance.csv",
                        SearchOption.AllDirectories);
                    if (fis.Length == 1) {
                        var entries =
                            EigenverbrauchsCalculator.ReadFileAndCalculateMatch(fis[0].FullName, 1, 3, 4, true);
                        var electricity = entries.Sum(x => x.Electricity);
                        var eigenverbrauch = entries.Sum(x => x.SelfConsumption);
                        var quota = eigenverbrauch / electricity;
                        var endReading = DateTime.Now;
                        lock (evcol) {
                            evcol.AddQuotaValue(sufo.Name, quota);
                            evcol.AddEnergyValue(sufo.Name, electricity);
                        }
                        var endLocking = DateTime.Now;
                        var forReading = endReading - taskStart;
                        var forLocking = endLocking - endReading;
                        var taskDuration = endLocking - taskStart;
                        Logger.Info("#" + tasknumber + ", for reading:" + forReading.TotalSeconds + ", for locking: " +
                                    forLocking.TotalSeconds + ", total:" + taskDuration.TotalSeconds);
                    }
                });
#pragma warning restore CC0022 // Should dispose object
                myTask.Start();
                tasks.Add(myTask);
            }
            while (tasks.Count > 0) {
                if (tasks[0].IsCompleted) {
                    tasks[0].Dispose();
                    tasks.RemoveAt(0);
                }
            }
            evcol.SortValues();
            return evcol;
        }

        public class EigenverbrauchsColumn {
            public EigenverbrauchsColumn(string name, string avgName) {
                Name = name;
                AvgName = avgName;
            }

            public double AverageEnergy => EnergyUse.Average(x => x.Item2);

            public double AverageQuota => Quotas.Average(x => x.Item2) * 100;
            public string AvgName { get; }
            public List<Tuple<string, double>> EnergyUse { get; } = new List<Tuple<string, double>>();

            public string Name { get; }
            public List<Tuple<string, double>> Quotas { get; } = new List<Tuple<string, double>>();

            public void AddEnergyValue(string hhname, double val) {
                EnergyUse.Add(
                    new Tuple<string, double>(hhname, val));
            }

            public void AddQuotaValue(string hhname, double val) {
                Quotas.Add(new Tuple<string, double>(hhname, val));
            }

            public void SortValues() {
                Quotas.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                EnergyUse.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            }
        }
    }
}*/