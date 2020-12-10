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
using Common.Enums;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class AffordanceTagReference : DBBase, IComparable<AffordanceTagReference> {
        public const string TableName = "tblAffTagReferences";

        private readonly PermittedGender _gender;
        private readonly int _maxAge;
        private readonly int _minAge;
        private readonly double _percentage;

        [CanBeNull] private readonly AffordanceTag _tag;

        private readonly int _taggingSetID;

        public AffordanceTagReference([NotNull] string name, int taggingSetID, [CanBeNull] AffordanceTag tag,
            [NotNull] string connectionString,
                                      [CanBeNull]int? pID, PermittedGender gender, int minAge, int maxAge, double percentage, StrGuid guid)
            : base(name, pID, TableName, connectionString, guid)
        {
            _taggingSetID = taggingSetID;
            _tag = tag;
            _gender = gender;
            _minAge = minAge;
            _maxAge = maxAge;
            _percentage = percentage;
            TypeDescription = "Affordance Tag Reference";
        }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTag> AllParentTags { get; set; }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTag> AllTags { get; set; }

        public PermittedGender Gender => _gender;

        public int MaxAge => _maxAge;

        public int MinAge => _minAge;

        public double Percentage => _percentage;

        [UsedImplicitly]
        public double Percentage100 => _percentage * 100;

        [UsedImplicitly]
        [NotNull]
        public AffordanceTag Tag => _tag ?? throw new InvalidOperationException();

        public int TaggingSetID => _taggingSetID;

        public int CompareTo([CanBeNull] AffordanceTagReference other)
        {
            if (_tag == null || other?._tag == null) {
                return 0;
            }
            var result = string.Compare(_tag.Name, other._tag.Name, StringComparison.Ordinal);
            if (result == 0) {
                result = Gender.CompareTo(other.Gender);
            }
            if (result == 0) {
                result = Percentage.CompareTo(other.Percentage);
            }
            if (result == 0) {
                result = _minAge.CompareTo(other.MinAge);
            }
            return result;
        }

        [NotNull]
        private static AffordanceTagReference AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var holidayDateID = dr.GetIntFromLong("ID");
            var taggingSetID = dr.GetIntFromLong("TaggingSetID");
            var tagID = dr.GetIntFromLong("TagID");
            var tag = aic.AffordanceTags.FirstOrDefault(myTag => myTag.ID == tagID);
            var name = GetName(tag);
            var gender = (PermittedGender) dr.GetIntFromLong("Gender", false, ignoreMissingFields);
            var percentage = dr.GetDouble("Percentage", false, 0, ignoreMissingFields);
            var minAge = dr.GetIntFromLong("MinAge", false, ignoreMissingFields, 1);
            var maxAge = dr.GetIntFromLong("MaxAge", false, ignoreMissingFields, 99);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new AffordanceTagReference(name, taggingSetID, tag, connectionString, holidayDateID, gender, minAge,
                maxAge, percentage,guid);
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
        private static string GetName([CanBeNull] AffordanceTag tag)
        {
            var name = string.Empty;
            if (tag != null) {
                name = tag.Name;
            }
            return name;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_tag == null) {
                message = "Tag was not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<AffordanceTagReference> result,
            [NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<AffordanceTag> allTags)
        {
            var aic = new AllItemCollections(allTags);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("TaggingSetID", _taggingSetID);
            if (_tag != null) {
                cmd.AddParameter("TagID", _tag.IntID);
            }
            cmd.AddParameter("Gender", _gender);
            cmd.AddParameter("MinAge", _minAge);
            cmd.AddParameter("MaxAge", _maxAge);
            cmd.AddParameter("Percentage", _percentage);
        }

        public override string ToString() => GetName(Tag);
    }
}