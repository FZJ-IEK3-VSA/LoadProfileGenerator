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
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class Generator : DBBaseElement {
        public const string TableName = "tblGenerators";

        [CanBeNull] private DateBasedProfile _dateBasedProfile;

        [CanBeNull] private string _description;

        [CanBeNull] private VLoadType _loadType;

        private double _scalingFactor;

        public Generator([NotNull] string pName, [CanBeNull] string description, [CanBeNull] VLoadType loadType, double scalingFactor,
            [CanBeNull] DateBasedProfile dateBasedProfile, [NotNull] string connectionString, StrGuid guid, [CanBeNull] int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            TypeDescription = "Generator";
            _description = description;
            _loadType = loadType;
            _scalingFactor = scalingFactor;
            _dateBasedProfile = dateBasedProfile;
        }

        [CanBeNull]
        [UsedImplicitly]
        public DateBasedProfile DateBasedProfile {
            get => _dateBasedProfile;
            set => SetValueWithNotify(value, ref _dateBasedProfile,false, nameof(DateBasedProfile));
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set {
                if (_description == value) {
                    return;
                }
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        [CanBeNull]
        public VLoadType LoadType {
            get => _loadType;
            set => SetValueWithNotify(value, ref _loadType,false, nameof(LoadType));
        }

        [UsedImplicitly]
        public double ScalingFactor {
            get => _scalingFactor;
            set => SetValueWithNotify(value, ref _scalingFactor, nameof(ScalingFactor));
        }

        [NotNull]
        private static Generator AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var name = (string) dr["Name"];
            if (name == null) {
                name = "no name";
            }
            var id = dr.GetIntFromLong("ID");
            var description = (string) dr["Description"];
            var vloadtypeID = dr.GetNullableIntFromLong("LoadtypeID", false);
            var dateBasedProfileID = dr.GetNullableIntFromLong("DateBasedProfileID", false);
            var scalingFactor = dr.GetDouble("ScalingFactor", false);
            VLoadType vlt = null;
            if (vloadtypeID != null) {
                vlt = aic.LoadTypes.FirstOrDefault(vlt1 => vlt1.ID == vloadtypeID);
            }
            DateBasedProfile dbp = null;
            if (dateBasedProfileID != null) {
                dbp = aic.DateBasedProfiles.FirstOrDefault(dbp1 => dbp1.ID == dateBasedProfileID);
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            return new Generator(name, description, vlt, scalingFactor, dbp, connectionString,guid, id);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
        {
            var generator = new Generator(FindNewName(isNameTaken, "New Generator "), "New generator description",
                null, 1, null, connectionString, System.Guid.NewGuid().ToStrGuid());
            return generator;
        }

        [NotNull]
        public double[] GetValues(DateTime startTime, DateTime endTime, TimeSpan stepSize)
        {
            if (_dateBasedProfile == null) {
                throw new LPGException("_datebased profile was null");
            }
            var values = _dateBasedProfile.GetValueArray(startTime, endTime, stepSize);
            for (var i = 0; i < values.Length; i++) {
                values[i] = values[i] * _scalingFactor;
            }
            return values;
        }

        [NotNull]
        [UsedImplicitly]
        public static Generator ImportFromItem([NotNull] Generator toImport, [NotNull] Simulator dstSim)
        {
            var vlt = GetItemFromListByName(dstSim.LoadTypes.MyItems, toImport.LoadType?.Name);
            var prof = GetItemFromListByName(dstSim.DateBasedProfiles.MyItems,
                toImport.DateBasedProfile?.Name);

            var gen = new Generator(toImport.Name, toImport.Description, vlt, toImport.ScalingFactor, prof,
                dstSim.ConnectionString, toImport.Guid);
            gen.SaveToDB();
            return gen;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<Generator> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes, [ItemNotNull] [NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(loadTypes: loadTypes, dateBasedProfiles: dateBasedProfiles);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            if(_description!=null) {
                cmd.AddParameter("Description", "@description", _description);
            }

            if (LoadType != null) {
                cmd.AddParameter("LoadTypeID", LoadType.IntID);
            }
            if (_dateBasedProfile != null) {
                cmd.AddParameter("DateBasedProfileID", _dateBasedProfile.IntID);
            }
            cmd.AddParameter("ScalingFactor", _scalingFactor);
        }

        public override string ToString() => Name;
        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((Generator)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}