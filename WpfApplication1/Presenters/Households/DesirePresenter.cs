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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

#endregion

namespace LoadProfileGenerator.Presenters.Households
{
    public class DesirePresenter : PresenterBaseDBBase<DesireView>
    {
        [NotNull]
        private readonly Desire _desire;
        [ItemNotNull]
        [NotNull]
        private readonly ObservableCollection<UsedIn> _usedIn;
        [ItemNotNull] [NotNull]
        private readonly ObservableCollection<string> _allDesireCategories = new ObservableCollection<string>();

        public DesirePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] DesireView view, [NotNull] Desire desire)
            : base(view, "ThisDesire.HeaderString", desire, applicationPresenter)
        {
            _desire = desire;
            _usedIn = new ObservableCollection<UsedIn>();
            RefreshUsedIn();
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> AllDesireCategories
        {
            get
            {
                var list = Sim.Desires.Items.Select(x => x.DesireCategory).Distinct().ToList();
                _allDesireCategories.SynchronizeWithList(list);
                return _allDesireCategories;
            }
        }

        [UsedImplicitly]
        [NotNull]
        public string CriticalTime
        {
            get
            {
                if (_desire.CriticalThreshold <= 0)
                {
                    return "forever";
                }
                decimal value = 1;
                var logVal = (decimal)Math.Log(0.5);
                var decayTimesteps = _desire.DefaultDecayRate * 60;
                var exponent = (double)(logVal / decayTimesteps);
                var factor = (decimal)Math.Exp(exponent);
                var time = 0;
                while (value > _desire.CriticalThreshold)
                {
                    time++;
                    value *= factor;
                }
                var ts = new TimeSpan(0, time, 0);
                return ts.ToString();
            }
        }

        [UsedImplicitly]
        public decimal CriticalThreshold
        {
            get => _desire.CriticalThreshold;
            set
            {
                _desire.CriticalThreshold = value;
                OnPropertyChanged(nameof(CriticalThreshold));
                OnPropertyChanged(nameof(CriticalTime));
            }
        }

        [NotNull]
        public Desire ThisDesire => _desire;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<UsedIn> UsedIn => _usedIn;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB)
            {
                _desire.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete()
        {
            Sim.Desires.DeleteItem(_desire);
            Close(false);
        }

        [SuppressMessage("ReSharper", "NotAllowedAnnotation")]
        public override bool Equals(object obj)
        {
            var presenter = obj as DesirePresenter;
            return presenter?.ThisDesire.Equals(_desire) == true;
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

        public void MakeDesireCopy()
        {
            var d = Sim.Desires.CreateNewItem(Sim.ConnectionString);
            d.ImportFromExistingItem(_desire);
            ApplicationPresenter.OpenItem(d);
        }

        public void RefreshUsedIn()
        {
            var usedIn = _desire.CalculateUsedIns(Sim);
            _usedIn.Clear();
            foreach (var p in usedIn)
            {
                _usedIn.Add(p);
            }
        }
    }
}