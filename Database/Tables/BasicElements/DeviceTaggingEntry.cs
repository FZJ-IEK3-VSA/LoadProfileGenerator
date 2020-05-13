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
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class DeviceTaggingEntry : DBBase, IComparable<DeviceTaggingEntry> {
        public const string TableName = "tblDeviceTaggingEntries";
        private readonly int _taggingSetID;

        [CanBeNull] private RealDevice _device;

        [CanBeNull] private DeviceTag _tag;

        public DeviceTaggingEntry([NotNull] string name, int taggingSetID, [CanBeNull] DeviceTag tag,
            [CanBeNull] RealDevice device,
            [NotNull] string connectionString, [CanBeNull]int? pID, [NotNull] StrGuid guid) : base(name, pID, TableName, connectionString, guid)
        {
            _taggingSetID = taggingSetID;
            _tag = tag;
            _device = device;
            TypeDescription = "Device Tagging Entry";
        }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceTag> AllTags { get; set; }

        [CanBeNull]
        [UsedImplicitly]
        public RealDevice Device {
            get => _device;
            set => SetValueWithNotify(value, ref _device,false, nameof(Device));
        }

        [CanBeNull]
        [UsedImplicitly]
        public DeviceTag Tag {
            get => _tag;
            set => SetValueWithNotify(value, ref _tag, false,nameof(Tag));
        }

        public int TaggingSetID => _taggingSetID;

        public int CompareTo([CanBeNull] DeviceTaggingEntry other)
        {
            if (_tag == null || other?._tag == null) {
                return 0;
            }
            if (_device == null || other._device == null) {
                return 0;
            }

            var result = string.Compare(_tag.Name, other._tag.Name, StringComparison.Ordinal);
            if (result == 0) {
                result = string.Compare(_device.Name, other._device.Name, StringComparison.Ordinal);
            }
            return result;
        }

        [NotNull]
        private static DeviceTaggingEntry AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var holidayDateID =dr.GetIntFromLong("ID");
            var taggingSetID = dr.GetIntFromLong("TaggingSetID");
            var tagID = dr.GetIntFromLong("TagID");
            var deviceID = dr.GetIntFromLong("DeviceID");
            var tag = aic.DeviceTags.FirstOrDefault(myTag => myTag.ID == tagID);
            var device = aic.RealDevices.FirstOrDefault(myDev => myDev.ID == deviceID);
            var name = GetName(tag, device);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new DeviceTaggingEntry(name, taggingSetID, tag, device, connectionString, holidayDateID,
                guid);
        }

        public override int CompareTo([CanBeNull] object obj)
        {
            if (!(obj is DeviceTaggingEntry other))
            {
                return 0;
            }
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        [NotNull]
        private static string GetName([CanBeNull] DeviceTag tag, [CanBeNull] RealDevice device)
        {
            var name = string.Empty;
            if (tag != null) {
                name = tag.Name;
            }
            if (device != null) {
                name += " - " + device.Name;
            }
            return name;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            if (_tag == null) {
                message = "Tag not found.";
                return false;
            }
            if (_device == null) {
                message = "Device not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DeviceTaggingEntry> result, [NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<RealDevice> allDevices,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceTag> allTags)
        {
            var aic = new AllItemCollections(realDevices: allDevices, deviceTags: allTags);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("TaggingSetID", _taggingSetID);
            if (_tag != null) {
                cmd.AddParameter("TagID", _tag.IntID);
            }
            if (_device != null) {
                cmd.AddParameter("DeviceID", _device.IntID);
            }
        }

        public override string ToString() => GetName(Tag, Device);
    }
}