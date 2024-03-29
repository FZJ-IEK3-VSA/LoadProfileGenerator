﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class TravelRouteSetEntry : DBBase {
        public const string TableName = "tblTravelRouteSetEntry";

        [CanBeNull] private readonly TravelRoute _travelRoute;

        private readonly int _travelRouteSetID;
        public readonly int _minimumAge;
        public readonly int _maximumAge;
        public readonly PermittedGender _gender;
        public AffordanceTag _affordanceTag;
        public readonly int? _personID;
        public readonly double _weight;

    public TravelRouteSetEntry([CanBeNull] int? pID, int travelRouteSetID, [JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] string name,
            [CanBeNull] TravelRoute travelRoute, int minimumAge, int maximumAge, PermittedGender gender, AffordanceTag affordanceTag, int? personID, double weight, [NotNull] StrGuid guid)
            : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Travel Route Step";
            ID = pID;

            _travelRoute = travelRoute;
            _travelRouteSetID = travelRouteSetID;
            _minimumAge = minimumAge;
            _maximumAge = maximumAge;
            _gender = gender;
            _affordanceTag = affordanceTag;
            _personID = personID;
            _weight = weight;
        }

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public TravelRoute TravelRoute => _travelRoute ?? throw new InvalidOperationException();

        [UsedImplicitly]
        public int TravelRouteSetID => _travelRouteSetID;

        [UsedImplicitly]
        public int MinimumAge => _minimumAge;

        [UsedImplicitly]
        public int MaximumAge => _maximumAge;

        [UsedImplicitly]
        public PermittedGender Gender => _gender;

        [UsedImplicitly]
        public AffordanceTag AffordanceTag
        {
            get => _affordanceTag;
            set => SetValueWithNotify(value, ref _affordanceTag, true, nameof(AffordanceTag));
        }

        [UsedImplicitly]
        public int? PersonID => _personID;

        [UsedImplicitly]
        public double Weight => _weight;

    [JetBrains.Annotations.NotNull]
        private static TravelRouteSetEntry AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var setid = dr.GetIntFromLong("TravelRouteSetID");
            var routeID = dr.GetIntFromLong("TravelRouteID");
            var route = aic.TravelRoutes.FirstOrDefault(x => x.ID == routeID);
            var minimumAge = dr.GetIntFromLong("MinimumAge",false, ignoreMissingFields,-1);
            var maximumAge = dr.GetIntFromLong("MaximumAge", false, ignoreMissingFields, -1);
            var gender = (PermittedGender) dr.GetIntFromLong("Gender", false, ignoreMissingFields, (int) PermittedGender.All);
            var affordanceTagID = dr.GetIntFromLong("AffordanceTagID", false, ignoreMissingFields, -1);
            var affordanceTag = aic.AffordanceTags?.FirstOrDefault(x => x.IntID == affordanceTagID);
            var personID = dr.GetNullableIntFromLong("PersonID", false, ignoreMissingFields);
            var weight = dr.GetDouble("Weight", false, 1.0, ignoreMissingFields);
            //var name = dr.GetString("Name",false,"",ignoreMissingFields);
            const string name = "no name";
            var guid = GetGuid(dr, ignoreMissingFields);
            var step = new TravelRouteSetEntry(id, setid, connectionString, name, route, minimumAge, maximumAge, gender, affordanceTag, personID, weight, guid);
            return step;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_travelRoute == null) {
                message = "Travel Route not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TravelRouteSetEntry> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, AllItemCollections aic)
        {
            // The AllItemCollections object is passed through to transfer the AffordanceTag objects
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("TravelRouteSetID", _travelRouteSetID);
            //cmd.AddParameter("Name",Name);
            if (_travelRoute != null)
            {
                cmd.AddParameter("TravelRouteID", _travelRoute.IntID);
            }
            cmd.AddParameter("MinimumAge", _minimumAge);
            cmd.AddParameter("MaximumAge", _maximumAge);
            cmd.AddParameter("Gender", _gender);
            cmd.AddParameter("AffordanceTagID", _affordanceTag?.IntID ?? -1);
            cmd.AddParameter("PersonID", _personID ?? -1);
            cmd.AddParameter("Weight", _weight);
    }

        public override string ToString() => Name;

        public double CalculateTotalDistance() => TravelRoute.CalculateTotalDistance();
    }
}