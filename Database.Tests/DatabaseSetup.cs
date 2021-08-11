//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region jenkins

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;

#endregion

namespace Database.Tests
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class DatabaseSetup: IDisposable
    {
        //public enum TestPackage
        //{
        //    DatabaseIo,
        //    LongTermMerger,
        //    CalcController,
        //    Calculation,
        //    ChartCreator,
        //    FullCalculation,
        //    LoadProfileGenerator,
        //    ReleaseBuilder,
        //    SimulationEngine,
        //    WebService,
        //    CalcPostProcessorTests,
        //}

        //[JetBrains.Annotations.NotNull]
        //private static readonly Dictionary<TestPackage, string> _packageNames =
        //    new Dictionary<TestPackage, string> {
        //        {TestPackage.DatabaseIo, "DatabaseIO.Tests"},
        //        {TestPackage.LongTermMerger, "IntegrationTests - LongTermMerge"},
        //        {TestPackage.CalcController, "CalcController.Tests"},
        //        {TestPackage.Calculation, "Calculation.Tests"},
        //        {TestPackage.ChartCreator, "ChartCreator.Tests"},
        //        {TestPackage.FullCalculation, "FullCalculation.Tests"},
        //        {TestPackage.LoadProfileGenerator, "LoadProfileGenerator.Tests"},
        //        {TestPackage.ReleaseBuilder, "ReleaseBuilder.Tests"},
        //        {TestPackage.SimulationEngine, "SimulationEngine.Tests"},
        //        {TestPackage.WebService, "WebService.Tests"},
        //        {TestPackage.CalcPostProcessorTests, "CalcPostProcessorTests"}
        //    };

        /// <summary>
        /// Warning:using a full path as source file name will use that exact file, not make a copy.
        /// </summary>
        /// <param name="testname"></param>
        /// <param name="sourceFileName"></param>
        public DatabaseSetup([JetBrains.Annotations.NotNull] string testname,
                             [CanBeNull] string sourceFileName = null)
        {
            DBBase.NeedsUpdateAllowed = true;
            var guid = string.Format(CultureInfo.InvariantCulture, "{0}", Guid.NewGuid());
            /*
            var myDrives = DriveInfo.GetDrives();
            var path = Path.Combine(Directory.GetCurrentDirectory());
            foreach (var drive in myDrives)
            {
                try {
                    if (drive.DriveType != DriveType.Network && drive.IsReady &&  drive.VolumeLabel == "RamDisk") {
                        path = drive.RootDirectory.FullName;
                    }
                }
                catch (Exception) {
                    // ignored because volumelabel can throw
                }
            }*/
            string path = WorkingDir.DetermineBaseWorkingDir(true);

            FileName = Path.Combine(path, "profile.UnitTests." + testname + "." + guid + ".db3");
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }

            SourcePath = GetSourcepath(sourceFileName);
            File.Copy(SourcePath, FileName);
            Logger.Info("Working with " + FileName + " copied from " + SourcePath);
            Config.IsInUnitTesting = true;
            ConnectionString = "Data Source=" + FileName;
            //DatabaseVersionChecker.CheckVersion(ConnectionString);
        }

        public string SourcePath { get; set; }

        [JetBrains.Annotations.NotNull]
        public string ConnectionString { get; }

        [JetBrains.Annotations.NotNull]
        public string FileName { get; }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Cleanup()
        {
            GC.WaitForPendingFinalizers();
            GC.Collect();
            if (File.Exists(FileName))
            {
                var success = false;
                var count = 0;
                while (!success && count < 20)
                {
                    try
                    {
                        File.Delete(FileName);
                        success = true;
                        if (count > 0)
                        {
                            Logger.Info("Deleting successful of " + FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception(ex);
                        Thread.Sleep(500);
                    }

                    count++;
                }

                if (!success)
                {
                    Logger.Info("Trying to delete again...");
                    Thread.Sleep(250);
                    File.Delete(FileName);
                    Logger.Info("Deleting successful.");
                }
            }

            //Logger.Info("finished cleaning.");
        }

        public void ClearTable([JetBrains.Annotations.NotNull] string name)
        {
            using (var con = new Connection(ConnectionString))
            {
                con.Open();
                using (var cmd = new Command(con))
                {
                    cmd.ExecuteNonQuery("DELETE FROM " + name);
                }
            }
        }

        [JetBrains.Annotations.NotNull]
        public static string GetImportFileFullPath([JetBrains.Annotations.NotNull] string srcfilename)
        {
            const string localPath = @"V:\Dropbox\LPG\ImportFiles\";

            FileInfo fi1 = new FileInfo(Path.Combine(localPath, srcfilename));
            if (fi1.Exists)
            {
                return fi1.FullName;
            }

            const string teamcityrelativePath = @"..\..\..\Importfiles\";

            FileInfo fi = new FileInfo(Path.Combine(teamcityrelativePath, srcfilename));
            if (fi.Exists)
            {
                return fi.FullName;
            }

            Logger.Info("file not found: " + fi.FullName + ", trying jenkins path next");
           /* const string jenkinsabsolutePath = @"c:\jenkins\workspace";
            fi = new FileInfo(Path.Combine(jenkinsabsolutePath, _packageNames[mypackage], "ImportFiles", srcfilename));

            if (fi.Exists)
            {
                return fi.FullName;
            }*/

            Logger.Info("file not found: " + fi.FullName + ", trying jenkins path next");
            throw new LPGException("Missing file: " + fi.FullName + "\n Current Directory:" +
                                   Directory.GetCurrentDirectory());
        }

        [JetBrains.Annotations.NotNull]
        public static string GetSourcepath([CanBeNull] string pSourcefilename)
        {
            Logger.Info("Looking for DB3 file");
            //figure out the filename
            var sourcefilename = "profilegenerator-latest.db3";
            if (pSourcefilename != null)
            {
                sourcefilename = pSourcefilename;
            }

            //find the right path
            FileInfo foundfile = FindDB3SourcePath(sourcefilename);
            if (foundfile == null)
            {
                throw new LPGException("Could not find the db3 database. Current directory is " +
                                       Directory.GetCurrentDirectory() + "\n");
            }

            if (foundfile.Length < 1000)
            {
                throw new LPGException("DB3 file is smaller than 1000 bytes");
            }

            Logger.Info("Full Source Path: " + foundfile.FullName);
            Logger.Info("Date modified:" + foundfile.LastWriteTime + ", Size:" + foundfile.Length);
            return foundfile.FullName;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Affordance> LoadAffordances(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeBasedProfile> timeBasedProfiles,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<SubAffordance> subAffordances,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceCategory> deviceCategories,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> realDevices,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Desire> desires,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeLimit> timeLimits,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Location> locations,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Variable> variables)
        {
            var affordances = new ObservableCollection<Affordance>();
            subAffordances = new ObservableCollection<SubAffordance>();
            SubAffordance.LoadFromDatabase(subAffordances, ConnectionString, desires, false, locations, variables);

            Affordance.LoadFromDatabase(affordances, ConnectionString, timeBasedProfiles, deviceCategories, realDevices,
                desires, subAffordances, loadTypes, timeLimits, deviceActions, deviceActionGroups, locations, false,
                variables);
            return affordances;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Affordance> LoadAffordances(
            [JetBrains.Annotations.NotNull] [ItemNotNull] out ObservableCollection<TimeBasedProfile> timeBasedProfiles,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<SubAffordance> subAffordances,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<DeviceCategory> deviceCategories,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<RealDevice> realDevices,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<Desire> desires,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<TimeLimit> timeLimits,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<DeviceAction> deviceActions,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<DeviceActionGroup> deviceActionGroups,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<Location> locations,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<Variable> variables,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<DateBasedProfile> dateBasedProfiles)
        {
            var affordances = new ObservableCollection<Affordance>();
            subAffordances = new ObservableCollection<SubAffordance>();
            desires = LoadDesires();
            variables = LoadVariables();
            timeBasedProfiles = LoadTimeBasedProfiles();
            realDevices = LoadRealDevices(out deviceCategories, out _, out loadTypes, timeBasedProfiles);
            locations = LoadLocations(realDevices, deviceCategories, loadTypes);
            SubAffordance.LoadFromDatabase(subAffordances, ConnectionString, desires, false, locations, variables);
            dateBasedProfiles = LoadDateBasedProfiles();
            timeLimits = LoadTimeLimits(dateBasedProfiles);
            deviceActionGroups = LoadDeviceActionGroups();
            deviceActions = LoadDeviceActions(timeBasedProfiles, realDevices, loadTypes, deviceActionGroups);
            Affordance.LoadFromDatabase(affordances, ConnectionString, timeBasedProfiles, deviceCategories, realDevices,
                desires, subAffordances, loadTypes, timeLimits, deviceActions, deviceActionGroups, locations, false,
                variables);
            return affordances;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<AffordanceTaggingSet> LoadAffordanceTaggingSets(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Affordance> affordances,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes)
        {
            var affordanceTaggingSets =
                new ObservableCollection<AffordanceTaggingSet>();
            AffordanceTaggingSet.LoadFromDatabase(affordanceTaggingSets, ConnectionString, false, affordances,
                loadTypes);
            return affordanceTaggingSets;
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<DateBasedProfile> LoadDateBasedProfiles()
        {
            var dateBasedProfiles = new ObservableCollection<DateBasedProfile>();
            DateBasedProfile.LoadFromDatabase(dateBasedProfiles, ConnectionString, false);
            return dateBasedProfiles;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Desire> LoadDesires()
        {
            var desires = new ObservableCollection<Desire>();
            Desire.LoadFromDatabase(desires, ConnectionString, false);
            return desires;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<DeviceActionGroup> LoadDeviceActionGroups()
        {
            var dags = new ObservableCollection<DeviceActionGroup>();
            DeviceActionGroup.LoadFromDatabase(dags, ConnectionString, false);
            return dags;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<DeviceAction> LoadDeviceActions(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeBasedProfile> timeProfiles,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> realDevices,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceActionGroup> groups)
        {
            var deviceActions = new ObservableCollection<DeviceAction>();
            DeviceAction.LoadFromDatabase(deviceActions, ConnectionString, timeProfiles, realDevices, loadTypes, groups,
                false);
            return deviceActions;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<DeviceCategory> LoadDeviceCategories(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> realDevices,
           [JetBrains.Annotations.NotNull]out DeviceCategory dcNone, bool ignoreMissingTables)
        {
            var deviceCategories = new ObservableCollection<DeviceCategory>();
            DeviceCategory.LoadFromDatabase(deviceCategories, out var dcNone1, ConnectionString, realDevices,
                ignoreMissingTables);
            dcNone = dcNone1;
            return deviceCategories;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<DeviceSelection> LoadDeviceSelections(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceCategory> deviceCategories,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> devices,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceActionGroup> groups)
        {
            var result = new ObservableCollection<DeviceSelection>();
            DeviceSelection.LoadFromDatabase(result, ConnectionString, deviceCategories, devices, deviceActions, groups,
                false);
            return result;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<EnergyStorage> LoadEnergyStorages(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes, ObservableCollection<Variable> variables)
        {
            var energyStorages = new ObservableCollection<EnergyStorage>();
            EnergyStorage.LoadFromDatabase(energyStorages, ConnectionString, loadTypes,variables, false);
            return energyStorages;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Generator> LoadGenerators(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DateBasedProfile> profiles)
        {
            var generators = new ObservableCollection<Generator>();
            Generator.LoadFromDatabase(generators, ConnectionString, loadTypes, profiles, false);
            return generators;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<GeographicLocation> LoadGeographicLocations(
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<Holiday> holidays,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeLimit> timeLimits)
        {
            var geographicLocations =
                new ObservableCollection<GeographicLocation>();
            holidays = LoadHolidays();
            GeographicLocation.LoadFromDatabase(geographicLocations, ConnectionString, holidays, timeLimits, false);
            return geographicLocations;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Holiday> LoadHolidays()
        {
            var holidays = new ObservableCollection<Holiday>();
            Holiday.LoadFromDatabase(holidays, ConnectionString, false);
            return holidays;
        }

        public void LoadHouseholdsAndHouses(
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<ModularHousehold> modularHouseholds,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<House> houses,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<TimeLimit> timeLimits,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TraitTag> traitTags,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<ChargingStationSet> chargingStationSets,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TravelRouteSet> travelRouteSets,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TransportationDeviceSet> transportationDeviceSets)
        {
            var timeBasedProfiles = LoadTimeBasedProfiles();
            var realDevices =
                LoadRealDevices(out ObservableCollection<DeviceCategory> deviceCategories, out _,
                    out ObservableCollection<VLoadType> loadTypes, timeBasedProfiles);
            var locations = LoadLocations(realDevices, deviceCategories,
                loadTypes);

            var desires = LoadDesires();
            var persons = LoadPersons();
            var dateBasedProfiles = LoadDateBasedProfiles();
            var deviceActionGroups = LoadDeviceActionGroups();
            var deviceActions =
                LoadDeviceActions(timeBasedProfiles, realDevices, loadTypes, deviceActionGroups);
            timeLimits = LoadTimeLimits(dateBasedProfiles);
            var variables = LoadVariables();
            var affordances = LoadAffordances(timeBasedProfiles, out _,
                deviceCategories
                , realDevices, desires, loadTypes, timeLimits, deviceActions, deviceActionGroups, locations, variables);

            var tags = LoadTraitTags();
            var lptags = LoadLivingPatternTags();
            var traits = LoadHouseholdTraits(locations, affordances, realDevices,
                deviceCategories, timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, deviceActionGroups,
                tags, variables,lptags);
            var deviceSelections = LoadDeviceSelections(deviceCategories, realDevices,
                deviceActions, deviceActionGroups);
            var vacations = LoadVacations();
            var hhtags = LoadHouseholdTags();
            modularHouseholds = LoadModularHouseholds(traits, deviceSelections, persons, vacations, hhtags, traitTags, lptags);
            var temperaturProfiles = LoadTemperatureProfiles();

            var geographicLocations = LoadGeographicLocations(out _,
                timeLimits);
            var energyStorages = LoadEnergyStorages(loadTypes,variables);
            var transformationDevices = LoadTransformationDevices(loadTypes,
                variables);

            var generators = LoadGenerators(loadTypes, dateBasedProfiles);
            var houseTypes = LoadHouseTypes(realDevices, deviceCategories, timeBasedProfiles,
                timeLimits, loadTypes, transformationDevices, energyStorages, generators, locations, deviceActions,
                deviceActionGroups, variables);

            houses = LoadHouses(modularHouseholds, temperaturProfiles,
                geographicLocations, houseTypes,chargingStationSets,
                travelRouteSets,transportationDeviceSets);
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TravelRouteSet> LoadTravelRouteSets([JetBrains.Annotations.NotNull][ItemNotNull]ObservableCollection<Location> locations,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TransportationDeviceCategory> categories,[JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<Site> sites,
            ObservableCollection<AffordanceTaggingSet> affordanceTaggingSets)
        {
            var travelRouteSets = new ObservableCollection<TravelRouteSet>();

            var travelRoutes = LoadTravelRoutes(categories, locations,
                out var sites1);
            sites = sites1;
            TravelRouteSet.LoadFromDatabase(travelRouteSets, ConnectionString, false, travelRoutes, affordanceTaggingSets);
            return travelRouteSets;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TravelRoute> LoadTravelRoutes(
            [JetBrains.Annotations.NotNull][ItemNotNull]ObservableCollection<TransportationDeviceCategory> transportationDeviceCategories,
            [JetBrains.Annotations.NotNull][ItemNotNull]ObservableCollection<Location> locations,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<Site> sites)

        {
            var tt = new ObservableCollection<TravelRoute>();
            sites = LoadSites(locations);
            TravelRoute.LoadFromDatabase(tt, ConnectionString, false, transportationDeviceCategories, sites);
            return tt;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Site> LoadSites([JetBrains.Annotations.NotNull][ItemNotNull]ObservableCollection<Location> locations)
        {
            var tt = new ObservableCollection<Site>();
            Site.LoadFromDatabase(tt, ConnectionString,
                false, locations);
            return tt;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TransportationDeviceSet> LoadTransportationDeviceSets(
            [ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadtypes,
            [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<TransportationDeviceCategory> devicesCategories,
            [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<TransportationDevice> transportationDevices)
        {
            var tranportationdevicesets = new ObservableCollection<TransportationDeviceSet>();
            devicesCategories = LoadTransportationDeviceCategory();
            transportationDevices = LoadTransportationDevices(devicesCategories, loadtypes);
            TransportationDeviceSet.LoadFromDatabase(tranportationdevicesets, ConnectionString, false, transportationDevices);
            return tranportationdevicesets;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TransportationDevice> LoadTransportationDevices(
            [ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<TransportationDeviceCategory> categories,
            [ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadtypes)
        {
            var tt = new ObservableCollection<TransportationDevice>();
            TransportationDevice.LoadFromDatabase(tt, ConnectionString, false, categories, loadtypes);
            return tt;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TransportationDeviceCategory> LoadTransportationDeviceCategory()
        {
            var tt = new ObservableCollection<TransportationDeviceCategory>();
            TransportationDeviceCategory.LoadFromDatabase(tt, ConnectionString, false);
            return tt;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<HouseholdTag> LoadHouseholdTags()
        {
            var tt = new ObservableCollection<HouseholdTag>();
            HouseholdTag.LoadFromDatabase(tt, ConnectionString, false);
            return tt;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<HouseholdTemplate> LoadHouseholdTemplates(
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<RealDevice> realDevices,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<DeviceCategory> deviceCategories,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<TimeBasedProfile> timeBasedProfiles,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<TimeLimit> timeLimits,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<DeviceAction> deviceActions,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<DeviceActionGroup> deviceActionGroups,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<HouseholdTrait> traits
        )
        {
            var householdTemplates = new ObservableCollection<HouseholdTemplate>();
            timeBasedProfiles = LoadTimeBasedProfiles();
            realDevices = LoadRealDevices(out deviceCategories, out _, out loadTypes, timeBasedProfiles);
            var locations = LoadLocations(realDevices, deviceCategories, loadTypes);
            var desires = LoadDesires();
            var persons = LoadPersons();
            var dateBasedProfiles = LoadDateBasedProfiles();
            timeLimits = LoadTimeLimits(dateBasedProfiles);
            deviceActionGroups = LoadDeviceActionGroups();
            deviceActions =
                LoadDeviceActions(timeBasedProfiles, realDevices, loadTypes, deviceActionGroups);

            var variables = LoadVariables();
            var affordances = LoadAffordances(timeBasedProfiles, out _,
                deviceCategories,
                realDevices, desires, loadTypes, timeLimits, deviceActions, deviceActionGroups, locations, variables);

            var tags = LoadTraitTags();
            var lptags = LoadLivingPatternTags();
            traits = LoadHouseholdTraits(locations, affordances, realDevices, deviceCategories, timeBasedProfiles,
                loadTypes, timeLimits, desires, deviceActions, deviceActionGroups, tags, variables, lptags);
            var vacations = LoadVacations();
            var templateTags = LoadHouseholdTags();
            HouseholdTemplate.LoadFromDatabase(householdTemplates, ConnectionString, traits, false, persons, tags,
                vacations, templateTags, dateBasedProfiles, lptags);
            return householdTemplates;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<HouseholdTrait> LoadHouseholdTraits(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Location> locations,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Affordance> affordances,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> devices,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceCategory> deviceCategories,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeBasedProfile> timeBasedProfiles,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeLimit> timeLimits,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Desire> desires,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceActionGroup> groups,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TraitTag> traitTags,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Variable> variables,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<LivingPatternTag> livingPatternTags
           )
        {
            var householdTraits = new ObservableCollection<HouseholdTrait>();

            HouseholdTrait.LoadFromDatabase(householdTraits, ConnectionString, locations, affordances, devices,
                deviceCategories, timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, groups, traitTags,
                livingPatternTags,false, variables);
            return householdTraits;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<House> LoadHouses(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<ModularHousehold> modularHouseholds,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TemperatureProfile> temperaturProfiles,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<GeographicLocation> geographicLocations,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<HouseType> houseTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<ChargingStationSet> chargingStationSets,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TravelRouteSet> travelRouteSets,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TransportationDeviceSet> transportationDeviceSets)
        {
            var houses = new ObservableCollection<House>();
            House.LoadFromDatabase(houses, ConnectionString, temperaturProfiles, geographicLocations,
                houseTypes, modularHouseholds,
                chargingStationSets,transportationDeviceSets, travelRouteSets,false);
            return houses;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<HouseType> LoadHouseTypes(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> realDevices,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceCategory> deviceCategories,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeBasedProfile> timeBasedProfiles,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeLimit> timeLimits,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TransformationDevice> transformationDevices,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<EnergyStorage> energyStorages,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Generator> generators,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Location> allLocations,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Variable> variables)
        {
            var houseTypes = new ObservableCollection<HouseType>();
            HouseType.LoadFromDatabase(houseTypes, ConnectionString, realDevices, deviceCategories, timeBasedProfiles,
                timeLimits, loadTypes, transformationDevices, energyStorages, generators, false, allLocations,
                deviceActions, deviceActionGroups, variables);
            return houseTypes;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<VLoadType> LoadLoadTypes()
        {
            var vLoadTypes = new ObservableCollection<VLoadType>();
            VLoadType.LoadFromDatabase(vLoadTypes, ConnectionString, false);
            return vLoadTypes;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Location> LoadLocations(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> realDevices,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceCategory> deviceCategories,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes)
        {
            var locations = new ObservableCollection<Location>();
            Location.LoadFromDatabase(locations, ConnectionString, realDevices, deviceCategories, loadTypes, false);
            return locations;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<ModularHousehold> LoadModularHouseholds(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<HouseholdTrait> householdTraits,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceSelection> deviceSelections,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Person> persons,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Vacation> vacations,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<HouseholdTag> tags,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TraitTag> traitTags,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<LivingPatternTag> livingPatternTags)
        {
            var modularHouseholds = new ObservableCollection<ModularHousehold>();
            ModularHousehold.LoadFromDatabase(modularHouseholds, ConnectionString, householdTraits, deviceSelections,
                false, persons, vacations, tags, traitTags, livingPatternTags);
            return modularHouseholds;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Person> LoadPersons()
        {
            var persons = new ObservableCollection<Person>();
            Person.LoadFromDatabase(persons, ConnectionString, false);
            return persons;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<RealDevice> LoadRealDevices(
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<DeviceCategory> deviceCategoriesOut,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeBasedProfile> profiles) =>
            LoadRealDevices(out deviceCategoriesOut, out _, out loadTypes, profiles);

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<RealDevice> LoadRealDevices(
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<DeviceCategory> deviceCategoriesOut,
           [JetBrains.Annotations.NotNull]out DeviceCategory none,
           [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TimeBasedProfile> profiles)
        {
            var realDevices = new ObservableCollection<RealDevice>();
            var deviceCategories = LoadDeviceCategories(realDevices, out DeviceCategory dcnone, false);
            loadTypes = LoadLoadTypes();
            RealDevice.LoadFromDatabase(realDevices, deviceCategories, dcnone, ConnectionString, loadTypes, profiles,
                false);
            deviceCategoriesOut = deviceCategories;
            none = dcnone;
            return realDevices;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TemperatureProfile> LoadTemperatureProfiles()
        {
            var temperaturProfiles = new ObservableCollection<TemperatureProfile>();
            TemperatureProfile.LoadFromDatabase(temperaturProfiles, ConnectionString, false);
            return temperaturProfiles;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TimeBasedProfile> LoadTimeBasedProfiles()
        {
            var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
            TimeBasedProfile.LoadFromDatabase(timeBasedProfiles, ConnectionString, false);
            return timeBasedProfiles;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TimeLimit> LoadTimeLimits(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles)
        {
            var timeLimits = new ObservableCollection<TimeLimit>();
            TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, ConnectionString, false);
            return timeLimits;
        }

        public void LoadTransportation([JetBrains.Annotations.NotNull][ItemNotNull]ObservableCollection<Location> locations,
            [JetBrains.Annotations.NotNull][ItemNotNull]  out ObservableCollection<TransportationDeviceSet> transportationDeviceSets,
            [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<TravelRouteSet> travelRouteSets,
            [ItemNotNull][JetBrains.Annotations.NotNull] out ObservableCollection<TransportationDevice> transportationDevices,
            [ItemNotNull][JetBrains.Annotations.NotNull] out ObservableCollection<TransportationDeviceCategory> transportationDeviceCategories,
            [ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadTypes,
            [JetBrains.Annotations.NotNull][ItemNotNull] out ObservableCollection<ChargingStationSet> chargingStationSets,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<AffordanceTaggingSet> affordanceTaggingSets)
        {
            transportationDeviceSets = LoadTransportationDeviceSets(
                loadTypes,
                out transportationDeviceCategories, out transportationDevices);
            travelRouteSets = LoadTravelRouteSets(locations, transportationDeviceCategories, out var sites, affordanceTaggingSets);
            chargingStationSets = LoadChargingStationSets(loadTypes,transportationDeviceCategories,sites);
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<ChargingStationSet> LoadChargingStationSets([JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<VLoadType> loadTypes,
                                                                               [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<TransportationDeviceCategory> transportationDeviceCategories,
                                                                               [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Site> sites
                                                                                )
        {
            var tags = new ObservableCollection<ChargingStationSet>();
            ChargingStationSet.LoadFromDatabase(tags, ConnectionString, false,loadTypes,transportationDeviceCategories,sites);
            return tags;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TraitTag> LoadTraitTags()
        {
            var tags = new ObservableCollection<TraitTag>();
            TraitTag.LoadFromDatabase(tags, ConnectionString, false);
            return tags;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<LivingPatternTag> LoadLivingPatternTags()
        {
            var tags = new ObservableCollection<LivingPatternTag>();
            LivingPatternTag.LoadFromDatabase(tags, ConnectionString, false);
            return tags;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<TransformationDevice> LoadTransformationDevices(
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<VLoadType> loadTypes,
           [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Variable> variables)
        {
            var transformationDevices =
                new ObservableCollection<TransformationDevice>();
            TransformationDevice.LoadFromDatabase(transformationDevices, ConnectionString, loadTypes, variables,
                false);
            return transformationDevices;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Vacation> LoadVacations()
        {
            var vacations = new ObservableCollection<Vacation>();
            Vacation.LoadFromDatabase(vacations, ConnectionString, false);
            return vacations;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<Variable> LoadVariables()
        {
            var variables = new ObservableCollection<Variable>();
            Variable.LoadFromDatabase(variables, ConnectionString, false);
            return variables;
        }

        [CanBeNull]
        private static FileInfo CheckAppDataLocal([JetBrains.Annotations.NotNull] string filename)
        {
            //used by the test runner
            if (Directory.GetCurrentDirectory().Contains(@"AppData\Local"))
            {
                Logger.Info("Currently working in Appdata-local, so using v:\\dropbox");
                DirectoryInfo di = new DirectoryInfo(@"v:\Dropbox\LPG\WpfApplication1");
                FileInfo fi = new FileInfo(Path.Combine(di.FullName, filename));
                if (fi.Exists)
                {
                    return fi;
                }
            }

            return null;
        }

        //[CanBeNull]
        //private static FileInfo CheckJenkins([JetBrains.Annotations.NotNull] string filename, TestPackage testPackage)
        //{
        //    const string jenkinsdir = "c:\\jenkins\\workspace";
        //    Logger.Info("trying jenkins: Checking for " + jenkinsdir);

        //    if (!Directory.Exists(jenkinsdir))
        //    {
        //        Logger.Info("it seems to not be jenkins: Not found: " + jenkinsdir);
        //        return null;
        //    }

        //    Logger.Info("it seems to be jenkins");
        //    if (!_packageNames.ContainsKey(testPackage)) {
        //        throw new LPGException("forgotten package name: " + testPackage);
        //    }
        //    string packageName = _packageNames[testPackage];
        //    FileInfo fileInfo = new FileInfo(Path.Combine(jenkinsdir, packageName, "WpfApplication1", filename));
        //    if (fileInfo.Exists)
        //    {
        //        return fileInfo;
        //    }
        //    Logger.Info("found jenkins, but not the file. Package wrong? Tried: " + fileInfo.FullName);

        //    string currentdir = Directory.GetCurrentDirectory();
        //    fileInfo = new FileInfo(Path.Combine(currentdir,  "WpfApplication1", filename));
        //    if (fileInfo.Exists)
        //    {
        //        Logger.Info("found jenkins, found db3 in the current dir: " + fileInfo.FullName);
        //        return fileInfo;
        //    }
        //    Logger.Info("found jenkins, but no db3 in the current dir: " + fileInfo.FullName);
        //    return null;
        //}

        [CanBeNull]
        private static FileInfo CheckTeamCityPath([JetBrains.Annotations.NotNull] string filename)
        {
            Logger.Info("trying team city");
            var tmpdir = Environment.GetEnvironmentVariable("TMPDIR");

            if (tmpdir == null)
            {
                Logger.Info("Tempdir was null. Seems to not be teamcity. \nCurrent directory is " +
                            Directory.GetCurrentDirectory());
                return null;
            }

            tmpdir = tmpdir.Replace(@"temp\buildTmp", "");
            Logger.Info("trying " + tmpdir);
            if (Directory.Exists(tmpdir))
            {
                Logger.Info("buildagent found");
                DirectoryInfo di = new DirectoryInfo(Path.Combine(tmpdir, "work"));
                Logger.Info("using path: " + di.FullName);
                var start = DateTime.Now;
                var fis = di.GetFiles(filename, SearchOption.AllDirectories).ToList();
                var end = DateTime.Now;
                Logger.Info("Searching for db3 took " + (end - start).TotalSeconds + " seconds");
                if (fis.Count == 0)
                {
                    throw new LPGException("error, profilegenerator-latest not found.");
                }

                fis.Sort((x, y) => y.LastWriteTime.CompareTo(x.LastWriteTime));
                return fis[0];
            }

            return null;
        }


        [CanBeNull]
        private static FileInfo CheckDropBoxPath([JetBrains.Annotations.NotNull] string filename)
        {
            // if started from the target directory
            DirectoryInfo di =
                new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), @"V:\Dropbox\LPG\WpfApplication1\"));
            Logger.Info("Trying " + di.FullName);
            if (di.Exists)
            {
                Logger.Info("found " + di.FullName);
                FileInfo fi = new FileInfo(Path.Combine(di.FullName, filename));
                if (fi.Exists)
                {
                    return fi;
                }
            }

            return null;
        }

        [CanBeNull]
        private static FileInfo CheckWpfApplicationPath([JetBrains.Annotations.NotNull] string filename)
        {
            // if started from the target directory
            DirectoryInfo di =
                new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\wpfapplication1"));
            Logger.Info("Trying " + di.FullName);
            if (!di.Exists)
            {
                // depending on the current directory we need to get one level higher
                di =
                   new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\wpfapplication1"));
                Logger.Info("Trying " + di.FullName);
            }
            if (di.Exists)
            {
                Logger.Info("found " + di.FullName);
                FileInfo fi = new FileInfo(Path.Combine(di.FullName, filename));
                if (fi.Exists)
                {
                    return fi;
                }
            }

            return null;
        }
        [JetBrains.Annotations.NotNull]
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                if(codeBase==null)
                {throw new LPGException("codebase was null");}
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path) ?? throw new LPGException("No path found for assembly");
            }
        }
        [CanBeNull]
        private static FileInfo FindDB3SourcePath([JetBrains.Annotations.NotNull] string filename)
        {
            var di = CheckDropBoxPath(filename);
            if (di != null)
            {
                return di;
            }
            var assemblyDir = AssemblyDirectory;
            Logger.Info("Assembly directory: " +  assemblyDir);

            if (filename.Contains(":")&&File.Exists(filename))
            {
                return new FileInfo(filename);
            }
            FileInfo fiCurr =new FileInfo(Path.Combine( assemblyDir, filename));
            if (fiCurr.Exists) {
                Logger.Info("Found the db in " + fiCurr.FullName);
                return fiCurr;
            }
            di = CheckAppDataLocal(filename);
            if (di != null)
            {
                return di;
            }

            di = CheckWpfApplicationPath(filename);
            if (di != null)
            {
                return di;
            }

            di = CheckTeamCityPath(filename);
            if (di != null)
            {
                return di;
            }

            //di = CheckJenkins(filename, testPackage);
            //if (di != null)
            //{
            //    return di;
            //}


            return null;
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}