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
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;

namespace LoadProfileGenerator.Views.Households
{
    /// <summary>
    ///     Interaktionslogik f�r DevicePickerView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class DeviceSelectionView
    {
        public DeviceSelectionView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private DeviceSelectionPresenter Presenter => (DeviceSelectionPresenter)DataContext;

        private void BtnAddDeviceActionClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbDeviceActionGroups.SelectedItem == null)
            {
                return;
            }
            if (CmbDeviceActions.SelectedItem == null)
            {
                return;
            }
            var da = (DeviceAction)CmbDeviceActions.SelectedItem;
            var dag = (DeviceActionGroup)CmbDeviceActionGroups.SelectedItem;
            Presenter.AddAction(dag, da);
        }

        private void BtnAddDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbDeviceCategories.SelectedItem == null)
            {
                return;
            }
            if (CmbDevices.SelectedItem == null)
            {
                return;
            }
            var rd = (RealDevice)CmbDevices.SelectedItem;
            var dc = (DeviceCategory)CmbDeviceCategories.SelectedItem;
            Presenter.AddItem(dc, rd);
        }

        private void BtnRemoveDeviceActionClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstDeviceActionSelections.SelectedItem == null)
            {
                return;
            }
            var dsi = (DeviceSelectionDeviceAction)LstDeviceActionSelections.SelectedItem;
            Presenter.DeleteAction(dsi);
        }

        private void BtnRemoveDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstDeviceSelections.SelectedItem == null)
            {
                return;
            }
            var dsi = (DeviceSelectionItem)LstDeviceSelections.SelectedItem;
            Presenter.DeleteItem(dsi);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.AskDeleteQuestion(
            Presenter.ThisDeviceSelection.HeaderString, Presenter.Delete);

        private void LstDeviceActionSelections_SelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstDeviceActionSelections.SelectedItem == null)
            {
                return;
            }
            var dsi = (DeviceSelectionDeviceAction)LstDeviceActionSelections.SelectedItem;
            Presenter.SelectedDeviceActionGroup = dsi.DeviceActionGroup;
            Presenter.SelectedDeviceAction = dsi.DeviceAction;
        }

        private void LstDeviceSelections_SelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstDeviceSelections.SelectedItem == null)
            {
                return;
            }
            var dsi = (DeviceSelectionItem)LstDeviceSelections.SelectedItem;
            Presenter.SelectedDeviceCategory = dsi.DeviceCategory;
            Presenter.SelectedDevice = dsi.Device;
        }
    }
}