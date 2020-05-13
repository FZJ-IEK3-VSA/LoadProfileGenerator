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
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class DeviceTag : DBBase, IComparable<DeviceTag> {
        public const string TableName = "tblDeviceTags";

        public DeviceTag([NotNull] string name, int taggingSetID, [NotNull] string connectionString, [CanBeNull]int? pID,[NotNull] StrGuid guid)
            : base(name, pID, TableName, connectionString, guid)
        {
            TaggingSetID = taggingSetID;
            TypeDescription = "Device Tag";
        }

        public int TaggingSetID { get; }

        public int CompareTo([CanBeNull] DeviceTag other)
        {
            if (other == null) {
                return 0;
            }
            return string.CompareOrdinal(Name, other.Name);
        }

        [NotNull]
        private static DeviceTag AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var tagID = dr.GetIntFromLong("ID");
            var taggingSetID = dr.GetIntFromLong("TaggingSetID");
            var name = dr.GetString("Name");
            var guid = GetGuid(dr, ignoreMissingFields);
            return new DeviceTag(name, taggingSetID, connectionString, tagID, guid);
        }

        public override int CompareTo([CanBeNull] object obj)
        {
            if (!(obj is DeviceTag other))
            {
                return 0;
            }
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([NotNull] [ItemNotNull] ObservableCollection<DeviceTag> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", Name);
            cmd.AddParameter("TaggingSetID", TaggingSetID);
        }

        public override string ToString() => Name;
    }
}