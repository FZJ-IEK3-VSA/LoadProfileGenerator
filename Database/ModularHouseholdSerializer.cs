using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Helpers;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database {
    public static class ModularHouseholdSerializer {
        [ItemNotNull]
        [NotNull]
        private static List<Settlement> Create([ItemNotNull] [NotNull] List<SimpleModularHousehold> simpleHHs, [NotNull] Simulator sim,
            [NotNull] GlobalOptions globalOptions) {
            var finishedHHs = new List<ModularHousehold>();
            var finishedHouses = new List<House>();

            foreach (var smhh in simpleHHs) {
                var name = globalOptions.HouseholdName + " - " + smhh.Name;
                var oldMhh = sim.ModularHouseholds.It.FirstOrDefault(x => x.Name == name);
                if (oldMhh != null) {
                    sim.ModularHouseholds.DeleteItem(oldMhh);
                    Logger.Warning("Deleted the previously created household with the name " + name);
                }
                var mhh = sim.ModularHouseholds.CreateNewItem(sim.ConnectionString);
                finishedHHs.Add(mhh);
                mhh.EnergyIntensityType = globalOptions.EnergyIntensityType;

                mhh.Name = name;
                Logger.Info("Created a household named: " + mhh.Name);

                mhh.Description = globalOptions.Description?? "";
                mhh.DeviceSelection = sim.DeviceSelections.It.First(x => x.PrettyName == globalOptions.DeviceSelection);
                mhh.Vacation = sim.Vacations.It.FirstOrDefault(x => x.PrettyName == smhh.Vacation);
                mhh.SaveToDB();
                foreach (var tagName in globalOptions.Tags) {
                    var tag = sim.HouseholdTags.It.FirstOrDefault(x => x.PrettyName == tagName);
                    if (tag == null) {
                        throw new LPGException("Could not recognize Tag: " + tagName);
                    }
                    mhh.AddHouseholdTag(tag);
                }
                if (mhh.Vacation == null) {
                    throw new LPGException("Could not identify vacation with the name: " + smhh.Vacation);
                }
                var personCount = 0;
                foreach (SimplePerson sp in smhh.Persons.Values) {
                    if (sp.Traits.Count > 0) {
                        var personName = sp.Name + " - " + smhh.Name;
                        var oldPerson = sim.Persons.It.FirstOrDefault(x => x.Name == personName);
                        if (oldPerson != null) {
                            sim.Persons.DeleteItem(oldPerson);
                            Logger.Warning("Deleted the previously created person with the name " + personName);
                        }

                        var p = sim.Persons.CreateNewItem(sim.ConnectionString);
                        p.Name = personName;
                        p.Age = sp.Age;
                        p.AverageSicknessDuration = sp.SicknessDuration;
                        p.Gender = sp.Gender?.ToLower(CultureInfo.CurrentCulture) == "male"
                            ? PermittedGender.Male
                            : PermittedGender.Female;
                        p.SickDays = sp.AverageSickDays;
                        personCount++;
                        TraitTag tag = sim.TraitTags.FindFirstByName(sp.TraitTag);
                        if(tag == null) {
                            throw new LPGException("Tag not found");
                        }
                        mhh.AddPerson(p,tag);

                        foreach (var traitName in sp.Traits) {
                            var trait = sim.HouseholdTraits.It.FirstOrDefault(x => x.PrettyName == traitName);
                            if (trait == null) {
                                trait = sim.HouseholdTraits.It.FirstOrDefault(x => x.PrettyNameOld == traitName);
                            }
                            if (trait == null) {
                                throw new LPGException("Could not find a trait with the name: " + traitName);
                            }
                            mhh.AddTrait(trait, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name, p);
                        }
                        Logger.Info("Added " + sp.Traits.Count + " traits to the person " + p.PrettyName);
                        p.SaveToDB();
                    }
                    else {
                        Logger.Info("Person " + sp.Name + " has no traits for " + smhh.Name);
                    }
                }
                Logger.Info("Imported " + personCount + " persons.");
                var house = MakeHouse(sim, globalOptions, smhh, mhh);
                finishedHouses.Add(house);
            }
            var resultingSettlements = new List<Settlement>();
            Logger.Info("Finished creating households.");
            // now make the settlement for the modular household testing
            Logger.Info("Creating a settlement");
            var settlementName = "Test settlement for imported " + globalOptions.HouseholdName;
            var oldSett = sim.Settlements.It.FirstOrDefault(x => x.Name == settlementName);
            if (oldSett != null) {
                sim.Settlements.DeleteItem(oldSett);
                Logger.Warning("Deleted old settlement " + settlementName);
            }
            var sett = sim.Settlements.CreateNewItem(sim.ConnectionString);
            foreach (var modularHousehold in finishedHHs) {
                sett.AddHousehold(modularHousehold, 1);
            }
            sett.Name = settlementName;
            sett.SaveToDB();
            resultingSettlements.Add(sett);
            // make the settlement for the house testing
            Logger.Info("Creating a settlement for the houses");
            var houseSettlementName = "Test settlement for imported houses for " + globalOptions.HouseholdName;
            var oldHouseSett = sim.Settlements.It.FirstOrDefault(x => x.Name == houseSettlementName);
            if (oldHouseSett != null) {
                sim.Settlements.DeleteItem(oldHouseSett);
                Logger.Warning("Deleted old house settlement " + houseSettlementName);
            }
            var houseSett = sim.Settlements.CreateNewItem(sim.ConnectionString);
            foreach (var house in finishedHouses) {
                houseSett.AddHousehold(house, 1);
            }
            houseSett.Name = houseSettlementName;
            houseSett.SaveToDB();
            resultingSettlements.Add(houseSett);

            return resultingSettlements;
        }

        public static void ExportAsCSV([NotNull] ModularHousehold mhh, [NotNull] Simulator sim, [NotNull] string fullFileName) {
            var csv = sim.MyGeneralConfig.CSVCharacter;
            var sb = new StringBuilder();
            sb.AppendLine("// This file was exported from LoadProfileGenerator.de");
            sb.AppendLine(
                "// It contains a household definition and can be used to easily create modified households based on the primary definition");
            sb.AppendLine(
                "// One example use could be modeling behavior changes as the occupants in an household age.");
            sb.AppendLine("// Syntax: Blank lines and all lines starting with // are ignored.");
            sb.AppendLine(
                "// a 1 means the trait applies in that case, every other value or blank means the trait doesn't apply.");
            sb.AppendLine(
                "// You can add as many cases as you want. The households in the LPG will then be labeled appropriately with the main household name and the case.");
            sb.AppendLine(
                "// Note that all names are case sensitive and the file format needs to be exactly the way it is since the parser is a bit fragile.");
            sb.AppendLine(
                "// If a person should not be added for a certain case, just remove all traits for that person in the appropriate column.");
            sb.AppendLine(
                "// Importing the same file twice will delete the previously created households, so be careful.");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Global Parameters");

            sb.Append("Household Name").Append(csv).AppendLine(mhh.PrettyName);
            sb.Append("Description").Append(csv).AppendLine(mhh.Description);
            sb.Append("DeviceSelection").Append(csv).AppendLine(mhh.DeviceSelection?.PrettyName);
            sb.Append("EnergyIntensity").Append(csv).Append(mhh.EnergyIntensityType).AppendLine();
            sb.Append("Geographic Location").Append(csv).AppendLine(sim.GeographicLocations.It[0].Name);
            sb.Append("Temperature Profile").Append(csv).AppendLine(sim.TemperatureProfiles.It[0].Name);

            var s = "";
            foreach (var tag in mhh.ModularHouseholdTags) {
                s += tag + csv;
            }
            sb.Append("Tags;").AppendLine(s);
            sb.AppendLine();
            sb.Append("Cases").Append(csv).Append("Case 1").Append(csv).Append("Case 2").Append(csv).Append("Case 3").Append(csv).Append("Case 4").Append(csv).AppendLine("Case 5");
            if (mhh.Vacation == null) {
                throw new LPGException("Vacation was null");
            }
            sb.Append("Vacation").Append(csv).AppendLine(MultiplyStrings(mhh.Vacation.PrettyName, csv));
            sb.Append("Housetype").Append(csv).AppendLine(MultiplyStrings(sim.HouseTypes[0].Name, csv));
            sb.AppendLine();
            var usedTraits = new HashSet<string>();
            foreach (var mhhPerson in mhh.Persons) {
                var p = mhhPerson.Person;
                sb.AppendLine();
                sb.Append("######").Append(csv).AppendLine(p.Name);
                sb.Append("Age").Append(csv).AppendLine(MakeCSVList(csv, p.Age, p.Age + 5, p.Age + 10, p.Age + 15, p.Age + 20));
                sb.Append("AverageSickdays").Append(csv).AppendLine(MakeCSVList(csv, 3, 5, 8, 10, 15));
                sb.Append("SicknessDuration").Append(csv).AppendLine(MakeCSVList(csv, 1, 1, 2, 5, 5));
                sb.Append("Gender").Append(csv).AppendLine(MultiplyStrings(p.Gender.ToString(), csv));
                string livingpattern = mhhPerson.TraitTag?.Name;
                if (livingpattern == null) {
                    livingpattern = "";
                }
                sb.Append("Living Pattern").Append(csv).AppendLine(MultiplyStrings(livingpattern, csv));
                sb.AppendLine();
                sb.Append("###").Append(csv).AppendLine("Traits");
                foreach (var trait in mhh.Traits.Where(x => x.DstPerson == p)) {
                    if (!usedTraits.Contains(trait.HouseholdTrait.PrettyName)) {
                        usedTraits.Add(trait.HouseholdTrait.PrettyName);
                    }
                    sb.Append(trait.HouseholdTrait.PrettyName).Append(csv).AppendLine(MakeTraitPattern("1", csv));
                }
            }
            sb.AppendLine();
            sb.AppendLine();
            sb.Append("%%%%%%").Append(csv).AppendLine("Unused traits (listed here to make it easier to copy them to a person and fill in)");
            foreach (var trait in sim.HouseholdTraits.It) {
                if (!usedTraits.Contains(trait.PrettyName)) {
                    sb.AppendLine(trait.PrettyName);
                }
            }
            var fi = new FileInfo(fullFileName);
            Logger.Info("Exporting to " + fi.FullName);
            using (var sw = new StreamWriter(fullFileName)) {
                sw.WriteLine(sb);
                sw.Close();
            }
        }

        [ItemNotNull]
        [NotNull]
        public static List<Settlement> ImportFromCSV([CanBeNull] string fileName, [NotNull] Simulator sim) {
            if (fileName == null) {
                Logger.Error("No filename was set.");
                return new List<Settlement>();
            }
            Logger.Info("Starting to import from " + fileName);
            var lines = ReadEntireFile(fileName, sim);
            var go = ProcessGlobalOptions(lines, new[] {sim.MyGeneralConfig.CSVCharacter});
            string[] csvArr = {sim.MyGeneralConfig.CSVCharacter};
            var hhs = ReadHouseholds(lines, csvArr);

            while (lines.Count > 0 && lines[0] != "%%%%%%") {
                if (!lines[0].StartsWith("######", StringComparison.Ordinal)) {
                    throw new LPGException("Didn't find a person with ###### in this line: " + lines[0]);
                }
                string currentPerson;
                {
                    var personLine = lines[0];
                    lines.RemoveAt(0);
                    var personArr = personLine.Split(csvArr, StringSplitOptions.None);
                    currentPerson = personArr[1];
                    for (var i = 0; i < hhs.Count; i++) {
                        hhs[i].Persons.Add(currentPerson, new SimplePerson(currentPerson));
                    }
                }
                {
                    var ageLine = lines[0];
                    lines.RemoveAt(0);
                    var ageArr = ageLine.Split(csvArr, StringSplitOptions.None);
                    for (var i = 1; i < ageArr.Length && i < hhs.Count + 1; i++) {
                        if (ageArr[i].Length > 0) {
                            hhs[i - 1].Persons[currentPerson].Age = Convert.ToInt32(ageArr[i],
                                CultureInfo.CurrentCulture);
                        }
                    }
                }
                {
                    var sickDays = lines[0];
                    lines.RemoveAt(0);
                    var sickDaysArr = sickDays.Split(csvArr, StringSplitOptions.None);
                    for (var i = 1; i < sickDays.Length && i < hhs.Count + 1; i++) {
                        if (sickDaysArr[i].Length > 0) {
                            hhs[i - 1].Persons[currentPerson].AverageSickDays = Convert.ToInt32(sickDaysArr[i],
                                CultureInfo.CurrentCulture);
                        }
                    }
                }
                {
                    var sicknessDuration = lines[0];
                    lines.RemoveAt(0);
                    var sickDurationArr = sicknessDuration.Split(csvArr, StringSplitOptions.None);
                    for (var i = 1; i < sickDurationArr.Length && i < hhs.Count + 1; i++) {
                        if (sickDurationArr[i].Length > 0) {
                            hhs[i - 1].Persons[currentPerson].SicknessDuration = Convert.ToInt32(sickDurationArr[i],
                                CultureInfo.CurrentCulture);
                        }
                    }
                }
                {
                    var gender = lines[0];
                    lines.RemoveAt(0);
                    var genderArr = gender.Split(csvArr, StringSplitOptions.None);
                    for (var i = 1; i < genderArr.Length && i < hhs.Count + 1; i++) {
                        if (genderArr[i].Length > 0) {
                            hhs[i - 1].Persons[currentPerson].Gender = genderArr[i];
                        }
                    }
                }
                {
                    var livingpattern = lines[0];
                    lines.RemoveAt(0);
                    var livingPatternArr = livingpattern.Split(csvArr, StringSplitOptions.None);
                    for (var i = 1; i < livingPatternArr.Length && i < hhs.Count + 1; i++)
                    {
                        if (livingPatternArr[i].Length > 0)
                        {
                            hhs[i - 1].Persons[currentPerson].TraitTag = livingPatternArr[i];
                        }
                    }
                }

                var traitMarker = lines[0];
                lines.RemoveAt(0);
                if (traitMarker != "###;Traits") {
                    throw new LPGException("Traitmarker '###;Traits' is missing? Found: " + traitMarker);
                }
                while (lines.Count > 0 && !lines[0].StartsWith("######", StringComparison.Ordinal)) {
                    var traitLine = lines[0];
                    lines.RemoveAt(0);
                    var traitArr = traitLine.Split(csvArr, StringSplitOptions.None);
                    var traitName = traitArr[0];
                    for (var i = 1; i - 1 < hhs.Count && i < traitArr.Length; i++) {
                        if (traitArr[i] == "1") {
                            hhs[i - 1].Persons[currentPerson].Traits.Add(traitName);
                        }
                    }
                }
            }
            return Create(hhs, sim, go);
        }

        [NotNull]
        private static string MakeCSVList([NotNull] string csv, [NotNull] params int[] list) {
            var s = "";
            foreach (var i in list) {
                s += i + csv;
            }
            return s;
        }

        [NotNull]
        private static House MakeHouse([NotNull] Simulator sim, [NotNull] GlobalOptions globalOptions, [NotNull] SimpleModularHousehold smhh,
            [NotNull] ModularHousehold mhh) {
            var housename = "House for " + mhh.Name;
            var myHouse = sim.Houses.FindFirstByName(housename);
            if (myHouse != null) {
                sim.Houses.DeleteItem(myHouse);
            }
            var ht = sim.HouseTypes.FindFirstByName(smhh.HouseType, FindMode.IgnoreCase);
            if (ht == null) {
#pragma warning disable IDE0016 // Use 'throw' expression
                throw new LPGException("Housetype " + smhh.HouseType + " was not found in the database. Maybe a typo?");
#pragma warning restore IDE0016 // Use 'throw' expression
            }
            var house = sim.Houses.CreateNewItem(sim.ConnectionString);
            house.Name = housename;
            house.Description = "Created by CSV Import";
            house.CreationType = CreationType.TemplateCreated;
            house.EnergyIntensityType = globalOptions.EnergyIntensityType;
            house.HouseType = ht;
            var geoLoc = sim.GeographicLocations.FindFirstByName(globalOptions.GeoLocName, FindMode.IgnoreCase);
            house.GeographicLocation = geoLoc;

            var temperatureProfile =
                sim.TemperatureProfiles.FindFirstByName(globalOptions.TemperatureProfileName, FindMode.IgnoreCase);
            house.TemperatureProfile = temperatureProfile;
            house.SaveToDB();
            house.AddHousehold(mhh,null,null,null);
            house.SaveToDB();
            return house;
        }

        [NotNull]
        private static string MakeTraitPattern([NotNull] string s1, [NotNull] string csv) {
            var s = "";
            for (var i = 0; i < 5; i++) {
                s += s1 + csv;
            }

            return s;
        }

        [NotNull]
        private static string MultiplyStrings([NotNull] string s1, [NotNull] string csv, bool randomize = false, [CanBeNull] Random rnd = null) {
            if (!randomize) {
                var s = "";
                for (var i = 0; i < 5; i++) {
                    s += s1 + csv;
                }

                return s;
            }
            var sR = "";
            if (rnd == null) {
                throw new LPGException("Rnd was null.");
            }
            for (var i = 0; i < 5; i++) {
                if (rnd.NextDouble() > 0.5) {
                    sR += s1 + csv;
                }
                else {
                    sR += csv;
                }
            }

            return sR;
        }

        [NotNull]
        private static GlobalOptions ProcessGlobalOptions([ItemNotNull] [NotNull] List<string> lines, [ItemNotNull] [NotNull] string[] csvArr) {
            var globalOptions = new GlobalOptions();
            while (!lines[0].StartsWith("Cases", StringComparison.Ordinal) && lines.Count > 0) {
                var s = lines[0];
                var arr = s.Split(csvArr, StringSplitOptions.None);
                ProcessOptions(arr, globalOptions);
                lines.RemoveAt(0);
            }
            return globalOptions;
        }

        private static void ProcessOptions([ItemNotNull] [NotNull] string[] s, [NotNull] GlobalOptions go) {
            switch (s[0]) {
                case "Household Name":
                    go.HouseholdName = s[1];
                    break;
                case "Description":
                    go.Description = s[1];
                    break;
                case "DeviceSelection":
                    go.DeviceSelection = s[1];
                    break;
                case "EnergyIntensity":
                    go.EnergyIntensity = s[1];
                    break;
                case "Geographic Location":
                    go.GeoLocName = s[1];
                    break;
                case "Temperature Profile":
                    go.TemperatureProfileName = s[1];
                    break;
                case "Tags":
                    for (var i = 1; i < s.Length; i++) {
                        go.Tags.Add(s[i]);
                    }

                    break;
                default:
                    throw new LPGException("Unknown option:" + s[0]);
            }
        }

        [ItemNotNull]
        [NotNull]
        private static List<string> ReadEntireFile([NotNull] string filename, [NotNull] Simulator sim) {
            var lines = new List<string>();

            using (var fs = new FileStream(filename, FileMode.Open)) {
                using (var sr = new StreamReader(fs)) {
                    var csv = sim.MyGeneralConfig.CSVCharacter;
                    while (!sr.EndOfStream) {
                        var line = sr.ReadLine()?.Trim();
                        if (line == null) {
                            throw new LPGException("The file could not be read.");
                        }

                        var tmpLine = line.Replace(csv, "").Trim();
                        while (line.EndsWith(csv, StringComparison.Ordinal)) {
                            line = line.Substring(0, line.Length - 1);
                        }

                        if (line == csv) {
                            line = "";
                        }

                        if (line.Length > 0 && tmpLine.Length > 0 && !line.StartsWith("//", StringComparison.Ordinal)) {
                            lines.Add(line);
                        }
                    }

                    sr.Close();
                }
            }
            if (lines[0] != "Global Parameters") {
                throw new LPGException("The first line should be parameters.");
            }
            lines.RemoveAt(0);
            return lines;
        }

        [ItemNotNull]
        [NotNull]
        private static List<SimpleModularHousehold> ReadHouseholds([ItemNotNull] [NotNull] List<string> lines, [NotNull] [ItemNotNull] string[] csvArr) {
            if (lines.Count == 0) {
                throw new LPGException("Did not find 'cases'");
            }

            var hhs = new List<SimpleModularHousehold>();
            var casesLine = lines[0];
            lines.RemoveAt(0);
            var casesArr = casesLine.Split(csvArr, StringSplitOptions.None);
            for (var i = 1; i < casesArr.Length; i++) {
                hhs.Add(new SimpleModularHousehold(casesArr[i]));
            }
            {
                var vacationLine = lines[0];
                lines.RemoveAt(0);
                var vacationArr = vacationLine.Split(csvArr, StringSplitOptions.None);
                if (vacationArr.Length < 2) {
                    throw new LPGException("Vacation line not found.");
                }
                if (vacationArr[0] != "Vacation") {
                    throw new LPGException("Vacation line not found.");
                }
                for (var i = 1; i < hhs.Count + 1 && i < vacationArr.Length; i++) {
                    hhs[i - 1].Vacation = vacationArr[i];
                }
            }
            {
                var housetypeline = lines[0];
                lines.RemoveAt(0);
                var htArr = housetypeline.Split(csvArr, StringSplitOptions.None);
                if (htArr.Length < 2) {
                    throw new LPGException("Housetype line not found.");
                }
                if (htArr[0] != "Housetype") {
                    throw new LPGException("Housetype line not found.");
                }
                for (var i = 1; i < hhs.Count + 1 && i < htArr.Length; i++) {
                    hhs[i - 1].HouseType = htArr[i];
                }
            }
            return hhs;
        }

        private class GlobalOptions {
            [CanBeNull]
            public string Description { get; set; }
            [CanBeNull]
            public string DeviceSelection { get; set; }

            [CanBeNull]
            [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
            public string EnergyIntensity { get; set; }

            public EnergyIntensityType EnergyIntensityType
                =>
                    Enum.GetValues(typeof(EnergyIntensityType))
                        .Cast<EnergyIntensityType>()
                        .First(x => x.ToString() == EnergyIntensity);

            [CanBeNull]
            public string GeoLocName { get; set; }

            [CanBeNull]
            public string HouseholdName { get; set; }

            [ItemNotNull]
            [NotNull]
            public List<string> Tags { get; } = new List<string>();
            [CanBeNull]
            public string TemperatureProfileName { get; set; }
        }

        private class SimpleModularHousehold {
            public SimpleModularHousehold([NotNull] string name) => Name = name;

            [CanBeNull]
            public string HouseType { get; set; }

            [NotNull]
            public string Name { get; }
            [NotNull]
            public Dictionary<string, SimplePerson> Persons { get; } = new Dictionary<string, SimplePerson>();
            [CanBeNull]
            public string Vacation { get; set; }
        }

        private class SimplePerson {
            public SimplePerson([NotNull] string name) => Name = name;

            public int Age { get; set; }
            public int AverageSickDays { get; set; }
            [CanBeNull]
            public string Gender { get; set; }
            [CanBeNull]
            public string TraitTag { get; set; }
            [NotNull]
            public string Name { get; }
            public int SicknessDuration { get; set; }
            [ItemNotNull]
            [NotNull]
            public List<string> Traits { get; } = new List<string>();
        }
    }
}