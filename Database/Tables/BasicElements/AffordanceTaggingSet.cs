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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class AffordanceTaggingSet : DBBaseElement {
        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((AffordanceTaggingSet)toImport, dstSim);
        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();

        public const string TableName = "tblAffTaggingSet";
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffordanceTaggingEntry> _entries;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffordanceTagReference> _tagReferences;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffordanceTag> _tags;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffordanceTaggingSetLoadType> _loadTypes = new ObservableCollection<AffordanceTaggingSetLoadType>();
        [JetBrains.Annotations.NotNull] private string _description;
        private bool _makeCharts;

        public AffordanceTaggingSet([JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] string description, [JetBrains.Annotations.NotNull] string connectionString, bool makeCharts, [NotNull] StrGuid guid,
                                    [CanBeNull]int? pID = null) : base(name, TableName, connectionString, guid)
        {
            _tags = new ObservableCollection<AffordanceTag>();
            _entries = new ObservableCollection<AffordanceTaggingEntry>();
            _tagReferences = new ObservableCollection<AffordanceTagReference>();
            ID = pID;
            _makeCharts = makeCharts;
            TypeDescription = "Affordance Tagging Set";
            _description = description;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTaggingEntry> Entries => _entries;

        [UsedImplicitly]
        public bool MakeCharts {
            get => _makeCharts;
            set => SetValueWithNotify(value, ref _makeCharts, nameof(MakeCharts));
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTagReference> TagReferences => _tagReferences;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTaggingSetLoadType> LoadTypes => _loadTypes;

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTag> Tags => _tags;

        [CanBeNull]
        public AffordanceTag AddNewTag([JetBrains.Annotations.NotNull] string name)
        {
            foreach (var affordanceTag in _tags) {
                if (affordanceTag.Name == name) {
                    return null;
                }
            }
            ColorRGB white = new ColorRGB(255,255,255);
            var at = new AffordanceTag(name, IntID, ConnectionString, null, white, System.Guid.NewGuid().ToStrGuid());
            Logger.Get().SafeExecuteWithWait(() => {
                _tags.Add(at);
                _tags.Sort();
            });
            SaveToDB();
            return at;
        }

        public void AddNewLoadType([JetBrains.Annotations.NotNull] VLoadType loadType)
        {
            if (_loadTypes.Any(x => x.LoadType == loadType)) {
                return;
            }

            var at = new AffordanceTaggingSetLoadType(loadType.Name, IntID, loadType, ConnectionString,null,
                System.Guid.NewGuid().ToStrGuid());
            _loadTypes.Add(at);
            SaveToDB();
        }

        public void AddTaggingEntry([JetBrains.Annotations.NotNull] AffordanceTag tag, [JetBrains.Annotations.NotNull] Affordance aff)
        {
            var at = new AffordanceTaggingEntry(string.Empty, IntID, tag, aff, ConnectionString, null, System.Guid.NewGuid().ToStrGuid())
            {
                AllParentTags = _tags
            };
            Logger.Get().SafeExecuteWithWait(() => {
                _entries.Add(at);

                _entries.Sort();
                SaveToDB();
            });
        }

        public void AddTagReference([JetBrains.Annotations.NotNull] AffordanceTag tag, PermittedGender gender, int minAge, int maxAge,
            double percentage)
        {
            var at = new AffordanceTagReference(tag.Name, IntID, tag, ConnectionString, null, gender, minAge, maxAge,
                percentage, System.Guid.NewGuid().ToStrGuid());
            Logger.Get().SafeExecuteWithWait(() => {
                _tagReferences.Add(at);
                _tagReferences.Sort();
            });
            at.SaveToDB();
        }

        [JetBrains.Annotations.NotNull]
        private static AffordanceTaggingSet AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var description = dr.GetString("Description");
            var makeCharts = dr.GetBool("MakeCharts", false, true, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new AffordanceTaggingSet(name, description, connectionString, makeCharts,guid, id);
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new
            AffordanceTaggingSet(FindNewName(isNameTaken, "New Affordance Tagging Set "),
                "(no description)",
                connectionString, true, System.Guid.NewGuid().ToStrGuid());

        private void DeleteEntry([JetBrains.Annotations.NotNull] AffordanceTaggingEntry at)
        {
            at.DeleteFromDB();
            _entries.Remove(at);
        }

        public override void DeleteFromDB()
        {
            base.DeleteFromDB();
            foreach (var date in _tags) {
                date.DeleteFromDB();
            }
            foreach (var value in _entries) {
                value.DeleteFromDB();
            }
            foreach (var reference in _tagReferences) {
                reference.DeleteFromDB();
            }
        }

        public void DeleteTag([JetBrains.Annotations.NotNull] AffordanceTag at)
        {
            at.DeleteFromDB();
            _tags.Remove(at);
            var entry = _entries.FirstOrDefault(x => x.Tag == at);
            while (entry != null) {
                DeleteEntry(entry);
                entry = _entries.FirstOrDefault(x => x.Tag == at);
            }
        }

        public void DeleteTagReference([JetBrains.Annotations.NotNull] AffordanceTagReference at)
        {
            at.DeleteFromDB();
            _tagReferences.Remove(at);
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static AffordanceTaggingSet ImportFromItem([JetBrains.Annotations.NotNull] AffordanceTaggingSet toImport, [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            var hd = new AffordanceTaggingSet(toImport.Name, toImport.Description, dstSim.ConnectionString,
                toImport.MakeCharts, toImport.Guid);
            hd.SaveToDB();
            foreach (var tag in toImport.Tags) {
                hd.AddNewTag(tag.Name);
            }
            foreach (var reference in toImport.TagReferences) {
                var mytag = GetItemFromListByName(hd.Tags, reference.Tag.Name);
                if (mytag == null) {
                    Logger.Error("The tag " + reference.Tag.Name + " was not found for import. Skipping.");
                    continue;
                }
                hd.AddTagReference(mytag, reference.Gender, reference.MinAge, reference.MaxAge,
                    reference.Percentage);
            }
            foreach (var ate in toImport._entries) {
                if (ate.Affordance != null) {
                    var aff = GetItemFromListByName(dstSim.Affordances.Items, ate.Affordance.Name);
                    if (aff == null) {
                        Logger.Error("The affordance " + ate.Affordance.Name + " was not found for import. Skipping.");
                        continue;
                    }
                    if (ate.Tag== null)
                    {
                        throw new LPGException("Tag was null");
                    }
                    var mytag = GetItemFromListByName(hd.Tags, ate.Tag.Name);
                    if (mytag == null) {
                        Logger.Error("The tag " + ate.Tag.Name + " was not found for import. Skipping.");
                        continue;
                    }
                    hd.AddTaggingEntry(mytag, aff);
                }
                else {
                    Logger.Error(
                        "Skipped entry " + ate.Name +
                        " because the affordance was not found in the database anymore.");
                }
            }
            foreach (AffordanceTaggingSetLoadType taggingSetLoadType in toImport._loadTypes) {
                if (taggingSetLoadType.LoadType == null)
                {
                    throw new LPGException("Loadtype was null");
                }
                VLoadType lt = GetItemFromListByName(dstSim.LoadTypes.Items, taggingSetLoadType.LoadType.Name);
                if (lt != null) {
                    hd.AddNewLoadType(lt);
                }
                else {
                    Logger.Info("Skipping import of tagging set load type " + taggingSetLoadType.LoadType.Name);
                }
            }
            hd.SaveToDB();
            return hd;
        }

        private static bool IsCorrectEntryParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var entry = (AffordanceTaggingEntry) child;
            if (parent.ID == entry.TaggingSetID) {
                var ats = (AffordanceTaggingSet) parent;
                ats.Entries.Add(entry);
                entry.AllParentTags = ats.Tags;
                return true;
            }
            return false;
        }

        private static bool IsCorrectLoadType([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var at = (AffordanceTaggingSetLoadType)child;
            if (parent.ID == at.TaggingSetID)
            {
                var ats = (AffordanceTaggingSet)parent;
                ats.LoadTypes.Add(at);
                return true;
            }
            return false;
        }
        private static bool IsCorrectTagParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var at = (AffordanceTag) child;
            if (parent.ID == at.TaggingSetID) {
                var ats = (AffordanceTaggingSet) parent;
                ats.Tags.Add(at);
                return true;
            }
            return false;
        }

        private static bool IsCorrectTagReferenceParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var at = (AffordanceTagReference) child;
            if (parent.ID == at.TaggingSetID) {
                var ats = (AffordanceTaggingSet) parent;
                ats.TagReferences.Add(at);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        /// <summary>
        ///     Loads a tagging set from the database
        /// </summary>
        /// <param name="result"></param>
        /// <param name="connectionString"></param>
        /// <param name="ignoreMissingTables"></param>
        /// <param name="affordances"></param>
        /// <param name="loadTypes"></param>
        /// ///
        /// <exception cref="LPGException">
        ///     The automatic deletion function for cleaning the database of stale entries didn't work.
        ///     Please report!
        /// </exception>
        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<AffordanceTaggingSet> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Affordance> affordances, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadTypes)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var tags = new ObservableCollection<AffordanceTag>();
            AffordanceTag.LoadFromDatabase(tags, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(tags), IsCorrectTagParent, ignoreMissingTables);

            var tagReferences = new ObservableCollection<AffordanceTagReference>();
            AffordanceTagReference.LoadFromDatabase(tagReferences, connectionString, ignoreMissingTables, tags);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(tagReferences), IsCorrectTagReferenceParent,
                ignoreMissingTables);

            var tagloadTypes = new ObservableCollection<AffordanceTaggingSetLoadType>();
            AffordanceTaggingSetLoadType.LoadFromDatabase(tagloadTypes, connectionString, ignoreMissingTables, loadTypes);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(tagloadTypes), IsCorrectLoadType,
                ignoreMissingTables);

            var entries = new ObservableCollection<AffordanceTaggingEntry>();
            AffordanceTaggingEntry.LoadFromDatabase(entries, connectionString, ignoreMissingTables, affordances, tags);
            foreach (var entry in entries) {
                if (entry.Affordance == null) {
                    throw new LPGException(
                        "The automatic deletion function for cleaning the database of stale entries didn't work. Please report!");
                }
            }
            SetSubitems(new List<DBBase>(result), new List<DBBase>(entries), IsCorrectEntryParent, ignoreMissingTables);
        }

        public void RefreshAffordances([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Affordance> affordances)
        {
            AffordanceTag noneTag = null;
            foreach (var affordanceTag in _tags) {
                if (affordanceTag.Name == "none") {
                    noneTag = affordanceTag;
                    break;
                }
            }
            if (noneTag == null) {
                noneTag = AddNewTag("none");
            }
            if (noneTag == null) {
                throw new LPGException("Failed to get a none tag.");
            }
            var items2Delete = new List<AffordanceTaggingEntry>();
            foreach (var entry in _entries) {
                if (entry.Affordance == null) {
                    items2Delete.Add(entry);
                }
            }
            foreach (var entry in items2Delete) {
                DeleteEntry(entry);
            }
            foreach (var affordance in affordances) {
                var found = false;
                foreach (var affordanceTaggingEntry in _entries) {
                    if (affordanceTaggingEntry.Affordance == affordance) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    AddTaggingEntry(noneTag, affordance);
                }
            }

            _entries.Sort();
            SaveToDB();
        }

        public void RemoveAllOldEntries([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Affordance> allAff)
        {
            var alltags = Tags.Select(x => x.Name).ToList();
            foreach (var aff in allAff) {
                if (alltags.Contains(aff.Name)) {
                    alltags.Remove(aff.Name);
                }
            }
            foreach (var alltag in alltags) {
                var tag = _tags.FirstOrDefault(x => x.Name == alltag);
                if (tag != null) {
                    DeleteTag(tag);
                }
            }
            foreach (var reference in _tagReferences) {
                reference.SaveToDB();
            }
        }

        public void ResortEntries()
        {
            _entries.Sort();
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var tag in _tags) {
                tag.SaveToDB();
            }
            foreach (var entry in _entries) {
                entry.SaveToDB();
            }
            foreach (var reference in _tagReferences) {
                reference.SaveToDB();
            }
            foreach (AffordanceTaggingSetLoadType type in _loadTypes) {
                type.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", Description);
            cmd.AddParameter("MakeCharts", _makeCharts);
        }

        public override string ToString() => Name;

        public void DeleteLoadType([JetBrains.Annotations.NotNull] AffordanceTaggingSetLoadType loadType)
        {
            loadType.DeleteFromDB();
            _loadTypes.Remove(loadType);
        }
    }
}