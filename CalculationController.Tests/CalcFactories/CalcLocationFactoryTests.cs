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
using Autofac;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.DtoFactories;
using CalculationController.Helpers;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.Tests;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;
using Logger = Common.Logger;

namespace CalculationController.Tests.CalcFactories {
    [TestFixture]
    public class CalcLocationFactoryTests : UnitTestBaseClass {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakeCalcLocationsTest()
        {
            var locations = new List<Location>();
            var loc = new Location("loc", 1, string.Empty, Guid.NewGuid().ToStrGuid());
            locations.Add(loc);
            Random r = new Random(1);
            DeviceCategoryPicker picker = new DeviceCategoryPicker(r, null);
            //var cp = new CalcFactoryParameters(picker);
            //var dict =new Dictionary<CalcLocation, List<IAssignableDevice>>();
            var deviceActions = new ObservableCollection<DeviceAction>();
            //var locdict = new Dictionary<Location, CalcLocation>();
            CalcParameters cp = CalcParametersFactory.MakeGoodDefaults();
            //var mock = new Mock<IOnlineDeviceActivationProcessor>();
            //var iodap = mock.Object;
            var locationDtoDict = new CalcLoadTypeDtoDictionary(new Dictionary<VLoadType, CalcLoadTypeDto>());
            var ltDict = new CalcLoadTypeDictionary(new Dictionary<CalcLoadTypeDto, CalcLoadType>());
            CalcLocationDtoFactory cldt = new CalcLocationDtoFactory(cp, picker, locationDtoDict);
            Dictionary<CalcLocationDto, List<IAssignableDevice>> deviceLocationDict = new Dictionary<CalcLocationDto, List<IAssignableDevice>>();
            LocationDtoDict calclocdict = new LocationDtoDict();
            List<DeviceCategoryDto> devcat = new List<DeviceCategoryDto>();
            using CalcRepo calcRepo = new CalcRepo();
            //devcat.Add(new DeviceCategoryDto(dc.FullPath, Guid.NewGuid().ToStrGuid()));
            var locdtos = cldt.MakeCalcLocations(locations,
                new HouseholdKey("hh1"),
                EnergyIntensityType.EnergyIntensive,
                deviceLocationDict,
                deviceActions,
                calclocdict,
                devcat);
            Assert.That(locdtos.Count, Is.EqualTo(1));
            Assert.That(locdtos[0].Name, Is.EqualTo(loc.Name));
            CalcLocationFactory clf = new CalcLocationFactory (ltDict,calcRepo);
            //"HH1", EnergyIntensityType.EnergySaving, dict,deviceActions,
            DtoCalcLocationDict dcl = new DtoCalcLocationDict();
            var calclocs = clf.MakeCalcLocations(locdtos, dcl,calcRepo);
            Assert.That(calclocs.Count, Is.EqualTo(1));
            Assert.That(calclocs[0].Name, Is.EqualTo(loc.Name));
            Assert.That(calclocs[0].Guid, Is.EqualTo(locdtos[0].Guid));
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakeCalcLocationsTestWith2Device()
        {
            var builder = new ContainerBuilder();
            var r = new Random(1);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(2018, 1, 1)
                .SetEndDate(new DateTime(2018, 1, 1, 2, 0, 0)).SetSettlingDays(0).EnableShowSettlingPeriod();
            var locations = new List<Location>();
            var loc = new Location("loc", 1, string.Empty, Guid.NewGuid().ToStrGuid());
            locations.Add(loc);
            var devices = new ObservableCollection<RealDevice>();
            var dc = new DeviceCategory("dc", -1, string.Empty, false, devices, Guid.NewGuid().ToStrGuid(), 1, true);
            List<DeviceCategoryDto> devcat = new List<DeviceCategoryDto> {
                new DeviceCategoryDto(dc.FullPath, Guid.NewGuid().ToStrGuid())
            };
            var rd = new RealDevice("rda", 1, string.Empty, dc, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid(), 1);
            var rd2 = new RealDevice("rdb", 1, string.Empty, dc, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid(), 1);
            loc.AddDevice(rd, false);
            loc.AddDevice(rd2, false);
            var deviceLocationDict = new Dictionary<CalcLocationDto, List<IAssignableDevice>>();
            var allDeviceActions = new ObservableCollection<DeviceAction>();
            //var locdict = new Dictionary<Location, CalcLocation>();
            builder.Register(x => new CalcLoadTypeDtoDictionary(new Dictionary<VLoadType, CalcLoadTypeDto>())).As<CalcLoadTypeDtoDictionary>()
                .SingleInstance();
            builder.Register(x => new CalcLoadTypeDictionary(new Dictionary<CalcLoadTypeDto, CalcLoadType>())).As<CalcLoadTypeDictionary>()
                .SingleInstance();

            builder.Register(x => new DeviceCategoryPicker(r, null)).As<IDeviceCategoryPicker>().SingleInstance();
            builder.Register(x => calcParameters).As<CalcParameters>().SingleInstance();
            builder.RegisterType<CalcLocationFactory>().As<CalcLocationFactory>().SingleInstance();
            Mock<IOnlineDeviceActivationProcessor> odapmock = new Mock<IOnlineDeviceActivationProcessor>();
            builder.Register(x => odapmock.Object).As<IOnlineDeviceActivationProcessor>().SingleInstance();
            builder.Register(x => r).As<Random>().SingleInstance();
            builder.RegisterType<CalcLocationDtoFactory>().As<CalcLocationDtoFactory>();
            builder.RegisterType<CalcDeviceFactory>().As<CalcDeviceFactory>().SingleInstance();
            builder.RegisterType<CalcRepo>().As<CalcRepo>().SingleInstance();
            var container = builder.Build();
            using var scope = container.BeginLifetimeScope();
            var calcRepo = scope.Resolve<CalcRepo>();

            var cldt = scope.Resolve<CalcLocationDtoFactory>();
            LocationDtoDict calclocdict = new LocationDtoDict();
            var locdtos = cldt.MakeCalcLocations(locations,
                new HouseholdKey("hh1"),
                EnergyIntensityType.EnergyIntensive,
                deviceLocationDict,
                allDeviceActions,
                calclocdict,
                devcat);

            CalcLocationFactory clf = scope.Resolve<CalcLocationFactory>();
            DtoCalcLocationDict dcl = new DtoCalcLocationDict();
            var clocations = clf.MakeCalcLocations(locdtos, dcl, calcRepo);
            Assert.AreEqual(1, clocations.Count);
            Assert.AreEqual(2, clocations[0].LightDevices.Count);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakeCalcLocationsTestWithDevice()
        {
            var builder = new ContainerBuilder();
            var r = new Random(1);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(2018, 1, 1)
                .SetEndDate(new DateTime(2018, 1, 1, 2, 0, 0)).SetSettlingDays(0).EnableShowSettlingPeriod();
            var picker = new DeviceCategoryPicker(r, null);
            builder.Register(x => picker).As<DeviceCategoryPicker>().SingleInstance();
            //var nr = new NormalRandom(0, 1, r);
            var locations = new List<Location>();
            var loc = new Location("loc", 1, string.Empty, Guid.NewGuid().ToStrGuid());
            locations.Add(loc);
            var devices = new ObservableCollection<RealDevice>();
            var dc = new DeviceCategory("dc", -1, string.Empty, false, devices, Guid.NewGuid().ToStrGuid(), 1, true);
            List<DeviceCategoryDto> devcat = new List<DeviceCategoryDto> {
                new DeviceCategoryDto(dc.FullPath, Guid.NewGuid().ToStrGuid())
            };
            var rd = new RealDevice("rd", 1, string.Empty, dc, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid(), 1);
            loc.AddDevice(rd, false);
            var deviceLocationDict = new Dictionary<CalcLocationDto, List<IAssignableDevice>>();
            var allDeviceActions = new ObservableCollection<DeviceAction>();

            //CalcLoadTypeDictionary cltd = CalcLoadTypeFactory.MakeLoadTypes(new ObservableCollection<VLoadType>(),calcParameters.InternalStepsize, calcParameters.LoadTypePriority);
            builder.Register(x => new DateStampCreator(x.Resolve<CalcParameters>())).As<DateStampCreator>().SingleInstance();
            builder.Register(x => new CalcLoadTypeDtoDictionary(new Dictionary<VLoadType, CalcLoadTypeDto>())).As<CalcLoadTypeDtoDictionary>()
                .SingleInstance();
            builder.Register(x => new CalcLoadTypeDictionary(new Dictionary<CalcLoadTypeDto, CalcLoadType>())).As<CalcLoadTypeDictionary>()
                .SingleInstance();
            builder.Register(x => new DeviceCategoryPicker(r, null)).As<IDeviceCategoryPicker>().SingleInstance();
            builder.Register(_ => calcParameters).As<CalcParameters>().SingleInstance();
            //builder.RegisterType<CalcLocationFactory>().As<CalcLocationFactory>().SingleInstance();
            Mock<IOnlineDeviceActivationProcessor> odapmock = new Mock<IOnlineDeviceActivationProcessor>();
            builder.Register(x => odapmock.Object).As<IOnlineDeviceActivationProcessor>().SingleInstance();
            builder.Register(x => r).As<Random>().SingleInstance();
            builder.RegisterType<CalcDeviceFactory>().As<CalcDeviceFactory>().SingleInstance();
            builder.RegisterType<CalcLocationFactory>().As<CalcLocationFactory>().SingleInstance();
            builder.RegisterType<CalcLocationDtoFactory>().As<CalcLocationDtoFactory>();
            builder.RegisterType<CalcRepo>().As<CalcRepo>().SingleInstance();
            var container = builder.Build();
            using var scope = container.BeginLifetimeScope();
            var calcRepo = scope.Resolve<CalcRepo>();
            var cldt = scope.Resolve<CalcLocationDtoFactory>();
            LocationDtoDict calclocdict = new LocationDtoDict();
            var locdtos = cldt.MakeCalcLocations(locations,
                new HouseholdKey("HH1"),
                EnergyIntensityType.EnergyIntensive,
                deviceLocationDict,
                allDeviceActions,
                calclocdict,
                devcat);

            //CalcDeviceFactory cdf = scope.Resolve<CalcDeviceFactory>();
            CalcLocationFactory clf = scope.Resolve<CalcLocationFactory>();
            DtoCalcLocationDict dtl = new DtoCalcLocationDict();
            var clocations = clf.MakeCalcLocations(locdtos, dtl, calcRepo);
            Assert.AreEqual(1, clocations.Count);
            Assert.AreEqual(1, clocations[0].LightDevices.Count);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakeCalcLocationsTestWithDeviceCategory()
        {
            using WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var builder = new ContainerBuilder();
            var r = new Random(1);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(2018, 1, 1)
                .SetEndDate(new DateTime(2018, 1, 1, 2, 0, 0)).SetSettlingDays(0).EnableShowSettlingPeriod();
            //CalcFactoryParameters.SetSkipChecking(true);
            //var nr = new NormalRandom(0, 1, r);
            var locations = new List<Location>();
            var loc = new Location("loc", 1, string.Empty, Guid.NewGuid().ToStrGuid());
            locations.Add(loc);
            var devices = new ObservableCollection<RealDevice>();

            var dc = new DeviceCategory("dc", -1, string.Empty, false, devices, Guid.NewGuid().ToStrGuid(), 1, true);
            var rd = new RealDevice("rd", 1, string.Empty, dc, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid(), 1);
            var rd2 = new RealDevice("rd2", 1, string.Empty, dc, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid(), 1);
            dc.SubDevices.Add(rd);
            loc.AddDevice(dc, false);
            loc.AddDevice(rd2, false);
            var deviceLocationDict = new Dictionary<CalcLocationDto, List<IAssignableDevice>>();

            List<DeviceCategoryDto> devcat = new List<DeviceCategoryDto> {
                new DeviceCategoryDto(dc.FullPath, Guid.NewGuid().ToStrGuid())
            };
            devices.Add(rd);
            devices.Add(rd2);
            //var dict =new Dictionary<CalcLocation, List<IAssignableDevice>>();
            var allDeviceActions = new ObservableCollection<DeviceAction>();
            //var locdict = new Dictionary<Location, CalcLocation>();
            builder.Register(x => new CalcLoadTypeDtoDictionary(new Dictionary<VLoadType, CalcLoadTypeDto>())).As<CalcLoadTypeDtoDictionary>()
                .SingleInstance();
            builder.Register(x => new CalcLoadTypeDictionary(new Dictionary<CalcLoadTypeDto, CalcLoadType>())).As<CalcLoadTypeDictionary>()
                .SingleInstance();
            builder.Register(x => new DeviceCategoryPicker(r, null)).As<IDeviceCategoryPicker>().SingleInstance();
            builder.Register(x => calcParameters).As<CalcParameters>().SingleInstance();
            //builder.RegisterType<CalcLocationFactory>().As<CalcLocationFactory>().SingleInstance();
            Mock<IOnlineDeviceActivationProcessor> odapmock = new Mock<IOnlineDeviceActivationProcessor>();
            builder.Register(x => odapmock.Object).As<IOnlineDeviceActivationProcessor>().SingleInstance();
            builder.Register(x => r).As<Random>().SingleInstance();
            var idl = wd.InputDataLogger;
            builder.Register(x => idl).As<IInputDataLogger>().SingleInstance();
            string path = wd.WorkingDirectory;
            builder.Register(x => new FileFactoryAndTracker(path, "HH1", idl)).As<FileFactoryAndTracker>()
                .SingleInstance();
            builder.Register(x => new SqlResultLoggingService(path)).As<SqlResultLoggingService>().SingleInstance();
            builder.Register(x => new DateStampCreator(x.Resolve<CalcParameters>())).As<DateStampCreator>().SingleInstance();
            builder.Register(x => new DateStampCreator(x.Resolve<CalcParameters>())).As<DateStampCreator>().SingleInstance();
            builder.Register(x => new OnlineLoggingData(x.Resolve<DateStampCreator>(), x.Resolve<IInputDataLogger>(), x.Resolve<CalcParameters>()))
                .As<OnlineLoggingData>().SingleInstance();
            builder.Register(x => new LogFile(calcParameters,
                x.Resolve<FileFactoryAndTracker>())).As<ILogFile>().SingleInstance();
            builder.RegisterType<CalcDeviceFactory>().As<CalcDeviceFactory>().SingleInstance();
            builder.RegisterType<CalcLocationFactory>().As<CalcLocationFactory>().SingleInstance();
            builder.RegisterType<CalcPersonFactory>().As<CalcPersonFactory>().SingleInstance();
            builder.RegisterType<CalcModularHouseholdFactory>().As<CalcModularHouseholdFactory>().SingleInstance();
            builder.RegisterType<CalcLocationDtoFactory>().As<CalcLocationDtoFactory>();
            builder.RegisterType<InputDataLogger>().As<InputDataLogger>().SingleInstance();
            builder.RegisterType<CalcRepo>().As<CalcRepo>().SingleInstance();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope()) {
                var cldt = scope.Resolve<CalcLocationDtoFactory>();
                var calcRepo = scope.Resolve<CalcRepo>();
                LocationDtoDict calclocdict = new LocationDtoDict();
                var locdtos = cldt.MakeCalcLocations(locations,
                    new HouseholdKey("HH1"),
                    EnergyIntensityType.EnergySaving,
                    deviceLocationDict,
                    allDeviceActions,
                    calclocdict,
                    devcat);

                CalcLocationFactory clf = scope.Resolve<CalcLocationFactory>();
                DtoCalcLocationDict dtl = new DtoCalcLocationDict();
                var clocations = clf.MakeCalcLocations(locdtos, dtl,calcRepo);

                Assert.AreEqual(1, clocations.Count);
                Assert.AreEqual(2, clocations[0].LightDevices.Count);
                foreach (var device in clocations[0].LightDevices) {
                    Logger.Info(device.Name);
                }
            }

            wd.CleanUp();
        }

        public CalcLocationFactoryTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}