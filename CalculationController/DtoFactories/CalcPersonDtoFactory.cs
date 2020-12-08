using System;
using System.Collections.Generic;
using Automation.ResultFiles;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class CalcPersonDtoFactory {
        [JetBrains.Annotations.NotNull]
        private readonly CalcParameters _calcParameters;
        [JetBrains.Annotations.NotNull]
        private readonly Random _random;
        [JetBrains.Annotations.NotNull]
        private readonly NormalRandom _normalRandom;
        [JetBrains.Annotations.NotNull]
        private readonly VacationDtoFactory _vacationDtoFactory;

        public CalcPersonDtoFactory([JetBrains.Annotations.NotNull] CalcParameters calcParameters, [JetBrains.Annotations.NotNull] Random random, [JetBrains.Annotations.NotNull] NormalRandom normalRandom, [JetBrains.Annotations.NotNull] VacationDtoFactory vacationDtoFactory)
        {
            _calcParameters = calcParameters;
            _random = random;
            _normalRandom = normalRandom;
            _vacationDtoFactory = vacationDtoFactory;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcPersonDto> MakePersonDtos([JetBrains.Annotations.NotNull][ItemNotNull] List<ModularHouseholdPerson> persons, [JetBrains.Annotations.NotNull] HouseholdKey householdKey, [JetBrains.Annotations.NotNull][ItemNotNull] List<VacationTimeframe> vacationTimes,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<ModularHousehold.PersonTraitDesireEntry> traitDesires, [JetBrains.Annotations.NotNull] string modularHouseholdName)
        {
            List<CalcPersonDto> personDtOs = new List<CalcPersonDto>();
            foreach (ModularHouseholdPerson hhPerson in persons) {
                string traitTagName = hhPerson.LivingPatternTag?.Name;
                if (traitTagName == null)
                {
                    traitTagName = "no trait tag set";
                }
                var sicknessSpans =
                    CalculateSicknessdays(hhPerson.Person.SickDays, hhPerson.Person.AverageSicknessDuration);
                var vacationSpans = _vacationDtoFactory.GetVacationSpans(vacationTimes);
                CalcPersonDto dto = new CalcPersonDto(hhPerson.Person.PrettyName,
                    Guid.NewGuid().ToStrGuid(),hhPerson.Person.Age,hhPerson.Person.Gender,
                    householdKey, sicknessSpans,vacationSpans,hhPerson.Person.IntID,traitTagName,modularHouseholdName);
                AddTraitDesiresToOnePerson(traitDesires,dto);
                personDtOs.Add(dto);
            }
            return personDtOs;
        }

        private static void AddTraitDesiresToOnePerson([JetBrains.Annotations.NotNull][ItemNotNull] List<ModularHousehold.PersonTraitDesireEntry> traitDesires,
                                                       [JetBrains.Annotations.NotNull] CalcPersonDto calcPerson)
        {
            foreach (var personTraitDesireEntry in traitDesires)
            {
                switch (personTraitDesireEntry.AssignType)
                {
                    case ModularHouseholdTrait.ModularHouseholdTraitAssignType.Age:
                        if ((calcPerson.Gender == personTraitDesireEntry.HHTDesire.Gender ||
                             personTraitDesireEntry.HHTDesire.Gender == PermittedGender.All) &&
                            calcPerson.Age >= personTraitDesireEntry.HHTDesire.MinAge &&
                            calcPerson.Age <= personTraitDesireEntry.HHTDesire.MaxAge) {
                            Desire d = personTraitDesireEntry.HHTDesire.Desire;
                            calcPerson.AddDesire(new PersonDesireDto(d.Name,d.IntID, personTraitDesireEntry.HHTDesire.Threshold,
                                personTraitDesireEntry.HHTDesire.DecayTime, personTraitDesireEntry.HHTDesire.Weight,
                                d.CriticalThreshold,personTraitDesireEntry.SrcTrait.PrettyName,d.DesireCategory,
                                personTraitDesireEntry.HHTDesire.SicknessDesire,d.IsSharedDesire));
                        }
                        break;
                    case ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name:
                        //              var foundPerson = false;
                        if (personTraitDesireEntry.Person == null)
                        {
                            throw new LPGException("BUG: Person was null. Please report.");
                        }
                        if (calcPerson.ID == personTraitDesireEntry.Person.ID)
                        {
                            //                        foundPerson = true;
                            // found
                            calcPerson.AddDesire(new PersonDesireDto(personTraitDesireEntry.HHTDesire.Name,
                                personTraitDesireEntry.HHTDesire.Desire.IntID,
                                personTraitDesireEntry.HHTDesire.Threshold,
                                personTraitDesireEntry.HHTDesire.DecayTime, personTraitDesireEntry.HHTDesire.Weight,
                                personTraitDesireEntry.HHTDesire.Desire.CriticalThreshold,
                                personTraitDesireEntry.SrcTrait.PrettyName,
                                personTraitDesireEntry.HHTDesire.Desire.DesireCategory,
                                personTraitDesireEntry.HHTDesire.SicknessDesire,
                                personTraitDesireEntry.HHTDesire.Desire.IsSharedDesire));
                        }
                        /*
                        if (!foundPerson)
                        {
                            throw new DataIntegrityException("The modular household " + modularhouseholdName +
                                                             " does not contain the Person " +
                                                             personTraitDesireEntry.Person.Name);
                        }*/

                        break;
                    default:
                        throw new LPGException(
                            "Unknown Assignment Type putting together a modular household. Looks like something was forgotten. Please report to the programmer.");
                }
            }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<DateSpan> CalculateSicknessdays(int sickDays, int averageSickDays)
        {
            List<DateSpan> dates = new List<DateSpan>();
            for (var year = _calcParameters.InternalStartTime.Year;
                year <= _calcParameters.InternalEndTime.Year;
                year++)
            {
                var yearStart = new DateTime(year, 1, 1);
                var yearEnd = new DateTime(year + 1, 1, 1);
                var daysInYear = (int)(yearEnd - yearStart).TotalDays;
                var leftSickDays = sickDays;
                while (leftSickDays > 0)
                {
                    var day = _random.Next(daysInYear);
                    var duration = (int)_normalRandom.NextDouble(averageSickDays, 3);
                    if (duration < 0)
                    {
                        duration *= -1;
                    }

                    if (duration == 0)
                    {
                        duration = 1;
                    }

                    if (leftSickDays > duration)
                    {
                        leftSickDays -= duration;
                    }
                    else
                    {
                        duration = leftSickDays;
                        leftSickDays = 0;
                    }

                    var sicknessstart = new DateTime(year, 1, 1).AddDays(day);
                    var sicknessend = sicknessstart.AddDays(duration);
                    dates.Add(new DateSpan(sicknessstart,sicknessend));
                }
            }

            return dates;
        }
    }
}