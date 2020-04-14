using System;
using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Helpers {
    public class LivingPatternVariationCreator {
        private void AddAffordanceEntry([NotNull] Simulator sim, [ItemNotNull] [NotNull] List<AffordanceEntry> affordances, [NotNull] string affname,
            [NotNull] string newName, double duration)
        {
            var aff = sim.Affordances.FindFirstByName(affname);
            if (aff == null) {
                throw new LPGException("Affordance not found: " + affname);
            }
            affordances.Add(new AffordanceEntry(aff, newName,duration));
        }

        private void AddTimeLimitEntry([NotNull] Simulator sim, [ItemNotNull] [NotNull] List<TimeLimitEntry> timeLimits,
                                       [NotNull] string timelimitName,
            [NotNull] string newName, [CanBeNull] string tagName, TimeSpan beginSpan )
        {
            var aff = sim.TimeLimits.FindFirstByName(timelimitName);
            if (aff == null)
            {
                throw new LPGException("Time Limit not found: " + timelimitName);
            }

            TraitTag tag = GetTraitTagByName(sim, tagName);
            timeLimits.Add(new TimeLimitEntry(newName, aff, tag, beginSpan));
        }

        [CanBeNull]
        private static TraitTag GetTraitTagByName([NotNull] Simulator sim, [CanBeNull] string tagName)
        {
            TraitTag tag = null;
            if (tagName != null)
            {
                tag = sim.TraitTags.FindFirstByName(tagName);
                if (tag == null)
                {
                    throw new LPGException("Tagname " + tagName + " not found.");
                }
            }

            return tag;
        }

        public void RunOfficeJobs([NotNull] Simulator sim)
        {
            //office 1
            //find the base thread
            var basetrait = sim.HouseholdTraits.FindFirstByName("Work - Office 1, 08h, from 06:00");
            const string baseName = "Work - Office 1";

            // get all the affordances
            var affordances = new List<AffordanceEntry>();
            AddAffordanceEntry(sim, affordances, "work at the office (06 h)", "06h",6);
            AddAffordanceEntry(sim, affordances, "work at the office (07 h)", "07h",7);
            AddAffordanceEntry(sim, affordances, "work at the office (08 h)", "08h",8);
            AddAffordanceEntry(sim, affordances, "work at the office (09 h)", "09h",9);
            AddAffordanceEntry(sim, affordances, "work at the office (10 h)", "10h",10);
            AddAffordanceEntry(sim, affordances, "work at the office (11 h)", "11h",11);
            // get all the time limits
            var timeLimits = new List<TimeLimitEntry>();

            //early
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 05:00 and 07:00", "05:00",
                "Living Pattern / Office Job / Early (5-7am)",TimeSpan.FromHours(5));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 06:00 and 08:00", "06:00",
                "Living Pattern / Office Job / Early (5-7am)", TimeSpan.FromHours(6));
            //medium

            AddTimeLimitEntry(sim, timeLimits, "Every workday between 07:00 and 09:00", "07:00",
                "Living Pattern / Office Job / Medium (7-9am)", TimeSpan.FromHours(7));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 08:00 and 10:00", "08:00",
                "Living Pattern / Office Job / Medium (7-9am)", TimeSpan.FromHours(8));
            //late
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 09:00 and 11:00", "09:00",
                "Living Pattern / Office Job / Late (9-11am)", TimeSpan.FromHours(9));

            AddTimeLimitEntry(sim, timeLimits, "Every workday between 10:00 and 12:00", "10:00",
                "Living Pattern / Office Job / Late (9-11am)", TimeSpan.FromHours(10));

            GenerateNewTraits(sim, basetrait, baseName, affordances, timeLimits, TaggingType.OfficeJob);

            //office 2
            //find the base thread
            var basetrait2 = sim.HouseholdTraits.FindFirstByName("Work - Office 2, 09h");
            const string baseName2 = "Work - Office 2";

            GenerateNewTraits(sim, basetrait2, baseName2, affordances, timeLimits, TaggingType.OfficeJob);
        }

        public void RunSleep([NotNull] Simulator sim)
        {
            //office 1
            //find the base thread
            var basetrait = sim.HouseholdTraits.FindFirstByName("Sleep Bed 01 06h");
            const string baseName = "Sleep Bed 01";

            // get all the affordances
            var affordances = new List<AffordanceEntry>();
            AddAffordanceEntry(sim, affordances, "sleep bed 01 (06 h)", "06h",6);
            AddAffordanceEntry(sim, affordances, "sleep bed 01 (07 h)", "07h",7);
            AddAffordanceEntry(sim, affordances, "sleep bed 01 (08 h)", "08h",8);
            AddAffordanceEntry(sim, affordances, "sleep bed 01 (09 h)", "09h",9);
            AddAffordanceEntry(sim, affordances, "sleep bed 01 (10 h)", "10h",10);
            // get all the time limits
            var timeLimits = new List<TimeLimitEntry>();

            //early
            AddTimeLimitEntry(sim, timeLimits, "Every day between 19:00 and 01:00", "19:00", null, TimeSpan.FromHours(19));
            AddTimeLimitEntry(sim, timeLimits, "Every day between 20:00 and 02:00", "20:00", null, TimeSpan.FromHours(20));
            AddTimeLimitEntry(sim, timeLimits, "Every day between 21:00 and 03:00", "21:00", null, TimeSpan.FromHours(21));
            AddTimeLimitEntry(sim, timeLimits, "Every day between 22:00 and 04:00", "22:00", null, TimeSpan.FromHours(22));
            AddTimeLimitEntry(sim, timeLimits, "Every day between 23:00 and 05:00", "23:00", null, TimeSpan.FromHours(23));
            AddTimeLimitEntry(sim, timeLimits, "Every day between 00:00 and 06:00", "00:00", null, TimeSpan.FromHours(24));
            AddTimeLimitEntry(sim, timeLimits, "Every day between 01:00 and 07:00", "01:00", null, TimeSpan.FromHours(1));
            AddTimeLimitEntry(sim, timeLimits, "Every day between 02:00 and 08:00", "02:00", null, TimeSpan.FromHours(2));
            AddTimeLimitEntry(sim, timeLimits, "Every day between 03:00 and 09:00", "03:00", null, TimeSpan.FromHours(3));

            GenerateNewTraits(sim, basetrait, baseName, affordances, timeLimits,TaggingType.AdultSleep);

            var basetrait2 = sim.HouseholdTraits.FindFirstByName("Sleep Bed 02 06h");
            const string baseName2 = "Sleep Bed 02";

            // get all the affordances
            var affordance2 = new List<AffordanceEntry>();
            AddAffordanceEntry(sim, affordance2, "sleep bed 02 (06 h)", "06h", 6);
            AddAffordanceEntry(sim, affordance2, "sleep bed 02 (07 h)", "07h", 7);
            AddAffordanceEntry(sim, affordance2, "sleep bed 02 (08 h)", "08h", 8);
            AddAffordanceEntry(sim, affordance2, "sleep bed 02 (09 h)", "09h", 9);
            AddAffordanceEntry(sim, affordance2, "sleep bed 02 (10 h)", "10h", 10);

            GenerateNewTraits(sim, basetrait2, baseName2, affordance2, timeLimits,TaggingType.AdultSleep);
        }

        public void RunAlarm([NotNull] Simulator sim)
        {
            //office 1
            //find the base thread
            var basetrait = sim.HouseholdTraits.FindFirstByName("Alarm clock A at 05:00");
            const string baseName = "Alarm clock ";

            // get all the affordances
            var affordances = new List<AffordanceEntry>();
            AddAffordanceEntry(sim, affordances, "wake up by alarm clock A", "Alarm A",-1);

            // get all the time limits
            var timeLimits = new List<TimeLimitEntry>();

            //early
            AddTimeLimitEntry(sim: sim, timeLimits: timeLimits, timelimitName: "Every workday between 05:00 and 05:30", newName: "05:00", tagName: null,beginSpan: TimeSpan.FromHours(5));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 05:15 and 05:30 vary by 15 minutes", "05:15+-15", null, TimeSpan.FromHours(5.25));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 06:00 and 06:30", "06:00", null, TimeSpan.FromHours(6));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 06:15 and 06:45 vary by 15 minutes", "6:15+-15", null, TimeSpan.FromHours(6.25));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 06:30 and 07:00", "06:30", null, TimeSpan.FromHours(6.5));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 07:00 and 07:30", "07:00", null, TimeSpan.FromHours(7));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 07:30 and 08:00", "07:30", null, TimeSpan.FromHours(7.5));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 08:00 and 08:30", "08:00", null, TimeSpan.FromHours(8));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 08:30 and 09:00", "08:30", null, TimeSpan.FromHours(8.5));
            AddTimeLimitEntry(sim, timeLimits, "Every workday between 09:00 and 09:30", "09:00", null, TimeSpan.FromHours(9));

            GenerateNewTraits(sim, basetrait, baseName, affordances, timeLimits, TaggingType.AlarmClock);

            var basetrait2 = sim.HouseholdTraits.FindFirstByName("Alarm clock B at 06:30");
            var affordances2 = new List<AffordanceEntry>();
            AddAffordanceEntry(sim, affordances2, "wake up by alarm clock B", "Alarm B", -1);

            GenerateNewTraits(sim, basetrait2, baseName, affordances2, timeLimits, TaggingType.AlarmClock);
        }

        private enum TaggingType {
            OfficeJob,
            AdultSleep,
            AlarmClock
        }
        private static void GenerateNewTraits([NotNull] Simulator sim,[CanBeNull] HouseholdTrait basetrait, [NotNull] string baseName,
            [ItemNotNull] [NotNull] List<AffordanceEntry> affordances, [ItemNotNull] [NotNull] List<TimeLimitEntry> timeLimits, TaggingType taggingType)
        {
            if (basetrait == null)
            {
                throw new LPGException("basetrait not found");
            }
            if (basetrait.Desires.Count != 1)
            {
                throw new LPGException("Not exactly one desire in " + basetrait.PrettyName);
            }

            //TODO: make sure all the affordances have different desires
            var uniquedesires = new List<Desire>();
            foreach (var affordance in affordances)
            {
                foreach (var affordanceDesire in affordance.Affordance.AffordanceDesires)
                {
                    var d = affordanceDesire.Desire;
                    if ( uniquedesires.Contains(d))
                    {
                        throw new LPGException("The desire " + d.PrettyName +
                                               " is used in multiple affordances. Not ok.");
                    }
                    uniquedesires.Add(affordanceDesire.Desire);
                }
            }

            foreach (var affordanceEntry in affordances)
            {
                foreach (var timeLimitEntry in timeLimits)
                {
                    var newName = baseName + ", " + affordanceEntry.Name + ", from " + timeLimitEntry.Name;
                    if (sim.HouseholdTraits.FindFirstByName(newName) != null)
                    {
                        continue;
                    }
                    var newTrait = basetrait.MakeCopy(sim);
                    newTrait.Name = newName;
                    var aff = affordanceEntry.Affordance;

                    //desires
                    newTrait.RemoveDesire(newTrait.Desires[0]);
                    if (newTrait.Desires.Count != 0)
                    {
                        throw new LPGException("Too many desires");
                    }
                    var oldDesire = basetrait.Desires[0];
                    newTrait.AddDesire(aff.AffordanceDesires[0].Desire, oldDesire.DecayTime, oldDesire.HealthStatus,
                        oldDesire.Threshold, oldDesire.Weight, oldDesire.MinAge, oldDesire.MaxAge, oldDesire.Gender);
                    //location
                    if (newTrait.Locations.Count != 1)
                    {
                        throw new LPGException("More than one location");
                    }
                    var hhtloc = newTrait.Locations.First();

                    if (hhtloc.AffordanceLocations.Count != 1)
                    {
                        throw new LPGException("Not exactly one location");
                    }
                    newTrait.DeleteAffordanceFromDB(hhtloc.AffordanceLocations.First());
                    newTrait.AddAffordanceToLocation(hhtloc, affordanceEntry.Affordance, timeLimitEntry.TimeLimit, 100,15,15,15,15);
                    // Tag
                    if (taggingType == TaggingType.OfficeJob)
                    {
                        TagOfficeJob(timeLimitEntry, newTrait);
                    }else if (taggingType == TaggingType.AdultSleep)
                    {
                        TagAdultSleep(sim, affordanceEntry, timeLimitEntry, newTrait);
                    }
                    else if (taggingType == TaggingType.AlarmClock)
                    {
                        TagAlarms(sim,  timeLimitEntry, newTrait);
                    }
                    else {
                        throw new LPGException("Unknown tagging type");
                    }
                    newTrait.SaveToDB();
                }
            }
        }

        private static void TagAlarms([NotNull] Simulator sim, [NotNull] TimeLimitEntry timeLimitEntry, [NotNull] HouseholdTrait newTrait)
        {
            var livingTags = newTrait.Tags.Where(x => x.Tag.Name.Contains("Living Pattern")).ToList();
            foreach (HHTTag livingTag in livingTags)
            {
                newTrait.DeleteHHTTag(livingTag);
            }

            TimeSpan endTime = timeLimitEntry.BeginTime;
            List<TraitTag> tags = new List<TraitTag>
            {
                GetTraitTagByName(sim, "Living Pattern / Part Time Job"),
                GetTraitTagByName(sim, "Living Pattern / Retiree"),
                GetTraitTagByName(sim, "Living Pattern / Stay at Home / Regular"),
                GetTraitTagByName(sim, "Living Pattern / Two Shift Work"),
                GetTraitTagByName(sim, "Living Pattern / University / Student Independent"),
                GetTraitTagByName(sim, "Living Pattern / University / Student Living at Home")
            };
            if (endTime.Hours <= 7)
            {
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Early (5-7am)"));
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Medium (7-9am)"));
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Late (9-11am)"));
            }
            else if (endTime.Hours <= 8)
            {
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Medium (7-9am)"));
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Late (9-11am)"));
            }
            else
            {
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Late (9-11am)"));
            }
            foreach (TraitTag tag in tags)
            {
                if (tag == null)
                {
                    throw new LPGException("Tag not found");
                }
                newTrait.AddTag(tag);
            }
        }

        private static void TagAdultSleep([NotNull] Simulator sim, [NotNull] AffordanceEntry affordanceEntry, [NotNull] TimeLimitEntry timeLimitEntry, [NotNull] HouseholdTrait newTrait)
        {
            var livingTags = newTrait.Tags.Where(x => x.Tag.Name.Contains("Living Pattern / Office Job")).ToList();
            foreach (HHTTag livingTag in livingTags) {
                newTrait.DeleteHHTTag(livingTag);
            }
            TimeSpan endTime = timeLimitEntry.BeginTime.Add(TimeSpan.FromHours(affordanceEntry.Duration));
            List<TraitTag> tags = new List<TraitTag>
            {
                GetTraitTagByName(sim, "Living Pattern / Part Time Job"),
                GetTraitTagByName(sim, "Living Pattern / Retiree"),
                GetTraitTagByName(sim, "Living Pattern / Stay at Home / Regular"),
                GetTraitTagByName(sim, "Living Pattern / Two Shift Work"),
                GetTraitTagByName(sim, "Living Pattern / University / Student Independent"),
                GetTraitTagByName(sim, "Living Pattern / University / Student Living at Home")
            };
            if (endTime.Hours <= 6)
            {
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Early (5-7am)"));
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Medium (7-9am)"));
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Late (9-11am)"));
            }
            else if (endTime.Hours <= 8)
            {
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Medium (7-9am)"));
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Late (9-11am)"));
            }
            else
            {
                tags.Add(GetTraitTagByName(sim, "Living Pattern / Office Job / Late (9-11am)"));
            }
            foreach (TraitTag tag in tags)
            {
                if (tag == null)
                {
                    throw new LPGException("Tag not found");
                }
                newTrait.AddTag(tag);
            }
        }

        private static void TagOfficeJob([NotNull] TimeLimitEntry timeLimitEntry, [NotNull] HouseholdTrait newTrait)
        {
            var livingTags = newTrait.Tags.Where(x => x.Tag.Name.Contains("Living Pattern / Office Job")).ToList();
            if (livingTags.Count != 1)
            {
                throw new LPGException("More than one living pattern");
            }
            newTrait.DeleteHHTTag(livingTags[0]);
            if (timeLimitEntry.Tag == null)
            {
                throw new LPGException("Tag was null");
            }
            newTrait.AddTag(timeLimitEntry.Tag);
        }

        private class AffordanceEntry {
            public AffordanceEntry([NotNull] Affordance affordance, [NotNull] string name, double duration)
            {
                Affordance = affordance;
                Name = name;
                Duration = duration;
            }

            [NotNull]
            public Affordance Affordance { get; }
            [NotNull]
            public string Name { get; }

            public double Duration { get; }
        }
/*
        private class LocationEntry {
            public LocationEntry(string name, Location location)
            {
                Name = name;
                Location = location;
            }

            public Location Location { get; }
            public string Name { get; }
        }*/

        private class TimeLimitEntry {
            public TimeLimitEntry([NotNull] string name,[NotNull] TimeLimit timeLimit, [CanBeNull] TraitTag tag, TimeSpan beginTime)
            {
                Name = name;
                TimeLimit = timeLimit;
                Tag = tag;
                BeginTime = beginTime;
            }

            [NotNull]
            public string Name { get; }
            [CanBeNull]
            public TraitTag Tag { get; }

            [NotNull]
            public TimeLimit TimeLimit { get; }

            public TimeSpan BeginTime { get; }
        }
    }
}