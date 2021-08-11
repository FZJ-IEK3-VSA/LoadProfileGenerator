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

using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.CalcFactories {
    public class DeviceLocationTuple {
        public DeviceLocationTuple([NotNull] Location location, [NotNull] IAssignableDevice device)
        {
            Location = location;
            Device = device;
        }

        [NotNull]
        public IAssignableDevice Device { get; }

        [NotNull]
        public Location Location { get; }
    }

    public class LocationDtoDict {
        [NotNull]
        public Dictionary<Location, CalcLocationDto> LocationDict { get; } = new Dictionary<Location, CalcLocationDto>();

        [NotNull]
        public CalcLocationDto GetDtoForLocation([NotNull] Location location) => LocationDict[location];

        public bool SimulateLocation([NotNull] Location l)
        {
            if (LocationDict.ContainsKey(l)) {
                return true;
            }

            return false;
        }
    }

    public class DtoCalcLocationDict {
        [NotNull]
        public Dictionary<CalcLocationDto, CalcLocation> LocationDtoDict { get; } = new Dictionary<CalcLocationDto, CalcLocation>();

        [NotNull]
        public CalcLocation GetCalcLocationByGuid(StrGuid calcLocationGuid)
        {
            return LocationDtoDict.Values.First(x => x.Guid == calcLocationGuid);
        }
    }

    public class CalcModularHouseholdFactory {
        [NotNull] private readonly CalcAffordanceFactory _caf;

        [NotNull] private readonly CalcVariableRepository _calcVariableRepository;
        private readonly CalcRepo _calcRepo;

        [NotNull] private readonly CalcDeviceFactory _cdf;

        [NotNull] private readonly CalcLocationFactory _clf;

        [NotNull] private readonly CalcPersonFactory _cpf;

        [NotNull] private readonly CalcTransportationFactory _ctf;


        [NotNull] private readonly CalcLoadTypeDictionary _ltDict;


        //private readonly CalcVariableDtoFactory _variableDtoFactory;

        public CalcModularHouseholdFactory([NotNull] CalcLoadTypeDictionary ltDict,
                                           [NotNull] CalcLocationFactory clf,
                                           [NotNull] CalcPersonFactory cpf,
                                           [NotNull] CalcDeviceFactory cdf,
                                           [NotNull] CalcAffordanceFactory caf,
                                           [NotNull] CalcTransportationFactory ctf,
                                           [NotNull] CalcVariableRepository variableRepository,
                                           CalcRepo calcRepo
        )
        {
            _ltDict = ltDict;
            _clf = clf;
            _cpf = cpf;
            _cdf = cdf;
            _caf = caf;
            _ctf = ctf;
            _calcVariableRepository = variableRepository;
            _calcRepo = calcRepo;
        }

        [NotNull]
        public CalcHousehold MakeCalcModularHousehold([NotNull] CalcHouseholdDto householdDto,
                                                      [NotNull] out DtoCalcLocationDict dtoCalcLocationDict,
                                                      [CanBeNull] string houseName,
                                                      [CanBeNull] string houseDescription,
                                                      [NotNull] List<CalcAffordanceTaggingSetDto> affordanceTaggingSets,
                                                      [NotNull] CalcRepo calcRepo)
        {
            CalcHousehold chh = null;
            _calcRepo.FileFactoryAndTracker.RegisterHousehold(householdDto.HouseholdKey,
                householdDto.Name,
                HouseholdKeyType.Household,
                householdDto.Description,
                houseName,
                houseDescription);
            string name = householdDto.Name + " " + householdDto.HouseholdKey;
            try {
                dtoCalcLocationDict = new DtoCalcLocationDict();
                var calcLocations = _clf.MakeCalcLocations(householdDto.LocationDtos, dtoCalcLocationDict, calcRepo);
                if (calcLocations.Count == 0) {
                    throw new LPGException("Not a single location could be created. Something in this household is wrong. Please fix.");
                }

                // devices
                var calcDevices = new List<CalcDevice>();
                _cdf.MakeCalcDevices(calcLocations, householdDto.DeviceDtos, calcDevices,
                    householdDto.HouseholdKey, _ltDict, calcRepo);

                var autodevs = _cdf.MakeCalcAutoDevs(householdDto.AutoDevices, dtoCalcLocationDict);

                //affordances
                /*if (householdDto.Vacation == null)
                {
                    throw new LPGException("Vacation was null");
                }*/

                //_cdf.MakeCalcDevices(calcLocations, householdDto.DeviceDtos, calcDevices,householdDto.HouseholdKey, _ltDict);
                var calcpersons = _cpf.MakeCalcPersons(householdDto.Persons, calcLocations, householdDto.Name, householdDto.HouseholdKey);

                chh = new CalcHousehold(name,
                    householdDto.GeographicLocationName,
                    householdDto.TemperatureprofileName,
                    householdDto.HouseholdKey,
                    householdDto.Guid,
                    _calcVariableRepository,
                    calcLocations,
                    calcpersons,
                    householdDto.Description,
                    _calcRepo);
                HashSet<StrGuid> deviceGuids = new HashSet<StrGuid>();
                foreach (CalcDevice device in calcDevices) {
                    if (!deviceGuids.Add(device.Guid)) {
                        throw new LPGException("Tried to add the same device guid twice");
                    }
                }

                chh.SetDevices(calcDevices);
                chh.SetAutoDevs(autodevs);
                //if (chh.AutoDevs.Count != householdDto.AutoDevices.Count) {
                    //throw new LPGException("Missing autonomous devices. This is a bug. Please report.");
                //}
                //chh.BridgeDays = householdDto.BridgeDays;
                _caf.SetAllAffordaces(householdDto.Affordances, dtoCalcLocationDict,
                    _calcVariableRepository, calcDevices, householdDto.HouseholdKey);
                CheckCalcAffordancesForExecutability(chh);
                if (householdDto.CalcTravelRoutes != null) {
                    Logger.Info("Starting to initialize transportation for household " + householdDto.Name + "...");
                    _ctf.MakeTransportation(householdDto, dtoCalcLocationDict, chh, affordanceTaggingSets);
                }
                else {
                    Logger.Info("No travel route was set for for household " + householdDto.Name + ", skipping transportation");
                }

                return chh;
            }
            catch {
                chh?.Dispose();
                throw;
            }
        }

        private static void CheckCalcAffordancesForExecutability([NotNull] CalcHousehold calcHousehold)
        {
            var calcAffordances = calcHousehold.Locations.SelectMany(x => x.Affordances).ToList();
            foreach (var calcAffordanceBase in calcAffordances) {
                var calcAffordance = (CalcAffordance)calcAffordanceBase;
                bool found = false;
                foreach (var person in calcHousehold.Persons) {
                    bool isvalidforsick = person.NewIsBasicallyValidAffordance(calcAffordance, true, false);
                    bool isvalidforhealth = person.NewIsBasicallyValidAffordance(calcAffordance, false, false);
                    if (isvalidforsick || isvalidforhealth) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    Logger.Debug("The affordance " + calcAffordance.Name + " from the trait " + calcAffordance.SourceTrait +
                                 " seems to not be executable " + " by any person in the household " + calcHousehold.Name + ".");
                    foreach (var person in calcHousehold.Persons) {
                        Logger.Info("Details for the person: " + person.Name);
                        person.NewIsBasicallyValidAffordance(calcAffordance, true, true);
                        person.NewIsBasicallyValidAffordance(calcAffordance, false, true);
                    }

                    throw new DataIntegrityException("The affordance " + calcAffordance.Name + " from the trait " + calcAffordance.SourceTrait +
                                                     " seems to not be executable " + " by any person in the household " + calcHousehold.Name +
                                                     ". Please fix.");
                }
            }
        }
    }
}