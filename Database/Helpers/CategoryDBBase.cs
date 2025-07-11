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

using Automation;
using Automation.ResultFiles;
using Common;
using Common.Extensions;
using Database.Database;
using Database.Tables;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#endregion

namespace Database.Helpers
{
    public class CategoryDBBase<T> : Category<T> where T : DBBase, IFilterable
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        [JetBrains.Annotations.NotNull][ItemNotNull] private readonly List<Func<string, bool>> _functionsToCallOnPropertyChanged;

        [JetBrains.Annotations.NotNull][ItemNotNull] private ObservableCollection<T> _filteredMyItems = new ObservableCollection<T>();
        [CanBeNull] private string _filterString = string.Empty;

        [ItemNotNull][CanBeNull] public ObservableCollection<T> _PrevFilteredMyItems;

        [CanBeNull]
        public T FindByGuid([CanBeNull] StrGuid? guid)
        {
            if (guid == null)
            {
                return null;
            }

            return Items.FirstOrDefault(x => x.Guid == guid);
        }

        /// <summary>
        /// Returns the default object of this type. This can be used if an object of this type is needed, but no
        /// specific object was selected.
        /// As of now, the default is simply the first element in the collection.
        /// </summary>
        /// <returns>the default object of this type</returns>
        public T GetDefault()
        {
            return this[0];
        }

        /// <summary>
        /// Looks up an object with the specified JsonReference. If null is passed as reference,
        /// returns the first object in the list as a default.
        /// </summary>
        /// <param name="reference">the JsonReference of the object to search for</param>
        /// <returns>the found object or the default object</returns>
        /// <exception cref="LPGPBadParameterException">if no object with the passed JsonReference exists</exception>
        public T FindOrDefault(JsonReference reference)
        {
            if (reference == null)
            {
                return GetDefault();
            }
            return FindWithException(reference);
        }

        /// <summary>
        /// Looks up an object with the specified JsonReference. Throws exceptions
        /// in case of an invalid reference.
        /// </summary>
        /// <param name="reference">the JsonReference of the object to search for</param>
        /// <param name="nullReferenceAllowed">whether passing null as reference is allowed or leads to an error;
        /// if true and null is passed as reference, null is returned</param>
        /// <returns>the object with the specified JsonReference</returns>
        /// <exception cref="LPGPBadParameterException">if an invalid JsonReference was passed</exception>
        public T FindWithException(JsonReference? reference, bool nullReferenceAllowed = false)
        {
            var objectTypeName = typeof(T).Name;
            if (reference is null)
            {
                // no reference was specified
                if (nullReferenceAllowed)
                    return null;
                throw new LPGPBadParameterException($"No {objectTypeName} reference was specified.");
            }
            T x = FindByJsonReference(reference);
            // check if the object was found
            if (x is null)
                throw new LPGPBadParameterException($"No {objectTypeName} with the specified JsonReference found: {reference}");
            return x;
        }

        /// <summary>
        /// Finds an object with the specified JsonReference, if it exists. Looks up by GUID
        /// if available, else by name.
        /// Returns null, if no object with a matching reference is found.
        /// </summary>
        /// <param name="reference">the reference of the object to look for</param>
        /// <returns>the found object or null</returns>
        [CanBeNull]
        public T FindByJsonReference([CanBeNull] JsonReference reference)
        {
            if (reference == null)
            {
                return null;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable twice HeuristicUnreachableCode
            if (reference.Guid != null && reference.Guid != StrGuid.Empty)
            {
                // find the object with the same GUID
                foreach (var x in Items)
                {
                    if (x.Guid == reference.Guid)
                    {
                        // if the name was also given, check if it matches to avoid confusion
                        if (!string.IsNullOrEmpty(reference.Name) && x.Name != reference.Name)
                        {
                            var objectTypeName = typeof(T).Name;
                            throw new LPGPBadParameterException($"Found {objectTypeName} reference by Guid '{reference.Guid}', but name '{reference.Name}' does not match.");
                        }
                        return x;
                    }
                }
                Logger.Warning("No object with GUID " + reference.Guid + " found.");
            }
            else if (reference.Name != null)
            {
                return FindFirstByName(reference.Name);
            }

            return null;
        }
        public CategoryDBBase([JetBrains.Annotations.NotNull] string name) : base(name, new ObservableCollection<T>())
        {
            //Items = new ObservableCollection<T>();
            _functionsToCallOnPropertyChanged = new List<Func<string, bool>>();
            Items.CollectionChanged += OnObservableCollectionChanged;
            var type = typeof(T);
            var info = type.GetMethod("ImportFromItem");
            if (!type.IsSubclassOf(typeof(DBBaseElement)))
            {
                throw new LPGException("Type " + type + " is not a DBBaseElement. This is a bug!");
                //Logger.Info("Type " + type + " is not a DBBaseElement.This is a bug!");
            }

            if (info == null)
            {
                throw new LPGException("Type " + type + " is missing the ImportFromItem-Function. This is a bug!");
            }

            var info2 = type.GetMethod("CreateNewItem");
            if (info2 == null)
            {
                throw new LPGException("Type " + type + " is missing the CreateNewItem-Function. This is a bug!");
            }

            foreach (var myItem in Items)
            {
                _filteredMyItems.Add(myItem);
            }

            Items.CollectionChanged += MyItemsOnCollectionChanged;
        }

        public int Count => Items.Count;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<T> FilteredItems => _filteredMyItems;

        private void AddItemToList([JetBrains.Annotations.NotNull] T item)
        {
            Items.Add(item);
            Items.Sort();
        }

        protected static void AddUniqueStringToList([ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<string> list,
                                                    [JetBrains.Annotations.NotNull] string valueToAdd)
        {
            var strToAdd = valueToAdd;
            strToAdd = strToAdd.Trim();
            if (strToAdd.Length == 0)
            {
                return;
            }

            foreach (var s1 in list)
            {
                if (s1 == strToAdd)
                {
                    return;
                }
            }

            list.Add(strToAdd);
        }

        public override void ApplyFilter(string filterStr)
        {
            _filterString = filterStr;
            if (string.IsNullOrWhiteSpace(filterStr))
            {
                _filteredMyItems = Items;
                if (_PrevFilteredMyItems != Items)
                {
                    OnPropertyChanged(nameof(FilteredItems));
                }

                _PrevFilteredMyItems = Items;
                return;
            }

            var foundItems2 = new ObservableCollection<T>();
            foreach (var myItem in Items)
            {
                if (myItem.IsValid(filterStr))
                {
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
            var items = Items.ToList();
            foreach (var item in items)
            {
                item.SaveToDB();
            }
        }

        [UsedImplicitly]
        // public because of dynamic call
        public int CheckForDuplicateNames(bool saveToDB)
        {
            // clean up the names first
            CleanNames(saveToDB);
            return FixDuplicateNames(saveToDB);
        }

        /// <summary>
        /// Replaces duplicate names by appending a counter or adapting an existing counter.
        /// The counter is an integer separated by a single space, and starts at 1 for the first
        /// duplicate. Always continues at the highest found counter for a specific base name, so
        /// gaps in the numbering can occur.
        /// </summary>
        /// <param name="saveToDB">if True, saves the changed names to the database</param>
        /// <returns>the number of items whose names where changed</returns>
        /// <exception cref="LPGException">if there was an error selecting the new name</exception>
        private int FixDuplicateNames(bool saveToDB)
        {
            // determine the highest counter for all base names
            var highestCounters = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var item in Items)
            {
                string baseName = GetNameWithoutCounter(item.Name, out int counter);
                if (highestCounters.TryGetValue(baseName, out int currentMax))
                {
                    highestCounters[baseName] = Math.Max(currentMax, counter);
                }
                else
                {
                    highestCounters[baseName] = counter;
                }
            }

            if (highestCounters.Count == Items.Count)
            {
                // every item has a unique basename
                return 0;
            }


            // use a single database connection to add all routes for a better performance
            string connectionString = Items[0].ConnectionString;
            using var con = new Connection(connectionString);
            con.Open();
            using var tr = con.BeginTransaction();

            // replace duplicate names by appending a new counter
            int changedNameCount = 0;
            var usedNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            //foreach (var item in Items)
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                string originalName = item.Name;
                if (usedNames.Add(originalName))
                {
                    // first occurrence of this name
                    continue;
                }

                // duplicate name found
                changedNameCount++;
                string baseName = GetNameWithoutCounter(originalName, out int _);
                int counter = ++highestCounters[baseName];
                item.SetNameWithoutEvents($"{baseName} {counter}");
                Logger.Info($"Changed name from '{originalName}' to '{item.Name}'");

                if (saveToDB)
                {
                    item.SaveToDB(con);
                }

                if (!usedNames.Add(item.Name))
                    throw new LPGException($"Bug in name duplicate fixing: produced another duplicate {item.Name}");
            }
            tr.Commit();

            return changedNameCount;
        }

        /// <summary>
        /// Cleans item names by removing unnecessary whitespaces.
        /// </summary>
        /// <param name="saveToDB">if true, saves the changed names to the database</param>
        /// <exception cref="LPGException">if an item was null</exception>
        private void CleanNames(bool saveToDB)
        {
            var items = Items.ToList();
            foreach (var item in items)
            {
                if (item == null)
                {
                    throw new LPGException("Item was null");
                }
                var name = item.Name;
                if (name.Trim() != name)
                {
                    item.Name = item.Name.Trim();
                    Logger.Info("Changed a name from " + name + " to " + item.Name);
                    if (saveToDB)
                    {
                        item.SaveToDB();
                    }
                }
                if (name.Replace("  ", " ") != name)
                {
                    item.Name = item.Name.Replace("  ", " ");
                    Logger.Info("Changed a name from " + name + " to " + item.Name);
                    if (saveToDB)
                    {
                        item.SaveToDB();
                    }
                }
            }
        }

        /// <summary>
        /// Splits an item name into basename and counter. For that, splits at
        /// the last space and checks if everything behind that is an integer.
        /// If so, the part before that is the basename. Otherwise, the name does
        /// not have a counter, and the full name is returned.
        /// </summary>
        /// <param name="name">an item name</param>
        /// <param name="counter">the counter from the name, or 0 if the name has no counter</param>
        /// <returns>the basename without the counter</returns>
        private string GetNameWithoutCounter(string name, out int counter)
        {
            int index = name.LastIndexOf(' ');
            // check if the name ends with a space followed by an integer
            if (index == -1 || !int.TryParse(name[index..], out counter))
            {
                // the name does not end with a counter
                counter = 0;
                return name;
            }
            // the name ends with a counter
            return name[..index];
        }

        // used dynnamically in the simintegrity checker

        [CanBeNull]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public object CheckForNumbersInNames()
        {
            if (Items.Count == 0)
            {
                return null;
            }

            if (Items[0].AreNumbersOkInNameForIntegrityCheck)
            {
                return null;
            }
            foreach (var item in Items)
            {
                var name = item.Name;
                if (string.IsNullOrEmpty(name))
                {
                    throw new DataIntegrityException("Name was null or empty. Please fix", item);
                }
                var lastspace = name.LastIndexOf(" ", StringComparison.Ordinal);
                if (lastspace > 0)
                {
                    var number = name.Substring(lastspace);
                    if (number.Length > 2)
                    {
                        return null;
                    }
                    var success = int.TryParse(number, out _);
                    if (success)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public override List<DBBase> CollectAllDBBaseItems()
        {
            var items = new List<DBBase>();
            foreach (var myItem in Items)
            {
                DBBase db = myItem;
                items.Add(db);
            }
            return items;
        }

        [JetBrains.Annotations.NotNull]
        public T CreateNewItem([JetBrains.Annotations.NotNull] string connectionString, Database.Connection? con = null)
        {
            var thisType = typeof(T);
            var theMethod = thisType.GetMethod("CreateNewItem");
            if (theMethod == null)
            {
                throw new LPGException("Method is missing.");
            }
            var func =
                (Func<Func<string, bool>, string, DBBase>)
                Delegate.CreateDelegate(typeof(Func<Func<string, bool>, string, DBBase>), theMethod);
            var item = func(IsNameTaken, connectionString);
            if (item == null)
            {
                throw new LPGException("Missing Type!");
            }
            if (con is not null)
            {
                item.SaveToDB(con);
            }
            else
            {
                item.SaveToDB();
            }
            var d = (T)item;
            AddItemToList(d);
            return d;
        }

        public void DeleteItem([JetBrains.Annotations.NotNull] T db)
        {
            db.DeleteFromDB();
            Logger.Get().SafeExecuteWithWait(() => Items.Remove(db));
        }

        public void DeleteItemNoWait([JetBrains.Annotations.NotNull] T db)
        {
            db.DeleteFromDB();
            Logger.Get().SafeExecute(() => Items.Remove(db));
        }

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public T SafeFindByName([JetBrains.Annotations.NotNull] string name, FindMode findMode = FindMode.Exact)
        {
            foreach (var myItem in Items)
            {
                if (myItem.Name == name)
                {
                    return myItem;
                }
            }
            if (findMode == FindMode.IgnoreCase)
            {
                foreach (var myItem in Items)
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
                foreach (var myItem in Items)
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
        [JetBrains.Annotations.NotNull]
        public T FindFirstByNameNotNull([CanBeNull] string nameRaw, FindMode findMode = FindMode.Exact)
        {
            if (nameRaw == null)
            {
                throw new LPGException("Name was null");
            }
            //no matter which mode, if anything matches exactly, then return that.
            //this prevents errors where partial matches would return something wrong
            foreach (var myItem in Items)
            {
                if (myItem.Name == nameRaw)
                {
                    return myItem;
                }
            }

            string nameUpper = nameRaw.ToUpperInvariant();
            if (findMode == FindMode.IgnoreCase)
            {
                foreach (var myItem in Items)
                {
                    if (string.Equals(myItem.Name.ToUpperInvariant(), nameUpper,
                        StringComparison.CurrentCulture))
                    {
                        return myItem;
                    }
                }
            }
            else
            if (findMode == FindMode.Partial)
            {
                foreach (var myItem in Items)
                {
                    if (myItem.Name.ToUpperInvariant().Contains(nameUpper))
                    {
                        return myItem;
                    }
                }
            }
            else
            if (findMode == FindMode.StartsWith)
            {
                foreach (var myItem in Items)
                {
                    if (myItem.Name.ToUpperInvariant().StartsWith(nameUpper))
                    {
                        return myItem;
                    }
                }
            }
            throw new LPGException("Not found");
        }
        [UsedImplicitly]
        [CanBeNull]
        public T FindFirstByName([CanBeNull] string nameRaw, FindMode findMode = FindMode.Exact)
        {
            if (nameRaw == null)
            {
                return null;
            }
            //no matter which mode, if anything matches exactly, then return that.
            //this prevents errors where partial matches would return something wrong
            foreach (var myItem in Items)
            {
                if (myItem.Name == nameRaw)
                {
                    return myItem;
                }
            }

            string nameUpper = nameRaw.ToUpperInvariant();
            if (findMode == FindMode.IgnoreCase)
            {
                foreach (var myItem in Items)
                {
                    if (string.Equals(myItem.Name.ToUpperInvariant(), nameUpper,
                        StringComparison.CurrentCulture))
                    {
                        return myItem;
                    }
                }
            }
            else
            if (findMode == FindMode.Partial)
            {
                foreach (var myItem in Items)
                {
                    if (myItem.Name.ToUpperInvariant().Contains(nameUpper))
                    {
                        return myItem;
                    }
                }
            }
            else
            if (findMode == FindMode.StartsWith)
            {
                foreach (var myItem in Items)
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

        public override bool ImportFromExistingElement(DBBase item, Simulator dstSim)
        {
            if (item == null)
            {
                throw new LPGException("Null-Item tried to import an empty item. This is a bug!");
            }
            var type = item.GetType();
            var info = type.GetMethod("ImportFromItem");
            if (info == null)
            {
                throw new LPGException("Type " + type + " is missing the ImportFromItem-Function. This is a bug!");
            }
            object[] parameters = { item, dstSim };
            Logger.Info("Processing type " + type + " now.");
            var newItem = (DBBase)info.Invoke(item, parameters);
            if (newItem == null)
            {
                throw new LPGException(
                    "Missing Type in the import-function. This is a bug. Please contact the programmer:" +
                    item.GetType());
            }
            newItem.SaveToDB();
            var d = (T)newItem;
            AddItemToList(d);
            Logger.Info("Imported " + newItem.Name);
            return true;
        }

        public bool IsNameTaken([JetBrains.Annotations.NotNull] string newname)
        {
            return Items.Any(item => item.Name == newname);
        }

        private void MyItemsOnCollectionChanged([JetBrains.Annotations.NotNull] object sender,
            [JetBrains.Annotations.NotNull] NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            ApplyFilter(_filterString);
        }

        private void OnObservableCollectionChanged([JetBrains.Annotations.NotNull] object sender,
            [JetBrains.Annotations.NotNull] NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add && notifyCollectionChangedEventArgs.NewItems != null)
            {
                foreach (var newItem in notifyCollectionChangedEventArgs.NewItems)
                {
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

        private void PropertyChangedEvent([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] PropertyChangedEventArgs propertyChangedEventArgs)
        {
            foreach (var func in _functionsToCallOnPropertyChanged)
            {
                func(propertyChangedEventArgs.PropertyName);
            }
            if (propertyChangedEventArgs.PropertyName != "Name")
            {
                return;
            }
            Logger.Get().SafeExecuteWithWait(Items.Sort);
        }

        public void SaveToDB()
        {
            foreach (var item in Items)
            {
                item.SaveToDB();
            }
        }
    }

    public enum FindMode
    {
        Exact,
        IgnoreCase,
        Partial,
        StartsWith
    }
}
