using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    public class HHTAffordance : DBBase, IJSonSubElement<HHTAffordance.JsonDto> {
        public const string ParentIDFieldName = "HouseholdTraitID";
        public const string TableName = "tblHHTAffordances";

        [CanBeNull] private  Affordance _affordance;

        [CanBeNull] private  HHTLocation _hhtLocation;

        private  int _householdTraitID;
        [CanBeNull]
        private  TimeLimit _timeLimit;

        private  int _weight;

        private  int _startMinusMinutes;
        private  int _startPlusMinutes;

        private  int _endMinusMinutes;
        private  int _endPlusMinutes;

        public HHTAffordance([CanBeNull]int? pID, [CanBeNull] Affordance affv, [CanBeNull] HHTLocation hhtLocation,
            int householdTraitID,
        [JetBrains.Annotations.NotNull]    string connectionString, [JetBrains.Annotations.NotNull] string hhaffName,[CanBeNull] TimeLimit timeLimit, int weight
            , int startMinusMinutes, int startPlusMinutes, int endMinusMinutes, int endPlusMinutes,
            [JetBrains.Annotations.NotNull] StrGuid guid) : base(hhaffName, TableName, connectionString, guid)
        {
            _timeLimit = timeLimit;
            _weight = weight;
            _startMinusMinutes = startMinusMinutes;
            _startPlusMinutes = startPlusMinutes;
            _endMinusMinutes = endMinusMinutes;
            _endPlusMinutes = endPlusMinutes;
            ID = pID;
            _affordance = affv;
            _hhtLocation = hhtLocation;
            _householdTraitID = householdTraitID;
            TypeDescription = "Household Trait Affordance Location";
        }

        [CanBeNull]
        public Affordance Affordance => _affordance;
        [CanBeNull]
        public HHTLocation HHTLocation => _hhtLocation;

        [CanBeNull]
        public TimeLimit TimeLimit => _timeLimit;

        public int Weight => _weight;

        [UsedImplicitly]
        public int HouseholdTraitID => _householdTraitID;

        public override string Name {
            get {
                string timelimitName = "no affordance";

                if(_affordance?.TimeLimit != null) {
                    timelimitName = _affordance.TimeLimit.Name;
                }
                if (_timeLimit != null) {
                    timelimitName = _timeLimit.Name;
                    if (_startMinusMinutes != 0 || _startPlusMinutes != 0 || _endMinusMinutes != 0 || _endPlusMinutes != 0) {
                        timelimitName += ",  start -" + _startMinusMinutes + " +" + _startPlusMinutes + ", end -" +_endMinusMinutes + " +" + _endPlusMinutes;
                    }
                }

                if (_hhtLocation?.Location != null && Affordance != null) {
                    return _hhtLocation.Location.Name + " - " + Affordance.Name + " (" + timelimitName + ", weight " +_weight+ ")" ;
                }
                if (_hhtLocation?.Location != null) {
                    return _hhtLocation.Location.Name + " - (no affordance, weight " + _weight + ")";
                }
                if (Affordance != null) {
                    return Affordance.Name + " (" + timelimitName + ", weight "  + _weight+ ")";
                }
                return "(no location) - (no device)";
            }
        }

        public int StartMinusMinutes => _startMinusMinutes;

        public int StartPlusMinutes => _startPlusMinutes;

        public int EndMinusMinutes => _endMinusMinutes;

        public int EndPlusMinutes => _endPlusMinutes;

        [JetBrains.Annotations.NotNull]
        private static HHTAffordance AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull]string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull]     AllItemCollections aic)
        {
            var hhdevlocID = dr.GetIntFromLong("ID");
            var householdTraitID = dr.GetIntFromLong("HouseholdTraitID");
            var affID = dr.GetIntFromLong("AffordanceID");
            var weight = dr.GetIntFromLong("Weight",false,ignoreMissingFields,100);
            var locationID = dr.GetIntFromLong("LocationID");
            var timeLimitID = dr.GetIntFromLong("TimeLimitID",false,ignoreMissingFields);
            var aff = aic.Affordances.FirstOrDefault(myaff => myaff.ID == affID);

            var startMinusMinutes = dr.GetIntFromLong("StartMinusMinutes", false, ignoreMissingFields);
            var startPlusMinutes = dr.GetIntFromLong("StartPlusMinutes", false, ignoreMissingFields);

            var endMinusMinutes = dr.GetIntFromLong("EndMinusMinutes", false, ignoreMissingFields);
            var endPlusMinutes = dr.GetIntFromLong("EndPlusMinutes", false, ignoreMissingFields);

            var hht =
                aic.HouseholdTraits.FirstOrDefault(myhouseholdtrait => myhouseholdtrait.ID == householdTraitID);
            HHTLocation hhtloc = null;
            if (hht != null) {
                hhtloc = hht.Locations.FirstOrDefault(myhhtl => myhhtl.Location.ID == locationID);
            }
            var affname = "(no name)";
            if (aff != null) {
                affname = aff.Name;
            }
            var hhlName = "(no name)";
            if (hhtloc != null) {
                hhlName = hhtloc.Name;
            }
            var householdName = affname + " - " + hhlName;
            TimeLimit timeLimit = aic.TimeLimits.FirstOrDefault(x => x.ID == timeLimitID);
            var guid = GetGuid(dr, ignoreMissingFields);

            var hhdl = new HHTAffordance(hhdevlocID, aff, hhtloc, householdTraitID, connectionString,
                householdName,timeLimit,weight, startMinusMinutes, startPlusMinutes, endMinusMinutes, endPlusMinutes, guid);
            hhtloc?.AffordanceLocations.Add(hhdl);
            return hhdl;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (Affordance == null) {
                message = "Affordance not found";
                return false;
            }
            if (_hhtLocation == null) {
                message = "Location not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HHTAffordance> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Affordance> affordances,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HouseholdTrait> allhouseholdTraits, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeLimit> timeLimits)
        {
            var aic = new AllItemCollections(affordances: affordances,
                householdTraits: allhouseholdTraits,timeLimits: timeLimits);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (Affordance != null) {
                cmd.AddParameter("AffordanceID", Affordance.IntID);
            }
            if (_hhtLocation?.Location != null) {
                cmd.AddParameter("LocationID", _hhtLocation.Location.IntID);
            }
            cmd.AddParameter("HouseholdTraitID", _householdTraitID);
            if(_timeLimit!=null) {
                cmd.AddParameter("TimeLimitID",_timeLimit.IntID);
            }
            cmd.AddParameter("StartMinusMinutes", _startMinusMinutes);
            cmd.AddParameter("StartPlusMinutes", _startPlusMinutes);
            cmd.AddParameter("EndMinusMinutes", _endMinusMinutes);
            cmd.AddParameter("EndPlusMinutes", _endPlusMinutes);
        }

        public JsonDto GetJson()
        {
            return new JsonDto(Affordance?.GetJsonReference(),
                TimeLimit?.GetJsonReference(),Weight,
                StartMinusMinutes,
                StartPlusMinutes,
                EndMinusMinutes,EndPlusMinutes,Guid);
        }

        public void SynchronizeDataFromJson(JsonDto json, Simulator sim)
        {
            List<string> checkedProperties = new List<string>();
           /*ValidateAndUpdateValueAsNeeded(nameof(Affordance),checkedProperties,
            () => Affordance?.Guid != json.Affordance.Guid,
            ()=> _affordance = sim.Affordances.FindByJsonReference(json.Affordance));
            ValidateAndUpdateValueAsNeeded(nameof(TimeLimit), checkedProperties,
                () => TimeLimit?.Guid != json.TimeLimit.Guid,
                () => _timeLimit = sim.TimeLimits.FindByJsonReference(json.TimeLimit));
                */
            CheckIfAllPropertiesWereCovered(checkedProperties, this);
            SaveToDB();
        }

        public override string ToString()
        {
            if (_affordance == null) {
                return "(no name)";
            }

            return _affordance.Name ;
        }

        public class JsonDto {
            public JsonDto(JsonReference affordance, JsonReference timeLimit, int weight, int startMinusMinutes, int startPlusMinutes, int endMinusMinutes, int endPlusMinutes, StrGuid guid)
            {
                Affordance = affordance;
                TimeLimit = timeLimit;
                Weight = weight;
                StartMinusMinutes = startMinusMinutes;
                StartPlusMinutes = startPlusMinutes;
                EndMinusMinutes = endMinusMinutes;
                EndPlusMinutes = endPlusMinutes;
                Guid = guid;
            }
            [Obsolete("Json only")]
            public JsonDto()
            {
            }

            public JsonReference Affordance { get; set; }

            public JsonReference TimeLimit { get; set; }
            public int Weight { get; set; }
            public int StartMinusMinutes { get; set; }
            public int StartPlusMinutes { get; set; }
            public int EndMinusMinutes { get; set; }
            public int EndPlusMinutes { get; set; }
            public  StrGuid Guid { get; set; }
        }

        public StrGuid RelevantGuid => Guid;
    }
}