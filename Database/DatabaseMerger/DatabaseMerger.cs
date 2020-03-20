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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Automation.ResultFiles;
using Common;
using Database.Helpers;
using Database.Tables;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.Validation;
using JetBrains.Annotations;

namespace Database.DatabaseMerger {
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class DatabaseMerger {
        [ItemNotNull] [NotNull] private readonly ObservableCollection<ImportEntry> _itemsToImport = new ObservableCollection<ImportEntry>();
        [NotNull] private readonly Simulator _mainSimulator;

        [CanBeNull] private Simulator _oldSimulator;

        public DatabaseMerger([NotNull] Simulator simulator) => _mainSimulator = simulator;

        [UsedImplicitly]
        public bool DisplayMessageBox { get; set; }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<ImportEntry> ItemsToImport => _itemsToImport;

        [NotNull]
        public Simulator OldSimulator => _oldSimulator ?? throw new InvalidOperationException();

        private static int Comparison([NotNull] BasicCategory category1, [NotNull] BasicCategory category2) => category1.LoadingNumber
            .CompareTo(category2.LoadingNumber);

        [CanBeNull]
        private static BasicCategory GetBasicCategory([NotNull] Type t, [ItemNotNull] [NotNull] List<BasicCategory> categories) {
            foreach (var basicCategory in categories) {
                if (basicCategory.GetType() == t) {
                    return basicCategory;
                }
            }
            return null;
        }

        [ItemNotNull]
        [NotNull]
        private static List<BasicCategory> GetCategoriesFromSim([NotNull] Simulator sim) {
            var categories = new List<BasicCategory>();
            foreach (var o in sim.Categories) {
                if (o is BasicCategory bc)
                {
                    var t = bc.GetType();
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(CategoryDBBase<>))
                    {
                        categories.Add(bc);
                    }
                }
            }
            categories.Add(sim.Affordances);
            categories.Add(sim.DeviceCategories);
            sim.DeviceCategories.RefreshRootCategories();
            categories.Add(sim.Settlements);
            categories.Add(sim.CalculationOutcomes);
            categories.Sort(Comparison);
            return categories;
        }

        [NotNull]
        private static BasicCategory GetCategoryFromSim([NotNull] Simulator sim, [NotNull] Type genericType) {
            foreach (var o in sim.Categories) {
                if (o is BasicCategory bc)
                {
                    var t = bc.GetType();
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(CategoryDBBase<>) &&
                        t.GetGenericArguments()[0] == genericType)
                    {
                        return bc;
                    }
                }
            }
            if (genericType == typeof(DeviceCategory)) {
                return sim.DeviceCategories;
            }
            if (genericType == typeof(Affordance)) {
                return sim.Affordances;
            }
            if (genericType == typeof(Settlement)) {
                return sim.Settlements;
            }
            if (genericType == typeof(CalculationOutcome)) {
                return sim.CalculationOutcomes;
            }
            throw new LPGException("Could not find match for type");
        }

        public void RunFindItems([CanBeNull] string fileName, [CanBeNull] Action<int> progressFunction) {
            Logger.Get().SafeExecute(_itemsToImport.Clear);
            var connectionstrOld = "Data Source=" + fileName;
            var oldSim = new Simulator(connectionstrOld, true);
            _oldSimulator = oldSim;
            var oldcategories = GetCategoriesFromSim(oldSim);
            var newcategories = GetCategoriesFromSim(_mainSimulator);
            var categoryNumber = 0;
            var loadingnumbers = new List<int>();
            foreach (var basicCategory in newcategories) {
                if (loadingnumbers.Contains(basicCategory.LoadingNumber)) {
                    throw new LPGException(
                        "Duplicate loading step number for " + basicCategory.Name + ". Please report");
                }
                loadingnumbers.Add(basicCategory.LoadingNumber);
            }

            foreach (var basicCategory in oldcategories) {
                categoryNumber++;
                progressFunction?.Invoke(categoryNumber);

                var newCategory = GetBasicCategory(basicCategory.GetType(), newcategories);
                if (newCategory != null) {
                    var oldItems = basicCategory.CollectAllDBBaseItems();
                    var newItems = newCategory.CollectAllDBBaseItems();
                    var allNewNames = new Dictionary<string, string>();
                    foreach (var item in newItems) {
                        if (!allNewNames.ContainsKey(item.Name.ToUpperInvariant())) {
                            allNewNames.Add(item.Name.ToUpperInvariant(), item.Name.ToUpperInvariant());
                        }
                    }
                    foreach (var oldItem in oldItems) {
                        if (!allNewNames.ContainsKey(oldItem.Name.ToUpperInvariant())) {
                            Logger.Get()
                                .SafeExecute(
                                    () => _itemsToImport.Add(new ImportEntry(oldItem, basicCategory.LoadingNumber)));
                        }
                    }
                }
            }
            Logger.Get().SafeExecute(_itemsToImport.Sort);
        }

        public void RunImport([CanBeNull] Action<int> reportProgress) {
            var count = 0;
            Logger.Get().SafeExecuteWithWait(_itemsToImport.Sort);
            foreach (var entry in _itemsToImport) {
                count++;
                reportProgress?.Invoke(count);
                if (entry.Import) {
                    var bc = GetCategoryFromSim(_mainSimulator, entry.Entry.GetType());
                    var isfinished = false;
                    Logger.Get()
                        .SafeExecute(() => {
                            bc.ImportFromExistingElement(entry.Entry, _mainSimulator);
                            isfinished = true;
                        });
                    while (!isfinished) {
                        Thread.Sleep(1);
                    }
                }
            }
            var changecount = 0;
            foreach (var category in _mainSimulator.Categories) {
                var thisType = category.GetType();
                if (thisType.Name.Contains("CategoryDBBase") || // || thisType.Name.Contains("CategoryDeviceCategory")
                    thisType.Name.Contains(nameof(CategorySettlement))) {
                    dynamic d = category;
                    changecount += d.CheckForDuplicateNames(false);
                    if (changecount != 0 && Config.IsInUnitTesting) {
                        throw new LPGException("Something went very wrong in the import!");
                    }
                }
            }
            if (changecount != 0 && Config.IsInUnitTesting) {
                throw new LPGException("Something went very wrong in the import!");
            }
            if (DisplayMessageBox && changecount != 0) {
                 MessageWindowHandler.Mw.ShowInfoMessage("The import encountered and error. Please check carefully.",
                    "Import error.");
            }
            else if (DisplayMessageBox) {
                MessageWindowHandler.Mw.ShowInfoMessage("Finished the import!", "Finished");
            }
        }

        public class ImportEntry : INotifyPropertyChanged, IComparable {
            private readonly int _step;
            private bool _import;

            public ImportEntry([NotNull] DBBase entry, int step) {
                Entry = entry;
                _step = step;
                _import = true;
                if (step < 1) {
                    throw new LPGException("The entry " + entry.Name +
                                           " has a loading number smaller than 1. This is a bug. It has the type " +
                                           entry.GetType() + ". Please report!");
                }
            }

            [NotNull]
            public DBBase Entry { get; set; }

            public bool Import {
                get => _import;
                set {
                    _import = value;
                    OnPropertyChanged(nameof(Import));
                }
            }

            [NotNull]
            public string TypeDescription => Entry.TypeDescription;

            public int CompareTo([CanBeNull] object obj) {
                if (obj == null) {
                    return 0;
                }
                if (!(obj is ImportEntry other)) {
                    return 0;
                }

                return _step.CompareTo(other._step);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([NotNull] string propertyName) {
                var handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            [NotNull]
            public override string ToString() => Entry.GetType() + " " + Entry.Name;
        }
    }
}