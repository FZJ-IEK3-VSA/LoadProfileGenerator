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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class DeviceTaggingSet : DBBaseElement {
        public const string TableName = "tblDeviceTaggingSets";
        [JetBrains.Annotations.NotNull] private string _description;

        public DeviceTaggingSet([JetBrains.Annotations.NotNull] string name,
                                [JetBrains.Annotations.NotNull] string description,
                                [JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] StrGuid guid,
                                [CanBeNull]int? pID = null) : base(name,
            TableName, connectionString, guid)
        {
            Tags = new ObservableCollection<DeviceTag>();
            Entries = new ObservableCollection<DeviceTaggingEntry>();
            References = new ObservableCollection<DeviceTaggingReference>();
            ID = pID;
            TypeDescription = "Device Tagging Set";
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
        public ObservableCollection<DeviceTaggingEntry> Entries { get; }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceTaggingSetLoadType> LoadTypes { get; } = new ObservableCollection<DeviceTaggingSetLoadType>();

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<DeviceTaggingReference> References { get; }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceTag> Tags { get; }

        [CanBeNull]
        public DeviceTag AddNewTag([JetBrains.Annotations.NotNull] string name)
        {
            foreach (var deviceTag in Tags) {
                if (deviceTag.Name == name) {
                    return null;
                }
            }
            var at = new DeviceTag(name, IntID, ConnectionString, null, System.Guid.NewGuid().ToStrGuid());
            Logger.Get().SafeExecuteWithWait(() => {
                Tags.Add(at);
                Tags.Sort();
            });
            SaveToDB();
            return at;
        }

        public void AddLoadType([JetBrains.Annotations.NotNull] VLoadType loadType)
        {
            if (LoadTypes.Any(x => x.LoadType == loadType)) {
                Logger.Warning("Loadtype " + loadType.Name + " is already in the list.");
                return;
            }

            var at = new DeviceTaggingSetLoadType(loadType.Name, IntID, loadType, ConnectionString, null,
                System.Guid.NewGuid().ToStrGuid());
            Logger.Get().SafeExecuteWithWait(() => LoadTypes.Add(at));
            SaveToDB();
        }

        public void AddReferenceEntry([JetBrains.Annotations.NotNull] DeviceTag tag, int personCount, double value, [JetBrains.Annotations.NotNull] VLoadType loadType)
        {
            var todelete = References
                .Where(x => x.PersonCount == personCount && x.Tag == tag).ToList();
            foreach (var deviceTaggingReference in todelete) {
                DeleteReference(deviceTaggingReference);
            }
            var refVal = new DeviceTaggingReference(string.Empty, IntID, tag, ConnectionString, null,
                personCount, value, loadType, System.Guid.NewGuid().ToStrGuid());
            References.Add(refVal);
            References.Sort();
            SaveToDB();
        }

        public void AddTaggingEntry([JetBrains.Annotations.NotNull] DeviceTag tag, [JetBrains.Annotations.NotNull] RealDevice device)
        {
            var at = new DeviceTaggingEntry(string.Empty, IntID, tag, device,
                ConnectionString, null, System.Guid.NewGuid().ToStrGuid());
            Entries.Add(at);
            Entries.Sort();
            SaveToDB();
        }

        [JetBrains.Annotations.NotNull]
        private static DeviceTaggingSet AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var description = dr.GetString("Description");
            return new DeviceTaggingSet(name, description, connectionString, System.Guid.NewGuid().ToStrGuid(), id);
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new
            DeviceTaggingSet(FindNewName(isNameTaken, "New Device Tagging Set "), "(no description)",
                connectionString, System.Guid.NewGuid().ToStrGuid());

        private void DeleteEntry([JetBrains.Annotations.NotNull] DeviceTaggingEntry at)
        {
            at.DeleteFromDB();
            Entries.Remove(at);
        }

        public override void DeleteFromDB()
        {
            base.DeleteFromDB();
            foreach (var date in Tags) {
                date.DeleteFromDB();
            }
            foreach (var value in Entries) {
                value.DeleteFromDB();
            }
            foreach (var deviceTaggingReference in References) {
                deviceTaggingReference.DeleteFromDB();
            }
        }

        public void DeleteReference([JetBrains.Annotations.NotNull] DeviceTaggingReference refVal)
        {
            refVal.DeleteFromDB();
            References.Remove(refVal);
        }

        public void DeleteTag([JetBrains.Annotations.NotNull] DeviceTag at)
        {
            at.DeleteFromDB();
            Tags.Remove(at);
        }

        [CanBeNull]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public MissingEntry GetNextMissingReference()
        {
            for (var persons = 1; persons < 7; persons++) {
                foreach (var deviceTag in Tags) {
                    var count = References.Count(x => x.PersonCount == persons && x.Tag == deviceTag);
                    if (count == 0) {
                        var me = new MissingEntry(deviceTag, persons);
                        return me;
                    }
                }
            }
            return null;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DeviceTaggingSet ImportFromItem([JetBrains.Annotations.NotNull] DeviceTaggingSet toImport, [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            var hd = new DeviceTaggingSet(toImport.Name,
                toImport.Description, dstSim.ConnectionString,System.Guid.NewGuid().ToStrGuid());
            hd.SaveToDB();
            foreach (var tag in toImport.Tags) {
                hd.AddNewTag(tag.Name);
            }
            hd.SaveToDB();
            foreach (var ate in toImport.Entries) {
                if (ate.Device != null) {
                    var rd = GetItemFromListByName(dstSim.RealDevices.Items, ate.Device.Name);
                    if (ate.Tag== null)
                    {
                        throw new LPGException("Tag was null");
                    }
                    var tag = GetItemFromListByName(hd.Tags, ate.Tag.Name);
                    if (rd == null) {
                        Logger.Error("Real device for import was null. Not importing matching tag.");
                    }
                    if (tag == null) {
                        Logger.Error("Tag for import was null. Not importing matching tag.");
                    }
                    if (rd != null && tag != null) {
                        hd.AddTaggingEntry(tag, rd);
                    }
                }
            }
            foreach (var deviceTaggingReference in toImport.References) {
                if (deviceTaggingReference.Tag == null)
                {
                    throw new LPGException("Tag was null");
                }
                var tag = GetItemFromListByName(hd.Tags, deviceTaggingReference.Tag.Name);
                var lt = GetItemFromListByName(dstSim.LoadTypes.Items, deviceTaggingReference.LoadType.Name);
                if (tag == null) {
                    Logger.Error("Tag was null while importing. Skipping.");
                    continue;
                }
                if (lt == null) {
                    Logger.Error("LoadType was null while importing. Skipping.");
                    continue;
                }
                hd.AddReferenceEntry(tag, deviceTaggingReference.PersonCount, deviceTaggingReference.ReferenceValue,
                    lt);
            }
            foreach (DeviceTaggingSetLoadType loadType in toImport.LoadTypes) {
                var lt = GetItemFromListByName(dstSim.LoadTypes.Items, loadType.Name);
                if (lt != null) {
                    hd.AddLoadType(lt);
                }
                else {
                    Logger.Warning("Not importing for the device tagging set the load type " + loadType.Name);
                }
            }
            hd.SaveToDB();
            return hd;
        }

        private static bool IsCorrectEntryParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var entry = (DeviceTaggingEntry) child;
            if (parent.ID == entry.TaggingSetID) {
                var ats = (DeviceTaggingSet) parent;
                ats.Entries.Add(entry);
                return true;
            }
            return false;
        }

        private static bool IsCorrectLoadTypeParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var entry = (DeviceTaggingSetLoadType)child;
            if (parent.ID == entry.TaggingSetID)
            {
                var ats = (DeviceTaggingSet)parent;
                ats.LoadTypes.Add(entry);
                return true;
            }
            return false;
        }

        private static bool IsCorrectReferenceParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var refValue = (DeviceTaggingReference) child;
            if (parent.ID == refValue.TaggingSetID) {
                var ats = (DeviceTaggingSet) parent;
                ats.References.Add(refValue);
                return true;
            }
            return false;
        }

        private static bool IsCorrectTagParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var at = (DeviceTag) child;
            if (parent.ID == at.TaggingSetID) {
                var ats = (DeviceTaggingSet) parent;
                ats.Tags.Add(at);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceTaggingSet> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<RealDevice> devices,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadtypes)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var tags = new ObservableCollection<DeviceTag>();
            DeviceTag.LoadFromDatabase(tags, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(tags), IsCorrectTagParent, ignoreMissingTables);

            var refValues = new ObservableCollection<DeviceTaggingReference>();
            DeviceTaggingReference.LoadFromDatabase(refValues, connectionString, ignoreMissingTables, tags, loadtypes);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(refValues), IsCorrectReferenceParent,
                ignoreMissingTables);

            var devloadtypes = new ObservableCollection<DeviceTaggingSetLoadType>();
            DeviceTaggingSetLoadType.LoadFromDatabase(devloadtypes, connectionString, ignoreMissingTables, loadtypes);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(devloadtypes),IsCorrectLoadTypeParent,
                ignoreMissingTables);

            var entries = new ObservableCollection<DeviceTaggingEntry>();
            DeviceTaggingEntry.LoadFromDatabase(entries, connectionString, ignoreMissingTables, devices, tags);
            var items2Delete = new List<DeviceTaggingEntry>();
            foreach (var entry in entries) {
                if (entry.Device == null) {
                    items2Delete.Add(entry);
                }
            }
            foreach (var entry in items2Delete) {
                entry.DeleteFromDB();
                entries.Remove(entry);
            }
            SetSubitems(new List<DBBase>(result), new List<DBBase>(entries), IsCorrectEntryParent, ignoreMissingTables);
        }

        public void RefreshDevices([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<RealDevice> devices)
        {
            DeviceTag noneTag = null;
            foreach (var deviceTag in Tags) {
                if (deviceTag.Name == "none") {
                    noneTag = deviceTag;
                    break;
                }
            }
            if (noneTag == null) {
                noneTag = AddNewTag("none");
            }
            if (noneTag == null) {
                throw new LPGException("Could not get a none tag.");
            }
            var items2Delete = new List<DeviceTaggingEntry>();
            foreach (var entry in Entries) {
                if (entry.Device == null) {
                    items2Delete.Add(entry);
                }
            }
            foreach (var entry in items2Delete) {
                DeleteEntry(entry);
            }
            foreach (var device in devices) {
                var found = false;
                foreach (var deviceTaggingEntry in Entries) {
                    if (deviceTaggingEntry.Device == device) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    AddTaggingEntry(noneTag, device);
                }
            }

            Entries.Sort();
            SaveToDB();
        }

        public void ResortEntries([JetBrains.Annotations.NotNull] Comparison<DeviceTaggingEntry> comparer)
        {
            Entries.Sort(comparer);
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var tag in Tags) {
                tag.SaveToDB();
            }
            foreach (var entry in Entries) {
                entry.SaveToDB();
            }
            foreach (var reference in References) {
                reference.SaveToDB();
            }
            foreach (DeviceTaggingSetLoadType loadType in LoadTypes) {
                loadType.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", Description);
        }

        public override string ToString() => Name;

        public class MissingEntry {
            public MissingEntry([JetBrains.Annotations.NotNull] DeviceTag tag, int personCount)
            {
                Tag = tag;
                PersonCount = personCount;
            }

            public int PersonCount { get; }

            [JetBrains.Annotations.NotNull]
            public DeviceTag Tag { get; }
        }

        public void DeleteLoadType([JetBrains.Annotations.NotNull] DeviceTaggingSetLoadType loadType)
        {
                loadType.DeleteFromDB();
                LoadTypes.Remove(loadType);
    }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((DeviceTaggingSet)toImport, dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}