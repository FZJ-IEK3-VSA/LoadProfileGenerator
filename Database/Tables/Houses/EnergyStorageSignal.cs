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

using System.Collections.ObjectModel;
using System.Linq;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class EnergyStorageSignal : DBBase {
        public const string TableName = "tblEnergyStorageSignals";

        public EnergyStorageSignal([CanBeNull]int? pID, int energyStorageID, [CanBeNull] VLoadType loadType, double triggerLevelOn,
            double triggerLevelOff, double value, [NotNull] string connectionString, [NotNull] string name, [NotNull] string guid)
            : base(name, TableName, connectionString, guid)
        {
            LoadType = loadType;
            TriggerLevelOn = triggerLevelOn;
            TriggerLevelOff = triggerLevelOff;
            Value = value;
            EnergyStorageID = energyStorageID;
            ID = pID;
            TypeDescription = "Energy Storage Signal";
        }

        public int EnergyStorageID { get; }
        [CanBeNull]
        public VLoadType LoadType { get; }

        public double TriggerLevelOff { get; }

        public double TriggerLevelOn { get; }

        public double Value { get; }

        [NotNull]
        private static EnergyStorageSignal AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var energyStorageID = dr.GetIntFromLong("EnergyStorageID");
            var loadTypeID = dr.GetIntFromLong("LoadTypeID");
            var vlt = aic.LoadTypes.FirstOrDefault(mylt => mylt.ID == loadTypeID);
            var triggerLevelOn = dr.GetDouble("TriggerLevelOn");
            var triggerLevelOff = dr.GetDouble("TriggerLevelOff");
            var value = dr.GetDouble("Value");
            var name = "(no name)";
            if (vlt != null) {
                name = vlt.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var ess = new EnergyStorageSignal(id, energyStorageID, vlt, triggerLevelOn, triggerLevelOff,
                value, connectionString, name, guid);
            return ess;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            if (LoadType == null) {
                message = "Load type not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([NotNull][ItemNotNull] ObservableCollection<EnergyStorageSignal> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(loadTypes: loadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("EnergyStorageID", EnergyStorageID);
            if (LoadType != null) {
                cmd.AddParameter("LoadTypeID", LoadType.IntID);
            }
            cmd.AddParameter("TriggerLevelOn", TriggerLevelOn);
            cmd.AddParameter("TriggerLevelOff", TriggerLevelOff);
            cmd.AddParameter("Value", Value);
        }

        public override string ToString() => Name;
    }
}