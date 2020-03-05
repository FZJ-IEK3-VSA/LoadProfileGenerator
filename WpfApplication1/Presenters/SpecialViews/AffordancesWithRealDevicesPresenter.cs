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
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

#endregion

namespace LoadProfileGenerator.Presenters.SpecialViews {
    internal class AffordancesWithRealDevicesPresenter : PresenterBaseWithAppPresenter<AffordancesWithRealDevicesView> {
        [ItemNotNull] [NotNull] private readonly ObservableCollection<AffEntry> _selectedAffordances = new ObservableCollection<AffEntry>();

        public AffordancesWithRealDevicesPresenter([NotNull] ApplicationPresenter applicationPresenter,
            [NotNull] AffordancesWithRealDevicesView view) : base(view, "HeaderString", applicationPresenter)
        {
            Refresh();
        }

        [NotNull]
        [UsedImplicitly]
        public string HeaderString => "Affordance with Real Devices View";

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffEntry> SelectedAffordances => _selectedAffordances;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            var presenter = obj as AffordancesWithRealDevicesPresenter;
            return presenter?.HeaderString.Equals(HeaderString) == true;
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
            var affordances = Sim.Affordances.MyItems;
            foreach (var affordance in affordances) {
                foreach (var device in affordance.AffordanceDevices) {
                    if (device.Device?.AssignableDeviceType == AssignableDeviceType.Device) {
                        var add = true;

                        // double check if this is a category to ignore like f.ex. furniture
                        var rd = device.Device as RealDevice;
                        if (rd?.DeviceCategory != null) {
                            if (rd.DeviceCategory.IgnoreInRealDeviceViews) {
                                add = false;
                            }
                        }
                        if (add) {
                            SelectedAffordances.Add(new AffEntry(affordance, device.Device));
                        }
                    }
                }
            }
        }

        public class AffEntry {
            public AffEntry([NotNull] Affordance affordance, [NotNull] IAssignableDevice devices)
            {
                Affordance = affordance;
                Device = devices;
            }

            [NotNull]
            [UsedImplicitly]
            public Affordance Affordance { [UsedImplicitly] get; }

            [NotNull]
            [UsedImplicitly]
            public IAssignableDevice Device { get; }
        }
    }
}