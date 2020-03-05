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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using CalculationController.DtoFactories;
using CalculationController.Queue;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Common.Tests;
using Database;
using Database.Tests;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Calculation.Tests.OnlineDeviceActivation {
    [TestFixture]
    public class OnlineDeviceActivationProcessorTests : UnitTestBaseClass {
        private static bool OpenTabFunc([CanBeNull] object o)
        {
            Logger.Warning("OpenTabFunc was called");
            return true;
        }

        private static bool ReportCancelFunc() => throw new LPGException("Not implemented");

        private static bool ReportFinishFuncForHouseAndSettlement(bool b,
                                                                  [NotNull] string arg3,
                                                                  [NotNull] [ItemNotNull] ObservableCollection<ResultFileEntry> arg4)
        {
            Logger.Info(Utili.GetCurrentMethodAndClass());
            return true;
        }

#pragma warning disable S4144 // Methods should not have identical implementations
        private static bool ReportFinishFuncForHousehold(bool b, [NotNull] string arg3, [NotNull] string arg4)
        {
#pragma warning restore S4144 // Methods should not have identical implementations
            Logger.Info(Utili.GetCurrentMethodAndClass());
            return true;
        }

        private static bool SetCalculationEntries([NotNull] [ItemNotNull] ObservableCollection<CalculationEntry> observableCollection)
        {
            Logger.Info("SetCalculationEntries was called");
            return true;
        }

        /// <summary>
        ///     this tests tests if the autonomous energy consumption is set to 0 correctly if a device that is both autonomous and
        ///     user controlled is activated manually
        ///     For example if a TV with standby use is turned on, then it is not running in standby simultanously.
        /// </summary>
        [Test]
        [Category("BasicTest")]
        public void OnlineDeviceActivationProcessorSetToZeroTest()
        {
            var rnd = new Random(1);
            var nr = new NormalRandom(0, 1, rnd);
            var startdate = new DateTime(2018, 1, 1);
            var enddate = startdate.AddMinutes(100);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate)
                .SetEndDate(enddate).EnableShowSettlingPeriod();
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            calcParameters.Enable(CalcOption.ActionsLogfile);
            calcParameters.Enable(CalcOption.DeviceProfiles);
            Config.ExtraUnitTestChecking = true;
            // calcProfile
            var profileWith100 = new CalcProfile("calcProfile", Guid.NewGuid().ToString(), new TimeSpan(0, 1, 0), ProfileType.Absolute, "blub");
            profileWith100.AddNewTimepoint(new TimeSpan(0), 100);
            profileWith100.AddNewTimepoint(new TimeSpan(0, 5, 0), 100);
            profileWith100.ConvertToTimesteps();
            var profileWith50 = new CalcProfile("calcProfile2", Guid.NewGuid().ToString(), new TimeSpan(0, 1, 0), ProfileType.Absolute, "blub");
            profileWith50.AddNewTimepoint(new TimeSpan(0), 50);
            profileWith50.AddNewTimepoint(new TimeSpan(0, 3, 0), 50);
            profileWith50.ConvertToTimesteps();
            // Loadtype
            var clt = new CalcLoadType("lt1", "W", "kWh", 1, true, Guid.NewGuid().ToString());
            // Location
            var cloc = new CalcLocation("Location", Guid.NewGuid().ToString());
            // devices

            var cdl = new CalcDeviceLoad("lt1", 100, clt, 100, 0, Guid.NewGuid().ToString());
            var loads = new List<CalcDeviceLoad>();
            var results = new List<string> {
                "100;0;",
                "100;0;",
                "100;0;",
                "100;0;",
                "100;0;",
                "100;0;",
                "0;50;",
                "0;50;",
                "0;50;",
                "100;0;",
                "100;0;",
                "100;0;",
                "100;0;",
                "100;0;",
                "100;0;"
            };
            loads.Add(cdl);
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh1", wd.InputDataLogger);
            fft.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General, "desc", null, null);
            //SqlResultLoggingService srls = new SqlResultLoggingService(Path.Combine(wd.WorkingDirectory,"results.sqlite"));
            DateStampCreator dsc = new DateStampCreator(calcParameters);
            IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters);
            using (var lf = new LogFile(calcParameters, fft, old, wd.SqlResultLoggingService)) {
                var odap = new OnlineDeviceActivationProcessor(nr, lf, calcParameters);
                List<VariableRequirement> requirements = new List<VariableRequirement>();
                string devCatGuid = Guid.NewGuid().ToString();
                var autodev = new CalcAutoDev("devicename",
                    profileWith100,
                    clt,
                    loads,
                    0,
                    devCatGuid,
                    odap,
                    new HouseholdKey("HH1"),
                    1,
                    cloc,
                    "device category",
                    calcParameters,
                    Guid.NewGuid().ToString(),
                    requirements);
                var device = new CalcDevice("devicename",
                    loads,
                    devCatGuid,
                    odap,
                    cloc,
                    new HouseholdKey("HH1"),
                    OefcDeviceType.Device,
                    "category",
                    string.Empty,
                    calcParameters,
                    Guid.NewGuid().ToString());
                var autoDevs = new List<CalcAutoDev> {
                    autodev
                };
                var devices = new List<CalcDevice> {
                    device
                };
                CalcHousehold.MatchAutonomousDevicesWithNormalDevices(autoDevs, devices);
                if (device.MatchingAutoDevs.Count == 0) {
                    throw new LPGException("Matching devices didn't work");
                }

                foreach (var pair in odap.Oefc.ColumnEntriesByLoadTypeByDeviceKey) {
                    Console.WriteLine(pair.Key.Name);
                    foreach (KeyValuePair<OefcKey, ColumnEntry> entry in pair.Value) {
                        Console.WriteLine(entry.Key + " - " + entry.Value.Name);
                    }
                }

                for (var i = 0; i < 15; i++) {
                    TimeStep ts = new TimeStep(i, 0, true);
                    if (!autodev.IsBusyDuringTimespan(ts, 1, 1, clt)) {
                        autodev.Activate(ts, nr);
                    }

                    if (i == 6) {
                        device.SetTimeprofile(profileWith50, ts, clt, "blub", "Person", 1, false);
                    }

                    var filerows = odap.ProcessOneTimestep(ts);
                    Assert.AreEqual(1, filerows.Count);
                    Assert.AreEqual(2, filerows[0].EnergyEntries.Count);
                    var entries = string.Empty;

                    foreach (var d in filerows[0].EnergyEntries) {
                        entries += d.ToString(CultureInfo.CurrentCulture) + ";";
                    }

                    Logger.Info(entries);
                    Assert.AreEqual(results[i], entries);
                }
            }

            wd.CleanUp();
        }

        [Test]
        [Category("BasicTest")]
        public void OnlineDeviceActivationProcessorTest()
        {
            var rnd = new Random(1);
            var nr = new NormalRandom(0, 1, rnd);
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().EnableShowSettlingPeriod();
            calcParameters.Enable(CalcOption.ActionsLogfile);
            calcParameters.Enable(CalcOption.DeviceProfiles);
            calcParameters.Enable(CalcOption.DetailedDatFiles);
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh1", wd.InputDataLogger);
            //SqlResultLoggingService srls = new SqlResultLoggingService(Path.Combine(wd.WorkingDirectory, "results.sqlite"));
            DateStampCreator dsc = new DateStampCreator(calcParameters);

            IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters);
            using (var lf = new LogFile(calcParameters, fft, old, wd.SqlResultLoggingService)) {
                var hhkey = new HouseholdKey("HH1");
                lf.InitHousehold(Constants.GeneralHouseholdKey, "generalhousehold", HouseholdKeyType.General, "Description", null, null);
                lf.InitHousehold(hhkey, "hh1", HouseholdKeyType.Household, "Description", null, null);
                var odap = new OnlineDeviceActivationProcessor(nr, lf, calcParameters);
                const string deviceGuid = "devguid";
                const string locationGuid = "locationGuid";
                const string loadtypeGuid = "ltguid";
                var key = new OefcKey(hhkey, OefcDeviceType.Device, deviceGuid, locationGuid, loadtypeGuid, "myCategory");
                var clt = new CalcLoadType("lt1", "W", "kWh", 1, true, loadtypeGuid);
                odap.RegisterDevice("devicename", key, "home", clt.ConvertToDto());
                double[] stepValues = {1.0, 0};
                var valueList = new List<double>(stepValues);
                var cp = new CalcProfile("myCalcProfile", Guid.NewGuid().ToString(), valueList, ProfileType.Absolute, "synthetic");
                TimeStep ts1 = new TimeStep(1, 0, false);
                odap.AddNewStateMachine(cp, ts1, 0, 10, "devicename", clt.ConvertToDto(), "blub", "name1", "p1", "syn", key);
                double[] resultValues = {0, 10.0, 0, 0, 0, 0, 0, 0, 0, 0};

                for (var i = 0; i < 10; i++) {
                    TimeStep ts = new TimeStep(i, 0, true);
                    var filerows = odap.ProcessOneTimestep(ts);
                    Assert.AreEqual(1, filerows.Count);
                    Assert.AreEqual(1, filerows[0].EnergyEntries.Count);
                    Logger.Info(filerows[0].EnergyEntries[0].ToString(CultureInfo.CurrentCulture));
                    Assert.AreEqual(resultValues[i], filerows[0].EnergyEntries[0]);
                    foreach (var fileRow in filerows) {
                        fileRow.Save(odap.BinaryOutStreams[fileRow.LoadType]);
                    }
                }
            }

            Logger.Info(wd.WorkingDirectory);
            wd.CleanUp();
        }

        [Test]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
        [Category("BasicTest")]
        public void PostProcessingTestSingleActivation()
        {
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            var startdate = new DateTime(2018, 1, 1);
            var enddate = startdate.AddMinutes(1000);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate)
                .SetEndDate(enddate).EnableShowSettlingPeriod();

            var rnd = new Random(1);
            var nr = new NormalRandom(0, 1, rnd);
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            calcParameters.Enable(CalcOption.IndividualSumProfiles);
            calcParameters.Enable(CalcOption.DeviceProfiles);
            calcParameters.Enable(CalcOption.TotalsPerDevice);
            calcParameters.Enable(CalcOption.TotalsPerLoadtype);
            calcParameters.Enable(CalcOption.DetailedDatFiles);
            HouseholdKey key = new HouseholdKey("hh1");
            wd.InputDataLogger.AddSaver(new CalcParameterLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.Save(calcParameters);
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hhname", wd.InputDataLogger);
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new DeviceActivationEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcLoadTypeDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcPersonDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new DeviceTaggingSetLogger(wd.SqlResultLoggingService));
            //wd.InputDataLogger.AddSaver(new CalcDeviceDtoLogger(wd.SqlResultLoggingService));
            DateStampCreator dsc = new DateStampCreator(calcParameters);
            IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters);

            CalcPersonDto calcPersonDto = new CalcPersonDto("blub",
                "personguid",
                1,
                PermittedGender.Male,
                key,
                new List<DateSpan>(),
                new List<DateSpan>(),
                1,
                "traittag",
                "householdname");
            List<CalcPersonDto> persons = new List<CalcPersonDto> {
                calcPersonDto
            };
            wd.InputDataLogger.SaveList(persons.ConvertAll(x => (IHouseholdKey)x));

            using (var lf = new LogFile(calcParameters, fft, old, wd.SqlResultLoggingService)) {
                lf.InitHousehold(key, "test hh", HouseholdKeyType.Household, "Description", null, null);
                lf.InitHousehold(Constants.GeneralHouseholdKey, "General", HouseholdKeyType.General, "Description", null, null);
                var odap = new OnlineDeviceActivationProcessor(nr, lf, calcParameters);
                var clt = new CalcLoadType("lt1", "W", "kWh", 3, true, Guid.NewGuid().ToString());
                List<CalcLoadTypeDto> loadTypeDtos = new List<CalcLoadTypeDto> {
                    clt.ConvertToDto()
                };
                wd.InputDataLogger.Save(loadTypeDtos);
                //var loadTypes = new List<CalcLoadType> {clt};
                var cdl = new CalcDeviceLoad("devload1", 10, clt, 666, 0, Guid.NewGuid().ToString());
                var deviceLoads = new List<CalcDeviceLoad> {
                    cdl
                };
                var cloc = new CalcLocation("locname", Guid.NewGuid().ToString());
                string deviceCategoryGuid = Guid.NewGuid().ToString();
                CalcDeviceDto calcDeviceDto = new CalcDeviceDto("devicename",
                    deviceCategoryGuid,
                    key,
                    OefcDeviceType.Device,
                    "category",
                    string.Empty,
                    Guid.NewGuid().ToString(),
                    cloc.Guid,
                    cloc.Name);
                List<CalcDeviceDto> calcDeviceDtos = new List<CalcDeviceDto> {
                    calcDeviceDto
                };
                wd.InputDataLogger.SaveList(calcDeviceDtos.ConvertAll(x => (IHouseholdKey)x));
                //device tagging set for the post processing
                List<DeviceTaggingSetInformation> cdts = new List<DeviceTaggingSetInformation>();
                DeviceTaggingSetInformation dtsi = new DeviceTaggingSetInformation("myset");
                dtsi.AddTag(calcDeviceDto.Name, "testTag ");
                cdts.Add(dtsi);
                wd.InputDataLogger.Save(cdts);

                //device
                var device = new CalcDevice("devicename",
                    deviceLoads,
                    deviceCategoryGuid,
                    odap,
                    cloc,
                    key,
                    OefcDeviceType.Device,
                    "category",
                    string.Empty,
                    calcParameters,
                    Guid.NewGuid().ToString());
                //var devices = new List<CalcDevice> {device};
                double[] resultValues = {0, 100.0, 0, 0, 0, 0, 0, 0, 0, 0};
                var cp = new CalcProfile("profile1", Guid.NewGuid().ToString(), new TimeSpan(0, 1, 0), ProfileType.Absolute, "custom");
                cp.AddNewTimepoint(new TimeSpan(0), 0);
                cp.AddNewTimepoint(new TimeSpan(0, 1, 0), 10);
                cp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0);
                cp.ConvertToTimesteps();
                //var locations = new List<CalcLocation> {cloc};
                TimeStep ts1 = new TimeStep(0, calcParameters);
                device.SetTimeprofile(cp, ts1, clt, "affordanceName", "activatorName", 10, false);
                for (var i = 0; i < 10; i++) {
                    TimeStep ts = new TimeStep(i, calcParameters);
                    var filerows = odap.ProcessOneTimestep(ts);
                    Assert.AreEqual(1, filerows.Count);
                    Assert.AreEqual(1, filerows[0].EnergyEntries.Count);
                    Logger.Info(filerows[0].EnergyEntries[0].ToString(CultureInfo.CurrentCulture));
                    Assert.AreEqual(resultValues[i], filerows[0].EnergyEntries[0]);
                    foreach (var fileRow in filerows) {
                        fileRow.Save(odap.BinaryOutStreams[fileRow.LoadType]);
                    }
                }

                lf.Close(); // needed to free the file access
                //var autoDevs = new List<CalcAutoDev>();
                //var ps = new Postprocessor(lf.FileFactoryAndTracker, calculationProfiler,calcParameters );
                //var householdKeys = new HashSet<string> {"1"};
                //var calcAffordanceTaggingSets = new List<CalcAffordanceTaggingSet>();
                //var calcDeviceTaggingSets = new List<CalcDeviceTaggingSet>();
                //var calcDeviceTaggingSets = new List<DeviceTaggingSetInformation>();
                //var householdPlans = new List<CalcHouseholdPlan>();
                //var householdNamesByNumber = new Dictionary<string, string> {["HH1"] = "household"};
                //var affordanceEnergyUseFile = new AffordanceEnergyUseFile(lf.FileFactoryAndTracker,calcParameters);

                //var results = new Dictionary<string, double>();
                //BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
                //BitArray isOnVacation = new BitArray(calcParameters.InternalTimesteps);
                //CalcPersonDto cpd = new CalcPersonDto("personname", Guid.NewGuid().ToString(),18,PermittedGender.Female,key,new List<DateSpan>(),new List<DateSpan>(),1,"traittag","hhname"  );
                //var persons = new List<CalcPerson> {new CalcPerson(cpd, new Random(), lf,cloc,calcParameters,isSick,isOnVacation)};
                //var deviceNamesToCategory = new Dictionary<string, string>();
                //CalcDeviceTaggingSets calcDeviceTaggingSets = new CalcDeviceTaggingSets();
                PostProcessingManager cpp = new PostProcessingManager(calculationProfiler, fft);
                cpp.Run(wd.WorkingDirectory);
                /*ps.EndOfSimulationProcessing(devices, locations, autoDevs, loadTypes, odap.Oefc, householdKeys
                    ,calcAffordanceTaggingSets, calcDeviceTaggingSets, householdPlans, householdNamesByNumber
                    ,affordanceEnergyUseFile, results, CalcObjectType.ModularHousehold, persons, deviceNamesToCategory,10);*/
                //var dstpath = Path.Combine(wd.WorkingDirectory,DirectoryNames.CalculateTargetdirectory(TargetDirectory.Reports),"DeviceSums." + clt.Name + ".csv");
                lf.Close(); // needed to free the file access
                DirectoryInfo di = new DirectoryInfo(wd.WorkingDirectory);
                FileInfo[] fis = di.GetFiles("DeviceSums.*", SearchOption.AllDirectories);

                if (fis.Length == 0) {
                    throw new LPGException("No Sum File was generated");
                }

                using (var sr = new StreamReader(fis[0].FullName)) {
                    sr.ReadLine(); //header
                    var result = sr.ReadLine(); //0
                    if (result == null) {
                        throw new LPGException("Result was null");
                    }

                    var arr = result.Split(';');
                    Assert.AreEqual("300", arr[1]);
                }
            }

            Logger.Info(wd.WorkingDirectory);
            wd.CleanUp();
        }

        [Test]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
        [Category("BasicTest")]
        public void PostProcessingTestTwoActivation()
        {
            var startdate = new DateTime(2018, 1, 1);
            var enddate = startdate.AddMinutes(100);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate)
                .SetEndDate(enddate).EnableShowSettlingPeriod();
            //CalculationProfiler calculationProfiler = new CalculationProfiler();

            var rnd = new Random(1);
            var nr = new NormalRandom(0, 1, rnd);
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            calcParameters.Enable(CalcOption.IndividualSumProfiles);
            calcParameters.Enable(CalcOption.DeviceProfiles);
            calcParameters.Enable(CalcOption.TotalsPerDevice);
            calcParameters.Enable(CalcOption.TotalsPerLoadtype);
            calcParameters.Enable(CalcOption.DetailedDatFiles);
            Config.IsInUnitTesting = true;
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcParameterLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcLoadTypeDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcObjectInformationLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new DeviceActivationEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcPersonDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new DeviceTaggingSetLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.Save(calcParameters);
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh1", wd.InputDataLogger);
            fft.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General, "desc", null, null);
            //fft.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General,"desc");
            //SqlResultLoggingService srls =new SqlResultLoggingService(Path.Combine(wd.WorkingDirectory, "results.sqlite"));
            DateStampCreator dsc = new DateStampCreator(calcParameters);
            IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters);
            CalcObjectInformation coi = new CalcObjectInformation(CalcObjectType.ModularHousehold, "objname", wd.WorkingDirectory);
            wd.InputDataLogger.Save(coi);
            using (var lf = new LogFile(calcParameters, fft, old, wd.SqlResultLoggingService)) {
                HouseholdKey key = new HouseholdKey("hh1");
                lf.InitHousehold(key, "hh1 key", HouseholdKeyType.Household, "Description", null, null);
                var odap = new OnlineDeviceActivationProcessor(nr, lf, calcParameters);
                var clt = new CalcLoadType("lt1", "W", "kWh", 3, true, Guid.NewGuid().ToString());
                var loadTypes = new List<CalcLoadTypeDto> {clt.ConvertToDto()};
                wd.InputDataLogger.Save(loadTypes);
                var cdl = new CalcDeviceLoad("devload1", 10, clt, 666, 0, Guid.NewGuid().ToString());
                var deviceLoads = new List<CalcDeviceLoad> {
                    cdl
                };
                var cloc = new CalcLocation("locname", Guid.NewGuid().ToString());
                string devguid = Guid.NewGuid().ToString();
                string devcategoryguid = Guid.NewGuid().ToString();
                CalcDeviceDto cdto = new CalcDeviceDto("devicename",
                    devcategoryguid,
                    key,
                    OefcDeviceType.Device,
                    "category",
                    "",
                    devguid,
                    cloc.Guid,
                    cloc.Name);
                var devices = new List<IHouseholdKey> {
                    cdto
                };
                wd.InputDataLogger.SaveList(devices);

                var device = new CalcDevice("devicename",
                    deviceLoads,
                    devcategoryguid,
                    odap,
                    cloc,
                    key,
                    OefcDeviceType.Device,
                    "category",
                    string.Empty,
                    calcParameters,
                    devguid);
                //var devices = new List<CalcDevice> {device};
                var cp = new CalcProfile("profile1", Guid.NewGuid().ToString(), new TimeSpan(0, 1, 0), ProfileType.Absolute, "custom");
                cp.AddNewTimepoint(new TimeSpan(0), 0);
                cp.AddNewTimepoint(new TimeSpan(0, 1, 0), 10);
                cp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0);
                cp.ConvertToTimesteps();
                //var locations = new List<CalcLocation> {cloc};
                TimeStep ts = new TimeStep(0, calcParameters);
                device.SetTimeprofile(cp, ts, clt, "affordanceName", "activatorName", 1, false);
                device.SetTimeprofile(cp, ts.AddSteps(5), clt, "affordanceName", "activatorName", 1, false);
                device.SetTimeprofile(cp.CompressExpandDoubleArray(0.5), ts.AddSteps(8), clt, "affordanceName", "activatorName", 1, false);
                device.SetTimeprofile(cp.CompressExpandDoubleArray(2), ts.AddSteps(10), clt, "affordanceName", "activatorName", 1, false);
                for (var i = 0; i < 30; i++) {
                    TimeStep ts1 = new TimeStep(i, calcParameters);
                    var filerows = odap.ProcessOneTimestep(ts1);
                    Assert.AreEqual(1, filerows.Count);
                    Assert.AreEqual(1, filerows[0].EnergyEntries.Count);
                    Logger.Info(filerows[0].EnergyEntries[0].ToString(CultureInfo.CurrentCulture));
                    foreach (var fileRow in filerows) {
                        fileRow.Save(odap.BinaryOutStreams[fileRow.LoadType]);
                    }
                }

                //var autoDevs = new List<CalcAutoDev>();
                //var ps = new Postprocessor(lf.FileFactoryAndTracker, calculationProfiler,calcParameters);
                //var householdKeys = new HashSet<string> {"1"};
                //var calcAffordanceTaggingSets = new List<CalcAffordanceTaggingSet>();
                var deviceTaggingSetInformation = new DeviceTaggingSetInformation("name");
                List<DeviceTaggingSetInformation> taggingsets = new List<DeviceTaggingSetInformation> {
                    deviceTaggingSetInformation
                };
                wd.InputDataLogger.Save(taggingsets);
                //var householdPlans = new List<CalcHouseholdPlan>();
                //var householdNamesByNumber = new Dictionary<string, string> {["HH1"] = "household"};
                //var affordanceEnergyUseFile = new AffordanceEnergyUseFile(lf.FileFactoryAndTracker,calcParameters);
                //lf.Close(null); // needed to free file access
                //var results = new Dictionary<string, double>();
                var persons = new List<CalcPersonDto>();
                CalcPersonDto dto = new CalcPersonDto("name",
                    Guid.NewGuid().ToString(),
                    18,
                    PermittedGender.Female,
                    key,
                    new List<DateSpan>(),
                    new List<DateSpan>(),
                    1,
                    "tag",
                    "hhname");
                persons.Add(dto);
                wd.InputDataLogger.SaveList(persons.ConvertAll(x => (IHouseholdKey)x));
                //var deviceNamesToCategory = new Dictionary<string, string>();
                lf.Close();
                CalculationProfiler cprof = new CalculationProfiler();
                PostProcessingManager ppm = new PostProcessingManager(cprof, fft);
                ppm.Run(wd.WorkingDirectory);
                TotalsEntryLogger tel = new TotalsEntryLogger(wd.SqlResultLoggingService);
                var results = tel.Read(key);
                Assert.That(results[0].Value, Is.EqualTo(150));
            }

            Logger.Info(wd.WorkingDirectory);
            wd.CleanUp();
        }

        [Test]
        [Category("BasicTest")]
        public void RunCalcStarter()
        {
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.Calculation);
            var sim = new Simulator(db.ConnectionString);
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            sim.MyGeneralConfig.StartDateString = "01.01.2013";
            Config.IsInUnitTesting = true;
            Config.ExtraUnitTestChecking = true;
            sim.MyGeneralConfig.EndDateString = "02.01.2013";
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.Reasonable);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerDevice);
            sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
            CalculationProfiler cp = new CalculationProfiler();
            var csps = new CalcStartParameterSet(ReportFinishFuncForHouseAndSettlement,
                ReportFinishFuncForHousehold,
                OpenTabFunc,
                null,
                sim.GeographicLocations[0],
                sim.TemperatureProfiles[0],
                sim.ModularHouseholds[0],
                SetCalculationEntries,
                EnergyIntensityType.EnergySaving,
                ReportCancelFunc,
                false,
                version,
                null,
                LoadTypePriority.Mandatory,
                null,
                null,
                sim.MyGeneralConfig.AllEnabledOptions(),
                new DateTime(2018, 1, 1),
                new DateTime(2018, 1, 2),
                new TimeSpan(0, 1, 0),
                ";",
                5,
                new TimeSpan(0, 1, 0),
                false,
                false,
                false,
                3,
                3,
                cp,
                null,
                null,
                DeviceProfileHeaderMode.Standard,
                false);
            var cs = new CalcStarter(sim);
            cs.Start(csps, wd.WorkingDirectory);
            /*var dstpath = Path.Combine(wd.WorkingDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Reports), "TotalsPerLoadtype.csv");
            using (var sr = new StreamReader(dstpath)) {
                sr.ReadLine();
                while (!sr.EndOfStream) {
                    var line = sr.ReadLine();
                    if (line == null) {
                        throw new LPGException("Result was null");
                    }

                    var arr = line.Split(';');
                    Logger.Info(line);
                    var devicesumFileName = Path.Combine(wd.WorkingDirectory,
                        DirectoryNames.CalculateTargetdirectory(TargetDirectory.Reports),
                        "DeviceSums." + arr[0] + ".csv");
                    using (var devsum = new StreamReader(devicesumFileName)) {
                        while (!devsum.EndOfStream) {
                            var s = devsum.ReadLine();
                            if (s == null) {
                                throw new LPGException("Result was null");
                            }

                            if (s.StartsWith("Sums;", StringComparison.Ordinal)) {
                                var sumarr = s.Split(';');
                                Logger.Info("Line from totalsPerLoadType");
                                Logger.Info(line);
                                Logger.Info("Line from " + devicesumFileName);
                                Logger.Info(s);
                                var arr1 = sumarr[1];
                                if (arr1.Length > 6) {
                                    arr1 = arr1.Substring(0, 6);
                                }

                                var arr2 = arr[3];
                                if (arr2.Length > 6) {
                                    arr2 = arr2.Substring(0, 6);
                                }

                                Assert.AreEqual(arr1, arr2);
                            }
                        }
                    }
                }
            }*/

            wd.CleanUp();
            db.Cleanup();
        }
    }
}