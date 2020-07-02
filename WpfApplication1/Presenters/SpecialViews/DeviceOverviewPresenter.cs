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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Media;
using Automation.ResultFiles;
using Common;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.SpecialViews {
    internal class DeviceOverviewPresenter : PresenterBaseWithAppPresenter<DeviceOverviewView> {
        [NotNull] [ItemNotNull] private readonly ObservableCollection<DeviceWithImg> _devices = new ObservableCollection<DeviceWithImg>();

        [CanBeNull] private string _sortBy;

        public DeviceOverviewPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] DeviceOverviewView view)
            : base(view, "HeaderString", applicationPresenter)
        {
            SortByOptions.Add("By Name");
            SortByOptions.Add("By Category");
            SortBy = "By Name";
            Resort();
        }

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [UsedImplicitly]
        public ObservableCollection<DeviceWithImg> Devices => _devices;

        [NotNull]
        [UsedImplicitly]
        public string HeaderString => "Device Overview";

        [CanBeNull]
        [UsedImplicitly]
        public string SortBy {
            get => _sortBy;
            set {
                _sortBy = value;
                Resort();
                OnPropertyChanged(nameof(SortBy));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public List<string> SortByOptions { get; } = new List<string>();

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        private int Comparison([NotNull] RealDevice dev1, [NotNull] RealDevice dev2)
        {
            switch (SortBy) {
                case "By Name":
                    return string.Compare(dev1.Name, dev2.Name, StringComparison.Ordinal);
                case "By Category":
                    var result = string.Compare(dev1.DeviceCategory?.FullPath, dev2.DeviceCategory?.FullPath,
                        StringComparison.Ordinal);
                    return result;
                default:
                    throw new LPGException("Unknown Sort By");
            }
        }

        public override bool Equals(object obj)
        {
            return obj is DeviceOverviewPresenter presenter && presenter.HeaderString.Equals(HeaderString);
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

        private void Resort()
        {
            var mydevices = new List<RealDevice>(Sim.RealDevices.Items);
            mydevices.Sort(Comparison);
            _devices.Clear();
            foreach (var rd in mydevices) {
                _devices.Add(new DeviceWithImg(rd, ApplicationPresenter));
            }
        }

        public class DeviceWithImg {
            [NotNull] private readonly ApplicationPresenter _applicationPresenter;

            public DeviceWithImg([NotNull] RealDevice rd, [NotNull] ApplicationPresenter ap)
            {
                Device = rd;
                _applicationPresenter = ap;
            }

            [NotNull]
            public RealDevice Device { get; }

            [CanBeNull]
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [UsedImplicitly]
            public string FullFileName {
                get {
                    var sim = _applicationPresenter.Simulator;
                    if (sim == null) {
                        return null;
                    }
                    string path;
                    if (sim.MyGeneralConfig.ImagePath.Contains(":")) {
                        path = sim.MyGeneralConfig.ImagePath;
                    }
                    else {
                        if(Config.StartPath == null) {
                            throw new LPGException("Startpath was null");
                        }

                        path = Path.Combine(Config.StartPath,
                            sim.MyGeneralConfig.ImagePath);
                    }

                    return Path.Combine(path, Device.Picture);
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            [UsedImplicitly]
            [CanBeNull]
            public ImageSource ImageUri {
                get {
                    try {
                        if (Device.Picture.Length > 0) {
                            if (File.Exists(FullFileName)) {
                                return new ImageSourceConverter().ConvertFromString(FullFileName) as ImageSource;
                            }
                            Logger.Error("Image does not exist:" + FullFileName);
                        }
                    }
                    catch (Exception e) {
                        Logger.Error("Image could not be loaded:" + FullFileName + ": " + e.Message);
                        Logger.Exception(e);
                    }

                    return null;
                }
            }
        }
    }
}