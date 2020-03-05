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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HHTLocation : DBBase, IJSonSubElement<HHTLocation.JsonDto> {
        public const string ParentIDFieldName = "HouseholdTraitID";
        public const string TableName = "tblHHTLocations";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTAffordance> _affordanceLocations;
        [CanBeNull]
        private readonly int? _householdTraitID;

        [CanBeNull] private Location _location;

        public HHTLocation([CanBeNull]int? pID, [CanBeNull] Location ploc, [CanBeNull] int? householdTraitID, [NotNull] string name,
            [NotNull] string connectionString, [NotNull] string guid)
            : base(name, TableName, connectionString, guid) {
            TypeDescription = "Household Trait Location";
            ID = pID;
            _location = ploc;
            DistanceToAllOtherLocs = new Dictionary<Location, double>();
            _affordanceLocations = new ObservableCollection<HHTAffordance>();
            _householdTraitID = householdTraitID;
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HHTAffordance> AffordanceLocations => _affordanceLocations;

        [NotNull]
        [UsedImplicitly]
        public Dictionary<Location, double> DistanceToAllOtherLocs { get; }
        [CanBeNull]
        public int? HouseholdTraitID => _householdTraitID;

        [NotNull]
        public Location Location => _location ?? throw new InvalidOperationException();

        [NotNull]
        [UsedImplicitly]
        public HHTLocation MyLocation => this;

        [NotNull]
        public new string Name => ToString();

        [UsedImplicitly]
        [CanBeNull]
        [IgnoreForJsonSync]
        public HouseholdTrait ParentHouseholdTrait { get; set; }

        [NotNull]
        private static HHTLocation AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var hhlID = dr.GetIntFromLong("ID");
            var locationID = dr.GetIntFromLong("LocationID");
            var householdID = dr.GetNullableIntFromLong("HouseholdTraitID", false, ignoreMissingFields);

            var loc = aic.Locations.FirstOrDefault(myloc => myloc.ID == locationID);
            var name = "(no name)";
            if (loc != null) {
                name = loc.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var hhl = new HHTLocation(hhlID, loc, householdID, name, connectionString, guid);
            return hhl;
        }

        public override void DeleteFromDB() {
            DeleteByID(IntID, TableName, ConnectionString);
            foreach (var affordance in _affordanceLocations) {
                affordance.DeleteFromDB();
            }
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_location == null) {
                message = "Location is missing";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HHTLocation> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<Location> locations, [ItemNotNull] [NotNull] ObservableCollection<Affordance> affordances,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections(locations: locations, affordances: affordances);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new ObservableCollection<HHTLocation>();
            foreach (var hhloc in result) {
                if (hhloc.HouseholdTraitID == null) {
                    items2Delete.Add(hhloc);
                }
            }
            foreach (var hhLocation in items2Delete) {
                hhLocation.DeleteFromDB();
                result.Remove(hhLocation);
            }
        }

        public void Notify([NotNull] string name) {
            OnPropertyChanged(name);
        }

        public override void SaveToDB() {
            base.SaveToDB();
            foreach (var affordanceLocation in _affordanceLocations) {
                affordanceLocation.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_location != null) {
                cmd.AddParameter("LocationID", "@LocationID", _location.IntID);
            }
            if (_householdTraitID != null) {
                cmd.AddParameter("HouseholdTraitID", "@HouseholdTraitID", _householdTraitID);
            }
        }

        public JsonDto GetJson()
        {
            var affs = new List<HHTAffordance.JsonDto>();
            foreach (var affordance in AffordanceLocations) {
                affs.Add(affordance.GetJson());
            }
            return new JsonDto(affs,Location.GetJsonReference(),Guid);
        }

        public void SynchronizeDataFromJson(JsonDto json, Simulator sim)
        {
            var checkedProperties = new List<string>();
            ValidateAndUpdateValueAsNeeded(nameof(Location),
                checkedProperties,Location.Guid ,json.Location.Guid,()=>
                    _location = sim.Locations.FindByJsonReference(json.Location));
            ValidateAndUpdateValueAsNeeded(nameof(Guid),
                checkedProperties, Guid,json.Guid, () =>
                    Guid = json.Guid);
            CheckIfAllPropertiesWereCovered(checkedProperties,this);
        }

        public override string ToString() {
            if (_location == null) {
                return "(no name)";
            }
            return _location.Name;
        }

        public class JsonDto :IGuidObject {
            public JsonDto(List<HHTAffordance.JsonDto> affordances, JsonReference location, [NotNull] string guid)
            {
                Affordances = affordances;
                Location = location;
                Guid = guid;
            }
            [Obsolete("Json only")]
            [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
            public JsonDto()
            {
            }

            public List<HHTAffordance.JsonDto> Affordances { get; set; }
            public JsonReference Location { get; set; }
            public string Guid { get; set; }
        }

        public string RelevantGuid => Guid;
    }
}