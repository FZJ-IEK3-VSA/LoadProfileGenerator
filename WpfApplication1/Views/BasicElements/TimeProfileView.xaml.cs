//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using DateTimeAxis = OxyPlot.Axes.DateTimeAxis;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;

#endregion

namespace LoadProfileGenerator.Views.BasicElements {
    /// <summary>
    ///     Interaktionslogik f�r TimeProfile.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class TimeProfileView {
        [JetBrains.Annotations.NotNull] private DateTimeAxis _dateTimeAxis;
        [JetBrains.Annotations.NotNull] private LinearAxis _linearAxis;
        [JetBrains.Annotations.NotNull] private PlotModel _plot;

        public TimeProfileView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private TimeProfilePresenter Presenter => (TimeProfilePresenter) DataContext;

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void Adddatapoint_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (!TimeSpan.TryParse(TxtTime.Text, out var ts)) {
                MessageWindowHandler.Mw.ShowInfoMessage("Could not interpret the time. Try for example: 00:00:01", "Error");
                return;
            }

            if (!double.TryParse(TxtValue.Text, out double val)) {
                MessageWindowHandler.Mw.ShowInfoMessage("Could not interpret the value. Try for example: 1", "Error");
                return;
            }

            Presenter.AddTimepoint(ts, val);
            RefreshZGraph();
        }

        private void Browse_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            fd.ShowDialog();
            var f = fd.FileName;
            TxtFilePath.Text = f;
            Presenter.CsvImporter.FileName = f;
        }

        private void BtnRefresUsedIns_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.RefreshUsedIn();
            LstUsedIn.ResizeColummns();
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisProfile.HeaderString, Presenter.Delete);

        private void Fixlastdatapoint_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var tps = Presenter.ThisProfile.ObservableDatapoints;
            var lasttp = tps[tps.Count - 1];
            var secondlasttp = tps[tps.Count - 2];
            Presenter.ThisProfile.AddNewTimepoint(lasttp.Time, secondlasttp.Value);
            Presenter.ThisProfile.DeleteTimepoint(lasttp);
            RefreshZGraph();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void ImportData_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var dr =
                MessageWindowHandler.Mw.ShowYesNoMessage(
                    "Add these " + Presenter.CsvImporter.Entries.Count + " entries as time points for this profile?",
                    "Add?");
            if (dr == LPGMsgBoxResult.No) {
                return;
            }

            Presenter.ImportData(RefreshZGraph);
        }

        private void LstUsedByMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var ui = LstUsedIn.SelectedItem as UsedIn;
            if (ui?.Item != null) {
                Presenter.ApplicationPresenter.OpenItem(ui.Item);
            }
        }

        private void RefreshChartClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => RefreshZGraph();

        private void RefreshPreview_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.CsvImporter.RefreshEntries();

        private void RefreshZGraph()
        {
            _plot.Series.Clear();
            // Build the Chart
            // Get a reference to the GraphPane
            var lineSeries1 = new LineSeries {
                Title = string.Empty,
                MarkerFill = OxyColor.FromRgb(255, 0, 0),
                MarkerSize = 5,
                MarkerStroke = OxyColor.FromRgb(255, 0, 0),
                MarkerStrokeThickness = 1.5,
                MarkerType = MarkerType.Circle
            };
            _plot.Series.Add(lineSeries1);
            var prof = Presenter.ThisProfile;
            for (var j = 0; j < prof.DatapointsCount; j++) {
                var tp1 = prof.ObservableDatapoints[j];
                var dt = new DateTime(2000, 1, 1).Add(tp1.Time);
                var x = DateTimeAxis.ToDouble(dt);

                var y = tp1.Value;
                var bottom = new DataPoint(x, y);
                lineSeries1.Points.Add(bottom);
                if (j < prof.DatapointsCount - 1) {
                    var tp2 = prof.ObservableDatapoints[j + 1];
                    var dt2 = new DateTime(2000, 1, 1).Add(tp2.Time).AddMilliseconds(-1);
                    var x2 = DateTimeAxis.ToDouble(dt2);
                    var y2 = tp1.Value;
                    var bottom2 = new DataPoint(x2, y2);
                    lineSeries1.Points.Add(bottom2);
                }
            }

            foreach (var axis in _plot.Axes) {
                axis.Reset();
            }

            if (prof.ObservableDatapoints.Count > 0) {
                var first = prof.ObservableDatapoints[0];
                var firstdt = new DateTime(2000, 1, 1).Add(first.Time).AddSeconds(-30);
                var minTime = DateTimeAxis.ToDouble(firstdt);
                _dateTimeAxis.Minimum = minTime;
                _linearAxis.Minimum = prof.ObservableDatapoints.Select(x => x.Value).Min() * 0.90;
                if (Math.Abs(_linearAxis.Minimum) < 0.0000001) {
                    _linearAxis.Minimum = -0.1;
                }

                var last = prof.ObservableDatapoints.Last();
                var lastdt = new DateTime(2000, 1, 1).Add(last.Time).AddSeconds(30);
                var maxTime = DateTimeAxis.ToDouble(lastdt);
                _dateTimeAxis.Maximum = maxTime;
                _linearAxis.Maximum = prof.ObservableDatapoints.Select(x => x.Value).Max() * 1.10;
            }

            _plot.InvalidatePlot(true);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void RemoveAlldatapoint_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var dr =
                MessageWindowHandler.Mw.ShowYesNoMessage(
                    "Delete all " + Presenter.ThisProfile.ObservableDatapoints.Count + " data points?",
                    "Delete?");
            if (dr == LPGMsgBoxResult.No) {
                return;
            }

            Presenter.DeleteAllDataPoints();
            RefreshZGraph();
        }

        private void Removedatapoint_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstTimePoints.SelectedItem == null) {
                return;
            }

            Presenter.RemoveTimepoint((TimeDataPoint) LstTimePoints.SelectedItem);
            RefreshZGraph();
        }

        private void UserControl_Loaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            _plot = new PlotModel();
            _dateTimeAxis = new DateTimeAxis {
                Position = AxisPosition.Bottom
            };
            _plot.Axes.Add(_dateTimeAxis);
            _linearAxis = new LinearAxis();
            _plot.Axes.Add(_linearAxis);
            var pv = new PlotView {
                Model = _plot
            };
            ChartGrid.Children.Add(pv);
            RefreshZGraph();
        }
    }
}