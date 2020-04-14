/*using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using CommonDataWPF;
using DatabaseIO;
using DatabaseIO.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace SimulationEngine.WebRunner {
    internal static class SqliteExporter {
        private static List<TraitCategory> MakeCategories(Simulator sim, SQLiteConnection dstcon) {
            const string createstr = "CREATE TABLE \"TraitCategory\"( " + "`ID`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                            "`Name`	Varchar(100)," + "`Priority`	INTEGER)";

            Logger.Info(createstr);
            using (var cmdOn = new SQLiteCommand(createstr, dstcon)) {
                cmdOn.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand("DELETE FROM TraitCategory", dstcon)) {
                cmd.ExecuteNonQuery();
            }
            var categories = new List<TraitCategory>();
            var i = 0;
            foreach (var tag in sim.TraitTags.It) {
                var count = sim.HouseholdTraits.It.Count(x => x.Tags.Any(y => y.Tag == tag));
                if (tag.TraitPriority != TraitPriority.ForExperts && count > 0) {
                    const string cmdStr =
                        "INSERT INTO TraitCategory (Name, Priority) VALUES (@name,@priority); SELECT last_insert_rowid()";
                    using (var cmd = new SQLiteCommand(cmdStr, dstcon)) {
                        cmd.Parameters.AddWithValue("@name", tag.Name);
                        cmd.Parameters.AddWithValue("@priority", (int) tag.TraitPriority);
                        var o = cmd.ExecuteScalar();
                        i++;
                        var id = (int) (long) o;
                        categories.Add(new TraitCategory(id, tag.Name));
                    }
                }
            }
            Logger.Info("Imported " + i + " Categories");
            return categories;
        }

        private static void MakeGeoLocs(SQLiteConnection dstcon, Simulator sim) {
            Console.WriteLine();
            const string createstr = "CREATE TABLE \"GeographicLocation\"( " +
                            "`ID`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," + "`Name`	Varchar(100))";
            Logger.Info(createstr);
            using (var cmdOn = new SQLiteCommand(createstr, dstcon)) {
                cmdOn.ExecuteNonQuery();
            }
            using (var delete = new SQLiteCommand("DELETE FROM GeographicLocation ", dstcon)) {
                delete.ExecuteNonQuery();
            }
            var i = 0;
            foreach (var tp in sim.GeographicLocations.It) {
                using (var cmd =
                    new SQLiteCommand("INSERT INTO GeographicLocation (ID,Name) VALUES (@ID,@name); ", dstcon)) {
                    cmd.Parameters.AddWithValue("@name", tp.Name);
                    cmd.Parameters.AddWithValue("@ID", tp.IntID);
                    cmd.ExecuteScalar();
                    i++;
                }
            }
            Logger.Info("Imported " + i + " Geographic Locations");
        }

        private static void MakeHouseholds(Simulator sim, SQLiteConnection dstcon) {
            const string createstr = "CREATE TABLE \"HouseholdTemplates\"( " +
                            "`ID`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," + "`Name`	Varchar(100))";

            Logger.Info(createstr);
            using (var cmdOn = new SQLiteCommand(createstr, dstcon)) {
                cmdOn.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand("DELETE FROM HouseholdTemplates", dstcon)) {
                cmd.ExecuteNonQuery();
            }
            var i = 0;
            foreach (var mhh in sim.ModularHouseholds.It) {
                const string cmdStr =
                    "INSERT INTO HouseholdTemplates (ID,Name) VALUES (@id, @name); SELECT last_insert_rowid()";
                using (var cmd = new SQLiteCommand(cmdStr, dstcon)) {
                    cmd.Parameters.AddWithValue("@name", mhh.Name);
                    cmd.Parameters.AddWithValue("@id", mhh.IntID);
                    cmd.ExecuteScalar();
                    i++;
                }
            }
            Logger.Info("Imported " + i + " household templates");
        }

        private static void MakePersonTemplates(Simulator sim, SQLiteConnection dstcon) {
            const string createstr = "CREATE TABLE \"PersonTemplates\"( " +
                            "`ID`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," + "`Name`	Varchar(100)," +
                            "`Description`	Varchar(100)," + "`Age` INTEGER," + "`Gender`	INTEGER," +
                            "`SickDays`	 INTEGER, `HouseholdNumber` INTEGER)";

            Logger.Info(createstr);
            using (var cmdOn = new SQLiteCommand(createstr, dstcon)) {
                cmdOn.ExecuteNonQuery();
            }

            const string createstr2 = "CREATE TABLE \"TemplateTraits\"( " + "`personTemplateId`	INTEGER, " +
                             "`TraitID`	INTEGER)";

            Logger.Info(createstr2);
            using (var cmdOn = new SQLiteCommand(createstr2, dstcon)) {
                cmdOn.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand("DELETE FROM PersonTemplates", dstcon)) {
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new SQLiteCommand("DELETE FROM TemplateTraits", dstcon)) {
                cmd.ExecuteNonQuery();
            }
            var i = 0;
            foreach (var tag in sim.TemplatePersons.It) {
                using (var cmd =
                    new SQLiteCommand(
                        "INSERT INTO PersonTemplates (ID,Name, Description, Age, Gender, SickDays, HouseholdNumber ) " +
                        "VALUES (@id, @name,@description, @age, @gender,@sickdays,@HouseholdNumber); select last_insert_rowid()",
                        dstcon)) {
                    cmd.Parameters.AddWithValue("@id", tag.ID);
                    cmd.Parameters.AddWithValue("@name", tag.Name);
                    cmd.Parameters.AddWithValue("@description", tag.Description);
                    cmd.Parameters.AddWithValue("@age", tag.Age);
                    cmd.Parameters.AddWithValue("@gender", tag.Gender);
                    cmd.Parameters.AddWithValue("@sickdays", tag.SickDays);
                    cmd.Parameters.AddWithValue("@HouseholdNumber", tag.BaseHousehold.IntID);
                    cmd.ExecuteScalar();
                    i++;
                    foreach (var templatePersonTrait in tag.Traits) {
                        using (var cmd2 =
                            new SQLiteCommand(
                                "INSERT INTO TemplateTraits (PersonTemplateID, TraitID) VALUES " +
                                "(@personTemplateId, @traitID); SELECT last_insert_rowid()", dstcon)) {
                            cmd2.Parameters.AddWithValue("@personTemplateId", tag.IntID);
                            cmd2.Parameters.AddWithValue("@traitID", templatePersonTrait.Trait.IntID);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                }
            }
            Logger.Info("Imported " + i + " Template Persons");
        }

        private static void MakeTemperatureProfiles(SQLiteConnection dstcon, Simulator sim) {
            Console.WriteLine();
            const string createstr = "CREATE TABLE \"TemperatureProfiles\"( " +
                            "`ID`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," + "`Name`	Varchar(100))";
            Logger.Info(createstr);
            using (var cmdOn = new SQLiteCommand(createstr, dstcon)) {
                cmdOn.ExecuteNonQuery();
            }
            using (var delete = new SQLiteCommand("DELETE FROM TemperatureProfiles ", dstcon)) {
                delete.ExecuteNonQuery();
            }
            var i = 0;

            foreach (var tp in sim.TemperatureProfiles.It) {
                using (var cmd =
                    new SQLiteCommand("INSERT INTO TemperatureProfiles (ID,Name) VALUES (@ID,@name); ", dstcon)) {
                    cmd.Parameters.AddWithValue("@name", tp.Name);
                    cmd.Parameters.AddWithValue("@ID", tp.IntID);
                    cmd.ExecuteScalar();
                    i++;
                }
            }
            Logger.Info("Imported " + i + " Temperature Profiles");
        }

        private static void MakeTraits(Simulator sim, SQLiteConnection dstcon, List<TraitCategory> categories) {
            Console.WriteLine();
            const string createstr = "CREATE TABLE \"Traits\"( " + "`ID`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                            "`Name`	Varchar(100), " + "`MinimumAge`	INTEGER, " + "`MaximumAge`	INTEGER, " +
                            "`TraitCategoryID`	INTEGER, " + "`Classification`	Varchar(100), " +
                            "`PrettyName`	Varchar(100)," + "`EstimatedTime`	DOUBLE, " + "`MinimumPeople`	INTEGER," +
                            "`MaximumPeople`	INTEGER," + "`Gender`	INTEGER" + ")";
            Logger.Info(createstr);
            using (var cmdOn = new SQLiteCommand(createstr, dstcon)) {
                cmdOn.ExecuteNonQuery();
            }
            using (var cmd = new SQLiteCommand("DELETE FROM Traits", dstcon)) {
                cmd.ExecuteNonQuery();
            }
            var i = 0;
            foreach (var trait in sim.HouseholdTraits.It) {
                var mytags = trait.Tags.Where(x => x.Tag.TraitPriority != TraitPriority.ForExperts).ToList();
                if (mytags.Count == 0) {
                    continue;
                }
                if (mytags.Count != 1) {
                    throw new LPGException("Too many  non-expert tags on a web trait:" + trait.PrettyName + " tag:" +
                                           mytags[0].PrettyName + " tag prio:" + mytags[0].Tag.TraitPriority);
                }
                using (var cmd =
                    new SQLiteCommand(
                        "INSERT INTO Traits (ID,Name, MinimumAge, MaximumAge,TraitCategoryID,Classification, PrettyName, EstimatedTime," +
                        " MinimumPeople,MaximumPeople,Gender ) VALUES " +
                        "(@id,@name,@minAge,@maxAge,@traitcategoryID, @classification, @prettyName, @estimatedTime," +
                        "@minimumpeople,@maximumpeople,@gender); SELECT last_insert_rowid()", dstcon)) {
                    i++;
                    cmd.Parameters.AddWithValue("@id", trait.IntID);
                    cmd.Parameters.AddWithValue("@name", trait.WebName);
                    cmd.Parameters.AddWithValue("@prettyname", trait.PrettyName);
                    cmd.Parameters.AddWithValue("@minAge", 1);
                    cmd.Parameters.AddWithValue("@maxAge", 99);
                    if (trait.Classification == null) {
                        throw new LPGException("Empty classification");
                    }
                    cmd.Parameters.AddWithValue("@classification", trait.Classification);
                    var categoryid = categories.First(x => x.Name == mytags[0].Name).ID;
                    cmd.Parameters.AddWithValue("@traitcategoryID", categoryid);
                    cmd.Parameters.AddWithValue("@estimatedTime", trait.EstimatedTimePerYearInH);
                    cmd.Parameters.AddWithValue("@minimumpeople", trait.MinimumPersonsInCHH);
                    cmd.Parameters.AddWithValue("@maximumpeople", trait.MaximumPersonsInCHH);
                    cmd.Parameters.AddWithValue("@gender", trait.PermittedGender);
                    cmd.ExecuteScalar();
                }
            }
            Logger.Info("Imported " + i + " traits");
        }

        public static void RunFullExport(string connectionString) {
            Logger.Threshold = Severity.Error;
            Config.MakePDFCharts = false;
            var sim = new Simulator(connectionString);
            SQLiteConnection.CreateFile("LPGData.sqlite");

            const string constr = "Data Source=LPGData.sqlite";
            using (var dstcon = new SQLiteConnection(constr)) {
                dstcon.Open();
                MakeTemperatureProfiles(dstcon, sim);
                MakeGeoLocs(dstcon, sim);
                var categories = MakeCategories(sim, dstcon);
                MakeHouseholds(sim, dstcon);
                MakeTraits(sim, dstcon, categories);
                MakePersonTemplates(sim, dstcon);
                dstcon.Close();
            }
        }

        private class TraitCategory {
            public TraitCategory(int id, string name) {
                ID = id;
                Name = name;
            }

            public int ID { get; }
            public string Name { get; }
        }

        [UsedImplicitly]
        internal class WebExporterOptions {
        }
    }
}*/