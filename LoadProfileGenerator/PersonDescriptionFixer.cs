using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Tables.ModularHouseholds;

namespace LoadProfileGenerator {
    public static class PersonDescriptionFixer {
        /*
        private static List<string> TraitsToIgnore()
        {
            List<string> t = new List<string>();
            t.Add("(Christmas Cooking) Christmas Cooking (1.5h, 2x/Day)");
            t.Add("(Sickness Activities) Sickness Activities to keep busy while sick 1 (1.3h, 1.2x/Day)");
            return t;
        }

        private static Dictionary<string, string> GoodNames()
        {
            Dictionary<string, string> t = new Dictionary<string, string>();
            t.Add("(Bar Visits) Bar Visit 3 (4.0h, 3.5x/Week)", "going out");
            t.Add("(Bar Visits) Bar Visit 4 (4.0h, 7x/Week)", "going out");
            t.Add("(Breakfast) Breakfast Interrupting, No Alarm (0.4h, 2x/Day)", string.Empty);
            t.Add("(Brunching) Brunching (4.0h, 1.8x/Week)", string.Empty);
            t.Add("(Brunching) Daily Brunching (1.5h, 2.4x/Day)", "brunching");
            t.Add("(Brunching) Sunday Brunching (4.0h, 1.8x/Week)", string.Empty);
            t.Add("(Children Visit) Children Visit 1 (6.0h, 3.8x/Month)", "children visiting");
            t.Add("(Computer Gaming) Computer Gaming 1 (1.5h, 2x/Day)", "computer gaming");
            t.Add("(Computer Use) Computer Use 1 (1.5h, 6x/Day)", "computer use");
            t.Add("(Computer Use) Computer Use 2 (1.5h, 3x/Day)", "computer use");
            t.Add("(Computer Use) Computer Use 3 (1.5h, 1.5x/Day)", "computer use");
            t.Add("(Computer Use) Computer Use 4 (1.5h, 7x/Week)", "computer use");
            t.Add("(Computer Use) Computer Use 5 (1.5h, 3.5x/Week)", "computer use");
            t.Add("(Computer Use) Computer Use of Dual Screen Computer (2.0h, 6x/Day)", "computer use");
            t.Add("(Cooking - Delivered) Eat Delivered Meal instead of cooking (0.5h, 6x/Day)", "meals are delivered");
            t.Add("(Cooking for Evening) Cooking for the Family in the Evening (2.5h, 2x/Day)", "cooking");
            t.Add("(Cooking Lunch) Cooking lunch (2.0h, 2x/Day)", "cooking");
            t.Add("(Cooking Together) Cooking Together in the Evenings (2.0h, 2.4x/Day)", "joint cooking");
            t.Add("(Cooking) Cooking Lunch on the Weekends (2.5h, 2x/Day)", "cooking");
            t.Add("(Cooking) Cooking Together 1 (2.0h, 3.5x/Week)", "joint cooking");
            t.Add("(Cooking) Cooking Together 2 (2.0h, 7x/Week)", "joint cooking");
            t.Add("(Dancing) Dance Class 2 (3.0h, 1.8x/Week)", "dancing");
            t.Add("(Dishwasher) Dishwashing by Hand 2 (0.5h, 2x/Day)", "housework");
            t.Add("(DIY) DIY 1 (2.0h, 7x/Week)", "DIY");
            t.Add("(Doctor Visit) Doctor Visit 2 (4.0h, 1.4x/Week)", "doctor visits");
            t.Add("(Dryer) Dry Laundry outside, Variable controlled (0.5h, 4x/Day)", "housework");
            t.Add("(Dryer) Dry Laundry, Always, Triggered (0.3h, 4x/Day)", "housework");
            t.Add("(Dryer) Dry Laundry, Outside if Warm, VariableControlled (0.4h, 4x/Day)", "housework");
            t.Add("(Exercise) Running 1 (1.0h, 7x/Week)", "running");
            t.Add("(Exercise) Walking, long 2 (2.0h, 3.5x/Week)", "walking");
            t.Add("(Festival Visit) Festival Visit (48.0h, 9.1x/Year)", "festival visits");
            t.Add("(Garden Play) Garden Play (1.5h, 3.5x/Week)", "gardening");
            t.Add("(Gardening) Gardening, every day 1 (2.0h, 7x/Week)", "gardening");
            t.Add("(Gardening) Gardening, every day 3 (2.0h, 3.5x/Week)", "gardening");
            t.Add("(General Internet) General Internet (2.0h, 7x/Week)", "Internet browsing");
            t.Add("(Have Coffee with Friends) Invite Friends for Coffee 1 (3.0h, 1.8x/Week)", "meeting friends");
            t.Add("(Homework) Homework Childrens room 1 (1.0h, 7x/Week)", "homework");
            t.Add("(Horse Riding) Horse Riding 1 (3.0h, 1.8x/Week)", "horse riding");
            t.Add("(Ironing) Ironing 1 (1.0h, 4x/Day)", "housework");
            t.Add("(Kindergarden) Kindergarden (6.0h, 1.5x/Day)", "Kindergarden");
            t.Add("(Laptop Use) Laptop Use 1 (1.5h, 1.7x/Day)", "laptop use");
            t.Add("(Laptop Use) Laptop Use 2 (1.5h, 6x/Day)", "laptop use");
            t.Add("(Maid Service) Maid Service (16.7h, 4x/Day)", "maid service");
            t.Add("(Mini Washing Machine Laundry) Mini Washing Machine Laundry variable controlled (0.3h, 4x/Day)",
                "housework");
            t.Add("(Movie Home Cinema) Watch Movie in Home Cinema 1 (1.8h, 2x/Day)", "watch movies");
            t.Add("(Movie Home Cinema) Watch Movie in Home Cinema 2 (1.8h, 7x/Week)", "watch movies");
            t.Add("(Movie Home Cinema) Watch Movie in Home Cinema 3 (1.8h, 3.5x/Week)", "watch movies");
            t.Add("(Music Instrument Practice) Digital Piano Playing 1 (1.0h, 7x/Week)", "piano playing");
            t.Add("(Music Instrument Practice) Music Instrument Practice 1 (1.0h, 7x/Week)", "music instrument");
            t.Add("(Music Listening) Music Listening on Active speakers (1.8h, 3.5x/Week)", "listening to music");
            t.Add("(Music Listening) Music Listening on Hifi 2 (2.0h, 3.5x/Week)", "listening to music");
            t.Add("(Napping) Napping (1.0h, 2.4x/Day)", string.Empty);
            t.Add("(Napping) Weekend Napping (2.0h, 2.4x/Day)", string.Empty);
            t.Add("(Novel Writing) Novel Writing 1 (2.0h, 3.5x/Week)", "novel writing");
            t.Add("(Office - Computer) Office - Use Computer (1.5h, 48x/Day)", "computer use");
            t.Add("(Office - Go Home) Leave Office around 16:00 (41.0h, 1.5x/Day)", string.Empty);
            t.Add("(Office - Meeting) Office - Meetings (0.8h, 2x/Day)", string.Empty);
            t.Add("(Office - Phone) Office Phone (0.2h, 6x/Day)", string.Empty);
            t.Add("(Office - Sickness) Office - Sickness (24.0h, 2x/Day)", string.Empty);
            t.Add("(Play with Toys) Play with Toys Child 1 (1.3h, 4x/Day)", "playing");
            t.Add("(Playstation Children Room) Playstation Children Room 1 (1.0h, 7x/Week)", "console gaming");
            t.Add("(Playstation Living Room) Playstation Living Room 1 (1.0h, 3x/Day)", "console gaming");
            t.Add("(Programming) Computer Programming 1 (2.0h, 3.5x/Week)", "programming");
            t.Add("(Programming) Programming, Children Room 1 (2.0h, 4x/Day)", "programming");
            t.Add("(Read a Book) Read a book 2 (1.0h, 7x/Week)", "reading");
            t.Add("(Read a Book) Read a book 4 (1.0h, 2x/Day)", "reading");
            t.Add("(Read a Book) Romance Novel Reading 3 (1.0h, 2x/Day)", "reading");
            t.Add("(Relax in the Garden) Relax in the Garden (1.5h, 3.5x/Week)", "gardening");
            t.Add("(School) Grammer School 1 (6.0h, 1.5x/Day)", "grammar school");
            t.Add("(School) Grammer School 2 (6.0h, 1.5x/Day)", "Grammar school");
            t.Add("(School) Mechanical Enginering Studies (6.0h, 7x/Week)", "studying");
            t.Add("(School) Philosophy Studies (6.0h, 1.5x/Day)", "studying");
            t.Add("(School) Primary school 1 (6.0h, 1.5x/Day)", "Primary school");
            t.Add("(School) Primary school 2 (6.0h, 1.5x/Day)", "Primary school");
            t.Add("(School) Primary school With Vacation (6.0h, 2x/Day)", "Primary school");
            t.Add("(School) School 1 (6.0h, 2x/Day)", "School");
            t.Add("(School) School 2 (6.0h, 1.5x/Day)", "School");
            t.Add("(School) Sports Studies (6.0h, 2x/Day)", "studying");
            t.Add("(Sewing) Sewing 1 (2.0h, 7x/Week)", "sewing");
            t.Add("(Showering) Showering with electric Air Heater (1.0h, 7x/Week)", string.Empty);
            t.Add("(Sickness Activities) Sickness Activities for Children (1.3h, 2.1x/Day)", string.Empty);
            t.Add("(Sickness Activities) Sickness Activities to keep busy while sick 2 (1.3h, 1.6x/Day)", string.Empty);
            t.Add("(Sickness Activities) Sickness Activities to keep busy while sick 3 (1.3h, 1.3x/Day)", string.Empty);
            t.Add("(Singing Lesson) Singing Lessons 2 (2.0h, 3.5x/Week)", "singing");
            t.Add("(Sleep) Shiftworker Sleep 8h Bed 06 (5.5h, 2x/Day)", "rotating 8h sleeping schedule");
            t.Add("(Sleep) Shiftworker Sleep 8h Bed 07 (5.5h, 2x/Day)", "rotating 8h sleeping schedule");
            t.Add("(Sleep) Sleep Bed 01 06h (6.0h, 4x/Day)", "6h sleeping");
            t.Add("(Sleep) Sleep Bed 01 08h (8.0h, 4x/Day)", "8h sleeping");
            t.Add("(Sleep) Sleep Bed 01 10h (10.0h, 4x/Day)", "10h sleeping");
            t.Add("(Sleep) Sleep Bed 02 06h (6.0h, 4x/Day)", "6h sleeping");
            t.Add("(Sleep) Sleep Bed 02 08h (8.0h, 4x/Day)", "8h sleeping");
            t.Add("(Sleep) Sleep Bed 02 10h (10.0h, 4x/Day)", "10h sleeping");
            t.Add("(Sleep) Sleep Bed 03 08h Child (8.0h, 2x/Day)", "8h sleeping");
            t.Add("(Sleep) Sleep bed 03 10h Child (10.0h, 2x/Day)", "10h sleeping");
            t.Add("(Sleep) Sleep Bed 03 12h Child (12.0h, 2x/Day)", "12h sleeping");
            t.Add("(Sleep) Sleep Bed 04 10h Child (10.0h, 2x/Day)", "10h sleeping");
            t.Add("(Sleep) Sleep Bed 04 10h, after dark (10.0h, 4x/Day)", "10h sleeping");
            t.Add("(Sleep) Sleep Bed 05 10h Child (11.0h, 2x/Day)", "10h sleeping");
            t.Add("(Sleep) Sleep Bed 08 06h (6.0h, 4x/Day)", "6h sleeping");
            t.Add("(Sleep) Sleep Bed 08 08h (8.0h, 4x/Day)", "8h sleeping");
            t.Add("(Sleep) Sleep Bed 09 06h (6.0h, 4x/Day)", "6h sleeping");
            t.Add("(Smartphone) Smartphone 1 (0.8h, 7x/Week)", string.Empty);
            t.Add("(Smartphone) Smartphone 2 (0.8h, 7x/Week)", string.Empty);
            t.Add("(Study at Home) Study at Home (1.0h, 7x/Week)", "studying");
            t.Add("(Summer Camp) Summer Camp, 1 week (168.0h, 4.4x/Year)", "visit summer camp");
            t.Add("(Summer Camp) Summer Camp, 2 weeks (336.0h, 1.5x/Year)", "visit summer camp");
            t.Add("(Swimming Outdoor) Swimming Outdoor (6.0h, 1.8x/Week)", "swimming");
            t.Add("(Talking on the Phone) Talking on the Phone 4 (0.5h, 2x/Day)", string.Empty);
            t.Add("(TV Home Cinema) Home Cinema 1 with Entertainment desire (1.4h, 6x/Day)", "watch movies");
            t.Add("(TV Home Cinema) Home Cinema 2 (1.4h, 6x/Day)", "watch movies");
            t.Add("(TV Home Cinema) Home Cinema 2(1.4h, 6x / Day)", "watch movies");
            t.Add("(TV Home Cinema) Home Cinema 3 (1.4h, 3x/Day)", "watch movies");
            t.Add("(TV Home Cinema) Home Cinema 4 (1.4h, 1.5x/Day)", "watch movies");
            t.Add("(TV Series) TV Series Watching (1.0h, 7x/Week)", "watch TV");
            t.Add("(TV) Watch TV Children Room_1 2 (1.1h, 2x/Day)", "watch TV");
            t.Add("(TV) Watch TV Children Room_2 3 (1.1h, 7x/Week)", "watch TV");
            t.Add("(TV) Watch TV Children Room_2 4 (1.1h, 2x/Day)", "watch TV");
            t.Add("(TV) Watch TV Living Room 1 (1.4h, 3x/Day)", "watch TV");
            t.Add("(Video Game Console XBox) Xbox Gaming 2 (1.0h, 2x/Day)", "console gaming");
            t.Add("(Volunteer Work) Volunteer Work 1 (3.0h, 7x/Week)", "volunteer work");
            t.Add("(Volunteer Work) Volunteer Work 2 (3.0h, 3.5x/Week)", "volunteer work");
            t.Add("(Volunteer Work) Volunteer Work 3 (3.0h, 1.8x/Week)", "volunteer");
            t.Add("(Walking) Walking, long 1 (2.0h, 7x/Week)", "walking");
            t.Add("(Work) Part Time Job (3.0h, 1.5x/Day)", "part time job");
            t.Add("(Work) Work - Office 1, 09h (9.0h, 3x/Day)", "9h office work");
            t.Add("(Work) Work - Office 1, 09h from 6:30 (9.0h, 3x/Day)", "9h office work");
            t.Add("(Work) Work - Office 2, 09h (9.0h, 3x/Day)", "9h office work");
            t.Add("(Work) Work - Office, 08h (8.0h, 3x/Day)", "8h office work");
            t.Add("(Work) Work - Office, 10h (10.0h, 3x/Day)", "10h office work");
            t.Add("(Work) Work - Office, 11h (11.0h, 3x/Day)", "11h office work");
            t.Add("(Work) Work - Shiftwork, 3 Shifts, rotating, Person 1, man (9.0h, 7x/Week)", "working in 3 shifts");
            t.Add("(Work) Work - Shiftwork, 3 Shifts, rotating, Person 2, woman (9.0h, 2x/Day)", "working in 3 shifts");
            t.Add("(Work) Work - Teacher School 1 (9.0h, 1.5x/Day)", "working as teacher");
            t.Add("(Work) Work - Teacher School 2 (9.0h, 1.5x/Day)", "working as teacher");
            t.Add("(Breakfast) Breakfast (1.1h, 2.2x/Day)", string.Empty);
            t.Add("(Breakfast) Breakfast, Interrupting other activities, but no integrated alarm clock (0.4h, 2x/Day)",
                "breakfast");
            t.Add("(Walking) Walking, long 2 (2.0h, 3.5x/Week)", "walking");

            return t;
        }*/

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static void FillPersonDescriptions([JetBrains.Annotations.NotNull] Simulator sim) {
            //int year = 365*24*60*60;
            //Dictionary<string, string> dict = GoodNames();
            //List<HouseholdTrait> missingtraits = new List<HouseholdTrait>();

            foreach (var pe in sim.Persons.Items) {
                var simPerson = pe;
                if (simPerson.Description == "(no description)") {
                    simPerson.Description = string.Empty;
                }

                var desc = simPerson.Gender + "/" + simPerson.Age + ", ";

                var chh = sim.ModularHouseholds.Items.First(x => x.PurePersons.Contains(simPerson));
                var traits = chh.Traits.Where(x => x.DstPerson == simPerson)
                    .Select(x => x.HouseholdTrait).ToList();
                var importantTraits = new List<HouseholdTrait>();
                foreach (var trait in traits) {
                    //trait.CalculateEstimatedTimes();
                    //double timesPerYear = year/trait.EstimatedTimeInSeconds;
                    //double estimatedDuration = trait.EstimateDuration();
                    //double totalDuration = estimatedDuration*timesPerYear;
                    var percentage = trait.EstimatedTimePerYearInH / 8760;
                    if (percentage > 0.03) {
                        if (trait.ShortDescription != "-") {
                            if (!string.IsNullOrWhiteSpace(trait.ShortDescription)) {
                                importantTraits.Add(trait);
                            }
                            else {
                                throw new LPGException("The trait " + trait.PrettyName +
                                                       " is missing a short description. The short descriptions are used to generate the person description and are required for all " +
                                                       " activities that take more than 3% of the total time per year of the respective person.");
                            }
                        }
                    }
                }
                if (importantTraits.All(x => x.Name.Length > 0)) {
                    var importantDescs = importantTraits.Select(x => x.ShortDescription).ToList();
                    importantDescs = importantDescs.Distinct().OrderBy(x => x).ToList();
                    var sb = new StringBuilder();
                    foreach (var s in importantDescs) {
                        sb.Append(s);
                        sb.Append(", ");
                    }

                    desc += " " + sb.ToString().Substring(0, sb.Length - 2);
                    while (desc.Contains("  ")) {
                        desc = desc.Replace("  ", " ");
                    }
                    simPerson.Description = desc;
                    Logger.Info("New Description for " + simPerson.PrettyName + ": " + desc);
                    simPerson.SaveToDB();
                }
            }
        }
    }
}