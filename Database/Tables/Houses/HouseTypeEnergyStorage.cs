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
using System.Linq;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class HouseTypeEnergyStorage : DBBase {
        public const string TableName = "tblHouseTypeEnergyStorages";

        [CanBeNull] private readonly EnergyStorage _energyStorage;

        public HouseTypeEnergyStorage([CanBeNull]int? pID, [JetBrains.Annotations.NotNull] string name, int houseID, [CanBeNull] EnergyStorage es,
            [JetBrains.Annotations.NotNull] string connectionString, [NotNull] StrGuid guid)
            : base(name, TableName, connectionString, guid)
        {
            ID = pID;
            HouseID = houseID;
            _energyStorage = es;

            TypeDescription = "House Type Energy Storage";
        }

        [CanBeNull]
        public EnergyStorage EnergyStorage => _energyStorage;

        public int HouseID { get; }

        [JetBrains.Annotations.NotNull]
        private static HouseTypeEnergyStorage AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var energyStorageID = dr.GetIntFromLong("EnergyStorageID");

            var houseID = dr.GetIntFromLong("HouseID");

            var es = aic.EnergyStorages.FirstOrDefault(energyStorage => energyStorage.ID == energyStorageID);
            var name = "(no name)";
            if (es != null) {
                name = es.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            return new HouseTypeEnergyStorage(id, name, houseID, es, connectionString, guid);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_energyStorage == null) {
                message = "Energy storage not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HouseTypeEnergyStorage> result,
            [JetBrains.Annotations.NotNull] string connectionString,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<EnergyStorage> energyStorages, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(energyStorages: energyStorages);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new List<HouseTypeEnergyStorage>();
            foreach (var houseTypeEnergyStorage in result) {
                if (houseTypeEnergyStorage.EnergyStorage == null) {
                    items2Delete.Add(houseTypeEnergyStorage);
                }
            }
            foreach (var houseTypeEnergyStorage in items2Delete) {
                houseTypeEnergyStorage.DeleteFromDB();
                result.Remove(houseTypeEnergyStorage);
            }
        }

        public void SetDeleteFunction([JetBrains.Annotations.NotNull] Action<HouseTypeEnergyStorage> deleteHouseEnergyStorage)
        {
            if(_energyStorage != null) {
                _energyStorage.FunctionsToCallAfterDelete.Add(delegate { deleteHouseEnergyStorage(this); });
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("HouseID", "@HouseID", HouseID);
            if (_energyStorage != null) {
                cmd.AddParameter("EnergyStorageID", _energyStorage.IntID);
            }
        }

        public override string ToString() => Name;
    }
}