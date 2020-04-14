using System;
using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace SimulationEngineLib.WebRunner {
    internal static class FileReader {
        private static int GetInt([NotNull] StreamReader sr, [NotNull] string key) {
            var s = sr.ReadLine();
            var keyLength = key.Length + 1;
            if (s == null) {
                throw new LPGException("Readline failed");
            }
            if (!s.StartsWith(key, StringComparison.Ordinal)) {
                throw new LPGException("Key not found: \"" + key + "\", string was: \"" + s + "\"");
            }
            var result = s.Substring(keyLength);
            var success = int.TryParse(result, out int r);
            if (!success) {
                throw new LPGException("ID not readable");
            }
            return r;
        }

        [NotNull]
        private static string GetValue([NotNull] StreamReader sr, [NotNull] string key) {
            var s = sr.ReadLine();
            var keyLength = key.Length + 1;
            if (s == null) {
                throw new LPGException("Readline failed");
            }
            if (!s.StartsWith(key, StringComparison.Ordinal)) {
                throw new LPGException("Key not found: \"" + key + "\", was \"" + s + "\"");
            }
            var result = s.Substring(keyLength);
            return result;
        }

        [NotNull]
        public static AspHousehold ReadMyTextFile([NotNull] string fileName) {
            using (var sr = new StreamReader(fileName)) {
                var hh = new AspHousehold
                {
                    Name = GetValue(sr, "HouseholdName"),
                    AspID = GetInt(sr, "AspID")
                };
                var vacationStartStr = GetValue(sr, "VacationStart");
                var vacationEndStr = GetValue(sr, "VacationEnd");
                hh.GeographicLocationID = GetInt(sr, "GeographicLocationID");
                hh.TemperatureProfileID = GetInt(sr, "TemperatureProfileID");
                // hh.ElectricityLt = GetInt(sr, "ElectricityLT");
                //hh.ElectricityLt = GetInt(sr, "WaterLT");
               hh.SetEnergyIntensityString(GetValue(sr, "EnergyIntensity"));
                var success = DateTime.TryParse(vacationStartStr, out var vacationStart);
                if (!success) {
                    throw new LPGException("Vacation Start Date was not understandable");
                }

                success = DateTime.TryParse(vacationEndStr, out var vacationEnd);
                if (!success) {
                    throw new LPGException("Vacation End Date was not understandable");
                }
                hh.VacationStart = vacationStart;
                hh.VacationEnd = vacationEnd;
                var p = ReadPerson(sr);
                while (p != null) {
                    hh.Persons.Add(p);
                    p = ReadPerson(sr);
                }
                return hh;
            }
        }

        [CanBeNull]
        private static AspPerson ReadPerson([NotNull] StreamReader sr) {
            var s = sr.ReadLine();
            if (s != "###") {
                return null;
            }
            var person = new AspPerson
            {
                Name = GetValue(sr, "Name"),
                Age = GetInt(sr, "Age"),
                Gender = GetInt(sr, "Gender"),
                Sickdays = GetInt(sr, "SickDays")
            };
#pragma warning disable S125 // Sections of code should not be "commented out"
            sr.ReadLine(); // ++ line
#pragma warning restore S125 // Sections of code should not be "commented out"
            s = sr.ReadLine();

            while (s != "---" && !sr.EndOfStream) {
                if (s == null) {
                    throw new LPGException("readline gave null");
                }
                var trait = new AspTrait(s);
                person.Traits.Add(trait);
                s = sr.ReadLine();
            }
            sr.ReadLine(); // ### line
            return person;
        }

        internal class AspHousehold {
            public int AspID { get; set; }
            public EnergyIntensityType EnergyIntensity { get; private set; }

            public void SetEnergyIntensityString([NotNull] string value)
            {
                switch (value)
                {
                    case "Intensive":
                        EnergyIntensity = EnergyIntensityType.EnergyIntensivePreferMeasured;
                        break;
                    case "Saving":
                        EnergyIntensity = EnergyIntensityType.EnergySavingPreferMeasured;
                        break;
                    default:
                        EnergyIntensity = EnergyIntensityType.Random;
                        break;
                }
            }

            public int GeographicLocationID { get; set; }

            [CanBeNull]
            public string Name { get; set; }
            //  public int ElectricityLt { get; set; }
//            public int WaterLt { get; set; }

            [ItemNotNull]
            [NotNull]
            public List<AspPerson> Persons { get; } = new List<AspPerson>();
            public int TemperatureProfileID { get; set; }
            public DateTime VacationEnd { get; set; }
            public DateTime VacationStart { get; set; }
        }

        internal class AspPerson {
            public int Age { get; set; }
            public int Gender { get; set; }
            [CanBeNull]
            public string Name { get; set; }
            public int Sickdays { get; set; }

            [CanBeNull]
            public string TraitTag { get; set; }
            //public string Template { get; set; }
            [ItemNotNull]
            [NotNull]
            public List<AspTrait> Traits { get; } = new List<AspTrait>();
        }

        internal class AspTrait {
            public AspTrait([NotNull] string name) => Name = name;

            [NotNull]
            public string Name { get; }
        }
    }
}