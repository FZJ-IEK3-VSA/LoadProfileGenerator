using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    [Serializable]
    public class CalcPersonDto : IHouseholdKey {
        public CalcPersonDto([NotNull]string name, [NotNull] StrGuid guid, int age, PermittedGender gender,
                             [NotNull]HouseholdKey householdKey, [ItemNotNull] [NotNull]List<DateSpan> sicknessSpans, [NotNull][ItemNotNull] List<DateSpan> vacationSpans, int id,
                             [NotNull] string traitTag, [NotNull] string householdName)
        {
            Name = name;
            Guid = guid;
            Age = age;
            Gender = gender;
            HouseholdKey = householdKey;
            SicknessSpans = sicknessSpans;
            VacationSpans = vacationSpans;
            ID = id;
            TraitTag = traitTag;
            HouseholdName = householdName;
        }

        [NotNull]
        public PersonInformation MakePersonInformation() => new PersonInformation(Name, Guid, TraitTag);
        [NotNull]
        public static CalcPersonDto MakeExamplePerson()
        {
            List<DateSpan> sicknessTimes = new List<DateSpan>();
            List<DateSpan> vacationTimes = new List<DateSpan>();
            return new CalcPersonDto("Example Person", System.Guid.NewGuid().ToStrGuid(),
                40,PermittedGender.Male,new HouseholdKey("HH1"),sicknessTimes,vacationTimes,1,"traitTag","householdname");
        }

        public int ID { get; }
        [NotNull]
        public string TraitTag { get; }
        [NotNull]
        public string HouseholdName { get; }
        [NotNull]
        public string Name { get; }
        [NotNull]
        public StrGuid Guid { get; }
        public int Age { get; }
        public PermittedGender Gender { get; }
        [Ignore][NotNull]
        public HouseholdKey HouseholdKey { get; }
        [NotNull][ItemNotNull]
        public List<PersonDesireDto> Desires { get; } = new List<PersonDesireDto>();
        [NotNull]
        [ItemNotNull]
        public List<DateSpan> SicknessSpans { get; }
        [NotNull]
        [ItemNotNull]
        public List<DateSpan> VacationSpans { get; }

        public void AddDesire([NotNull] PersonDesireDto desiredto)
        {
            Desires.Add(desiredto);
        }
    }
}