using System;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database;
using Database.Tables;
using JetBrains.Annotations;
using PowerArgs;

namespace SimulationEngine.Calculation {
    [UsedImplicitly]
    public class ListOptions {
        [ArgDescription("Lists the geographic locations profiles")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public bool GeographicLocations { get; set; }

        [ArgDescription("Lists the houses")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public bool Houses { get; set; }

        [ArgDescription("Lists the LoadtypePriorities (which load types are to be generated)")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public bool LoadtypePriorities { get; set; }

        [ArgDescription("Lists the load types")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public bool LoadTypes { get; set; }

        [ArgDescription("Lists the modular households")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public bool ModularHouseholds { get; set; }

        [ArgDescription("Lists the settlements")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public bool Settlements { get; set; }

        [ArgDescription("Lists the temperature profiles")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public bool TemperatureProfiles { get; set; }
    }

    internal static class ListProcessor {
        private static void PrintList([NotNull] string connectionString, CalcObjectType type) {
            Logger.Info("Loading...");
            var sim = new Simulator(connectionString);
            Logger.Info("Loading finished.");
            var i = 0;
            List<DBBase> items;
            switch (type) {
                case CalcObjectType.ModularHousehold:
                    items = new List<DBBase>(sim.ModularHouseholds.It);
                    break;
                case CalcObjectType.House:
                    items = new List<DBBase>(sim.Houses.It);
                    break;
                case CalcObjectType.Settlement:
                    items = new List<DBBase>(sim.Settlements.It);
                    break;
                default: throw new LPGException("Unknown type");
            }
            foreach (var household in items) {
                Logger.Info("[" + i++ + "] " + household.Name);
            }
        }

        private static void PrintList([NotNull] string connectionString, [NotNull] string type) {
            Logger.Info("Loading...");
            var sim = new Simulator(connectionString);
            Logger.Info("Loading finished.");
            var i = 0;
            List<DBBase> items;
            switch (type) {
                case "TemperatureProfiles":
                    items = new List<DBBase>(sim.TemperatureProfiles.It);
                    break;
                case "GeographicLocations":
                    items = new List<DBBase>(sim.GeographicLocations.It);
                    break;
                default:
                    Logger.Error("Found a bug!");
                    throw new LPGException("Unknown print option!");
            }
            foreach (var household in items) {
                Logger.Info("[" + i + "] " + household.Name);
                i++;
            }
        }

        private static void PrintLoadTypePriorities([NotNull] string connectionString) {
            Logger.Info("Loading...");
            var sim = new Simulator(connectionString);
            Logger.Info("Loading finished.");
            Logger.Info(
                "Each level includes all load types of the levels below, so level 2 includes all of the loadtypes of level 0 and 1.");
            foreach (LoadTypePriority value in Enum.GetValues(typeof(LoadTypePriority))) {
                if (!LoadTypePriorityHelper.LoadTypePriorityDictionaryAll.ContainsKey(value)) {
                    continue;
                }
                var odf =  value;
                var s = "[" + (int) value + "] " + LoadTypePriorityHelper.LoadTypePriorityDictionaryAll[odf] +
                        Environment.NewLine+"\t(";
                var lts = sim.LoadTypes.It.Where(x => x.Priority == odf);
                foreach (var vLoadType in lts) {
                    s += vLoadType.Name + ", ";
                }
                s = s.Substring(0, s.Length - 2) + ")";
                Logger.Info(s);
            }
        }

        public static void RunProcessing([NotNull] ListOptions lo, [NotNull] string connectionString) {
            if (lo.ModularHouseholds) {
                PrintList(connectionString, CalcObjectType.ModularHousehold);
                return;
            }
            if (lo.TemperatureProfiles) {
                PrintList(connectionString, "TemperatureProfiles");
                return;
            }
            if (lo.GeographicLocations) {
                PrintList(connectionString, "GeographicLocations");
                return;
            }

            if (lo.Houses) {
                PrintList(connectionString, CalcObjectType.House);
                return;
            }

            if (lo.Settlements) {
                PrintList(connectionString, CalcObjectType.Settlement);
                return;
            }

            if (lo.LoadtypePriorities) {
                PrintLoadTypePriorities(connectionString);
                return;
            }
            Logger.Error("No option was specified.");
        }
    }
}