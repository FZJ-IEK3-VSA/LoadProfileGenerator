using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.Validation {
    public class CalculationOutcome : DBBaseElement, IComparable<CalculationOutcome> {
        public const string TableName = "tblCalculationOutcomes";
        [NotNull] private readonly string _timeResolution;
        private int _randomSeed;

        public CalculationOutcome([NotNull] string name, [NotNull] string householdName, [NotNull] string lpgVersion, [NotNull] string temperatureProfile,
            [NotNull] string geographicLocaiton, [NotNull] string errorMessage, [NotNull] string timeResolution, [NotNull] string energyIntensity,
            TimeSpan calculationDuration, DateTime simulationStartTime, DateTime simulationEndtime,
            [NotNull] string connectionString, int numberOfPersons, int randomSeed, [CanBeNull] int? pID, [NotNull] string guid) : base(name, TableName,
            connectionString, guid) {
            Entries = new ObservableCollection<LoadtypeOutcome>();
            AffordanceTimeUses = new ObservableCollection<AffordanceTimeUseOutcome>();
            HouseholdName = householdName;
            LPGVersion = lpgVersion;
            TemperatureProfile = temperatureProfile;
            GeographicLocationName = geographicLocaiton;
            ErrorMessage = errorMessage;
            _timeResolution = timeResolution;
            EnergyIntensity = energyIntensity;
            CalculationDuration = calculationDuration;
            SimulationEndTime = simulationEndtime;
            SimulationStartTime = simulationStartTime;
            NumberOfPersons = numberOfPersons;
            _randomSeed = randomSeed;
            ID = pID;
            TypeDescription = "Calculation Outcome";
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTimeUseOutcome> AffordanceTimeUses { get; }

        public TimeSpan CalculationDuration { get; }

        [NotNull]
        [UsedImplicitly]
        public string Electricity {
            get {
                var value = double.NaN;
                foreach (var entry in Entries) {
                    if (entry.LoadTypeName?.ToUpperInvariant() == "ELECTRICITY") {
                        value = entry.Value;
                    }
                }
                return value.ToString("N1", CultureInfo.CurrentCulture) + " kWh";
            }
        }

        [UsedImplicitly]
        public double ElectricityDouble {
            get {
                var value = double.NaN;
                foreach (var entry in Entries) {
                    if (entry.LoadTypeName?.ToUpperInvariant() == "ELECTRICITY") {
                        value = entry.Value;
                    }
                }
                return value;
            }
        }

        [NotNull]
        public string EnergyIntensity { get; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<LoadtypeOutcome> Entries { get; }

        [NotNull]
        public string ErrorMessage { get; }

        [NotNull]
        public string GeographicLocationName { get; }

        [NotNull]
        [UsedImplicitly]
        public string HouseholdName { get; }

        [NotNull]
        public string LPGVersion { get; }

        public int NumberOfPersons { get; }

        public int RandomSeed {
            get => _randomSeed;
            set => SetValueWithNotify(value, ref _randomSeed, nameof(RandomSeed));
        }

        public DateTime SimulationEndTime { get; }

        public DateTime SimulationStartTime { get; }

        [NotNull]
        public string TemperatureProfile { get; }

        public int CompareTo([CanBeNull] CalculationOutcome other) {
            if (other == null) {
                return 0;
            }

            if (HouseholdName != other.HouseholdName) {
                return string.Compare(HouseholdName, other.HouseholdName, StringComparison.Ordinal);
            }
            if (GeographicLocationName != other.GeographicLocationName) {
                return string.Compare(GeographicLocationName, other.GeographicLocationName, StringComparison.Ordinal);
            }
            if (TemperatureProfile != other.TemperatureProfile) {
                return string.Compare(TemperatureProfile, other.TemperatureProfile, StringComparison.Ordinal);
            }

            return string.Compare(EnergyIntensity, other.EnergyIntensity, StringComparison.Ordinal);
        }

        public void AddAffordanceTimeUse([NotNull] string affordanceName, [NotNull] string personName, double timeInMinutes,
            int executions) {
            var name = affordanceName + " -  " + personName;
            var lo = new AffordanceTimeUseOutcome(name, IntID, affordanceName, timeInMinutes,
                personName, executions, ConnectionString, System.Guid.NewGuid().ToString());
            lo.SaveToDB();
            AffordanceTimeUses.Add(lo);
        }

        public void AddLoadType([NotNull] string loadTypeName, double value) {
            var name = loadTypeName + " -  " + value;
            var lo = new LoadtypeOutcome(name, IntID, loadTypeName, value, ConnectionString, System.Guid.NewGuid().ToString());
            lo.SaveToDB();
            Entries.Add(lo);
        }

        [NotNull]
        private static CalculationOutcome AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var householdName = dr.GetString("HouseholdName", false, string.Empty, ignoreMissingFields);
            var lpgVersion = dr.GetString("LPGVersion", false, string.Empty, ignoreMissingFields);
            if (!string.IsNullOrEmpty(lpgVersion) && lpgVersion.Contains(".")) {
                var splitv = lpgVersion.Split('.');
                if (splitv.Length > 2) {
                    lpgVersion = splitv[0] + "." + splitv[1] + "." + splitv[2];
                }
            }
            var temperatureProfile = dr.GetString("TemperatureProfile", false, string.Empty, ignoreMissingFields);
            var geographicLocationName =
                dr.GetString("GeographicLocation", false, string.Empty, ignoreMissingFields);
            var errorMessage = dr.GetString("ErrorMessage", false, string.Empty, ignoreMissingFields);
            var timeResolution = dr.GetString("TimeResolution", false, string.Empty, ignoreMissingFields);
            var energyIntensity = dr.GetString("EnergyIntensity", false, string.Empty, ignoreMissingFields);
            var calculationDuration = new TimeSpan(dr.GetLong("CalculationDuration"));
            var simulationStartTime = dr.GetDateTime("SimulationStartTime");
            var simulationEndTime = dr.GetDateTime("SimulationEndTime");
            var numberOfPersons = dr.GetIntFromLong("NumberOfPersons");
            var name = MakeName(householdName, temperatureProfile, geographicLocationName, lpgVersion,
                energyIntensity);
            var randomSeed = dr.GetIntFromLong("RandomSeed");

            return new CalculationOutcome(name, householdName, lpgVersion, temperatureProfile, geographicLocationName,
                errorMessage, timeResolution, energyIntensity, calculationDuration, simulationStartTime,
                simulationEndTime, connectionString,
                numberOfPersons, randomSeed, id, System.Guid.NewGuid().ToString());
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new
            CalculationOutcome(FindNewName(isNameTaken, "Calculation Outcome "), string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, TimeSpan.Zero, DateTime.Today,
                DateTime.Today, connectionString, 0, 0, null, System.Guid.NewGuid().ToString());

        public override void DeleteFromDB() {
            base.DeleteFromDB();
            foreach (var entry in Entries) {
                entry.DeleteFromDB();
            }
        }

        [NotNull]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dstSim")]
        [UsedImplicitly]
        public static CalculationOutcome ImportFromItem([NotNull] CalculationOutcome toImport, [NotNull] Simulator dstSim)
        {
            var hd = new CalculationOutcome(toImport.Name, toImport.HouseholdName, toImport.LPGVersion,
                toImport.TemperatureProfile, toImport.GeographicLocationName, toImport.ErrorMessage,
                toImport._timeResolution, toImport.EnergyIntensity, toImport.CalculationDuration,
                toImport.SimulationStartTime, toImport.SimulationEndTime,dstSim.ConnectionString, toImport.NumberOfPersons,
                toImport.RandomSeed, null, toImport.Guid);
            hd.SaveToDB();
            foreach (var entry in toImport.Entries) {
                if(entry.LoadTypeName!=null) {
                    hd.AddLoadType(entry.LoadTypeName, entry.Value);
                }
            }
            foreach (var affordanceTimeUse in toImport.AffordanceTimeUses) {
                hd.AddAffordanceTimeUse(affordanceTimeUse.AffordanceName, affordanceTimeUse.PersonName,
                    affordanceTimeUse.TimeInMinutes, affordanceTimeUse.Executions);
            }
            hd.SaveToDB();

            return hd;
        }

        private static bool IsCorrectAffTimeUseParent([NotNull] DBBase parent, [NotNull] DBBase child) {
            var entry = (AffordanceTimeUseOutcome) child;
            if (parent.ID == entry.CalculationOutcomeID) {
                var ats = (CalculationOutcome) parent;
                ats.AffordanceTimeUses.Add(entry);
                return true;
            }
            return false;
        }

        private static bool IsCorrectEntryParent([NotNull] DBBase parent, [NotNull] DBBase child) {
            var entry = (LoadtypeOutcome) child;
            if (parent.ID == entry.CalculationOutcomeID) {
                var ats = (CalculationOutcome) parent;
                ats.Entries.Add(entry);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<CalculationOutcome> result, [NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var entries = new ObservableCollection<LoadtypeOutcome>();
            LoadtypeOutcome.LoadFromDatabase(entries, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(entries), IsCorrectEntryParent, ignoreMissingTables);
            var afftimeUses =
                new ObservableCollection<AffordanceTimeUseOutcome>();
            AffordanceTimeUseOutcome.LoadFromDatabase(afftimeUses, connectionString, ignoreMissingTables);

            SetSubitems(new List<DBBase>(result), new List<DBBase>(afftimeUses), IsCorrectAffTimeUseParent,
                ignoreMissingTables);
        }

        [NotNull]
        private static string MakeName([NotNull] string householdname, [NotNull] string temperatureprofile, [NotNull] string geoLoc,
            [NotNull] string lpgversion, [NotNull] string intensity) => householdname + "#" + temperatureprofile + "#" + geoLoc + "#" +
                                                    lpgversion + "#" + intensity;

        public override void SaveToDB() {
            base.SaveToDB();
            foreach (var outcome in Entries) {
                outcome.SaveToDB();
            }
            foreach (var affordanceTimeUse in AffordanceTimeUses) {
                affordanceTimeUse.SaveToDB();
            }
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            cmd.AddParameter("HouseholdName", HouseholdName);
            cmd.AddParameter("LPGVersion", LPGVersion);
            cmd.AddParameter("TemperatureProfile", TemperatureProfile);
            cmd.AddParameter("GeographicLocation", GeographicLocationName);
            cmd.AddParameter("ErrorMessage", ErrorMessage);
            cmd.AddParameter("TimeResolution", _timeResolution);
            cmd.AddParameter("EnergyIntensity", EnergyIntensity);
            cmd.AddParameter("CalculationDuration", CalculationDuration.Ticks);
            cmd.AddParameter("SimulationStartTime", SimulationStartTime);
            cmd.AddParameter("SimulationEndTime", SimulationEndTime);
            cmd.AddParameter("NumberOfPersons", NumberOfPersons);
            cmd.AddParameter("RandomSeed", RandomSeed);
        }

        public override string ToString() => Name;
        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((CalculationOutcome)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();

        public static void ClearTable([NotNull] string simConnectionString)
        {
            using (Connection conn = new Connection(simConnectionString)) {
                conn.Open();
                using (Command cmd = new Command(conn)) {
                    cmd.DeleteEntireTable(TableName);
                    cmd.DeleteEntireTable(LoadtypeOutcome.TableName);
                    cmd.DeleteEntireTable(AffordanceTimeUseOutcome.TableName);
                }
            }
        }
    }
}