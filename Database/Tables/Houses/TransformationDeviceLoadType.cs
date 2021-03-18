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
using Automation;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public enum TransformationFactorType {
        FixedFactor,
        Interpolated,
        FixedValue

    }

    public class TransformationDeviceLoadType : DBBase {
        public const string TableName = "tblTransformationDeviceLoadType";

        [CanBeNull] private readonly VLoadType _loadType;

        public TransformationDeviceLoadType([CanBeNull]int? pID, [CanBeNull] VLoadType loadType, double factor,
            int transformationDeviceID,
            [JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] string loadTypeName, TransformationFactorType factorType ,[NotNull] StrGuid guid)
            : base(loadTypeName, TableName, connectionString, guid)
        {
            ID = pID;
            _loadType = loadType;
            Factor = factor;
            TransformationDeviceID = transformationDeviceID;
            FactorType = factorType;
            TypeDescription = "Transformation Device Load Type";
        }

        [UsedImplicitly]
        public double Factor { get; }

        public TransformationFactorType FactorType { get; }
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string FactorTypeStr => FactorType.ToString();

        public override string Name {
            get {
                if (_loadType != null) {
                    return _loadType.Name;
                }
                return base.Name;
            }
        }

        public int TransformationDeviceID { get; }

        [UsedImplicitly]
        [CanBeNull]
        public VLoadType VLoadType => _loadType;

        [JetBrains.Annotations.NotNull]
        private static TransformationDeviceLoadType AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id =  dr.GetIntFromLong("ID");
            var loadTypeID = dr.GetIntFromLong("VLoadTypeID");
            var factor = dr.GetDouble("Factor");
            var transformationDeviceID = dr.GetIntFromLong("TransformationDeviceID");
            var vlt = aic.LoadTypes.FirstOrDefault(mylt => mylt.ID == loadTypeID);
            var factorType =
                (TransformationFactorType) dr.GetIntFromLong("FactorType", false, ignoreMissingFields);
            var loadTypeName = string.Empty;
            if (vlt != null) {
                loadTypeName = vlt.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var tdlt = new TransformationDeviceLoadType(id, vlt, factor, transformationDeviceID,
                connectionString, loadTypeName, factorType, guid);
            return tdlt;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_loadType == null) {
                message = "Load type not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TransformationDeviceLoadType> result,
            [JetBrains.Annotations.NotNull] string connectionString, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadTypes, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(loadTypes: loadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_loadType != null) {
                cmd.AddParameter("VLoadTypeID", _loadType.IntID);
            }
            cmd.AddParameter("Factor", Factor);
            cmd.AddParameter("TransformationDeviceID", TransformationDeviceID);
            cmd.AddParameter("FactorType", (int) FactorType);
        }

        public override string ToString() => Name;
    }
}