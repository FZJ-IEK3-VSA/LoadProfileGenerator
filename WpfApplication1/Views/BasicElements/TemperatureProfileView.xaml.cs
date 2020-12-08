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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
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

namespace LoadProfileGenerator.Views.BasicElements {
    /// <summary>
    ///     Interaktionslogik f�r TemperatureView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class TemperatureProfileView {
        [JetBrains.Annotations.NotNull] private DateTimeAxis _dateTimeAxis;
        [JetBrains.Annotations.NotNull] private LinearAxis _linearAxis;
        [JetBrains.Annotations.NotNull] private PlotModel _plot;

        public TemperatureProfileView() {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private TemperatureProfilePresenter Presenter => (TemperatureProfilePresenter)DataContext;

        private void Adddatapoint_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (!DateTime.TryParse(TxtTime.Text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var ts)) {
                MessageWindowHandler.Mw.ShowInfoMessage("Could not interpret the date/time. Try for example: 1.1.2012 00:00:01",
                    "Fail!");
                return;
            }
            if (!double.TryParse(TxtValue.Text, out var val)) {
                MessageWindowHandler.Mw.ShowInfoMessage("Could not interpret the value. Try for example: 1", "Error");
                return;
            }
            Presenter.AddTemperaturePoint(ts, val);
            RefreshZGraph();
        }

        private void Browse_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            FileDialog fd = new OpenFileDialog();
            fd.ShowDialog();
            var f = fd.FileName;
            TxtFilePath.Text = f;
            Presenter.CsvImporter.FileName = f;
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.AskDeleteQuestion(
            Presenter.ThisProfile.HeaderString, Presenter.Delete);

        private void ImportData_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (Presenter.CsvImporter.Entries.Count == 0) {
                Logger.Error("Importing zero entries is not very useful.");
                return;
            }
            var dr =
                MessageWindowHandler.Mw.ShowYesNoMessage(
                    "Add these " + Presenter.CsvImporter.Entries.Count + " entries as data points for this profile?",
                    "Add?");
            if (dr == LPGMsgBoxResult.No) {
                return;
            }
            Presenter.ImportData();
        }

        private void RefreshPreview_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.CsvImporter.RefreshEntries();

        private void RefreshZGraph() {
            _plot.Series.Clear();
            // Build the Chart
            // Get a reference to the GraphPane
            var lineSeries1 = new LineSeries
            {
                Title = string.Empty,
                MarkerFill = OxyColor.FromRgb(255, 0, 0),
                MarkerSize = 2,
                MarkerStroke = OxyColor.FromRgb(255, 0, 0),
                MarkerStrokeThickness = 1.5,
                MarkerType = MarkerType.Circle
            };
            _plot.Series.Add(lineSeries1);
            var prof = Presenter.ThisProfile.TemperatureValues;

            foreach (var tp1 in prof) {
                var x = DateTimeAxis.ToDouble(tp1.Time);

                var y = tp1.Value;
                var bottom = new DataPoint(x, y);
                lineSeries1.Points.Add(bottom);
            }

            foreach (var axis in _plot.Axes) {
                axis.Reset();
            }

            _plot.InvalidatePlot(true);
        }

        private void RemoveAlldatapoint_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            var dr =
                MessageWindowHandler.Mw.ShowYesNoMessage(
                    "Delete all " + Presenter.ThisProfile.TemperatureValues.Count + " data points?",
                    "Delete?");
            if (dr == LPGMsgBoxResult.No) {
                return;
            }
            Presenter.DeleteAllDataPoints();
        }

        private void Removedatapoint_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (LstTimePoints.SelectedItem == null) {
                return;
            }
            Presenter.RemoveTimepoint((TemperatureValue) LstTimePoints.SelectedItem);
            RefreshZGraph();
        }

        private void UserControl_Loaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            _plot = new PlotModel();
            _dateTimeAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom
            };
            _plot.Axes.Add(_dateTimeAxis);
            _linearAxis = new LinearAxis();
            _plot.Axes.Add(_linearAxis);

            var pv = new PlotView
            {
                Model = _plot
            };

            ChartGrid.Children.Add(pv);
            RefreshZGraph();
        }
    }
}