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
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using CalculationController.DtoFactories;
using CalculationController.Queue;
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
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests.OnlineDeviceActivation {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class OnlineDeviceActivationProcessorTests : UnitTestBaseClass {
        public OnlineDeviceActivationProcessorTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        private static bool OpenTabFunc([CanBeNull] object o)
        {
            Logger.Warning("OpenTabFunc was called");
            return true;
        }

        private static bool ReportCancelFunc() => throw new LPGException("Not implemented");

        private static bool ReportFinishFuncForHouseAndSettlement(bool b, [JetBrains.Annotations.NotNull] string arg3,
                                                                  [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<ResultFileEntry> arg4)
        {
            Logger.Info(Utili.GetCurrentMethodAndClass());
            return true;
        }

#pragma warning disable S4144 // Methods should not have identical implementations
        private static bool ReportFinishFuncForHousehold(bool b, [JetBrains.Annotations.NotNull] string arg3, [JetBrains.Annotations.NotNull] string arg4)
        {
#pragma warning restore S4144 // Methods should not have identical implementations
            Logger.Info(Utili.GetCurrentMethodAndClass());
            return true;
        }

        /// <summary>
        ///     this tests tests if the autonomous energy consumption is set to 0 correctly if a device that is both autonomous and
        ///     user controlled is activated manually
        ///     For example if a TV with standby use is turned on, then it is not running in standby simultanously.
        /// </summary>
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void OnlineDeviceActivationProcessorSetToZeroTest()
        {
            var rnd = new Random(1);
            var nr = new NormalRandom(0, 1, rnd);
            var startdate = new DateTime(2018, 1, 1);
            var enddate = startdate.AddMinutes(100);
            var calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).EnableShowSettlingPeriod();
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                //calcParameters.Enable(CalcOption.ActionsLogfile);
                calcParameters.Enable(CalcOption.DeviceProfiles);
                Config.ExtraUnitTestChecking = true;
                // calcProfile
                var profileWith100 = new CalcProfile("calcProfile", Guid.NewGuid().ToStrGuid(), new TimeSpan(0, 1, 0), ProfileType.Absolute, "blub");
                profileWith100.AddNewTimepoint(new TimeSpan(0), 100);
                profileWith100.AddNewTimepoint(new TimeSpan(0, 5, 0), 100);
                profileWith100.ConvertToTimesteps();
                var profileWith50 = new CalcProfile("calcProfile2", Guid.NewGuid().ToStrGuid(), new TimeSpan(0, 1, 0), ProfileType.Absolute, "blub");
                profileWith50.AddNewTimepoint(new TimeSpan(0), 50);
                profileWith50.AddNewTimepoint(new TimeSpan(0, 3, 0), 50);
                profileWith50.ConvertToTimesteps();
                // Loadtype
                var clt = new CalcLoadType("lt1", "W", "kWh", 1, true, Guid.NewGuid().ToStrGuid());
                // Location
                var cloc = new CalcLocation("Location", Guid.NewGuid().ToStrGuid());
                // devices

                var cdl = new CalcDeviceLoad("lt1", 100, clt, 100, 0);
                var loads = new List<CalcDeviceLoad>();
                var results = new List<string> {
                    "100;0;", "100;0;",
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
                using (var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh1", wd.InputDataLogger)) {
                    fft.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General, "desc", null, null);
                    //SqlResultLoggingService srls = new SqlResultLoggingService(Path.Combine(wd.WorkingDirectory,"results.sqlite"));
                    var dsc = new DateStampCreator(calcParameters);
                    using (IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters)) {
                        {
                            var odap = new OnlineDeviceActivationProcessor(old, calcParameters, fft);
                            using (var calcRepo = new CalcRepo(calcParameters: calcParameters, odap: odap, rnd: rnd, normalRandom: nr)) {
                                var requirements = new List<VariableRequirement>();
                                var devCatGuid = Guid.NewGuid().ToStrGuid();
                                var key = new HouseholdKey("HH1");
                                var cddauto = new CalcDeviceDto("devicename", devCatGuid, key, OefcDeviceType.Device, "device category",
                                    " (autonomous)", Guid.NewGuid().ToStrGuid(), cloc.Guid, cloc.Name);
                                var autodev = new CalcAutoDev(profileWith100, clt, loads, 0, 1, cloc, requirements, cddauto, calcRepo);
                                var cdd = new CalcDeviceDto("devicename", devCatGuid, key, OefcDeviceType.Device, "device category", "",
                                    Guid.NewGuid().ToStrGuid(), cloc.Guid, cloc.Name);
                                var device = new CalcDevice(loads, cloc, cdd, calcRepo);
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
                                    Logger.Info(pair.Key.Name);
                                    foreach (var entry in pair.Value) {
                                        Logger.Info(entry.Key + " - " + entry.Value.Name);
                                    }
                                }

                                for (var i = 0; i < 15; i++) {
                                    var ts = new TimeStep(i, 0, true);
                                    if (!autodev.IsBusyDuringTimespan(ts, 1, 1, clt)) {
                                        autodev.Activate(ts);
                                    }

                                    if (i == 6) {
                                        device.SetTimeprofile(profileWith50, ts, clt, "blub", "Person", 1, false);
                                    }

                                    var filerows = odap.ProcessOneTimestep(ts);
                                    filerows.Count.Should().Be(1);
                                    filerows[0].EnergyEntries.Count.Should().Be(2);
                                    var entries = string.Empty;

                                    foreach (var d in filerows[0].EnergyEntries) {
                                        entries += d.ToString(CultureInfo.CurrentCulture) + ";";
                                    }

                                    Logger.Info(entries);
                                    results[i].Should().Be(entries);
                                }
                            }
                        }
                    }
                }

                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void OnlineDeviceActivationProcessorArrayTest()
        {
            var rnd = new Random(1);
            var nr = new NormalRandom(0, 1, rnd);
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                var calcParameters = CalcParametersFactory.MakeGoodDefaults().EnableShowSettlingPeriod();
                //calcParameters.Enable(CalcOption.ActionsLogfile);
                calcParameters.Enable(CalcOption.DeviceProfiles);
                calcParameters.Enable(CalcOption.DetailedDatFiles);
                using (var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh1", wd.InputDataLogger)) {
                    //SqlResultLoggingService srls = new SqlResultLoggingService(Path.Combine(wd.WorkingDirectory, "results.sqlite"));
                    var dsc = new DateStampCreator(calcParameters);

                    using (IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters)) {
                        var hhkey = new HouseholdKey("HH1");
                        fft.RegisterHousehold(Constants.GeneralHouseholdKey, "generalhousehold", HouseholdKeyType.General, "Description", null, null);
                        fft.RegisterHousehold(hhkey, "hh1", HouseholdKeyType.Household, "Description", null, null);
                        var odap = new OnlineDeviceActivationProcessor(old, calcParameters, fft);
                        var deviceGuid = "devguid".ToStrGuid();
                        var locationGuid = "locationGuid".ToStrGuid();
                        var loadtypeGuid = "ltguid".ToStrGuid();
                        var cdd = new CalcDeviceDto("devicename", "devcatguid".ToStrGuid(), hhkey, OefcDeviceType.Device, "devcatname", "",
                            deviceGuid, locationGuid, "loc");

                        var key = new OefcKey(cdd, loadtypeGuid);
                        var clt = new CalcLoadType("lt1", "W", "kWh", 1, true, loadtypeGuid);
                        odap.RegisterDevice(clt.ConvertToDto(), cdd);
                        double[] stepValues = {1.0, 0};
                        var valueList = new List<double>(stepValues);
                        var cp = new CalcProfile("myCalcProfile", Guid.NewGuid().ToStrGuid(), valueList, ProfileType.Absolute, "synthetic");
                        var ts1 = new TimeStep(1, 0, false);
                        var cdl = new CalcDeviceLoad("", 1, clt, 0, 0);
                        var sv = StepValues.MakeStepValues(cp, 10, RandomValueProfile.MakeStepValues(cp.StepValues.Count, nr, 0), cdl);
                        odap.AddNewStateMachine(ts1, clt.ConvertToDto(), "name1", "p1", key, cdd, sv);
                        double[] resultValues = {0, 10.0, 0, 0, 0, 0, 0, 0, 0, 0};

                        for (var i = 0; i < 10; i++) {
                            var ts = new TimeStep(i, 0, true);
                            var filerows = odap.ProcessOneTimestep(ts);
                            filerows.Count.Should().Be(1);
                            filerows[0].EnergyEntries.Count.Should().Be(1);
                            Logger.Info(filerows[0].EnergyEntries[0].ToString(CultureInfo.CurrentCulture));
                            resultValues[i].Should().Be(filerows[0].EnergyEntries[0]);
                            foreach (var fileRow in filerows) {
                                fileRow.Save(odap.BinaryOutStreams[fileRow.LoadType]);
                            }
                        }
                    }
                }

                Logger.Info(wd.WorkingDirectory);
                wd.CleanUp();
            }
        }

        [Fact]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void PostProcessingTestSingleActivation()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                wd.InputDataLogger.AddSaver(new CalcParameterLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new DeviceActivationEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new CalcLoadTypeDtoLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new CalcPersonDtoLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new DeviceTaggingSetLogger(wd.SqlResultLoggingService));
                var calculationProfiler = new CalculationProfiler();

                var startdate = new DateTime(2018, 1, 1);
                var enddate = startdate.AddMinutes(1000);
                var calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).EnableShowSettlingPeriod();

                var rnd = new Random(1);
                var nr = new NormalRandom(0, 1, rnd);

                calcParameters.Enable(CalcOption.IndividualSumProfiles);
                calcParameters.Enable(CalcOption.DeviceProfiles);
                calcParameters.Enable(CalcOption.TotalsPerDevice);
                calcParameters.Enable(CalcOption.TotalsPerLoadtype);
                calcParameters.Enable(CalcOption.DetailedDatFiles);
                calcParameters.Enable(CalcOption.DeviceActivations);
                var key = new HouseholdKey("hh1");

                wd.InputDataLogger.Save(calcParameters);

                using (var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hhname", wd.InputDataLogger)) {
                    //wd.InputDataLogger.AddSaver(new CalcDeviceDtoLogger(wd.SqlResultLoggingService));
                    var dsc = new DateStampCreator(calcParameters);
                    using (IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters)) {
                        var calcPersonDto = new CalcPersonDto("blub", "personguid".ToStrGuid(), 1, PermittedGender.Male, key, new List<DateSpan>(),
                            new List<DateSpan>(), 1, "traittag", "householdname");
                        var persons = new List<CalcPersonDto> {
                            calcPersonDto
                        };
                        wd.InputDataLogger.SaveList(persons.ConvertAll(x => (IHouseholdKey)x));

                        using (var lf = new LogFile(calcParameters, fft)) {
                            fft.RegisterHousehold(key, "test hh", HouseholdKeyType.Household, "Description", null, null);
                            fft.RegisterHousehold(Constants.GeneralHouseholdKey, "General", HouseholdKeyType.General, "Description", null, null);
                            var odap = new OnlineDeviceActivationProcessor(old, calcParameters, fft);
                            var clt = new CalcLoadType("lt1", "W", "kWh", 3, true, Guid.NewGuid().ToStrGuid());
                            var loadTypeDtos = new List<CalcLoadTypeDto> {
                                clt.ConvertToDto()
                            };
                            wd.InputDataLogger.Save(loadTypeDtos);
                            //var loadTypes = new List<CalcLoadType> {clt};
                            var cdl = new CalcDeviceLoad("devload1", 10, clt, 666, 0);
                            var deviceLoads = new List<CalcDeviceLoad> {
                                cdl
                            };
                            var cloc = new CalcLocation("locname", Guid.NewGuid().ToStrGuid());
                            var deviceCategoryGuid = Guid.NewGuid().ToStrGuid();
                            var calcDeviceDto = new CalcDeviceDto("devicename", deviceCategoryGuid, key, OefcDeviceType.Device, "category",
                                string.Empty, Guid.NewGuid().ToStrGuid(), cloc.Guid, cloc.Name);
                            var calcDeviceDtos = new List<CalcDeviceDto> {
                                calcDeviceDto
                            };
                            wd.InputDataLogger.SaveList(calcDeviceDtos.ConvertAll(x => (IHouseholdKey)x));
                            //device tagging set for the post processing
                            var cdts = new List<DeviceTaggingSetInformation>();
                            var dtsi = new DeviceTaggingSetInformation("myset");
                            dtsi.AddTag(calcDeviceDto.Name, "testTag ");
                            cdts.Add(dtsi);
                            wd.InputDataLogger.Save(cdts);
                            //device
                            using (var calcRepo = new CalcRepo(calcParameters: calcParameters, odap: odap, rnd: rnd, normalRandom: nr)) {
                                var device = new CalcDevice(deviceLoads, cloc, calcDeviceDto, calcRepo);
                                //var devices = new List<CalcDevice> {device};
                                double[] resultValues = {0, 100.0, 0, 0, 0, 0, 0, 0, 0, 0};
                                var cp = new CalcProfile("profile1", Guid.NewGuid().ToStrGuid(), new TimeSpan(0, 1, 0), ProfileType.Absolute,
                                    "custom");
                                cp.AddNewTimepoint(new TimeSpan(0), 0);
                                cp.AddNewTimepoint(new TimeSpan(0, 1, 0), 10);
                                cp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0);
                                cp.ConvertToTimesteps();
                                //var locations = new List<CalcLocation> {cloc};
                                var ts1 = new TimeStep(0, calcParameters);
                                device.SetTimeprofile(cp, ts1, clt, "affordanceName", "activatorName", 10, false);
                                for (var i = 0; i < 10; i++) {
                                    var ts = new TimeStep(i, calcParameters);
                                    var filerows = odap.ProcessOneTimestep(ts);
                                    filerows.Count.Should().Be(1);
                                    filerows[0].EnergyEntries.Count.Should().Be(1);
                                    Logger.Info(filerows[0].EnergyEntries[0].ToString(CultureInfo.CurrentCulture));
                                    filerows[0].EnergyEntries[0].Should().Be(resultValues[i]);
                                    foreach (var fileRow in filerows) {
                                        fileRow.Save(odap.BinaryOutStreams[fileRow.LoadType]);
                                    }
                                }
                            }
                            old.FinalSaveToDatabase();
                            fft.Dispose();
                            lf.Dispose(); // needed to free the file access
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
                            //CalcPersonDto cpd = new CalcPersonDto("personname", Guid.NewGuid().ToStrGuid(),18,PermittedGender.Female,key,new List<DateSpan>(),new List<DateSpan>(),1,"traittag","hhname"  );
                            //var persons = new List<CalcPerson> {new CalcPerson(cpd, new Random(), lf,cloc,calcParameters,isSick,isOnVacation)};
                            //var deviceNamesToCategory = new Dictionary<string, string>();
                            //CalcDeviceTaggingSets calcDeviceTaggingSets = new CalcDeviceTaggingSets();
                            var cpp = new PostProcessingManager(calculationProfiler, fft);
                            cpp.Run(wd.WorkingDirectory);
                            /*ps.EndOfSimulationProcessing(devices, locations, autoDevs, loadTypes, odap.Oefc, householdKeys
                                ,calcAffordanceTaggingSets, calcDeviceTaggingSets, householdPlans, householdNamesByNumber
                                ,affordanceEnergyUseFile, results, CalcObjectType.ModularHousehold, persons, deviceNamesToCategory,10);*/
                            //var dstpath = Path.Combine(wd.WorkingDirectory,DirectoryNames.CalculateTargetdirectory(TargetDirectory.Reports),"DeviceSums." + clt.Name + ".csv");
                            lf.Dispose(); // needed to free the file access
                            var di = new DirectoryInfo(wd.WorkingDirectory);
                            var fis = di.GetFiles("DeviceSums.*", SearchOption.AllDirectories);

                            if (fis.Length == 0) {
                                throw new LPGException("No Sum File was generated");
                            }
                            fft.Dispose();
                            using (var sr = new StreamReader(fis[0].FullName)) {
                                sr.ReadLine(); //header
                                var result = sr.ReadLine(); //0
                                if (result == null) {
                                    throw new LPGException("Result was null");
                                }

                                var arr = result.Split(';');
                                Assert.Equal("300", arr[1]);
                            }
                        }
                    }
                }

                Logger.Info(wd.WorkingDirectory);
                wd.CleanUp();
            }
        }

        [Fact]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void PostProcessingTestTwoActivation()
        {
            var startdate = new DateTime(2018, 1, 1);
            var enddate = startdate.AddMinutes(100);
            var calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).EnableShowSettlingPeriod();
            //CalculationProfiler calculationProfiler = new CalculationProfiler();

            var rnd = new Random(1);
            var nr = new NormalRandom(0, 1, rnd);
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                calcParameters.Enable(CalcOption.IndividualSumProfiles);
                calcParameters.Enable(CalcOption.DeviceProfiles);
                calcParameters.Enable(CalcOption.TotalsPerDevice);
                calcParameters.Enable(CalcOption.TotalsPerLoadtype);
                calcParameters.Enable(CalcOption.DeviceActivations);
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
                using (var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh1", wd.InputDataLogger)) {
                    fft.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General, "desc", null, null);
                    //fft.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General,"desc");
                    //SqlResultLoggingService srls =new SqlResultLoggingService(Path.Combine(wd.WorkingDirectory, "results.sqlite"));
                    var dsc = new DateStampCreator(calcParameters);
                    using (IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters)) {
                        var coi = new CalcObjectInformation(CalcObjectType.ModularHousehold, "objname", wd.WorkingDirectory);
                        wd.InputDataLogger.Save(coi);
                        using (var lf = new LogFile(calcParameters, fft)) {
                            var key = new HouseholdKey("hh1");
                            fft.RegisterHousehold(key, "hh1 key", HouseholdKeyType.Household, "Description", null, null);
                            var odap = new OnlineDeviceActivationProcessor(old, calcParameters, fft);
                            var clt = new CalcLoadType("lt1", "W", "kWh", 3, true, Guid.NewGuid().ToStrGuid());
                            var loadTypes = new List<CalcLoadTypeDto> {clt.ConvertToDto()};
                            wd.InputDataLogger.Save(loadTypes);
                            var cdl = new CalcDeviceLoad("devload1", 10, clt, 666, 0);
                            var deviceLoads = new List<CalcDeviceLoad> {
                                cdl
                            };
                            var cloc = new CalcLocation("locname", Guid.NewGuid().ToStrGuid());
                            var devguid = Guid.NewGuid().ToStrGuid();
                            var devcategoryguid = Guid.NewGuid().ToStrGuid();
                            var cdto = new CalcDeviceDto("devicename", devcategoryguid, key, OefcDeviceType.Device, "category", "", devguid,
                                cloc.Guid, cloc.Name);
                            var devices = new List<IHouseholdKey> {
                                cdto
                            };
                            wd.InputDataLogger.SaveList(devices);
                            using (var calcRepo = new CalcRepo(odap, calcParameters: calcParameters, rnd: rnd, normalRandom: nr)) {
                                var device = new CalcDevice(deviceLoads, cloc, cdto, calcRepo);
                                //var devices = new List<CalcDevice> {device};
                                var cp = new CalcProfile("profile1", Guid.NewGuid().ToStrGuid(), new TimeSpan(0, 1, 0), ProfileType.Absolute,
                                    "custom");
                                cp.AddNewTimepoint(new TimeSpan(0), 0);
                                cp.AddNewTimepoint(new TimeSpan(0, 1, 0), 10);
                                cp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0);
                                cp.ConvertToTimesteps();
                                //var locations = new List<CalcLocation> {cloc};
                                var ts = new TimeStep(0, calcParameters);
                                device.SetTimeprofile(cp, ts, clt, "affordanceName", "activatorName", 1, false);
                                device.SetTimeprofile(cp, ts.AddSteps(5), clt, "affordanceName", "activatorName", 1, false);
                                device.SetTimeprofile(cp.CompressExpandDoubleArray(0.5), ts.AddSteps(8), clt, "affordanceName", "activatorName", 1,
                                    false);
                                device.SetTimeprofile(cp.CompressExpandDoubleArray(2), ts.AddSteps(10), clt, "affordanceName", "activatorName", 1,
                                    false);
                            }

                            for (var i = 0; i < 30; i++) {
                                var ts1 = new TimeStep(i, calcParameters);
                                var filerows = odap.ProcessOneTimestep(ts1);
                                Assert.Equal(1, filerows.Count);
                                Assert.Equal(1, filerows[0].EnergyEntries.Count);
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
                            var taggingsets = new List<DeviceTaggingSetInformation> {
                                deviceTaggingSetInformation
                            };

                            wd.InputDataLogger.Save(taggingsets);
                            //var householdPlans = new List<CalcHouseholdPlan>();
                            //var householdNamesByNumber = new Dictionary<string, string> {["HH1"] = "household"};
                            //var affordanceEnergyUseFile = new AffordanceEnergyUseFile(lf.FileFactoryAndTracker,calcParameters);
                            //lf.Close(null); // needed to free file access
                            //var results = new Dictionary<string, double>();
                            var persons = new List<CalcPersonDto>();
                            var dto = new CalcPersonDto("name", Guid.NewGuid().ToStrGuid(), 18, PermittedGender.Female, key, new List<DateSpan>(),
                                new List<DateSpan>(), 1, "tag", "hhname");
                            persons.Add(dto);
                            wd.InputDataLogger.SaveList(persons.ConvertAll(x => (IHouseholdKey)x));
                            //var deviceNamesToCategory = new Dictionary<string, string>();
                            old.FinalSaveToDatabase();
                            lf.Dispose();
                            fft.Dispose();
                            var cprof = new CalculationProfiler();
                            var ppm = new PostProcessingManager(cprof, fft);
                            ppm.Run(wd.WorkingDirectory);
                            var tel = new TotalsPerLoadtypeEntryLogger(wd.SqlResultLoggingService);
                            var results = tel.Read(key);
                            results[0].Value.Should().Be(150);
                        }
                    }
                }

                Logger.Info(wd.WorkingDirectory);
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void RunCalcStarter()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass())) {
                    var sim = new Simulator(db.ConnectionString);
                    sim.MyGeneralConfig.StartDateString = "01.01.2013";
                    Config.IsInUnitTesting = true;
                    Config.ExtraUnitTestChecking = true;
                    sim.MyGeneralConfig.EndDateString = "02.01.2013";
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.Reasonable);
                    sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
                    sim.MyGeneralConfig.Enable(CalcOption.TotalsPerDevice);
                    sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
                    var cp = new CalculationProfiler();
                    var csps = new CalcStartParameterSet(ReportFinishFuncForHouseAndSettlement, ReportFinishFuncForHousehold, OpenTabFunc, null,
                        sim.GeographicLocations[0], sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.EnergySaving,
                        ReportCancelFunc, false, null, LoadTypePriority.Mandatory, null, null, sim.MyGeneralConfig.AllEnabledOptions(),
                        new DateTime(2018, 1, 1), new DateTime(2018, 1, 2), new TimeSpan(0, 1, 0), ";", 5, new TimeSpan(0, 1, 0), false, false, false,
                        3, 3, cp, null, null, DeviceProfileHeaderMode.Standard, false, wd.WorkingDirectory, false);
                    var cs = new CalcStarter(sim);
                    cs.Start(csps);
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

                                        (arr2).Should().Be(arr1);
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
    }
}