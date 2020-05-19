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
using Automation;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Views.BasicElements;

#endregion

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class LoadTypePresenter : PresenterBaseDBBase<LoadTypeView> {
        [NotNull] private readonly ApplicationPresenter _applicationPresenter;
        private double _amountForTesting;

        public LoadTypePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] LoadTypeView view, [NotNull] VLoadType loadtype)
            : base(view, "ThisLoadType.HeaderString", loadtype, applicationPresenter)
        {
            _applicationPresenter = applicationPresenter;
            ThisLoadType = loadtype;
            _amountForTesting = 1000;
            TrueFalse.Add(true);
            TrueFalse.Add(false);
            RefreshUsedIn();
        }

        [UsedImplicitly]
        public double AmountForTesting {
            get => _amountForTesting;
            set {
                _amountForTesting = value;
                OnPropertyChanged(nameof(AmountForTesting));
                RefreshCalculations();
            }
        }

        [UsedImplicitly]
        public double ExampleOfPower {
            get => ThisLoadType.ExampleOfPower;
            set {
                ThisLoadType.ExampleOfPower = value;
                OnPropertyChanged(nameof(ExampleOfPower));
                RefreshCalculations();
            }
        }

        [UsedImplicitly]
        public double ExampleOfSum {
            get => ThisLoadType.ExampleOfSum;
            set {
                ThisLoadType.ExampleOfSum = value;
                OnPropertyChanged(nameof(ExampleOfSum));
                RefreshCalculations();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string FifteenMinuteExample
            =>
                _amountForTesting + " " + ThisLoadType.UnitOfPower + " for 15 minute is " +
                ThisLoadType.ConvertPowerValueWithTime(_amountForTesting, new TimeSpan(0, 0, 15, 0)) + " " +
                ThisLoadType.UnitOfSum;

        [NotNull]
        [UsedImplicitly]
        public string OneDayExample
            =>
                _amountForTesting + " " + ThisLoadType.UnitOfPower + " for 1 day is " +
                ThisLoadType.ConvertPowerValueWithTime(_amountForTesting, new TimeSpan(0, 24, 0, 0)) + " " +
                ThisLoadType.UnitOfSum;

        [NotNull]
        [UsedImplicitly]
        public string OneHourExample
            =>
                _amountForTesting + " " + ThisLoadType.UnitOfPower + " for 1 hour is " +
                ThisLoadType.ConvertPowerValueWithTime(_amountForTesting, new TimeSpan(0, 1, 0, 0)) + " " +
                ThisLoadType.UnitOfSum;

        [NotNull]
        [UsedImplicitly]
        public string OneMinuteExample
            =>
                _amountForTesting + " " + ThisLoadType.UnitOfPower + " for 1 minute is " +
                ThisLoadType.ConvertPowerValueWithTime(_amountForTesting, new TimeSpan(0, 0, 1, 0)) + " " +
                ThisLoadType.UnitOfSum;

        [NotNull]
        [UsedImplicitly]
        public string OneSecondExample
            =>
                _amountForTesting + " " + ThisLoadType.UnitOfPower + " for 1 second is " +
                ThisLoadType.ConvertPowerValueWithTime(_amountForTesting, new TimeSpan(0, 0, 0, 1)) + " " +
                ThisLoadType.UnitOfSum;

        [NotNull]
        [UsedImplicitly]
        public Dictionary<LoadTypePriority, string> Priorities
            => LoadTypePriorityHelper.LoadTypePriorityDictionarySelection;

        [NotNull]
        [UsedImplicitly]
        public VLoadType ThisLoadType { get; }

        [UsedImplicitly]
        public TimeSpan TimeSpanForSum {
            get => ThisLoadType.TimeSpanForSum;
            set {
                ThisLoadType.TimeSpanForSum = value;
                OnPropertyChanged(nameof(TimeSpanForSum));
                RefreshCalculations();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<bool> TrueFalse { get; } = new ObservableCollection<bool>();

        [NotNull]
        [UsedImplicitly]
        public string UnitOfPower {
            get => ThisLoadType.UnitOfPower;
            set {
                ThisLoadType.UnitOfPower = value;
                OnPropertyChanged(nameof(UnitOfPower));
                RefreshCalculations();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string UnitOfSum {
            get => ThisLoadType.UnitOfSum;
            set {
                ThisLoadType.UnitOfSum = value;
                OnPropertyChanged(nameof(UnitOfSum));
                RefreshCalculations();
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIn { get; } = new ObservableCollection<UsedIn>();

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                ThisLoadType.SaveToDB();
            }

            _applicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete()
        {
            Sim.LoadTypes.DeleteItem(ThisLoadType);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as LoadTypePresenter;
            return presenter?.ThisLoadType.Equals(ThisLoadType) == true;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + TabHeaderPath.GetHashCode();
                return hash;
            }
        }

        public void RefreshUsedIn()
        {
            var usedIn = ThisLoadType.CalculateUsedIns(Sim);
            UsedIn.Clear();
            foreach (var p in usedIn) {
                UsedIn.Add(p);
            }
        }

        private void RefreshCalculations()
        {
            OnPropertyChanged(nameof(OneSecondExample));
            OnPropertyChanged(nameof(OneMinuteExample));
            OnPropertyChanged(nameof(FifteenMinuteExample));
            OnPropertyChanged(nameof(OneHourExample));
            OnPropertyChanged(nameof(OneDayExample));
        }
    }
}