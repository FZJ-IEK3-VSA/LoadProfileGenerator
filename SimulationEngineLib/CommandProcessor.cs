using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Common;
using JetBrains.Annotations;
using PowerArgs;
using SimulationEngineLib.Calculation;
using SimulationEngineLib.HouseJobProcessor;
using SimulationEngineLib.Other;

namespace SimulationEngineLib {
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    [ArgDescription("The simulation engine command line interface for the generation of new load profiles.")]
    //[ArgExample("SimulationEngine  ProgressSettlement -Directory c:\\work -SettlementNumber 1 -SettlementCheck", "", Title = "Check a ")]
    public class CommandProcessor {

        public static Action<DirectoryInfo, CalculationProfiler> MakeFlameChart { get; set; }
        [NotNull]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
#pragma warning disable CA1044 // Properties should not be write only
        public static string ConnectionString { private get; set; }
#pragma warning restore CA1044 // Properties should not be write only
        [HelpHook]
        [ArgShortcut("-?")]
        [ArgDescription("Shows this help")]
        [UsedImplicitly]
        public bool Help { get; set; }



        [ArgActionMethod]
        [ArgDescription("Imports a date based profile from a CSV file. Meant to automate importing new profiles if you have a lot of them. " +
                        "Warning: Large profiles slow down the start of the "
                        + "LPG, since they are loaded fully into memory at program start."
                        + " So don't overdo it. Five is probably fine, 500 will take a while to load.'")]
        [ArgExample("simulationengine.exe CSVImport -i mycsv -d ; -n my_profile",
            "How to create a batch file to calculate the houses in settlement 1")]
        [UsedImplicitly]
        public void CSVImport([NotNull] CsvImportOptions args)
        {
            Logger.Info("Running LoadProfileGenerator Version " + Assembly.GetExecutingAssembly().GetName().Version);
            CsvTimeProfileImporter ctpi  = new CsvTimeProfileImporter(ConnectionString);
            ctpi.Import(args, out _);
        }

        [ArgActionMethod]
        [ArgDescription("Imports a household definition from a text file")]
        [ArgExample("simulationengine.exe ImportHouseholdDefinition -File txt.csv",
            "How to import a household definition:")]
        [UsedImplicitly]
        public void ImportHouseholdDefinition([NotNull] HouseholdDefinitionImporter.HouseholdDefinitionImporterOptions args) {
            HouseholdDefinitionImporter.ImportHouseholdDefinition(args, ConnectionString);
        }



        [ArgActionMethod]
        [ArgDescription("Creates an example for house jobs which is the best way to automate the LPG for the calculation of large settlements.")]
        [ArgExample("simulationengine.exe CreateExampleHouseJob",
            "How to prepare districts for calculations.:")]
        [ArgShortcut("PEHJ")]
        [UsedImplicitly]
        public void CreateExampleHouseJob()
        {
            HouseGenerator.CreateExampleHouseJob(ConnectionString);
        }

        [ArgActionMethod]
        [ArgDescription("Creates python files that will help you use call the LPG from Python")]
        [ArgExample("simulationengine.exe CreatePythonBindings",
            "How to create the python bindings:")]
        [ArgShortcut("cpy")]
        [UsedImplicitly]
        public void CreatePythonBindings()
        {
            PythonGenerator.MakeFullPythonBindings(ConnectionString, "lpgpythonbindings.py", @"lpgdata.py");
        }

        [ArgActionMethod]
        [ArgDescription("Reads a house data definitions in Json-Format to create new house and calculates the result." )]
        [ArgExample("simulationengine.exe ProcessHouseJob -JsonPath c:\\work\\myhousejob.json",
            "How to calculate a house:")]
        [ArgShortcut("PHJ")]
        [UsedImplicitly]
        public void ProcessHouseJob([NotNull] HouseJobProcessingOptions args)
        {
            HouseGenerator.ProcessHouseJob(args);
        }

        [ArgActionMethod]
        [ArgDescription("Launches the calculations for all the JSON files in a given directory " +
                        "in parallel while optimally utilizing multiple cores.")]
        [ArgExample("simulationengine.exe LaunchJsonParallel -NumberOfCores 20 -JsonDirectory Results ", "How to launch all the json files from the directory Results, with a maximum of 20 calculations running at the same time.")]
        [UsedImplicitly]
        [ArgShortcut("LJP")]
        public void LaunchJsonParallel([NotNull] ParallelJsonLauncher.ParallelJsonLauncherOptions args)
        {
            ParallelJsonLauncher.LaunchParallel(args);
        }

        [ArgActionMethod]
        [ArgDescription("Exports selected database contents as a single JSON file.")]
        [ArgExample("simulationengine.exe ExportDatabaseObjectsAsJson -t HouseholdTemplates -o myexport.json", "Export all household templates into a json file.")]
        [UsedImplicitly]
        public void ExportDatabaseObjectsAsJson([NotNull] JsonDatabaseExportOptions args)
        {
            JsonDatabaseExporter hte = new JsonDatabaseExporter(ConnectionString);
            hte.Export(args);
        }

        [ArgActionMethod]
        [ArgDescription("Imports all household templates from a single JSON file.")]
        [ArgExample("simulationengine.exe ImportDatabaseObjectsAsJson -t HouseholdTemplates  -i myexport.json",
            "Import all household templates from a json file. This will overwrite "
                    + "any changes to ones with the same guid, but will not delete extra profiles.")]
        [UsedImplicitly]
        public void ImportDatabaseObjectsAsJson([NotNull] JsonDatabaseImportOptions args)
        {
            JsonDatabaseImporter hti = new JsonDatabaseImporter(ConnectionString);
            hti.Import(args);
        }
    }
}