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

#region

using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Transportation;
using Common;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.OnlineLogging {
    public interface ILogFile: IDisposable {
        [CanBeNull]
        DesiresLogFile DesiresLogfile { get; }

        //HashSet<string> HouseholdKeys { get; }
        [CanBeNull]
        EnergyStorageLogfile EnergyStorageLogfile { get; }

        //[NotNull] FileFactoryAndTracker FileFactoryAndTracker { get; }

        [CanBeNull]
        IThoughtsLogFile ThoughtsLogFile1 { get; }

        //ActionLogFile ActionLogFile { get; }
        //[CanBeNull]LocationsLogFile LocationsLogFile { get; }

        //[CanBeNull]TransportationLogFile TransportationLogFile { get; }
    }

    public interface IOnlineLoggingData: IDisposable {
        void AddActionEntry([NotNull] TimeStep timeStep, [NotNull] StrGuid personGuid, [NotNull] string personName, bool isSick,
                            [NotNull] string affordanceName,
                            [NotNull] StrGuid affordanceGuid, [NotNull] HouseholdKey householdKey,
                            [NotNull] string affordanceCategory, BodilyActivityLevel bodilyActivityLevel);

        void AddColumnEntry([NotNull] ColumnEntry ce);
        void AddLocationEntry([NotNull] LocationEntry le);

        void AddTransportationEvent([NotNull] HouseholdKey householdkey,
                                    [NotNull] string person, [NotNull] TimeStep timestep,
                                    [NotNull] string srcSite,
                                    [NotNull] string dstSite,
                                    [NotNull] string route,
                                    [NotNull] string transportationDevice,
                                    int transportationDuration,
                                    int affordanceDuration,
                                    [NotNull] string affordanceName,
                                    [NotNull] [ItemNotNull]
                                    List<CalcTravelRoute.CalcTravelDeviceUseEvent> travelDeviceUseEvents);

        void AddTransportationStatus([NotNull] TransportationStatus transportationStatus);

        void FinalSaveToDatabase();
        void SaveIfNeeded([NotNull] TimeStep timestep);
        void RegisterDeviceActivation([NotNull] DeviceActivationEntry affordanceActivationEntry);
        void AddPersonStatus([NotNull] PersonStatus ps);
        void AddTransportationDeviceState([NotNull] TransportationDeviceStateEntry tdse);
        void AddChargingStationState([NotNull] ChargingStationState state);
        void AddVariableStatus([NotNull] CalcVariableEntry calcVariableEntry);
    }

    public class OnlineLoggingData : IOnlineLoggingData {
        [NotNull] [ItemNotNull] private readonly List<ActionEntry> _actionEntries = new List<ActionEntry>();

        [NotNull] [ItemNotNull] private readonly List<DeviceActivationEntry> _affordanceActivationEntries;

        [ItemNotNull] [NotNull] private readonly List<ColumnEntry> _columnEntries;

        [NotNull] private readonly DateStampCreator _dsc;

        [NotNull] private readonly IInputDataLogger _idl;
        [NotNull] private readonly CalcParameters _calcParameters;

        [ItemNotNull] [NotNull] private readonly List<LocationEntry> _locationEntries;
        [ItemNotNull] [NotNull] private readonly List<PersonStatus> _personStatus;

        [ItemNotNull] [NotNull] private readonly List<TransportationEventEntry> _transportationEvents;
        [ItemNotNull] [NotNull] private readonly List<TransportationStatus> _transportationStatuses;
        [ItemNotNull] [NotNull] private readonly List<TransportationDeviceStateEntry> _transportationDeviceState;
        [ItemNotNull] [NotNull] private readonly List<ChargingStationState> _chargingStationStates;
        [ItemNotNull] [NotNull] private readonly List<CalcVariableEntry> _variableEntries;
        [ItemNotNull] [NotNull] private readonly List<dynamic> _lists = new List<dynamic>();
        public OnlineLoggingData([NotNull] DateStampCreator dsc, [NotNull] IInputDataLogger idl,
                                 [NotNull] CalcParameters calcParameters)
        {
            _dsc = dsc;
            _idl = idl;
            _calcParameters = calcParameters;
            _columnEntries = new List<ColumnEntry>();
            _lists.Add(_columnEntries);
            _affordanceActivationEntries = new List<DeviceActivationEntry>();
            _lists.Add(_affordanceActivationEntries);
            _transportationStatuses = new List<TransportationStatus>();
            _lists.Add(_transportationStatuses);
            _transportationDeviceState = new List<TransportationDeviceStateEntry>();
            _lists.Add(_transportationDeviceState);
            _transportationEvents = new List<TransportationEventEntry>();
            _lists.Add(_transportationEvents);
            _locationEntries = new List<LocationEntry>();
            _lists.Add(_locationEntries);
            _personStatus = new List<PersonStatus>();
            _lists.Add(_personStatus);
            _chargingStationStates = new List<ChargingStationState>();
            _lists.Add(_chargingStationStates);
            _variableEntries = new List<CalcVariableEntry>();
            _lists.Add(_variableEntries);
        }

        public void AddTransportationDeviceState(TransportationDeviceStateEntry tdse)
        {
            if (_calcParameters.Options.Contains(CalcOption.TransportationStatistics)) {
                _transportationDeviceState.Add(tdse);
            }
        }

        public void AddActionEntry([NotNull] TimeStep timeStep, [NotNull] StrGuid personGuid, [NotNull] string personName,
                                   bool isSick, [NotNull] string affordanceName, [NotNull] StrGuid affordanceGuid,
                                   [NotNull] HouseholdKey householdKey, [NotNull]
                                   string affordanceCategory, BodilyActivityLevel bodilyActivityLevel)
        {
            if (!timeStep.DisplayThisStep) {
                return;
            }
            ActionEntry ae = ActionEntry.MakeActionEntry(timeStep,
                personGuid, personName, isSick, affordanceName, affordanceGuid,
                householdKey, affordanceCategory, _dsc.MakeDateFromTimeStep(timeStep), bodilyActivityLevel);
            _actionEntries.Add(ae);
        }

        public void AddColumnEntry(ColumnEntry ce)
        {
            _columnEntries.Add(ce);
        }

        public void AddPersonStatus(PersonStatus ps)
        {
            if (!_calcParameters.IsSet(CalcOption.PersonStatus)) {
                return;
            }

            _personStatus.Add(ps);
        }
        public void AddLocationEntry(LocationEntry le)
        {
            _locationEntries.Add(le);
        }

        public void AddTransportationEvent([NotNull] HouseholdKey householdkey,
                                           [NotNull] string person, [NotNull] TimeStep timestep,
                                           [NotNull] string srcSite,
                                           [NotNull] string dstSite,
                                           [NotNull] string route,
                                           [NotNull] string transportationDevice,
                                           int transportationDuration,
                                           int affordanceDuration,
                                           [NotNull] string affordanceName,
                                           [NotNull] [ItemNotNull] List<CalcTravelRoute.CalcTravelDeviceUseEvent> travelDeviceUseEvents
            )
        {
            if (!_calcParameters.Options.Contains(CalcOption.TransportationStatistics)) {
                return;
            }

            //put all the individual travel device uses in as events
                if (transportationDuration > 0) {
                TimeStep mytimestep = timestep;
                foreach (CalcTravelRoute.CalcTravelDeviceUseEvent calcTravelDeviceUseEvent in travelDeviceUseEvents) {
                    string description = route + " with " + transportationDevice + " for " + transportationDuration +
                                         " steps to " +
                                         affordanceName + " - " + calcTravelDeviceUseEvent.Device.Name ;

                    TransportationEventEntry tee = new TransportationEventEntry(householdkey, person, mytimestep,
                        srcSite,dstSite,
                        CurrentActivity.InTransport,description, calcTravelDeviceUseEvent.Device.Name,
                        transportationDuration, calcTravelDeviceUseEvent.TotalDistance);
                    _transportationEvents.Add(tee);
                    mytimestep = mytimestep.AddSteps(calcTravelDeviceUseEvent.DurationInSteps);
                }
            }

            //write the arrival
            TransportationEventEntry tee2 = new TransportationEventEntry(householdkey, person,
                timestep.AddSteps(transportationDuration),
                dstSite,dstSite,
                CurrentActivity.InAffordance, affordanceName, string.Empty,
                0,0);
            _transportationEvents.Add(tee2);
        }

        public void AddTransportationStatus(TransportationStatus transportationStatus)
        {
            if (_calcParameters.Options.Contains(CalcOption.TransportationStatistics)) {
                _transportationStatuses.Add(transportationStatus);
            }
        }

        public void SaveIfNeeded([NotNull] TimeStep timestep)
        {
            if (timestep.InternalStep % 1000 != 0) {
                return;
            }

            int totalCount = 0;
            foreach (dynamic list in _lists) {
                totalCount += list.Count;
            }

            if (totalCount < 30000) {
                return;
            }

            //TODO: only every x timesteps and only if count > 1000
            FinalSaveToDatabase();
        }

        public void FinalSaveToDatabase()
        {
            if (_actionEntries.Count > 0) {
                _idl.SaveList(_actionEntries.ConvertAll(x => (IHouseholdKey)x));
                _actionEntries.Clear();
            }

            if (_affordanceActivationEntries.Count > 0) {
                _idl.SaveList(_affordanceActivationEntries.ConvertAll(x => (IHouseholdKey)x));
                _affordanceActivationEntries.Clear();
            }

            if (_columnEntries.Count > 0) {
                _idl.Save(_columnEntries);
                _columnEntries.Clear();
            }

            if (_locationEntries.Count > 0) {
                _idl.SaveList(_locationEntries.ConvertAll(x => (IHouseholdKey)x));
                _locationEntries.Clear();
            }

            if (_transportationStatuses.Count > 0) {
                _idl.SaveList(_transportationStatuses.ConvertAll(x => (IHouseholdKey)x));
                _transportationStatuses.Clear();
            }

            if (_transportationEvents.Count > 0) {
                _idl.SaveList(_transportationEvents.ConvertAll(x => (IHouseholdKey)x));
                _transportationEvents.Clear();
            }
            if (_personStatus.Count > 0)
            {
                _idl.SaveList(_personStatus.ConvertAll(x => (IHouseholdKey)x));
                _personStatus.Clear();
            }
            if (_transportationDeviceState.Count > 0)
            {
                _idl.SaveList(_transportationDeviceState.ConvertAll(x => (IHouseholdKey)x));
                _transportationDeviceState.Clear();
            }

            if (_chargingStationStates.Count > 0)
            {
                _idl.SaveList(_chargingStationStates.ConvertAll(x => (IHouseholdKey)x));
                _chargingStationStates.Clear();
            }
            if (_variableEntries.Count > 0)
            {
                _idl.SaveList(_variableEntries.ConvertAll(x => (IHouseholdKey)x));
                _variableEntries.Clear();
            }
        }

        public void RegisterDeviceActivation(DeviceActivationEntry affordanceActivationEntry)
        {
            _affordanceActivationEntries.Add(affordanceActivationEntry);
        }

        public void AddChargingStationState(ChargingStationState state)
        {
            if (_calcParameters.Options.Contains(CalcOption.TransportationStatistics)) {
                _chargingStationStates.Add(state);
            }
        }

        public void AddVariableStatus(CalcVariableEntry calcVariableEntry)
        {
            _variableEntries.Add(calcVariableEntry);
        }

        public void Dispose()
        {
            FinalSaveToDatabase();
        }
    }

    public sealed class LogFile :  ILogFile {
        [CanBeNull] private readonly DesiresLogFile _desiresLogfile;

        [CanBeNull] private readonly EnergyStorageLogfile _energyStorageLogfile;


        [CanBeNull] private readonly IThoughtsLogFile _thoughtsLogFile;

        public LogFile([NotNull] CalcParameters calcParameters,
                       FileFactoryAndTracker fft,
                       bool writeToConsole = false
                       )
        {
            if (calcParameters.IsSet(CalcOption.ThoughtsLogfile)) {
                if (writeToConsole) {
                    _thoughtsLogFile = new ConsoleThoughts();
                }
                else {
                    _thoughtsLogFile = new ThoughtsLogFile(fft, calcParameters);
                }
            }

            if (calcParameters.IsSet(CalcOption.DesiresLogfile)) {
                _desiresLogfile = new DesiresLogFile(fft,  calcParameters);
            }

            if (calcParameters.IsSet(CalcOption.EnergyStorageFile)) {
                _energyStorageLogfile = new EnergyStorageLogfile(calcParameters, fft);
            }

            //_transportationLogFile = new TransportationLogFile(_fft,_calcParameters);
        }

        //[CanBeNull]public TransportationLogFile TransportationLogFile => _transportationLogFile;

        //[CanBeNull]private readonly TransportationLogFile _transportationLogFile;


        public void Dispose()
        {
            //ActionLogFile?.Close();
            _thoughtsLogFile?.Dispose();
            //LocationsLogFile?.Close();
            _desiresLogfile?.Dispose();
            //_transportationLogFile?.Close();
            _energyStorageLogfile?.Dispose();
        }

        [CanBeNull]
        public DesiresLogFile DesiresLogfile => _desiresLogfile;

        [CanBeNull]
        public EnergyStorageLogfile EnergyStorageLogfile => _energyStorageLogfile;


        //public void InitHousehold([NotNull] HouseholdKey householdKey, [NotNull] string name, HouseholdKeyType type, string description,
        //                          [CanBeNull] string housename, string houseDescription)
        //{

        //    /*if (ActionLogFile == null) {
        //        ActionLogFile = new ActionLogFile(_fft, householdKey,_calcParameters);
        //    }
        //    else {
        //        ActionLogFile.AddHousehold(householdKey);
        //    }*/
        //    //LocationsLogFile = new LocationsLogFile(_displayNegativeTime, _fft, _calcParameters);
        //    //TransportationLogFile?.InitSw(householdKey);
        //}


        //public HashSet<string> HouseholdKeys => _householdKeys;

        //[CanBeNull]public LocationsLogFile LocationsLogFile { get; private set; }
        [CanBeNull]
        public IThoughtsLogFile ThoughtsLogFile1 => _thoughtsLogFile;
    }
}