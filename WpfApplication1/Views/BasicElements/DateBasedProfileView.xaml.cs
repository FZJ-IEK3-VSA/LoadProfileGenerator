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
using System.IO;
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
    ///     Interaktionslogik f�r DateBasedProfileView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class DateBasedProfileView {
        [JetBrains.Annotations.NotNull] private DateTimeAxis _dateTimeAxis;
        [JetBrains.Annotations.NotNull] private LinearAxis _linearAxis;

        [JetBrains.Annotations.NotNull] private PlotModel _plot;

        public DateBasedProfileView() {
            InitializeComponent();
        }
        [JetBrains.Annotations.NotNull]
        private DateBasedProfilePresenter Presenter => (DateBasedProfilePresenter)DataContext;

        private void Adddatapoint_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(TxtTime.Text)) {
                Logger.Error("Enter a date first!");
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtValue.Text)) {
                Logger.Error("Enter a value first!");
                return;
            }
            var success = DateTime.TryParse(TxtTime.Text, out var dt);
            if (!success) {
                Logger.Error("Could not convert " + TxtTime.Text);
            }
            success = double.TryParse(TxtValue.Text, out double d);
            if (!success) {
                Logger.Error("Could not convert " + TxtValue.Text);
            }
            Presenter.AddDataPoint(dt, d);
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

        private void ExportToCSV_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            var sfd = new SaveFileDialog
            {
                FileName = "CSVExport.csv",
                Filter = "*.csv|*.csv|*.*|*.*",
                Title = "Save Profile as CSV"
            };
            sfd.ShowDialog();
            if (!string.IsNullOrEmpty(sfd.FileName)) {
                try {
                    var dp = Presenter.ThisProfile.Datapoints;
                    using (var sw = new StreamWriter(sfd.FileName)) {
                        foreach (var datapoint in dp) {
                            var s = datapoint.DateAndTime.ToString(CultureInfo.CurrentCulture) + ";" +
                                    datapoint.Value.ToString(CultureInfo.CurrentCulture);

                            sw.WriteLine(s);
                        }
                    }
                }
                catch (Exception ex) {
                    Logger.Exception(ex);
                }
            }
        }

        private void ImportData_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (string.IsNullOrEmpty(TxtFilePath.Text)) {
                Logger.Error("You need to select a file first!");
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
            RefreshZGraph();
        }

        private void RefreshPreview_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.CsvImporter.RefreshEntries();

        private void RefreshZGraph() {
            _plot.Series.Clear();
            var lineSeries1 = new LineSeries
            {
                Title = string.Empty,
                MarkerFill = OxyColor.FromRgb(255, 0, 0),
                MarkerSize = 1,
                MarkerStroke = OxyColor.FromRgb(255, 0, 0),
                MarkerStrokeThickness = 1.5,
                MarkerType = MarkerType.Circle
            };
            _plot.Series.Add(lineSeries1);
            var prof = Presenter.ThisProfile.Datapoints;
            foreach (var tp1 in prof) {
                var x = DateTimeAxis.ToDouble(tp1.DateAndTime);
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
                    "Delete all " + Presenter.ThisProfile.Datapoints.Count + " data points?", "Delete?");
            if (dr == LPGMsgBoxResult.No) {
                return;
            }
            Presenter.DeleteAllDataPoints();
        }

        private void Removedatapoint_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (LstTimePoints.SelectedItem == null) {
                return;
            }
            Presenter.RemoveTimepoint((DateProfileDataPoint) LstTimePoints.SelectedItem);
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