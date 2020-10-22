

namespace Automation {
    public class PersonData {
        public PersonData(int age, Gender gender, string personName)
        {
            Age = age;
            Gender = gender;
            PersonName = personName;
        }

        public int Age { get; set; }
        public Gender Gender { get; set; }

        public string? LivingPatternTag { get; set; }
        public string PersonName { get; set; }
    }
}