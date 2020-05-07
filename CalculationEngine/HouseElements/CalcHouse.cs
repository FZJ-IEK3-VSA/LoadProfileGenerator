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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using Common;
using JetBrains.Annotations;

namespace CalculationEngine.HouseElements {
    public sealed class CalcHouse : ICalcAbleObject {
        private readonly CalcRepo _calcRepo;

        //[CanBeNull] private Dictionary<int, CalcProfile> _allProfiles;

        [ItemNotNull] [CanBeNull] private List<CalcAutoDev> _autoDevs;

        [CanBeNull] private CalcAirConditioning _calcAirConditioning;

        [CanBeNull] private CalcSpaceHeating _calcSpaceHeating;

        [ItemNotNull] [CanBeNull] private List<CalcEnergyStorage> _energyStorages;

        [ItemNotNull] [CanBeNull] private List<CalcGenerator> _generators;

        [ItemNotNull] [CanBeNull] private List<ICalcAbleObject> _households;

        [ItemNotNull] [CanBeNull] private List<CalcTransformationDevice> _transformationDevices;

        public CalcHouse([NotNull] string name, [NotNull] HouseholdKey houseKey,
                         CalcRepo calcRepo)
        {
            _calcRepo = calcRepo;
            Name = name;
            HouseholdKey = houseKey;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcAutoDev> CollectAutoDevs()
        {
            var autoDevs = new List<CalcAutoDev>();
            if (_households == null) {
                throw new LPGException("households should not be null.");
            }

            foreach (var calcAbleObject in _households) {
                autoDevs.AddRange(calcAbleObject.CollectAutoDevs());
            }

            return autoDevs;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcDevice> CollectDevices()
        {
            var devices = new List<CalcDevice>();
            if (_households == null) {
                throw new LPGException("households should not be null.");
            }

            foreach (var hh in _households) {
                devices.AddRange(hh.CollectDevices());
            }

            return devices;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcLocation> CollectLocations()
        {
            var locations = new List<CalcLocation>();
            if (_households == null) {
                throw new LPGException("households should not be null.");
            }

            foreach (var calcAbleObject in _households) {
                locations.AddRange(calcAbleObject.CollectLocations());
            }

            return locations;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcPerson> CollectPersons()
        {
            var persons = new List<CalcPerson>();
            if (_households == null) {
                throw new LPGException("households should not be null.");
            }

            foreach (var ableObject in _households) {
                persons.AddRange(ableObject.CollectPersons());
            }

            return persons;
        }

        public void DumpHouseholdContentsToText()
        {
            if (_households == null) {
                throw new LPGException("households should not be null.");
            }

            if (_calcRepo.CalcParameters.IsSet(CalcOption.HouseholdContents)) {

                using (var swHouse =
                    _calcRepo.FileFactoryAndTracker.MakeFile<StreamWriter>("HouseSpec." + Constants.HouseKey.Key + ".txt",
                        "Detailed house description", true, ResultFileID.PersonFile, Constants.HouseKey, TargetDirectory.Root,
                        _calcRepo.CalcParameters.InternalStepsize)) {
                    swHouse.WriteLine("Space Heating");
                    if (_calcSpaceHeating != null) {
                        swHouse.WriteLine(_calcSpaceHeating.Name);
                        var summedDegreeDays = _calcSpaceHeating.CalcDegreeDays.Values.Select(x => x.HeatingAmount).Sum();
                        swHouse.WriteLine("Degree day sum: " + summedDegreeDays);
                        swHouse.WriteLine("Load types");
                        CalcLoadType lt = null;
                        foreach (var load in _calcSpaceHeating.PowerUsage) {
                            lt = load.LoadType;
                            swHouse.WriteLine("\t" + load.LoadType.Name + " Avg:" + load.AverageYearlyConsumption + " - StdDev:" + load.PowerStandardDeviation + " Val:" + load.Value);
                        }

                        int count = 0;
                        if (lt == null) {
                            throw new LPGException("Loadtype was null in the space heating calculation");
                        }
                        for (int i = 0; i < _calcSpaceHeating.IsBusyForLoadType[lt].Length; i++) {
                            count += _calcSpaceHeating.IsBusyForLoadType[lt][i] ? 1 : 0;
                        }

                        swHouse.WriteLine("Busy timesteps: " +count);
                        foreach (var val in _calcSpaceHeating.CalcDegreeDays.Values) {
                            swHouse.WriteLine(val.HeatingAmount);
                        }
                    }
                }
            }
            foreach (var calcHousehold in _households) {
                calcHousehold.DumpHouseholdContentsToText();
            }

        }

        public void FinishCalculation()
        {
            if (_households == null) {
                throw new LPGException("households should not be null.");
            }

            foreach (var hh in _households) {
                hh.FinishCalculation();
            }
        }

        [NotNull]
        public HouseholdKey HouseholdKey { get; }

        public void Init([NotNull] DayLightStatus daylightArray,
                         int simulationSeed)
        {
            //_allProfiles = new Dictionary<int, CalcProfile>();
            //var subhouseholdNumber = 1;
            if (_households == null) {
                throw new LPGException("Households was null");
            }

            foreach (var chh in _households) {
                chh.Init(daylightArray, //"HH" + subhouseholdNumber,
                    simulationSeed);
                /*var profiles = chh.CollectAllProfiles();
                foreach (var keyValuePair in profiles) {
                    if (!_allProfiles.ContainsKey(keyValuePair.Key)) {
                        _allProfiles.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }*/

                //subhouseholdNumber++;
            }
        }

        [NotNull]
        public string Name { get; }

        [CanBeNull]
        [ItemNotNull]
        public List<CalcEnergyStorage> EnergyStorages => _energyStorages;

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void RunOneStep([NotNull] TimeStep timestep, DateTime now, bool runProcessing)
        {
            /*if (_allProfiles == null) {
                throw new LPGException("all profiles was null");
            }*/

            if (_households == null) {
                throw new LPGException("households was null");
            }

            if (_autoDevs == null) {
                throw new LPGException("_autoDevs was null");
            }

            if (_transformationDevices == null) {
                throw new LPGException("_transformationDevices was null");
            }

            if (_energyStorages == null) {
                throw new LPGException("_energyStorages was null");
            }

            if (_generators == null) {
                throw new LPGException("_generators was null");
            }

            foreach (var household in _households) {
                household.RunOneStep(timestep, now, false);
            }

            if (_calcSpaceHeating != null) {
                if (!_calcSpaceHeating.IsBusyDuringTimespan(timestep, 1, 1, _calcSpaceHeating.PowerUsage[0].LoadType)) {
                    _calcSpaceHeating.Activate(timestep,  now);
                }
            }

            if (_calcAirConditioning?.IsBusyDuringTimespan(timestep, 1, 1,
                    _calcAirConditioning.PowerUsage[0].LoadType) == false) {
                _calcAirConditioning.Activate(timestep,  now);
            }

            foreach (var calcAutoDev in _autoDevs) {
                if (!calcAutoDev.IsBusyDuringTimespan(timestep, 1, 1, calcAutoDev.LoadType)) {
                    calcAutoDev.Activate(timestep);
                }
            }

            var runAgain = true;
            var energyFileRows = _calcRepo.Odap.ProcessOneTimestep(timestep);

            var repetitioncount = 0;
            List<string> log = null;
            while (runAgain) {
                runAgain = false;
                repetitioncount++;
                if (repetitioncount == 98) {
                    log = new List<string>();
                }

                if (repetitioncount > 100) {
                    var builder = new StringBuilder();
                    foreach (var transformationDevice in _transformationDevices) {
                        builder.Append(transformationDevice.Name).Append(Environment.NewLine);
                    }

                    foreach (var dev in _energyStorages) {
                        builder.Append(dev.Name).Append(Environment.NewLine);
                    }

                    foreach (var dev in _generators) {
                        builder.Append(dev.Name).Append(Environment.NewLine);
                    }

                    var protocol = string.Empty;
                    if (log != null) {
                        foreach (var s1 in log) {
                            protocol += s1 + Environment.NewLine;
                        }
                    }

                    throw new DataIntegrityException(
                        "Did more than 100 tries while trying to calculate the transformation devices in the house " +
                        Name + " without reaching a solution. " +
                        "This most likely means you have a transformation device loop which is not permitted. " +
                        "A loop might be device A makes water from electricity and device B makes electricty from water. " +
                        "The devices are:" + Environment.NewLine + builder +
                        Environment.NewLine + Environment.NewLine + "The last actions were:" + Environment.NewLine +
                        protocol);
                }

                foreach (var transformationDevice in _transformationDevices) {
                    if (transformationDevice.ProcessOneTimestep(energyFileRows, log)) {
                        runAgain = true;
                    }
                }

                foreach (var energyStorage in _energyStorages) {
                    if (energyStorage.ProcessOneTimestep(energyFileRows, timestep, log)) {
                        runAgain = true;
                    }
                }

                foreach (var calcGenerator in _generators) {
                    if (calcGenerator.ProcessOneTimestep(energyFileRows, timestep, log)) {
                        runAgain = true;
                    }
                }
            }

            foreach (var fileRow in energyFileRows) {
                if (_calcRepo.CalcParameters.IsSet(CalcOption.DetailedDatFiles)) {
                    fileRow.Save(_calcRepo.Odap.BinaryOutStreams[fileRow.LoadType]);
                }

                if (_calcRepo.CalcParameters.IsSet(CalcOption.OverallDats) || _calcRepo.CalcParameters.IsSet(CalcOption.OverallSum)) {
                    fileRow.SaveSum(_calcRepo.Odap.SumBinaryOutStreams[fileRow.LoadType]);
                }
            }
        }

        // ReSharper disable once UnusedParameter.Local
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public void Dispose()
        {
            if (_households == null) {
#pragma warning disable S3877 // Exceptions should not be thrown from unexpected methods
                throw new LPGException("_households was null");
#pragma warning restore S3877 // Exceptions should not be thrown from unexpected methods
            }
            _calcRepo.Dispose();
            foreach (var household in _households) {
                household.Dispose();
            }
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcDevice> CollectHouseDevices()
        {
            var devices = new List<CalcDevice>();
            if (_autoDevs == null) {
                throw new LPGException("_autodevs was null");
            }

            foreach (var autodevs in _autoDevs) {
                devices.Add(autodevs);
            }

            if (_calcAirConditioning != null) {
                devices.Add(_calcAirConditioning);
            }

            if (_calcSpaceHeating != null) {
                devices.Add(_calcSpaceHeating);
            }

            return devices;
        }

        public void SetAirConditioning([NotNull] CalcAirConditioning airConditioning)
        {
            _calcAirConditioning = airConditioning;
        }

        public void SetAutoDevs([NotNull] [ItemNotNull] List<CalcAutoDev> autoDevs)
        {
            _autoDevs = autoDevs;
        }

        public void SetGenerators([NotNull] [ItemNotNull] List<CalcGenerator> generators)
        {
            _generators = generators;
        }

        public void SetHouseholds([NotNull] [ItemNotNull] List<ICalcAbleObject> households)
        {
            _households = households;
        }

        public void SetSpaceHeating([CanBeNull] CalcSpaceHeating spaceHeating)
        {
            _calcSpaceHeating = spaceHeating;
        }

        public void SetStorages([NotNull] [ItemNotNull] List<CalcEnergyStorage> energyStorages)
        {
            _energyStorages = energyStorages;
        }

        public void SetTransformationDevices(
            [NotNull] [ItemNotNull] List<CalcTransformationDevice> transformationDevices)
        {
            _transformationDevices = transformationDevices;
        }
    }
}