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
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;
using Microsoft.Win32;

#endregion

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik f�r DeviceView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class DeviceView {
        public DeviceView()
        {
            InitializeComponent();
        }

        [NotNull]
        private DevicePresenter Presenter => (DevicePresenter) DataContext;

        private void BtnAddLoad_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbLoadTypes.SelectedItem == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtMaximumPower.Text)) {
                Logger.Warning("No maximum power defined.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtStandardDeviation.Text)) {
                Logger.Warning("No standard deviation defined.");
                return;
            }

            var loadType = (VLoadType) CmbLoadTypes.SelectedItem;

            var maxpower = Utili.ConvertToDoubleWithMessage(TxtMaximumPower.Text);
            var standardDeviation = Utili.ConvertToDoubleWithMessage(TxtStandardDeviation.Text);
            var averageYearlyConsumption = Utili.ConvertToDoubleWithMessage(TxtAverageYearlyConsumption.Text);
            Presenter.AddRealDeviceLoadType(loadType, maxpower, standardDeviation, averageYearlyConsumption);
        }

        private void BtnBrowseForFile([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog {
                DefaultExt = ".jpg",
                Filter = "JPEG Files (*.jpg)|*.jpg|All files (*.*)|*.*"
            };
            var result = ofd.ShowDialog();
            if (result == true) {
                var filename = ofd.FileName;
                Presenter.ThisDevice.Picture = filename;
            }
        }

        private void BtnFixToCosPhi095_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var rdload =
                Presenter.ThisDevice.Loads.FirstOrDefault(
                    x => string.Equals(x.Name, "electricity", StringComparison.OrdinalIgnoreCase));
            if (rdload == null) {
                Logger.Warning("No electricity load type found.");
                return;
            }

            const double factor = 0.95;
            var apparentPower = rdload.MaxPower / factor;
            {
                var appLoad =
                    Presenter.ThisDevice.Loads.FirstOrDefault(
                        x => string.Equals(x.Name, "apparent", StringComparison.OrdinalIgnoreCase));

                if (appLoad?.LoadType != null) {
                    if (Math.Abs(appLoad.MaxPower - apparentPower) > 0.01) {
                        Presenter.ThisDevice.AddLoad(appLoad.LoadType, apparentPower, appLoad.StandardDeviation,
                            appLoad.AverageYearlyConsumption);
                    }
                }
                else {
                    var apparent = Presenter.Simulator.LoadTypes.FindFirstByName("apparent", FindMode.Partial);
                    if (apparent == null) {
                        Logger.Error("Could not find the apparent load type. Please fix.");
                        return;
                    }

                    Presenter.ThisDevice.AddLoad(apparent, apparentPower, rdload.StandardDeviation,
                        rdload.AverageYearlyConsumption / factor);
                }
            }
            {
                var reacLoad =
                    Presenter.ThisDevice.Loads.FirstOrDefault(
                        x => string.Equals(x.Name, "reactive", StringComparison.OrdinalIgnoreCase));
                var reactivePower = Math.Sqrt(apparentPower * apparentPower - rdload.MaxPower * rdload.MaxPower);
                if (reacLoad?.LoadType != null) {
                    if (Math.Abs(reacLoad.MaxPower - reactivePower) > 0.1) {
                        Presenter.ThisDevice.AddLoad(reacLoad.LoadType, reactivePower, reacLoad.StandardDeviation,
                            reacLoad.AverageYearlyConsumption);
                    }
                }
                else {
                    var reactive = Presenter.Simulator.LoadTypes.FindFirstByName("reactive", FindMode.Partial);
                    if (reactive == null) {
                        Logger.Error("Could not find the apparent load type. Please fix.");
                        return;
                    }

                    Presenter.ThisDevice.AddLoad(reactive, reactivePower, rdload.StandardDeviation,
                        rdload.AverageYearlyConsumption);
                }
            }
        }

        private void BtnImportDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedImportDevice == null) {
                Logger.Info("Please select a device to import first.");
                return;
            }

            Presenter.ImportDevice();
        }

        private void BtnRefreshUses_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.RefreshUsedIns();
            LstUses.ResizeColummns();
        }

        private void BtnRemoveLoad_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstLoads.SelectedItem == null) {
                return;
            }

            var rdlt = (RealDeviceLoadType) LstLoads.SelectedItem;
            Presenter.DeleteLoad(rdlt);
        }

        private void ButtonBase_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var action = Presenter.ThisDevice.MakeDeviceAction(Presenter.Simulator);

            Presenter.ApplicationPresenter.OpenItem(action);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void CreateCopy_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.CreateCopy();

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisDevice.HeaderString, Presenter.Delete);

        private void LstUsedIns_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstUses.SelectedItem == null) {
                return;
            }

            var ui = (UsedIn) LstUses.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }

        private void UserControl_Loaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter
            .DeviceCategoryPickerPresenter
            .Select();
    }
}