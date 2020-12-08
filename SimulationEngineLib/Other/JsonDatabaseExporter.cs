using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SimulationEngineLib.Other {
    public class JsonDatabaseExporter {
        [JetBrains.Annotations.NotNull] private readonly string _connectionString;
        [JetBrains.Annotations.NotNull] private readonly CalculationProfiler _calculationProfiler;
        public JsonDatabaseExporter([JetBrains.Annotations.NotNull] string connectionString)
        {
            _connectionString = connectionString;
            _calculationProfiler = new CalculationProfiler();
        }

        public bool Export([JetBrains.Annotations.NotNull] JsonDatabaseExportOptions calcDirectoryOptions)
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            string jsonFileName = calcDirectoryOptions.Output;
            if (jsonFileName == null)
            {
                Logger.Error("No file was set.");
                return false;
            }

            Logger.Info("Loading...");
            var sim = new Simulator(_connectionString);
            Logger.Info("Loading finished.");
            switch (calcDirectoryOptions.ProcessingType) {
                case TypesToProcess.HouseholdTemplates:
                    ExportStuff<HouseholdTemplate.JsonDto,HouseholdTemplate>(jsonFileName,
                        sim.HouseholdTemplates.Items.ToList());
                    break;
                case TypesToProcess.ModularHouseholds:
                    ExportStuff<ModularHousehold.JsonModularHousehold, ModularHousehold>(jsonFileName,
                        sim.ModularHouseholds.Items.ToList());
                    break;
                case TypesToProcess.None:
                    throw new LPGException("You need to set a type that you want to process");
                case TypesToProcess.HouseholdTraits:
                    ExportStuff<HouseholdTrait.JsonDto, HouseholdTrait>(jsonFileName,
                        sim.HouseholdTraits.Items.ToList());
                    break;
                case TypesToProcess.HouseholdTraitsWithDeviceCategories:
                    ExportHHTsWithDeviceCategories(jsonFileName, sim);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(TypesToProcess));
            }

            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return true;
        }

        private class HouseholdTraitJtoForDeviceCategoryExport{
            public HouseholdTraitJtoForDeviceCategoryExport(string trait) => Trait = trait;

            public string Trait { [UsedImplicitly] get; }
            public class PossibleDevice {
                public PossibleDevice(string deviceName, string fullDeviceCategory)
                {
                    DeviceName = deviceName;
                    FullDeviceCategory = fullDeviceCategory;
                }

                public string DeviceName { get; }
                public string FullDeviceCategory { [UsedImplicitly] get; }
            }
            public List<PossibleDevice> PossibleDevices { get;  } = new List<PossibleDevice>();

            public void AddDevice(RealDevice rd)
            {
                if (PossibleDevices.Any(x => x.DeviceName == rd.Name)) {
                    return;
                }
                PossibleDevices.Add(new PossibleDevice(rd.Name,rd.DeviceCategory?.FullPath) );
            }
        }

        private static void ExportHHTsWithDeviceCategories([JetBrains.Annotations.NotNull] string jsonFileName, [JetBrains.Annotations.NotNull] Simulator sim)
        {
            List<HouseholdTraitJtoForDeviceCategoryExport> hhtj = new List<HouseholdTraitJtoForDeviceCategoryExport>();

            foreach (var trait in sim.HouseholdTraits.Items) {
                HouseholdTraitJtoForDeviceCategoryExport htj = new HouseholdTraitJtoForDeviceCategoryExport(trait.Name);
                hhtj.Add(htj);
                foreach (var autodev in trait.Autodevs) {
                    var rds = autodev.Device?.GetRealDevices(sim.DeviceActions.Items);
                    if (rds != null) {
                        foreach (var realDevice in rds) {
                            htj.AddDevice(realDevice);
                        }
                    }
                }

                foreach (var loc in trait.Locations) {
                    foreach (var affloc in loc.AffordanceLocations) {
                        var affdevs = affloc.Affordance?.AffordanceDevices;
                        if (affdevs != null) {
                            foreach (var affdev in affdevs) {
                                var rds = affdev.Device?.GetRealDevices(sim.DeviceActions.Items);
                                if (rds != null) {
                                    foreach (var realDevice in rds) {
                                        htj.AddDevice(realDevice);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string json = JsonConvert.SerializeObject(hhtj, Formatting.Indented);
            File.WriteAllText(jsonFileName, json);
            Logger.Info("Finished exporting " + hhtj.Count + " items to " + jsonFileName);
        }
        // ReSharper disable once InconsistentNaming
        private static void ExportStuff<T,U>([JetBrains.Annotations.NotNull] string jsonFileName,  [JetBrains.Annotations.NotNull] [ItemNotNull] List<U> items) where U : IJsonSerializable<T>
        {
            List<T> hhts = new List<T>();
            foreach (var template in items) {
                hhts.Add(template.GetJson());
            }

            string json = JsonConvert.SerializeObject(hhts, Formatting.Indented);
            File.WriteAllText(jsonFileName, json);
            Logger.Info("Finished exporting " + hhts.Count + " items to " + jsonFileName);
        }
    }
}