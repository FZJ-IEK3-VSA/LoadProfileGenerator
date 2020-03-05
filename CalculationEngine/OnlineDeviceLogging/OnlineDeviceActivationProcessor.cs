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
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.OnlineDeviceLogging {
    using Common.SQLResultLogging.Loggers;

    public class SetToZeroEntry {
        public SetToZeroEntry([NotNull] TimeStep startTime, [NotNull] TimeStep endTime, ZeroEntryKey key) {
            StartTime = startTime;
            EndTime = endTime;
            Key = key;
        }

        [NotNull]
        public TimeStep EndTime { get; }

        public ZeroEntryKey Key { get; }
        [NotNull]
        public TimeStep StartTime { get; }
    }

    public interface IOnlineDeviceActivationProcessor
    {
        void RegisterDevice([NotNull] string deviceName, OefcKey key, [NotNull] string locationName, [NotNull] CalcLoadTypeDto loadType);

        void AddNewStateMachine([NotNull] CalcProfile calcProfile, [NotNull] TimeStep startTimeStep,
            double powerStandardDeviation, double powerUsage, [NotNull] string deviceName,
                                [NotNull] CalcLoadTypeDto loadType, [NotNull] string affordanceName, [NotNull] string activatorName, [NotNull] string profileName,
            [NotNull] string profileSource, OefcKey oefckey);

        void AddZeroEntryForAutoDev([NotNull] HouseholdKey householdKey, OefcDeviceType deviceType, [NotNull] string deviceGuid,
                                    [NotNull] string locationGuid, [NotNull] TimeStep starttime, int totalDuration);

        [NotNull]
        OnlineEnergyFileColumns Oefc { get; }

        [NotNull]
        [ItemNotNull]
        List<OnlineEnergyFileRow> ProcessOneTimestep([NotNull] TimeStep timeStep);

        [NotNull]
        Dictionary<CalcLoadTypeDto, BinaryWriter> BinaryOutStreams { get; }
        [NotNull]
        Dictionary<CalcLoadTypeDto, BinaryWriter> SumBinaryOutStreams { get; }
        [NotNull]
        Dictionary<ProfileActivationEntry.ProfileActivationEntryKey, ProfileActivationEntry> ProfileEntries { get; }
    }
    public class OnlineDeviceActivationProcessor : IOnlineDeviceActivationProcessor {
        [NotNull]
        private readonly Dictionary<CalcLoadTypeDto, BinaryWriter> _binaryOutStreams;
        [NotNull]
        private readonly FileFactoryAndTracker _fft;
        [NotNull]
        private readonly ILogFile _lf;
        [NotNull]
        private readonly CalcParameters _calcParameters;
        [NotNull]
        private readonly Dictionary<CalcLoadTypeDto, int> _loadTypeDict;
        [NotNull]
        private readonly NormalRandom _nr;
        [NotNull]
        private readonly Dictionary<ProfileActivationEntry.ProfileActivationEntryKey, ProfileActivationEntry>
            _profileEntries =
                new Dictionary<ProfileActivationEntry.ProfileActivationEntryKey, ProfileActivationEntry>();
        [NotNull]
        private readonly Dictionary<CalcLoadTypeDto, BinaryWriter> _sumBinaryOutStreams;
        [ItemNotNull]
        [NotNull]
        private readonly List<SetToZeroEntry> _zeroEntries = new List<SetToZeroEntry>();
        [ItemNotNull]
        [NotNull]
        private List<OnlineDeviceStateMachine> _statemachines = new List<OnlineDeviceStateMachine>();

        public OnlineDeviceActivationProcessor([NotNull] NormalRandom nr, [NotNull] ILogFile lf, [NotNull] CalcParameters calcParameters) {
            Oefc = new OnlineEnergyFileColumns(lf);
            _loadTypeDict = new Dictionary<CalcLoadTypeDto, int>();
            _fft = lf.FileFactoryAndTracker;
            _binaryOutStreams = new Dictionary<CalcLoadTypeDto, BinaryWriter>();
            _sumBinaryOutStreams = new Dictionary<CalcLoadTypeDto, BinaryWriter>();
            _nr = nr;
            _lf = lf;
            _calcParameters = calcParameters;
            Logger.Info("Initializing the online device activation processor...");
        }

        [NotNull]
        public Dictionary<CalcLoadTypeDto, BinaryWriter> BinaryOutStreams => _binaryOutStreams;

        [NotNull]
        public OnlineEnergyFileColumns Oefc { get; }

        [NotNull]
        public Dictionary<ProfileActivationEntry.ProfileActivationEntryKey, ProfileActivationEntry> ProfileEntries
            => _profileEntries;

        [NotNull]
        public Dictionary<CalcLoadTypeDto, BinaryWriter> SumBinaryOutStreams => _sumBinaryOutStreams;

        public void AddNewStateMachine([NotNull] CalcProfile calcProfile, TimeStep startTimeStep,
                                       double powerStandardDeviation, double powerUsage, [NotNull] string deviceName,
                                       [NotNull] CalcLoadTypeDto loadType, [NotNull] string affordanceName, [NotNull] string activatorName, [NotNull] string profileName,
                                       [NotNull] string profileSource,  OefcKey oefckey)
        {
            Oefc.IsDeviceRegistered(loadType, oefckey);
            //OefcKey oefckey = new OefcKey(householdKey, deviceType, deviceID, locationID, loadType.ID);
            // this is for logging the used time profiles which gets dumped to the time profile log
            ProfileActivationEntry.ProfileActivationEntryKey key =
                new ProfileActivationEntry.ProfileActivationEntryKey(deviceName, profileName, profileSource,
                    loadType.Name);
            if (!_profileEntries.ContainsKey(key))
            {
                ProfileActivationEntry entry = new ProfileActivationEntry(deviceName, profileName, profileSource,
                    loadType.Name,_calcParameters);
                _profileEntries.Add(entry.GenerateKey(), entry);
            }
            _profileEntries[key].ActivationCount++;
            // do the device activiation
            var dsm = new OnlineDeviceStateMachine(calcProfile, startTimeStep,
                powerStandardDeviation, powerUsage, _nr, loadType, deviceName, oefckey, affordanceName, _calcParameters);
            _statemachines.Add(dsm);

            // log the affordance energy use.
            if (_calcParameters.IsSet(CalcOption.AffordanceEnergyUse) || _calcParameters.IsSet(CalcOption.TotalsPerDevice)) {
                double totalPowerSum = dsm.CalculateOfficialEnergyUse();
                double totalEnergysum = loadType.ConversionFactor * totalPowerSum;
                var entry = new DeviceActivationEntry(dsm.HouseholdKey, dsm.AffordanceName,
                    dsm.LoadType,totalEnergysum , activatorName,deviceName,calcProfile.StepValues.Count); // dsm.StepValues.ToArray(),
                _lf.OnlineLoggingData.RegisterDeviceActivation(entry);
            }
        }

        public void AddZeroEntryForAutoDev([NotNull] HouseholdKey householdKey, OefcDeviceType deviceType, [NotNull] string deviceGuid, [NotNull] string locationGuid,
            [NotNull] TimeStep starttime, int totalDuration) {
            var zeKey = new ZeroEntryKey(householdKey, deviceType, deviceGuid, locationGuid);
            var stze = new SetToZeroEntry(starttime, starttime.AddSteps( totalDuration), zeKey);
            _zeroEntries.Add(stze);
        }

        private static void CleanExpiredStateMachines([NotNull] TimeStep timestep, [NotNull][ItemNotNull] ref List<OnlineDeviceStateMachine> statemachines) {
            // alte state machines entsorgen
            var newstatemachinelist = new List<OnlineDeviceStateMachine>();
            foreach (var machine in statemachines) {
                if (!machine.IsExpired(timestep)) {
                    newstatemachinelist.Add(machine);
                }
            }
            statemachines = newstatemachinelist;
        }

        private void CleanZeroValueEntries([NotNull] TimeStep currentTime)
        {
            TimeStep nextStep = currentTime.AddSteps(1);
            var items2Delete = _zeroEntries.Where(x => x.EndTime < nextStep).ToList();
            foreach (var entry in items2Delete) {
                _zeroEntries.Remove(entry);
            }
        }

        [NotNull]
        [ItemNotNull]
        public List<OnlineEnergyFileRow> ProcessOneTimestep([NotNull] TimeStep timeStep) {
            CleanExpiredStateMachines(timeStep, ref _statemachines);
            var fileRows = new List<OnlineEnergyFileRow>();
            var procesedMachines = new List<OnlineDeviceStateMachine>();
            foreach (OnlineDeviceStateMachine stateMachine in _statemachines) {
                if (!_loadTypeDict.ContainsKey(stateMachine.LoadType)) {
                    throw new LPGException("Found a state machine for a load type that does not exist: " + stateMachine.DeviceKey + ": " + stateMachine.LoadType);
                }
            }
            foreach (var loadType in _loadTypeDict.Keys) {
                var energyvalues = new List<double>(new double[Oefc.ColumnCountByLoadType[loadType]]);
                var columnEntriesDeviceKey = Oefc.ColumnEntriesByLoadTypeByDeviceKey[loadType];
                foreach (var machine in _statemachines) {
                    if (columnEntriesDeviceKey.ContainsKey(machine.DeviceKey)) {
                        energyvalues[Oefc.GetColumnNumber(loadType, machine.DeviceKey)] +=
                            machine.GetEnergyValueForTimeStep(timeStep, loadType, _zeroEntries);
                        if (Config.ExtraUnitTestChecking) {
                            procesedMachines.Add(machine);
                        }
                    }
                }
                var fileRow = new OnlineEnergyFileRow(timeStep, energyvalues, loadType);
                fileRows.Add(fileRow);
            }
            if (Config.ExtraUnitTestChecking) {
                if (procesedMachines.Count != _statemachines.Count) {
                    var nonprocessed =
                        _statemachines.Where(x => !procesedMachines.Contains(x)).ToList();
                    throw new LPGException("Not all machines were processed! Processed:" + procesedMachines.Count +
                                           " Total:" + _statemachines.Count + Environment.NewLine + nonprocessed);
                }
            }
            CleanZeroValueEntries(timeStep);
            return fileRows;
        }

        public void RegisterDevice([NotNull] string deviceName, OefcKey key, [NotNull] string locationName,
                                   [NotNull] CalcLoadTypeDto loadType) {
            if(key.LoadtypeGuid != loadType.Guid && key.LoadtypeGuid != "-1") {
                throw new LPGException("bug: loadtype id was wrong while registering a device");
            }

            Oefc.AddColumnEntry(deviceName, key, locationName, loadType, key.DeviceGuid, key.HouseholdKey, key.DeviceCategory);

            if (!_loadTypeDict.ContainsKey(loadType)) {
                _loadTypeDict.Add(loadType, 1);
                if (_calcParameters.IsSet(CalcOption.DetailedDatFiles)) {
                    var s = _fft.MakeFile<BinaryWriter>("OnlineDeviceEnergyUsage." + loadType.Name + ".dat",
                        "Binary Device energy usage per device for " + loadType.Name, false,
                        ResultFileID.OnlineDeviceActivationFiles, Constants.GeneralHouseholdKey, TargetDirectory.Temporary,
                        _calcParameters.InternalStepsize, loadType.ConvertToLoadTypeInformation());
                    _binaryOutStreams.Add(loadType, s);
                }
                if (_calcParameters.IsSet(CalcOption.OverallDats) ||
                    _calcParameters.IsSet(CalcOption.OverallSum)) {
                    var binaryWriter =
                        _fft.MakeFile<BinaryWriter>("OnlineDeviceEnergyUsage.Sums." + loadType.Name + ".dat",
                            "Binary Device summed energy usage per device for " + loadType.Name, false,
                            ResultFileID.OnlineSumActivationFiles, Constants.GeneralHouseholdKey, TargetDirectory.Temporary, _calcParameters.InternalStepsize,
                            loadType.ConvertToLoadTypeInformation());
                    _sumBinaryOutStreams.Add(loadType, binaryWriter);
                }
            }
        }
    }
}