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
using System.Linq;
using Automation;
using Database.Database;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    public class AffordanceSubAffordance : DBBase {
        public const string TableName = "tblAffordanceSubAffordance";
        [CanBeNull]
        private readonly int? _affordanceID;

        private readonly decimal _delayTime;

        [CanBeNull] private readonly SubAffordance _subaff;

        public AffordanceSubAffordance([CanBeNull]int? pID, [CanBeNull] SubAffordance subaff, decimal delayTime, [NotNull] string name,
            [NotNull] string connectionString, [CanBeNull]int? affordanceID, [NotNull] StrGuid guid) : base(name, TableName, connectionString, guid) {
            TypeDescription = "Affordance - Subaffordance";
            ID = pID;
            _affordanceID = affordanceID;
            _subaff = subaff;
            _delayTime = delayTime;
            if (_delayTime < 0) {
                _delayTime = 0;
            }
        }

        public AffordanceSubAffordance([CanBeNull]int? pID, [CanBeNull] SubAffordance subaff, decimal delayTime, [CanBeNull]int? affordanceID,
            [NotNull] string affordanceName, [NotNull] string connectionString, [NotNull] string subaffname, [NotNull] StrGuid guid)
            : base(subaffname + "(" + affordanceName + ")", TableName, connectionString, guid) {
            if (subaffname == null) {
                throw new ArgumentNullException(nameof(subaffname));
            }

            TypeDescription = "Affordance - Subaffordance";
            ID = pID;
            _affordanceID = affordanceID;
            _subaff = subaff;
            _delayTime = delayTime;
            if (_delayTime < 0) {
                _delayTime = 0;
            }
        }
        [CanBeNull]
        public int? AffordanceID => _affordanceID;

        public decimal DelayTime => _delayTime;

        [NotNull]
        [UsedImplicitly]
        public string Desc {
            get {
                if (_subaff == null) {
                    return "(no name)";
                }
                return _subaff.Name;
            }
        }

        [UsedImplicitly]
        public int DesireCount {
            get {
                if (SubAffordance != null) {
                    return SubAffordance.SubAffordanceDesires.Count;
                }
                return 0;
            }
        }

        [UsedImplicitly]
        [CanBeNull]
        public string DesireList => SubAffordance?.DesireList;

        [CanBeNull]
        public SubAffordance SubAffordance => _subaff;

        [NotNull]
        private static AffordanceSubAffordance AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var affordanceID = dr.GetInt("AffID", false, -1, ignoreMissingFields);
            var subAffID = dr.GetInt("SubAffID");
            var delaytime =  dr.GetDecimal("DelayTime");
            var aff = aic.Affordances.FirstOrDefault(affordance => affordance.ID == affordanceID);
            var subaff = aic.SubAffordances.FirstOrDefault(subAfford => subAfford.ID == subAffID);
            var name = "(no subaffordance)";
            var subaffname = "(no subaffordance)";
            if (subaff != null) {
                subaffname = subaff.Name;
                name = subaff.Name;
            }
            if (aff != null) {
                name = name + "(" + aff.Name + ")";
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var affsubaff = new AffordanceSubAffordance(id, subaff, delaytime, affordanceID, name,
                connectionString, subaffname, guid);
            return affsubaff;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_subaff == null) {
                message = "Subaffordance not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<AffordanceSubAffordance> result,
            [NotNull] string connectionString, [ItemNotNull] [NotNull] ObservableCollection<Affordance> affordances,
            [ItemNotNull] [NotNull] ObservableCollection<SubAffordance> subAffordances, bool ignoreMissingTables) {
            var aic = new AllItemCollections(affordances: affordances, subAffordances: subAffordances);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new List<AffordanceSubAffordance>();
            foreach (var affSubAff in result) {
                if (affSubAff._affordanceID == null || affSubAff.SubAffordance == null) {
                    items2Delete.Add(affSubAff);
                }
            }
            foreach (var affSubAff in items2Delete) {
                affSubAff.DeleteFromDB();
                result.Remove(affSubAff);
            }
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            if (_affordanceID != null) {
                cmd.AddParameter("AffID", "@AffID", _affordanceID);
            }
            if (_subaff != null) {
                cmd.AddParameter("SubAffID", "@SubAffID", _subaff.IntID);
            }
            cmd.AddParameter("DelayTime", "@DelayTime", _delayTime);
        }

        public override string ToString() {
            if (_subaff == null) {
                return "(no name)";
            }
            var s = _subaff.Name;
            return s;
        }
    }
}