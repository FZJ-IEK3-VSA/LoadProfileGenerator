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

using System.Collections.ObjectModel;
using System.Linq;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.Transportation {
    public class TransportationDeviceLoad : DBBase {
        public const string TableName = "tblTransportationDeviceLoads";

        [CanBeNull] private readonly VLoadType _loadType;

        private readonly double _maxPower;
        [CanBeNull]
        private readonly int? _transportationDeviceID;

        public TransportationDeviceLoad([NotNull] string name, [CanBeNull] int? transportationDeviceID, double maxPower,
            [CanBeNull] VLoadType loadType,
             [NotNull] string connectionString,[NotNull] string guid, [CanBeNull] int? id = null)
            : base(name, TableName, connectionString, guid) {
            _transportationDeviceID = transportationDeviceID;
            _maxPower = maxPower;
            ID = id;
            _loadType = loadType;
            TypeDescription = "Transportation Device Load";
        }

        [CanBeNull]
        public VLoadType LoadType => _loadType;

        public double MaxPower => _maxPower;
        [CanBeNull]
        public int? TransportationDeviceID => _transportationDeviceID;

        [NotNull]
        private static TransportationDeviceLoad AssignFields([NotNull] DataReader dr, [NotNull]
                                                             string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = (int) dr.GetLong("ID");
            var realDeviceID = dr.GetNullableIntFromLong("TransportationDeviceID", false);
            var loadTypeID = (int) dr.GetLong("LoadTypeID");
            var value = dr.GetDouble("Value");
            var lt = aic.LoadTypes.FirstOrDefault(x => x.ID == loadTypeID);
            var name = "(no name)";
            if (lt != null) {
                name = lt.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var tup = new TransportationDeviceLoad(name, realDeviceID,  value, lt,
                connectionString,guid, id);
            return tup;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_loadType == null) {
                message = "Load type not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TransportationDeviceLoad> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes, bool ignoreMissingTables) {
            var aic = new AllItemCollections(loadTypes: loadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            if (_transportationDeviceID != null) {
                cmd.AddParameter("TransportationDeviceID", _transportationDeviceID);
            }
            if (_loadType != null) {
                cmd.AddParameter("LoadTypeID", _loadType.IntID);
            }
            cmd.AddParameter("Value", _maxPower);
        }
    }
}