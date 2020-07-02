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
using System.Linq;
using Common;
using Common.Enums;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

#endregion

namespace LoadProfileGenerator.Presenters.Households {
    public class TemplatePersonPresenter : PresenterBaseDBBase<TemplatePersonView> {
        [NotNull] private readonly TemplatePerson _thisTemplate;
        private TraitPriority _selectedPriority;

        public TemplatePersonPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] TemplatePersonView view,
            [NotNull] TemplatePerson template) : base(view, "ThisTemplate.HeaderString", template, applicationPresenter)
        {
            _thisTemplate = template;
            RefreshTree(TraitPrios, Sim, template);
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTrait> FilteredTraits { get; } =
            new ObservableCollection<HouseholdTrait>();

        [NotNull]
        [UsedImplicitly]
        public Dictionary<PermittedGender, string> Genders => GenderHelper.GenderEnumDictionary;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHousehold> ModularHouseholds
            => Sim.ModularHouseholds.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Person> Persons => Sim.Persons.Items;

        [NotNull]
        [UsedImplicitly]
        public Dictionary<TraitPriority, string> Priorities => TraitPriorityHelper
            .TraitPriorityDictionaryEnumDictionaryWithAll;

        [UsedImplicitly]
        public PermittedGender SelectedGender {
            get => _thisTemplate.Gender;
            set {
                _thisTemplate.Gender = value;
                OnPropertyChanged(nameof(SelectedGender));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public object SelectedItem { get; set; }

        [UsedImplicitly]
        public TraitPriority SelectedPriority {
            get => _selectedPriority;
            set {
                _selectedPriority = value;
                FilterTraits();
            }
        }

        [NotNull]
        public TemplatePerson ThisTemplate => _thisTemplate;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TraitPrio> TraitPrios { get; } = new ObservableCollection<TraitPrio>();

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _thisTemplate.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete()
        {
            Sim.TemplatePersons.DeleteItem(_thisTemplate);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as TemplatePersonPresenter;
            return presenter?.ThisTemplate.Equals(_thisTemplate) == true;
        }

        private void FilterTraits()
        {
            List<HouseholdTrait> traits;
            if (_selectedPriority == TraitPriority.All) {
                traits = Sim.HouseholdTraits.Items.ToList();
            }
            else {
                traits =
                    Sim.HouseholdTraits.Items.Where(
                        x => x.Tags.Any(y => y.Tag.TraitPriority == _selectedPriority)).ToList();
            }
            FilteredTraits.SynchronizeWithList(traits);
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

        public void RefreshTree()
        {
            RefreshTree(TraitPrios, Sim, _thisTemplate);
        }

        public static void RefreshTree([ItemNotNull] [NotNull] ObservableCollection<TraitPrio> traitPrios, [NotNull] Simulator sim,
            [NotNull] TemplatePerson thisTemplate)
        {
            traitPrios.Clear();
            var prios = sim.TraitTags.Items.Select(x => x.TraitPriority).Distinct().ToList();
            foreach (var traitPriority in prios) {
                var traitPrio =
                    new TraitPrio(TraitPriorityHelper.TraitPriorityDictionaryEnumDictionaryComplete[traitPriority]);
                traitPrios.Add(traitPrio);
                var traitTags = sim.TraitTags.Items.Where(x => x.TraitPriority == traitPriority).ToList();
                foreach (var traitTag in traitTags) {
                    var traitCat = new TraitCategory(traitTag.Name);
                    var traits =
                        thisTemplate.Traits.Where(x => x.Trait.Tags.Any(y => y.Tag == traitTag)).ToList();
                    if (traits.Count > 0) {
                        foreach (var trait in traits) {
                            traitCat.Traits.Add(trait);
                        }
                        traitPrio.TraitCategories.Add(traitCat);
                    }
                }
            }
        }

        public class TraitCategory {
            public TraitCategory([NotNull] string name) => Name = name;

            [NotNull]
            public string Name { get; set; }

            [ItemNotNull]
            [NotNull]
            public ObservableCollection<TemplatePersonTrait> Traits { get; } =
                new ObservableCollection<TemplatePersonTrait>();
        }

        public class TraitPrio {
            public TraitPrio([NotNull] string name) => Name = name;

            [NotNull]
            public string Name { get; set; }

            [ItemNotNull]
            [NotNull]
            [UsedImplicitly]
            public ObservableCollection<TraitCategory> TraitCategories { get; } =
                new ObservableCollection<TraitCategory>();
        }
    }
}