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
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Houses;

namespace LoadProfileGenerator.Presenters.Houses {
    public class EnergyStoragePresenter : PresenterBaseDBBase<EnergyStorageView> {
        [NotNull] private readonly EnergyStorage _thisStorage;

        public EnergyStoragePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] EnergyStorageView view,
            [NotNull] EnergyStorage energyStorage) : base(view, "ThisStorage.HeaderString", energyStorage, applicationPresenter)
        {
            _thisStorage = energyStorage;
            RefreshUsedIns();
        }

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType LoadType {
            get => _thisStorage.LoadType;
            set {
                _thisStorage.LoadType = value;
                OnPropertyChanged(nameof(LoadType));
                OnPropertyChanged(nameof(SelectedUnitsString));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string SelectedUnitsString {
            get {
                var unitofpower = string.Empty;
                var unitofsum = string.Empty;
                if (_thisStorage.LoadType != null) {
                    unitofpower = _thisStorage.LoadType.UnitOfPower;
                    unitofsum = _thisStorage.LoadType.UnitOfSum;
                }
                return "[" + unitofpower + "] / [" + unitofsum + "]";
            }
        }

        [NotNull]
        public EnergyStorage ThisStorage => _thisStorage;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns { get; } = new ObservableCollection<UsedIn>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> VLoadTypes => Sim.LoadTypes.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Variable> Variables => Sim.Variables.MyItems;

        public void Delete()
        {
            Sim.EnergyStorages.DeleteItem(_thisStorage);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as EnergyStoragePresenter;
            return presenter?.ThisStorage.Equals(_thisStorage) == true;
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

        public void RefreshUsedIns()
        {
            var list = ThisStorage.CalculateUsedIns(Sim);
            UsedIns.SynchronizeWithList(list);
        }
    }
}