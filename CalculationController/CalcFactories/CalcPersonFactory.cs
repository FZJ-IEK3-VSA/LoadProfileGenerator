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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Enums;
using JetBrains.Annotations;

namespace CalculationController.CalcFactories {
    public class CalcPersonFactory {
        private readonly CalcRepo _calcRepo;

        [NotNull] private readonly List<DateTime> _internalDateTimeForSteps;


        public CalcPersonFactory(CalcRepo calcRepo)
        {
            _calcRepo = calcRepo;
            _internalDateTimeForSteps = new List<DateTime> {
                _calcRepo.CalcParameters.InternalStartTime
            };
            for (var i = 1; i < _calcRepo.CalcParameters.InternalTimesteps; i++) {
                _internalDateTimeForSteps.Add(_internalDateTimeForSteps[i - 1] + _calcRepo.CalcParameters.InternalStepsize);
            }
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcPerson> MakeCalcPersons([NotNull] [ItemNotNull] List<CalcPersonDto> persons,
                                                [NotNull] CalcLocation startLocation,
                                                [NotNull] string householdName)
        {
            var calcPersons = new List<CalcPerson>();
            Dictionary<string, SharedDesireValue> sharedDesireValues = new Dictionary<string, SharedDesireValue>();
            foreach (var hhPerson in persons) {
                var isSick = SetBitArrayWithDateSpans(hhPerson.SicknessSpans);
                var vacationTimes = SetBitArrayWithDateSpans(hhPerson.VacationSpans);

                var cp = new CalcPerson(hhPerson, startLocation,  isSick, vacationTimes,_calcRepo);
                /* repetitionCount, _random,hhPerson.Person.Age, hhPerson.Person.Gender, _logFile,
                 householdKey, startLocation, traitTagName, householdName, _calcParameters, isSick,
                 Guid.NewGuid().ToStrGuid());*/
                // vacations setzen
                foreach (PersonDesireDto desire in hhPerson.Desires) {
                    AddDesireToPerson(desire, cp, sharedDesireValues, householdName);
                }


                calcPersons.Add(cp);
            }

            return calcPersons;
        }

        [NotNull]
        [ItemNotNull]
        public BitArray SetBitArrayWithDateSpans([NotNull] [ItemNotNull] List<DateSpan> dates)
        {
            BitArray myArray = new BitArray(_calcRepo.CalcParameters.InternalTimesteps);
            var internalTimeSteps = _calcRepo.CalcParameters.InternalTimesteps;
            //var internalDateTimeForSteps = _calcParameters.InternalDateTimeForSteps;
            foreach (DateSpan span in dates) {
                for (var i = 0; i < internalTimeSteps; i++) {
                    if (_internalDateTimeForSteps[i] > span.Start && _internalDateTimeForSteps[i] < span.End) {
                        myArray[i] = true;
                    }
                }
            }

            return myArray;
        }

        private void AddDesireToPerson([NotNull] PersonDesireDto desire,
                                       [NotNull] CalcPerson calcPerson,
                                       [NotNull] Dictionary<string, SharedDesireValue> sharedDesireValues,
                                       [NotNull] string householdName)
        {
            var sdv = GetSharedDesireValue(desire, sharedDesireValues);
            var cd1 = new CalcDesire(desire.Name,
                desire.DesireID,
                desire.Threshold,
                desire.DecayTime,
                1,
                desire.Weight,
                _calcRepo.CalcParameters.TimeStepsPerHour,
                desire.CriticalThreshold,
                sdv,
                desire.SourceTrait,
                desire.DesireCategory);
            if (desire.HealthStatus == HealthStatus.Healthy || desire.HealthStatus == HealthStatus.HealthyOrSick) {
                CheckIfDesireViolatesCategory(desire, calcPerson.PersonDesires, calcPerson, householdName);
                calcPerson.PersonDesires.AddDesires(cd1);
            }

            if (desire.HealthStatus == HealthStatus.Sick || desire.HealthStatus == HealthStatus.HealthyOrSick) {
                CheckIfDesireViolatesCategory(desire, calcPerson.SicknessDesires, calcPerson, householdName);
                calcPerson.SicknessDesires.AddDesires(cd1);
            }
        }

        private static void CheckIfDesireViolatesCategory([NotNull] PersonDesireDto desire,
                                                          [NotNull] CalcPersonDesires calcDesires,
                                                          [NotNull] CalcPerson person,
                                                          [NotNull] string householdName)
        {
            string desirecategory = desire.DesireCategory;
            if (string.IsNullOrWhiteSpace(desirecategory)) {
                return;
            }

            if (calcDesires.Desires.ContainsKey(desire.DesireID)) {
                return;
            }

            var existingDesireCategories = calcDesires.Desires.Values.Select(x => x.DesireCategory).Distinct().ToList();
            if (existingDesireCategories.Contains(desirecategory)) {
                var existingdesires = calcDesires.Desires.Values.Where(x => x.DesireCategory == desirecategory).ToList();
                var source = existingdesires[0];
                throw new DataIntegrityException("Trying to add two desires from the desire category " + Environment.NewLine + desirecategory +
                                                 " to the person " + person.Name + " in the household " + householdName +
                                                 ". This is not permitted. " + "Please fix. The first desire was " + Environment.NewLine +
                                                 source.Name + " from " + source.SourceTrait + " and the second desire was " + Environment.NewLine +
                                                 desire.Name + " from " + desire.SourceTrait + Environment.NewLine);
            }
        }

        [CanBeNull]
        private static SharedDesireValue GetSharedDesireValue([NotNull] PersonDesireDto desire,
                                                              [NotNull] Dictionary<string, SharedDesireValue> sharedDesireValues)
        {
            if (desire.IsSharedDesire) {
                /*
                if (sharedDesireValues == null)
                {
                    throw new LPGException("Shared desire values was null");
                }*/

                if (sharedDesireValues.ContainsKey(desire.Name)) {
                    return sharedDesireValues[desire.Name];
                }

                var sdv = new SharedDesireValue(1, null);
                sharedDesireValues.Add(desire.Name, sdv);
                return sdv;
            }

            return null;
        }
    }
}