using System;
using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using JetBrains.Annotations;
using PowerArgs;

namespace SimulationEngineLib.Calculation {
    public class CalculationOptions {
        [UsedImplicitly]
        [ArgDescription("Sets the number of the object to calculate. You can find the number with the List function.")]
        [ArgShortcut(null)]
        [CanBeNull]
        public int? CalcObjectNumber { get; set; }

        [ArgDescription("Sets what to calculate")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public CalcObjectType CalcObjectType { get; set; }

        [CanBeNull]
        [UsedImplicitly]
        [ArgDescription("Enables you to set specific calculation options. Mostly for debugging purposes.")]
        [ArgShortcut(null)]
        public List<CalcOption> CalcOption { get; set; }= new List<CalcOption>();

        [UsedImplicitly]
        [ArgDescription("Enables the logging of the duration of individual calculation parts to help with debugging.")]
        [ArgShortcut(null)]
        public bool MeasureCalculationTimes { get; set; }

        [CanBeNull]
        [ArgIgnore]
        [UsedImplicitly]

        public string ConnectionString { get; set; }

        [CanBeNull]
        [ArgDescription("Sets the database file to use. ")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public string Database { get; set; }

        [ArgDescription("Makes the LPG delete most of the files except the PDF")]
        [ArgShortcut(null)]
        [UsedImplicitly]
     //   [ArgDefaultValue(false)]
        public bool DeleteAllButPDF { get; set; }

        [ArgDescription("Makes the LPG delete the DAT files")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public bool DeleteDAT { get; set; }

        [CanBeNull]
        [ArgDescription("Name of the device selection to use")]
        [ArgShortcut(null)]
        [UsedImplicitly]
       // [ArgDefaultValue(null)]
        public string DeviceSelectionName { get; set; }

        [ArgDescription("End date of the simulation")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [CanBeNull]
        public DateTime? EndDate { get; set; }

        [ArgDescription("Energy intensity type to be used")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public EnergyIntensityType EnergyIntensityType { get; set; } = EnergyIntensityType.AsOriginal;

        [UsedImplicitly]
        [ArgShortcut(null)]
        [ArgDescription("Show the excel day and time column in the result files")]
        public bool ExcelColumn { get; set; }

        [CanBeNull]
        [ArgDescription("Set an external time resolution")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public string ExternalTimeResolution { get; set; }

        [ArgDescription("Force the creation of charts")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [ArgDefaultValue(false)]
        public bool ForceCharts { get; set; }

        [ArgDescription("Force the creation of the PDF")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        //[ArgDefaultValue(false)]
        public bool ForcePDF { get; set; }

        [ArgDescription("Index of the geographic location to use for the calculation")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [CanBeNull]
        public int? GeographicLocationIndex { get; set; }

        [UsedImplicitly]
        [ArgShortcut(null)]
        [ArgDescription("Sets the load types to calculate. Less load types are faster.")]
        [ArgDefaultValue(LoadTypePriority.Undefined)]
        public LoadTypePriority LoadTypePriority { get; set; } = (LoadTypePriority.Undefined);

        [CanBeNull]
        [UsedImplicitly]
        [ArgShortcut(null)]
        [ArgDescription("Sets the output directory for the result files")]
        public string OutputDirectory { get; set; }

        [ArgDescription("Sets the set of output files to include. For details see the settings inside the desktop client.")]
        [UsedImplicitly]
        [ArgShortcut(null)]
        [ArgDefaultValue(OutputFileDefault.ReasonableWithChartsAndPDF)]
        public OutputFileDefault OutputFileDefault { get; set; } = OutputFileDefault.ReasonableWithChartsAndPDF;

        [ArgDescription("Sets the random seed to make calculations reproducible")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [ArgDefaultValue(-1)]
        public int RandomSeed { get; set; } = -1;

        [UsedImplicitly]
        [ArgDescription(
            "The LPG has a settling period of 3 days before the official simulation start to initialize the values. With this option" +
            "you can display the settling period in the result files.")]
        [ArgShortcut(null)]
        [ArgDefaultValue(false)]
        public bool ShowSettlingPeriod { get; set; }

        [UsedImplicitly]
        [ArgDescription("Skips the calculation if the calculation result already exists.")]
        [ArgShortcut(null)]
        public bool SkipExisting { get; set; }

        [ArgDescription("Sets the start date for the calculation. Default: 01.01. of the current year.")]
        [UsedImplicitly]
        [ArgShortcut(null)]
        [CanBeNull]
        public DateTime? StartDate { get; set; }

        [ArgDescription("Sets the index of the temperature profile to use for the calculation. Use list to find the index.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [CanBeNull]
        public int? TemperatureProfileIndex { get; set; }

        [ArgDescription("Sets the id of the transportation device set to use.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [CanBeNull]
        public int? TransportationDeviceSetIndex { get; set; }

        [ArgDescription("Sets the id of the travel route set to use.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [CanBeNull]
        public int? TravelRouteSetIndex { get; set; }

        [UsedImplicitly]
        public bool Testing { get; set; }
        [ArgDescription("Sets the id of the charging station set to use.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [CanBeNull]
        public int? ChargingStationSetIndex { get; set; }

        public bool CheckSettings([NotNull] string connectionString)
        {
            ConnectionString = connectionString;
            if (!string.IsNullOrWhiteSpace(Database)) {
                var fi = new FileInfo(Database);
                if (!fi.Exists) {
                    throw new LPGException("Database was not found.");
                }
                ConnectionString = "Data Source=" + Database;
            }
            if (CalcObjectNumber == null) {
                Logger.Error("No calc object number was set. Can't continue.");
                return false;
            }
            if (StartDate == null) {
                StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            }
            if (EndDate == null) {
                EndDate = new DateTime(DateTime.Now.Year, 12, 31);
            }
            if (OutputDirectory == null) {
                OutputDirectory = CalcObjectType + "_" + CalcObjectNumber;
            }
            if (TemperatureProfileIndex == null) {
                TemperatureProfileIndex = 0;
            }
            if (GeographicLocationIndex == null) {
                GeographicLocationIndex = 0;
            }
            Logger.Info("Output File Setting is: " + OutputFileDefault.ToString());
            Logger.Info("The used LoadTypePriority Default was: " + LoadTypePriority.ToString());
            if ( LoadTypePriority == LoadTypePriority.Undefined) {
                Logger.Info("LoadTypePriority was not set. Determing setting...");
                if (CalcObjectType == CalcObjectType.ModularHousehold) {
                    LoadTypePriority = LoadTypePriority.RecommendedForHouseholds;
                    Logger.Info("LoadTypePriority for households was selected");
                }
                else {
                    LoadTypePriority = LoadTypePriority.RecommendedForHouses;
                    Logger.Info("LoadTypePriority for houses was selected");
                }
            }

            return true;
        }
    }
}