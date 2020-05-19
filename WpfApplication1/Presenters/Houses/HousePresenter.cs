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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Common.Enums;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Houses;

#endregion

namespace LoadProfileGenerator.Presenters.Houses {
    public class HousePresenter : PresenterBaseDBBase<HouseView> {
        [NotNull] private readonly EnergyIntensityConverter _eic = new EnergyIntensityConverter();
        [NotNull] private readonly House _house;

        [CanBeNull] private string _selectedAddCategory;

        private CalcObjectType _selectedCalcObjectType;
        [CanBeNull] private ChargingStationSet _selectedChargingStationSet;
        [CanBeNull] private TravelRouteSet _selectedTravelRouteSet;
        [CanBeNull] private TransportationDeviceSet _selectedTransportationDeviceSet;
        private bool _enableTransportationModelling;

        public HousePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] HouseView view, [NotNull] House house) : base(view,
            "ThisHouse.HeaderString", house, applicationPresenter)
        {
            _house = house;
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<RealDevice> AllDevices => Sim.RealDevices.MyItems;

        [NotNull]
        [UsedImplicitly]
        public Dictionary<CalcObjectType, string> CalcObjectTypes => CalcObjectTypeHelper
            .CalcObjectTypeHouseholdDictionary;
        [NotNull]
        [UsedImplicitly]
        public Dictionary<CreationType, string> CreationTypes => CreationTypeHelper.CreationTypeDictionary;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceCategory> DeviceCategories => Sim.DeviceCategories
            .MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ChargingStationSet> ChargingStationSets => Sim.ChargingStationSets.MyItems;

        [CanBeNull]
        [UsedImplicitly]
        public ChargingStationSet SelectedChargingStationSet {
            get => _selectedChargingStationSet;
            set {
                if (Equals(value, _selectedChargingStationSet)) {
                    return;
                }

                _selectedChargingStationSet = value;
                OnPropertyChanged(nameof(SelectedChargingStationSet));
            }
        }


        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TravelRouteSet> TravelRouteSets => Sim.TravelRouteSets.MyItems;

        [CanBeNull]
        [UsedImplicitly]
        public TravelRouteSet SelectedTravelRouteSet
        {
            get => _selectedTravelRouteSet;
            set
            {
                if (Equals(value, _selectedTravelRouteSet))
                {
                    return;
                }

                _selectedTravelRouteSet = value;
                OnPropertyChanged(nameof(SelectedTravelRouteSet));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TransportationDeviceSet> TransportationDeviceSets => Sim.TransportationDeviceSets.MyItems;

        [CanBeNull]
        [UsedImplicitly]
        public TransportationDeviceSet SelectedTransportationDeviceSet
        {
            get => _selectedTransportationDeviceSet;
            set
            {
                if (Equals(value, _selectedTransportationDeviceSet))
                {
                    return;
                }

                _selectedTransportationDeviceSet = value;
                OnPropertyChanged(nameof(SelectedTransportationDeviceSet));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<EnergyIntensityConverter.EnergyIntensityForDisplay> EnergyIntensities => _eic.All;

        [NotNull]
        [UsedImplicitly]
        public EnergyIntensityConverter.EnergyIntensityForDisplay EnergyIntensity {
            get => _eic.GetAllDisplayElement(_house.EnergyIntensityType);
            set {
                _house.EnergyIntensityType = value.EnergyIntensityType;
                OnPropertyChanged(nameof(EnergyIntensity));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<GeographicLocation> GeographicLocations => Sim
            .GeographicLocations.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseType> HouseTypes => Sim.HouseTypes.MyItems;

        [UsedImplicitly]
        public Visibility IsModularHouseholdVisible {
            get {
                if (_selectedCalcObjectType == CalcObjectType.ModularHousehold) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHousehold> ModularHouseholds => Sim
            .ModularHouseholds.MyItems;

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

        [UsedImplicitly]
        public CalcObjectType SelectedCalcObjectType {
            get => _selectedCalcObjectType;
            set {
                _selectedCalcObjectType = value;
                OnPropertyChanged(nameof(SelectedCalcObjectType));
                OnPropertyChanged(nameof(IsModularHouseholdVisible));
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
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TemperatureProfile> TemperatureProfiles => Sim
            .TemperatureProfiles.MyItems;

        [NotNull]
        [UsedImplicitly]
        public House ThisHouse => _house;

        public bool EnableTransportationModelling {
            get => _enableTransportationModelling;
            set {
                _enableTransportationModelling = value;
                OnPropertyChanged(nameof(EnableTransportationModelling));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeLimit> TimeLimits => Sim.TimeLimits.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeBasedProfile> Timeprofiles => Sim.Timeprofiles
            .MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> VLoadTypes => Sim.LoadTypes.MyItems;

        public void AddCalcObject([NotNull] ICalcObject calcObject, [CanBeNull] ChargingStationSet
                                      chargingStationSet, [CanBeNull] TransportationDeviceSet transportationDeviceSet,
                                  [CanBeNull] TravelRouteSet travelRouteSet, bool useTransportation)
        {
            _house.AddHousehold(calcObject, useTransportation,chargingStationSet, travelRouteSet,transportationDeviceSet );
        }

        public void Delete()
        {
            Sim.Houses.DeleteItem(_house);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as HousePresenter;
            return presenter?.ThisHouse.Equals(_house) == true;
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

        public void RemoveHousehold([NotNull] HouseHousehold hhh)
        {
            _house.DeleteHouseholdFromDB(hhh);
        }
    }
}