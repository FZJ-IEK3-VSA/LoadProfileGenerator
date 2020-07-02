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
using Database.Helpers;
using Database.Tables;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.SpecialViews {
    internal class HouseholdsWithRealDevicesPresenter : PresenterBaseWithAppPresenter<HouseholdsWithRealDevicesView> {
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHEntry> _selectedAffordances = new ObservableCollection<HHEntry>();

        public HouseholdsWithRealDevicesPresenter([NotNull] ApplicationPresenter applicationPresenter,
            [NotNull] HouseholdsWithRealDevicesView view) : base(view, "HeaderString", applicationPresenter)
        {
            Refresh();
        }

        [NotNull]
        [UsedImplicitly]
        public string HeaderString => "Household Device Validation View";

        [NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public ObservableCollection<HHEntry> SelectedHouseholds => _selectedAffordances;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public override bool Equals(object obj)
        {
            return obj is HouseholdsWithRealDevicesPresenter presenter && presenter.HeaderString.Equals(HeaderString);
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
            _selectedAffordances.Clear();
            var houses = Sim.HouseTypes.Items;
            foreach (var house in houses) {
                foreach (var hhAutonomousDevice in house.HouseDevices) {
                    if (hhAutonomousDevice.Device?.AssignableDeviceType == AssignableDeviceType.Device) {
                        var add = true;

                        var rd = hhAutonomousDevice.Device as RealDevice;
                        if (rd?.DeviceCategory != null) {
                            if (rd.DeviceCategory.IgnoreInRealDeviceViews) {
                                add = false;
                            }
                        }
                        if (add) {
                            SelectedHouseholds.Add(new HHEntry(house, null, hhAutonomousDevice.Device, "House Type"));
                        }
                    }
                }
            }
        }

        public class HHEntry {
            public HHEntry([NotNull] DBBase hh, [CanBeNull] Location loc, [NotNull] IAssignableDevice device, [NotNull] string type)
            {
                Household = hh;
                Location = loc;
                Device = device;
                Type = type;
            }

            [NotNull]
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [UsedImplicitly]
            public IAssignableDevice Device { get; set; }

            [NotNull]
            [UsedImplicitly]
            public DBBase Household { get; set; }

            [CanBeNull]
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [UsedImplicitly]
            public Location Location { get; set; }

            [NotNull]
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [UsedImplicitly]
            public string Type { get; set; }
        }
    }
}