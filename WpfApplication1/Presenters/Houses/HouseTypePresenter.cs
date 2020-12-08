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

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Common;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Houses;

namespace LoadProfileGenerator.Presenters.Houses {
    public class HouseTypePresenter : PresenterBaseDBBase<HouseTypeView> {
        [JetBrains.Annotations.NotNull] private readonly HouseType _houseType;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<UsedIn> _usedIns;

        [CanBeNull] private string _selectedAddCategory;

        [CanBeNull] private TemperatureProfile _testingTemperatureProfile;

        public HouseTypePresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] HouseTypeView view, [JetBrains.Annotations.NotNull] HouseType houseType)
            : base(view, "ThisHouseType.HeaderString", houseType, applicationPresenter)
        {
            _houseType = houseType;
            if (TemperaturProfiles.Count > 0) {
                _testingTemperatureProfile = TemperaturProfiles[0];
            }
            ThisHouseType.PropertyChanged += ThisHouseTypePropertyChanged;
            _usedIns = new ObservableCollection<UsedIn>();
            RefreshUsedIn();
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<RealDevice> AllDevices => Sim.RealDevices.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> CategoryOrDevice => Sim.CategoryOrDevice;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceCategory> DeviceCategories
            => Sim.DeviceCategories.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<RealDevice> Devices => Sim.RealDevices.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<EnergyStorage> EnergyStorages
            => Sim.EnergyStorages.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Generator> Generators => Sim.Generators.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<GeographicLocation> GeographicLocations
            => Sim.GeographicLocations.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Location> Locations => Sim.Locations.Items;

        [UsedImplicitly]
        public double ResultingCoolingEnergyUse => GetDegreeHoursCoolingTotal();

        [UsedImplicitly]
        public double ResultingDegreeDays => GetDegreeDays();

        [UsedImplicitly]
        public double ResultingDegreeHours => GetDegreeHours();

        [UsedImplicitly]
        public double ResultingEnergyUse => GetDegreeDaysHeatingTotal();

        [CanBeNull]
        [UsedImplicitly]
        public string SelectedAddCategory {
            get => _selectedAddCategory;
            set {
                _selectedAddCategory = value;
                OnPropertyChanged(nameof(SelectedAddCategory));
                OnPropertyChanged(nameof(ShowCategoryDropDown));
                OnPropertyChanged(nameof(ShowDeviceDropDown));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public RealDevice SelectedDevice { get; set; }

        [UsedImplicitly]
        public Visibility ShowCategoryDropDown {
            get {
                if (_selectedAddCategory == "Device") {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceDropDown {
            get {
                if (_selectedAddCategory != "Device") {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<TemperatureProfile> TemperatureProfiles
            => Sim.TemperatureProfiles.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<TemperatureProfile> TemperaturProfiles
            => Sim.TemperatureProfiles.Items;

        [CanBeNull]
        [UsedImplicitly]
        public TemperatureProfile TestingTemperatureProfile {
            get => _testingTemperatureProfile;
            set {
                _testingTemperatureProfile = value;
                OnPropertyChanged(nameof(TestingTemperatureProfile));
                Refresh();
            }
        }

        [JetBrains.Annotations.NotNull]
        public HouseType ThisHouseType => _houseType;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeLimit> TimeLimits => Sim.TimeLimits.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeBasedProfile> Timeprofiles
            => Sim.Timeprofiles.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<TransformationDevice> TransformationDevices
            => Sim.TransformationDevices.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns => _usedIns;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> VLoadTypes => Sim.LoadTypes.Items;

        public void Delete()
        {
            Sim.HouseTypes.DeleteItem(_houseType);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as HouseTypePresenter;
            return presenter?.ThisHouseType.Equals(_houseType) == true;
        }

        private double GetDegreeDays()
        {
            if (TestingTemperatureProfile == null) {
                return 0;
            }
            var now = DateTime.Now;
            var start = new DateTime(now.Year, 1, 1);
            var end = new DateTime(now.Year, 12, 31);
            var days = MakeDegreeDaysClass.MakeDegreeDays(TestingTemperatureProfile, start, end,
                ThisHouseType.HeatingTemperature, ThisHouseType.RoomTemperature, ThisHouseType.HeatingYearlyTotal,
                false,
                ThisHouseType.ReferenceDegreeDays);
            double sum = 0;
            foreach (var degreeDay in days) {
                sum += degreeDay.GetDegreeDay(ThisHouseType.HeatingTemperature, ThisHouseType.RoomTemperature);
            }
            return sum;
        }

        private double GetDegreeDaysHeatingTotal()
        {
            if (TestingTemperatureProfile == null)
            {
                return 0;
            }
            var now = DateTime.Now;
            var start = new DateTime(now.Year, 1, 1);
            var end = new DateTime(now.Year, 12, 31);
            var days = MakeDegreeDaysClass.MakeDegreeDays(TestingTemperatureProfile, start, end,
                ThisHouseType.HeatingTemperature, ThisHouseType.RoomTemperature, ThisHouseType.HeatingYearlyTotal,
                ThisHouseType.AdjustYearlyEnergy, ThisHouseType.ReferenceDegreeDays);
            double sum = 0;
            foreach (var degreeDay in days) {
                sum += degreeDay.HeatingAmount;
            }
            return sum;
        }

        private double GetDegreeHours()
        {
            if (TestingTemperatureProfile == null)
            {
                return 0;
            }
            var now = DateTime.Now;
            var start = new DateTime(now.Year, 1, 1);
            var end = new DateTime(now.Year, 12, 31);
            var hours = DbCalcDegreeHour.GetCalcDegreeHours(TestingTemperatureProfile, start, end,
                ThisHouseType.CoolingTemperature, ThisHouseType.CoolingYearlyTotal, false,
                ThisHouseType.ReferenceCoolingHours);
            double sum = 0;
            foreach (var degreeHour in hours) {
                sum += degreeHour.GetDegreeDay(ThisHouseType.CoolingTemperature);
            }
            return sum;
        }

        private double GetDegreeHoursCoolingTotal()
        {
            if (TestingTemperatureProfile == null)
            {
                return 0;
            }
            var now = DateTime.Now;
            var start = new DateTime(now.Year, 1, 1);
            var end = new DateTime(now.Year, 12, 31);
            var hours = DbCalcDegreeHour.GetCalcDegreeHours(TestingTemperatureProfile, start, end,
                ThisHouseType.CoolingTemperature, ThisHouseType.CoolingYearlyTotal, ThisHouseType.AdjustYearlyCooling,
                ThisHouseType.ReferenceCoolingHours);
            double sum = 0;
            foreach (var degreeHour in hours) {
                sum += degreeHour.CoolingAmount;
            }
            return sum;
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

        public void Refresh()
        {
            OnPropertyChanged(nameof(ResultingDegreeDays));
            OnPropertyChanged(nameof(ResultingEnergyUse));
            OnPropertyChanged(nameof(ResultingDegreeHours));
            OnPropertyChanged(nameof(ResultingCoolingEnergyUse));
        }

        public void RefreshUsedIn()
        {
            var usedIn = ThisHouseType.CalculateUsedIns(Sim);
            _usedIns.SynchronizeWithList(usedIn);
        }

        private void ThisHouseTypePropertyChanged([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] PropertyChangedEventArgs e)
        {
            // heating
            if (e.PropertyName == "HeatingYearlyTotal" || e.PropertyName == "ReferenceDegreeDays" ||
                e.PropertyName == "AdjustYearlyEnergy" || e.PropertyName == "HeatingTemperature" ||
                e.PropertyName == "RoomTemperature") {
                Refresh();
            }
            if (e.PropertyName == "CoolingYearlyTotal" || e.PropertyName == "CoolingTemperature" ||
                e.PropertyName == "AdjustYearlyCooling" || e.PropertyName == "ReferenceCoolingHours") {
                Refresh();
            }
        }

        public void MakeACopy()
        {
            var newht = (HouseType) HouseType.ImportFromItem(ThisHouseType, Sim);
            newht.Name = newht.Name + " (Copy)";
            newht.SaveToDB();
            Sim.HouseTypes.Items.Add(newht);
            ApplicationPresenter.OpenItem(newht);

        }
    }
}