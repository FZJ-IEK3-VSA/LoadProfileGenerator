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
using System.Linq;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.SpecialViews {
#pragma warning disable S3897 // Classes that provide "Equals(<T>)" or override "Equals(Object)" should implement "IEquatable<T>"
    internal class AffordanceUnusedPresenter : PresenterBaseWithAppPresenter<AffordancesUnusedView> {
#pragma warning restore S3897 // Classes that provide "Equals(<T>)" or override "Equals(Object)" should implement "IEquatable<T>"
        [JetBrains.Annotations.NotNull] private readonly ApplicationPresenter _applicationPresenter;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<Affordance> _selectedAffordances = new ObservableCollection<Affordance>();

        public AffordanceUnusedPresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] AffordancesUnusedView view)
            : base(view, "HeaderString", applicationPresenter)
        {
            _applicationPresenter = applicationPresenter;
            Refresh();
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string HeaderString => "Unused Affordances";

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [UsedImplicitly]
        public ObservableCollection<Affordance> SelectedAffordances => _selectedAffordances;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            _applicationPresenter.CloseTab(this, removeLast);
        }

        public override bool Equals(object obj)
        {
            return obj is AffordanceUnusedPresenter presenter && presenter.HeaderString.Equals(HeaderString);
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
            var affordances = Sim.Affordances.Items;
            foreach (var affordance in affordances) {
                var found = false;
                foreach (var householdTrait in Sim.HouseholdTraits.Items) {
                    if (!found) {
                        foreach (var hhLocation in householdTrait.Locations) {
                            if (hhLocation.AffordanceLocations.Any(x => x.Affordance == affordance)) {
                                found = true;
                            }
                        }
                    }
                }
                if (!found) {
                    _selectedAffordances.Add(affordance);
                }
            }
        }
    }
}