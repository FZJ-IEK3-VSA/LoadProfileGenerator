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
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Helper;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using CalculationEngine.Transportation;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.HouseholdElements {
    public sealed class CalcHousehold : CalcBase, ICalcAbleObject {
        [NotNull] private readonly CalcParameters _calcParameters;

        [NotNull] private readonly CalcVariableRepository _calcVariableRepository;

        //[NotNull] private readonly string _cleanName;

        [NotNull] private readonly HouseholdKey _householdKey;

        [NotNull] private readonly string _locationname;

        [ItemNotNull] [NotNull] private readonly List<CalcLocation> _locations;

        [NotNull] private readonly string _name;

        [ItemNotNull] [NotNull] private readonly List<CalcPerson> _persons;

        [NotNull] private readonly string _temperatureprofileName;

        //private readonly VariableOperator _variableOperator = new VariableOperator();

        [ItemNotNull] [CanBeNull] private List<CalcAutoDev> _autoDevs;

        [CanBeNull] private DayLightStatus _daylightArray;

        [ItemNotNull] [CanBeNull] private List<CalcDevice> _devices;

        private DateTime _lastDisplay = DateTime.MinValue;
        private DateTime _startSimulation = DateTime.MinValue;

        [CanBeNull] private ILogFile _lf;
        [CanBeNull] private NormalRandom _normalDistributedRandom;

        [CanBeNull] private IOnlineDeviceActivationProcessor _odap;
        [CanBeNull] private Random _randomGenerator;

        private int _simulationSeed;
        [NotNull] private readonly string _description;

        //[CanBeNull] private VariableLogfile _variableLogfile;

        public CalcHousehold([NotNull] string pName, [NotNull] string locationname,
                             [NotNull] string temperatureprofileName, [NotNull] HouseholdKey householdkey,
                             [NotNull] CalcParameters calcParameters, [NotNull] string guid,
                             [NotNull] CalcVariableRepository calcVariableRepository,
                             [NotNull] SqlResultLoggingService srls,
                             [ItemNotNull] [NotNull] List<CalcLocation> locations,
                             [ItemNotNull] [NotNull] List<CalcPerson> persons, [NotNull] string description) : base(pName,  guid)
        {
            Srls = srls;
            _locations = locations;
            _persons = persons;
            _description = description;
            //_cleanName = AutomationUtili.CleanFileName(pName);
            _name = pName;
            _locationname = locationname;
            _temperatureprofileName = temperatureprofileName;
            _householdKey = householdkey;
            _calcParameters = calcParameters;
            _calcVariableRepository = calcVariableRepository;
        }

        //[NotNull]public List<DateTime> BridgeDays { get; set; }

#pragma warning disable CC0017 // Use auto property

        [NotNull]
        [ItemNotNull]
        public List<CalcLocation> Locations => _locations;

#pragma warning restore CC0017 // Use auto property

#pragma warning disable CC0017 // Use auto property

        [NotNull]
        [ItemNotNull]
        public List<CalcPerson> Persons => _persons;

#pragma warning restore CC0017 // Use auto property
        [NotNull]
        public SqlResultLoggingService Srls { get; }

        [CanBeNull]
        public TransportationHandler TransportationHandler { get; set; }

        public void CloseLogfile()
        {
            _lf?.Close();
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcAutoDev> CollectAutoDevs()
        {
            if (_autoDevs == null) {
                throw new LPGException("Autodevs should not be null");
            }

            return _autoDevs;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcDevice> CollectDevices()
        {
            if (_devices == null) {
                throw new LPGException("_devices should not be null");
            }

            return _devices;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcLocation> CollectLocations()
        {
            if (_locations == null) {
                throw new LPGException("_locations should not be null");
            }

            return _locations;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcPerson> CollectPersons()
        {
            if (_persons == null) {
                throw new LPGException("missing persons");
            }

            return new List<CalcPerson>(_persons);
        }

        public void DumpHouseholdContentsToText()
        {
            //DumpAffordanceInformation(taggingSets);
            if (_lf == null) {
                throw new LPGException("lf was null");
            }

            if (_persons == null) {
                throw new LPGException("Persons was null");
            }

            if (_locations == null) {
                throw new LPGException("_locations was null");
            }

            if (_devices == null) {
                throw new LPGException("_devices was null");
            }

            if (_autoDevs == null) {
                throw new LPGException("_autoDevs was null");
            }

            using (var swPerson =
                _lf.FileFactoryAndTracker.MakeFile<StreamWriter>("Persons." + _householdKey + ".txt",
                    "Overview of the persons", true, ResultFileID.PersonFile, _householdKey, TargetDirectory.Root,
                    _calcParameters.InternalStepsize)) {
                foreach (var calcPerson in _persons) {
                    swPerson.WriteLine(calcPerson.PrettyName);
                }
            }

            using (var swAff =
                _lf.FileFactoryAndTracker.MakeFile<StreamWriter>("AffordanceDefinition." + _householdKey + ".txt",
                    "Definition of the Affordances", false, ResultFileID.AffordanceDefinition, _householdKey,
                    TargetDirectory.Root, _calcParameters.InternalStepsize)) {
                foreach (var calcLocation in Locations) {
                    swAff.WriteLine(calcLocation.Name + ":");
                    foreach (var affordance in calcLocation.Affordances) {
                        swAff.WriteLine("  " + affordance.PrettyNameForDumping + " (Time limit: " +
                                        affordance.TimeLimitName + ")");
                        if (affordance.Energyprofiles != null) {
                            foreach (var profile in affordance.Energyprofiles) {
                                swAff.WriteLine("    " + profile.CalcDevice.Name + " Offset: " + profile.TimeOffset +
                                                " / " + profile.TimeOffsetInSteps);
                            }
                        }
                    }

                    swAff.WriteLine();
                }
            }

            using (var sw = _lf.FileFactoryAndTracker.MakeFile<StreamWriter>(
                "HouseholdContents." + _householdKey + "."  + ".txt",
                "List of persons, locations, devices and affordances in this household", true, ResultFileID.Dump,
                _householdKey, TargetDirectory.Root, _calcParameters.InternalStepsize)) {
                sw.WriteLine("Name:" + _name);
                sw.WriteLine("Location:" + _locationname);
                sw.WriteLine("Temperatureprofile:" + _temperatureprofileName);
                sw.WriteLine("Persons:");
                foreach (var calcPerson in _persons) {
                    sw.WriteLine(calcPerson.Name);
                    sw.WriteLine("\tDesires:");
                    foreach (var desire in calcPerson.PersonDesires.Desires) {
                        sw.WriteLine("\t\t" + desire.Value.Name);
                    }

                    sw.WriteLine("\tSickness Desires:");
                    foreach (var desire in calcPerson.SicknessDesires.Desires) {
                        sw.WriteLine("\t\t" + desire.Value.Name);
                    }
                }

                sw.WriteLine();
                sw.WriteLine(Environment.NewLine + Environment.NewLine + "Locations:");

                foreach (var calcLocation in _locations) {
                    sw.WriteLine();
                    sw.WriteLine(Environment.NewLine + Environment.NewLine + calcLocation.Name);

                    sw.WriteLine(Environment.NewLine + "\tLight Devices:");
                    if (calcLocation.LightDevices.Count > 0) {
                        foreach (var lightcalcDevice in calcLocation.LightDevices) {
                            sw.WriteLine("\t\t" + lightcalcDevice.PrettyName);
                        }
                    }
                    else {
                        sw.WriteLine("\t\t(None)");
                    }

                    sw.WriteLine(Environment.NewLine + "\tDevices:");
                    foreach (var calcDevice in calcLocation.Devices) {
                        sw.WriteLine("\t\t" + calcDevice.PrettyName);
                    }

                    sw.WriteLine(Environment.NewLine + "\tAffordances (with desires):");
                    foreach (var calcaff in calcLocation.Affordances) {
                        sw.WriteLine("\t\t" + calcaff.Name);
                        foreach (var calcDesire in calcaff.Satisfactionvalues) {
                            sw.WriteLine("\t\t\t" + calcDesire.Name);
                        }
                    }
                }

                sw.WriteLine();
                sw.WriteLine(Environment.NewLine + Environment.NewLine + "All Devices:");
                foreach (var calcdev in _devices) {
                    sw.WriteLine(calcdev.Name);
                }

                sw.WriteLine();
                sw.WriteLine(Environment.NewLine + Environment.NewLine + "Autonomous Devices:");
                var devicenames = new List<string>();
                foreach (var dev in _autoDevs) {
                    devicenames.Add(dev.Name + " @ " + dev.Location + ", load type " + dev.LoadType.Name +
                                    ", profile " + dev.CalcProfile.Name);
                }

                devicenames.Sort();
                foreach (var devicename in devicenames) {
                    sw.WriteLine(devicename);
                }

                if (TransportationHandler != null) {
                    sw.WriteLine();
                    sw.WriteLine(Environment.NewLine + Environment.NewLine + "Transportation:");
                    var transportdevices = new List<string>();
                    foreach (var dev in TransportationHandler.VehicleDepot) {
                        transportdevices.Add(dev.Category + ": " + dev.Name);
                    }

                    foreach (var dev in TransportationHandler.LocationUnlimitedDevices) {
                        transportdevices.Add(dev.Category + ": " + dev.Name);
                    }

                    transportdevices.Sort();
                    foreach (var devicename in transportdevices) {
                        sw.WriteLine(devicename);
                    }
                }
            }
        }

        public void FinishCalculation()
        {
            DumpTimeProfiles();
        }

        [NotNull]
        public HouseholdKey HouseholdKey => _householdKey;

        //public Dictionary<int, CalcProfile> AllProfiles => _allProfiles;

        public void Init([NotNull] ILogFile lf, [NotNull] Random randomGenerator,
                         [NotNull] DayLightStatus daylightArray,
                         [NotNull] NormalRandom normalDistributedRandom,
                         [NotNull] IOnlineDeviceActivationProcessor odap,
                         int simulationSeed)
        {
            _simulationSeed = simulationSeed;
            _lf = lf;
            _randomGenerator = randomGenerator;
            _daylightArray = daylightArray;
            _odap = odap;
            _normalDistributedRandom = normalDistributedRandom;
            if (_lf == null) {
                throw new LPGException("logfile was null");
            }

            if (_persons == null) {
                throw new LPGException("_persons was null");
            }

            _lf.InitHousehold(_householdKey, Name, HouseholdKeyType.Household,_description,null,null);
            //_lf.TransportationLogFile.SetTransportationHandler(TransportationHandler);
            if (_calcParameters.IsSet(CalcOption.DesiresLogfile)) {
                if (_lf.DesiresLogfile == null) {
                    throw new LPGException("Desires logfile was null");
                }

                foreach (var p in _persons) {
                    _lf.DesiresLogfile.RegisterDesires(p.PersonDesires.Desires.Values);
                    _lf.DesiresLogfile.RegisterDesires(p.SicknessDesires.Desires.Values);
                }
            }

            CalcConsistencyCheck.CheckConsistency(this, _calcParameters);
            if (_calcParameters.IsSet(CalcOption.VariableLogFile)) {
                //_variableLogfile = new VariableLogfile(Srls,_calcVariableRepository);
                //_variableLogfile = new VariableLogfile(_lf, _householdKey,_calcParameters);
            }

            if (_autoDevs == null) {
                throw new LPGException("Autodevs should not be null");
            }

            if (_devices == null) {
                throw new LPGException("_devices should not be null");
            }

            MatchAutonomousDevicesWithNormalDevices(_autoDevs, _devices);
            //if (_calcParameters.IsSet(CalcOption.HouseholdContents)) {
            //  SaveBridgeDays();
            //SaveVacationDays();
            //}
            var affordances = 0;
            foreach (var calcLocation in Locations) {
                affordances += calcLocation.Affordances.Count;
            }

            if (affordances == 0) {
                throw new LPGException(
                    "Somehow the number of affordances is 0 after initializing, which should never happen. Please report.");
            }
            _startSimulation = DateTime.Now;
        }

        /*
        public void WriteInformation()
        {
            throw new NotImplementedException();
        }
        */
        public void RunOneStep([NotNull] TimeStep timestep, DateTime now, bool runProcessing)
        {
            if (_locations == null) {
                throw new LPGException("_locations should not be null");
            }

            if (_persons == null) {
                throw new LPGException("_persons should not be null");
            }

            if (_daylightArray == null) {
                throw new LPGException("_daylightArray should not be null");
            }

            if (_normalDistributedRandom == null) {
                throw new LPGException("_normalDistributedRandom should not be null");
            }

            if (_randomGenerator == null) {
                throw new LPGException("_randomGenerator should not be null");
            }

            if (_devices == null) {
                throw new LPGException("_devices should not be null");
            }

            if (_autoDevs == null) {
                throw new LPGException("_autoDevs should not be null");
            }

            foreach (var p in _persons) {
                p.NextStep(timestep, _locations, _daylightArray, _normalDistributedRandom, _randomGenerator,
                    _householdKey, _persons, _simulationSeed);
            }

            /*    if ((timestep % RangeCleaningFrequency) == 0)
            {
                foreach (CalcDevice device in _devices)
                    device.ClearExpiredRanges(timestep);
                foreach (CalcAutoDev autoDev in _autoDevs)
                    autoDev.ClearExpiredRanges(timestep);
            }*/
            if (timestep.InternalStep % 5000 == 0 || (DateTime.Now - _lastDisplay).TotalSeconds > 5) {
                var timeelapesed = DateTime.Now - _startSimulation;
                var speed = timestep.InternalStep / timeelapesed.TotalSeconds;
                string timeLeftStr = "";
                if (speed > 20) {
                    int stepsLeft = _calcParameters.InternalTimesteps - timestep.InternalStep;
                    double timeLeftSeconds = stepsLeft / speed;
                    if (timeLeftSeconds > 0) {
                        TimeSpan timeLeft = TimeSpan.FromSeconds(timeLeftSeconds);
                        timeLeftStr = ", estimated time left:" + timeLeft;
                    }
                }

                Logger.Info("Simulating household " + Name + " Time:" + now.ToShortDateString() + " " +
                            now.ToShortTimeString() + ", Timestep:" + timestep.InternalStep
                            + ", speed: "
                            + speed.ToString("F2") + " steps/second, " + timeelapesed.ToString() + " elapsed" + timeLeftStr);

                _lastDisplay = DateTime.Now;
            }

            foreach (var calcAutoDev in _autoDevs) {
                if (!calcAutoDev.IsBusyDuringTimespan(timestep, 1, 1, calcAutoDev.LoadType)) {
                    calcAutoDev.Activate(timestep, _normalDistributedRandom);
                }
            }
            if (TransportationHandler != null)
            {
                foreach (CalcTransportationDevice device in TransportationHandler.AllMoveableDevices)
                {
                    device.DriveAndCharge(timestep);
                    if (device.Currentsite != null && TransportationHandler.VehicleDepot.Contains(device))
                    {
                        throw new LPGException("Forgot to remove the car from the depot after use");
                    }
                }
                foreach (var calcSite in TransportationHandler.CalcSites)
                {
                    foreach (CalcChargingStation calcChargingStation in calcSite.ChargingDevices)
                    {
                        calcChargingStation.ProcessRequests(timestep);
                    }
                }
            }

            if (runProcessing) {
                if (_odap == null) {
                    throw new LPGException("Odap was null");
                }

                var energyFileRows = _odap.ProcessOneTimestep(timestep);
                foreach (var fileRow in energyFileRows) {
                    if (_calcParameters.IsSet(CalcOption.DetailedDatFiles)) {
                        fileRow.Save(_odap.BinaryOutStreams[fileRow.LoadType]);
                    }

                    if (_calcParameters.IsSet(CalcOption.OverallDats) || _calcParameters.IsSet(CalcOption.OverallSum)) {
                        fileRow.SaveSum(_odap.SumBinaryOutStreams[fileRow.LoadType]);
                    }
                }
            }

            _calcVariableRepository.Execute(timestep);

            if (_calcParameters.IsSet(CalcOption.ActionsLogfile)) {
                if (_lf == null) {
                    throw new LPGException("Logfile was null");
                }

                //_lf.OnlineLoggingData.AddActionEntry(timestep, _householdKey);
            }

            foreach (CalcPerson person in _persons) {
                person.LogPersonStatus(timestep);
            }
        }

        public void Dispose()
        {
            CloseLogfile();
        }

        public static void MatchAutonomousDevicesWithNormalDevices([NotNull] [ItemNotNull] List<CalcAutoDev> autoDevs,
                                                    [NotNull] [ItemNotNull] List<CalcDevice> devices)
        {
            //TODO: test this and make sure it does what it is supposed to do
            foreach (var device in devices) {
                foreach (var autoDev in autoDevs) {
                    if (autoDev.Name == device.Name && autoDev.CalcLocation == device.CalcLocation ) {
                        device.MatchingAutoDevs.Add(autoDev);
                    }
                }
            }
        }
        /*
        private void SaveBridgeDays()
        {
            if (_lf == null) {
                throw new LPGException("logfile was null");
            }
            var sw = _lf.FileFactoryAndTracker.MakeFile<StreamWriter>(
                "Bridgedays." + _householdKey + ".txt", "List of all bridge days around holidays", true,
                ResultFileID.BridgeDays, _householdKey, TargetDirectory.Reports,
                _calcParameters.InternalStepsize);
            foreach (var day in BridgeDays) {
                if (day >= _calcParameters.OfficialStartTime &&
                    day <= _calcParameters.OfficialEndTime) {
                    sw.WriteLine(day.ToShortDateString());
                }
            }
            sw.Close();
        }*/
        /*
        private void SaveVacationDays()
        {
            if (_lf == null) {
                throw new LPGException("logfile was null");
            }
            if (_persons == null) {
                throw new LPGException("_persons was null");
            }
            var sw = _lf.FileFactoryAndTracker.MakeFile<StreamWriter>(
                "VacationDays." + _householdKey + ".txt", "List of all vacation days", true,
                ResultFileID.VacationDays, _householdKey, TargetDirectory.Reports,
                _calcParameters.InternalStepsize);
            var p1 = _persons[0];
            foreach (var vacation in p1.Vacations) {
                if (vacation.Item1 >= _calcParameters.OfficialStartTime ||
                    vacation.Item2 >= _calcParameters.OfficialStartTime) {
                    sw.WriteLine(vacation.Item1.ToShortDateString() + _calcParameters.CSVCharacter +
                                 vacation.Item2.ToShortDateString());
                }
            }
            sw.Close();
        }*/

        public void SetAutoDevs([NotNull] [ItemNotNull] List<CalcAutoDev> autoDevs)
        {
            _autoDevs = autoDevs;
        }

        public void SetDevices([NotNull] [ItemNotNull] List<CalcDevice> devices)
        {
            _devices = devices;
        }

        /*
         //todo: fix this and change to sql
        private void DumpAffordanceInformation([NotNull][ItemNotNull] List<CalcAffordanceTaggingSetDto> taggingSets)
        {
            if (_lf == null) {
                throw new LPGException("lf was null");
            }
            var dstPath = Constants.AffordanceInformationFileName + _householdKey + ".json";
            if (_locations == null) {
                throw new LPGException("_locations was null");
            }
            var ai = new AffordanceInformation();
            foreach (var calcLocation in _locations) {
                foreach (var affordance in calcLocation.Affordances)
                {
                    {
                        var persontimesteps = affordance.DefaultPersonProfileLength;
                        var personSeconds = _calcParameters.InternalStepsize.TotalSeconds *persontimesteps;
                        var sai = new AffordanceInformation.SingleAffordanceInfo(
                            affordance.ID, affordance.CalcAffordanceType, affordance.Name,
                            calcLocation.Name, affordance.TimeLimitName??"",
                            affordance.SourceTrait, TimeSpan.FromSeconds(personSeconds), affordance.AffCategory,
                            affordance.CalcAffordanceSerial);
                        foreach (var taggingSet in taggingSets) {
                            var tag = taggingSet.AffordanceToTagDict[affordance.Name];
                            sai.AffordanceTags.Add(taggingSet.Name, tag);
                        }

                        if (affordance.Energyprofiles != null) {
                            foreach (var energyprofile in affordance.Energyprofiles) {
                                var di = new AffordanceInformation.DeviceInformation(
                                    energyprofile.CalcDevice.PrettyName,
                                    energyprofile.TimeProfile.Name,
                                    energyprofile.TimeOffsetInSteps, energyprofile.LoadType.Name);
                                sai.DeviceInformations.Add(di);
                            }
                        }

                        ai.AffordanceInfos.Add(sai);
                    }
                    // subaffordance
                    foreach (var subaffordance in affordance.SubAffordances)
                    {
                        var subpersontimesteps = subaffordance.DefaultPersonProfileLength;
                        var personSeconds = _calcParameters.InternalStepsize.TotalSeconds *
                                            subpersontimesteps;
                        var personTs = TimeSpan.FromSeconds(personSeconds);
                        var ssai = new AffordanceInformation.SingleAffordanceInfo(subaffordance.ID,
                            CalcAffordanceType.Subaffordance, subaffordance.Name, calcLocation.Name,
                            "", affordance.SourceTrait, personTs, affordance.AffCategory,
                            subaffordance.CalcAffordanceSerial);
                        foreach (var taggingSet in taggingSets)
                        {
                            var tag = taggingSet.AffordanceToTagDict[subaffordance.Name];
                            ssai.AffordanceTags.Add(taggingSet.Name, tag);
                        }
                        ai.AffordanceInfos.Add(ssai);
                    }
                }
            }
            using (var swAffordanceInformations =
                _lf.FileFactoryAndTracker.MakeFile<StreamWriter>(dstPath,
                    "Overview of the affordances for settlement processing", false, ResultFileID.AffordanceInformation,
                    _householdKey, TargetDirectory.Root,
                    _calcParameters.InternalStepsize)) {
                ai.WriteResultEntries(swAffordanceInformations);
            }
        }
        */
        private void DumpTimeProfiles()
        {
            if (!_calcParameters.IsSet(CalcOption.TimeProfileFile)) {
                return;
            }

            if (_lf == null) {
                throw new LPGException("Logfile was null");
            }

            var swTime = _lf.FileFactoryAndTracker.MakeFile<StreamWriter>(
                "TimeProfiles." + _householdKey + "."  + ".txt",
                "List of time profiles used in this household", true, ResultFileID.DumpTime, _householdKey,
                TargetDirectory.Debugging, _calcParameters.InternalStepsize);
            var c = _calcParameters.CSVCharacter;
            swTime.WriteLine("Device" + c + "Load Type" + c + "Profile" + c + "Number of Activations");
            if (_odap == null) {
                throw new LPGException("_odap was null");
            }

            var entries = _odap.ProfileEntries.Values.ToList();
            entries.Sort((x, y) => {
                if (x.Device != y.Device) {
                    return string.Compare(x.Device, y.Device, StringComparison.Ordinal);
                }

                if (x.LoadType != y.LoadType) {
                    return string.Compare(x.LoadType, y.LoadType, StringComparison.Ordinal);
                }

                if (x.Profile != y.Profile) {
                    return string.Compare(x.Profile, y.Profile, StringComparison.Ordinal);
                }

                return 0;
            });
            foreach (var entry in entries) {
                swTime.WriteLine(entry.Line);
            }
        }
    }
}