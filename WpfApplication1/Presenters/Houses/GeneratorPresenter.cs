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
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Houses;

namespace LoadProfileGenerator.Presenters.Houses {
    public class GeneratorPresenter : PresenterBaseDBBase<GeneratorView> {
        [JetBrains.Annotations.NotNull] private readonly Generator _thisGenerator;

        public GeneratorPresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] GeneratorView view, [JetBrains.Annotations.NotNull] Generator generator)
            : base(view, "ThisGenerator.HeaderString", generator, applicationPresenter)
        {
            _thisGenerator = generator;
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<DateBasedProfile> DateBasedProfiles
            => Sim.DateBasedProfiles.Items;

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType LoadType {
            get => _thisGenerator.LoadType;
            set {
                _thisGenerator.LoadType = value;
                OnPropertyChanged(nameof(LoadType));
                OnPropertyChanged(nameof(SelectedUnitsString));
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string SelectedUnitsString {
            get {
                if (_thisGenerator.LoadType == null) {
                    return string.Empty;
                }
                return "[" + _thisGenerator.LoadType.UnitOfPower + "] [" + _thisGenerator.LoadType.UnitOfSum + "]";
            }
        }

        [JetBrains.Annotations.NotNull]
        public Generator ThisGenerator => _thisGenerator;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> VLoadTypes => Sim.LoadTypes.Items;

        public void Delete()
        {
            Sim.Generators.DeleteItem(_thisGenerator);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as GeneratorPresenter;
            return presenter?.ThisGenerator.Equals(_thisGenerator) == true;
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
    }
}