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
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls.Usercontrols;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

#endregion

namespace LoadProfileGenerator.Presenters.Households {
    public class DeviceCategoryPresenter : PresenterBaseDBBase<DeviceCategoryView> {
        [NotNull] private readonly DeviceCategory _deviceCategory;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<UsedIn> _usedIns;
        [NotNull] private DeviceCategoryPickerPresenter _dcp;
        [CanBeNull] private DeviceCategory _selectedParentCategory;

        public DeviceCategoryPresenter(
            [NotNull] ApplicationPresenter applicationPresenter,
            [NotNull] DeviceCategoryView view, [NotNull] DeviceCategory deviceCategory)
            : base(view, "ThisDeviceCategory.Name", deviceCategory, applicationPresenter)
        {
            _deviceCategory = deviceCategory;
            _dcp = new DeviceCategoryPickerPresenter(Sim, _deviceCategory,
                view.DeviceCategoryPicker1);
            view.DeviceCategoryPicker1.DataContext = _dcp;
            _selectedParentCategory = _deviceCategory.ParentCategory;
            _deviceCategory.RefreshSubDevices();
            _usedIns = new ObservableCollection<UsedIn>();
            RefreshUsedIn();
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceCategory> AllDeviceCategories => Sim.DeviceCategories.MyItems;

        [NotNull]
        [UsedImplicitly]
        public DeviceCategoryPickerPresenter DeviceCategoryPicker {
            get => _dcp;
            set => _dcp = value;
        }

        [CanBeNull]
        [UsedImplicitly]
        public DeviceCategory SelectedParentCategory {
            get => _selectedParentCategory;
            set {
                _selectedParentCategory = value;
                _deviceCategory.ParentCategory = value;
                OnPropertyChanged(nameof(SelectedParentCategory));
            }
        }

        [NotNull]
        public DeviceCategory ThisDeviceCategory => _deviceCategory;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns => _usedIns;

        public void Delete()
        {
            Sim.DeviceCategories.DeleteItem(_deviceCategory);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            return obj is DeviceCategoryPresenter presenter && presenter.ThisDeviceCategory.Equals(_deviceCategory);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + TabHeaderPath.GetHashCode();
                return hash;
            }
        }

        public void RefreshUsedIn()
        {
            var usedIn =
                _deviceCategory.GetHouseholdsForDeviceCategory(
                    Sim.Affordances.MyItems,
                    Sim.Locations.MyItems,
                    Sim.HouseholdTraits.MyItems,
                    Sim.HouseTypes.MyItems);
            _usedIns.Clear();
            foreach (var dui in usedIn) {
                _usedIns.Add(dui);
            }
        }
    }
}