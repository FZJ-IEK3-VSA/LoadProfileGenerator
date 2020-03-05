
namespace Automation {
    public class PersonData {
        public PersonData(int age, Gender gender)
        {
            Age = age;
            Gender = gender;
        }

        public int Age { get; set; }
        public Gender Gender { get; set; }
    }
}