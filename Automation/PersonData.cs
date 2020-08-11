
using System.Collections.Generic;

namespace Automation {
    public class PersonData {
        public PersonData(int age, Gender gender)
        {
            Age = age;
            Gender = gender;
        }

        public int Age { get; set; }
        public Gender Gender { get; set; }

        public List<string> PersonTags { get; set; } = new List<string>();
    }
}