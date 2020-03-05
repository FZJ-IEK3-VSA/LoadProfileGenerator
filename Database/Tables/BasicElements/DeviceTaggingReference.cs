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
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class DeviceTaggingReference : DBBase, IComparable<DeviceTaggingReference> {
        public const string TableName = "tblDeviceTaggingReferences";

        [CanBeNull] private readonly VLoadType _loadType;

        private readonly int _personCount;
        private readonly double _referenceValue;
        private readonly int _taggingSetID;

        [CanBeNull] private DeviceTag _tag;

        public DeviceTaggingReference([NotNull] string name, int taggingSetID, [CanBeNull] DeviceTag tag, [NotNull] string connectionString,
                                      [CanBeNull]int? pID,
            int personCount, double referenceValue, [CanBeNull] VLoadType loadType,
            [NotNull] string guid) : base(name, pID, TableName,
            connectionString, guid)
        {
            _taggingSetID = taggingSetID;
            _tag = tag;
            _personCount = personCount;
            _referenceValue = referenceValue;
            _loadType = loadType;
            TypeDescription = "Device Tagging Reference Entry";
        }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceTag> AllTags { get; set; }

        [UsedImplicitly]
        [NotNull]
        public VLoadType LoadType => _loadType ?? throw new InvalidOperationException();

        public int PersonCount => _personCount;

        public double ReferenceValue => _referenceValue;

        [CanBeNull]
        [UsedImplicitly]
        public DeviceTag Tag {
            get => _tag;
            set => SetValueWithNotify(value, ref _tag,false,  nameof(Tag));
        }

        public int TaggingSetID => _taggingSetID;

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public int CompareTo([CanBeNull] DeviceTaggingReference other)
        {
            if (_tag == null || other?._tag == null) {
                return 0;
            }
            if (other.LoadType != null && _loadType != null) {
                var ltresult = string.Compare(_loadType.Name, other.LoadType.Name, StringComparison.Ordinal);
                if (ltresult != 0) {
                    return ltresult;
                }
            }
            var result = _personCount.CompareTo(other._personCount);
            if (result != 0) {
                return result;
            }
            result = string.Compare(_tag.Name, other._tag.Name, StringComparison.Ordinal);
            if (result != 0) {
                return result;
            }
            return _referenceValue.CompareTo(other._referenceValue);
        }

        [NotNull]
        private static DeviceTaggingReference AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var taggingSetID = dr.GetIntFromLong("TaggingSetID");
            var tagID = dr.GetIntFromLong("TagID");
            var tag = aic.DeviceTags.FirstOrDefault(myTag => myTag.ID == tagID);
            var personCount = dr.GetIntFromLong("PersonCount", false, ignoreMissingFields, -1);
            var referenceValue = dr.GetDouble("ReferenceValue", false, -1, ignoreMissingFields);
            var loadtypeID = dr.GetIntFromLong("LoadTypeID", false, ignoreMissingFields);
            var loadType = aic.LoadTypes.FirstOrDefault(x => x.IntID == loadtypeID);
            if (loadType == null) {
                loadType = aic.LoadTypes.FirstOrDefault(
                    x => x.Name.ToLower(CultureInfo.CurrentCulture) == "electricity");
            }
            var name = GetName(tag, personCount, referenceValue, loadType);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new DeviceTaggingReference(name, taggingSetID, tag, connectionString, id, personCount,
                referenceValue,
                loadType,guid);
        }

        public override int CompareTo([CanBeNull] object obj)
        {
            if (!(obj is DeviceTaggingReference other))
            {
                return 0;
            }
            return CompareTo(other);
        }

        [NotNull]
        private static string GetName([CanBeNull] DeviceTag tag, int personcount, double referenceValue,
            [CanBeNull] VLoadType loadType)
        {
            var name = string.Empty;
            if (tag != null) {
                name = tag.Name;
            }
            name += " Persons: " + personcount + " Value: " + referenceValue;
            if (loadType != null) {
                name += ", Loadtype: " + loadType.PrettyName;
            }
            return name;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            if (_tag == null) {
                message = "Tag not found.";
                return false;
            }
            if (_loadType == null) {
                message = "Loadtype not found.";
                return false;
            }

            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DeviceTaggingReference> result,
            [NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<DeviceTag> allTags,
            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes)
        {
            var aic = new AllItemCollections(deviceTags: allTags, loadTypes: loadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("TaggingSetID", _taggingSetID);
            if (_tag != null) {
                cmd.AddParameter("TagID", _tag.IntID);
            }
            cmd.AddParameter("ReferenceValue", _referenceValue);
            cmd.AddParameter("PersonCount", _personCount);
            if (_loadType != null) {
                cmd.AddParameter("LoadTypeID", _loadType.IntID);
            }
        }

        public override string ToString() => GetName(Tag, _personCount, _referenceValue, _loadType);
    }
}