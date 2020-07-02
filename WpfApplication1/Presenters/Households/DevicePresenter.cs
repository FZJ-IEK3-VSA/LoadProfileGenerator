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

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Media;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls.Usercontrols;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

#endregion

namespace LoadProfileGenerator.Presenters.Households {
    public class DevicePresenter : PresenterBaseDBBase<DeviceView> {
        [NotNull] private readonly RealDevice _realDevice;

        [NotNull] private readonly ObservableCollection<TimeProfileType> _timeProfileTypes =
            new ObservableCollection<TimeProfileType>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<UsedIn> _usedIns = new ObservableCollection<UsedIn>();

        [CanBeNull] private RealDevice _selectedImportDevice;

        public DevicePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] DeviceView view, [NotNull] RealDevice realDevice)
            : base(view, "ThisDevice.HeaderString", realDevice, applicationPresenter)
        {
            _realDevice = realDevice;
            DeviceCategoryPickerPresenter = new DeviceCategoryPickerPresenter(Sim,
                _realDevice.DeviceCategory, view.DeviceCategoryPicker1);
            view.DeviceCategoryPicker1.DataContext = DeviceCategoryPickerPresenter;
            DeviceCategoryPickerPresenter.Select();
            DeviceCategoryPickerPresenter.PropertyChanged += DcpOnPropertyChanged;
            _timeProfileTypes.Add(TimeProfileType.Relative);
            _timeProfileTypes.Add(TimeProfileType.Absolute);
            var u = ThisDevice.CalculateUsedIns(Sim);
            _usedIns.SynchronizeWithList(u);
        }

        [NotNull]
        [UsedImplicitly]
        public DeviceCategoryPickerPresenter DeviceCategoryPickerPresenter { get; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<RealDevice> Devices => Sim.RealDevices.Items;

        [NotNull]
        [UsedImplicitly]
        public string FullFileName {
            get {
                string path;
                if (Sim.MyGeneralConfig.ImagePath.Contains(":")) {
                    path = Sim.MyGeneralConfig.ImagePath;
                }
                else {
                    if(Config.StartPath == null) {
                        throw new LPGException("Config was null");
                    }

                    path = Path.Combine(Config.StartPath, Sim.MyGeneralConfig.ImagePath);
                }
                return Path.Combine(path, ThisDevice.Picture);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [UsedImplicitly]
        [CanBeNull]
        public ImageSource ImageUri {
            get {
                try {
                    if (_realDevice.Picture.Length > 0) {
                        if (File.Exists(FullFileName)) {
                            var i = new ImageSourceConverter().ConvertFromString(FullFileName) as ImageSource;
                            return i;
                        }
                        Logger.Error("Image does not exist:" + FullFileName);
                    }
                }
                catch (Exception e) {
                    Logger.Error("Image could not be loaded:" + FullFileName + ": " + e.Message);
                }

                return null;
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> LoadTypes => Sim.LoadTypes.Items;

        [CanBeNull]
        [UsedImplicitly]
        public RealDevice SelectedImportDevice {
            get => _selectedImportDevice;
            set {
                _selectedImportDevice = value;
                OnPropertyChanged(nameof(SelectedImportDevice));
            }
        }

        [NotNull]
        public Simulator Simulator => Sim;

        [NotNull]
        public RealDevice ThisDevice => _realDevice;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeBasedProfile> TimeProfiles
            => Sim.Timeprofiles.Items;

        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeProfileType> TimeProfileTypes => _timeProfileTypes;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns => _usedIns;

        public void AddRealDeviceLoadType([NotNull] VLoadType loadType, double maxpower, double standardDeviation,
            double averageYearlyConsumption)
        {
            _realDevice.AddLoad(loadType, maxpower, standardDeviation, averageYearlyConsumption);
        }

        public void CreateCopy()
        {
            var rd = Sim.RealDevices.CreateNewItem(Sim.ConnectionString);
            rd.ImportFromOtherDevice(ThisDevice);
            rd.Name = ThisDevice.Name + " (Copy)";
            rd.SaveToDB();
            ApplicationPresenter.OpenItem(rd);
        }

        private void DcpOnPropertyChanged([NotNull] object sender, [NotNull] PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "SelectedItem") {
                _realDevice.DeviceCategory = DeviceCategoryPickerPresenter.SelectedItem;
            }
        }

        public void Delete()
        {
            Sim.RealDevices.DeleteItem(_realDevice);
            Close(false);
        }

        public void DeleteLoad([NotNull] RealDeviceLoadType rdlt)
        {
            _realDevice.DeleteLoad(rdlt);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as DevicePresenter;
            return presenter?.ThisDevice.Equals(_realDevice) == true;
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

        public void ImportDevice()
        {
            ThisDevice.ImportFromOtherDevice(SelectedImportDevice);
        }

        public void RefreshUsedIns()
        {
            var u = ThisDevice.CalculateUsedIns(Sim);
            _usedIns.SynchronizeWithList(u);
        }

        public void AddInnerHeatLoad()
        {
            RealDeviceLoadType electricityLoad = null;
            RealDeviceLoadType innerLoad = null;
            RealDeviceLoadType water = null;
            foreach (var load in ThisDevice.Loads) {
                if (load.LoadType?.Name == "Electricity") {
                    electricityLoad = load;
                }

                if (load.LoadType?.Name?.Contains("Inner") == true) {
                    innerLoad = load;
                }
                if (load.LoadType?.Name?.ToLower().Contains("water") == true)
                {
                    water = load;
                }
            }

            if (water != null) {
                throw  new DataIntegrityException("Devices with water load can not be fixed automatically.");
            }
            if (electricityLoad != null && innerLoad == null) {
                var innerloadtype = Simulator.LoadTypes.FindFirstByName("Inner", FindMode.StartsWith);
                if (innerloadtype == null) {
                    throw new LPGException("Could not find load type for inner heat gains");
                }
                ThisDevice.AddLoad(innerloadtype,electricityLoad.MaxPower,electricityLoad.StandardDeviation,
                    0);
            }
        }
    }
}