using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Common.JSON;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using SimulationEngineLib.Calculation;

namespace SimulationEngineLib.SettlementCalculation {
    public static class BatchfileFromSettlement {
        [NotNull]
        private static string GetAllOptions([NotNull] Simulator sim, [NotNull] Settlement settlement, [NotNull] BatchOptions bo) {
            var options = " -SkipExisting -MeasureCalculationTimes ";
            options += " -OutputFileDefault " + bo.OutputFileDefault;
            if (bo.OutputFileDefault == OutputFileDefault.OnlyOverallSum) {
                options += " -DeleteDAT ";
            }
            if (settlement.TemperatureProfile != null) {
                var temperatureIndex = sim.TemperatureProfiles.It.IndexOf(settlement.TemperatureProfile);
                options += " -TemperatureProfileIndex " + temperatureIndex;
            }
            if (settlement.GeographicLocation != null) {
                if (bo.GeographicLocationIndex == null) {
                    var locindex = sim.GeographicLocations.It.IndexOf(settlement.GeographicLocation);
                    options += " -GeographicLocationIndex " + locindex;
                }
                else {
                    options += " -GeographicLocationIndex " + bo.GeographicLocationIndex;
                }
            }
            if (string.IsNullOrEmpty(bo.StartDate)) {

                if (settlement.StartDate != null) {
                    options += " -StartDate " + settlement.StartDate.Value.ToShortDateString();
                }
            }
            else {
                options += " -StartDate " + bo.StartDate;
            }
            if (string.IsNullOrEmpty(bo.EndDate)) {
                if (settlement.EndDate != null) {
                    options += " -EndDate " + settlement.EndDate.Value.ToShortDateString();
                }
            }
            else {
                options += " -EndDate " + bo.EndDate;
            }
            options += " -EnergyIntensityType " + bo.EnergyIntensity;

            if (!string.IsNullOrWhiteSpace(bo.ExternalTimeResolution)) {
                options += " -ExternalTimeResolution " + bo.ExternalTimeResolution + " ";
            }
            if (!string.IsNullOrWhiteSpace(bo.DeviceSelectionName)) {
                if (bo.DeviceSelectionName.Contains(" ") &&
                    !bo.DeviceSelectionName.StartsWith("\"", StringComparison.Ordinal)) {
                    bo.DeviceSelectionName += "\"" + bo.DeviceSelectionName + "\"";
                }

                options += "-DeviceSelectionName " + bo.DeviceSelectionName;
            }
            return options;
        }

        [NotNull]
        private static string GetCalcObjectName([NotNull] string name) {
            var cleanname = AutomationUtili.CleanFileName(name);
            cleanname = cleanname.Replace(" ", string.Empty).Replace("+", string.Empty).Replace(",", "_");
            if (cleanname.Length > 20) {
                cleanname = cleanname.Substring(0, 20);
            }
            return cleanname;
        }

        public static void MakeBatchfileFromSettlement([NotNull] string connectionString, [NotNull] BatchOptions bo) {
            Logger.Info("Loading...");
            var sim = new Simulator(connectionString);
            Logger.Info("Loading finished.");
            MakeBatchfileFromSettlement(sim, bo);
        }

        public static void MakeSettlementJson([NotNull] Settlement settlement, [NotNull] Simulator sim, [NotNull] BatchOptions bo, [NotNull] string dstPath)
        {
            SettlementInformation si = new SettlementInformation(
                sim.MyGeneralConfig.CSVCharacter, settlement.Name,
                bo.EnergyIntensity,
                Assembly.GetExecutingAssembly().GetName().Version.ToString()
            );

            foreach (SettlementHH settlementHousehold in settlement.Households) {
                if (settlementHousehold.CalcObject == null) {
                    continue;
                }

                if (settlementHousehold.CalcObjectType == CalcObjectType.House) {
                    House house = (House) settlementHousehold.CalcObject;
                    if(house.HouseType == null) {
                        throw new LPGException("Housetype was null");
                    }
                    SettlementInformation.HouseInformation hi
                        = new SettlementInformation.HouseInformation(
                            settlementHousehold.Name,
                            GetCalcObjectName(settlementHousehold.CalcObject.Name),
                            house.HouseType.Name);
                    si.HouseInformations.Add(hi);

                    foreach (var entry in house.Households) {
                        if (entry.CalcObject == null) {
                            throw new LPGException("Calcobject was null");
                        }
                        SettlementInformation.HouseholdInformation hhi = new SettlementInformation.HouseholdInformation(entry.CalcObject.Name);
                        hi.HouseholdInformations.Add(hhi);
                        ModularHousehold mhh = (ModularHousehold) entry.CalcObject;

                        foreach (ModularHouseholdTag tag in mhh.ModularHouseholdTags) {
                            hhi.Tags.Add(tag.Tag.Name);
                        }
                        foreach (ModularHouseholdPerson mhhperson in mhh.Persons) {
                            Person person = mhhperson.Person;
                            string traittag = mhhperson.TraitTag?.Name;
                            if (traittag == null) {
                                traittag = "";
                            }
                            SettlementInformation.SettlementPersonInformation spi =
                                new SettlementInformation.SettlementPersonInformation(person.Name,person.Age,person.Gender,traittag);

                            hhi.Persons.Add(spi);
                        }
                    }
                }
            }
            si.WriteResultEntries(dstPath);
        }

        public static void MakeBatchfileFromSettlement([NotNull] Simulator sim, [NotNull] BatchOptions bo) {
            Settlement settlement;
            if (bo.SettlementIndex != null) {
                settlement = sim.Settlements[bo.SettlementIndex.Value];
            }
            else if (!string.IsNullOrWhiteSpace(bo.SettlementName)) {
                settlement = sim.Settlements.FindFirstByName(bo.SettlementName);
                if (settlement == null) {
                    throw new LPGException("Could not find the settlement!");
                }
            }
            else {
                throw new LPGException(
                    "Neither settlement index or settlement name was set. Please set at least one of the two.");
            }
            Logger.Info("Selected " + settlement.Name);
            var cleanSettlementName = AutomationUtili.CleanFileName(settlement.Name);
            cleanSettlementName = cleanSettlementName.Replace(" ", string.Empty);
            if (cleanSettlementName.Length > 20) {
                cleanSettlementName = cleanSettlementName.Substring(0, 20);
            }
            var filename = "Start-" + cleanSettlementName + "." + bo.Suffix + ".cmd";
            using (var sw = new StreamWriter(filename)) {
                var options = GetAllOptions(sim, settlement, bo);
                const string start = "Simulationengine.exe Calculate ";
                var idx = 1;
                var totalentries = settlement.HouseholdCount;
                var formatstring = "D1";
                if (totalentries > 9) {
                    formatstring = "D2";
                }
                if (totalentries > 99) {
                    formatstring = "D3";
                }
                if (totalentries > 999) {
                    formatstring = "D4";
                }
                foreach (var settlementHH in settlement.Households) {
                    for (var i = 0; i < settlementHH.Count; i++) {
                        var outputDir = idx.ToString(formatstring, CultureInfo.CurrentCulture) + "_" +
                                        GetCalcObjectName(settlementHH.Name);
                        var calcobject = string.Empty;
                        if (settlementHH.CalcObjectType == CalcObjectType.ModularHousehold) {
                            var hhindex = sim.ModularHouseholds.It.IndexOf((ModularHousehold) settlementHH.CalcObject);
                            calcobject = "-CalcObjectType ModularHousehold  -CalcObjectNumber " + hhindex +
                                         " -OutputDirectory " + outputDir;
                        }
                        if (settlementHH.CalcObjectType == CalcObjectType.House) {
                            var hhindex = sim.Houses.It.IndexOf((House) settlementHH.CalcObject);
                            calcobject = "-CalcObjectType House  -CalcObjectNumber " + hhindex + " -OutputDirectory " +
                                         outputDir + " -LoadtypePriority RecommendedForHouses ";
                        }
                        var cmdline = start + " " + calcobject + " " + options;
                        while (cmdline.Contains("  ")) {
                            cmdline = cmdline.Replace("  ", " ");
                        }
                        sw.WriteLine(cmdline);
                        idx++;
                    }
                }
                sw.Close();
            }

            var settlementIndex = sim.Settlements.It.IndexOf(settlement);
             MakeSettlementJson(settlement,sim,bo,new DirectoryInfo(".").FullName);

            Logger.Info("Finished writing to " + filename);
            var parfilename = "Start-" + cleanSettlementName + "." + bo.Suffix + "Parallel.cmd";
            using (var sw = new StreamWriter(parfilename)) {
                if (string.IsNullOrWhiteSpace(bo.ParallelCores)) {
                    bo.ParallelCores = Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture);
                }
                sw.WriteLine("Simulationengine.exe LaunchParallel -NumberOfCores " + bo.ParallelCores + " -Batchfile " + filename +
                             " -ArchiveDirectory f:\\"+ cleanSettlementName + "_archive");
                sw.WriteLine("Simulationengine.exe ProcessSettlement -Directory . -SettlementIndex " + settlementIndex +
                             " -CheckSettlement ");
                sw.WriteLine("Simulationengine.exe ProcessSettlement -Directory . -AverageSumForDirectory ");
                sw.WriteLine("Simulationengine.exe ProcessSettlement -Directory . -CondenseActionEntries ");
                sw.WriteLine("Simulationengine.exe ProcessSettlement -Directory . -CondenseDeviceSums ");

                //this one is not very useful and takes forever
                sw.WriteLine("# Simulationengine.exe ProcessSettlement -Directory . -MergeActionEntries ");
                //this one is not very useful and takes forever
                sw.WriteLine("# Simulationengine.exe ProcessSettlement -Directory . -MergeAffordanceEnergyUse ");
                sw.WriteLine("# Simulationengine.exe ProcessSettlement -Directory . -MergeAffordanceTagging ");

                sw.WriteLine("# Simulationengine.exe ProcessSettlement -Directory . -MergeDeviceSumProfiles ");
                sw.WriteLine("Simulationengine.exe ProcessSettlement -Directory . -MergeDeviceSums ");
                sw.WriteLine("Simulationengine.exe ProcessSettlement -Directory . -MergeDeviceTagging ");
                sw.WriteLine("Simulationengine.exe ProcessSettlement -Directory . -MergeMulitpleWeekdayProfiles ");
                sw.WriteLine("Simulationengine.exe ProcessSettlement -Directory . -CalcCollectResults ");
            }
        }
    }
}