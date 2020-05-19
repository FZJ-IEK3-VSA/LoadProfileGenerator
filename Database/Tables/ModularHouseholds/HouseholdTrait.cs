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
using System.Globalization;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public enum EstimateType {
        Theoretical,
        FromCalculations
    }

    public readonly struct AffordanceWithTimeLimit {
        public AffordanceWithTimeLimit([NotNull] Affordance affordance,
                                       [CanBeNull] TimeLimit timeLimit,
                                       int weight,
                                       int startMinusTime,
                                       int startPlusTime,
                                       int endMinusTime,
                                       int endPlusTime,
                                       [NotNull] string srcTraitName)
        {
            Affordance = affordance;
            TimeLimit = timeLimit;
            Weight = weight;
            StartMinusTime = startMinusTime;
            StartPlusTime = startPlusTime;
            EndMinusTime = endMinusTime;
            EndPlusTime = endPlusTime;
            SrcTraitName = srcTraitName;
        }

        [NotNull]
        public string SrcTraitName { get; }

        [NotNull]
        [UsedImplicitly]
        public Affordance Affordance { get; }

        [UsedImplicitly]
        [CanBeNull]
        public TimeLimit TimeLimit { get; }

        [UsedImplicitly]
        public int Weight { get; }

        [UsedImplicitly]
        public int StartMinusTime { get; }

        [UsedImplicitly]
        public int StartPlusTime { get; }

        [UsedImplicitly]
        public int EndMinusTime { get; }

        [UsedImplicitly]
        public int EndPlusTime { get; }

        public bool Equals(AffordanceWithTimeLimit other)
        {
            if (other.Affordance == Affordance && TimeLimit == other.TimeLimit) {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AffordanceWithTimeLimit)) {
                return false;
            }

            return Equals((AffordanceWithTimeLimit)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = -1402040524;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SrcTraitName);
            hashCode = hashCode * -1521134295 + EqualityComparer<Affordance>.Default.GetHashCode(Affordance);
            hashCode = hashCode * -1521134295 + EqualityComparer<TimeLimit>.Default.GetHashCode(TimeLimit);
            hashCode = hashCode * -1521134295 + Weight.GetHashCode();
            hashCode = hashCode * -1521134295 + StartMinusTime.GetHashCode();
            hashCode = hashCode * -1521134295 + StartPlusTime.GetHashCode();
            hashCode = hashCode * -1521134295 + EndMinusTime.GetHashCode();
            return hashCode * -1521134295 + EndPlusTime.GetHashCode();
        }

        public static bool operator ==(AffordanceWithTimeLimit left, AffordanceWithTimeLimit right) => left.Equals(right);

        public static bool operator !=(AffordanceWithTimeLimit left, AffordanceWithTimeLimit right) => !(left == right);
    }

    public class HouseholdTrait : DBBaseElement, IHouseholdOrTrait, IJsonSerializable<HouseholdTrait.JsonDto> {
        internal const string TableName = "tblHouseholdTraits";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTAutonomousDevice> _autodevs;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTDesire> _desires;
        [NotNull] [ItemNotNull] private readonly ObservableCollection<HHTLocation> _locations;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTTrait> _subTraits;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTTag> _tags;

        [CanBeNull] private string _cachedPrettyName;

        [NotNull] private string _classification;
        [NotNull] private string _description;
        private double _estimatedDuration2InMinutes;
        private double _estimatedTimeCount;
        private double _estimatedTimeCount2;
        private double _estimatedTimePerYearInH;
        private double _estimatedTimes;
        private double _estimatedTimes2;
        private TimeType _estimatedTimeType;
        private TimeType _estimatedTimeType2;
        private EstimateType _estimateType;

        private int _maximumNumberInCHH;
        private int _maximumPersonsInCHH;
        private int _minimumPersonsInCHH;
        [NotNull] private string _shortDescription;

        public HouseholdTrait([NotNull] string pName,
                              [CanBeNull] int? id,
                              [NotNull] string description,
                              [NotNull] string connectionString,
                              [NotNull] string classification,
                              int minimumPersonsInCHH,
                              int maximumPersonsInCHH,
                              int maximumNumberInCHH,
                              double estimatedTimes,
                              double estimatedTimeCount,
                              TimeType estimatedTimeType,
                              double estimatedTimes2,
                              double estimatedTimeCount2,
                              TimeType estimatedTimeType2,
                              double estimatedDuration2InMinutes,
                              double estimatedTimePerYearInH,
                              EstimateType estimateType,
                              [NotNull] string shortDescription,
                              StrGuid guid) : base(pName, TableName, connectionString, guid)
        {
            ID = id;
            _locations = new ObservableCollection<HHTLocation>();
            _autodevs = new ObservableCollection<HHTAutonomousDevice>();
            _desires = new ObservableCollection<HHTDesire>();
            _subTraits = new ObservableCollection<HHTTrait>();
            _tags = new ObservableCollection<HHTTag>();
            TypeDescription = "Household Trait";
            AreNumbersOkInNameForIntegrityCheck = true;
            _description = description;
            _classification = classification;
            _minimumPersonsInCHH = minimumPersonsInCHH;
            _maximumPersonsInCHH = maximumPersonsInCHH;
            _maximumNumberInCHH = maximumNumberInCHH;
            _estimatedTimes = estimatedTimes;
            _estimatedTimeType = estimatedTimeType;
            _estimatedTimeCount = estimatedTimeCount;
            _estimatedTimeCount2 = estimatedTimeCount2;
            _estimatedTimeType2 = estimatedTimeType2;
            _estimatedTimes2 = estimatedTimes2;
            _estimatedDuration2InMinutes = estimatedDuration2InMinutes;
            _estimatedTimePerYearInH = estimatedTimePerYearInH;
            _estimateType = estimateType;
            _shortDescription = shortDescription;
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HHTAutonomousDevice> Autodevs => _autodevs;

        [NotNull]
        [UsedImplicitly]
        public string Classification {
            get => _classification;
            set {
                SetValueWithNotify(value, ref _classification, nameof(Classification));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HHTDesire> Desires => _desires;

        [UsedImplicitly]
        public double EstimatedDuration2InMinutes {
            get => _estimatedDuration2InMinutes;
            set {
                SetValueWithNotify(value, ref _estimatedDuration2InMinutes, nameof(EstimatedDuration2InMinutes));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [UsedImplicitly]
        public double EstimatedTimeCount {
            get => _estimatedTimeCount;
            set {
                SetValueWithNotify(value, ref _estimatedTimeCount, nameof(EstimatedTimeCount));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [UsedImplicitly]
        public double EstimatedTimeCount2 {
            get => _estimatedTimeCount2;
            set {
                SetValueWithNotify(value, ref _estimatedTimeCount2, nameof(EstimatedTimeCount2));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [UsedImplicitly]
        public double EstimatedTimeInSeconds { get; set; }

        [UsedImplicitly]
        public double EstimatedTimePerYearInH {
            get => _estimatedTimePerYearInH;
            set {
                SetValueWithNotify(value, ref _estimatedTimePerYearInH, nameof(EstimatedTimePerYearInH));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [UsedImplicitly]
        public double EstimatedTimes {
            get => _estimatedTimes;
            set {
                SetValueWithNotify(value, ref _estimatedTimes, nameof(EstimatedTimes));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [UsedImplicitly]
        public double EstimatedTimes2 {
            get => _estimatedTimes2;
            set {
                SetValueWithNotify(value, ref _estimatedTimes2, nameof(EstimatedTimes2));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [UsedImplicitly]
        public TimeType EstimatedTimeType {
            get => _estimatedTimeType;
            set {
                SetValueWithNotify(value, ref _estimatedTimeType, nameof(EstimatedTimeType));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [UsedImplicitly]
        public TimeType EstimatedTimeType2 {
            get => _estimatedTimeType2;
            set {
                SetValueWithNotify(value, ref _estimatedTimeType2, nameof(EstimatedTimeType2));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [UsedImplicitly]
        public EstimateType EstimateType {
            get => _estimateType;
            set {
                SetValueWithNotify(value, ref _estimateType, nameof(EstimateType));
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<HHTLocation> Locations => _locations;

        [UsedImplicitly]
        public int MaximumNumberInCHH {
            get => _maximumNumberInCHH;
            set => SetValueWithNotify(value, ref _maximumNumberInCHH, nameof(MaximumNumberInCHH));
        }

        [UsedImplicitly]
        public int MaximumPersonsInCHH {
            get => _maximumPersonsInCHH;
            set => SetValueWithNotify(value, ref _maximumPersonsInCHH, nameof(MaximumPersonsInCHH));
        }

        [UsedImplicitly]
        public int MinimumPersonsInCHH {
            get => _minimumPersonsInCHH;
            set => SetValueWithNotify(value, ref _minimumPersonsInCHH, nameof(MinimumPersonsInCHH));
        }

        public int PermittedGender {
            get {
                var affs = CollectAffordances(true);
                var genders = affs.Select(x => x.PermittedGender).ToList();
                if (genders.Contains(Common.Enums.PermittedGender.All)) {
                    return -1;
                }

                var males = genders.Count(x => x == Common.Enums.PermittedGender.Male);
                var females = genders.Count(x => x == Common.Enums.PermittedGender.Female);
                if (males == 0 && females == 0) {
                    return -1;
                }

                if (males > 0 && females == 0) {
                    return 0;
                }

                if (females > 0 && males == 0) {
                    return 1;
                }

                return -1;
            }
        }

        public override string PrettyName {
            get {
                if (IsLoading && _cachedPrettyName != null) {
                    return _cachedPrettyName;
                }

                if (CollectAffordances(true).Count == 0) {
                    return "(" + _classification + ") " + Name;
                }

                if (Math.Abs(_estimatedTimeCount2) < 0.00001) {
                    var estimatedTime = TheoreticalTimeEstimateString();
                    _cachedPrettyName = "(" + _classification + ") " + Name + estimatedTime;
                    return _cachedPrettyName;
                }

                // time estimate v2: from calcs
                var duration = (_estimatedDuration2InMinutes / 60).ToString("N1", CultureInfo.CurrentCulture) + "h, ";
                var estimatedTime2 = " (" + duration + EstimatedTimes2 + "x/" + EstimatedTimeCount2 + " " + EstimatedTimeType2 + ")";
                if (Math.Abs(EstimatedTimeCount2 - 1) < Constants.Ebsilon) {
                    estimatedTime2 = " (" + duration + EstimatedTimes2 + "x/" + EstimatedTimeType2 + ")";
                }

                _cachedPrettyName = "(" + _classification + ") " + Name + estimatedTime2;
                return _cachedPrettyName;
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string PrettyNameOld {
            get {
                if (CollectAffordances(true).Count == 0) {
                    return "(" + _classification + ") " + Name;
                }

                var estimatedTime = TheoreticalTimeEstimateString();
                return "(" + _classification + ") " + Name + estimatedTime;
            }
        }

        // shortdescription is used for the person description creator that creates the template persons for the web interface
        [NotNull]
        [UsedImplicitly]
        public string ShortDescription {
            get => _shortDescription;
            set => SetValueWithNotify(value, ref _shortDescription, nameof(ShortDescription));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HHTTrait> SubTraits => _subTraits;

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<HHTTag> Tags => _tags;

        [NotNull]
        public string WebName {
            get {
                if (CollectAffordances(true).Count == 0) {
                    return Name;
                }

                var duration = CalculateAverageAffordanceDuration().TotalHours.ToString("N1", CultureInfo.CurrentCulture) + "h, ";
                var estimatedTime = " (" + duration + EstimatedTimes + "x/" + EstimatedTimeCount + " " + EstimatedTimeType + ")";
                if (Math.Abs(EstimatedTimeCount - 1) < Constants.Ebsilon) {
                    estimatedTime = " (" + duration + EstimatedTimes + "x/" + EstimatedTimeType + ")";
                }

                var webName = Name + estimatedTime;
                return webName;
            }
        }

        public List<Affordance> CollectAffordances(bool onlyRelevant)
        {
            var allAffordances = new List<Affordance>();
            foreach (var hhLocation in Locations) {
                var range = hhLocation.AffordanceLocations.Where(x => x.Affordance != null).Select(x => x.Affordance).ToList();
                foreach (var aff in range) {
                    if (aff == null) {
                        throw new LPGException("Found a bug in: " + LPGException.ErrorLocation());
                    }
                }

                allAffordances.AddRange(range);
            }

            allAffordances = allAffordances.Distinct().ToList();
            if (onlyRelevant) {
                var desires = new List<Desire>();
                foreach (var hhtDesire in _desires) {
                    desires.Add(hhtDesire.Desire);
                }

                var filtered = new List<Affordance>();
                foreach (var affordance in allAffordances) {
                    foreach (var desire in affordance.AffordanceDesires) {
                        if (desires.Contains(desire.Desire)) {
                            filtered.Add(affordance);
                            break;
                        }
                    }
                }

                allAffordances = filtered;
            }

            return allAffordances;
        }

        public List<IAssignableDevice> CollectStandbyDevices()
        {
            var allDevices = new List<IAssignableDevice>();
            foreach (var autodev in _autodevs) {
                allDevices.Add(autodev.Device);
            }

            return allDevices;
        }

        public override string Name {
            get => base.Name;
            set {
                base.Name = value;
                OnPropertyChanged(nameof(PrettyName));
            }
        }

        public JsonDto GetJson()
        {
            JsonDto jto = new JsonDto(Name,
                Description,
                Classification,
                EstimatedDuration2InMinutes,
                EstimatedTimeCount,
                EstimatedTimeCount2,
                EstimatedTimePerYearInH,
                EstimatedTimes,
                EstimatedTimes2,
                EstimatedTimeType,
                EstimatedTimeType2,
                EstimateType,
                MaximumNumberInCHH,
                MaximumPersonsInCHH,
                MinimumPersonsInCHH,
                ShortDescription,
                Guid,
                EstimatedTimeInSeconds);
            foreach (var autodev in Autodevs) {
                jto.AutonomousDevices.Add(autodev.GetJson());
            }

            foreach (var location in Locations) {
                jto.Locations.Add(location.GetJson());
            }

            foreach (var desire in Desires) {
                jto.Desires.Add(desire.GetJson());
            }

            foreach (var tag in Tags) {
                jto.Tags.Add(tag.Tag.GetJsonReference());
            }

            foreach (var subTrait in SubTraits) {
                jto.SubTraits.Add(subTrait.ThisTrait.GetJsonReference());
            }

            return jto;
        }

        [CanBeNull]
        public HHTTag AddTag([NotNull] TraitTag tag)
        {
            if (_tags.Any(x => x.Tag == tag)) {
                return null;
            }

            var hhttag = new HHTTag(null, IntID, tag, ConnectionString, tag.Name, System.Guid.NewGuid().ToStrGuid());
            _tags.Add(hhttag);
            _tags.Sort();
            hhttag.SaveToDB();
            return hhttag;
        }

        [CanBeNull]
        public HHTTag AddTagFromJto(JsonReference json, [NotNull] Simulator sim)
        {
            var tag = sim.TraitTags.FindByJsonReference(json) ?? throw new LPGException("Could not find trait tag " + json);
            return AddTag(tag);
        }

        public void CalculateEstimatedTimes()
        {
            if (CollectAffordances(true).Count == 0) {
                EstimatedTimes = 0;
                EstimatedTimeCount = 0;
                EstimatedTimeType = TimeType.Day;
                Logger.Error("The trait " + PrettyName + " has zero executable affordances! This probably should be fixed.");
                return;
            }

            var ts = TimeSpan.Zero;
            var count = 0;
            foreach (var desire in Desires) {
                ts = ts.Add(TimeSpan.FromHours((double)desire.DecayTime * 2));
                count++;
            }

            EstimatedTimeInSeconds = ts.TotalSeconds / count;
            ts = TimeSpan.FromSeconds(ts.TotalSeconds / count); // average time

            if (ts.TotalHours < 24) {
                EstimatedTimeType = TimeType.Day;
                EstimatedTimeCount = 1;
                EstimatedTimes = Math.Round(24 / ts.TotalHours, 1);
                return;
            }

            if (ts.TotalDays < 7) {
                EstimatedTimeType = TimeType.Week;
                EstimatedTimeCount = 1;
                EstimatedTimes = Math.Round(7 / ts.TotalDays, 1);
                return;
            }

            if (ts.TotalDays < 30) {
                EstimatedTimeType = TimeType.Month;
                EstimatedTimeCount = 1;
                EstimatedTimes = Math.Round(30 / ts.TotalDays, 1);
                return;
            }

            EstimatedTimeType = TimeType.Year;
            EstimatedTimeCount = 1;
            EstimatedTimes = Math.Round(365 / ts.TotalDays, 1);
        }

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var used = new List<UsedIn>();
            foreach (var chh in sim.ModularHouseholds.It) {
                foreach (var trait in chh.Traits) {
                    if (trait.HouseholdTrait == this) {
                        if (trait.AssignType == ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name && trait.DstPerson != null) {
                            used.Add(new UsedIn(chh, "Modular Household", trait.DstPerson.PrettyName));
                        }
                        else {
                            used.Add(new UsedIn(chh, "Modular Household"));
                        }
                    }
                }
            }

            return used;
        }

        [NotNull]
        public List<AffordanceWithTimeLimit> CollectAffordancesForLocation([NotNull] Location loc,
                                                                           [NotNull] string assignedTo,
                                                                           [CanBeNull] Person person)
        {
            var affordances = new List<AffordanceWithTimeLimit>();
            var hhtLocation = _locations.FirstOrDefault(x => x.Location == loc);
            if (hhtLocation != null) {
                foreach (var affloc in hhtLocation.AffordanceLocations) {
                    if (affloc.Affordance == null) {
                        throw new LPGException("Affordance was null");
                    }

                    bool reallyAdd = true;
                    if (person != null) {
                        reallyAdd = affloc.Affordance.IsValidPerson(person);
                    }

                    if (reallyAdd) {
                        AffordanceWithTimeLimit atl = new AffordanceWithTimeLimit(affloc.Affordance,
                            affloc.TimeLimit,
                            affloc.Weight,
                            affloc.StartMinusMinutes,
                            affloc.StartPlusMinutes,
                            affloc.EndMinusMinutes,
                            affloc.EndPlusMinutes,
                            PrettyName + " (assigned to " + assignedTo + ")");

                        affordances.Add(atl);
                    }
                }
            }

            foreach (var subTrait in SubTraits) {
                affordances.AddRange(subTrait.ThisTrait.CollectAffordancesForLocation(loc, assignedTo, person));
            }

            return affordances;
        }

        [ItemNotNull]
        [NotNull]
        public List<DeviceActionGroup> CollectDeviceActionGroups()
        {
            var dcs = new List<DeviceActionGroup>();
            foreach (var autonomousDevice in _autodevs) {
                if (autonomousDevice.Device?.AssignableDeviceType == AssignableDeviceType.DeviceActionGroup) {
                    dcs.Add((DeviceActionGroup)autonomousDevice.Device);
                }
            }

            foreach (var hhLocation in _locations) {
                dcs.AddRange(hhLocation.Location.CollectDeviceActionGroups());
                foreach (var hhaff in hhLocation.AffordanceLocations) {
                    if (hhaff.Affordance == null) {
                        throw new LPGException("Affordance was null");
                    }

                    foreach (var device in hhaff.Affordance.AffordanceDevices) {
                        if (device.Device?.AssignableDeviceType == AssignableDeviceType.DeviceActionGroup) {
                            dcs.Add((DeviceActionGroup)device.Device);
                        }
                    }
                }
            }

            var dcs2 = new List<DeviceActionGroup>();
            foreach (var group in dcs) {
                if (!dcs2.Contains(group)) {
                    dcs2.Add(group);
                }
            }

            return dcs2;
        }

        [ItemNotNull]
        [NotNull]
        public List<DeviceCategory> CollectDeviceCategories()
        {
            var dcs = new List<DeviceCategory>();

            foreach (var autonomousDevice in _autodevs) {
                dcs.Add(autonomousDevice.DeviceCategory);
            }

            foreach (var hhLocation in _locations) {
                dcs.AddRange(hhLocation.Location.CollectDeviceCategories());
            }

            var dcs2 = new List<DeviceCategory>();
            foreach (var deviceCategory in dcs) {
                if (!dcs2.Contains(deviceCategory)) {
                    dcs2.Add(deviceCategory);
                }
            }

            return dcs2;
        }

        [ItemNotNull]
        [NotNull]
        public List<Tuple<Location, IAssignableDevice>> CollectDevicesFromTrait()
        {
            var locdev = new List<Tuple<Location, IAssignableDevice>>();
            foreach (var location in _locations) {
                foreach (var affordance in location.AffordanceLocations) {
                    if (affordance.Affordance == null) {
                        throw new LPGException("Affordance was null");
                    }

                    if (affordance.HHTLocation == null) {
                        throw new LPGException("HHTLocation was null");
                    }

                    foreach (var affordanceDevice in affordance.Affordance.AffordanceDevices) {
                        var tup = new Tuple<Location, IAssignableDevice>(affordance.HHTLocation.Location, affordanceDevice.Device);
                        if (!locdev.Contains(tup)) {
                            locdev.Add(tup);
                        }
                    }
                }
            }

            foreach (var trait in _subTraits) {
                locdev.AddRange(trait.ThisTrait.CollectDevicesFromTrait());
            }

            return locdev;
        }
        /*
        public List<HouseholdTrait> CollectRecursiveSubtraits()
        {
            var traits = new List<HouseholdTrait>();
            traits.Add(this);
            foreach (var subTrait in SubTraits) {
                traits.AddRange(subTrait.ThisTrait.CollectRecursiveSubtraits());
            }
            return traits;
        }*/

        public override int CompareTo(BasicElement other)
        {
            if (other is HouseholdTrait trait) {
                return string.Compare(PrettyName, trait.PrettyName, StringComparison.Ordinal);
            }

            return base.CompareTo(other);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new HouseholdTrait(
            FindNewName(isNameTaken, "New Household Trait "),
            null,
            "(no description yet)",
            connectionString,
            "unknown",
            1,
            10,
            10,
            1,
            1,
            TimeType.Week,
            1,
            1,
            TimeType.Week,
            1,
            0,
            EstimateType.Theoretical,
            "",
            System.Guid.NewGuid().ToStrGuid());

        public void DeleteAffordanceFromDB([NotNull] HHTAffordance hhaff)
        {
            hhaff.DeleteFromDB();
            var hhl = _locations.First(x => x == hhaff.HHTLocation);
            Logger.Get().SafeExecuteWithWait(() => hhl.AffordanceLocations.Remove(hhaff));
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public override void DeleteFromDB()
        {
            if (ID != null) {
                DeleteAllForOneParent(IntID, HHTLocation.ParentIDFieldName, HHTLocation.TableName, ConnectionString);
                DeleteAllForOneParent(IntID, HHTAutonomousDevice.ParentIDFieldName, HHTAutonomousDevice.TableName, ConnectionString);
                DeleteAllForOneParent(IntID, HHTDesire.ParentIDFieldName, HHTDesire.TableName, ConnectionString);
                DeleteAllForOneParent(IntID, HHTTrait.ParentIDFieldName, HHTTrait.TableName, ConnectionString);
                DeleteAllForOneParent(IntID, HHTTag.ParentIDFieldName, HHTTag.TableName, ConnectionString);
                DeleteAllForOneParent(IntID, HHTAffordance.ParentIDFieldName, HHTAffordance.TableName, ConnectionString);
                DeleteByID((int)ID, TableName, ConnectionString);
            }
        }
        /*
        public double EstimateDuration()
        {
            double totalSeconds = 0;
            var count = 0;
            foreach (var location in _locations) {
                foreach (var affloc in location.AffordanceLocations) {
                    totalSeconds += affloc.Affordance.PersonProfile.Duration.TotalSeconds;
                    count++;
                }
            }
            return totalSeconds / count;
        }*/

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public IEnumerable<IAutonomousDevice> GetAllAutodevs(int count = 0)
        {
            if (count > 10) {
                throw new DataIntegrityException(
                    "The stack of household traits is more than 10 levels deep. This most likely means that a loop was created where Trait A contains B and B contains A. The current trait is " +
                    Name + ". Please fix.");
            }

            var autodevs = new List<IAutonomousDevice>();
            foreach (var subTrait in SubTraits) {
                autodevs.AddRange(subTrait.ThisTrait.GetAllAutodevs(count + 1));
            }

            autodevs.AddRange(_autodevs);
            return autodevs;
        }

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public List<HHTDesire> GetAllDesires(int count = 0)
        {
            if (count > 10) {
                throw new DataIntegrityException(
                    "The stack of household traits is more than 10 levels deep. This most likely means that a loop was created where Trait A contains B and B contains A. The current trait is " +
                    Name + ". Please fix.");
            }

            var desires = new List<HHTDesire>();
            foreach (var subTrait in SubTraits) {
                desires.AddRange(subTrait.ThisTrait.GetAllDesires(count + 1));
            }

            desires.AddRange(_desires);
            return desires;
        }

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public IEnumerable<HHTLocation> GetAllLocations(int count = 0)
        {
            if (count > 10) {
                throw new DataIntegrityException(
                    "The stack of household traits is more than 10 levels deep. This most likely means that a loop was created where Trait A contains B and B contains A. The current trait is " +
                    Name + ". Please fix.");
            }

            var locations = new List<HHTLocation>();
            foreach (var subTrait in SubTraits) {
                locations.AddRange(subTrait.ThisTrait.GetAllLocations(count + 1));
            }

            locations.AddRange(_locations);
            return locations;
        }

        public int GetExecuteableAffordanes([NotNull] Person p)
        {
            var affs = CollectAffordances(true);
            if (affs.Count == 0) // no need to check traits without any affordances.
            {
                return 0;
            }

            var count = 0;
            foreach (var aff in affs) {
                if (aff.IsValidPerson(p)) {
                    count++;
                }
            }

            return count;
        }

        [ItemNotNull]
        [NotNull]
        public List<VLoadType> GetLoadTypes([ItemNotNull] [NotNull] ObservableCollection<Affordance> affordances)
        {
            var loadTypes = new Dictionary<VLoadType, bool>();

            foreach (var affordance in affordances) {
                foreach (var affdev in affordance.AffordanceDevices) {
                    if (affdev.LoadType == null) {
                        throw new LPGException("Loadtype was null");
                    }

                    if (!loadTypes.ContainsKey(affdev.LoadType)) {
                        loadTypes.Add(affdev.LoadType, true);
                    }
                }
            }

            foreach (var autodev in _autodevs) {
                if (autodev.LoadType == null) {
                    throw new LPGException("Loadtype was null");
                }

                if (!loadTypes.ContainsKey(autodev.LoadType)) {
                    loadTypes.Add(autodev.LoadType, true);
                }
            }

            var result = new List<VLoadType>();
            result.AddRange(loadTypes.Keys);
            return result;
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim) => ImportFromItem((HouseholdTrait)toImport, dstSim);

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] HouseholdTrait item, [NotNull] Simulator dstSim)
        {
            var hh = new HouseholdTrait(item.Name,
                null,
                item.Description,
                dstSim.ConnectionString,
                item._classification,
                item.MinimumPersonsInCHH,
                item.MaximumPersonsInCHH,
                item.MaximumNumberInCHH,
                item.EstimatedTimes,
                item.EstimatedTimeCount,
                item.EstimatedTimeType,
                item.EstimatedTimes2,
                item.EstimatedTimeCount2,
                item.EstimatedTimeType2,
                item.EstimatedDuration2InMinutes,
                item.EstimatedTimePerYearInH,
                item.EstimateType,
                item.ShortDescription,
                item.Guid);
            hh.SaveToDB();
            foreach (var autodev in item.Autodevs) {
                var iad = GetAssignableDeviceFromListByName(dstSim.RealDevices.MyItems,
                    dstSim.DeviceCategories.MyItems,
                    dstSim.DeviceActions.It,
                    dstSim.DeviceActionGroups.It,
                    autodev.Device);
                TimeBasedProfile tbp = null;
                if (autodev.TimeProfile != null) {
                    tbp = GetItemFromListByName(dstSim.Timeprofiles.MyItems, autodev.TimeProfile.Name);
                }

                VLoadType vlt = null;
                if (autodev.LoadType != null) {
                    vlt = GetItemFromListByName(dstSim.LoadTypes.MyItems, autodev.LoadType.Name);
                }

                TimeLimit dt = null;
                if (autodev.TimeLimit != null) {
                    dt = GetItemFromListByName(dstSim.TimeLimits.MyItems, autodev.TimeLimit.Name);
                }

                Location loc = null;
                if (autodev.Location != null) {
                    loc = GetItemFromListByName(dstSim.Locations.MyItems, autodev.Location.Name);
                }

                Variable variable = null;
                if (autodev.Variable != null) {
                    variable = GetItemFromListByName(dstSim.Variables.It, autodev.Variable.Name);
                }

                if (iad != null) {
                    hh.AddAutomousDevice(iad,
                        tbp,
                        autodev.TimeStandardDeviation,
                        vlt,
                        dt,
                        loc,
                        autodev.VariableValue,
                        autodev.VariableCondition,
                        variable);
                }
            }

            foreach (var hhLocation in item.Locations) {
                var l = GetItemFromListByName(dstSim.Locations.MyItems, hhLocation.Location.Name);
                if (l != null) {
                    var hhl = hh.AddLocation(l);

                    foreach (var affloc in hhLocation.AffordanceLocations) {
                        var aff = GetItemFromListByName(dstSim.Affordances.It, affloc.Affordance?.Name);
                        var timeLimit = GetItemFromListByName(dstSim.TimeLimits.It, affloc.TimeLimit?.Name);
                        if (aff != null) {
                            hh.AddAffordanceToLocation(hhl,
                                aff,
                                timeLimit,
                                affloc.Weight,
                                affloc.StartMinusMinutes,
                                affloc.StartPlusMinutes,
                                affloc.EndMinusMinutes,
                                affloc.EndPlusMinutes);
                        }
                    }
                }
            }

            foreach (var hhtDesire in item.Desires) {
                var newDesire = GetItemFromListByName(dstSim.Desires.MyItems, hhtDesire.Desire.Name);
                if (newDesire != null) {
                    hh.AddDesire(newDesire,
                        hhtDesire.DecayTime,
                        hhtDesire.HealthStatus,
                        hhtDesire.Threshold,
                        hhtDesire.Weight,
                        hhtDesire.MinAge,
                        hhtDesire.MaxAge,
                        hhtDesire.Gender);
                }
            }

            foreach (var hhttag in item.Tags) {
                var tag = GetItemFromListByName(dstSim.TraitTags.MyItems, hhttag.Tag.Name);
                if (tag != null) {
                    hh.AddTag(tag);
                }
            }

            foreach (var subtrait in item.SubTraits) {
                var trait = GetItemFromListByName(dstSim.HouseholdTraits.MyItems, subtrait.ThisTrait.Name);
                if (trait != null) {
                    hh.AddTrait(trait);
                }
                else {
                    Logger.Warning("Import failed for the sub-trait " + subtrait.ThisTrait.Name + ". Please add this subtrait manually.");
                }
            }

            hh.SaveToDB();
            return hh;
        }

        public void ImportFromJsonObject([NotNull] JsonDto json, [NotNull] Simulator sim)
        {
            var checkedProperties = new List<string>();
            ValidateAndUpdateValueAsNeeded(nameof(Name), checkedProperties, Name, json.Name, () => Name = json.Name ?? "No name");
            ValidateAndUpdateValueAsNeeded(nameof(Description),
                checkedProperties,
                Description,
                json.Description,
                () => Description = json.Description);

            ValidateAndUpdateValueAsNeeded(nameof(Classification),
                checkedProperties,
                Classification,
                json.Classification,
                () => Classification = json.Classification);
            ValidateAndUpdateValueAsNeeded(nameof(EstimatedDuration2InMinutes),
                checkedProperties,
                EstimatedDuration2InMinutes,
                json.EstimatedDuration2InMinutes,
                () => EstimatedDuration2InMinutes = json.EstimatedDuration2InMinutes);
            ValidateAndUpdateValueAsNeeded(nameof(EstimatedTimeCount),
                checkedProperties,
                EstimatedTimeCount,
                json.EstimatedTimeCount,
                () => EstimatedTimeCount = json.EstimatedTimeCount);
            ValidateAndUpdateValueAsNeeded(nameof(EstimatedTimeCount2),
                checkedProperties,
                EstimatedTimeCount2,
                json.EstimatedTimeCount2,
                () => EstimatedTimeCount2 = json.EstimatedTimeCount2);
            ValidateAndUpdateValueAsNeeded(nameof(EstimatedTimeInSeconds),
                checkedProperties,
                EstimatedTimeInSeconds,
                json.EstimatedTimeInSeconds,
                () => EstimatedTimeInSeconds = json.EstimatedTimeInSeconds);
            ValidateAndUpdateValueAsNeeded(nameof(EstimatedTimePerYearInH),
                checkedProperties,
                EstimatedTimePerYearInH,
                json.EstimatedTimePerYearInH,
                () => EstimatedTimePerYearInH = json.EstimatedTimePerYearInH);
            ValidateAndUpdateValueAsNeeded(nameof(EstimatedTimes),
                checkedProperties,
                EstimatedTimes,
                json.EstimatedTimes,
                () => EstimatedTimes = json.EstimatedTimes);
            ValidateAndUpdateValueAsNeeded(nameof(EstimatedTimes2),
                checkedProperties,
                EstimatedTimes2,
                json.EstimatedTimes2,
                () => EstimatedTimes2 = json.EstimatedTimes2);
            ValidateAndUpdateValueAsNeeded(nameof(EstimatedTimeType),
                checkedProperties,
                EstimatedTimeType,
                json.EstimatedTimeType,
                () => EstimatedTimeType = json.EstimatedTimeType);
            ValidateAndUpdateValueAsNeeded(nameof(EstimatedTimeType2),
                checkedProperties,
                EstimatedTimeType2,
                json.EstimatedTimeType2,
                () => EstimatedTimeType2 = json.EstimatedTimeType2);
            ValidateAndUpdateValueAsNeeded(nameof(EstimateType),
                checkedProperties,
                EstimateType,
                json.EstimateType,
                () => EstimateType = json.EstimateType);
            ValidateAndUpdateValueAsNeeded(nameof(MaximumNumberInCHH),
                checkedProperties,
                MaximumNumberInCHH,
                json.MaximumNumberInCHH,
                () => MaximumNumberInCHH = json.MaximumNumberInCHH);
            ValidateAndUpdateValueAsNeeded(nameof(MaximumPersonsInCHH),
                checkedProperties,
                MaximumPersonsInCHH,
                json.MaximumPersonsInCHH,
                () => MaximumPersonsInCHH = json.MaximumPersonsInCHH);
            ValidateAndUpdateValueAsNeeded(nameof(MinimumPersonsInCHH),
                checkedProperties,
                MinimumPersonsInCHH,
                json.MinimumPersonsInCHH,
                () => MinimumPersonsInCHH = json.MinimumPersonsInCHH);
            ValidateAndUpdateValueAsNeeded(nameof(ShortDescription),
                checkedProperties,
                ShortDescription,
                json.ShortDescription,
                () => ShortDescription = json.ShortDescription);
            ValidateAndUpdateValueAsNeeded(nameof(Guid), checkedProperties, Guid, json.Guid, () => Guid = json.Guid);
            if (NeedsUpdate) {
                Logger.Info("Adjusting values based on Json-Data for " + json.Name);
            }

            CheckIfAllPropertiesWereCovered(checkedProperties, this);
            //lists
            SynchronizeListWithCreation(Autodevs, json.AutonomousDevices, AddAutonomousDeviceFromJto, sim);
            SynchronizeListWithCreation(Locations, json.Locations, AddLocationFromJto, sim);
            SynchronizeListWithCreation(Desires, json.Desires, AddDesireFromJto, sim);
            SynchronizeListWithCreation(Tags, json.Tags, AddTagFromJto, sim);
            SaveToDB();
        }

        public void ImportHouseholdTrait([NotNull] HouseholdTrait selectedImportHousehold)
        {
            Classification = selectedImportHousehold._classification;
            Description = selectedImportHousehold.Description;
            EstimatedTimeCount = selectedImportHousehold.EstimatedTimeCount;
            EstimatedTimeType = selectedImportHousehold.EstimatedTimeType;
            EstimatedTimes = selectedImportHousehold.EstimatedTimes;
            MaximumNumberInCHH = selectedImportHousehold.MaximumNumberInCHH;
            MaximumPersonsInCHH = selectedImportHousehold.MaximumPersonsInCHH;
            MinimumPersonsInCHH = selectedImportHousehold.MinimumPersonsInCHH;
            ShortDescription = selectedImportHousehold.ShortDescription;

            foreach (var hhAutonomousDevice in selectedImportHousehold._autodevs) {
                if (hhAutonomousDevice.Device != null) {
                    AddAutomousDevice(hhAutonomousDevice.Device,
                        hhAutonomousDevice.TimeProfile,
                        hhAutonomousDevice.TimeStandardDeviation,
                        hhAutonomousDevice.LoadType,
                        hhAutonomousDevice.TimeLimit,
                        hhAutonomousDevice.Location,
                        hhAutonomousDevice.VariableValue,
                        hhAutonomousDevice.VariableCondition,
                        hhAutonomousDevice.Variable);
                }
            }

            foreach (var hhLocation in selectedImportHousehold._locations) {
                var newLocation = AddLocation(hhLocation.Location);
                foreach (var affordance in hhLocation.AffordanceLocations) {
                    if (affordance.Affordance == null) {
                        throw new LPGException("Affordance was null");
                    }

                    AddAffordanceToLocation(newLocation,
                        affordance.Affordance,
                        affordance.TimeLimit,
                        affordance.Weight,
                        affordance.StartMinusMinutes,
                        affordance.StartPlusMinutes,
                        affordance.EndMinusMinutes,
                        affordance.EndPlusMinutes);
                }
            }

            foreach (var tag in selectedImportHousehold._tags) {
                AddTag(tag.Tag);
            }

            foreach (var hhtDesire in selectedImportHousehold.Desires) {
                AddDesire(hhtDesire.Desire,
                    hhtDesire.DecayTime,
                    hhtDesire.HealthStatus,
                    hhtDesire.Threshold,
                    hhtDesire.Weight,
                    hhtDesire.MinAge,
                    hhtDesire.MaxAge,
                    hhtDesire.Gender);
            }

            foreach (var subTrait in selectedImportHousehold.SubTraits) {
                AddTrait(subTrait.ThisTrait);
            }
        }

        [UsedImplicitly]
        [NotNull]
        public static List<HouseholdTrait> ImportObjectFromJson([NotNull] Simulator sim, [NotNull] [ItemNotNull] List<JsonDto> jsonTraits)
        {
            List<HouseholdTrait> newTraits = new List<HouseholdTrait>();
            foreach (var jsonTrait in jsonTraits) {
                var trait = sim.HouseholdTraits.FindByGuid(jsonTrait.Guid);
                if (trait == null) {
                    Logger.Info(jsonTrait.Name + " not found, creating...");
                    trait = sim.HouseholdTraits.CreateNewItem(sim.ConnectionString);
                    newTraits.Add(trait);
                }

                trait.ImportFromJsonObject(jsonTrait, sim);
            }

            sim.HouseholdTraits.It.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            return newTraits;
        }

        public bool IsValidForHousehold([NotNull] ModularHousehold chh)
        {
            var persons = chh.Persons.Count;
            if (persons < MinimumPersonsInCHH || persons > MaximumPersonsInCHH) {
                return false;
            }

            return true;
        }

        public bool IsValidForPerson([CanBeNull] Person p)
        {
            if (p == null) {
                return false;
            }

            var affs = CollectAffordances(true);
            if (affs.Count == 0) // no need to check traits without any affordances.
            {
                return true;
            }

            foreach (var aff in affs) {
                if (aff.IsValidPerson(p)) {
                    return true;
                }
            }

            return false;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HouseholdTrait> result,
                                            [NotNull] string connectionString,
                                            [ItemNotNull] [NotNull] ObservableCollection<Location> allLocations,
                                            [ItemNotNull] [NotNull] ObservableCollection<Affordance> allAffordances,
                                            [ItemNotNull] [NotNull] ObservableCollection<RealDevice> allDevices,
                                            [ItemNotNull] [NotNull] ObservableCollection<DeviceCategory> allDeviceCategories,
                                            [ItemNotNull] [NotNull] ObservableCollection<TimeBasedProfile> allTimeBasedProfiles,
                                            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes,
                                            [ItemNotNull] [NotNull] ObservableCollection<TimeLimit> timeLimits,
                                            [ItemNotNull] [NotNull] ObservableCollection<Desire> allDesires,
                                            [ItemNotNull] [NotNull] ObservableCollection<DeviceAction> deviceActions,
                                            [ItemNotNull] [NotNull] ObservableCollection<DeviceActionGroup> groups,
                                            [ItemNotNull] [NotNull] ObservableCollection<TraitTag> traittags,
                                            bool ignoreMissingTables,
                                            [ItemNotNull] [NotNull] ObservableCollection<Variable> variables)
        {
            var aic = new AllItemCollections(realDevices: allDevices,
                deviceCategories: allDeviceCategories,
                loadTypes: loadTypes,
                timeLimits: timeLimits,
                timeProfiles: allTimeBasedProfiles,
                affordances: allAffordances,
                locations: allLocations,
                desires: allDesires);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            // hhlocations
            var hhtLocations = new ObservableCollection<HHTLocation>();
            HHTLocation.LoadFromDatabase(hhtLocations, connectionString, allLocations, allAffordances, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hhtLocations), IsCorrectHHTLocationParent, ignoreMissingTables);

            // hhautonomous devices
            var hhAutonomousDevices = new ObservableCollection<HHTAutonomousDevice>();

            HHTAutonomousDevice.LoadFromDatabase(hhAutonomousDevices,
                connectionString,
                allTimeBasedProfiles,
                allDevices,
                allDeviceCategories,
                loadTypes,
                timeLimits,
                ignoreMissingTables,
                allLocations,
                deviceActions,
                groups,
                variables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hhAutonomousDevices), IsCorrectHHAutonomousParent, ignoreMissingTables);

            // hhtdesires devices
            var hhtDesires = new ObservableCollection<HHTDesire>();
            HHTDesire.LoadFromDatabase(hhtDesires, connectionString, allDesires, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hhtDesires), IsCorrectHHTDesireParent, ignoreMissingTables);

            // hhAffordance
            var hhAffordances = new ObservableCollection<HHTAffordance>();
            HHTAffordance.LoadFromDatabase(hhAffordances, connectionString, ignoreMissingTables, allAffordances, result, timeLimits);
            // no setsubitems, because that's already done in the loadfromdatabase

            // hhtTraits
            var hhtTraits = new ObservableCollection<HHTTrait>();
            HHTTrait.LoadFromDatabase(hhtTraits, connectionString, result, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hhtTraits), IsCorrectHHTTraitParent, ignoreMissingTables);

            // hhtTags
            var hhttags = new ObservableCollection<HHTTag>();
            HHTTag.LoadFromDatabase(hhttags, connectionString, ignoreMissingTables, traittags);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hhttags), IsCorrectHHTTagParent, ignoreMissingTables);

            // sort
            foreach (var hh in result) {
                hh._locations.Sort();
                hh._autodevs.Sort();

                foreach (var hhLocation in hh.Locations) {
                    hhLocation.AffordanceLocations.Sort();
                }
            }

            // cleanup
            result.Sort();
        }

        [NotNull]
        public HouseholdTrait MakeCopy([NotNull] Simulator sim)
        {
            var newTrait = sim.HouseholdTraits.CreateNewItem(sim.ConnectionString);
            var s = Name;
            var name = Name + " (Copy)";
            var lastspace = s.LastIndexOf(" ", StringComparison.Ordinal);
            if (lastspace > 0) {
                var number = s.Substring(lastspace);
                var success = int.TryParse(number, out int output);
                if (success) {
                    var newname = name.Substring(0, lastspace);
                    output++;
                    while (sim.HouseholdTraits.IsNameTaken(newname + " " + output)) {
                        output++;
                    }

                    name = newname + " " + output;
                }
            }

            newTrait.Name = name;
            newTrait.ImportHouseholdTrait(this);
            return newTrait;
        }

        public void RemoveDesire([NotNull] HHTDesire desire)
        {
            _desires.Remove(desire);
            desire.DeleteFromDB();
        }

        public override void SaveToDB()
        {
            base.SaveToDB();

            foreach (var hhl in _locations) {
                hhl.SaveToDB();
            }

            foreach (var hhAutonomous in Autodevs) {
                hhAutonomous.SaveToDB();
            }

            foreach (var trait in _subTraits) {
                trait.SaveToDB();
            }

            foreach (var tag in _tags) {
                tag.SaveToDB();
            }

            foreach (HHTDesire desire in _desires) {
                desire.SaveToDB();
            }
        }

        [NotNull]
        public string TheoreticalTimeEstimateString()
        {
            var duration = CalculateAverageAffordanceDuration().TotalHours.ToString("N1", CultureInfo.CurrentCulture) + "h, ";
            var estimatedTime = " (" + duration + EstimatedTimes + "x/" + EstimatedTimeCount + " " + EstimatedTimeType + ")";
            if (Math.Abs(EstimatedTimeCount - 1) < Constants.Ebsilon) {
                estimatedTime = " (" + duration + EstimatedTimes + "x/" + EstimatedTimeType + ")";
            }

            return estimatedTime;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", _description);
            cmd.AddParameter("Classification", _classification);
            cmd.AddParameter("MinimumPersonCount", _minimumPersonsInCHH);
            cmd.AddParameter("MaximumPersonCount", _maximumPersonsInCHH);
            cmd.AddParameter("MaximumNumberCount", _maximumNumberInCHH);
            cmd.AddParameter("EstimatedTimeCount", _estimatedTimeCount);
            cmd.AddParameter("estimatedTimeType", (int)_estimatedTimeType);
            cmd.AddParameter("EstimatedTimes", _estimatedTimes);
            cmd.AddParameter("EstimatedTimeCount2", _estimatedTimeCount2);
            cmd.AddParameter("estimatedTimeType2", (int)_estimatedTimeType2);
            cmd.AddParameter("EstimatedTimes2", _estimatedTimes2);
            cmd.AddParameter("EstimatedDuration2", _estimatedDuration2InMinutes);
            cmd.AddParameter("EstimatedTimePerYearInH", _estimatedTimePerYearInH);
            cmd.AddParameter("EstimateType", _estimateType);
            cmd.AddParameter("ShortDescription", _shortDescription);
        }

        internal void AddAffordanceToLocation([NotNull] Location location,
                                              [NotNull] Affordance aff,
                                              [CanBeNull] TimeLimit timeLimit,
                                              int weight,
                                              int startMinusTime,
                                              int startPlusTime,
                                              int endMinusTime,
                                              int endPlusTime)
        {
            var hhl = _locations.FirstOrDefault(loc => location == loc.Location);
            if (hhl == null) {
                Logger.Error("HHT Location could not be found");
                return;
            }

            AddAffordanceToLocation(hhl, aff, timeLimit, weight, startMinusTime, startPlusTime, endMinusTime, endPlusTime);
        }

        internal void AddAffordanceToLocation([NotNull] HHTLocation location,
                                              [NotNull] Affordance aff,
                                              [CanBeNull] TimeLimit timeLimit,
                                              int weight,
                                              int startMinusTime,
                                              int startPlusTime,
                                              int endMinusTime,
                                              int endPlusTime)
        {
            var hhl = _locations.First(loc => location.Location == loc.Location);

            foreach (var hhAffordance in hhl.AffordanceLocations) {
                if (hhAffordance.HHTLocation?.Location == location.Location && hhAffordance.Affordance == aff &&
                    hhAffordance.TimeLimit == timeLimit && hhAffordance.Weight == weight && hhAffordance.StartMinusMinutes == startMinusTime &&
                    hhAffordance.StartPlusMinutes == startPlusTime && hhAffordance.EndMinusMinutes == endMinusTime &&
                    hhAffordance.EndPlusMinutes == endPlusTime) {
                    Logger.Info("This trait was already added.");
                    return;
                }
            }

            var hhaff = new HHTAffordance(null,
                aff,
                hhl,
                IntID,
                ConnectionString,
                aff.Name + " - " + hhl.Name,
                timeLimit,
                weight,
                startMinusTime,
                startPlusTime,
                endMinusTime,
                endPlusTime,
                System.Guid.NewGuid().ToStrGuid());
            hhaff.SaveToDB();
            Logger.Get().SafeExecute(() => {
                hhl.AffordanceLocations.Add(hhaff);
                hhl.AffordanceLocations.Sort();

                hhl.Notify("AvailableAffordances");
            });
        }

        [NotNull]
        internal HHTAutonomousDevice AddAutomousDevice([NotNull] IAssignableDevice device,
                                                       [CanBeNull] TimeBasedProfile timeBasedProfile,
                                                       decimal timeStandardDeviation,
                                                       [CanBeNull] VLoadType vLoadType,
                                                       [CanBeNull] TimeLimit timeLimit,
                                                       [CanBeNull] Location loc,
                                                       double variableValue,
                                                       VariableCondition variableCondition,
                                                       [CanBeNull] Variable variable)
        {
            var name = device.Name;
            var hhad = new HHTAutonomousDevice(null,
                device,
                timeBasedProfile,
                IntID,
                timeStandardDeviation,
                vLoadType,
                timeLimit,
                ConnectionString,
                name,
                loc,
                variableValue,
                variableCondition,
                variable,
                System.Guid.NewGuid().ToStrGuid());

            Logger.Get().SafeExecuteWithWait(() => {
                Autodevs.Add(hhad);
                Autodevs.Sort();
            });
            SaveToDB();
            return hhad;
        }

        [NotNull]
        internal HHTDesire AddDesire([NotNull] Desire desire,
                                     decimal decayTime,
                                     [NotNull] string healthStatus,
                                     decimal threshold,
                                     decimal weight,
                                     int minAge,
                                     int maxAge,
                                     PermittedGender gender)
        {
            var name = desire.Name;
            var hht = new HHTDesire(null,
                IntID,
                decayTime,
                desire,
                HHTDesire.GetHealthStatusEnumForHealthStatusString(healthStatus),
                threshold,
                weight,
                ConnectionString,
                name,
                minAge,
                maxAge,
                gender,
                System.Guid.NewGuid().ToStrGuid());
            Desires.Add(hht);
            hht.SaveToDB();
            return hht;
        }

        [NotNull]
        internal HHTLocation AddLocation([NotNull] Location location)
        {
            foreach (var hhLocation in _locations) {
                if (hhLocation.Location == location) {
                    return hhLocation;
                }
            }

            var hhl = new HHTLocation(null, location, ID, location.Name, ConnectionString, System.Guid.NewGuid().ToStrGuid());
            _locations.Add(hhl);
            hhl.SaveToDB();
            _locations.Sort();
            return hhl;
        }

        internal void AddTrait([NotNull] HouseholdTrait trait)
        {
            if (trait == this) {
                return;
            }

            var hhad = new HHTTrait(null, ID, trait, ConnectionString, trait.Name, System.Guid.NewGuid().ToStrGuid());
            _subTraits.Add(hhad);
            _subTraits.Sort();
            SaveToDB();
        }

        internal void DeleteHHTAutonomousDeviceFromDB([NotNull] HHTAutonomousDevice hhAutonomous)
        {
            hhAutonomous.DeleteFromDB();
            Autodevs.Remove(hhAutonomous);
        }

        internal void DeleteHHTLocationFromDB([NotNull] HHTLocation hhl)
        {
            if (hhl.ID != null) {
                hhl.DeleteFromDB();
            }

            _locations.Remove(hhl);
        }

        internal void DeleteHHTTag([NotNull] HHTTag tag)
        {
            tag.DeleteFromDB();
            _tags.Remove(tag);
        }

        internal void DeleteHHTTrait([NotNull] HHTTrait hhtrait)
        {
            hhtrait.DeleteFromDB();
            _subTraits.Remove(hhtrait);
        }

        [NotNull]
        private HHTAutonomousDevice AddAutonomousDeviceFromJto([NotNull] HHTAutonomousDevice.JsonDto jto, [NotNull] Simulator sim)
        {
            var dev = sim.GetAssignableDeviceByGuid(jto.Device.Guid) ?? throw new LPGException("Could not find " + jto.Device);
            var timeprofile = sim.Timeprofiles.FindByJsonReference(jto.TimeProfile);
            var lt = sim.LoadTypes.FindByJsonReference(jto.LoadType);
            var timelimit = sim.TimeLimits.FindByJsonReference(jto.TimeLimit);
            var loc = sim.Locations.FindByJsonReference(jto.Location);
            var variable = sim.Variables.FindByJsonReference(jto.Variable);
            return AddAutomousDevice(dev, timeprofile, jto.StandardDeviation, lt, timelimit, loc, jto.VariableValue, jto.VariableCondition, variable);
        }

        [NotNull]
        private HHTDesire AddDesireFromJto([NotNull] HHTDesire.JsonDto jto, [NotNull] Simulator sim)
        {
            Desire d = sim.Desires.FindByJsonReference(jto.Desire) ?? throw new LPGException("Could not import desire " + jto.Desire);
            var desire = AddDesire(d, jto.DecayTime, jto.Sicknessdesire.ToString(), jto.Threshold, jto.Weight, jto.MinAge, jto.MaxAge, jto.Gender);
            return desire;
        }

        [NotNull]
        private HHTLocation AddLocationFromJto([NotNull] HHTLocation.JsonDto jto, [NotNull] Simulator sim)
        {
            var loc = sim.Locations.FindByJsonReference(jto.Location) ?? throw new LPGException("Could not import Location " + jto.Location);
            var hhtloc = AddLocation(loc);
            foreach (var jtoAffordance in jto.Affordances) {
                var aff = sim.Affordances.FindByJsonReference(jtoAffordance.Affordance) ??
                          throw new LPGException("Could not find Affordance " + jtoAffordance.Affordance);
                var timelimit = sim.TimeLimits.FindByJsonReference(jtoAffordance.TimeLimit);
                AddAffordanceToLocation(hhtloc,
                    aff,
                    timelimit,
                    jtoAffordance.Weight,
                    jtoAffordance.StartMinusMinutes,
                    jtoAffordance.StartPlusMinutes,
                    jtoAffordance.EndMinusMinutes,
                    jtoAffordance.EndPlusMinutes);
            }

            return hhtloc;
        }

        [NotNull]
        private static HouseholdTrait AssignFields([NotNull] DataReader dr,
                                                   [NotNull] string connectionString,
                                                   bool ignoreMissingFields,
                                                   [NotNull] AllItemCollections aic)
        {
            var hhid = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name", "no name");
            var description = dr.GetString("Description", false);
            var classificaiton = dr.GetString("Classification", false, "unknown", ignoreMissingFields);
            var minimumPersonCount = dr.GetIntFromLong("MinimumPersonCount", false, ignoreMissingFields, -1);
            var maximumPersonCount = dr.GetIntFromLong("MaximumPersonCount", false, ignoreMissingFields, -1);
            var maximumNumberCount = dr.GetIntFromLong("MaximumNumberCount", false, ignoreMissingFields, -1);
            var estimatedTimes = dr.GetDouble("EstimatedTimes", false, -1, ignoreMissingFields);
            var estimatedTimeCount = dr.GetDouble("EstimatedTimeCount", false, -1, ignoreMissingFields);
            var estimatedTimeType = (TimeType)dr.GetIntFromLong("estimatedTimeType", false, ignoreMissingFields);

            var estimatedTimes2 = dr.GetDouble("EstimatedTimes2", false, -1, ignoreMissingFields);
            var estimatedTimeCount2 = dr.GetDouble("EstimatedTimeCount2", false, -1, ignoreMissingFields);
            var estimatedTimeType2 = (TimeType)dr.GetIntFromLong("estimatedTimeType2", false, ignoreMissingFields);
            var estimatedDuration2 = dr.GetDouble("EstimatedDuration2", false, -1, ignoreMissingFields);
            var estimatedTimePerYear = dr.GetDouble("EstimatedTimePerYearInH", false, 0, ignoreMissingFields);
            var estimateType = (EstimateType)dr.GetIntFromLong("EstimateType", false, ignoreMissingFields);
            var shortDescription = dr.GetString("ShortDescription", false, "", ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var hh = new HouseholdTrait(name,
                hhid,
                description,
                connectionString,
                classificaiton,
                minimumPersonCount,
                maximumPersonCount,
                maximumNumberCount,
                estimatedTimes,
                estimatedTimeCount,
                estimatedTimeType,
                estimatedTimes2,
                estimatedTimeCount2,
                estimatedTimeType2,
                estimatedDuration2,
                estimatedTimePerYear,
                estimateType,
                shortDescription,
                guid);
            return hh;
        }

        private TimeSpan CalculateAverageAffordanceDuration()
        {
            var ts = TimeSpan.FromTicks(0);
            var count = 0;
            foreach (var affordance in CollectAffordances(true)) {
                if (affordance.PersonProfile != null) {
                    ts = ts.Add(affordance.PersonProfile.Duration);
                    count++;
                }
            }

            if (count > 0) {
                var totalseconds = ts.TotalSeconds / count;
                return TimeSpan.FromSeconds(totalseconds);
            }

            return new TimeSpan(0);
        }

        private static bool IsCorrectHHAutonomousParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (HHTAutonomousDevice)child;
            if (parent.ID == hd.HouseholdTraitID) {
                var hh = (HouseholdTrait)parent;
                hh.Autodevs.Add(hd);
                return true;
            }

            return false;
        }

        private static bool IsCorrectHHTDesireParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (HHTDesire)child;
            if (parent.ID == hd.HouseholdTraitID) {
                var hh = (HouseholdTrait)parent;
                hh.Desires.Add(hd);
                return true;
            }

            return false;
        }

        private static bool IsCorrectHHTLocationParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (HHTLocation)child;
            if (parent.ID == hd.HouseholdTraitID) {
                var hht = (HouseholdTrait)parent;
                hht._locations.Add(hd);
                hd.ParentHouseholdTrait = hht;
                return true;
            }

            return false;
        }

        private static bool IsCorrectHHTTagParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (HHTTag)child;
            if (parent.ID == hd.HouseholdTraitID) {
                var hh = (HouseholdTrait)parent;
                hh.Tags.Add(hd);
                return true;
            }

            return false;
        }

        private static bool IsCorrectHHTTraitParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (HHTTrait)child;
            if (parent.ID == hd.ParentTraitID) {
                var hht = (HouseholdTrait)parent;
                hht.SubTraits.Add(hd);
                return true;
            }

            return false;
        }

        public class JsonDto {
            [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
            [Obsolete("Json Only")]
            public JsonDto()
            {
            }

            public JsonDto([CanBeNull] string name,
                           [NotNull] string description,
                           [NotNull] string classification,
                           double estimatedDuration2InMinutes,
                           double estimatedTimeCount,
                           double estimatedTimeCount2,
                           double estimatedTimePerYearInH,
                           double estimatedTimes,
                           double estimatedTimes2,
                           TimeType estimatedTimeType,
                           TimeType estimatedTimeType2,
                           EstimateType estimateType,
                           int maximumNumberInCHH,
                           int maximumPersonsInCHH,
                           int minimumPersonsInCHH,
                           string shortDescription,
                           StrGuid guid,
                           double estimatedTimeInSeconds)
            {
                Name = name;
                Description = description;
                Classification = classification;
                EstimatedDuration2InMinutes = estimatedDuration2InMinutes;
                EstimatedTimeCount = estimatedTimeCount;
                EstimatedTimeCount2 = estimatedTimeCount2;
                EstimatedTimePerYearInH = estimatedTimePerYearInH;
                EstimatedTimes = estimatedTimes;
                EstimatedTimes2 = estimatedTimes2;
                EstimatedTimeType = estimatedTimeType;
                EstimatedTimeType2 = estimatedTimeType2;
                EstimateType = estimateType;
                MaximumNumberInCHH = maximumNumberInCHH;
                MaximumPersonsInCHH = maximumPersonsInCHH;
                MinimumPersonsInCHH = minimumPersonsInCHH;
                ShortDescription = shortDescription;
                Guid = guid;
                EstimatedTimeInSeconds = estimatedTimeInSeconds;
            }

            [NotNull]
            [ItemNotNull]
            public List<HHTAutonomousDevice.JsonDto> AutonomousDevices { get; set; } = new List<HHTAutonomousDevice.JsonDto>();

            [NotNull]
            public string Classification { get; set; }

            [NotNull]
            public string Description { get; set; }

            [ItemNotNull]
            [NotNull]
            public List<HHTDesire.JsonDto> Desires { get; set; } = new List<HHTDesire.JsonDto>();

            public double EstimatedDuration2InMinutes { get; set; }
            public double EstimatedTimeCount { get; set; }
            public double EstimatedTimeCount2 { get; set; }
            public double EstimatedTimeInSeconds { get; set; }
            public double EstimatedTimePerYearInH { get; set; }
            public double EstimatedTimes { get; set; }
            public double EstimatedTimes2 { get; set; }
            public TimeType EstimatedTimeType { get; set; }
            public TimeType EstimatedTimeType2 { get; set; }
            public EstimateType EstimateType { get; set; }
            public StrGuid Guid { get; set; }

            [NotNull]
            [ItemNotNull]
            public List<HHTLocation.JsonDto> Locations { get; set; } = new List<HHTLocation.JsonDto>();

            public int MaximumNumberInCHH { get; set; }
            public int MaximumPersonsInCHH { get; set; }
            public int MinimumPersonsInCHH { get; set; }

            [CanBeNull]
            public string Name { get; set; }

            public string ShortDescription { get; set; }

            [ItemNotNull]
            [NotNull]
            public List<JsonReference> SubTraits { get; set; } = new List<JsonReference>();

            public List<JsonReference> Tags { get; set; } = new List<JsonReference>();
        }
    }
}