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
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    public class Affordance : DBBaseElement {
        public const string TableName = "tblAffordances";
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffordanceDesire> _affDesires;

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffordanceStandby> _affStandby;

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffordanceDevice> _deviceprofiles;

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffVariableOperation> _executedVariables;

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffVariableRequirement> _requiredVariables;

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<AffordanceSubAffordance> _subAffordances;

        private ActionAfterInterruption _actionAfterInterruption;

        [CanBeNull] private string _affCategory;

        private ColorRGB _carpetPlotColor;

        [CanBeNull] private string _description;

        private bool _isInterruptable;
        private bool _isInterrupting;
        private int _maximumAge;
        private int _minimumAge;
        private bool _needsLight;
        private PermittedGender _permittedGender;

        [CanBeNull] private TimeBasedProfile _personProfile;

        private bool _randomDesireResults;
        private bool _requireAllDesires;

        private BodilyActivityLevel _bodilyActivityLevel;

        [CanBeNull] private TimeLimit _timeLimit;

        private decimal _timeStandardDeviation;

        public Affordance([JetBrains.Annotations.NotNull] string pName, [CanBeNull] TimeBasedProfile pPersonProfile, [CanBeNull] int? id,
            bool needsLight,
            PermittedGender permittedGender, decimal timeStandardDeviation, ColorRGB carpetPlotColor,
            [JetBrains.Annotations.NotNull] string affCategory,
            [CanBeNull] TimeLimit timeLimitLimit, [CanBeNull] string description, [JetBrains.Annotations.NotNull] string connectionString,
            bool isInterruptable,
            bool isInterrupting, int minimumAge, int maximumAge, bool randomResults,
            ActionAfterInterruption actionAfterInterruption, bool requireAllDesires, StrGuid guid,
            BodilyActivityLevel bodilyActivityLevel) : base(pName, TableName,
            connectionString, guid)
        {
            RandomDesireResults = false;
            ID = id;
            _personProfile = pPersonProfile;
            _affDesires = new ObservableCollection<AffordanceDesire>();
            _deviceprofiles = new ObservableCollection<AffordanceDevice>();
            _subAffordances = new ObservableCollection<AffordanceSubAffordance>();
            _affStandby = new ObservableCollection<AffordanceStandby>();
            _executedVariables = new ObservableCollection<AffVariableOperation>();
            _requiredVariables = new ObservableCollection<AffVariableRequirement>();
            _needsLight = needsLight;
            _permittedGender = permittedGender;
            _timeStandardDeviation = timeStandardDeviation;
            _carpetPlotColor = carpetPlotColor;
            _affCategory = affCategory;
            _timeLimit = timeLimitLimit;
            _isInterruptable = isInterruptable;
            _isInterrupting = isInterrupting;
            _description = description;
            _maximumAge = maximumAge;
            _minimumAge = minimumAge;
            _randomDesireResults = randomResults;
            _actionAfterInterruption = actionAfterInterruption;
            TypeDescription = "Affordance";
            _affStandby = new ObservableCollection<AffordanceStandby>();
            _requireAllDesires = requireAllDesires;
            _bodilyActivityLevel = bodilyActivityLevel;
        }

        [UsedImplicitly]
        public ActionAfterInterruption ActionAfterInterruption {
            get => _actionAfterInterruption;
            set {
                SetValueWithNotify(value, ref _actionAfterInterruption, nameof(ActionAfterInterruption));
                OnPropertyChanged(nameof(ActionAfterInterruptionStr));
            }
        }

        [UsedImplicitly]
        public BodilyActivityLevel BodilyActivityLevel
        {
            get => _bodilyActivityLevel;
            set => SetValueWithNotify(value, ref _bodilyActivityLevel, nameof(BodilyActivityLevel));
        }

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public string ActionAfterInterruptionStr {
            get => ActionAfterInterruptionHelper.ConvertToDescription(_actionAfterInterruption);
            set {
                SetValueWithNotify(ActionAfterInterruptionHelper.ConvertToEnum(value), ref _actionAfterInterruption,
                    nameof(ActionAfterInterruption));
                OnPropertyChanged(nameof(ActionAfterInterruption));
            }
        }

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public string AffCategory {
            get => _affCategory ?? throw new InvalidOperationException();
            set => SetValueWithNotify(value, ref _affCategory, nameof(AffCategory));
        }

        [ItemNotNull]
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<AffordanceDesire> AffordanceDesires => _affDesires;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<AffordanceDevice> AffordanceDevices => _deviceprofiles;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<AffordanceStandby> AffordanceStandbys => _affStandby;

        [UsedImplicitly]
        public int Blue {
            get => CarpetPlotColor.B;
            set {
                if (value == _carpetPlotColor.B) {
                    return;
                }

                _carpetPlotColor = new ColorRGB(_carpetPlotColor.R, _carpetPlotColor.G, (byte) value);
                ColorChanged(true);
            }
        }

       /*
        //todo: reimplment somehow
            [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public Brush CarpetPlotBrush => new SolidColorBrush(_carpetPlotColor);*/

        public ColorRGB CarpetPlotColor {
            get => _carpetPlotColor;
            set {
                if (_carpetPlotColor == value) {
                    return;
                }

                if ( _carpetPlotColor.R == value.R &&
                    _carpetPlotColor.G == value.G && _carpetPlotColor.B == value.B) {
                    return;
                }

                _carpetPlotColor = value;
                ColorChanged(false);
            }
        }

        [UsedImplicitly]
        [CanBeNull]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<AffVariableOperation> ExecutedVariables => _executedVariables;

        [UsedImplicitly]
        public int Green {
            get => CarpetPlotColor.G;
            set {
                if (value == _carpetPlotColor.G) {
                    return;
                }

                _carpetPlotColor = new ColorRGB(_carpetPlotColor.R, (byte) value, _carpetPlotColor.B);
                ColorChanged(true);
            }
        }

        [UsedImplicitly]
        public bool IsInterruptable {
            get => _isInterruptable;
            set => SetValueWithNotify(value, ref _isInterruptable, nameof(IsInterruptable));
        }

        [UsedImplicitly]
        public bool IsInterrupting {
            get => _isInterrupting;
            set => SetValueWithNotify(value, ref _isInterrupting, nameof(IsInterrupting));
        }

        [UsedImplicitly]
        public int MaximumAge {
            get => _maximumAge;
            set => SetValueWithNotify(value, ref _maximumAge, nameof(MaximumAge));
        }

        [UsedImplicitly]
        public int MinimumAge {
            get => _minimumAge;
            set => SetValueWithNotify(value, ref _minimumAge, nameof(MinimumAge));
        }

        [UsedImplicitly]
        public bool NeedsLight {
            get => _needsLight;
            set => SetValueWithNotify(value, ref _needsLight, nameof(NeedsLight));
        }

        public PermittedGender PermittedGender {
            get => _permittedGender;
            [UsedImplicitly] set => SetValueWithNotify(value, ref _permittedGender, nameof(PermittedGender));
        }

        [CanBeNull]
        public TimeBasedProfile PersonProfile {
            get => _personProfile;
            [UsedImplicitly] set => SetValueWithNotify(value, ref _personProfile, false, nameof(PersonProfile));
        }

        [UsedImplicitly]
        public bool RandomDesireResults {
            get => _randomDesireResults;
            set => SetValueWithNotify(value, ref _randomDesireResults, nameof(RandomDesireResults));
        }

        [UsedImplicitly]
        public int Red {
            get => CarpetPlotColor.R;
            set {
                if (value == _carpetPlotColor.R) {
                    return;
                }

                _carpetPlotColor = new ColorRGB((byte) value, _carpetPlotColor.G, _carpetPlotColor.B);
                ColorChanged(true);
            }
        }

        [UsedImplicitly]
        public bool RequireAllDesires {
            get => _requireAllDesires;
            set => SetValueWithNotify(value, ref _requireAllDesires, nameof(RequireAllDesires));
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<AffVariableRequirement> RequiredVariables => _requiredVariables;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<AffordanceSubAffordance> SubAffordances => _subAffordances;

        [UsedImplicitly]
        [CanBeNull]
        public TimeLimit TimeLimit {
            get => _timeLimit;
            set => SetValueWithNotify(value, ref _timeLimit, false, nameof(TimeLimit));
        }

        public decimal TimeStandardDeviation {
            get => _timeStandardDeviation;
            [UsedImplicitly]
            set => SetValueWithNotify(value, ref _timeStandardDeviation, nameof(TimeStandardDeviation));
        }

        public void AddDesire([JetBrains.Annotations.NotNull] Desire desire, decimal satisfactionvalue,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<Desire> desires)
        {
            // delete old desire values for the same desire for the update function
            for (var i = 0; i < _affDesires.Count; i++) {
                if (_affDesires[i].Desire == desire) {
                    _affDesires[i].DeleteFromDB();
                    _affDesires.RemoveAt(i);
                    i = _affDesires.Count + 1;
                }
            }

            var affordanceDesire = new AffordanceDesire(null, desire, IntID, satisfactionvalue, desires,
                desire.Name, ConnectionString, System.Guid.NewGuid().ToStrGuid());
            _affDesires.Add(affordanceDesire);
            affordanceDesire.SaveToDB();
            _affDesires.Sort();
        }

        public void AddDeviceProfile([JetBrains.Annotations.NotNull] IAssignableDevice device, [CanBeNull] TimeBasedProfile tp,
            decimal timeoffset,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<RealDevice> allDevices,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<DeviceCategory> allDeviceCategories,
            [CanBeNull] VLoadType vLoadType, double probability)
        {
            var deviceName = device.Name;
            var dtl = new AffordanceDevice(device, tp, null, timeoffset, IntID, allDevices,
                allDeviceCategories, deviceName, vLoadType, ConnectionString, probability, System.Guid.NewGuid().ToStrGuid());

            _deviceprofiles.Add(dtl);
            _deviceprofiles.Sort();
            dtl.SaveToDB();
        }

        public void AddStandby([JetBrains.Annotations.NotNull] IAssignableDevice device)
        {
            var deviceName = device.Name;
            var standby = new AffordanceStandby(device, null,
                IntID, ConnectionString, deviceName, System.Guid.NewGuid().ToStrGuid());
            _affStandby.Add(standby);
            _affStandby.Sort();
            standby.SaveToDB();
        }

        public void AddSubAffordance([JetBrains.Annotations.NotNull] SubAffordance subAffordance, decimal delaytime)
        {
            var affordanceSubAffordance = new AffordanceSubAffordance(null, subAffordance,
                delaytime, IntID, Name, ConnectionString, subAffordance.Name, System.Guid.NewGuid().ToStrGuid());
            SubAffordances.Add(affordanceSubAffordance);
            affordanceSubAffordance.SaveToDB();
        }

        public void AddVariableOperation(double value, VariableLocationMode clm, [CanBeNull] Location loc,
            VariableAction action, [JetBrains.Annotations.NotNull] Variable variable, [CanBeNull] string description,
            VariableExecutionTime executionTime)
        {
            var itemsToDelete = _executedVariables
                .Where(x => x.Variable == variable && x.LocationMode == clm && x.Location == loc).ToList();
            foreach (var variableOperation in itemsToDelete) {
                variableOperation.DeleteFromDB();
                _executedVariables.Remove(variableOperation);
            }

            //if (variable != null) {
            var name = variable.Name;
            //}
            var at = new AffVariableOperation(value, null, IntID, ConnectionString, clm, loc, action,
                variable, description, executionTime, name, System.Guid.NewGuid().ToStrGuid());

            _executedVariables.Add(at);
            at.SaveToDB();
            _executedVariables.Sort();
        }

        public void AddVariableRequirement(double value, VariableLocationMode clm, [CanBeNull] Location loc,
            VariableCondition action, [JetBrains.Annotations.NotNull] Variable variable, [JetBrains.Annotations.NotNull] string description)
        {
            var itemsToDelete = _requiredVariables
                .Where(x => x.Variable == variable && x.LocationMode == clm && x.Location == loc).ToList();
            foreach (var variableRequirement in itemsToDelete) {
                variableRequirement.DeleteFromDB();
                _requiredVariables.Remove(variableRequirement);
            }

            var at = new AffVariableRequirement(value, null, IntID, ConnectionString, clm, loc,
                action, variable, description, "(no name)", System.Guid.NewGuid().ToStrGuid());
            _requiredVariables.Add(at);
            at.SaveToDB();
            _requiredVariables.Sort();
        }

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static Affordance AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID", false, ignoreMissingFields);
            var name = dr.GetString("Name", "no name");

            TimeBasedProfile tp = null;
            if (dr["PersonProfileID"] != DBNull.Value) {
                var personprofileID = dr.GetInt("PersonProfileID"); // time profile for the person
                tp = aic.TimeProfiles.FirstOrDefault(tpl => tpl.ID == personprofileID);
            }

            var minimumage = dr.GetInt("MinimumAge", true, 0);
            var maximumage = dr.GetInt("MaximumAge", true, 99);
            var randomDesireResults = dr.GetBool("randomDesireResults");
            var needsLight = dr.GetBool("NeedsLight", false);
            var timeStandardDeviation = dr.GetDecimal("TimeStandardDeviation", false, 0.1m, ignoreMissingFields);
            PermittedGender permittedGender =
                (PermittedGender) dr.GetInt("PermittedGender", false, (int) PermittedGender.All);
            var description = dr.GetString("Description", false, string.Empty, ignoreMissingFields);
            var affCategory = dr.GetString("AffCategory", false);
            var colorint1 = dr.GetInt("CarpetPlotColor1", false, 0, ignoreMissingFields);
            var colorint2 = dr.GetInt("CarpetPlotColor2", false, 0, ignoreMissingFields);
            var colorint3 = dr.GetInt("CarpetPlotColor3", false, 0, ignoreMissingFields);
            var colorint4 = dr.GetInt("CarpetPlotColor4", false, 0, ignoreMissingFields);
            var bodilyactivitylevel =(BodilyActivityLevel) dr.GetLong("BodilyActivityLevel", false, 0, ignoreMissingFields);
            var carpetPlotColor = new ColorRGB((byte) colorint1, (byte) colorint2, (byte) colorint3,
                (byte) colorint4);
            var timeLimitID = dr.GetNullableIntFromLong("TimeLimitID", false, ignoreMissingFields);
            if (timeLimitID == null && ignoreMissingFields) {
                timeLimitID = dr.GetNullableIntFromLong("DeviceTimeID", false, ignoreMissingFields);
            }

            TimeLimit timeLimit = null;
            if (timeLimitID != null) {
                timeLimit = aic.TimeLimits.FirstOrDefault(x => x.ID == timeLimitID);
            }

            var isInterruptable = dr.GetBool("IsInterruptable", false, false, ignoreMissingFields);
            var isInterrupting = dr.GetBool("IsInterrupting", false, false, ignoreMissingFields);
            var aai =
                (ActionAfterInterruption) dr.GetIntFromLong("ActionAfterInterruption", false, ignoreMissingFields);
            bool requireAllDesires = dr.GetBool("RequireAllDesires", false, false, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var aff = new Affordance(name, tp, id, needsLight, permittedGender, timeStandardDeviation,
                carpetPlotColor, affCategory, timeLimit, description, connectionString, isInterruptable, isInterrupting,
                minimumage, maximumage, randomDesireResults, aai, requireAllDesires, guid, bodilyactivitylevel);

            return aff;
        }

        [JetBrains.Annotations.NotNull]
        public Dictionary<VLoadType, double> CalculateAverageEnergyUse(
            [CanBeNull] [ItemNotNull] ObservableCollection<DeviceAction> allActions)
        {
            var results = new Dictionary<VLoadType, double>();

            foreach (var device in AffordanceDevices) {
                if (device.Device == null) {
                    Logger.Error("Device was null");
                    continue;
                }

                if (device.Device.AssignableDeviceType == AssignableDeviceType.Device && device.LoadType == null) {
                    Logger.Error("The device " + device.Device.Name +
                                 " has no load type assigned. This should be fixed.");
                    return new Dictionary<VLoadType, double>();
                }

                var result =
                    device.Device.CalculateAverageEnergyUse(device.LoadType, allActions, device.TimeProfile, 1,
                        device.Probability);
                foreach (var tuple in result) {
                    if (!results.ContainsKey(tuple.Item1)) {
                        results.Add(tuple.Item1, 0);
                    }

                    results[tuple.Item1] += tuple.Item2;
                }
            }

            return results;
        }

        public TimeSpan CalculateMaximumInternalTimeResolution()
        {
            var result = TimeSpan.MaxValue;
            var personmin = TimeSpan.MaxValue;

            if (_personProfile != null) {
                personmin = TimeBasedProfile.CalculateMinimumTimespan();
            }

            if (personmin < result) {
                result = personmin;
            }

            foreach (var affordanceDevice in _deviceprofiles) {
                if (affordanceDevice.Device == null) {
                    Logger.Error("Device was null");
                    continue;
                }

                switch (affordanceDevice.Device.AssignableDeviceType) {
                    case AssignableDeviceType.Device:
                    case AssignableDeviceType.DeviceCategory:
                        var minimumTimespan = TimeSpan.MaxValue;
                        if (affordanceDevice.TimeProfile != null) {
                            minimumTimespan = TimeBasedProfile.CalculateMinimumTimespan();
                        }

                        if (minimumTimespan < result) {
                            result = minimumTimespan;
                        }

                        break;
                    case AssignableDeviceType.DeviceAction:
                        if (DeviceAction.CalculateMinimumTimespan() < result) {
                            result = DeviceAction.CalculateMinimumTimespan();
                        }

                        break;
                    case AssignableDeviceType.DeviceActionGroup:
                        if (DeviceActionGroup.CalculateMinimumTimespan() < result) {
                            result = DeviceActionGroup.CalculateMinimumTimespan();
                        }

                        break;
                    default: throw new LPGException("Forgot an AssignableDeviceType. Please report!");
                }
            }

            return result;
        }

        public int CompareColor([CanBeNull] Affordance otherAffordance)
        {
            if (otherAffordance == null) {
                return 0;
            }

            if (Red != otherAffordance.Red) {
                return Red.CompareTo(otherAffordance.Red);
            }

            if (Green != otherAffordance.Green) {
                return Green.CompareTo(otherAffordance.Green);
            }

            if (Blue != otherAffordance.Blue) {
                return Blue.CompareTo(otherAffordance.Blue);
            }

            var c1 = System.Drawing.Color.FromArgb(0, Red, Green, Blue);
            var c2 =
                System.Drawing.Color.FromArgb(0, otherAffordance.Red, otherAffordance.Green, otherAffordance.Blue);
            return c1.GetBrightness().CompareTo(c2.GetBrightness());
        }

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString)
        {
            var aff = new Affordance(FindNewName(isNameTaken, "New Affordance "), null, null, false,
                PermittedGender.All, 0.1m, new ColorRGB(255, 255, 255), string.Empty, null, string.Empty,
                connectionString, false, false, 0, 99, false, ActionAfterInterruption.GoBackToOld,
                false, System.Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            return aff;
        }

        public bool DeleteAffordanceDesireFromDB([JetBrains.Annotations.NotNull] AffordanceDesire affordanceDesire)
        {
            affordanceDesire.DeleteFromDB();
            _affDesires.Remove(affordanceDesire);
            return true;
        }

        public void DeleteAffordanceSubAffFromDB([JetBrains.Annotations.NotNull] AffordanceSubAffordance affordanceSubAffordance)
        {
            affordanceSubAffordance.DeleteFromDB();
            SubAffordances.Remove(affordanceSubAffordance);
        }

        public void DeleteDeviceFromDB([JetBrains.Annotations.NotNull] AffordanceDevice affordanceDevice)
        {
            affordanceDevice.DeleteFromDB();
            _deviceprofiles.Remove(affordanceDevice);
        }

        public override void DeleteFromDB()
        {
            if (ID != null) {
                foreach (var affordanceDesire in AffordanceDesires) {
                    affordanceDesire.DeleteFromDB();
                }

                foreach (var affordanceDevice in AffordanceDevices) {
                    affordanceDevice.DeleteFromDB();
                }

                foreach (var affordanceSubAffordance in SubAffordances) {
                    affordanceSubAffordance.DeleteFromDB();
                }

                foreach (var standby in _affStandby) {
                    standby.DeleteFromDB();
                }

                foreach (var affVariable in _executedVariables) {
                    affVariable.DeleteFromDB();
                }

                foreach (var requiredVariable in _requiredVariables) {
                    requiredVariable.DeleteFromDB();
                }

                base.DeleteFromDB();
            }
        }

        public void DeleteStandby([JetBrains.Annotations.NotNull] AffordanceStandby standby)
        {
            standby.DeleteFromDB();
            _affStandby.Remove(standby);
        }

        public void DeleteVariableOperation([JetBrains.Annotations.NotNull] AffVariableOperation variableOperation)
        {
            variableOperation.DeleteFromDB();
            _executedVariables.Remove(variableOperation);
        }

        public void DeleteVariableRequirement([JetBrains.Annotations.NotNull] AffVariableRequirement variableRequirement)
        {
            variableRequirement.DeleteFromDB();
            _requiredVariables.Remove(variableRequirement);
        }

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var usedIns = new List<UsedIn>();
            foreach (var trait in sim.HouseholdTraits.Items) {
                foreach (var hhtLocation in trait.Locations) {
                    if (hhtLocation.AffordanceLocations.Any(x => x.Affordance == this)) {
                        usedIns.Add(new UsedIn(trait, "Household Trait"));
                    }
                }
            }
            return usedIns;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public static Affordance ImportFromItem([JetBrains.Annotations.NotNull] Affordance toImport, [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            var tp = GetItemFromListByName(dstSim.Timeprofiles.Items, toImport.PersonProfile?.Name);
            TimeLimit timeLimit = null;
            if (toImport.TimeLimit != null) {
                timeLimit = GetItemFromListByName(dstSim.TimeLimits.Items, toImport.TimeLimit.Name);
                if (timeLimit != null) {
                    if (timeLimit.ConnectionString != dstSim.ConnectionString) {
                        throw new LPGException("imported for the wrong db!");
                    }
                }
                else {
                    Logger.Error("Could not find a time limit to import.");
                }
            }

            var aff = new Affordance(toImport.Name, tp, null, toImport.NeedsLight, toImport.PermittedGender,
                toImport.TimeStandardDeviation, toImport.CarpetPlotColor, toImport.AffCategory, timeLimit,
                toImport.Description, dstSim.ConnectionString, toImport.IsInterruptable, toImport.IsInterrupting,
                toImport.MinimumAge, toImport.MaximumAge, toImport.RandomDesireResults,
                toImport._actionAfterInterruption, toImport.RequireAllDesires, toImport.Guid,
                toImport.BodilyActivityLevel);
            aff.SaveToDB();

            foreach (var affordanceDesire in toImport.AffordanceDesires) {
                var des = GetItemFromListByName(dstSim.Desires.Items, affordanceDesire.Desire.Name);
                if (des == null) {
                    throw new LPGException("item not found");
                }

                if (des.ConnectionString != dstSim.ConnectionString) {
                    throw new LPGException("imported for the wrong db!");
                }

                aff.AddDesire(des, affordanceDesire.SatisfactionValue, dstSim.Desires.Items);
            }

            foreach (var affordanceDevice in toImport.AffordanceDevices) {
                var iad = GetAssignableDeviceFromListByName(dstSim.RealDevices.Items,
                    dstSim.DeviceCategories.Items, dstSim.DeviceActions.Items, dstSim.DeviceActionGroups.Items,
                    affordanceDevice.Device);
                TimeBasedProfile affordanceTimeProfile = null;
                if (affordanceDevice.TimeProfile != null) {
                    affordanceTimeProfile =
                        GetItemFromListByName(dstSim.Timeprofiles.Items, affordanceDevice.TimeProfile.Name);
                }

                VLoadType vlt = null;
                if (affordanceDevice.LoadType != null) {
                    vlt = GetItemFromListByName(dstSim.LoadTypes.Items, affordanceDevice.LoadType.Name);
                }

                if (iad.ConnectionString != dstSim.ConnectionString) {
                    throw new LPGException("imported for the wrong db!");
                }

                if (affordanceTimeProfile != null && affordanceTimeProfile.ConnectionString != dstSim.ConnectionString) {
                    throw new LPGException("imported for the wrong db!");
                }

                if (vlt != null && vlt.ConnectionString != dstSim.ConnectionString) {
                    throw new LPGException("imported for the wrong db!");
                }

                aff.AddDeviceProfile(iad, affordanceTimeProfile, affordanceDevice.TimeOffset,
                    dstSim.RealDevices.Items, dstSim.DeviceCategories.Items, vlt, affordanceDevice.Probability);
            }

            foreach (var affordanceSubAffordance in toImport.SubAffordances) {
                var subAffordance =
                    GetItemFromListByName(dstSim.SubAffordances.Items, affordanceSubAffordance.SubAffordance.Name);
                if (subAffordance.ConnectionString != dstSim.ConnectionString) {
                    throw new LPGException("imported for the wrong db!");
                }

                aff.AddSubAffordance(subAffordance, affordanceSubAffordance.DelayTime);
            }

            foreach (var standby in toImport.AffordanceStandbys) {
                var iad = GetAssignableDeviceFromListByName(dstSim.RealDevices.Items,
                    dstSim.DeviceCategories.Items, dstSim.DeviceActions.Items, dstSim.DeviceActionGroups.Items,
                    standby.Device);
                aff.AddStandby(iad);
            }

            foreach (var affVariableOperation in toImport.ExecutedVariables) {
                Location loc = null;
                if (affVariableOperation.Location != null) {
                    loc = GetItemFromListByName(dstSim.Locations.Items, affVariableOperation.Location.Name);
                }

                Variable variable = null;
                if (affVariableOperation.Variable != null) {
                    variable = GetItemFromListByName(dstSim.Variables.Items, affVariableOperation.Variable.Name);
                }

                aff.AddVariableOperation(affVariableOperation.Value, affVariableOperation.LocationMode, loc,
                    affVariableOperation.Action, variable, affVariableOperation.Description,
                    affVariableOperation.ExecutionTime);
            }

            foreach (var variableRequirement in toImport.RequiredVariables) {
                Location loc = null;
                if (variableRequirement.Location != null) {
                    loc = GetItemFromListByName(dstSim.Locations.Items, variableRequirement.Location.Name);
                }

                Variable variable = null;
                if (variableRequirement.Variable != null) {
                    variable = GetItemFromListByName(dstSim.Variables.FilteredItems, variableRequirement.Variable.Name);
                }

                aff.AddVariableRequirement(variableRequirement.Value, variableRequirement.LocationMode, loc,
                    variableRequirement.Condition, variable, variableRequirement.Description);
            }

            return aff;
        }

        public void ImportFromOtherAffordance([JetBrains.Annotations.NotNull] Affordance item,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<Desire> desires,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<RealDevice> realDevices,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<DeviceCategory> deviceCategories)
        {
            Name = item.Name + " (copy)";
            PersonProfile = item.PersonProfile;
            NeedsLight = item.NeedsLight;
            PermittedGender = item.PermittedGender;
            TimeStandardDeviation = item.TimeStandardDeviation;
            CarpetPlotColor = item.CarpetPlotColor;
            AffCategory = item.AffCategory;
            TimeLimit = item.TimeLimit;
            Description = item.Description;
            IsInterruptable = item.IsInterruptable;
            IsInterrupting = item.IsInterrupting;
            MinimumAge = item.MinimumAge;
            MaximumAge = item.MaximumAge;
            foreach (var affordanceDesire in item.AffordanceDesires) {
                AddDesire(affordanceDesire.Desire, affordanceDesire.SatisfactionValue, desires);
            }

            foreach (var affordanceDevice in item.AffordanceDevices) {
                if (affordanceDevice.Device != null) {
                    AddDeviceProfile(affordanceDevice.Device, affordanceDevice.TimeProfile, affordanceDevice.TimeOffset,
                        realDevices, deviceCategories, affordanceDevice.LoadType, affordanceDevice.Probability);
                }
            }

            foreach (var affordanceSubAffordance in item.SubAffordances) {
                if (affordanceSubAffordance.SubAffordance != null) {
                    AddSubAffordance(affordanceSubAffordance.SubAffordance, affordanceSubAffordance.DelayTime);
                }
            }

            foreach (var otherstandby in item.AffordanceStandbys) {
                if (otherstandby.Device != null) {
                    AddStandby(otherstandby.Device);
                }
            }

            foreach (var variableOperation in item.ExecutedVariables) {
                AddVariableOperation(variableOperation.Value, variableOperation.LocationMode,
                    variableOperation.Location, variableOperation.Action, variableOperation.Variable,
                    variableOperation.Description, variableOperation.ExecutionTime);
            }

            foreach (var variableRequirement in item.RequiredVariables) {
                AddVariableRequirement(variableRequirement.Value, variableRequirement.LocationMode,
                    variableRequirement.Location, variableRequirement.Condition, variableRequirement.Variable,
                    variableRequirement.Description);
            }
        }

        public bool IsAffordanceAvailable([JetBrains.Annotations.NotNull] [ItemNotNull] List<IAssignableDevice> devicesAtLoc,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<DeviceAction> allActions)
        {
            if (_deviceprofiles.Count == 0) {
                return false;
            }

            foreach (var affordanceDevice in _deviceprofiles) {
                if (!CheckAssignableDevice(devicesAtLoc, affordanceDevice, allActions)) {
                    return false;
                }
            }

            return true;
        }

        public bool IsValidPerson([JetBrains.Annotations.NotNull] Person person)
        {
            if (person.Age < MinimumAge) {
                return false;
            }

            if (person.Age > MaximumAge) {
                return false;
            }

            if (PermittedGender != PermittedGender.All) {
                if (PermittedGender != person.Gender) {
                    return false;
                }
            }

            return true;
        }

        public static void LoadFromDatabase([JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<Affordance> result,
            [JetBrains.Annotations.NotNull] string connectionString,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<TimeBasedProfile> pTimeProfiles,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<DeviceCategory> deviceCategories,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<RealDevice> devices,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<Desire> desires,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<SubAffordance> subAffordances,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<VLoadType> loadTypes,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<TimeLimit> timeLimits,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<Location> locations,
            bool ignoreMissingTables, [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<Variable> variables)
        {
            var aic = new AllItemCollections(desires: desires, timeProfiles: pTimeProfiles,
                timeLimits: timeLimits, realDevices: devices, deviceCategories: deviceCategories, loadTypes: loadTypes,
                subAffordances: subAffordances);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var affordanceDevices = new ObservableCollection<AffordanceDevice>();
            AffordanceDevice.LoadFromDatabase(affordanceDevices, connectionString, deviceCategories, devices,
                pTimeProfiles, result, loadTypes, deviceActions, deviceActionGroups, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(affordanceDevices), IsCorrectAffDeviceParent,
                ignoreMissingTables);
            var affDesires = new ObservableCollection<AffordanceDesire>();
            AffordanceDesire.LoadFromDatabase(affDesires, connectionString, desires, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(affDesires), IsCorrectAffDesireParent,
                ignoreMissingTables);
            var affordanceSubAffordances =
                new ObservableCollection<AffordanceSubAffordance>();
            AffordanceSubAffordance.LoadFromDatabase(affordanceSubAffordances, connectionString, result, subAffordances,
                ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(affordanceSubAffordances),
                IsCorrectAffordanceSubAffordanceParent, ignoreMissingTables);
            var affordances = result;
            var standbys = new ObservableCollection<AffordanceStandby>();
            AffordanceStandby.LoadFromDatabase(standbys, connectionString, deviceCategories, devices, result,
                deviceActions, deviceActionGroups, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(standbys), IsCorrectAffordanceStandbyParent,
                ignoreMissingTables);
            var variableOperations =
                new ObservableCollection<AffVariableOperation>();
            AffVariableOperation.LoadFromDatabase(variableOperations, connectionString, ignoreMissingTables, locations,
                variables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(variableOperations),
                IsCorrectAffordanceVariableParent, ignoreMissingTables);

            var variableRequirements =
                new ObservableCollection<AffVariableRequirement>();
            AffVariableRequirement.LoadFromDatabase(variableRequirements, connectionString, ignoreMissingTables,
                locations, variables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(variableRequirements),
                IsCorrectAffordanceVariableRequiredParent, ignoreMissingTables);

            affordances.Sort();
            foreach (var affordance in affordances) {
                affordance.AffordanceDevices.Sort();
                affordance.AffordanceDesires.Sort();
                affordance.SubAffordances.Sort();
            }
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var affordanceDesire in AffordanceDesires) {
                affordanceDesire.SaveToDB();
            }

            foreach (var affordanceDevice in AffordanceDevices) {
                affordanceDevice.SaveToDB();
            }

            foreach (var affordanceSubAffordance in _subAffordances) {
                affordanceSubAffordance.SaveToDB();
            }

            foreach (var standby in _affStandby) {
                standby.SaveToDB();
            }

            foreach (var affVariable in _executedVariables) {
                affVariable.SaveToDB();
            }

            foreach (var requirement in _requiredVariables) {
                requirement.SaveToDB();
            }
        }

        public override string ToString() => Name;
        public override DBBase ImportFromGenericItem(DBBase toImport,  Simulator dstSim)
            => ImportFromItem((Affordance) toImport,dstSim);

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        protected override void SetSqlParameters(Command cmd)
        {
            if (Name == null) {
                Name = "(no name)";
            }

            cmd.AddParameter("Name", "@myname", Name);
            if (_personProfile != null) {
                cmd.AddParameter("PersonProfileID", "@PersonProfileID", _personProfile.IntID);
            }

            cmd.AddParameter("RandomDesireResults", "@RandomDesireResults", RandomDesireResults);
            cmd.AddParameter("NeedsLight", "@NeedsLight", _needsLight);
            cmd.AddParameter("MinimumAge", "@MinimumAge", _minimumAge);
            cmd.AddParameter("MaximumAge", "@MaximumAge", _maximumAge);
            cmd.AddParameter("PermittedGender", "@PermittedGender", _permittedGender);
            cmd.AddParameter("TimeStandardDeviation", "@TimeStandardDeviation", _timeStandardDeviation);
            cmd.AddParameter("CarpetPlotColor1", "@CarpetPlotColor1", _carpetPlotColor.A);
            cmd.AddParameter("CarpetPlotColor2", "@CarpetPlotColor2", _carpetPlotColor.R);
            cmd.AddParameter("CarpetPlotColor3", "@CarpetPlotColor3", _carpetPlotColor.G);
            cmd.AddParameter("CarpetPlotColor4", "@CarpetPlotColor4", _carpetPlotColor.B);
            cmd.AddParameter("BodilyActivityLevel", _bodilyActivityLevel);
            if (_affCategory != null) {
                cmd.AddParameter("AffCategory", _affCategory);
            }

            if (_description != null) {
                cmd.AddParameter("Description", _description);
            }

            cmd.AddParameter("IsInterruptable", _isInterruptable);
            cmd.AddParameter("IsInterrupting", _isInterrupting);
            cmd.AddParameter("ActionAfterInterruption", _actionAfterInterruption);
            if (_timeLimit != null) {
                cmd.AddParameter("TimeLimitID", _timeLimit.IntID);
                cmd.AddParameter("RequireAllDesires", _requireAllDesires);
            }
        }

        private static bool CheckAssignableDevice([JetBrains.Annotations.NotNull] [ItemNotNull] List<IAssignableDevice> devicesAtLoc,
            [JetBrains.Annotations.NotNull] AffordanceDevice affordanceDevice,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<DeviceAction> allActions)
        {
            var dev = affordanceDevice.Device;
            if (dev == null) {
                return false;
            }

            switch (dev.AssignableDeviceType) {
                case AssignableDeviceType.Device:
                    // real device itself is the simplest case: either it is there or not
                    if (devicesAtLoc.Contains(dev)) {
                        return true;
                    }

                    return false;
                case AssignableDeviceType.DeviceCategory: {
                    // device category is more tricky. Either the device category is there or
                    // one of the devices in the device category
                    if (devicesAtLoc.Contains(dev)) // the device category is not there.. check all the devices
                    {
                        return true;
                    }

                    var dc = (DeviceCategory) dev;
                    var foundAnyRealDevice = dc.SubDevicesWithoutRefresh.Intersect(devicesAtLoc).Any();
                    if (foundAnyRealDevice) {
                        return true;
                    }

                    return false;
                }
                case AssignableDeviceType.DeviceAction:
                    // device action depends only on the device or the device action. if neither is there, no affordance
                    var da = (DeviceAction) dev;
                    if (devicesAtLoc.Contains(da.Device) || devicesAtLoc.Contains(da)) {
                        return true;
                    }

                    return false;
                case AssignableDeviceType.DeviceActionGroup: {
                    // device action group needs either the device action group, one
                    // of the device actions or a single device
                    var dag = (DeviceActionGroup) dev;
                    if (devicesAtLoc.Contains(dag)) // first check for the group
                    {
                        return true;
                    }

                    // check for one of the device  actions
                    var foundAnyDeviceAction = dag.GetDeviceActions(allActions).Intersect(devicesAtLoc).Any();
                    if (foundAnyDeviceAction) {
                        return true;
                    }

                    var devicesinDeviceActions =
                        dag.GetDeviceActions(allActions).Select(myDev => myDev.Device);
                    // check the real devices next
                    var foundAnyRealDevice = devicesinDeviceActions.Intersect(devicesAtLoc).Any();
                    if (foundAnyRealDevice) {
                        return true;
                    }

                    // check for device categories
                    // first see if there is a single device category
                    DeviceCategory dc = null;
                    foreach (var deviceAction in dag.GetDeviceActions(allActions)) {
                        if (deviceAction.Device?.DeviceCategory != null) {
                            if (dc == null) {
                                dc = deviceAction.Device.DeviceCategory;
                            }
                        }

                        if (deviceAction.Device != null) {
                            if (dc != deviceAction.Device.DeviceCategory) {
                                return false; // different device categories -> no go
                            }
                        }
                    }

                    if (devicesAtLoc.Contains(dc)) {
                        return true;
                    }

                    return false;
                }
                default: throw new LPGException("unknown assignable device type. This is a bug. Please report!");
            }
        }

        private void ColorChanged(bool save)
        {
            OnPropertyChanged(nameof(CarpetPlotColor));
            //OnPropertyChanged(nameof(CarpetPlotBrush));
            OnPropertyChanged(nameof(Red));
            OnPropertyChanged(nameof(Green));
            OnPropertyChanged(nameof(Blue));
            if (save) {
                SaveToDB();
            }
        }

        private static bool IsCorrectAffDesireParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (AffordanceDesire) child;
            if (parent.ID == hd.AffordanceID) {
                var aff = (Affordance) parent;
                aff.AffordanceDesires.Add(hd);
                hd.DeleteThis = aff.DeleteAffordanceDesireFromDB;
                return true;
            }

            return false;
        }

        private static bool IsCorrectAffDeviceParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (AffordanceDevice) child;
            if (parent == hd.ParentAffordance) {
                var aff = (Affordance) parent;
                aff.AffordanceDevices.Add(hd);
                return true;
            }

            return false;
        }

        private static bool IsCorrectAffordanceStandbyParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (AffordanceStandby) child;
            if (parent.ID == hd.AffordanceID) {
                var aff = (Affordance) parent;
                aff.AffordanceStandbys.Add(hd);
                return true;
            }

            return false;
        }

        private static bool IsCorrectAffordanceSubAffordanceParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (AffordanceSubAffordance) child;
            if (parent.ID == hd.AffordanceID) {
                var aff = (Affordance) parent;
                aff.SubAffordances.Add(hd);
                return true;
            }

            return false;
        }

        private static bool IsCorrectAffordanceVariableParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (AffVariableOperation) child;
            if (parent.ID == hd.AffordanceID) {
                var aff = (Affordance) parent;
                aff._executedVariables.Add(hd);
                return true;
            }

            return false;
        }

        private static bool IsCorrectAffordanceVariableRequiredParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (AffVariableRequirement) child;
            if (parent.ID == hd.AffordanceID) {
                var aff = (Affordance) parent;
                aff._requiredVariables.Add(hd);
                return true;
            }

            return false;
        }
    }
}