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
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public class SubAffordanceDesire : DBBase {
        public const string TableName = "tblSubAffordanceDesires";

        [CanBeNull] private readonly Desire _desire;

        private readonly decimal _satisfactionvalue;

        [CanBeNull] private Func<SubAffordanceDesire, bool> _deleteThis;

        public SubAffordanceDesire([CanBeNull]int? pID, [CanBeNull] Desire desire, [CanBeNull]int? affordanceID, decimal satisfactionvalue,
            [ItemNotNull][NotNull] ObservableCollection<Desire> simdesires, [CanBeNull] Func<SubAffordanceDesire, bool> deleteThis,
            [NotNull] string connectionString,
            [NotNull] string name, [NotNull] string guid) : base(name, TableName, connectionString, guid) {
            ID = pID;
            _deleteThis = deleteThis;
            _desire = desire;
            SubaffordanceID = affordanceID;
            _satisfactionvalue = satisfactionvalue;
            simdesires.CollectionChanged += SimdesiresOnCollectionChanged;
            TypeDescription = "Subaffordance - Desire";
        }

        [CanBeNull]
        public Desire Desire => _desire;

        public decimal SatisfactionValue => _satisfactionvalue;

        [NotNull]
        public string SatisfactionValuePercent => (_satisfactionvalue * 100).ToString("#0.00",
            CultureInfo.CurrentCulture);
        [CanBeNull]
        public int? SubaffordanceID { get; }

        [NotNull]
        private static SubAffordanceDesire AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            int? affordanceID = null;
            if (dr["SubAffordanceID"] != DBNull.Value) {
                affordanceID =  dr.GetInt("SubAffordanceID");
            }
            var desireID = dr.GetInt("DesireID");
            var satisfactionValue =  dr.GetDecimal("SatisfactionValue");
            var dstDesire = aic.Desires.FirstOrDefault(desire => desire.IntID == desireID);
            var name = "(no name)";
            if (dstDesire != null) {
                name = dstDesire.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var affordanceDesire = new SubAffordanceDesire(id, dstDesire, affordanceID,
                satisfactionValue, aic.Desires, null, connectionString, name, guid);
            return affordanceDesire;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_desire == null) {
                message = "Desire not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<SubAffordanceDesire> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<Desire> desires, bool ignoreMissingTables) {
            var aic = new AllItemCollections(desires: desires);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new List<SubAffordanceDesire>();
            foreach (var subAffordanceDesire in result) {
                if (subAffordanceDesire._desire == null || subAffordanceDesire.SubaffordanceID == null) {
                    items2Delete.Add(subAffordanceDesire);
                }
            }
            foreach (var desire in items2Delete) {
                desire.DeleteFromDB();
                result.Remove(desire);
            }
        }

        [NotNull]
        public string MakeDescription() {
            if (_desire != null) {
                var desc = _desire.Name;
                desc += ", Satisfaction: " + SatisfactionValuePercent;
                return desc;
            }
            return "(no desire)";
        }

        public void SetDeleteFuncFromParent([NotNull] Func<SubAffordanceDesire, bool> deleteThis) {
            _deleteThis = deleteThis;
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            if (SubaffordanceID != null) {
                cmd.AddParameter("SubAffordanceID", "@SubAffordanceID", SubaffordanceID);
            }
            if (_desire != null) {
                cmd.AddParameter("DesireID", "@DesireID", _desire.IntID);
            }
            cmd.AddParameter("SatisfactionValue", "@SatisfactionValue", _satisfactionvalue);
        }

        private void SimdesiresOnCollectionChanged([NotNull] object sender,
            [NotNull] NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove &&
                notifyCollectionChangedEventArgs.OldItems[0] == Desire) {
                var handler = _deleteThis;
                handler?.Invoke(this);
            }
        }

        public override string ToString() {
            if (_desire == null) {
                return "(none)";
            }
            var s = _desire.Name;
            return s;
        }
    }
}