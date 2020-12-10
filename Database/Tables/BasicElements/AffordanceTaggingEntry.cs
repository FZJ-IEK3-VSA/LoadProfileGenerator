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
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
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
    public class AffordanceTaggingEntry : DBBase, IComparable<AffordanceTaggingEntry> {
        public const string TableName = "tblAffTaggingEntries";
        private readonly int _taggingSetID;

        [CanBeNull] private Affordance _affordance;

        [CanBeNull] private AffordanceTag _tag;

        public AffordanceTaggingEntry([NotNull] string name, int taggingSetID, [CanBeNull] AffordanceTag tag,
            [CanBeNull] Affordance affordance, [NotNull] string connectionString,[CanBeNull] int? pID,StrGuid guid)
            : base(name, pID, TableName,
            connectionString,guid)
        {
            _taggingSetID = taggingSetID;
            _tag = tag;
            _affordance = affordance;
            TypeDescription = "Affordance Tagging Entry";
        }

        [CanBeNull]
        [UsedImplicitly]
        public Affordance Affordance {
            get => _affordance;
            set => SetValueWithNotify(value, ref _affordance,false, nameof(Affordance));
        }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTag> AllParentTags { get; set; }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTag> AllTags { get; set; }

        [CanBeNull]
        [UsedImplicitly]
        public AffordanceTag Tag {
            get => _tag;
            set => SetValueWithNotify(value, ref _tag,false, nameof(Tag));
        }

        public int TaggingSetID => _taggingSetID;

        public int CompareTo([CanBeNull] AffordanceTaggingEntry other)
        {
            if (_tag == null || other?._tag == null) {
                return 0;
            }
            if (_affordance == null || other._affordance == null) {
                return 0;
            }

            var result = string.Compare(_tag.Name, other._tag.Name, StringComparison.Ordinal);
            if (result == 0) {
                result = string.Compare(_affordance.Name, other._affordance.Name, StringComparison.Ordinal);
            }
            return result;
        }

        [NotNull]
        private static AffordanceTaggingEntry AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var holidayDateID = dr.GetIntFromLong("ID");
            var taggingSetID = dr.GetIntFromLong("TaggingSetID");
            var tagID = dr.GetIntFromLong("TagID");
            var affordanceID = dr.GetIntFromLong("AffordanceID");
            var tag = aic.AffordanceTags.FirstOrDefault(myTag => myTag.ID == tagID);
            var affordance = aic.Affordances.FirstOrDefault(myAff => myAff.ID == affordanceID);
            var name = GetName(tag, affordance);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new AffordanceTaggingEntry(name, taggingSetID, tag, affordance, connectionString, holidayDateID,
                guid);
        }

        public override int CompareTo(object obj)
        {
            if (!(obj is AffordanceTaggingEntry other))
            {
                return 0;
            }
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        [NotNull]
        private static string GetName([CanBeNull] AffordanceTag tag, [CanBeNull] Affordance affordance)
        {
            var name = string.Empty;
            if (tag != null) {
                name = tag.Name;
            }
            if (affordance != null) {
                name += " - " + affordance.Name;
            }
            return name;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_affordance == null) {
                message = "Affordance was not found.";
                return false;
            }
            if (_tag == null) {
                message = "Tag was not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<AffordanceTaggingEntry> result,
            [NotNull] string connectionString, bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<Affordance> allAffordances,
            [ItemNotNull] [NotNull] ObservableCollection<AffordanceTag> allTags)
        {
            var aic = new AllItemCollections(affordances: allAffordances, affordanceTags: allTags);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("TaggingSetID", _taggingSetID);
            if (_tag != null) {
                cmd.AddParameter("TagID", _tag.IntID);
            }
            if (_affordance != null) {
                cmd.AddParameter("AffordanceID", _affordance.IntID);
            }
        }

        public override string ToString() => GetName(Tag, Affordance);
    }
}