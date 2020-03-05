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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

#endregion

namespace LoadProfileGenerator.Controls.Usercontrols {
    /// <summary>
    ///     Interaktionslogik f�r DeviceCategoryTree.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class DeviceCategoryPicker {
        [UsedImplicitly] [NotNull] public static readonly DependencyProperty PickedDeviceCategoryProperty =
            DependencyProperty.Register("PickedDeviceCategory", typeof(DeviceCategory), typeof(DeviceCategoryPicker));

        public DeviceCategoryPicker()
        {
            InitializeComponent();
        }

        [UsedImplicitly]
        [CanBeNull]
        public DeviceCategory PickedDeviceCategory {
            get => Presenter.SelectedItem;
            set {
                Presenter.SelectedItem = value;
                if(value!= null) {
                    CategoryView.SelectItem(value, true);
                }
            }
        }

        [NotNull]
        public DeviceCategoryPickerPresenter Presenter => (DeviceCategoryPickerPresenter) DataContext;

        private void MyTreeViewMouseDoubleClick([CanBeNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            if (sender is TreeView ti) {
                var o = ti.SelectedItem;
                if (o is DeviceCategory dc) {
                    Presenter.SelectedItem = dc;
                }

                e.Handled = false;
            }
        }

        [CanBeNull]
        private static DeviceCategory Search(
            [ItemNotNull] [NotNull] ObservableCollection<DeviceCategory> deviceCategories, [NotNull] string s)
        {
            foreach (var dc in deviceCategories) {
                if (dc.ShortName.ToLower(CultureInfo.CurrentCulture).Contains(s.ToLower(CultureInfo.CurrentCulture))) {
                    return dc;
                }

                var cdc = Search(dc.Children, s);
                if (cdc != null) {
                    return cdc;
                }
            }

            return null;
        }

        private void TextBoxBase_OnTextChanged([CanBeNull] object sender, [CanBeNull] TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text)) {
                return;
            }

            var dc = Search(Presenter.DeviceCategoriesRoot, SearchBox.Text);
            if (dc != null) {
                CategoryView.SelectItem(dc, true);
                SearchBox.Focus();
            }
        }
    }
}