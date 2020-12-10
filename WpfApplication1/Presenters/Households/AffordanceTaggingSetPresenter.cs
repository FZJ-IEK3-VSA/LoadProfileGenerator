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
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public class AffordanceTaggingSetPresenter : PresenterBaseDBBase<AffordanceTaggingSetView> {
        [NotNull] private readonly AffordanceTaggingSet _affordanceTaggingSet;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<ReferenceStatistic> _refStatistics =
            new ObservableCollection<ReferenceStatistic>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<StatisticsEntry>
            _statistics = new ObservableCollection<StatisticsEntry>();

        public AffordanceTaggingSetPresenter([NotNull] ApplicationPresenter applicationPresenter,
                                             [NotNull] AffordanceTaggingSetView view,
            [NotNull] AffordanceTaggingSet aff) : base(view, "ThisAffordanceTaggingSet.HeaderString", aff, applicationPresenter)
        {
            _affordanceTaggingSet = aff;
            foreach (var entry in aff.Entries) {
                entry.AllTags = aff.Tags;
            }

            RefreshStatistics();
            RefreshRefStatistics();
            Genders.Add(PermittedGender.Male);
            Genders.Add(PermittedGender.Female);
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Affordance> Affordances => Sim.Affordances.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTag> AllTags => ThisAffordanceTaggingSet.Tags;

        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<PermittedGender> Genders { get; } = new ObservableCollection<PermittedGender>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> LoadTypes => Sim.LoadTypes.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ReferenceStatistic> RefStatistics => _refStatistics;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<StatisticsEntry> Statistics => _statistics;

        [NotNull]
        public AffordanceTaggingSet ThisAffordanceTaggingSet => _affordanceTaggingSet;

        public void AddAllAffordances()
        {
            foreach (var entry in _affordanceTaggingSet.Entries) {
                if (entry.Affordance?.Name != entry.Tag?.Name) {
                    AffordanceTag newtag = null;
                    foreach (var tag in _affordanceTaggingSet.Tags) {
                        if (tag.Name == "none") {
                            newtag = tag;
                        }
                    }

                    if (newtag != null) {
                        Logger.Warning("Fixed messed up tag:" + entry.Tag?.Name + " for " + entry.Affordance?.Name);
                        entry.Tag = newtag;
                        entry.SaveToDB();
                    }
                }
            }

            var covered = new List<string>();
            foreach (var entry in _affordanceTaggingSet.Entries) {
                if (entry.Tag?.Name.ToLower(CultureInfo.CurrentCulture) != "none") {
                    covered.Add(entry.Affordance?.Name);
                }
            }

            foreach (var affordance in Affordances) {
                if (!covered.Contains(affordance.Name)) {
                    _affordanceTaggingSet.AddNewTag(affordance.Name);
                }
            }

            foreach (var entry in _affordanceTaggingSet.Entries) {
                if (entry.Tag?.Name.ToLower(CultureInfo.CurrentCulture) == "none") {
                    foreach (var affordanceTag in _affordanceTaggingSet.Tags) {
                        if (affordanceTag.Name == entry.Affordance?.Name) {
                            entry.Tag = affordanceTag;
                            break;
                        }
                    }
                }
            }
        }

        public void AddTag([NotNull] string s)
        {
            _affordanceTaggingSet.AddNewTag(s);
        }

        public void Delete()
        {
            Sim.AffordanceTaggingSets.DeleteItem(_affordanceTaggingSet);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as AffordanceTaggingSetPresenter;
            return presenter?.ThisAffordanceTaggingSet.Equals(_affordanceTaggingSet) == true;
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

        public void RefreshRefStatistics()
        {
            foreach (var refStatistic in RefStatistics) {
                refStatistic.Value = 0;
            }

            foreach (var reference in _affordanceTaggingSet.TagReferences) {
                var key = GetRefStatisticDescription(reference.Gender, reference.MinAge, reference.MaxAge);
                var referenceStatistic = _refStatistics.FirstOrDefault(x => x.Description == key);
                if (referenceStatistic == null) {
                    referenceStatistic = new ReferenceStatistic(key);
                    _refStatistics.Add(referenceStatistic);
                }

                referenceStatistic.Value += reference.Percentage;
            }
        }

        public void RefreshStatistics()
        {
            Statistics.Clear();
            foreach (var tags in ThisAffordanceTaggingSet.Tags) {
                var entry = new StatisticsEntry(tags.Name, 0);
                _statistics.Add(entry);
            }

            foreach (var affordanceTaggingEntry in ThisAffordanceTaggingSet.Entries) {
                var tag = affordanceTaggingEntry.Tag?.Name;
                var entry = Statistics.FirstOrDefault(myEntry => myEntry.Name == tag);
                if (entry == null) {
                    throw new LPGException("Statistics Entry was null! Please Report");
                }

                entry.Count++;
            }
        }

        public void RemoveOldAffordanceTags()
        {
            ThisAffordanceTaggingSet.RemoveAllOldEntries(Sim.Affordances.Items);
        }

        [NotNull]
        private static string GetRefStatisticDescription(PermittedGender gender, int minAge, int maxAge) =>
            gender + " between " + minAge + " and " + maxAge;

        public class ReferenceStatistic : INotifyPropertyChanged {
            private double _value;

            public ReferenceStatistic([NotNull] string description) => Description = description;

            [NotNull]
            public string Description { get; }

            public double Value {
                get { return _value; }
                set {
                    if (value.Equals(_value)) {
                        return;
                    }

                    _value = value;
                    OnPropertyChanged();
#pragma warning disable S3236 // Methods with caller info attributes should not be invoked with explicit arguments
                    OnPropertyChanged(nameof(Value100));
#pragma warning restore S3236 // Methods with caller info attributes should not be invoked with explicit arguments
                }
            }

            [UsedImplicitly]
            public double Value100 => Value * 100;

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class StatisticsEntry {
            public StatisticsEntry([NotNull] string name, int count)
            {
                Name = name;
                Count = count;
            }

            public int Count { get; set; }
            [NotNull]
            public string Name { get; }
        }
    }
}