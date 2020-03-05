using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SimulationEngine.SimZukunftProcessor {
    public class HouseGenerationStatistics
    {
        public class StatisticsEntry
        {
            public StatisticsEntry([NotNull] string fileName, [NotNull] JsonCalcSpecification jcs, [NotNull] string sourceDirectory)
            {
                this.FileName = fileName;
                Jcs = jcs;
                SourceDirectory = sourceDirectory;
            }

            [NotNull]
            public string FileName { get; set; }
            [NotNull]
            public JsonCalcSpecification Jcs { get; set; }

            [NotNull]
            public string SourceDirectory { get; set; }

            public int TotalPersonCount { get; set; } = -1;
            public int HouseholdCount { get; set; } = -1;
            public int PlannedHouseholdCount { get; set; } = -1;
            public int PlannedPersonCount { get; set; } = -1;

            [CanBeNull]
            public string SourceDistrict { get; set; }

            [NotNull]
            public string GetCsvLine()
            {
                return FileName + ";"  + Jcs.CalculationName + ";" + Jcs.CalcObject?.Guid + ";" + Jcs.OutputDirectory + ";" + TotalPersonCount
                       + ";" + HouseholdCount + ";" + PlannedPersonCount + ";" + PlannedHouseholdCount + ";" + SourceDistrict;
            }
        }
        public void Run([NotNull] string directory, [NotNull] string districtDefinitionJsonPath)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            var subdirs = di.GetDirectories();
            List<StatisticsEntry> entries = new List<StatisticsEntry>();
            foreach (var subdir in subdirs) {
                var files = subdir.GetFiles("*.json");
                foreach (var file in files) {
                    string json = File.ReadAllText(file.FullName);
                    JsonCalcSpecification jcs = JsonConvert.DeserializeObject<JsonCalcSpecification>(json);
                    var e = new StatisticsEntry(file.FullName,jcs,subdir.Name);
                    entries.Add(e);
                }

                var database = subdir.GetFiles("*.db3").ToList();
                if (database.Count != 1) {
                    throw new LPGException("No database found");
                }

                string connectionString = "Data Source=" + database[0].FullName;
                Simulator sim = new Simulator(connectionString);
                foreach (StatisticsEntry entry in entries) {
                    var house = sim.Houses.FindByJsonReference(entry.Jcs.CalcObject);
                    if (house != null) {
                        entry.HouseholdCount = house.Households.Count;
                        entry.TotalPersonCount = house.Households.Sum(x => x.CalcObject?.AllPersons.Count ?? 0);
                    }
                }
            }
            ReadSourceData(entries, districtDefinitionJsonPath);
            StreamWriter sw = new StreamWriter(di.CombineName("Statistics.csv"));
            sw.WriteLine("Calc Definition;Name;HouseGuid;Directory;Real Persons;Real Households;Planned Persons;Planned Households;Source File");
            foreach (StatisticsEntry statisticsEntry in entries) {
                sw.WriteLine(statisticsEntry.GetCsvLine());
            }
            sw.Close();
        }

        private void ReadSourceData([NotNull] [ItemNotNull] List<StatisticsEntry> statisticsEntries, [NotNull] string jsonExportPath)
        {
            DirectoryInfo sourcejsonPath = new DirectoryInfo(jsonExportPath);
            var files = sourcejsonPath.GetFiles("*.json");
            List<HouseData> houses = new List<HouseData>();
            Dictionary<HouseData, DistrictData> districts = new Dictionary<HouseData, DistrictData>();
            foreach (var fi in files)
            {
                string json = File.ReadAllText(fi.FullName);
                var hds = JsonConvert.DeserializeObject<DistrictData>(json);
                houses.AddRange(hds.Houses);
                foreach (var hdsHouse in hds.Houses) {
                    districts.Add(hdsHouse, hds);
                }
            }

            foreach (StatisticsEntry entry in statisticsEntries) {
                HouseData hd = houses.Single(x => x.Name == entry.Jcs.CalculationName);
                entry.PlannedHouseholdCount = hd.Households.Count;
                foreach (var household in hd.Households) {
                    switch (household.HouseholdDataSpecifictionType) {
                        case HouseholdDataSpecifictionType.ByPersons:
                            entry.PlannedPersonCount += household.HouseholdDataPersonSpecification?.Persons.Count??throw new LPGException("No persons specified");
                            break;
                        case HouseholdDataSpecifictionType.ByTemplateName:
                            throw new ArgumentOutOfRangeException();
                        case HouseholdDataSpecifictionType.ByHouseholdName:
                            throw new ArgumentOutOfRangeException();
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                entry.SourceDistrict = districts[hd].Name;
            }
        }
    }
}