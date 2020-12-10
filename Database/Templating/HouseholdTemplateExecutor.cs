using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo("DatabaseIO.Tests")]

namespace Database.Templating {
    internal static class HouseholdTemplateExecutor {
        [ItemNotNull]
        [NotNull]
        public static List<ModularHousehold> GenerateHouseholds([NotNull] Simulator sim,
                                                                bool generateSettlement,
                                                                [ItemNotNull] [NotNull] List<STTraitLimit> limits,
                                                                [NotNull] HouseholdTemplate template,
                                                                List<TraitTag> forbiddenTraitTags)
        {
            if (template == null) {
                throw new LPGException("No template was assigned when calling the GenerateHouseholds-Function");
            }

            if (template.Persons.Count == 0) {
                Logger.Error("The household template " + template.Name + " has no persons. This isn't going to work.");
                return new List<ModularHousehold>();
            }

            if (template.Vacations.Count == 0 && template.TemplateVacationType == TemplateVacationType.FromList) {
                Logger.Error("The household template " + template.Name + " has no vacations. This isn't going to work.");
                return new List<ModularHousehold>();
            }

            try {
                var r = new Random();
                var min = 1;
                var max = 0;
                var createdHH = new List<ModularHousehold>();
                var numberofPersons = template.Persons.Count;
                for (var i = 0; i < template.Count; i++) {
                    ModularHousehold chh = null;
                    var i1 = i;
                    Logger.Get().SafeExecuteWithWait(() => chh = GenerateEmptyHousehold(sim, template, r, ref min, ref max, i1));
                    createdHH.Add(chh);
                    chh.SaveToDB();
                    AddDesiredTraits(sim, limits, template, r, numberofPersons, chh, forbiddenTraitTags);
                    chh.SaveToDB();
                    foreach (var hhTemplateTag in template.TemplateTags) {
                        chh.AddHouseholdTag(hhTemplateTag.Tag);
                    }

                    template.GeneratedHouseholds.Add(chh);
                    Logger.Info("Created household " + chh.Name);
                }

                MakeSettlement(sim, generateSettlement, template, min, max, createdHH);
                return createdHH;
            }
            catch (Exception e) {
                Logger.Exception(e);
                throw;
            }
        }

        [NotNull]
        internal static Vacation GenerateVacation([NotNull] HouseholdTemplate template,
                                                  [NotNull] Random rnd,
                                                  [NotNull] Simulator sim,
                                                  [NotNull] string hhname)
        {
            switch (template.TemplateVacationType) {
                case TemplateVacationType.FromList:
                    var vacindex = rnd.Next(template.Vacations.Count);
                    if (template.Vacations.Count == 0) {
                        throw new DataIntegrityException("Please set at least one vacation to choose from for this template.");
                    }

                    return template.Vacations[vacindex].Vacation;
                case TemplateVacationType.RandomlyGenerated:
                    var vacationSpan = template.MaxTotalVacationDays - template.MinTotalVacationDays;
                    if (vacationSpan < 1) {
                        throw new LPGException("Possible vacation duration is less than 1. Please fix.");
                    }

                    if (template.AverageVacationDuration == 0) {
                        throw new DataIntegrityException("An average vacation duration of 0 makes no sense.");
                    }

                    if (template.MinNumberOfVacations == 0) {
                        throw new DataIntegrityException("An minimum number of vacations of 0 makes no sense..");
                    }

                    var totalVacationDays = template.MinTotalVacationDays + rnd.Next(vacationSpan);

                    // figure out the duration of the vacations
                    var numberOfVacations = template.MinNumberOfVacations + rnd.Next(template.MaxNumberOfVacations - template.MinNumberOfVacations);
                    var durations = new int[numberOfVacations];
                    for (var i = 0; i < durations.Length; i++) {
                        durations[i] = template.AverageVacationDuration;
                    }

                    var count = 0;
                    while (durations.Sum() < totalVacationDays && count < 1000) {
                        var iToIncreate = rnd.Next(durations.Length);

                        durations[iToIncreate]++;
                        count++;
                    }

                    count = 0;
                    while (durations.Sum() > totalVacationDays && count < 1000) {
                        var iToIncreate = rnd.Next(durations.Length);
                        if (durations[iToIncreate] > 1) {
                            durations[iToIncreate]--;
                        }

                        count++;
                    }

                    if (durations.Sum() != totalVacationDays) {
                        throw new LPGException("Could not reach any distribution of the vacation days with these settings.");
                    }

                    //get the probabilities
                    if (template.TimeProfileForVacations == null) {
                        throw new LPGException(" Time profiles for vacation was null");
                    }

                    var probabilityPickArray = VacationMakeProbabilityArray(template.TimeProfileForVacations.Datapoints);

                    //make the list of vacation entries

                    var targetVacationEntries = new List<VacationEntry>();

                    for (var i = 0; i < durations.Length; i++) {
                        var ve = new VacationEntry {
                            TargetDuration = durations[i]
                        };
                        var dstProbability = rnd.Next(probabilityPickArray.Count);
                        ve.TargetProbability = probabilityPickArray[dstProbability];
                        targetVacationEntries.Add(ve);
                    }

                    // now allocate the entries with specific dates
                    VacationSetDatesToTargetEntries(rnd, targetVacationEntries, template.TimeProfileForVacations);
                    var newVac = sim.Vacations.CreateNewItem(sim.ConnectionString);

                    newVac.MinimumAge = 1;
                    newVac.MaximumAge = 99;
                    var vacTypes = Enum.GetValues(typeof(VacationType)).Length;
                    foreach (var targetVacationEntry in targetVacationEntries) {
                        var vt = (VacationType)rnd.Next(vacTypes);
                        newVac.AddVacationTime(targetVacationEntry.StartDate, targetVacationEntry.EndDate, vt);
                    }

                    newVac.Name = "Generated Vacation for " + hhname;
                    newVac.CreationType = CreationType.TemplateCreated;
                    newVac.SaveToDB();
                    return newVac;
                default: throw new LPGException("Missing TemplateVacationType");
            }
        }

        [ItemNotNull]
        [NotNull]
        internal static List<VacationProbabilityRange> VacationGetProbabilityRanges([NotNull] double[] probabilities, int year)
        {
            //change to probability ranges
            var currentProbability = probabilities[0];
            var lastStart = new DateTime(year, 1, 1);
            var vprs = new List<VacationProbabilityRange>();
            for (var i = 0; i < probabilities.Length; i++) {
                if (Math.Abs(probabilities[i] - currentProbability) > Constants.Ebsilon) {
                    //found a different probabilty, so new entry
                    var currDate = new DateTime(year, 1, 1).AddDays(i);
                    var vpr = new VacationProbabilityRange {
                        Start = lastStart,
                        End = currDate.AddDays(-1)
                    };
                    lastStart = currDate;
                    vpr.Probability = currentProbability;
                    currentProbability = probabilities[i];
                    vprs.Add(vpr);
                }
            }

            var vprLast = new VacationProbabilityRange {
                Start = lastStart,
                End = new DateTime(year + 1, 1, 1),
                Probability = currentProbability
            };
            vprs.Add(vprLast);
            return vprs.Where(x => x.Probability > 0).ToList();
        }

        //only public for unit testing
        [NotNull]
        internal static List<double> VacationMakeProbabilityArray([ItemNotNull] [NotNull] ObservableCollection<DateProfileDataPoint> dataPoints)
        {
            var probabilities = new List<double>();

            for (var dayIndex = 0; dayIndex < dataPoints.Count; dayIndex++) {
                var dp = dataPoints[dayIndex];
                if (!probabilities.Contains(dp.Value)) {
                    probabilities.Add(dp.Value);
                }
            }

            var probsum = probabilities.Sum();
            var adjustedProbabilites = new List<double>(probabilities.Count);
            for (var i = 0; i < probabilities.Count; i++) {
                adjustedProbabilites.Add(probabilities[i] / probsum);
            }

            adjustedProbabilites.Sort((x, y) => y.CompareTo(x));

            //make an array that contains the probability in proper amounts to be able to just pick from it
            var pickArray = new List<double>();
            for (var i = 0; i < adjustedProbabilites.Count; i++) {
                var num = (int)(adjustedProbabilites[i] * 1000);
                for (var j = 0; j < num; j++) {
                    pickArray.Add(adjustedProbabilites[i]);
                }
            }

            return pickArray;
        }

        private static void AddDesiredTraits([NotNull] Simulator sim,
                                             [ItemNotNull] [NotNull] List<STTraitLimit> settlementTraitLimits,
                                             [NotNull] HouseholdTemplate template,
                                             [NotNull] Random rnd,
                                             int numberofPersons,
                                             [NotNull] ModularHousehold chh,
                                             List<TraitTag> forbiddenTraitTags)
        {
            // find the living pattern all tag for speed
            var allTag = sim.LivingPatternTags.Items.First(x => x.Name == "Living Pattern / All");
            if (allTag == null) {
                throw new LPGException("The tag Living Pattern / All was not found.");
            }

            //add the traits
            foreach (var entry in template.Entries) {
                var potentialTraits = sim.HouseholdTraits.Items.Where(t =>
                    t.Tags.Any(tag => tag.Tag == entry.TraitTag) && numberofPersons >= t.MinimumPersonsInCHH &&
                    numberofPersons <= t.MaximumPersonsInCHH).ToList();
                if (potentialTraits.Count == 0) {
                    throw new DataIntegrityException("The tag " + entry.TraitTag.Name +
                                                     " has no household traits associated with it that would fit this household. This is not going to work.");
                }

                var variableNumberofTraits = entry.TraitCountMax - entry.TraitCountMin;
                var numberofTraits = rnd.Next(variableNumberofTraits) + entry.TraitCountMin;
                var successes = 0;
                var traitexists = 0;
                var classificationexists = 0;
                var invalidForPerson = 0;
                var violatesLimit = 0;
                for (var j = 0; j < numberofTraits; j++) {
                    AssignSingleTrait(settlementTraitLimits, template, rnd, chh,
                        forbiddenTraitTags, entry, potentialTraits, allTag,
                        ref traitexists, ref classificationexists, ref invalidForPerson,
                        ref violatesLimit, ref successes);
                }

                if (successes != numberofTraits) {
                    var persons = string.Empty;
                    var builder = new StringBuilder();
                    builder.Append(persons);
                    foreach (var person in entry.Persons) {
                        builder.Append(person.PrettyName).Append(", ");
                    }

                    persons = builder.ToString();
                    persons = persons.Substring(0, persons.Length - 1);
                    var totalcount = traitexists + classificationexists + invalidForPerson;
                    var traitExistsPercentage = (double)traitexists / totalcount * 100.0;
                    var classificationExistsPercentage = (double)classificationexists / totalcount * 100.0;
                    var invalidForPersonpercentage = (double)invalidForPerson / totalcount * 100.0;
                    var limitViolationPercentage = (double)violatesLimit / totalcount * 100;
                    var reasonString = "Reasons: \n\tTrait already exits up to the trait count limit: " + traitExistsPercentage.ToString("N1", CultureInfo.CurrentCulture) +
                                       "%,\n\tClassification exists up to the classificaiton limit: " + classificationExistsPercentage.ToString("N1", CultureInfo.CurrentCulture) +
                                       "%,\n\tRandomly chosen Trait unsuitable for person: " + invalidForPersonpercentage.ToString("N1", CultureInfo.CurrentCulture) +
                                       "%,\n\tTrait violates age/gender limits: " + limitViolationPercentage.ToString("N1", CultureInfo.CurrentCulture) + "%";
                    Logger.Warning("Only found " + successes + " out of " + numberofTraits + " traits for " + entry.TraitTag.PrettyName +
                                   " for the person " + persons + "." + Environment.NewLine + reasonString);
                    if (entry.IsMandatory)
                    {
                        throw new LPGException("Tried to assign " + entry.TraitTag.Name + " but didn't succeed although the trait is marked as manadatory. " +
                                               "Maybe you need to have more kind of work, school or sleep? " +
                                               "Reasons for failure during 100 tries to find a match: \n" + reasonString);
                    }
                }
            }
        }

        private static void AssignSingleTrait([NotNull] List<STTraitLimit> settlementTraitLimits, HouseholdTemplate template, [NotNull] Random rnd, [NotNull] ModularHousehold chh,
                                              List<TraitTag> forbiddenTraitTags, [NotNull] HHTemplateEntry entry, [NotNull] List<HouseholdTrait> potentialTraits, LivingPatternTag allTag,ref int traitexists,
                                              ref int classificationexists, ref int invalidForPerson, ref int violatesLimit, ref int successes)
        {
            var personnumber = rnd.Next(entry.Persons.Count);
            var person = entry.Persons[personnumber].Person;
            var dstLivingPattern = chh.Persons.First(x => x.Person == person).LivingPatternTag;
            if (dstLivingPattern == null) {
                throw new LPGException("The living pattern was not set.");
            }

            //do this explicitly and with seperate lists for debugging purposes
            //filter out the wrong living patterns
            //needs to use the startWith
            var traitswithRightTag = potentialTraits.Where(x =>
                //any trait that has the same living pattern
                x.LivingPatternTags.Any(y => dstLivingPattern.Name.StartsWith(y.Name)));
            //or has the all pattern tag
            var traitsWithAllTag = potentialTraits.Where(x => x.LivingPatternTags.Any(y => y.Tag == allTag));
            //or doesn't have a living pattern tag
            var traitsWithNoTag = potentialTraits.Where(x => x.LivingPatternTags.Count == 0).ToList();
            var filteredTraits1 = new List<HouseholdTrait>();
            filteredTraits1.AddRange(traitswithRightTag);
            filteredTraits1.AddRange(traitsWithAllTag);
            filteredTraits1.AddRange(traitsWithNoTag);
            filteredTraits1 = filteredTraits1.Distinct().ToList();
            if (filteredTraits1.Count == 0) {
                string s = "Not a single trait found for entry " + entry.TraitTag.Name + " and the living pattern tag " + dstLivingPattern.Name +
                           " in the household template " + template.Name + ". The following traits are available with their tags:";
                throw new DataIntegrityException(s);
            }

            var ft2 = new List<HouseholdTrait>();
            foreach (var trait1 in filteredTraits1) {
                bool foundForbidden = false;
                foreach (var tag in forbiddenTraitTags) {
                    if (trait1.Tags.Any(x => x.Tag == tag)) {
                        foundForbidden = true;
                    }
                }

                if (!foundForbidden) {
                    ft2.Add(trait1);
                }
            }

            PickATrait(rnd, chh, ft2, ref traitexists, ref classificationexists, ref invalidForPerson, person, out var trait, settlementTraitLimits,
                ref violatesLimit, entry.TraitTag);

            if (trait != null) {
                var dstLivingPattern1 = chh.Persons.Single(x => x.Person == person).LivingPatternTag?.Name;
                var traitlptags = trait.LivingPatternTags.Select(x => x.Name).ToList();
                CheckTraitAssigment(traitlptags, dstLivingPattern1, allTag);
                chh.AddTrait(trait, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name, person);
                //settlementTraitLimits is the function to avoid too many brunches in a settlement for example
                settlementTraitLimits.ForEach(x => x.RegisterTrait(trait));
                successes++;
            }
        }

        private static void CheckTraitAssigment([NotNull] List<string> traitlptags, string dstLivingPattern1, [NotNull] LivingPatternTag alltag)
        {
            if (traitlptags.Contains(alltag.Name)) {
                return;
            }
            if (traitlptags.Contains(dstLivingPattern1)) {
                return;
            }

            foreach (var traitlptag in traitlptags) {
                if (dstLivingPattern1.StartsWith(traitlptag)) {
                    return;
                }
            }
            throw new LPGException("Assigning trait to wrong person");
        }

        [NotNull]
        private static ModularHousehold GenerateEmptyHousehold([NotNull] Simulator sim,
                                                               [NotNull] HouseholdTemplate template,
                                                               [NotNull] Random r,
                                                               ref int min,
                                                               ref int max,
                                                               int i1)
        {
            try {
                var chh = sim.ModularHouseholds.CreateNewItem(sim.ConnectionString);
                var nameoffset = 1;
                //name finding

                while (sim.ModularHouseholds.IsNameTaken(template.NewHHName + " " + (i1 + nameoffset).ToString("D2", CultureInfo.CurrentCulture))) {
                    nameoffset++;
                }

                if (i1 == 0) {
                    min = nameoffset;
                }

                if (i1 == template.Count - 1) {
                    max = nameoffset + i1;
                }

                chh.Name = template.NewHHName + " " + (i1 + nameoffset).ToString("D2", CultureInfo.CurrentCulture);

                //other settings
                chh.Source = "Generated by " + template.Name + " at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                chh.CreationType = CreationType.TemplateCreated;
                chh.EnergyIntensityType = template.EnergyIntensityType;
                chh.GeneratorID = template.IntID;
                //add persons

                //TODO: fix this
                foreach (var person in template.Persons) {
                    var livingpattern = person.LivingPatternTag;
                    string[] components = livingpattern?.Name.Split('/');
                    if (components?.Length == 3) {
                        string basename = livingpattern.Name.Substring(0, livingpattern.Name.Length - components[2].Length);
                        List<LivingPatternTag> possibleTags = sim.LivingPatternTags.Items
                            .Where(x => x.Name.StartsWith(basename, StringComparison.InvariantCultureIgnoreCase)).ToList();
                        if (possibleTags.Count > 1) {
                            livingpattern = possibleTags[r.Next(possibleTags.Count)];
                        }
                    }

                    chh.AddPerson(person.Person, livingpattern);
                }

                var vac = GenerateVacation(template, r, sim, chh.Name);
                chh.Vacation = vac;
                chh.SaveToDB();
                return chh;
            }
            catch (Exception ex) {
                Logger.Exception(ex);
                throw;
            }
        }

        private static void MakeSettlement([NotNull] Simulator sim,
                                           bool generateSettlement,
                                           [NotNull] HouseholdTemplate template,
                                           int min,
                                           int max,
                                           [ItemNotNull] [NotNull] List<ModularHousehold> createdHH)
        {
            if (!generateSettlement) {
                return;
            }

            Logger.Get().SafeExecuteWithWait(() => {
                var newSettlement = sim.Settlements.CreateNewItem(sim.ConnectionString);
                newSettlement.Name = "Settlement for testing the households  " + template.NewHHName + " from " + min + " to " + max;
                newSettlement.SaveToDB();
                foreach (var household in createdHH) {
                    newSettlement.AddHousehold(household, 1);
                }

                Logger.Info("Created " + newSettlement.Name);
            });
        }

        private static void PickATrait([NotNull] Random r, [NotNull] ModularHousehold chh,
                                       [ItemNotNull] [NotNull] List<HouseholdTrait> potentialTraits, ref int traitexists,
                                       ref int classificationexists, ref int invalidForPerson, [NotNull] Person p,
                                       [CanBeNull] out HouseholdTrait trait, [ItemNotNull] [NotNull] List<STTraitLimit> limits, ref int violatesLimit,
                                       TraitTag entryTraitTag)
        {
            var trycount = 0;
            trait = null;
            if (potentialTraits.Count == 0)
            {
                Logger.Warning("Not a single trait was found for the trait tag " + entryTraitTag.Name);
                return;
            }
            while (trycount < 100 && trait == null) {
                var traitnumber = r.Next(potentialTraits.Count);
                var success = true;
                var traitsofThisPeron = chh.Traits.Where(t => t.DstPerson == p).Select(x => x.HouseholdTrait).ToList();
                var classifications = traitsofThisPeron.Select(x => x.Classification).ToList();
                var traitcount = chh.Traits.Count(x => x.HouseholdTrait == potentialTraits[traitnumber]);
                var maximumNumber = potentialTraits[traitnumber].MaximumNumberInCHH;
                if (traitcount >= maximumNumber) {
                    success = false;
                    traitexists++;
                }

                if (traitsofThisPeron.Contains(potentialTraits[traitnumber])) {
                    success = false;
                    traitexists++;
                }
                else if (classifications.Contains(potentialTraits[traitnumber].Classification)) {
                    success = false;
                    classificationexists++;
                }
                else if (!potentialTraits[traitnumber].IsValidForPerson(p)) {
                    success = false;
                    invalidForPerson++;
                }
                else if (!limits.All(x => x.IsPermitted(potentialTraits[traitnumber]))) {
                    success = false;
                    violatesLimit++;
                }

                if (success) {
                    trait = potentialTraits[traitnumber];
                }

                trycount++;
            }
        }

        private static void VacationSetDatesToTargetEntries([NotNull] Random rnd,
                                                            [ItemNotNull] [NotNull] List<VacationEntry> targetVacationEntries,
                                                            [NotNull] DateBasedProfile dbp)
        {
            //normalize to first entry for each day
            var year = DateTime.Now.Year;
            var probabilityarr = dbp.GetValueArray(new DateTime(year, 1, 1), new DateTime(year + 1, 1, 1), new TimeSpan(1, 0, 0, 0));
            foreach (var targetVacationEntry in targetVacationEntries) {
                var ranges = VacationGetProbabilityRanges(probabilityarr, year);
                //first find the proper ranges
                var possibleRanges = ranges.Where(x =>
                    x.DurationInDays >= targetVacationEntry.TargetDuration &&
                    Math.Abs(x.Probability - targetVacationEntry.TargetProbability) < Constants.Ebsilon).ToList();
                if (possibleRanges.Count == 0) {
                    var probabilities = targetVacationEntries.Select(x => x.TargetProbability).Distinct().OrderByDescending(x => x).ToList();
                    var pbtoTry = 0;
                    while (possibleRanges.Count == 0 && pbtoTry < probabilities.Count) {
                        //if nothing is available, use all probability ranges
                        possibleRanges = ranges.Where(x =>
                            x.DurationInDays >= targetVacationEntry.TargetDuration &&
                            Math.Abs(x.Probability - probabilities[pbtoTry]) < Constants.Ebsilon).ToList();
                        pbtoTry++;
                    }
                }

                if (possibleRanges.Count == 0) {
                    throw new LPGException("Could not find any suitable probability range to allocate the vacation with a duration of " +
                                           targetVacationEntry.TargetDuration);
                }

                //pick a range
                var rangeidx = rnd.Next(possibleRanges.Count);
                var vpr = possibleRanges[rangeidx];
                var vacationStartOffsetRange = (int)(vpr.DurationInDays - targetVacationEntry.TargetDuration);
                if (vacationStartOffsetRange < 0) {
                    throw new LPGException("Negative duration? Bug! Please report.");
                }

                var vacationStartOffset = rnd.Next(vacationStartOffsetRange);
                targetVacationEntry.StartDate = vpr.Start.AddDays(vacationStartOffset);
                targetVacationEntry.EndDate = targetVacationEntry.StartDate.AddDays(targetVacationEntry.TargetDuration);
                var startidx = targetVacationEntry.StartDate.DayOfYear;
                var endidx = targetVacationEntry.EndDate.DayOfYear;
                for (var i = startidx; i < endidx; i++) {
                    probabilityarr[i] = 0;
                }
            }
        }

        private class VacationEntry {
            //public double Duration => (EndDate - StartDate).TotalDays;
            public DateTime EndDate { get; set; }

            public DateTime StartDate { get; set; }
            public int TargetDuration { get; set; }
            public double TargetProbability { get; set; }
        }

        internal class VacationProbabilityRange {
            public double DurationInDays => (End - Start).TotalDays;
            public DateTime End { get; set; }
            public double Probability { get; set; }
            public DateTime Start { get; set; }
        }
    }
}