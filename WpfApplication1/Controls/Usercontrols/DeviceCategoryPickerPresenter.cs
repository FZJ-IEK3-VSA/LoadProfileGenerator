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
using Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

#endregion

namespace LoadProfileGenerator.Controls.Usercontrols
{
    public class DeviceCategoryPickerPresenter : Notifier
    {
        [NotNull] private readonly Simulator _sim;
        [NotNull] private readonly DeviceCategoryPicker _deviceCategoryPicker;
        [CanBeNull] private DeviceCategory _selectedItem;

        public DeviceCategoryPickerPresenter([NotNull] Simulator sim, [CanBeNull] DeviceCategory selectedItem,
            [NotNull] DeviceCategoryPicker deviceCategoryPicker)
        {
            _sim = sim;
            _selectedItem = selectedItem;
            _deviceCategoryPicker = deviceCategoryPicker;
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceCategory> DeviceCategoriesRoot => _sim.DeviceCategories.DeviceCategoriesRoot;

        [CanBeNull]
        public DeviceCategory SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                _sim.DeviceCategories.RefreshRootCategories();
            }
        }

        public void Select()
        {
            _deviceCategoryPicker.CategoryView.ExpandAll();
            if(_selectedItem != null) {
                _deviceCategoryPicker.CategoryView.SelectItem(_selectedItem, true);
            }
        }
    }
}
