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
using System.Globalization;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HHTDesire : DBBase, IJSonSubElement<HHTDesire.JsonDto> {
        public class JsonDto: IGuidObject
        {
            [Obsolete("Only json")]
            // ReSharper disable once NotNullMemberIsNotInitialized
            public JsonDto()
            {

            }

            public JsonDto(JsonReference desire, decimal decayTime, HealthStatus sicknessdesire, decimal threshold, decimal weight, int minAge, int maxAge, PermittedGender gender, StrGuid guid)
            {
                Desire = desire;
                DecayTime = decayTime;
                Sicknessdesire = sicknessdesire;
                Threshold = threshold;
                Weight = weight;
                MinAge = minAge;
                MaxAge = maxAge;
                Gender = gender;
                Guid = guid;
            }

            public JsonReference Desire { get; set; }
            public decimal DecayTime { get; set; }
            public HealthStatus Sicknessdesire { get; set; }
            public decimal Threshold { get; set; }
            public decimal Weight { get; set; }
            public int MinAge { get; set; }
            public int MaxAge { get; set; }
            public PermittedGender Gender { get; set; }
            public StrGuid Guid { get; set; }
        }
        public const string ParentIDFieldName = "HouseholdTraitID";
        public const string TableName = "tblHHTDesires";
        private  decimal _decayTime;

        [CanBeNull] private Desire _desire;
        [CanBeNull]
        private readonly int? _householdTraitID;

        static HHTDesire()
        {
                HealthStatusStrings = new ObservableCollection<string>
                {
                    "Healthy",
                    "Sick",
                    "Healthy or Sick"
                };
        }
        public HHTDesire([CanBeNull]int? pID, [CanBeNull] int? householdTraitID, decimal decayTime, [CanBeNull] Desire desire,
            HealthStatus sicknessdesire,
            decimal threshold, decimal weight, [NotNull] string connectionString, [NotNull] string name, int minAge, int maxAge,
            PermittedGender gender, StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            ID = pID;
            _decayTime = decayTime;
            _householdTraitID = householdTraitID;
            _desire = desire;
            SicknessDesire = sicknessdesire;
            Threshold = threshold;
            Weight = weight;
            MinAge = minAge;
            MaxAge = maxAge;
            Gender = gender;
            TypeDescription = "Household Trait Desire";
        }

        public decimal DecayTime => _decayTime;

        [NotNull]
        public Desire Desire => _desire ?? throw new InvalidOperationException();

        [NotNull]
        [UsedImplicitly]
        public string Estimated25PercentTime {
            get {
                var ts = TimeSpan.FromHours((double) _decayTime * 2);
                return ts.ToString();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string EstimatedExecutions => CalculateEstimatedExecutions();

        public PermittedGender Gender { get; private set; }

        [NotNull]
        public string HealthStatus => HealthStatusStrings[(int) SicknessDesire];

        [ItemNotNull]
        [NotNull]
        [IgnoreForJsonSync]
        public static ObservableCollection<string> HealthStatusStrings { get; private set; }
        [CanBeNull]
        public int? HouseholdTraitID => _householdTraitID;

        public int MaxAge { get; private set; }

        public int MinAge { get; private set; }

        [NotNull]
        [UsedImplicitly]
        public new string Name {
            get {
                if(_desire != null) {
                    return _desire.Name;
                }
                return "no name";
            }
        }

        [UsedImplicitly]
        public HealthStatus SicknessDesire { get; private set; }

        public decimal Threshold { get; private set; }

        public decimal Weight { get; private set; }

        [NotNull]
        private static HHTDesire AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var hhtDesireID =  dr.GetIntFromLong("ID");
            var householdTraitID = dr.GetIntFromLong("HouseholdTraitID");
            var decayTime = dr.GetDecimal("DecayTime", false);
            var threshold = dr.GetDecimal("Threshold", false);
            var weight = dr.GetDecimal("Weight", false);
            var sicknessdesire = (HealthStatus) dr.GetIntFromLong("SicknessDesire", false);
            var mininumAge = dr.GetIntFromLong("MinimumAge", false);
            var maxAge = dr.GetIntFromLong("MaximumAge", false);
            var gender = (PermittedGender) dr.GetIntFromLong("Gender", false);
            var desireID = dr.GetNullableIntFromLong("DesireID", false);
            Desire desire = null;
            var name = "(no name)";
            if (desireID != null) {
                desire = aic.Desires.FirstOrDefault(myDesire => myDesire.ID == desireID);
            }

            if (desire != null) {
                name = desire.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var hhad = new HHTDesire(hhtDesireID, householdTraitID, decayTime, desire, sicknessdesire, threshold,
                weight, connectionString, name, mininumAge, maxAge, gender, guid);
            return hhad;
        }

        [NotNull]
        private string CalculateEstimatedExecutions()
        {
            var ts = TimeSpan.FromHours((double) _decayTime * 2);
            if (ts.TotalHours < 24) {
                var daytimes = 24 / ts.TotalHours;
                return daytimes.ToString("N1", CultureInfo.CurrentCulture) + " per day";
            }
            if (ts.TotalDays < 7) {
                var weektimes = 7 / ts.TotalDays;
                return weektimes.ToString("N1", CultureInfo.CurrentCulture) + " per week";
            }
            if (ts.TotalDays < 30) {
                var monthtimes = 30 / ts.TotalDays;
                return monthtimes.ToString("N1", CultureInfo.CurrentCulture) + " per month";
            }
            var times = 365 / ts.TotalDays;
            return times.ToString("N1", CultureInfo.CurrentCulture) + " per year";
        }

        public static HealthStatus GetHealthStatusEnumForHealthStatusString([NotNull] string healthStatusStr)
        {
            for (var i = 0; i < HealthStatusStrings.Count; i++) {
                if (HealthStatusStrings[i] == healthStatusStr) {
                    return (HealthStatus)i;
                }
            }

            return 0;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_desire == null) {
                message = "Desire not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([NotNull] [ItemNotNull] ObservableCollection<HHTDesire> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<Desire> desires, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(desires: desires);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        [NotNull]
        public string MakeDescription()
        {
            if (_desire != null) {
                var desc = _desire.Name;
                desc += ", Weight: " + Weight.ToString(CultureInfo.CurrentCulture);
                desc += ", Threshold: " + Threshold.ToString(CultureInfo.CurrentCulture);
                desc += ", Decay: " + _decayTime.ToString(CultureInfo.CurrentCulture);
                return desc;
            }
            return "(no desire)";
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_desire != null) {
                cmd.AddParameter("DesireID", Desire.IntID);
            }
            cmd.AddParameter("DecayTime", DecayTime);
            cmd.AddParameter("SicknessDesire", SicknessDesire);
            cmd.AddParameter("Threshold", Threshold);
            cmd.AddParameter("Weight", Weight);
            if (_householdTraitID != null) {
                cmd.AddParameter("HouseholdTraitID", _householdTraitID);
            }
            cmd.AddParameter("MinimumAge", MinAge);
            cmd.AddParameter("MaximumAge", MaxAge);
            cmd.AddParameter("Gender", Gender);
        }

        public override string ToString() => Desire.Name;


        public JsonDto GetJson()
        {
            return new JsonDto(Desire.GetJsonReference(),
                DecayTime,
                SicknessDesire,Threshold,Weight,MinAge,MaxAge,
                Gender,Guid);
        }

        public void SynchronizeDataFromJson(JsonDto jto, Simulator sim)
        {
            var checkedProperties = new List<string>();
            ValidateAndUpdateValueAsNeeded(nameof(Desire), checkedProperties,
            Desire.Guid , jto.Desire.Guid,
                    ()=>_desire = sim.Desires.FindByGuid(jto.Desire.Guid) ??
                              throw new LPGException("Could not find a desire with the guid "
                                                     + jto.Desire.Guid + " and the name " + jto.Desire.Name));
            ValidateAndUpdateValueAsNeeded(nameof(DecayTime), checkedProperties,
            _decayTime , jto.DecayTime,()=>
                _decayTime = jto.DecayTime);
            ValidateAndUpdateValueAsNeeded(nameof(SicknessDesire),
                checkedProperties,
                SicknessDesire , jto.Sicknessdesire,
                () => SicknessDesire = jto.Sicknessdesire);
            ValidateAndUpdateValueAsNeeded(nameof(Threshold), checkedProperties,
            Threshold , jto.Threshold,()=>
                Threshold = jto.Threshold);
            ValidateAndUpdateValueAsNeeded(nameof(Weight), checkedProperties, Weight,
                jto.Weight, () => Weight = jto.Weight);
            ValidateAndUpdateValueAsNeeded(nameof(MinAge), checkedProperties,
            MinAge , jto.MinAge,()=>
                MinAge = jto.MinAge);
            ValidateAndUpdateValueAsNeeded(nameof(MaxAge), checkedProperties,
            MaxAge , jto.MaxAge,()=>
                MaxAge = jto.MaxAge);

            ValidateAndUpdateValueAsNeeded(nameof(Gender), checkedProperties,  Gender ,jto.Gender, () => Gender = jto.Gender);
                NeedsUpdate = true;
                ValidateAndUpdateValueAsNeeded(nameof(Guid), checkedProperties, Guid, jto.Guid, () => Guid = jto.Guid);
            CheckIfAllPropertiesWereCovered(checkedProperties, this);
            SaveToDB();
        }

        public StrGuid RelevantGuid => Guid;
    }
}