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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Tables;
using JetBrains.Annotations;

#endregion

namespace Database.Helpers {
    public class CategoryDBBase<T> : Category<T> where T : DBBase, IFilterable {
        // ReSharper disable once CollectionNeverUpdated.Local
        [NotNull] [ItemNotNull] private readonly List<Func<string, bool>> _functionsToCallOnPropertyChanged;

        [NotNull] [ItemNotNull] private ObservableCollection<T> _filteredMyItems = new ObservableCollection<T>();
        [CanBeNull] private string _filterString = string.Empty;

        [ItemNotNull] [CanBeNull] public ObservableCollection<T> _PrevFilteredMyItems;

        [CanBeNull]
        public T FindByGuid([CanBeNull] string guid)
        {
            if (guid == null) {
                return null;
            }

            return It.FirstOrDefault(x => x.Guid == guid);
        }

        [CanBeNull]
        public T FindByJsonReference([CanBeNull] JsonReference reference)
        {
            if (reference == null)
            {
                return null;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable twice HeuristicUnreachableCode
            if (reference.Guid == null) {
                return null;
            }
            return It.FirstOrDefault(x => x.Guid == reference.Guid);
        }
        public CategoryDBBase([NotNull] string name) : base(name)
        {
            MyItems = new ObservableCollection<T>();
            _functionsToCallOnPropertyChanged = new List<Func<string, bool>>();
            MyItems.CollectionChanged += OnObservableCollectionChanged;
            var type = typeof(T);
            var info = type.GetMethod("ImportFromItem");
            if (!type.IsSubclassOf(typeof(DBBaseElement))) {
                throw new LPGException("Type " + type + " is not a DBBaseElement. This is a bug!");
                //Console.WriteLine("Type " + type + " is not a DBBaseElement.This is a bug!");
            }

            if (info == null) {
                throw new LPGException("Type " + type + " is missing the ImportFromItem-Function. This is a bug!");
            }

            var info2 = type.GetMethod("CreateNewItem");
            if (info2 == null) {
                throw new LPGException("Type " + type + " is missing the CreateNewItem-Function. This is a bug!");
            }

            foreach (var myItem in MyItems) {
                _filteredMyItems.Add(myItem);
            }

            MyItems.CollectionChanged += MyItemsOnCollectionChanged;
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<T> FilteredItems => _filteredMyItems;

        private void AddItemToList([NotNull] T item)
        {
            MyItems.Add(item);
            MyItems.Sort();
        }

        protected static void AddUniqueStringToList([ItemNotNull] [NotNull] ObservableCollection<string> list,
                                                    [NotNull] string valueToAdd)
        {
            var strToAdd = valueToAdd;
            strToAdd = strToAdd.Trim();
            if (strToAdd.Length == 0) {
                return;
            }

            foreach (var s1 in list) {
                if (s1 == strToAdd) {
                    return;
                }
            }

            list.Add(strToAdd);
        }

        public override void ApplyFilter([CanBeNull] string filterStr)
        {
            _filterString = filterStr;
            if (string.IsNullOrWhiteSpace(filterStr)) {
                _filteredMyItems = MyItems;
                if (_PrevFilteredMyItems != MyItems) {
                    OnPropertyChanged(nameof(FilteredItems));
                }

                _PrevFilteredMyItems = MyItems;
                return;
            }

            var foundItems2 = new ObservableCollection<T>();
            foreach (var myItem in MyItems) {
                if (myItem.IsValid(filterStr)) {
                    foundItems2.Add(myItem);
                }
            }

            _filteredMyItems = foundItems2;
            _PrevFilteredMyItems = foundItems2;
            OnPropertyChanged(nameof(FilteredItems));
        }

        [UsedImplicitly]
        // public because of dynamic call
        public void SaveEverything()
        {
            var items = MyItems.ToList();
            foreach (var item in items) {
                item.SaveToDB();
            }
        }

    [UsedImplicitly]
        // public because of dynamic call
        public int CheckForDuplicateNames(bool saveToDB)
        {
            var count = 0;
            // fix names
            var items = MyItems.ToList();
            foreach (var item in items) {
                if(item==null) {
                    throw new LPGException("Item was null");
                }
                var name = item.Name;
                if (name.Trim() != name) {
                    item.Name = item.Name.Trim();
                    Logger.Info("Changed a name from " + name + " to " + item.Name);
                    if (saveToDB) {
                        item.SaveToDB();
                    }
                }
                if (name.Replace("  ", " ") != name) {
                    item.Name = item.Name.Replace("  ", " ");
                    Logger.Info("Changed a name from " + name + " to " + item.Name);
                    if (saveToDB) {
                        item.SaveToDB();
                    }
                }
            }
            // fix duplicates

            var repeat = true;
            while (repeat) {
                var hs = new HashSet<string>();
                T itemToChange = null;
                foreach (var item in MyItems) {
                    if (hs.Contains(item.Name.ToUpperInvariant())) {
                        itemToChange = item;
                        break;
                    }
                    hs.Add(item.Name.ToUpperInvariant());
                }
                if (itemToChange != null) {
                    var oldname = itemToChange.Name;
                    var i = 1;
                    while (i < 100 && IsNameTaken(itemToChange.Name + " " + i)) {
                        i++;
                    }

                    itemToChange.Name = itemToChange.Name + " " + i;
                    Logger.Info("Changed a name from " + oldname + " to " + itemToChange.Name);
                    count++;
                    if (saveToDB) {
                        itemToChange.SaveToDB();
                    }
                }
                else {
                    repeat = false;
                }
            }
            return count;
        }

        // used dynnamically in the simintegrity checker

        [CanBeNull]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public object CheckForNumbersInNames()
        {
            if (MyItems.Count == 0) {
                return null;
            }

            if (MyItems[0].AreNumbersOkInNameForIntegrityCheck) {
                return null;
            }
            foreach (var item in MyItems) {
                var name = item.Name;
                if (string.IsNullOrEmpty(name)) {
                    throw new DataIntegrityException("Name was null or empty. Please fix", item);
                }
                var lastspace = name.LastIndexOf(" ", StringComparison.Ordinal);
                if (lastspace > 0) {
                    var number = name.Substring(lastspace);
                    if (number.Length > 2) {
                        return null;
                    }
                    var success = int.TryParse(number, out _);
                    if (success) {
                        return item;
                    }
                }
            }
            return null;
        }

        [ItemNotNull]
        [NotNull]
        public override List<DBBase> CollectAllDBBaseItems()
        {
            var items = new List<DBBase>();
            foreach (var myItem in MyItems) {
                DBBase db = myItem;
                items.Add(db);
            }
            return items;
        }

        [NotNull]
        public T CreateNewItem([NotNull] string connectionString)
        {
            var thisType = typeof(T);
            var theMethod = thisType.GetMethod("CreateNewItem");
            if (theMethod == null) {
                throw new LPGException("Method is missing.");
            }
            var func =
                (Func<Func<string, bool>, string, DBBase>)
                Delegate.CreateDelegate(typeof(Func<Func<string, bool>, string, DBBase>), theMethod);
            var item = func(IsNameTaken, connectionString);
            if (item == null) {
                throw new LPGException("Missing Type!");
            }
            item.SaveToDB();
            var d = (T) item;
            AddItemToList(d);
            return d;
        }

        public void DeleteItem([NotNull] T db)
        {
            db.DeleteFromDB();
            Logger.Get().SafeExecuteWithWait(() => MyItems.Remove(db));
        }

        public void DeleteItemNoWait([NotNull] T db)
        {
            db.DeleteFromDB();
            Logger.Get().SafeExecute(() => MyItems.Remove(db));
        }

        [UsedImplicitly]
        [NotNull]
        public T SafeFindByName([NotNull] string name, FindMode findMode = FindMode.Exact)
        {
            foreach (var myItem in MyItems)
            {
                if (myItem.Name == name)
                {
                    return myItem;
                }
            }
            if (findMode == FindMode.IgnoreCase)
            {
                foreach (var myItem in MyItems)
                {
                    if (string.Equals(myItem.Name.ToUpperInvariant(), name.ToUpperInvariant(),
                        StringComparison.CurrentCulture))
                    {
                        return myItem;
                    }
                }
            }
            if (findMode == FindMode.Partial)
            {
                foreach (var myItem in MyItems)
                {
                    if (myItem.Name.ToUpperInvariant().Contains(name.ToUpperInvariant()))
                    {
                        return myItem;
                    }
                }
            }
            throw new LPGException("Failed to find " + name);
        }

        [UsedImplicitly]
        [CanBeNull]
        public T FindFirstByName([CanBeNull] string nameRaw, FindMode findMode = FindMode.Exact)
        {
            if (nameRaw == null) {
                return null;
            }
            //no matter which mode, if anything matches exactly, then return that.
            //this prevents errors where partial matches would return something wrong
            foreach (var myItem in MyItems) {
                if (myItem.Name == nameRaw) {
                    return myItem;
                }
            }

            string nameUpper = nameRaw.ToUpperInvariant();
            if (findMode == FindMode.IgnoreCase) {
                foreach (var myItem in MyItems) {
                    if (string.Equals(myItem.Name.ToUpperInvariant(), nameUpper,
                        StringComparison.CurrentCulture)) {
                        return myItem;
                    }
                }
            }else
            if (findMode == FindMode.Partial) {
                foreach (var myItem in MyItems) {
                    if (myItem.Name.ToUpperInvariant().Contains(nameUpper)) {
                        return myItem;
                    }
                }
            }else
            if (findMode == FindMode.StartsWith)
            {
                foreach (var myItem in MyItems)
                {
                    if (myItem.Name.ToUpperInvariant().StartsWith(nameUpper))
                    {
                        return myItem;
                    }
                }
            }
            return null;
        }

        //public int? GetIndexOf(string name, FindMode findMode = FindMode.Exact) {
        //    var item = FindByName(name, findMode);
        //    if (item != null) {
        //        return MyItems.IndexOf(item);
        //    }
        //    return null;
        //}

        public override bool ImportFromExistingElement([NotNull] DBBase item,  [NotNull] Simulator dstSim)
        {
            if (item == null) {
                throw new LPGException("Null-Item tried to import an empty item. This is a bug!");
            }
            var type = item.GetType();
            var info = type.GetMethod("ImportFromItem");
            if (info == null) {
                throw new LPGException("Type " + type + " is missing the ImportFromItem-Function. This is a bug!");
            }
            object[] parameters = {item,  dstSim};
            Logger.Info("Processing type " + type + " now.");
            var newItem = (DBBase) info.Invoke(item, parameters);
            if (newItem == null) {
                throw new LPGException(
                    "Missing Type in the import-function. This is a bug. Please contact the programmer:" +
                    item.GetType());
            }
            newItem.SaveToDB();
            var d = (T) newItem;
            AddItemToList(d);
            Logger.Info("Imported " + newItem.Name);
            return true;
        }

        public bool IsNameTaken([NotNull] string newname)
        {
            return MyItems.Any(item => item.Name == newname);
        }

        private void MyItemsOnCollectionChanged([NotNull] object sender,
            [NotNull] NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            ApplyFilter(_filterString);
        }

        private void OnObservableCollectionChanged([NotNull] object sender,
            [NotNull] NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add) {
                foreach (var newItem in notifyCollectionChangedEventArgs.NewItems) {
                    if (newItem is DBBase newdb)
                    {
                        newdb.PropertyChanged += PropertyChangedEvent;
                    }
                    else
                    {
                        throw new DataIntegrityException("Couldn't add notification to " + newItem);
                    }
                }
            }
        }

        private void PropertyChangedEvent([NotNull] object sender, [NotNull] PropertyChangedEventArgs propertyChangedEventArgs)
        {
            foreach (var func in _functionsToCallOnPropertyChanged) {
                func(propertyChangedEventArgs.PropertyName);
            }
            if (propertyChangedEventArgs.PropertyName != "Name") {
                return;
            }
            Logger.Get().SafeExecuteWithWait(MyItems.Sort);
        }

        public void SaveToDB()
        {
            foreach (var item in MyItems) {
                item.SaveToDB();
            }
        }
    }

    public enum FindMode {
        Exact,
        IgnoreCase,
        Partial,
        StartsWith
    }
}