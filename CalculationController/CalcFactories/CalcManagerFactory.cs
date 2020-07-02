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
using System.IO;
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

namespace CalculationController.CalcFactories
{
    using Common.SQLResultLogging.Loggers;

    [SuppressMessage("ReSharper", "CatchAllClause")]
    public class CalcManagerFactory
    {
        public static bool DoIntegrityRun { get; set; } = true;

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht")]
        [SuppressMessage("ReSharper", "ThrowingSystemException")]
        [NotNull]
        public CalcManager GetCalcManager([NotNull] Simulator sim,
                [NotNull] CalcStartParameterSet csps,  bool forceRandom)
            //, ICalcObject hh,
            //bool forceRandom, TemperatureProfile temperatureProfile,
            //GeographicLocation geographicLocation, EnergyIntensityType energyIntensity,
            //string fileVersion, LoadTypePriority loadTypePriority, [CanBeNull] DeviceSelection deviceSelection,
            //TransportationDeviceSet transportationDeviceSet, TravelRouteSet travelRouteSet,
            //)
        {
            csps.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() +  " Initializing");
            if (sim == null) {
                throw new LPGException("Simulation was null");
            }

            if (csps.CalcOptions.Contains(CalcOption.LogAllMessages)) {
                Logger.Get().StartCollectingAllMessages();
            }

            CalcManager cm = null;
            Logger.Info("Starting the calculation of " + csps.CalcTarget.Name);
            try
            {
                if (DoIntegrityRun) {
                    SimIntegrityChecker.Run(sim);
                }

                if (csps.CalcTarget.CalcObjectType == CalcObjectType.House &&
                    (csps.LoadTypePriority == LoadTypePriority.RecommendedForHouseholds ||
                     csps.LoadTypePriority == LoadTypePriority.Mandatory)) {
                    throw new DataIntegrityException(
                        "You are trying to calculate a house with only the load types for a household. This would mess up the warm water calculations. Please fix the load type selection.");
                }

                var chh = csps.CalcTarget as ModularHousehold;

                var ds = GetDeviceSelection(csps, csps.CalcTarget, chh);

                var cpf = new CalcParametersFactory();
                var calcParameters = cpf.MakeCalculationParametersFromConfig(csps,forceRandom);

                var sqlFileName = Path.Combine(csps.ResultPath, "Results.sqlite");
                var builder = new ContainerBuilder();
                RegisterEverything(sim, csps.ResultPath, csps, csps.CalcTarget, builder,
                    sqlFileName, calcParameters, ds);
                csps.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " Initializing");
                csps.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " Generating Model");
                var container = builder.Build();
                using (var scope = container.BeginLifetimeScope())
                {
                    var calcRepo = PrepareCalculation(sim, csps, scope,
                         out var dtoltdict,  out var dls, out var variableRepository);
                    cm = new CalcManager(csps.ResultPath,
                        //hh.Name,
                        //householdPlans,
                        //csps.LPGVersion,
                        calcParameters.ActualRandomSeed,dls, variableRepository,calcRepo
                        //scope.Resolve<SqlResultLoggingService>()
                        );
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
                    if (csps.CalcTarget.GetType() == typeof(House)) {
                        ch = MakeCalcHouseObject(sim, csps, csps.CalcTarget,  scope,
                            cvrdto, variableRepository, out cot, calcRepo);
                        CalcHouse chd =(CalcHouse) ch;
                        if (chd.EnergyStorages != null) {
                            foreach (var calcEnergyStorage in chd.EnergyStorages) {
                                foreach (var taggingSet in devicetaggingSets.AllCalcDeviceTaggingSets) {
                                    taggingSet.AddTag(calcEnergyStorage.Name,"Energy Storage");
                                }
                            }
                        }

                    }
                    else if (csps.CalcTarget.GetType() == typeof(ModularHousehold)) {
                        ch = MakeCalcHouseholdObject(sim, csps, csps.CalcTarget, scope,  cvrdto, variableRepository, out cot,calcRepo);
                    }
                    else
                    {
                        throw new LPGException("The type " + csps.CalcTarget.GetType() + " is missing!");
                    }

                    if (calcRepo.CalcParameters.Options.Contains(CalcOption.HouseholdContents)) {
                        calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey,
                            devicetaggingSets.AllCalcDeviceTaggingSets);
                    }

                    CalcObjectInformation coi = new CalcObjectInformation(cot,ch.Name,csps.ResultPath);
                    if (calcRepo.CalcParameters.Options.Contains(CalcOption.HouseholdContents)) {
                        calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, coi);
                    }

                    //this logger doesnt save json, but strings!
                    calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, csps);
                    calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, dtoltdict.GetLoadTypeDtos());
                    cm.SetCalcObject(ch);
                    CalcManager.ExitCalcFunction = false;

                    //LogSeed(calcParameters.ActualRandomSeed, lf.FileFactoryAndTracker, calcParameters);
                    csps.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " Generating Model");
                    return cm;
                }
            }
            catch
            {
                cm?.Dispose();
                throw;
            }
        }

        [NotNull]
        private static CalcRepo PrepareCalculation([NotNull] Simulator sim, [NotNull] CalcStartParameterSet csps,
                                                   [NotNull] ILifetimeScope scope,
                                                   [NotNull] out CalcLoadTypeDtoDictionary dtoltdict,
                                                   [NotNull] out DayLightStatus dls,
                                                   [NotNull] out CalcVariableRepository variableRepository
                                                   )
        {
            CalcRepo calcRepo = scope.Resolve<CalcRepo>();
            var inputDataLogger = scope.Resolve<IInputDataLogger>();
            inputDataLogger.Save(Constants.GeneralHouseholdKey, calcRepo.CalcParameters);
            inputDataLogger.Save(Constants.GeneralHouseholdKey, csps.TemperatureProfile);

            dtoltdict = scope.Resolve<CalcLoadTypeDtoDictionary>();

            var affordanceTaggingSetFactory = scope.Resolve<AffordanceTaggingSetFactory>();
            var affordanceTaggingSets = affordanceTaggingSetFactory.GetAffordanceTaggingSets(sim);
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
        private static DeviceSelection GetDeviceSelection([NotNull] CalcStartParameterSet csps, [NotNull] ICalcObject hh, [CanBeNull] ModularHousehold chh)
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

        [NotNull]
        private static ICalcAbleObject MakeCalcHouseObject([NotNull] Simulator sim,
                                                           [NotNull] CalcStartParameterSet csps, [NotNull] ICalcObject hh,
                                                      [NotNull] ILifetimeScope scope,
                                                      [NotNull] CalcVariableDtoFactory cvrdto, [NotNull] CalcVariableRepository variableRepository,
                                                      out CalcObjectType cot, [NotNull] CalcRepo calcRepo)
        {
            var house =(House) hh;
            calcRepo.FileFactoryAndTracker.RegisterHousehold(Constants.HouseKey, "House Infrastructure",
                HouseholdKeyType.House, "House Infrastructure",house.Name,house.Description);
            var housedtoFac = scope.Resolve<CalcHouseDtoFactory>();
            var housedto = housedtoFac.MakeHouseDto(sim, house, csps.TemperatureProfile,
                csps.GeographicLocation,csps.EnergyIntensity);
            foreach (HouseholdKeyEntry entry in housedto.GetHouseholdKeyEntries()) {
                calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, entry);
            }

            var convertedAutoDevList = housedto.AutoDevs.ConvertAll(x => (IHouseholdKey)x).ToList();

            if (calcRepo.CalcParameters.Options.Contains(CalcOption.HouseholdContents)) {
                calcRepo.InputDataLogger.SaveList(convertedAutoDevList);
                calcRepo.InputDataLogger.Save(Constants.GeneralHouseholdKey, housedto);
            }

            var chf = scope.Resolve<CalcHouseFactory>();
            RegisterAllDtoVariables(cvrdto, variableRepository);
            ICalcAbleObject ch = chf.MakeCalcHouse(housedto,calcRepo);
            cot = CalcObjectType.House;
            return ch;
        }

        [NotNull]
        private static ICalcAbleObject MakeCalcHouseholdObject([NotNull] Simulator sim, [NotNull] CalcStartParameterSet csps, [NotNull] ICalcObject hh,
                                                          [NotNull] ILifetimeScope scope,
                                                          [NotNull] CalcVariableDtoFactory cvrdto,
                                                          [NotNull] CalcVariableRepository variableRepository,
                                                               out CalcObjectType cot,
                                                               [NotNull] CalcRepo calcRepo)
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
            ICalcAbleObject ch = cmhf.MakeCalcModularHousehold(dto, out _,null,null, calcRepo);
            cot = CalcObjectType.ModularHousehold;
            return ch;
        }

        private void RegisterEverything([NotNull] Simulator sim, [NotNull] string resultpath, [NotNull] CalcStartParameterSet csps, [NotNull] ICalcObject hh,
                                          [NotNull] ContainerBuilder builder, [NotNull] string sqlFileName, [NotNull] CalcParameters calcParameters,
                                          [CanBeNull] DeviceSelection ds)
        {
            builder.Register(c => new SqlResultLoggingService(sqlFileName)).As<SqlResultLoggingService>()
                .SingleInstance();
            builder.Register(c => calcParameters).As<CalcParameters>().SingleInstance();
            Random rnd = new Random(calcParameters.ActualRandomSeed);
            builder.Register(c => rnd).As<Random>().SingleInstance();
            builder.Register(c => csps.CalculationProfiler).As<CalculationProfiler>().SingleInstance();
            builder.Register(c => new NormalRandom(0, 0.1, rnd)).As<NormalRandom>().SingleInstance();
            builder.RegisterType<OnlineDeviceActivationProcessor>().As<IOnlineDeviceActivationProcessor>()
                .SingleInstance();
            builder.RegisterType<CalcHouseFactory>().As<CalcHouseFactory>().SingleInstance();
            builder.RegisterType<CalcManager>().As<CalcManager>().SingleInstance();
            builder.RegisterType<AffordanceTaggingSetFactory>().As<AffordanceTaggingSetFactory>().SingleInstance();
            builder.Register(x =>
                    CalcLoadTypeDtoFactory.MakeLoadTypes(sim.LoadTypes.Items, calcParameters.InternalStepsize,
                        csps.LoadTypePriority))
                .As<CalcLoadTypeDtoDictionary>().SingleInstance();
            builder.Register(x => CalcLoadTypeFactory.MakeLoadTypes(x.Resolve<CalcLoadTypeDtoDictionary>()))
                .As<CalcLoadTypeDictionary>().SingleInstance();
            //builder.Register(x =>ds).As<DeviceSelection>().SingleInstance();
            builder.Register(x => {
                CalcDeviceTaggingSetFactory ctsf =
                    new CalcDeviceTaggingSetFactory(x.Resolve<CalcParameters>(), x.Resolve<CalcLoadTypeDtoDictionary>());
                return ctsf.GetDeviceTaggingSets(sim, hh.CalculatePersonCount());
            }).As<CalcDeviceTaggingSets>().SingleInstance();
            builder.Register(x => new DeviceCategoryPicker(rnd, ds)).As<IDeviceCategoryPicker>().SingleInstance();
            builder.RegisterType<CalcModularHouseholdFactory>().As<CalcModularHouseholdFactory>().SingleInstance();
            builder.RegisterType<CalcHouseFactory>().As<CalcHouseFactory>().SingleInstance();
            builder.RegisterType<CalcLocationFactory>().As<CalcLocationFactory>().SingleInstance();
            builder.RegisterType<CalcPersonFactory>().As<CalcPersonFactory>().SingleInstance();
            builder.RegisterType<CalcDeviceFactory>().As<CalcDeviceFactory>().SingleInstance();
            builder.RegisterType<CalcRepo>().As<CalcRepo>().SingleInstance();
            builder.RegisterType<CalcAffordanceFactory>().As<CalcAffordanceFactory>().SingleInstance();
            builder.RegisterType<CalcTransportationFactory>().As<CalcTransportationFactory>().SingleInstance();

            builder.RegisterType<CalcVariableDtoFactory>().As<CalcVariableDtoFactory>().SingleInstance();

            builder.RegisterType<VacationDtoFactory>().As<VacationDtoFactory>().SingleInstance();
            builder.RegisterType<CalcVariableRepository>().As<CalcVariableRepository>().SingleInstance();
            builder.RegisterType<TemperatureDataLogger>().As<TemperatureDataLogger>().SingleInstance();
            builder.Register(x => new FileFactoryAndTracker(resultpath, hh.Name, x.Resolve<IInputDataLogger>()))
                .As<FileFactoryAndTracker>().SingleInstance();
            builder.Register(_ => new SqlResultLoggingService(resultpath)).As<SqlResultLoggingService>().SingleInstance();
            builder.Register(x => new DateStampCreator(x.Resolve<CalcParameters>())).As<DateStampCreator>().SingleInstance();
            builder.Register(c => new OnlineLoggingData(c.Resolve<DateStampCreator>(), c.Resolve<IInputDataLogger>(),
                    c.Resolve<CalcParameters>()))
                .As<OnlineLoggingData>().As<IOnlineLoggingData>().SingleInstance();
            builder.Register(x => new LogFile(calcParameters, x.Resolve<FileFactoryAndTracker>())).As<ILogFile>().SingleInstance();
            builder.RegisterType<AffordanceTaggingSetFactory>().As<AffordanceTaggingSetFactory>();
            builder.RegisterType<CalcPersonDtoFactory>().As<CalcPersonDtoFactory>();
            builder.RegisterType<CalcDeviceDtoFactory>().As<CalcDeviceDtoFactory>();
            builder.RegisterType<CalcLocationDtoFactory>().As<CalcLocationDtoFactory>();
            builder.RegisterType<CalcAffordanceDtoFactory>().As<CalcAffordanceDtoFactory>();
            builder.RegisterType<CalcHouseDtoFactory>().As<CalcHouseDtoFactory>();
            builder.RegisterType<CalcModularHouseholdDtoFactory>().As<CalcModularHouseholdDtoFactory>();
            builder.RegisterType<AvailabilityDtoRepository>().As<AvailabilityDtoRepository>().SingleInstance();
            builder.RegisterType<CalcTransportationDtoFactory>().As<CalcTransportationDtoFactory>();
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
            builder.RegisterType<CalcPersonDtoLogger>().As<IDataSaverBase>();
            builder.RegisterType<CalcDeviceDtoLogger>().As<IDataSaverBase>();
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

            builder.RegisterType<AffordanceEnergyUseLogger>().As<IDataSaverBase>();
            //builder.Register(x=> x.Resolve<CalcVariableDtoFactory>().GetRepository()).As<CalcVariableRepository>().SingleInstance();
            builder.Register(x => MakeLightNeededArray(csps.GeographicLocation, csps.TemperatureProfile,
                rnd,
                new List<VacationTimeframe>(), hh.Name, calcParameters)).As<DayLightStatus>().SingleInstance();
        }

        private static void RegisterAllDtoVariables([NotNull] CalcVariableDtoFactory cvrdto, [NotNull] CalcVariableRepository variableRepository)
        {
            foreach (var v in cvrdto.GetAllVariableDtos()) {
                variableRepository.RegisterVariable(new CalcVariable(v.Name, v.Guid, v.Value, v.LocationName,
                    v.LocationGuid, v.HouseholdKey));
            }
        }

        /*
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void LogSeed(int seed, FileFactoryAndTracker fft, CalcParameters calcParameters)
        {
            if (!calcParameters.IsSet(CalcOption.HouseholdContents)) {
                return;
            }

            if (_resultPath == null) {
                throw new LPGException("Resultpath was null.");
            }

            try
            {
                var seedsaver = fft.MakeFile<StreamWriter>("seed.txt",
                    "The seed used for the random number generator in this calculation.",
                    false, ResultFileID.Seed, Constants.GeneralHouseholdKey, TargetDirectory.Root, TimeSpan.FromMinutes(1));
                seedsaver.WriteLine(seed);
                Logger.Debug("The seed was " + seed);
                seedsaver.Close();
            }
            catch (Exception e)
            {
                Logger.Error("While saving the random seed, the following problem occured:" + Environment.NewLine +
                             e.Message);
                Logger.Exception(e);
            }
        }*/

        [NotNull]
        private DayLightStatus MakeLightNeededArray([NotNull] GeographicLocation geographicLocation, [NotNull] TemperatureProfile tp,
            [NotNull] Random r, [NotNull][ItemNotNull] List<VacationTimeframe> vacations, [NotNull] string householdname, [NotNull] CalcParameters calcParameters)
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