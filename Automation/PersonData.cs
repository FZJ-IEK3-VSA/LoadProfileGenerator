

using System.Collections.Generic;

namespace Automation {
    public class PersonData
    {
        public PersonData(int age, Gender gender, string personName) : this(age, gender, personName, null) { }

        public PersonData(int age, Gender gender, string personName, List<TransportationPreference>? transportationPreferences)
        {
            Age = age;
            Gender = gender;
            PersonName = personName;
            TransportationPreferences = transportationPreferences;
        }

        public int Age { get; set; }
        public Gender Gender { get; set; }

        public string? LivingPatternTag { get; set; }
        public string PersonName { get; set; }

        public List<TransportationPreference>? TransportationPreferences { get; set; }
    }
}