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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Common;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households
{
    public class LocationPresenter : PresenterBaseDBBase<LocationView>
    {
        [ItemNotNull]
        [NotNull]
        private readonly ObservableCollection<UsedIn> _households;
        [NotNull]
        private readonly Location _location;
        [NotNull]
        private string _selectedAddCategory;

        public LocationPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] LocationView view, [NotNull] Location location)
            : base(view, "ThisLocation.HeaderString", location, applicationPresenter)
        {
            _location = location;
            _households = new ObservableCollection<UsedIn>();
            _selectedAddCategory = CategoryOrDevice[0];
            RefreshUses();
        }

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<string> CategoryOrDevice => Sim.CategoryOrDevice;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DeviceCategory> DeviceCategories
            => Sim.DeviceCategories.MyItems;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<RealDevice> Devices => Sim.RealDevices.MyItems;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<UsedIn> Households => _households;

        [UsedImplicitly]
        [NotNull]
        public string SelectedAddCategory
        {
            get => _selectedAddCategory;
            set
            {
                _selectedAddCategory = value;
                OnPropertyChanged(nameof(SelectedAddCategory));
                OnPropertyChanged(nameof(ShowCategoryDropDown));
                OnPropertyChanged(nameof(ShowDeviceDropDown));
            }
        }

        [UsedImplicitly]
        public Visibility ShowCategoryDropDown
        {
            get
            {
                if (_selectedAddCategory == "Device")
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceDropDown
        {
            get
            {
                if (_selectedAddCategory == "Device")
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        [NotNull]
        public Location ThisLocation => _location;

        public void AddDevice([NotNull] IAssignableDevice adev)
        {
            _location.AddDevice(adev);
        }

        public void Delete()
        {
            Sim.Locations.DeleteItem(_location);
            Close(false);
        }

        [SuppressMessage("ReSharper", "NotAllowedAnnotation")]
        public override bool Equals(object obj)
        {
            var presenter = obj as LocationPresenter;
            return presenter?.ThisLocation.Equals(_location) == true;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                const int hash = 17;
                // Suitable nullity checks etc, of course :)
                return hash * 23 + TabHeaderPath.GetHashCode();
            }
        }

        public void RefreshUses()
        {
            var usedInHH = _location.CalculateUsedIns(Sim);
            _households.SynchronizeWithList(usedInHH);
        }

        public void RemoveDevice([NotNull] LocationDevice ld)
        {
            _location.DeleteDevice(ld);
        }
    }
}