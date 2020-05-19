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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.SpecialViews {
    internal class TimeLimitUnusedPresenter : PresenterBaseWithAppPresenter<TimeLimitUnusedView> {
        [ItemNotNull] [NotNull] private readonly ObservableCollection<TimeLimit> _selectedTimeLimits = new ObservableCollection<TimeLimit>();

        public TimeLimitUnusedPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] TimeLimitUnusedView view)
            : base(view, "HeaderString", applicationPresenter)
        {
            Refresh();
        }

        [NotNull]
        [UsedImplicitly]
        public string HeaderString => "Unused Time Limits";

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [UsedImplicitly]
        public ObservableCollection<TimeLimit> SelectedTimeLimits => _selectedTimeLimits;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public override bool Equals(object obj)
        {
            return obj is TimeLimitUnusedPresenter presenter && presenter.HeaderString.Equals(HeaderString);
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
            _selectedTimeLimits.Clear();
            var usedTimeLimits = new Dictionary<TimeLimit, bool>();

            foreach (var affordance in Sim.Affordances.MyItems) {
                if (affordance.TimeLimit != null && !usedTimeLimits.ContainsKey(affordance.TimeLimit)) {
                    usedTimeLimits.Add(affordance.TimeLimit, true);
                }
            }
            foreach (var household in Sim.HouseholdTraits.MyItems) {
                foreach (var hhAutonomousDevice in household.Autodevs) {
                    if (hhAutonomousDevice.TimeLimit != null &&
                        !usedTimeLimits.ContainsKey(hhAutonomousDevice.TimeLimit)) {
                        usedTimeLimits.Add(hhAutonomousDevice.TimeLimit, true);
                    }
                }
            }
            foreach (var timeLimit in Sim.TimeLimits.MyItems) {
                if (!usedTimeLimits.ContainsKey(timeLimit)) {
                    _selectedTimeLimits.Add(timeLimit);
                }
            }
        }
    }
}