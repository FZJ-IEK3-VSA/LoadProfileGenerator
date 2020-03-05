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
        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((AffordanceTaggingSet)toImport, dstSim);
        [ItemNotNull]
        [NotNull]
        public override List<UsedIn> CalculateUsedIns([NotNull] Simulator sim) => throw new NotImplementedException();

        public const string TableName = "tblAffTaggingSet";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<AffordanceTaggingEntry> _entries;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<AffordanceTagReference> _tagReferences;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<AffordanceTag> _tags;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<AffordanceTaggingSetLoadType> _loadTypes = new ObservableCollection<AffordanceTaggingSetLoadType>();
        [NotNull] private string _description;
        private bool _makeCharts;

        public AffordanceTaggingSet([NotNull] string name, [NotNull] string description, [NotNull] string connectionString, bool makeCharts, [NotNull] string guid,
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

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTaggingEntry> Entries => _entries;

        [UsedImplicitly]
        public bool MakeCharts {
            get => _makeCharts;
            set => SetValueWithNotify(value, ref _makeCharts, nameof(MakeCharts));
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTagReference> TagReferences => _tagReferences;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTaggingSetLoadType> LoadTypes => _loadTypes;

        [NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTag> Tags => _tags;

        [CanBeNull]
        public AffordanceTag AddNewTag([NotNull] string name)
        {
            foreach (var affordanceTag in _tags) {
                if (affordanceTag.Name == name) {
                    return null;
                }
            }
            ColorRGB white = new ColorRGB(255,255,255);
            var at = new AffordanceTag(name, IntID, ConnectionString, null, white, System.Guid.NewGuid().ToString());
            Logger.Get().SafeExecuteWithWait(() => {
                _tags.Add(at);
                _tags.Sort();
            });
            SaveToDB();
            return at;
        }

        public void AddNewLoadType([NotNull] VLoadType loadType)
        {
            if (_loadTypes.Any(x => x.LoadType == loadType)) {
                return;
            }

            var at = new AffordanceTaggingSetLoadType(loadType.Name, IntID, loadType, ConnectionString,null,
                System.Guid.NewGuid().ToString());
            _loadTypes.Add(at);
            SaveToDB();
        }

        public void AddTaggingEntry([NotNull] AffordanceTag tag, [NotNull] Affordance aff)
        {
            var at = new AffordanceTaggingEntry(string.Empty, IntID, tag, aff, ConnectionString, null, System.Guid.NewGuid().ToString())
            {
                AllParentTags = _tags
            };
            Logger.Get().SafeExecuteWithWait(() => {
                _entries.Add(at);

                _entries.Sort();
                SaveToDB();
            });
        }

        public void AddTagReference([NotNull] AffordanceTag tag, PermittedGender gender, int minAge, int maxAge,
            double percentage)
        {
            var at = new AffordanceTagReference(tag.Name, IntID, tag, ConnectionString, null, gender, minAge, maxAge,
                percentage, System.Guid.NewGuid().ToString());
            Logger.Get().SafeExecuteWithWait(() => {
                _tagReferences.Add(at);
                _tagReferences.Sort();
            });
            at.SaveToDB();
        }

        [NotNull]
        private static AffordanceTaggingSet AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var description = dr.GetString("Description");
            var makeCharts = dr.GetBool("MakeCharts", false, true, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new AffordanceTaggingSet(name, description, connectionString, makeCharts,guid, id);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new
            AffordanceTaggingSet(FindNewName(isNameTaken, "New Affordance Tagging Set "),
                "(no description)",
                connectionString, true, System.Guid.NewGuid().ToString());

        private void DeleteEntry([NotNull] AffordanceTaggingEntry at)
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

        public void DeleteTag([NotNull] AffordanceTag at)
        {
            at.DeleteFromDB();
            _tags.Remove(at);
            var entry = _entries.FirstOrDefault(x => x.Tag == at);
            while (entry != null) {
                DeleteEntry(entry);
                entry = _entries.FirstOrDefault(x => x.Tag == at);
            }
        }

        public void DeleteTagReference([NotNull] AffordanceTagReference at)
        {
            at.DeleteFromDB();
            _tagReferences.Remove(at);
        }

        [NotNull]
        [UsedImplicitly]
        public static AffordanceTaggingSet ImportFromItem([NotNull] AffordanceTaggingSet toImport, [NotNull] Simulator dstSim)
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
                    var aff = GetItemFromListByName(dstSim.Affordances.MyItems, ate.Affordance.Name);
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
                VLoadType lt = GetItemFromListByName(dstSim.LoadTypes.It, taggingSetLoadType.LoadType.Name);
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

        private static bool IsCorrectEntryParent([NotNull] DBBase parent, [NotNull] DBBase child)
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

        private static bool IsCorrectLoadType([NotNull] DBBase parent, [NotNull] DBBase child)
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
        private static bool IsCorrectTagParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var at = (AffordanceTag) child;
            if (parent.ID == at.TaggingSetID) {
                var ats = (AffordanceTaggingSet) parent;
                ats.Tags.Add(at);
                return true;
            }
            return false;
        }

        private static bool IsCorrectTagReferenceParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var at = (AffordanceTagReference) child;
            if (parent.ID == at.TaggingSetID) {
                var ats = (AffordanceTaggingSet) parent;
                ats.TagReferences.Add(at);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
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
        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<AffordanceTaggingSet> result, [NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<Affordance> affordances, [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes)
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

        public void RefreshAffordances([ItemNotNull] [NotNull] ObservableCollection<Affordance> affordances)
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

        public void RemoveAllOldEntries([ItemNotNull] [NotNull] ObservableCollection<Affordance> allAff)
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

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", Description);
            cmd.AddParameter("MakeCharts", _makeCharts);
        }

        public override string ToString() => Name;

        public void DeleteLoadType([NotNull] AffordanceTaggingSetLoadType loadType)
        {
            loadType.DeleteFromDB();
            _loadTypes.Remove(loadType);
        }
    }
}