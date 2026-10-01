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

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Automation.ResultFiles;
using Common;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls.Usercontrols;
using LoadProfileGenerator.Presenters.Houses;

namespace LoadProfileGenerator.Views.Houses {
    /// <summary>
    ///     Interaktionslogik f�r HouseTypeView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class HouseTypeView {
        internal HouseTypeView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private HouseTypePresenter Presenter => (HouseTypePresenter) DataContext;

        private void BtnAddEnergyStorage_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbEnergyStorageDevices.SelectedItem == null) {
                return;
            }

            var es = (EnergyStorage) CmbEnergyStorageDevices.SelectedItem;
            Presenter.ThisHouseType.AddEnergyStorage(es);
        }

        private void BtnAddGenerator_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbGenerator.SelectedItem == null) {
                return;
            }

            var gen = (Generator) CmbGenerator.SelectedItem;
            Presenter.ThisHouseType.AddGenerator(gen);
        }

        private void BtnAddTransformer_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbTransformationDevices.SelectedItem == null) {
                return;
            }

            var td = (TransformationDevice) CmbTransformationDevices.SelectedItem;
            Presenter.ThisHouseType.AddTransformationDevice(td);
        }

        private void BtnRefreshHouseholds_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshUsedIn();

        private void BtnRemoveGenerator_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstGenerators.SelectedItem == null) {
                return;
            }

            var hgen = (HouseTypeGenerator) LstGenerators.SelectedItem;
            Presenter.ThisHouseType.DeleteHouseGenerator(hgen);
        }

        private void BtnRemoveHouseEnergyStorage_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstEnergyStorageDevices.SelectedItem == null) {
                return;
            }

            var hes = (HouseTypeEnergyStorage) LstEnergyStorageDevices.SelectedItem;
            Presenter.ThisHouseType.DeleteHouseEnergyStorage(hes);
        }

        private void BtnRemoveTransformer_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstTransformDevices.SelectedItem == null) {
                return;
            }

            var td = (HouseTypeTransformationDevice) LstTransformDevices.SelectedItem;
            Presenter.ThisHouseType.DeleteHouseTransformationDeviceFromDB(td);
        }

        private void ChangedHeating([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e) =>
            Presenter.Refresh();

        private void ChangedHeatingText([CanBeNull] object sender, [CanBeNull] TextChangedEventArgs e) =>
            Presenter.Refresh();

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool CheckCombobox([CanBeNull] object box, [JetBrains.Annotations.NotNull] string s)
        {
            if (box == null) {
                Logger.Error("Please select a " + s + " to continue");
                return false;
            }

            return true;
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.AskDeleteQuestion(Presenter.ThisHouseType.HeaderString, Presenter.Delete);

#pragma warning disable CC0068 // Unused Method
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void DeviceSelectorControl_OnOnAddedDevice([JetBrains.Annotations.NotNull] object sender,
            [JetBrains.Annotations.NotNull] DeviceSelectorControl.DeviceAddedEventArgs e)
        {
            if (!CheckCombobox(AutoDevs.AssignableDevice, "Device")) {
                return;
            }

            if (!CheckCombobox(AutoDevs.TimeLimit, "Time Limit")) {
                return;
            }

            if (!CheckCombobox(AutoDevs.Location, "Location")) {
                return;
            }

            if (AutoDevs.SelectedDeviceType == AssignableDeviceType.Device ||
                AutoDevs.SelectedDeviceType == AssignableDeviceType.DeviceCategory) {
                if (!CheckCombobox(AutoDevs.TimeBasedProfile, "TimeBasedProfile")) {
                    return;
                }

                if (!CheckCombobox(AutoDevs.LoadType, "LoadType")) {
                    return;
                }
            }

            var adev = AutoDevs.AssignableDevice;
            if (adev == null) {
                throw new LPGException("Bug: adev should never be null");
            }
            var tp = AutoDevs.TimeBasedProfile;
            var timeStandardDeviation = AutoDevs.TimeDeviation;
            var vlt = AutoDevs.LoadType;
            var loc = (Location) AutoDevs.Location;
            var timeLimit = AutoDevs.TimeLimit;
            var tc = AutoDevs.VariableCondition;
            var variable = AutoDevs.SelectedVariable;
            if (timeLimit == null) {
                Logger.Error("no time limit set, not adding");
                return;
            }

            Presenter.ThisHouseType.AddHouseTypeDevice(adev, timeLimit, tp, (double) timeStandardDeviation, vlt, loc,
                AutoDevs.VariableValue, tc, variable);
        }
#pragma warning restore CC0068 // Unused Method

        private void LstEnergyStorageDevices_OnMouseDoubleClick([CanBeNull] object sender,
            [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstEnergyStorageDevices.SelectedItem == null) {
                return;
            }

            var htstor = (HouseTypeEnergyStorage) LstEnergyStorageDevices.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(htstor.EnergyStorage);
        }

        private void LstGenerators_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstGenerators.SelectedItem == null) {
                return;
            }

            var ui = (HouseTypeGenerator) LstGenerators.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Generator);
        }

        private void LstTransformDevices_OnMouseDoubleClick([CanBeNull] object sender,
            [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstTransformDevices.SelectedItem == null) {
                return;
            }

            var httd = (HouseTypeTransformationDevice) LstTransformDevices.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(httd.TransformationDevice);
        }

        private void LstTypeUsedBy_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstTypeUsedBy.SelectedItem == null) {
                return;
            }

            var ui = (UsedIn) LstTypeUsedBy.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }
#pragma warning disable CC0068 // Unused Method
#pragma warning disable S1144 // Unused private types or members should be removed
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void AutoDevs_Loaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => AutoDevs.Simulator = Presenter.ApplicationPresenter.Simulator;
#pragma warning restore S1144 // Unused private types or members should be removed

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void AutoDevs_OnOnRemovedDevice([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] DeviceSelectorControl.DeviceRemovedEventArgs e)
        {
            if (e.ItemToRemove is HouseTypeDevice ad) {
                Presenter.ThisHouseType.DeleteHouseDeviceFromDB(ad);
            }
        }
#pragma warning restore CC0068 // Unused Method
        private void BtnMakeACopy(object sender, RoutedEventArgs e)
        {
            Presenter.MakeACopy();
        }
    }
}