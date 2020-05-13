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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.HouseholdElements;
using Common.CalcDto;
using JetBrains.Annotations;

namespace CalculationController.CalcFactories {
    public class CalcAffordanceFactory {
        //private readonly CalcVariableDtoFactory _variableRepository;
        [NotNull] private readonly AvailabilityDtoRepository _availabilityDtoRepository;
        private readonly CalcRepo _calcRepo;


        [NotNull] private readonly CalcLoadTypeDictionary _loadTypeDictionary;

        public CalcAffordanceFactory([NotNull] CalcLoadTypeDictionary loadTypeDictionary,
                                     [NotNull] AvailabilityDtoRepository availabilityDtoRepository,
            CalcRepo calcRepo
            //[NotNull] CalcVariableDtoFactory variableRepository
        )
        {
            _loadTypeDictionary = loadTypeDictionary;
            _availabilityDtoRepository = availabilityDtoRepository;
            _calcRepo = calcRepo;
            //_variableRepository = variableRepository;
        }

        [NotNull]
        public static string FixAffordanceName([NotNull] string name, [NotNull] string csvCharacter)
        {
            name = name.Replace(".", "_");
            var replacementChar = ",";
            if (csvCharacter == ",") {
                replacementChar = "_";
            }

            return name.Replace(csvCharacter, replacementChar);
        }

        public void SetAllAffordaces([NotNull] [ItemNotNull] List<CalcAffordanceDto> affordances,
                                     [NotNull] DtoCalcLocationDict locations,
                                     [NotNull] CalcVariableRepository variableRepository,
                                     [ItemNotNull] [NotNull] List<CalcDevice> devices)
        {
            if (affordances.Count == 0) {
                throw new LPGException("No Affordances found.");
            }

            foreach (CalcAffordanceDto affordancedto in affordances) {
                CalcProfile personProfile = CalcDeviceFactory.MakeCalcProfile(affordancedto.PersonProfile, _calcRepo.CalcParameters);
                CalcLocation calcLocation = locations.GetCalcLocationByGuid(affordancedto.CalcLocationGuid);
                var calcDesires = MakeCalcDesires(affordancedto.Satisfactionvalues);
                var color = new ColorRGB(affordancedto.ColorR, affordancedto.ColorG, affordancedto.ColorB);
                var variableOps = MakeVariableOps(locations, affordancedto.VariableOps, variableRepository);
                List<VariableRequirement> requirements = new List<VariableRequirement>();
                foreach (VariableRequirementDto requirementDto in affordancedto.VariableRequirements) {
                    VariableRequirement vrq = new VariableRequirement(requirementDto.Name,
                        requirementDto.Value, requirementDto.CalcLocationName, requirementDto.LocationGuid,
                        requirementDto.VariableCondition, variableRepository, requirementDto.VariableGuid);
                    requirements.Add(vrq);
                }

                var deviceEnergyProfiles = MakeDeviceEnergyProfileTuples(devices, affordancedto);

                var busyarr = _availabilityDtoRepository.GetByGuid(affordancedto.IsBusyArray.Guid);
                CalcAffordance caff = new CalcAffordance(
                    affordancedto.Name,
                    personProfile, calcLocation,
                    affordancedto.RandomEffect,
                    calcDesires,
                    affordancedto.MiniumAge,
                    affordancedto.MaximumAge,
                    affordancedto.PermittedGender,
                    affordancedto.NeedsLight,
                    affordancedto.TimeStandardDeviation,
                    color,
                    affordancedto.AffCategory,
                    affordancedto.IsInterruptable,
                    affordancedto.IsInterrupting,
                    variableOps,
                    requirements,
                    affordancedto.ActionAfterInterruption,
                    affordancedto.TimeLimitName,
                    affordancedto.Weight,
                    affordancedto.RequireAllDesires,
                    affordancedto.SrcTrait,
                    affordancedto.Guid,
                    variableRepository, deviceEnergyProfiles,
                    busyarr, affordancedto.BodilyActivityLevel,_calcRepo);
                MakeSubAffordances(locations, variableRepository, affordancedto, caff);

                calcLocation.AddAffordance(caff);
            }
        }

        private void MakeSubAffordances([NotNull] DtoCalcLocationDict locations, [NotNull] CalcVariableRepository variableRepository,
                                        [NotNull] CalcAffordanceDto affordancedto, [NotNull] CalcAffordance caff)
        {
            foreach (CalcSubAffordanceDto sdto in affordancedto.SubAffordance) {
                CalcLocation subaffLocation = locations.GetCalcLocationByGuid(sdto.LocGuid);
                List<CalcDesire> satisfactionValues = MakeCalcDesires(sdto.Satisfactionvalues);
                var varOps = MakeVariableOps(locations, sdto.VariableOps, variableRepository);
                //subaffordances have no time limits
                BitArray isBusySub = new BitArray(_calcRepo.CalcParameters.InternalTimesteps, false);
                CalcSubAffordance csuf = new CalcSubAffordance(sdto.Name,
                    subaffLocation,
                    satisfactionValues,
                    sdto.MiniumAge, sdto.MaximumAge,
                    sdto.Delaytimesteps,
                    sdto.PermittedGender,
                    sdto.AffCategory,
                    sdto.IsInterruptable,
                    sdto.IsInterrupting,
                    caff,
                    varOps,
                    sdto.Weight,
                    sdto.SourceTrait,
                    sdto.Guid, isBusySub, variableRepository, caff.BodilyActivityLevel,_calcRepo);
                caff.SubAffordances.Add(csuf);
            }
        }

        [ItemNotNull]
        [NotNull]
        private List<CalcAffordance.DeviceEnergyProfileTuple> MakeDeviceEnergyProfileTuples([ItemNotNull] [NotNull] List<CalcDevice> devices,
                                                                                            [NotNull] CalcAffordanceDto affordancedto)
        {
            List<CalcAffordance.DeviceEnergyProfileTuple> deviceEnergyProfiles =
                new List<CalcAffordance.DeviceEnergyProfileTuple>();
            foreach (DeviceEnergyProfileTupleDto dto in affordancedto.Energyprofiles) {
                StrGuid devguid = dto.CalcDeviceGuid;
                CalcDevice device = devices.Single(x => x.Guid == devguid);
                CalcProfile cp = CalcDeviceFactory.MakeCalcProfile(dto.TimeProfile, _calcRepo.CalcParameters);
                CalcLoadType clt = _loadTypeDictionary.GetLoadtypeByGuid(dto.CalcLoadTypeGuid);
                CalcAffordance.DeviceEnergyProfileTuple dep = new CalcAffordance.DeviceEnergyProfileTuple(device,
                    cp, clt, dto.TimeOffset, _calcRepo.CalcParameters.InternalStepsize, dto.Multiplier, dto.Probability);
                deviceEnergyProfiles.Add(dep);
            }

            return deviceEnergyProfiles;
        }

        [NotNull]
        [ItemNotNull]
        private static List<CalcDesire> MakeCalcDesires([NotNull] [ItemNotNull] List<CalcDesireDto> satisfactionValues)
        {
            List<CalcDesire> calcDesires = new List<CalcDesire>();
            foreach (CalcDesireDto desireDto in satisfactionValues) {
                CalcDesire calcDesire = new CalcDesire(desireDto.Name,
                    desireDto.DesireID, desireDto.Threshold, desireDto.DecayTime, desireDto.Value,
                    desireDto.Weight, desireDto.TimestepsPerHour, desireDto.CriticalThreshold, null,
                    desireDto.SourceTrait,
                    desireDto.DesireCategory);
                calcDesires.Add(calcDesire);
            }

            return calcDesires;
        }

        [NotNull]
        [ItemNotNull]
        private static List<CalcAffordanceVariableOp> MakeVariableOps([NotNull] DtoCalcLocationDict locations,
                                                                      [NotNull] [ItemNotNull]
                                                                      List<CalcAffordanceVariableOpDto> affordancedto,
                                                                      [NotNull] CalcVariableRepository repository)
        {
            List<CalcAffordanceVariableOp> variableOps = new List<CalcAffordanceVariableOp>();
            foreach (CalcAffordanceVariableOpDto variableOpDto in affordancedto) {
                if (!repository.IsVariableRegistered(variableOpDto.VariableGuid)) {
                    throw new LPGException("Variable " + variableOpDto.Name + " was not registered.");
                }

                CalcLocation variableLocation = locations.GetCalcLocationByGuid(variableOpDto.LocationGuid);
                var variableOp = new CalcAffordanceVariableOp(variableOpDto.Name, variableOpDto.Value,
                    variableLocation, variableOpDto.VariableAction, variableOpDto.ExecutionTime,
                    variableOpDto.VariableGuid);
                variableOps.Add(variableOp);
            }

            return variableOps;
        }
    }
}