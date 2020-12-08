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
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public class SubAffordancePresenter : PresenterBaseDBBase<SubAffordanceView> {
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<string> _allActions = new ObservableCollection<string>();
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<string> _executionTimes = new ObservableCollection<string>();

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<string> _locationModes = new ObservableCollection<string>();
        [JetBrains.Annotations.NotNull] private readonly SubAffordance _subaff;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<UsedIn> _usedIns;

        public SubAffordancePresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] SubAffordanceView view,
            [JetBrains.Annotations.NotNull] SubAffordance subaff) : base(view, "ThisSubAffordance.HeaderString", subaff, applicationPresenter)
        {
            _subaff = subaff;
            _usedIns = new ObservableCollection<UsedIn>();
            _allActions.SynchronizeWithList(VariableActionHelper.CollectAllStrings());
            _locationModes.SynchronizeWithList(VariableLocationModeHelper.CollectAllStrings());
            _executionTimes.SynchronizeWithList(VariableExecutionTimeHelper.CollectAllStrings());
            RefreshUsedIn();
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> AllActions => _allActions;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Location> AllLocations => Sim.Locations.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Variable> AllVariables => Sim.Variables.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Desire> Desires => Sim.Desires.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> ExecutionTimes => _executionTimes;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> LocationModes => _locationModes;

        [JetBrains.Annotations.NotNull]
        public SubAffordance ThisSubAffordance => _subaff;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeBasedProfile> Timeprofiles
            => Sim.Timeprofiles.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns => _usedIns;

        public void AddDesire([JetBrains.Annotations.NotNull] Desire d, decimal satisfactionvalue)
        {
            _subaff.AddDesire(d, satisfactionvalue, Sim.Desires.Items);
        }

        public void Delete()
        {
            Sim.SubAffordances.DeleteItem(_subaff);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            return obj is SubAffordancePresenter presenter && presenter.ThisSubAffordance.Equals(_subaff);
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

        public void RefreshUsedIn()
        {
            var usedIn = _subaff.CalculateUsedIns(Sim);
            _usedIns.Clear();
            foreach (var dui in usedIn) {
                _usedIns.Add(dui);
            }
        }

        public void RemoveDesire([JetBrains.Annotations.NotNull] SubAffordanceDesire ad)
        {
            _subaff.DeleteAffordanceDesireFromDB(ad);
            _subaff.SaveToDB();
        }
    }
}