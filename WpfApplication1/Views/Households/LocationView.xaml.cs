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

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Automation.ResultFiles;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;

#endregion

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik f�r LocationView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class LocationView {
        public LocationView()
        {
            InitializeComponent();
        }

        [NotNull]
        private LocationPresenter Presenter => (LocationPresenter) DataContext;

        private void BtnAddDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbDeviceOrCategory.SelectedItem == null) {
                return;
            }

            if (CmbDeviceOrCategory.SelectedItem.ToString() == "Device" && CmbDevices.SelectedItem == null) {
                return;
            }

            if (CmbDeviceOrCategory.SelectedItem.ToString() == "Device Category" &&
                CmbDeviceCategories.SelectedItem == null) {
                return;
            }

            IAssignableDevice adev;
            switch (CmbDeviceOrCategory.SelectedItem.ToString()) {
                case "Device":
                    adev = (RealDevice) CmbDevices.SelectedItem;
                    break;
                case "Device Category":
                    adev = (DeviceCategory) CmbDeviceCategories.SelectedItem;
                    break;
                default:
                    throw new LPGException("Unknown Device Source");
            }

            Presenter.AddDevice(adev);
        }

        private void BtnRefreshHouseholds_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshUses();

        private void BtnRemoveDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstDevs.SelectedItem == null) {
                return;
            }

            var ld = (LocationDevice) LstDevs.SelectedItem;
            Presenter.RemoveDevice(ld);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void CmbDeviceOrCategory_OnSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems.Count > 0) {
                Presenter.SelectedAddCategory = (string) e.AddedItems[0];
            }
        }

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisLocation.HeaderString, Presenter.Delete);

        private void LstDevs_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstDevs.SelectedItem == null) {
                return;
            }

            var ld = (LocationDevice) LstDevs.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ld.Device);
        }

        private void LstLocationHouseholds_MouseDoubleClick([CanBeNull] object sender,
            [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstLocationHouseholds.SelectedItem == null) {
                return;
            }

            var o = LstLocationHouseholds.SelectedItem;
            var ui = (UsedIn) o;

            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }
    }
}