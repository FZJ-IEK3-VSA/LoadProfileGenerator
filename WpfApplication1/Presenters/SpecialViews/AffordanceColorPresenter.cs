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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Automation.ResultFiles;
using Common;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

#endregion

namespace LoadProfileGenerator.Presenters.SpecialViews {
#pragma warning disable S3897 // Classes that provide "Equals(<T>)" or override "Equals(Object)" should implement "IEquatable<T>"
    public class AffordanceColorPresenter : PresenterBaseWithAppPresenter<AffordanceColorView> {
        [NotNull] [ItemNotNull]
        private readonly ObservableCollection<Affordance> _affordances = new ObservableCollection<Affordance>();

        [CanBeNull] private string _sortBy;

        public AffordanceColorPresenter([NotNull] ApplicationPresenter applicationPresenter,
            [NotNull] AffordanceColorView view)
            : base(view, "HeaderString", applicationPresenter)
        {
            SortByOptions.Add("By Name");
            SortByOptions.Add("By Category");
            SortBy = "By Name";
            var myAffordances = new List<Affordance>(Sim.Affordances.MyItems);
            myAffordances.Sort(Comparison);
            _affordances.Clear();
            foreach (var myAffordance in myAffordances) {
                _affordances.Add(myAffordance);
            }
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Affordance> Affordances => _affordances;

        [UsedImplicitly]
        [NotNull]
        public string HeaderString => "Affordance Color Setter";

        [UsedImplicitly]
        [NotNull]
        public string SortBy {
            get {
                if (_sortBy != null) {
                    return _sortBy;
                }

                throw new InvalidOperationException();
            }

            set {
                _sortBy = value;
                Resort();
                OnPropertyChanged(nameof(SortBy));
            }
        }

        [UsedImplicitly]
        [NotNull]
        [ItemNotNull]
        public List<string> SortByOptions { get; } = new List<string>();

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            var presenter = obj as AffordanceColorPresenter;
            return presenter?.HeaderString.Equals(HeaderString) == true;
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

        public void RefreshSort()
        {
            Resort();
        }

        private int Comparison([NotNull] Affordance affordance, [NotNull] Affordance affordance1)
        {
            if (affordance == null) {
                throw new LPGException("affordance was null");
            }

            switch (SortBy) {
                case "By Name":
                    return string.Compare(affordance.Name, affordance1.Name, StringComparison.Ordinal);
                case "By Category":
                    var result = string.Compare(affordance.AffCategory, affordance1.AffCategory,
                        StringComparison.Ordinal);
                    if (result == 0) {
                        result = affordance.CompareColor(affordance1);
                    }

                    return result;
                default:
                    throw new LPGException("Unknown Sort By");
            }
        }

        private void Resort()
        {
            _affordances.Sort(Comparison);
        }
#pragma warning restore S3897 // Classes that provide "Equals(<T>)" or override "Equals(Object)" should implement "IEquatable<T>"
    }
}