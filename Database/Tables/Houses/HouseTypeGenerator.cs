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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class HouseTypeGenerator : DBBase {
        public const string TableName = "tblHouseTypeGenerators";

        [CanBeNull] private readonly Generator _generator;

        public HouseTypeGenerator([CanBeNull]int? pID, int houseID, [CanBeNull] Generator gen, [NotNull] string connectionString,
            [NotNull] string generatorName, StrGuid guid)
            : base(generatorName, TableName, connectionString, guid)
        {
            ID = pID;
            HouseID = houseID;
            _generator = gen;
            TypeDescription = "House Type Generator";
        }

        [CanBeNull]
        public Generator Generator => _generator;

        public int HouseID { get; }

        public override string Name {
            get {
                if (_generator != null) {
                    return _generator.Name;
                }
                return base.Name;
            }
        }

        [NotNull]
        private static HouseTypeGenerator AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var generatorID = dr.GetIntFromLong("GeneratorID");
            var houseID = dr.GetIntFromLong("HouseID");
            var mygen = aic.Generators.FirstOrDefault(gen => gen.ID == generatorID);
            var generatorName = string.Empty;
            if (mygen != null) {
                generatorName = mygen.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            return new HouseTypeGenerator(id, houseID, mygen, connectionString, generatorName,
                guid);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (Generator == null) {
                message = "Generator not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HouseTypeGenerator> result, [NotNull] string connectionString,
            [NotNull] [ItemNotNull] ObservableCollection<Generator> generators, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(generators: generators);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new List<HouseTypeGenerator>();
            foreach (var houseGenerator in result) {
                if (houseGenerator.Generator == null) {
                    items2Delete.Add(houseGenerator);
                }
            }
            foreach (var houseGenerator in items2Delete) {
                houseGenerator.DeleteFromDB();
                result.Remove(houseGenerator);
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("HouseID", "@HouseID", HouseID);
            if (_generator != null) {
                cmd.AddParameter("GeneratorID", _generator.IntID);
            }
        }

        public override string ToString() => Name;
    }
}