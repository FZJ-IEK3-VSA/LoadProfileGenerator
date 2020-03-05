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
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Automation.ResultFiles;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public class DeviceTaggingSetPresenter : PresenterBaseDBBase<DeviceTaggingSetView> {
        [NotNull] private readonly DeviceTaggingSet _deviceTaggingSet;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<StatisticsEntry> _statistics = new ObservableCollection<StatisticsEntry>();

        [NotNull] private string _sortBy;

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public DeviceTaggingSetPresenter([NotNull] ApplicationPresenter applicationPresenter,
                                         [NotNull] DeviceTaggingSetView view,
                                         [NotNull] DeviceTaggingSet aff) : base(view, "ThisDeviceTaggingSet.HeaderString", aff, applicationPresenter)
        {
            _deviceTaggingSet = aff;
            foreach (var entry in aff.Entries) {
                entry.AllTags = aff.Tags;
            }

            RefreshStatistics();
            SortByOptions.Add("By Device");
            SortByOptions.Add("By Device Category");
            SortByOptions.Add("By Tag");
            _sortBy = SortByOptions[0];
            RefreshReferenceStatistic();
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> AllLoadTypes => Sim.LoadTypes.It;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<RealDevice> Devices => Sim.RealDevices.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<RefStatisEntry> RefStatisEntries { get; } = new ObservableCollection<RefStatisEntry>();

        [NotNull]
        [UsedImplicitly]
        public string SortBy {
            get => _sortBy;
            set {
                _sortBy = value;
                Resort();
                OnPropertyChanged(nameof(SortBy));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public List<string> SortByOptions { get; } = new List<string>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<StatisticsEntry> Statistics => _statistics;

        [NotNull]
        public DeviceTaggingSet ThisDeviceTaggingSet => _deviceTaggingSet;

        public void AddReferenceValue([NotNull] DeviceTag tag, int personcount, double refval, [NotNull] VLoadType loadtype)
        {
            _deviceTaggingSet.AddReferenceEntry(tag, personcount, refval, loadtype);
        }

        public void AddTag([NotNull] string s)
        {
            _deviceTaggingSet.AddNewTag(s);
        }

        public void Delete()
        {
            Sim.DeviceTaggingSets.DeleteItem(_deviceTaggingSet);
            Close(false);
        }

        public override bool Equals([CanBeNull] object obj) =>
            obj is DeviceTaggingSetPresenter presenter && presenter.ThisDeviceTaggingSet.Equals(_deviceTaggingSet);

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

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public void RefreshReferenceStatistic()
        {
            foreach (var deviceTaggingReference in _deviceTaggingSet.References) {
                var e = RefStatisEntries.FirstOrDefault(x => x.PersonCount == deviceTaggingReference.PersonCount);
                if (e == null) {
                    e = new RefStatisEntry(deviceTaggingReference.PersonCount, 0);
                    RefStatisEntries.Add(e);
                }

                e.Sum += deviceTaggingReference.ReferenceValue;
            }
        }

        public void RefreshStatistics()
        {
            Statistics.Clear();
            foreach (var tags in ThisDeviceTaggingSet.Tags) {
                var entry = new StatisticsEntry(0, tags.Name);
                _statistics.Add(entry);
            }

            foreach (var affordanceTaggingEntry in ThisDeviceTaggingSet.Entries) {
                var tag = affordanceTaggingEntry.Tag?.Name;
                var entry = Statistics.First(myEntry => myEntry.Name == tag);
                entry.Count++;
            }
        }

        public void Resort()
        {
            ThisDeviceTaggingSet.ResortEntries(Comparison);
        }

        private int Comparison([NotNull] DeviceTaggingEntry x, [NotNull] DeviceTaggingEntry y)
        {
            switch (SortBy) {
                case "By Device":
                    return string.Compare(x.Device?.Name, y.Device?.Name, StringComparison.Ordinal);
                case "By Tag": {
                    if (x.Tag == null) {
                        return 0;
                    }

                    var result = x.Tag.CompareTo(y.Tag);
                    if (result == 0) {
                        result = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    }

                    return result;
                }
                case "By Device Category": {
                    if (x.Device?.DeviceCategory == null) {
                        return 0;
                    }

                    if (y.Device?.DeviceCategory == null) {
                        return 0;
                    }

                    var result = x.Device.DeviceCategory.CompareTo(y.Device.DeviceCategory);
                    if (result == 0) {
                        result = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    }

                    return result;
                }
                default:
                    throw new LPGException("Unknown Sort By");
            }
        }

        public class RefStatisEntry : INotifyPropertyChanged {
            private double _sum;

            /// <exception cref="Exception">A delegate callback throws an exception.</exception>
            public RefStatisEntry(int personCount, double sum)
            {
                PersonCount = personCount;
                Sum = sum;
            }

            public int PersonCount { get; }

            public double Sum {
                get => _sum;
                set {
                    if (value.Equals(_sum)) {
                        return;
                    }

                    _sum = value;
                    OnPropertyChanged1();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            /// <exception cref="Exception">A delegate callback throws an exception.</exception>
            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1([CanBeNull] [CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class StatisticsEntry {
            public StatisticsEntry(int count, [NotNull] string name)
            {
                Count = count;
                Name = name;
            }

            public int Count { get; set; }

            [NotNull]
            public string Name { get; set; }
        }
    }
}