using System.IO;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace SimEngine2.Other {
    internal class CsvTimeProfileImporter
    {
        [NotNull] private readonly string _connectionString;
        [NotNull] private readonly CalculationProfiler _calculationProfiler;
        public CsvTimeProfileImporter([NotNull] string connectionString)
        {
            _connectionString = connectionString;
            _calculationProfiler = new CalculationProfiler();
        }

        public bool Import([NotNull] CsvImportOptions calcDirectoryOptions, [CanBeNull] out DateBasedProfile newProfile )
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            string csvFilename = calcDirectoryOptions.Input;
            newProfile = null;
            if (csvFilename == null)
            {
                Logger.Error("No file was set.");
                return false;
            }

            if (!File.Exists(csvFilename)) {
                Logger.Error("File does not exist");
                return false;
            }

            if (calcDirectoryOptions.Delimiter == null) {
                Logger.Error("No delimiter set.");
                return false;
            }
            Logger.Info("Loading...");
            CSVImporter ci = new CSVImporter(true) {
                FileName = csvFilename,
                TimeColumn = 1,
                HeaderLineCount = 1,
                Separator = calcDirectoryOptions.Delimiter[0],
                Column = 2
            };
            ci.RefreshEntries();
            if (ci.Entries.Count == 0) {
                throw new LPGException("Not a single entry was found");
            }
            Logger.Info("PreviewText from the import:");
            Logger.Info(ci.PreviewText,true);
            //asd

            var sim = new Simulator(_connectionString);
            Logger.Info("Loading finished.");

            Logger.Info("Importing " + ci.Entries.Count + " datapoints...");
            var dbp = sim.DateBasedProfiles.CreateNewItem(sim.ConnectionString);
            for (var i = 0; i < ci.Entries.Count; i++)
            {
                var ce = ci.Entries[i];
                var date = ce.Time;
                dbp.AddNewDatePoint(date, ce.Value, false);
            }
            dbp.Datapoints.Sort();
            dbp.SaveToDB();
            newProfile = dbp;
            Logger.Info("Imported all data points.");

            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return true;
        }

    }
}