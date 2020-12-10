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
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

#endregion

namespace Database.Helpers {
    public class CategoryAffordance : CategoryDBBase<Affordance> {
        [ItemNotNull] [NotNull] private readonly ObservableCollection<string> _allAffordanceCategories = new ObservableCollection<string>();

        public CategoryAffordance([NotNull] string name) : base(name) {
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<string> AllAffordanceCategories => _allAffordanceCategories;

        [NotNull]
        public new Affordance CreateNewItem([NotNull] string connectionString) {
            var t = base.CreateNewItem(connectionString);
            return t;
        }

        public void RefreshAllAffordanceCategory() {
            var afflist = new List<string>();

            foreach (var aff in Items) {
                AddUniqueStringToList(AllAffordanceCategories, aff.AffCategory);
                afflist.Add(aff.AffCategory);
            }
            var toremove = new List<string>();
            foreach (var allAffordanceCategory in _allAffordanceCategories) {
                if (!afflist.Contains(allAffordanceCategory)) {
                    toremove.Add(allAffordanceCategory);
                }
            }
            foreach (var s in toremove) {
                _allAffordanceCategories.Remove(s);
            }
        }
    }
}