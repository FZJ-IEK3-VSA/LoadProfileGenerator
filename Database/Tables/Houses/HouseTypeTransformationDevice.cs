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
    public class HouseTypeTransformationDevice : DBBase {
        public const string TableName = "tblHouseTypeTransformationDevice";

        [CanBeNull] private readonly TransformationDevice _transformationDevice;

        public HouseTypeTransformationDevice([CanBeNull]int? pID, int houseID, [CanBeNull] TransformationDevice td,
            [JetBrains.Annotations.NotNull] string connectionString,
            [JetBrains.Annotations.NotNull] string tdName, [NotNull] StrGuid guid) : base(tdName, TableName, connectionString, guid)
        {
            ID = pID;
            HouseID = houseID;
            _transformationDevice = td;
            TypeDescription = "House Type Transformation Device";
        }

        public int HouseID { get; }

        public override string Name {
            get {
                if (_transformationDevice != null) {
                    return _transformationDevice.Name;
                }
                return base.Name;
            }
        }

        [CanBeNull]
        public TransformationDevice TransformationDevice => _transformationDevice;

        [JetBrains.Annotations.NotNull]
        private static HouseTypeTransformationDevice AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var transformationDeviceID = dr.GetIntFromLong("TransformationDeviceID");

            var houseID = dr.GetIntFromLong("HouseID");

            var td =
                aic.TransformationDevices.FirstOrDefault(tradev => tradev.ID == transformationDeviceID);
            var tdName = string.Empty;
            if (td != null) {
                tdName = td.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            return new HouseTypeTransformationDevice(id, houseID, td, connectionString, tdName, guid);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_transformationDevice == null) {
                message = "Transformation device not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HouseTypeTransformationDevice> result,
            [JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<TransformationDevice> transformationDevices,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(transformationDevices: transformationDevices);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new List<HouseTypeTransformationDevice>();
            foreach (var houseTransformationDevice in result) {
                if (houseTransformationDevice._transformationDevice == null) {
                    items2Delete.Add(houseTransformationDevice);
                }
            }
            foreach (var device in items2Delete) {
                device.DeleteFromDB();
                result.Remove(device);
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("HouseID", "@HouseID", HouseID);
            if (_transformationDevice != null) {
                cmd.AddParameter("TransformationDeviceID", _transformationDevice.IntID);
            }
        }

        public override string ToString() => Name;
    }
}