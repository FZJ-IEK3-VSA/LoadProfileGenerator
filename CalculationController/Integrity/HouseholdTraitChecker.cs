using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class HouseholdTraitChecker : BasicChecker {
        public HouseholdTraitChecker(bool performCleanupChecks) : base("Household Traits", performCleanupChecks)
        {
        }

        private void CheckAffordanceUse([NotNull] Simulator sim, [NotNull] HouseholdTrait trait)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            var validaffordances = trait.CollectAffordances(true);
            var allAffordances = trait.CollectAffordances(false);
            if (validaffordances.Count != allAffordances.Count) {
                var uses = trait.CalculateUsedIns(sim);
                if (uses.Count == 0) {
                    throw new DataIntegrityException(
                        "The trait " + trait.PrettyName +
                        " has affordances that are never executed, but the trait is not used. Please delete!", trait);
                }
                {
                    foreach (var affordance in allAffordances) {
                        if (!validaffordances.Contains(affordance)) {
                            throw new DataIntegrityException(
                                "The trait " + trait.PrettyName +
                                " has affordances that are never executed, but the trait is in use. At least one of the affordances is " +
                                affordance.Name + ". Please fix!", trait);
                        }
                    }
                }
            }
            foreach (var location in trait.Locations) {
                if (location.AffordanceLocations.Count == 0) {
                    if (trait.Autodevs.All(x => x.Location != location.Location)) {
                        throw new DataIntegrityException(
                            "The Location " + location.Location.Name + " in the household trait " + trait.Name +
                            " is empty. Please fix.", trait);
                    }
                }

                for (var index = 0; index < location.AffordanceLocations.Count; index++) {
                    var affordance = location.AffordanceLocations[index];
                    if (!validaffordances.Contains(affordance.Affordance)) {
                        var s = "The affordance " + affordance.Affordance?.Name +
                                " is not executable by a single Person in the household trait " + trait.Name +
                                ". Delete this affordance?";
                        Logger.Error(s);

                        var res =
                            MessageWindowHandler.Mw.ShowYesNoMessage(
                                "The affordance " + affordance.Affordance?.Name +
                                " is not executable by a single Person in the household trait " + trait.Name +
                                ". Delete this affordance?", "Delete?");
                        if (res == LPGMsgBoxResult.Yes) {
                            trait.DeleteAffordanceFromDB(affordance);
                            index = -1;
                        }
                    }
                }
            }
        }

        private void CheckAutoDevTimeProfileForZero1([NotNull] Simulator sim, [NotNull] HouseholdTrait householdTrait)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            foreach (var autodev in householdTrait.Autodevs) {
                var tps = GetTimeProfile(autodev, sim.DeviceActions.Items);
                var profilesWithZero = new List<TimeBasedProfile>();
                foreach (var profile in tps) {
                    var lasttdp = profile.ObservableDatapoints[profile.ObservableDatapoints.Count - 1];
                    var secondlasttdp = profile.ObservableDatapoints[profile.ObservableDatapoints.Count - 2];
                    if (Math.Abs(lasttdp.Value) < 0.000001 && Math.Abs(secondlasttdp.Value) > 0.0000001) {
                        profilesWithZero.Add(profile);
                    }
                }
                if (profilesWithZero.Count > 0) {
                    var uniqueProfilesWithZero = profilesWithZero.Distinct();
                    var elements = new List<BasicElement>();
                    elements.AddRange(uniqueProfilesWithZero);
                    throw new DataIntegrityException(
                        "The opened time profiles for the device " + autodev.Device?.Name +
                        " has zero as last value. This will lead to odd drops in the autonomous device consumption. Please fix.",
                        elements);
                }
            }
        }

        private static void CheckAutonomousDevices([NotNull] HouseholdTrait trait)
        {
            var usedLocations = new List<Location>();
            usedLocations.AddRange(trait.Locations.Select(x => x.Location));
            foreach (var autodev in trait.Autodevs) {
                if (!usedLocations.Contains(autodev.Location)) {
                    throw new DataIntegrityException(
                        "The household trait " + trait.Name + " has the autonomous device " + autodev.Device?.Name +
                        " at the Location " + autodev.Location + ", which doesn't exist in the household. Please fix.",
                        trait);
                }
            }
        }

        private void CheckClassifications([NotNull][ItemNotNull] ObservableCollection<HouseholdTrait> traits)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            var classifications = traits.Select(x => x.Classification).Distinct().ToList();
            foreach (var classi in classifications) {
                var traitsForC = traits.Where(x => x.Classification == classi).ToList();
                var tags = new List<TraitTag>();
                foreach (var trait in traitsForC) {
                    tags.AddRange(
                        trait.Tags.Where(x => x.Name.StartsWith("Web /", StringComparison.Ordinal))
                            .Select(y => y.Tag)
                            .ToList());
                }
                tags = tags.Distinct().ToList();
                if (tags.Count > 1) {
                    var s = string.Empty;
                    foreach (var traitTag in tags) {
                        s += traitTag.PrettyName + Environment.NewLine;
                    }
                    var bes = new List<BasicElement>();
                    var traitNames = "";
                    foreach (var trait in traitsForC) {
                        bes.Add(trait);
                        traitNames += trait.Name + Environment.NewLine;
                    }
                    throw new DataIntegrityException(
                        "The following traits share the same classification, but have different trait tags:"+ Environment.NewLine +
                        s + Environment.NewLine + "Traits:" + Environment.NewLine + traitNames,
                        bes);
                }
            }
        }

        private static void CheckDesires([NotNull] HouseholdTrait trait)
        {
            foreach (var desire in trait.Desires) {
                if (desire.Threshold > 100) {
                    throw new DataIntegrityException(
                        "The trait " + trait.Name +
                        " has a desire threshold over 100%. 100% is the maximum. Please fix.", trait);
                }
                foreach (var other in trait.Desires) {
                    if (other != desire) {
                        if (desire.Desire == other.Desire && desire.SicknessDesire == other.SicknessDesire &&
                            desire.Gender == other.Gender) {
                            throw new DataIntegrityException(
                                "The desire " + desire.Desire.PrettyName + " seems to be twice in the trait " +
                                trait.Name + ". Please fix.", trait);
                        }
                    }
                }
            }
        }

        private void CheckFoodTimeLimit([NotNull] HouseholdTrait trait)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            foreach (var traitloc in trait.Locations) {
                if (trait.Name.ToLowerInvariant().Contains("lunch")) {
                    foreach (var affordanceLocation in traitloc.AffordanceLocations) {
                        var tl = affordanceLocation.TimeLimit;
                        if (tl == null) {
                            throw new DataIntegrityException(
                                "The trait " + trait.Name + " has no time limit set for the affordance " +
                                affordanceLocation.Affordance?.PrettyName, trait);
                        }
                        if (!tl.Name.ToLowerInvariant().Contains("lunch")) {
                            throw new DataIntegrityException(
                                "The trait " + trait.Name + " has a time limit set for the affordance " +
                                affordanceLocation.Affordance?.PrettyName + " set that is not lunch", trait);
                        }
                    }
                }

                if (trait.Name.ToLowerInvariant().Contains("dinner")) {
                    foreach (var affordanceLocation in traitloc.AffordanceLocations) {
                        var tl = affordanceLocation.TimeLimit;
                        if (tl == null) {
                            throw new DataIntegrityException(
                                "The trait " + trait.Name + " has no time limit set for the affordance " +
                                affordanceLocation.Affordance?.PrettyName, trait);
                        }
                        if (!tl.Name.ToLowerInvariant().Contains("dinner")) {
                            throw new DataIntegrityException(
                                "The trait " + trait.Name + " has a time limit set for the affordance " +
                                affordanceLocation.Affordance?.PrettyName + " set that is not dinner", trait);
                        }
                    }
                }
            }
        }

        private void CheckForAffordancesWithoutTraits([NotNull][ItemNotNull] ObservableCollection<HouseholdTrait> traits,
            [NotNull][ItemNotNull] ObservableCollection<Affordance> affordances)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            var allUsedAffordances = new HashSet<Affordance>();
            foreach (var trait in traits) {
                var affs = trait.CollectAffordances(true);
                foreach (var affordance in affs) {
                    if (!allUsedAffordances.Contains(affordance)) {
                        allUsedAffordances.Add(affordance);
                    }
                }
            }
            var count = 0;

            foreach (var affordance in affordances) {
                if (!allUsedAffordances.Contains(affordance)) {
                    count++;
                    Logger
                        .Error("Found affordance not used in any trait " + count + ": " + affordance.Name);
                    throw new DataIntegrityException("Found affordance not used in any trait : " + affordance.Name,
                        affordance);
                }
            }
        }

        private void CheckPersonCounts([NotNull] HouseholdTrait householdTrait)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            if (householdTrait.MinimumPersonsInCHH < 1 || householdTrait.MaximumPersonsInCHH > 20 ||
                householdTrait.MaximumPersonsInCHH < 1) {
                householdTrait.MinimumPersonsInCHH = 1;
                householdTrait.MaximumPersonsInCHH = 10;
                throw new DataIntegrityException(
                    "The opened traits have either a minimum Person count of less than 1," +
                    " a maximum Person count less than 1 or more than 20. Or the number of times it can occur is more than 20 or less than 1.",
                    householdTrait);
            }
            if (householdTrait.MaximumNumberInCHH < 1 || householdTrait.MaximumNumberInCHH > 20) {
                householdTrait.MaximumNumberInCHH = 20;

                throw new DataIntegrityException(
                    "The opened traits have either a minimum Person count of less than 1," +
                    " a maximum Person count less than 1 or more than 20. Or the number of times it can occur is more than 20 or less than 1.",
                    householdTrait);
            }
        }

        private void CheckSleepDesire([NotNull] HouseholdTrait trait)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            if (trait.Name.ToUpperInvariant().Contains("SLEEP")) {
                switch (trait.Desires.Count) {
                    case 1:
                        if (trait.Desires[0].SicknessDesire != HealthStatus.HealthyOrSick) {
                            throw new DataIntegrityException(
                                "Desire in the sleep trait " + trait.PrettyName +
                                " is not for both health and sickness.", trait);
                        }
                        break;
                    case 2:
                        // TODO: two desires... how to check they both apply to the same bed? The bed is only in the name. String matching sucks.
                        // leave it for now.
                        break;
                    default:
                        throw new DataIntegrityException("More than one desire in the sleep trait " + trait.PrettyName,
                            trait);
                }
            }
        }

        private void CheckTraitLocations([NotNull] HouseholdTrait trait)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            foreach (var hhtloc in trait.Locations) {
                if (hhtloc.Location.LocationDevices.Count > 0) {
                    foreach (var hhtAffordance in hhtloc.AffordanceLocations) {
                        if (hhtAffordance.Affordance == null) {
                            throw new DataIntegrityException("Affordance was null");
                        }
                        if (!hhtAffordance.Affordance.NeedsLight &&
                            !hhtAffordance.Name.ToUpperInvariant().Contains("SLEEP") &&
                            !hhtAffordance.Name.ToUpperInvariant().Contains("NAP")) {
                            throw new DataIntegrityException(
                                "The affordance " + hhtAffordance.Affordance.PrettyName + " is in the trait " +
                                trait.Name + " at the Location " + hhtloc.Location.PrettyName +
                                " which has light devices, but doesn't have  'needs light' enabled. Please fix.",
                                hhtAffordance.Affordance);
                        }
                    }
                }
                else {
                    foreach (var affordance in hhtloc.AffordanceLocations) {
                        if (affordance.Affordance == null)
                        {
                            throw new DataIntegrityException("Affordance was null");
                        }
                        if (affordance.Affordance.NeedsLight) {
                            throw new DataIntegrityException(
                                "The affordance " + affordance.Affordance.PrettyName + " in the trait " + trait.Name +
                                " at the Location " + hhtloc.Location.PrettyName +
                                " has 'needs light' enabled, but the Location has no light. Please fix.",
                                affordance.Affordance);
                        }
                    }
                }
            }
        }

        /*private void CheckUsedIns(HouseholdTrait householdTrait, Simulator sim, ref int notUsedCount)
        {
            var used = householdTrait.GetUsesForTrait(sim.ModularHouseholds.It);
            if (used.Count == 0) {
                notUsedCount++;
                Logger.Warning("Trait " + notUsedCount + " not used:" + householdTrait.PrettyName);
            }
        }*/

        private void CleanTraitNames([NotNull][ItemNotNull] ObservableCollection<HouseholdTrait> traits)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            var traitscopy = traits.ToList();
            var traitstofix = new List<BasicElement>();
            for (var i = 0; i < traitscopy.Count; i++) {
                var householdTrait = traitscopy[i];

                if (Math.Abs(householdTrait.EstimatedTimeCount - -1) < Constants.Ebsilon ||
                    Math.Abs(householdTrait.EstimatedTimes - -1) < Constants.Ebsilon) {
                    householdTrait.CalculateEstimatedTimes();
                    householdTrait.SaveToDB();
                }
                if (householdTrait.Name.Contains("(")) {
                    var s = householdTrait.Name;
                    var begin = s.IndexOf("(", StringComparison.Ordinal);
                    var end = s.IndexOf(")", StringComparison.Ordinal);
                    if (begin > 0 && end > 0) {
                        var brackets = s.Substring(begin, end - begin + 1);
                        if(Config.IsInUnitTesting) {
                            throw new LPGException("found brackets in the name of a trait: " + householdTrait.Name);
                        }
                        var mbr =
                            MessageWindowHandler.Mw.ShowYesNoMessage(
                                "Remove from the household trait name " + householdTrait.Name + " the substring \"" +
                                brackets + "\"", "Delete?");
                        if (mbr == LPGMsgBoxResult.Yes) {
                            Logger.Get()
                                .SafeExecuteWithWait(
                                    () =>
                                        householdTrait.Name =
                                            householdTrait.Name.Replace(brackets, string.Empty).Trim());
                            householdTrait.SaveToDB();
                        }
                        else {
                            traitstofix.Add(householdTrait);
                        }
                    }
                }
            }
            if (traitstofix.Count > 0) {
                throw new DataIntegrityException("The opened household traits have ( or ) in their names. Please fix.",
                    traitstofix);
            }
        }

        private void GeneralCleanupChecks([NotNull] HouseholdTrait trait)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            var percentage = trait.EstimatedTimePerYearInH / 8760;
            if (percentage > 0.03 && string.IsNullOrWhiteSpace(trait.ShortDescription)) {
                throw new DataIntegrityException("The trait " + trait.PrettyName + " is missing a short description.",
                    trait);
            }
            if (trait.Name.ToUpperInvariant().Contains("(COPY)")) {
                throw new DataIntegrityException(
                    "The trait " + trait.PrettyName + " contains (copy) in the name. This is untidy. Please fix.",
                    trait);
            }

            if (string.IsNullOrEmpty(trait.Classification) || trait.Classification.ToUpperInvariant() == "UNKNOWN") {
                throw new DataIntegrityException(
                    "The household trait " + trait.Name + " is missing a classification. Please fix.", trait);
            }
            if (trait.Tags.Count == 0) {
                throw new DataIntegrityException(
                    "The household trait " + trait.Name + " is missing at least one tag. Please fix.", trait);
            }
            // check completeness
            var affordancecount = trait.Locations.Select(x => x.AffordanceLocations.Count).Sum();
            if (affordancecount == 0 && trait.Autodevs.Count == 0 && trait.Desires.Count == 0 &&
                trait.SubTraits.Count == 0) {
                throw new DataIntegrityException(
                    "The household trait " + trait.Name + " seems to be empty. Please fix or delete!", trait);
            }
            if (affordancecount != 0 && trait.Desires.Count == 0) {
                throw new DataIntegrityException(
                    "The household trait " + trait.Name + " has affordances, but no desires. Please fix.", trait);
            }
            if (affordancecount == 0 && trait.Desires.Count != 0 &&
                !trait.Name.ToUpperInvariant().Contains("DESIRE FOR FOOD") && !trait.Name.ToUpperInvariant().Contains("JOIN ONLY")) {
                throw new DataIntegrityException(
                    "The household trait " + trait.Name + " has desires, but no affordances. Please fix.", trait);
            }
        }

        [NotNull]
        [ItemNotNull]
        private static List<TimeBasedProfile> GetTimeProfile([NotNull] HHTAutonomousDevice autodev,
            [NotNull][ItemNotNull] ObservableCollection<DeviceAction> actions)
        {
            if(autodev.Device==null) {
                throw new DataIntegrityException("Device was null");
            }
            var tps = new List<TimeBasedProfile>();
            switch (autodev.Device.AssignableDeviceType) {
                case AssignableDeviceType.Device:
                case AssignableDeviceType.DeviceCategory:
                    tps.Add(autodev.TimeProfile);
                    return tps;
                case AssignableDeviceType.DeviceAction:
                    var da = (DeviceAction) autodev.Device;
                    foreach (var deviceActionProfile in da.Profiles) {
                        tps.Add(deviceActionProfile.Timeprofile);
                    }
                    return tps;
                case AssignableDeviceType.DeviceActionGroup:
                    var dag = (DeviceActionGroup) autodev.Device;
                    var mylist = dag.GetDeviceActions(actions);
                    foreach (var action in mylist) {
                        foreach (var deviceActionProfile in action.Profiles) {
                            tps.Add(deviceActionProfile.Timeprofile);
                        }
                    }
                    return tps;
                default:
                    throw new LPGException("Unknown Assignable Device Type");
            }
        }

        //public void CheckWorkFromHome(HouseholdTrait trait, TraitTag wfhTag)
        //{
        //    if (PerformCleanupChecks) {
        //        bool foundWfh = false;
        //        foreach (var tag in trait.LivingPatternTags) {
        //            if (!tag.Name.Contains("Living Pattern / Stay Home")) {
        //                foreach (var tag1 in trait.Tags) {
        //                    if (tag1.Name.Contains("Living Pattern / Work From Home")) {
        //                        foundWfh = true;
        //                    }
        //                }
        //            }

        //        }
        //        /*
        //        if (!foundWfh) {
        //            var result = MessageWindowHandler.Mw.ShowYesNoMessage("Add " + wfhTag.Name + " to " + trait.Name + "?","Yes/No");
        //            if (result == LPGMsgBoxResult.Yes) {
        //                trait.AddTag(wfhTag);
        //                trait.SaveToDB();
        //            }
        //        }*/
        //    }
        //}

        protected override void Run(Simulator sim, CheckingOptions options)
        {
            CheckForAffordancesWithoutTraits(sim.HouseholdTraits.Items, sim.Affordances.Items);
            CheckClassifications(sim.HouseholdTraits.Items);
            CleanTraitNames(sim.HouseholdTraits.Items);
            //var wfhPattern = sim.TraitTags.FindFirstByNameNotNull("Living Pattern / Work From Home");
            //var notusedCount = 0;
            foreach (var householdTrait in sim.HouseholdTraits.Items) {
               // CheckWorkFromHome(householdTrait, wfhPattern);
                CheckAutoDevTimeProfileForZero1(sim, householdTrait);
                CheckSleepDesire(householdTrait);
                CheckPersonCounts(householdTrait);
                CheckTraitLocations(householdTrait);
                GeneralCleanupChecks(householdTrait);
                CheckDesires(householdTrait);
                CheckAutonomousDevices(householdTrait);
                CheckAffordanceUse(sim, householdTrait);
                CheckFoodTimeLimit(householdTrait);
                //if(PerformCleanupChecks)
                  //  CheckUsedIns(householdTrait, sim, ref notusedCount);
            }
            List<HouseholdTrait> traitsWithMissingTags = new List<HouseholdTrait>();
            const string officeTag = "Living Pattern / Office";
            const string wfhtag = "Living Pattern / Work From Home";
            const string worktag = "Work / Work";
            foreach (var item in sim.HouseholdTraits.Items) {
                if (item.Tags.Any(x => x.Tag.Name == worktag)) {
                    continue;
                }
                if (item.LivingPatternTags.Any(x => x.Tag.Name.StartsWith(officeTag))) {
                    if (!item.LivingPatternTags.Any(x => x.Tag.Name.Contains(wfhtag))) {
                        traitsWithMissingTags.Add(item);
                    }
                }
            }

            if (traitsWithMissingTags.Count > 0) {
                throw new DataIntegrityException("The following traits have office tags but not work from home tags:",
                    traitsWithMissingTags.Take(10).Select(x=>(BasicElement) x).ToList());
            }
        }


    }
}