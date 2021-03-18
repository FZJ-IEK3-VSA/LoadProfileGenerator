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
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Automation;
using Common;
using Database.Database;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    public class AffordanceDesire : DBBase {
        public const string TableName = "tblAffordanceDesires";

        [CanBeNull] private readonly Desire _desire;

        private readonly decimal _satisfactionvalue;

        public AffordanceDesire([CanBeNull]int? pID, [CanBeNull] Desire desire, [CanBeNull] int? affordanceID, decimal satisfactionvalue,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Desire> desires, [JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] string connectionString,
                                [NotNull] StrGuid guid) : base(name, TableName,
            connectionString, guid) {
            desires.CollectionChanged += DesirecollOnCollectionChanged;
            ID = pID;
            AffordanceID = affordanceID;
            _desire = desire;
            _satisfactionvalue = satisfactionvalue;
            TypeDescription = "Affordance Desire";
        }
        [CanBeNull]
        public int? AffordanceID { get; }

        [CanBeNull]
        [UsedImplicitly]
        public Func<AffordanceDesire, bool> DeleteThis { get; set; }

        [JetBrains.Annotations.NotNull]
        public Desire Desire => _desire ?? throw new InvalidOperationException();

        public decimal SatisfactionValue => _satisfactionvalue;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string SatisfactionValuePercent => (_satisfactionvalue * 100).ToString("#0.00",
            CultureInfo.CurrentCulture);

        [JetBrains.Annotations.NotNull]
        private static AffordanceDesire AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var id =  dr.GetIntFromLong("ID");
            var affordanceID = dr.GetIntFromLongOrInt("AffordanceID", false, ignoreMissingFields, -1);
            var desireID = dr.GetInt("DesireID");
            var satisfactionValue = dr.GetDecimal("SatisfactionValue");
            var name = "no name";
            var des = aic.Desires.FirstOrDefault(tp => tp.ID == desireID);
            if (des != null) {
                name = des.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var affordanceDesire = new AffordanceDesire(id, des, affordanceID, satisfactionValue,
                aic.Desires, name, connectionString, guid);
            return affordanceDesire;
        }

        public override int CompareTo(BasicElement other) {
            var otheraff = other as AffordanceDesire;
            if (otheraff?._desire != null && _desire != null) {
                return _desire.CompareTo(otheraff._desire);
            }
            return base.CompareTo(other);
        }

        private void DesirecollOnCollectionChanged([JetBrains.Annotations.NotNull] object sender,
            [JetBrains.Annotations.NotNull] NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove &&
                notifyCollectionChangedEventArgs.OldItems != null &&
                notifyCollectionChangedEventArgs.OldItems[0] == Desire) {
                var handler = DeleteThis;
                handler?.Invoke(this);
            }
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_desire == null) {
                message = "Desire not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<AffordanceDesire> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Desire> desires, bool ignoreMissingTables) {
            var aic = new AllItemCollections(desires: desires);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);

            var items2Delete = new List<AffordanceDesire>();
            foreach (var affordanceDesire in result) {
                if (affordanceDesire._desire == null || affordanceDesire.AffordanceID == null) {
                    items2Delete.Add(affordanceDesire);
                }
            }
            foreach (var desire in items2Delete) {
                desire.DeleteFromDB();
                result.Remove(desire);
            }
        }

        [JetBrains.Annotations.NotNull]
        public string MakeDescription() {
            if (_desire != null) {
                var desc = _desire.Name;
                desc += ", Satisfaction: " + SatisfactionValuePercent;
                return desc;
            }
            return "(no desire)";
        }

        protected override void SetSqlParameters(Command cmd) {
            if (AffordanceID != null) {
                cmd.AddParameter("AffordanceID", "@AffordanceID", AffordanceID);
            }
            if (_desire != null) {
                cmd.AddParameter("DesireID", "@DesireID", _desire.IntID);
            }
            cmd.AddParameter("SatisfactionValue", "@SatisfactionValue", _satisfactionvalue);
        }

        public override string ToString() {
            if (_desire != null) {
                return _desire.Name;
            }
            return "(no name)";
        }
    }
}