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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;

#endregion

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik f�r DeviceView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class DeviceCategoryView {
        public DeviceCategoryView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private DeviceCategoryPresenter Presenter => (DeviceCategoryPresenter) DataContext;

        private void BtnRefreshDevices_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter
            .ThisDeviceCategory
            .RefreshSubDevices();

        private void BtnRefreshHouseholds_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshUsedIn();

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisDeviceCategory.Name, Presenter.Delete);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void DeviceCategoryPicker1_Loaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            DeviceCategoryPicker1.Presenter
                .PropertyChanged += SelectedItemOnPropertyChanged;

        private void LstHouseholds_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstUsedIn.SelectedItem == null) {
                return;
            }

            var dc = (UsedIn) LstUsedIn.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(dc.Item);
        }

        private void LstPersonDesiresMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var o = LstDevicesInCategory.SelectedItem;
            if (o == null) {
                return;
            }

            Presenter.ApplicationPresenter.OpenItem(o);
        }

        private void SelectedItemOnPropertyChanged([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "SelectedItem" && DeviceCategoryPicker1.Presenter.SelectedItem != null) {
                Presenter.SelectedParentCategory = DeviceCategoryPicker1.Presenter.SelectedItem;
            }
        }

        private void UserControl_Loaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.DeviceCategoryPicker.Select();
    }
}