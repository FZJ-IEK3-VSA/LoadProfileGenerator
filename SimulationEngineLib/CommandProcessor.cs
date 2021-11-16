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
        public void CSVImport([JetBrains.Annotations.NotNull] CsvImportOptions args)
        {
            Logger.Info("Running LoadProfileGenerator Version " + Assembly.GetExecutingAssembly().GetName().Version);
            var connectionString = MainSimEngine.GetConnectionString();
            CsvTimeProfileImporter ctpi  = new CsvTimeProfileImporter(connectionString);
            ctpi.Import(args, out _);
        }

        [ArgActionMethod]
        [ArgDescription("Imports a household definition from a text file")]
        [ArgExample("simulationengine.exe ImportHouseholdDefinition -File txt.csv",
            "How to import a household definition:")]
        [UsedImplicitly]
        public void ImportHouseholdDefinition([JetBrains.Annotations.NotNull] HouseholdDefinitionImporter.HouseholdDefinitionImporterOptions args)
        {
            var connectionString = MainSimEngine.GetConnectionString();
            HouseholdDefinitionImporter.ImportHouseholdDefinition(args, connectionString);
        }



        [ArgActionMethod]
        [ArgDescription("Creates an example for house jobs which is the best way to automate the LPG for the calculation of large settlements.")]
        [ArgExample("simulationengine.exe CreateExampleHouseJob",
            "How to prepare districts for calculations.:")]
        [ArgShortcut("PEHJ")]
        [UsedImplicitly]
        public void CreateExampleHouseJob()
        {
            var connectionString = MainSimEngine.GetConnectionString();
            HouseGenerator.CreateExampleHouseJob(connectionString);
        }

        [ArgActionMethod]
        [ArgDescription("Creates python files that will help you use call the LPG from Python")]
        [ArgExample("simulationengine.exe CreatePythonBindings",
            "How to create the python bindings:")]
        [ArgShortcut("cpy")]
        [UsedImplicitly]
        public void CreatePythonBindings()
        {
            var connectionString = MainSimEngine.GetConnectionString();
            PythonGenerator.MakeFullPythonBindings(connectionString, "lpgpythonbindings.py", @"lpgdata.py");
        }

        [ArgActionMethod]
        [ArgDescription("Reads a house data definitions in Json-Format to create new house and calculates the result." )]
        [ArgExample("simulationengine.exe ProcessHouseJob -JsonPath c:\\work\\myhousejob.json",
            "How to calculate a house:")]
        [ArgShortcut("PHJ")]
        [UsedImplicitly]
        public void ProcessHouseJob([JetBrains.Annotations.NotNull] HouseJobProcessingOptions args)
        {
            HouseGenerator.ProcessHouseJob(args);
        }

        [ArgActionMethod]
        [ArgDescription("Launches the calculations for all the JSON files in a given directory " +
                        "in parallel while optimally utilizing multiple cores.")]
        [ArgExample("simulationengine.exe LaunchJsonParallel -NumberOfCores 20 -JsonDirectory Results ", "How to launch all the json files from the directory Results, with a maximum of 20 calculations running at the same time.")]
        [UsedImplicitly]
        [ArgShortcut("LJP")]
        public void LaunchJsonParallel([JetBrains.Annotations.NotNull] ParallelJsonLauncher.ParallelJsonLauncherOptions args)
        {
            ParallelJsonLauncher.LaunchParallel(args);
        }

        [ArgActionMethod]
        [ArgDescription("Exports selected database contents as a single JSON file.")]
        [ArgExample("simulationengine.exe ExportDatabaseObjectsAsJson -t HouseholdTemplates -o myexport.json", "Export all household templates into a json file.")]
        [UsedImplicitly]
        public void ExportDatabaseObjectsAsJson([JetBrains.Annotations.NotNull] JsonDatabaseExportOptions args)
        {
            var connectionString = MainSimEngine.GetConnectionString();
            JsonDatabaseExporter hte = new JsonDatabaseExporter(connectionString);
            hte.Export(args);
        }

        [ArgActionMethod]
        [ArgDescription("Imports all household templates from a single JSON file.")]
        [ArgExample("simulationengine.exe ImportDatabaseObjectsAsJson -t HouseholdTemplates  -i myexport.json",
            "Import all household templates from a json file. This will overwrite "
                    + "any changes to ones with the same guid, but will not delete extra profiles.")]
        [UsedImplicitly]
        public void ImportDatabaseObjectsAsJson([JetBrains.Annotations.NotNull] JsonDatabaseImportOptions args)
        {
            var connectionString = MainSimEngine.GetConnectionString();
            JsonDatabaseImporter hti = new JsonDatabaseImporter(connectionString);
            hti.Import(args);
        }
    }
}