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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls.Usercontrols;
using LoadProfileGenerator.Presenters.Households;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using LinearAxis = OxyPlot.Axes.LinearAxis;

#endregion

namespace LoadProfileGenerator.Views.Households {

    public static class LPGExtensions {
        public static Color GetColor([JetBrains.Annotations.NotNull] this ColorRGB color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        [JetBrains.Annotations.NotNull]
        public static ColorRGB GetColorRGB(this System.Windows.Media.Color color)
        {
            return new ColorRGB(color.A,color.R,color.G,color.B);
        }
    }
    /// <summary>
    ///     Interaktionslogik f�r AffordanceView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class AffordanceView {
        [CanBeNull] private CategoryAxis _categoryAxis;

        [CanBeNull] private LinearAxis _dateTimeAxis;

        [CanBeNull] private PlotModel _plot;

        public AffordanceView()
        {
            var a = new Action(() => InitializeComponent());
            Dispatcher.BeginInvoke(a);
        }

        [JetBrains.Annotations.NotNull]
        private AffordancePresenter Presenter => (AffordancePresenter) DataContext;

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public ScrollViewer ScViewer => ScrollViewer1;

        private void BtnAddDesireClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            LstAffordanceDesires.ResizeColummns();
            if (CmbDesires.SelectedItem == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtSatisfactionValue.Text)) {
                return;
            }

            var success = decimal.TryParse(TxtSatisfactionValue.Text, out var d);
            if (!success) {
                Logger.Error("Could not convert " + TxtSatisfactionValue.Text + " to double");
            }

            d /= 100;
            Presenter.AddDesire((Desire) CmbDesires.SelectedItem, d);
        }

        private void BtnAddSubAffordanceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbSubAffordances.SelectedItem == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtDelayTime.Text)) {
                return;
            }

            var strDelayTime = TxtDelayTime.Text;
            var delaytime = Utili.ConvertToDecimalWithMessage(strDelayTime);
            var subaff = (SubAffordance) CmbSubAffordances.SelectedItem;
            Presenter.AddSubAffordance(subaff, delaytime);
        }

        private void BtnAddVariableOpClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtOpValue.Text)) {
                return;
            }

            if (CmbOpAction.SelectedItem == null) {
                return;
            }

            if (CmbOpVariable.SelectedItem == null) {
                return;
            }

            if (CmbOpExecutionTime.SelectedItem == null) {
                return;
            }

            Location loc = null;
            if (CmbOpLocation.SelectedItem != null) {
                loc = (Location) CmbOpLocation.SelectedItem;
            }

            var mode = (VariableLocationMode) CmbOpLocationMode.SelectedItem;
            var success = double.TryParse(TxtOpValue.Text, out double val);
            if (!success) {
                Logger.Error("Could not convert " + TxtOpValue.Text + " to double");
            }

            var variableAction = (VariableAction) CmbOpAction.SelectedItem;
            var v = (Variable) CmbOpVariable.SelectedItem;
            var desc = string.Empty;
            if (!string.IsNullOrWhiteSpace(TxtOpDesc.Text)) {
                desc = TxtOpDesc.Text;
            }

            var executionTime =
                VariableExecutionTimeHelper.ConvertToEnum((string) CmbOpExecutionTime.SelectedItem);
            Presenter.ThisAffordance.AddVariableOperation(val, mode, loc, variableAction, v, desc, executionTime);
            LstOpVariables.ResizeColummns();
        }

        private void BtnAddVariableReqClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtReqVariableValue.Text)) {
                return;
            }

            if (CmbReqCondition.SelectedItem == null) {
                return;
            }

            if (CmbReqVariable.SelectedItem == null) {
                return;
            }

            Location loc = null;
            if (CmbReqLocation.SelectedItem != null) {
                loc = (Location) CmbReqLocation.SelectedItem;
            }

            var mode = (VariableLocationMode) CmbReqLocationMode.SelectedItem;
            var success = double.TryParse(TxtReqVariableValue.Text, out double val);
            if (!success) {
                Logger.Error("Could not convert " + TxtReqVariableValue.Text + " to double");
            }

            var variableCondition =
                VariableConditionHelper.ConvertToVariableCondition((string) CmbReqCondition.SelectedItem);
            var va = (Variable) CmbReqVariable.SelectedItem;
            var desc = string.Empty;
            if (!string.IsNullOrWhiteSpace(TxtReqDesc.Text)) {
                desc = TxtReqDesc.Text;
            }

            Presenter.ThisAffordance.AddVariableRequirement(val, mode, loc, variableCondition, va, desc);
            LstReqVariables.ResizeColummns();
        }

        private void BtnCreateNewDesireClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var des =
                Presenter.Sim.Desires.CreateNewItem(Presenter.Sim.ConnectionString);
            Presenter.AddDesire(des, 1);
            des.Name = "Desire for " + Presenter.ThisAffordance.Name;
            Presenter.ApplicationPresenter.OpenItem(des);
        }

        private void BtnCreateSubAffordanceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.CreateNewSubaffordance();
        }

        private void BtnImportAffordanceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.ImportAffordance();
        }

        //private void BtnPickColorClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        //{
        //    using (var cd = new ColorDialog()) {
        //        var mediaColor = Presenter.ThisAffordance.CarpetPlotColor;

        //        var c =
        //            Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        //        cd.Color = c;
        //        cd.ShowDialog();
        //        var c2 = cd.Color;
        //        var rescolor = System.Windows.Media.Color.FromArgb(c2.A, c2.R, c2.G, c2.B);
        //        Presenter.ThisAffordance.CarpetPlotColor = rescolor.GetColorRGB();
        //        Presenter.ThisAffordance.SaveToDB();
        //    }
        //}

        private void BtnRefreshHouseholds_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.RefreshUsedIn();
            LstUsedIn.ResizeColummns();
        }

        private void BtnRemoveDesireClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstAffordanceDesires.SelectedItem == null) {
                return;
            }

            Presenter.RemoveDesire((AffordanceDesire) LstAffordanceDesires.SelectedItem);
        }

        private void BtnRemoveSubAffordanceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstSubAffordances.SelectedItem == null) {
                return;
            }

            var pt = (AffordanceSubAffordance) LstSubAffordances.SelectedItem;
            Presenter.RemoveSubAffordance(pt);
        }

        private void BtnRemoveVariableOpClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstOpVariables.SelectedItem == null) {
                return;
            }

            var variableOperation = (AffVariableOperation) LstOpVariables.SelectedItem;

            Presenter.ThisAffordance.DeleteVariableOperation(variableOperation);
        }

        private void BtnRemoveVariableReqClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstReqVariables.SelectedItem == null) {
                return;
            }

            var variableRequirement = (AffVariableRequirement) LstReqVariables.SelectedItem;

            Presenter.ThisAffordance.DeleteVariableRequirement(variableRequirement);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.Close(true);
        }

        private void CmbPersonProfileSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            Action a = ()=>RefreshGraph();
            Dispatcher.BeginInvoke(a);
        }

        private void CreateAffordance_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.MakeAffordanceCopy();
        }

        private void CreateTrait_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.CreateTrait();
        }

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.AskDeleteQuestion(Presenter.ThisAffordance.HeaderString, Presenter.Delete);
        }

        private void LstAffordanceDesires_OnMouseDoubleClick([CanBeNull] object sender,
            [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstAffordanceDesires.SelectedItem is AffordanceDesire affdev) {
                Presenter.ApplicationPresenter.OpenItem(affdev.Desire);
            }
        }

        private void LstSubAffordances_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var ui = LstSubAffordances.SelectedItem as AffordanceSubAffordance;
            if (ui?.SubAffordance != null) {
                Presenter.ApplicationPresenter.OpenItem(ui.SubAffordance);
            }
        }

        private void LstUsedByMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var ui = LstUsedIn.SelectedItem as UsedIn;
            if (ui?.Item != null) {
                Presenter.ApplicationPresenter.OpenItem(ui.Item);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void RefreshGraph()
        {
            if (_plot == null || _categoryAxis == null) {
                return;
            }

            _plot.Series.Clear();
            var l = new Legend();
            _plot.Legends.Add(l);
            l.LegendPlacement = LegendPlacement.Outside;
            l.LegendPosition = LegendPosition.BottomCenter;
            l.LegendOrientation = LegendOrientation.Horizontal;
            _categoryAxis.Labels.Clear();
            _categoryAxis.Labels.Add("Person Profile");
            var personSeries = new IntervalBarSeries {
                Title = "Person"
            };

            const double personstart = 0;
            double personEnd = 0;
            if (Presenter.ThisAffordance.PersonProfile != null) {
                personEnd = Presenter.ThisAffordance.PersonProfile.Duration.TotalMinutes;
            }

            personSeries.Items.Add(new IntervalBarItem(personstart, personEnd));
            _plot.Series.Add(personSeries);
            // Build the Chart
            // Get a reference to the GraphPane

            var minSeries = new IntervalBarSeries {
                Title = "Minimum"
            };
            minSeries.Items.Add(new IntervalBarItem(0, 0));
            var bodySeries = new IntervalBarSeries {
                Title = "Always"
            };
            bodySeries.Items.Add(new IntervalBarItem(0, 0));
            var maxSeries = new IntervalBarSeries {
                Title = "Maximum"
            };
            maxSeries.Items.Add(new IntervalBarItem(0, 0));

            var sim = Presenter.Sim;
            var count = 0;
            count++;
            var aff = Presenter.ThisAffordance;
            if (aff == null) {
                throw new LPGException("Affordance was null");
            }

            foreach (var affordanceDevice in aff.AffordanceDevices) {
                if (affordanceDevice.Device.AssignableDeviceType == AssignableDeviceType.DeviceActionGroup) {
                    var minStartTime = new Dictionary<VLoadType, double>();
                    var maxStartTime = new Dictionary<VLoadType, double>();
                    var minEndTime = new Dictionary<VLoadType, double>();
                    var maxEndTime = new Dictionary<VLoadType, double>();
                    var dag = (DeviceActionGroup) affordanceDevice.Device;
                    foreach (var device in dag.GetDeviceActions(sim.DeviceActions.Items)) {
                        foreach (var actionProfile in device.Profiles) {
                            var start = (double) actionProfile.TimeOffset + (double) affordanceDevice.TimeOffset;
                            if (!minStartTime.ContainsKey(actionProfile.VLoadType)) {
                                minStartTime.Add(actionProfile.VLoadType, start);
                            }
                            else if (minStartTime[actionProfile.VLoadType] > start) {
                                minStartTime[actionProfile.VLoadType] = start;
                            }

                            if (!maxStartTime.ContainsKey(actionProfile.VLoadType)) {
                                maxStartTime.Add(actionProfile.VLoadType, start);
                            }
                            else if (maxStartTime[actionProfile.VLoadType] < start) {
                                maxStartTime[actionProfile.VLoadType] = start;
                            }

                            var maxstart = maxStartTime[actionProfile.VLoadType];
                            var length = actionProfile.Timeprofile.Duration.TotalMinutes;
                            var end = maxstart + length;
                            if (!minEndTime.ContainsKey(actionProfile.VLoadType)) {
                                minEndTime.Add(actionProfile.VLoadType, end);
                            }
                            else if (minEndTime[actionProfile.VLoadType] > end) {
                                minEndTime[actionProfile.VLoadType] = end;
                            }

                            if (!maxEndTime.ContainsKey(actionProfile.VLoadType)) {
                                maxEndTime.Add(actionProfile.VLoadType, end);
                            }
                            else if (maxEndTime[actionProfile.VLoadType] < end) {
                                maxEndTime[actionProfile.VLoadType] = end;
                            }
                        }
                    }

                    foreach (var vLoadType in minStartTime.Keys) {
                        minSeries.Items.Add(new IntervalBarItem(minStartTime[vLoadType], maxStartTime[vLoadType]));
                        bodySeries.Items.Add(new IntervalBarItem(maxStartTime[vLoadType], minEndTime[vLoadType]));
                        maxSeries.Items.Add(new IntervalBarItem(minEndTime[vLoadType], maxEndTime[vLoadType]));
                        _categoryAxis.Labels.Add(affordanceDevice.Name + " - " + vLoadType.PrettyName);
                        count++;
                    }
                }

                if (affordanceDevice.Device.AssignableDeviceType == AssignableDeviceType.DeviceCategory) {
                    var start = (double) affordanceDevice.TimeOffset;
                    var end = affordanceDevice.TimeProfile.Duration.TotalMinutes;
                    bodySeries.Items.Add(new IntervalBarItem(start, end));
                    _categoryAxis.Labels.Add(affordanceDevice.Name + " - " + affordanceDevice.LoadType?.PrettyName);
                    count++;
                }

                if (affordanceDevice.Device.AssignableDeviceType == AssignableDeviceType.Device) {
                    var start = (double) affordanceDevice.TimeOffset;
                    var end = start + affordanceDevice.TimeProfile.Duration.TotalMinutes;
                    bodySeries.Items.Add(new IntervalBarItem(start, end));
                    _categoryAxis.Labels.Add(affordanceDevice.Name + " - " + affordanceDevice.LoadType.PrettyName);
                    count++;
                }

                if (affordanceDevice.Device.AssignableDeviceType == AssignableDeviceType.DeviceAction) {
                    var da = (DeviceAction) affordanceDevice.Device;
                    foreach (var realDeviceLoadType in da.Profiles) {
                        var start = (double) affordanceDevice.TimeOffset;
                        var end = start + realDeviceLoadType.Timeprofile.Duration.TotalMinutes;
                        bodySeries.Items.Add(new IntervalBarItem(start, end));
                        _categoryAxis.Labels.Add(
                            affordanceDevice.Name + " - " + realDeviceLoadType.VLoadType.PrettyName);
                        count++;
                    }
                }
            }

            DeviceGrid.RowDefinitions[2].Height = new GridLength(count * 30 + 50);
            _plot.Series.Add(minSeries);
            _plot.Series.Add(bodySeries);
            _plot.Series.Add(maxSeries);
            _plot.InvalidatePlot(true);
        }

        private void UserControl_Loaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Action a = () => {
                MyDeviceSelector.Simulator = Presenter.ApplicationPresenter.Simulator;
                MyDeviceSelector.OpenItem = Presenter.ApplicationPresenter.OpenItem;
                MyStandbySelector.Simulator = Presenter.ApplicationPresenter.Simulator;
                MyStandbySelector.OpenItem = Presenter.ApplicationPresenter.OpenItem;
                _plot = new PlotModel();
                _dateTimeAxis = new LinearAxis {
                    Position = AxisPosition.Bottom
                };
                _plot.Axes.Add(_dateTimeAxis);
                _categoryAxis = new CategoryAxis {
                    MinorStep = 1,
                    Position = AxisPosition.Left,
                    IsZoomEnabled = false,
                    IsPanEnabled = false
                };
                _dateTimeAxis.IsZoomEnabled = false;
                _dateTimeAxis.IsPanEnabled = false;
                _plot.Axes.Add(_categoryAxis);

                var pv = new PlotView {
                    Model = _plot
                };
                DeviceGrid.Children.Add(pv);
                Grid.SetRow(pv, 2);

                RefreshGraph();
            };
            Dispatcher.BeginInvoke(a);
        }

#pragma warning disable CC0068 // Unused Method

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void MyDeviceSelector_OnOnAddedDevice([CanBeNull] object sender,
            [CanBeNull] DeviceSelectorControl.DeviceAddedEventArgs e)
        {
            if (MyDeviceSelector.AssignableDevice == null) {
                Logger.Info("No device selected");
                return;
            }
            Presenter.AddDeviceProfile(MyDeviceSelector.AssignableDevice, MyDeviceSelector.TimeBasedProfile,
                MyDeviceSelector.TimeOffset, MyDeviceSelector.LoadType, MyDeviceSelector.Probability / 100);
            MyDeviceSelector.ResizeColummns();
            RefreshGraph();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void MyDeviceSelector_OnOnRemovedDevice([CanBeNull] object sender,
            [CanBeNull] DeviceSelectorControl.DeviceRemovedEventArgs e)
        {
            if (e == null) {
                return;
            }

            if (!(e.ItemToRemove is AffordanceDevice ad)) {
                Logger.Error("Could not select the affordance device");
                return;
            }

            Presenter.RemoveDeviceAndTimeprofile(ad);
            RefreshGraph();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void MyStandby_OnOnAddedDevice([CanBeNull] object sender,
            [CanBeNull] DeviceSelectorControl.DeviceAddedEventArgs e)
        {
            if (MyStandbySelector.AssignableDevice == null) {
                Logger.Error("No Device selected.");
                return;
            }

            Presenter.ThisAffordance.AddStandby(MyStandbySelector.AssignableDevice);
            MyStandbySelector.ResizeColummns();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void MyStandby_OnOnRemovedDevice([CanBeNull] object sender,
            [CanBeNull] DeviceSelectorControl.DeviceRemovedEventArgs e)
        {
            if (e != null) {
                if (!(e.ItemToRemove is AffordanceStandby ad)) {
                    Logger.Error("Could not select the affordance standby");
                    return;
                }

                Presenter.ThisAffordance.DeleteStandby(ad);
            }
        }

#pragma warning restore CC0068 // Unused Method
    }
}