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
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public class PersonPresenter : PresenterBaseDBBase<PersonView> {
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<UsedIn> _households;
        [JetBrains.Annotations.NotNull] private readonly Person _person;

        [CanBeNull] private Person _selectedOtherPerson;

        public PersonPresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] PersonView view, [JetBrains.Annotations.NotNull] Person person)
            : base(view, "ThisPerson.HeaderString", person, applicationPresenter)
        {
            _person = person;
            _households = new ObservableCollection<UsedIn>();
            RefreshHouseholds();
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Desire> Desires => Sim.Desires.Items;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public Dictionary<PermittedGender, string> Genders1 => GenderHelper.GenderEnumDictionary;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> Households => _households;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Person> Persons => Sim.Persons.Items;

        [UsedImplicitly]
        public PermittedGender SelectedGender {
            get => _person.Gender;
            set => _person.Gender = value;
        }

        [CanBeNull]
        [UsedImplicitly]
        public Person SelectedOtherPerson {
            get => _selectedOtherPerson;
            set {
                _selectedOtherPerson = value;
                OnPropertyChanged(nameof(SelectedOtherPerson));
            }
        }
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public Person ThisPerson => _person;

        public void CreatePersonCopy()
        {
            var p = Sim.Persons.CreateNewItem(Sim.ConnectionString);
            p.Gender = ThisPerson.Gender;
            p.Name = ThisPerson.Name + " (Copy)";
            p.Age = ThisPerson.Age;
            p.SickDays = ThisPerson.SickDays;
            p.SaveToDB();
            ApplicationPresenter.OpenItem(p);
        }

        public void Delete()
        {
            AskDeleteQuestion("Delete this person?", () => {
                Sim.Persons.DeleteItem(_person);
                Close(false);
            });
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as PersonPresenter;
            return presenter?.ThisPerson.Equals(_person) == true;
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

        public void RefreshHouseholds()
        {
            var usedInHH = ThisPerson.CalculateUsedIns(Sim);
            _households.SynchronizeWithList(usedInHH);
        }
    }
}