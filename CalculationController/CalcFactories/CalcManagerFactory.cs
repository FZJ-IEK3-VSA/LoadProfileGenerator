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
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationController.Helpers;
using CalculationController.InputLoggers;
using CalculationController.Integrity;
using CalculationController.Queue;
using CalculationEngine;
using CalculationEngine.Helper;
using CalculationEngine.HouseElements;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using Common.SQLResultLogging.Loggers;

namespace CalculationController.CalcFactories
{
    public class CalcManagerFactory
    {
        public static bool DoIntegrityRun { get; set; } = true;

        private readonly Simulator sim;
        private readonly CalcParameters calcParameters;

        public CalcManagerFactory(Simulator simulator, CalcParameters parameters)
        {
            sim = simulator;
            calcParameters = parameters;
            SharedObjectsScope = RegisterSharedObjects();
        }

        /// <summary>
        /// Scope that contains the shared objects that are used by all CalcManagers.
        /// Calling Dispose() on this also disposes all contained objects and should only
        /// be done once all CalcManagers created by this factory are disposed.
        /// </summary>
        public ILifetimeScope SharedObjectsScope { get; }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht")]
        [SuppressMessage("ReSharper", "ThrowingSystemException")]
        [JetBrains.Annotations.NotNull]
        public CalcManager GetCalcManager([JetBrains.Annotations.NotNull] CalcStartParameterSet csps)
        {
            if (sim == null) {
                throw new LPGException("Simulation was null");
            }

            if (csps.CalcParams != calcParameters)
                throw new LPGException("Invalid CalcParameters: a CalcManagerFactory can only used with the same set of CalcParameters");

            if (csps.CalcOptions.Contains(CalcOption.LogAllMessages))
            {
                Logger.Get().StartCollectingAllMessages();
            }

            Logger.Info("Starting the calculation of " + csps.CalcTarget.Name);
            try {
                csps.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " Integrity Check");
                if (DoIntegrityRun) {
                    SimIntegrityChecker.Run(sim, CheckingOptions.FromStartParameters(csps));
                }

                if (csps.CalcTarget.CalcObjectType == CalcObjectType.House && (csps.LoadTypePriority == LoadTypePriority.RecommendedForHouseholds ||
                                                                               csps.LoadTypePriority == LoadTypePriority.Mandatory)) {
                    throw new DataIntegrityException(
                        "You are trying to calculate a house with only the load types for a household. This would mess up the warm water calculations. Please fix the load type selection.");
                }
            }
            finally {
                csps.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " Integrity Check");
            }

            var chh = csps.CalcTarget as ModularHousehold;
            var ds = GetDeviceSelection(csps, csps.CalcTarget, chh);

            // create a new scope for this CalcManager as a child of the common scope, inheriting all its registered objects
            ILifetimeScope scope = SharedObjectsScope.BeginLifetimeScope(builder => RegisterEverything(csps, builder, ds));

            CalcManager? cm = null;
            try
            {
                csps.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " Generating Model");
                var calcRepo = PrepareCalculation(csps, scope, out var dtoltdict, out var dls, out var variableRepository);

                //_calcParameters.Logfile = cm.Logfile;
                //_calcParameters.NormalDistributedRandom = normalDistributedRandom;
                //_calcParameters.RandomGenerator = randomGenerator;
                //_calcParameters.Odap = cm.Odap;
                //_calcParameters.EnergyIntensity = csps.EnergyIntensity;
                // no vacation times needed for the light array
                CalcObjectType cot;
                ICalcAbleObject ch;
                CalcVariableDtoFactory cvrdto = scope.Resolve<CalcVariableDtoFactory>();
                CalcDeviceTaggingSets devicetaggingSets = scope.Resolve<CalcDeviceTaggingSets>();
                var affordanceTaggingSets = scope.Resolve<List<CalcAffordanceTaggingSetDto>>();
                if (csps.CalcTarget.GetType() == typeof(House)) {
                    ch = MakeCalcHouseObject(sim, csps, csps.CalcTarget, scope, cvrdto, variableRepository, affordanceTaggingSets, out cot, calcRepo);
                    CalcHouse chd = (CalcHouse)ch;
                    if (chd.EnergyStorages != null) {
                        foreach (var calcEnergyStorage in chd.EnergyStorages) {
                            foreach (var taggingSet in devicetaggingSets.AllCalcDeviceTaggingSets) {
                                taggingSet.AddTag(calcEnergyStorage.Name, "Energy Storage");
                            }
                        }
                    }

                }
                else if (csps.CalcTarget.GetType() == typeof(ModularHousehold)) {
                    ch = MakeCalcHouseholdObject(sim, csps, csps.CalcTarget, scope, cvrdto, variableRepository, out cot, affordanceTaggingSets, calcRepo);
                }
                else {
                    throw new LPGException("The type " + csps.CalcTarget.GetType() + " is missing!");
                }

                if (calcRepo.CalcParameters.Options.Contains(CalcOption.DeviceTaggingSets)) {
                    calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, devicetaggingSets.AllCalcDeviceTaggingSets);
                }

                CalcObjectInformation coi = new CalcObjectInformation(cot, ch.Name, csps.ResultPath);
                if (calcRepo.CalcParameters.Options.Contains(CalcOption.HouseholdContents)) {
                    calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, coi);
                }

                //this logger doesnt save json, but strings!
                calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, csps);
                calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, dtoltdict.GetLoadTypeDtos());
                cm = new CalcManager(scope, ch, csps.ResultPath, dls, variableRepository, calcRepo);
                ch.Init(dls);
                CalcManager.ExitCalcFunction = false;

                //LogSeed(calcParameters.ActualRandomSeed, lf.FileFactoryAndTracker, calcParameters);

                return cm;
            }
            catch {
                cm?.Dispose();
                throw;
            }
            finally {
                csps.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " Generating Model");
            }
        }

        [JetBrains.Annotations.NotNull]
        private CalcRepo PrepareCalculation([JetBrains.Annotations.NotNull] CalcStartParameterSet csps, [JetBrains.Annotations.NotNull] ILifetimeScope scope,
                                                   [JetBrains.Annotations.NotNull] out CalcLoadTypeDtoDictionary dtoltdict,
                                                   [JetBrains.Annotations.NotNull] out DayLightStatus dls,
                                                   [JetBrains.Annotations.NotNull] out CalcVariableRepository variableRepository
                                                   )
        {
            Logger.LogRAMUsage($"Start PrepareCalculation");
            CalcRepo calcRepo = scope.Resolve<CalcRepo>();
            var inputDataLogger = scope.Resolve<IInputDataLogger>();
            inputDataLogger.Save(Constants.GeneralHouseholdKey, calcRepo.CalcParameters);
            inputDataLogger.Save(Constants.GeneralHouseholdKey, csps.TemperatureProfile);

            dtoltdict = scope.Resolve<CalcLoadTypeDtoDictionary>();

            var affordanceTaggingSets = scope.Resolve<List<CalcAffordanceTaggingSetDto>>();
            if (calcRepo.CalcParameters.Options.Contains(CalcOption.AffordanceTaggingSets)) {
                inputDataLogger.Save(affordanceTaggingSets);
            }

            calcRepo.FileFactoryAndTracker.RegisterGeneralHouse();
            dls = scope.Resolve<DayLightStatus>();
            if (calcRepo.CalcParameters.Options.Contains(CalcOption.DaylightTimesList)) {
                inputDataLogger.Save(Constants.GeneralHouseholdKey, dls);
            }
            variableRepository = scope.Resolve<CalcVariableRepository>();
            return calcRepo;
        }

        [CanBeNull]
        private static DeviceSelection GetDeviceSelection([JetBrains.Annotations.NotNull] CalcStartParameterSet csps, [JetBrains.Annotations.NotNull] ICalcObject hh, [CanBeNull] ModularHousehold chh)
        {
// device selection
            DeviceSelection ds = null;
            if (csps.DeviceSelection != null) {
                ds = csps.DeviceSelection;
            }
            else {
                if (chh != null) {
                    ds = chh.DeviceSelection;
                }

                if (hh.CalcObjectType == CalcObjectType.House) {
                    var house = (House)hh;
                    foreach (var houseHousehold in house.Households) {
                        var houseModularHousehold = houseHousehold.CalcObject as ModularHousehold;

                        if (houseModularHousehold?.DeviceSelection != null) {
                            ds = houseModularHousehold.DeviceSelection;
                        }
                    }
                }
            }

            return ds;
        }

        [JetBrains.Annotations.NotNull]
        private static ICalcAbleObject MakeCalcHouseObject([JetBrains.Annotations.NotNull] Simulator sim,
                                                           [JetBrains.Annotations.NotNull] CalcStartParameterSet csps, [JetBrains.Annotations.NotNull] ICalcObject hh,
                                                           [JetBrains.Annotations.NotNull] ILifetimeScope scope,
                                                           [JetBrains.Annotations.NotNull] CalcVariableDtoFactory cvrdto, [JetBrains.Annotations.NotNull] CalcVariableRepository variableRepository,
                                                           [JetBrains.Annotations.NotNull] List<CalcAffordanceTaggingSetDto> affordanceTaggingSets,
                                                           out CalcObjectType cot, [JetBrains.Annotations.NotNull] CalcRepo calcRepo)
        {
            Logger.LogRAMUsage($"Start MakeCalcHouseObject");
            var house =(House) hh;
            calcRepo.FileFactoryAndTracker.RegisterHousehold(Constants.HouseKey, "House Infrastructure",
                HouseholdKeyType.House, "House Infrastructure",house.Name,house.Description);
            var housedtoFac = scope.Resolve<CalcHouseDtoFactory>();
            var housedto = housedtoFac.MakeHouseDto(sim, house, csps.TemperatureProfile,
                csps.GeographicLocation,csps.EnergyIntensity);
            Logger.LogRAMUsage($"Finished MakeHouseDto");
            foreach (HouseholdKeyEntry entry in housedto.GetHouseholdKeyEntries()) {
                calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, entry);
            }

            var convertedAutoDevList = housedto.AutoDevs.ConvertAll(x => (IHouseholdKey)x).ToList();

            if (calcRepo.CalcParameters.Options.Contains(CalcOption.HouseholdContents)) {
                if (convertedAutoDevList.Count > 0) {
                    calcRepo.InputDataLogger.SaveList<CalcAutoDevDto>(convertedAutoDevList);
                }
                calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, housedto);
            }

            var chf = scope.Resolve<CalcHouseFactory>();
            RegisterAllDtoVariables(cvrdto, variableRepository);
            ICalcAbleObject ch = chf.MakeCalcHouse(housedto,calcRepo, affordanceTaggingSets);
            cot = CalcObjectType.House;
            return ch;
        }

        [JetBrains.Annotations.NotNull]
        private static ICalcAbleObject MakeCalcHouseholdObject([JetBrains.Annotations.NotNull] Simulator sim, [JetBrains.Annotations.NotNull] CalcStartParameterSet csps, [JetBrains.Annotations.NotNull] ICalcObject hh,
                                                          [JetBrains.Annotations.NotNull] ILifetimeScope scope,
                                                          [JetBrains.Annotations.NotNull] CalcVariableDtoFactory cvrdto,
                                                          [JetBrains.Annotations.NotNull] CalcVariableRepository variableRepository,
                                                          out CalcObjectType cot,
                                                          [JetBrains.Annotations.NotNull] List<CalcAffordanceTaggingSetDto> affordanceTaggingSets,
                                                          [JetBrains.Annotations.NotNull] CalcRepo calcRepo)
        {
            var cmhdf = scope.Resolve<CalcModularHouseholdDtoFactory>();
            HouseholdKey householdKey = new HouseholdKey("HH1");
            CalcHouseholdDto dto = cmhdf.MakeCalcModularHouseholdDto(sim, (ModularHousehold)hh,
                csps.TemperatureProfile, householdKey, csps.GeographicLocation,
                out _, csps.TransportationDeviceSet, csps.TravelRouteSet,
                csps.EnergyIntensity, csps.ChargingStationSet);
            var cmhf = scope.Resolve<CalcModularHouseholdFactory>();
            /*foreach (var v in dto.CalcVariables)
                            {
                                variableRepository.RegisterVariable(new CalcVariable(v.Name, v.Guid, v.Value, v.LocationName, v.LocationGuid, v.HouseholdKey));
                            }*/
            foreach (HouseholdKeyEntry entry in dto.GetHouseholdKeyEntries()) {
                calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, entry);
            }

            calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, dto);
            RegisterAllDtoVariables(cvrdto, variableRepository);
            ICalcAbleObject ch = cmhf.MakeCalcModularHousehold(dto, out _,null,null, affordanceTaggingSets, calcRepo);
            cot = CalcObjectType.ModularHousehold;
            return ch;
        }

        /// <summary>
        /// Register common objects that are shared among all CalcManagers.
        /// </summary>
        /// <returns>scope for resolving the objects</returns>
        private ILifetimeScope RegisterSharedObjects()
        {
            ContainerBuilder builder = new();

            builder.RegisterInstance(calcParameters);
            builder.RegisterInstance(CalcLoadTypeDtoFactory.MakeLoadTypes(sim.LoadTypes.Items, calcParameters.InternalStepsize,
                        calcParameters.LoadTypePriority));
            builder.RegisterType<AffordanceTaggingSetFactory>().SingleInstance();
            builder.Register(x => x.Resolve<AffordanceTaggingSetFactory>().GetAffordanceTaggingSets(sim)).SingleInstance();
            builder.Register(x => CalcLoadTypeFactory.MakeLoadTypes(x.Resolve<CalcLoadTypeDtoDictionary>())).SingleInstance();
            builder.RegisterType<AvailabilityDtoRepository>().SingleInstance();
            builder.Register(x => new DateStampCreator(x.Resolve<CalcParameters>())).SingleInstance();
            builder.RegisterType<CalcTransportationDtoFactory>();

            var container = builder.Build();
            return container;
        }

        /// <summary>
        /// Register the objects that are required only for the CalcManager that is
        /// being created at the moment.
        /// </summary>
        /// <param name="csps">full set of calculation parameters</param>
        /// <param name="builder">container builder to register objects</param>
        /// <param name="ds">device selection for this simulation target</param>
        private void RegisterEverything(CalcStartParameterSet csps, ContainerBuilder builder, DeviceSelection? ds)
        {
            // CalcVariableDtoFactory stores all CalcVariableDto objects created with it and therefore must be rebuilt for every house
            builder.RegisterType<CalcVariableDtoFactory>().SingleInstance();

            Random rnd = new(csps.RandomSeed);
            builder.RegisterInstance(rnd);
            builder.RegisterInstance(csps.CalculationProfiler);
            builder.RegisterInstance(new NormalRandom(0, 0.1, rnd));
            builder.RegisterType<OnlineDeviceActivationProcessor>().As<IOnlineDeviceActivationProcessor>().SingleInstance();
            builder.RegisterType<CalcHouseFactory>().SingleInstance();
            builder.RegisterType<CalcManager>().SingleInstance();
            builder.RegisterType<CalcDeviceTaggingSetFactory>().SingleInstance();
            builder.Register(x => x.Resolve<CalcDeviceTaggingSetFactory>().GetDeviceTaggingSets(sim, csps.CalcTarget.CalculatePersonCount())).SingleInstance();
            builder.RegisterInstance<IDeviceCategoryPicker>(new DeviceCategoryPicker(rnd, ds));
            builder.RegisterType<CalcModularHouseholdFactory>().SingleInstance();
            builder.RegisterType<CalcLocationFactory>().SingleInstance();
            builder.RegisterType<CalcPersonFactory>().SingleInstance();
            builder.RegisterType<CalcDeviceFactory>().SingleInstance();
            builder.RegisterType<CalcRepo>().SingleInstance();
            builder.RegisterType<CalcAffordanceFactory>().SingleInstance();
            builder.RegisterType<CalcTransportationFactory>().SingleInstance();

            builder.RegisterType<VacationDtoFactory>().SingleInstance();
            builder.RegisterType<CalcVariableRepository>().SingleInstance();
            builder.RegisterType<TemperatureDataLogger>().SingleInstance();
            builder.Register(x => new FileFactoryAndTracker(csps.ResultPath, csps.CalcTarget.Name, x.Resolve<IInputDataLogger>()))
                .As<FileFactoryAndTracker>().SingleInstance();
            builder.Register(_ => ResultLoggingFactory.CreateResultLoggingService(csps.ResultPath)).SingleInstance();
            builder.RegisterType<OnlineLoggingData>().As<IOnlineLoggingData>().SingleInstance();
            builder.Register(x => new LogFile(calcParameters, x.Resolve<FileFactoryAndTracker>())).As<ILogFile>().SingleInstance();
            builder.RegisterType<CalcPersonDtoFactory>();
            builder.RegisterType<CalcDeviceDtoFactory>();
            builder.RegisterType<CalcLocationDtoFactory>();
            builder.RegisterType<CalcAffordanceDtoFactory>();
            builder.RegisterType<CalcHouseDtoFactory>();
            builder.RegisterType<CalcModularHouseholdDtoFactory>();
            //data save loggers + input data loggers
            builder.RegisterType<InputDataLogger>().As<IInputDataLogger>().SingleInstance();
            builder.RegisterType<CalcParameterLogger>().As<IDataSaverBase>();
            builder.RegisterType<DeviceTaggingSetLogger>().As<IDataSaverBase>();
            builder.RegisterType<HouseholdDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<DaylightTimesLogger>().As<IDataSaverBase>();
            builder.RegisterType<TemperatureDataLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcStartParameterSetLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcLoadTypeDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<ActionEntryLogger>().As<IDataSaverBase>();
            builder.RegisterType<FlexibilityDataLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcPersonDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcDeviceDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcDeviceArchiveDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcAutoDevDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcVariableDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<HouseholdKeyLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcObjectInformationLogger>().As<IDataSaverBase>();
            builder.RegisterType<BodilyActivityLevelStatisticsLogger>().As<IDataSaverBase>();
            builder.RegisterType<ResultFileEntryLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcAffordanceDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcAffordanceTaggingSetDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<DeviceActivationEntryLogger>().As<IDataSaverBase>();
            builder.RegisterType<ColumnEntryLogger>().As<IDataSaverBase>();
            builder.RegisterType<BridgeDayEntryLogger>().As<IDataSaverBase>();
            builder.RegisterType<HouseDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<LocationEntryLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcSiteDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcTransportationDeviceDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcTravelRouteDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationEventLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationStatusLogger>().As<IDataSaverBase>();
            builder.RegisterType<PersonStatusLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationStateEntryLogger>().As<IDataSaverBase>();
            builder.RegisterType<ChargingStationStateLogger>().As<IDataSaverBase>();
            builder.RegisterType<VariableEntryLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationDeviceStatisticsLogger>().As<IDataSaverBase>();
            builder.RegisterType<TransportationDeviceChoiceLogger>().As<IDataSaverBase>();
            builder.RegisterType<AffordanceEnergyUseLogger>().As<IDataSaverBase>();
            //builder.Register(x=> x.Resolve<CalcVariableDtoFactory>().GetRepository()).As<CalcVariableRepository>().SingleInstance();
            builder.Register(_ => MakeLightNeededArray(csps.GeographicLocation, csps.TemperatureProfile,
                rnd, [], csps.CalcTarget.Name, calcParameters)).SingleInstance();
        }

        private static void RegisterAllDtoVariables([JetBrains.Annotations.NotNull] CalcVariableDtoFactory cvrdto, [JetBrains.Annotations.NotNull] CalcVariableRepository variableRepository)
        {
            foreach (var v in cvrdto.GetAllVariableDtos()) {
                variableRepository.RegisterVariable(new CalcVariable(v.Name, v.Guid, v.Value, v.LocationName,
                    v.LocationGuid, v.HouseholdKey));
            }
        }

        [JetBrains.Annotations.NotNull]
        private DayLightStatus MakeLightNeededArray([JetBrains.Annotations.NotNull] GeographicLocation geographicLocation, [JetBrains.Annotations.NotNull] TemperatureProfile tp,
            [JetBrains.Annotations.NotNull] Random r, [JetBrains.Annotations.NotNull][ItemNotNull] List<VacationTimeframe> vacations, [JetBrains.Annotations.NotNull] string householdname, [JetBrains.Annotations.NotNull] CalcParameters calcParameters)
        {
            if (geographicLocation.LightTimeLimit == null) {
                throw new DataIntegrityException("Geographic Location has no definition for the light time",
                    geographicLocation);
            }
            if (geographicLocation.LightTimeLimit.RootEntry==null) {
                throw new LPGException("Root entry of the light array was null");
            }

            var br =
                geographicLocation.LightTimeLimit.RootEntry.GetOneYearArray(
                    calcParameters.InternalStepsize,
                    calcParameters.InternalStartTime,
                    calcParameters.InternalEndTime,
                    tp, geographicLocation, r, vacations, householdname, out _, 0, 0, 0, 0);
            return new DayLightStatus(br.Not());
        }
    }
}
