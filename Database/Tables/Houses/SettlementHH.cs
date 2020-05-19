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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using Database.Database;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.Houses {
    public class SettlementHH : DBBase {
        public const string TableName = "tblSettlementHH";

        [CanBeNull] private readonly ICalcObject _calcObject;
        [CanBeNull]
        private readonly int? _settlementID;

        public SettlementHH([CanBeNull]int? pID, [CanBeNull] ICalcObject pHH, int count, [CanBeNull] int? settlementID,
            [NotNull] string connectionString, [NotNull] string householdName, StrGuid guid)
            : base(householdName, TableName, connectionString, guid) {
            TypeDescription = "Settlement Household";
            ID = pID;
            Count = count;
            _settlementID = settlementID;
            _calcObject = pHH;
        }
        [CanBeNull]
        public ICalcObject CalcObject => _calcObject;

        [UsedImplicitly]
        public CalcObjectType CalcObjectType {
            get {
                if (_calcObject != null) {
                    return _calcObject.CalcObjectType;
                }
                return CalcObjectType.ModularHousehold;
            }
        }

        public int Count { get; }

        [NotNull]
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        [UsedImplicitly]
        public string ObjectTypeStr {
            get {
                if (_calcObject == null) {
                    throw new LPGException("No CalcObject");
                }
                return _calcObject.CalcObjectType.ToString();
            }
        }
        [CanBeNull]
        public int? SettlementID => _settlementID;

        [NotNull]
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        private static SettlementHH AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var shhID =dr.GetIntFromLong("ID");
            var settlementID = dr.GetInt("SettlementID", false, -1, ignoreMissingFields);
            var householdID = dr.GetIntFromLongOrInt("HouseholdID", false);
            var count = dr.GetInt("Count");
            var calcObjectTypeID = dr.GetNullableIntFromLong("CalcObjectType", false, ignoreMissingFields);
            var objectType = CalcObjectType.ModularHousehold;
            if (objectType == (CalcObjectType) 1) {
                objectType = CalcObjectType.ModularHousehold;
            }
            if (calcObjectTypeID != null && calcObjectTypeID != 0) {
                objectType = (CalcObjectType) calcObjectTypeID;
            }

            ICalcObject calcObject;

            switch (objectType) {
                case CalcObjectType.ModularHousehold:
                    calcObject = aic.ModularHouseholds.FirstOrDefault(hh1 => hh1.ID == householdID);
                    break;
                case CalcObjectType.House:
                    calcObject = aic.Houses.FirstOrDefault(hh1 => hh1.ID == householdID);
                    break;

                default: throw new LPGException("Unknown CalcObjectType! This is a bug.");
            }

            var householdName = string.Empty;
            if (calcObject != null) {
                householdName = calcObject.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var shh = new SettlementHH(shhID, calcObject, count, settlementID, connectionString,
                householdName, guid);
            return shh;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_calcObject == null) {
                message = "Household or house not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<SettlementHH> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<ModularHousehold> modularHouseholds,
            [ItemNotNull] [NotNull] ObservableCollection<House> houses, bool ignoreMissingTables) {
            var aic = new AllItemCollections(
                modularHouseholds: modularHouseholds, houses: houses);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new List<SettlementHH>();
            foreach (var settlementHh in result) {
                if (settlementHh.CalcObject == null) {
                    items2Delete.Add(settlementHh);
                }
            }
            foreach (var settlementHh in items2Delete) {
                settlementHh.DeleteFromDB();
            }
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_calcObject != null) {
                cmd.AddParameter("HouseholdID", _calcObject.IntID);
                cmd.AddParameter("CalcObjectType", (int) _calcObject.CalcObjectType);
            }
            cmd.AddParameter("Count", "@Count", Count);
            if (_settlementID != null) {
                cmd.AddParameter("SettlementID", "@SettlementID", _settlementID);
            }
        }

        public override string ToString() {
            if (_calcObject == null) {
                return "(no name)";
            }
            return _calcObject.Name;
        }
    }
}