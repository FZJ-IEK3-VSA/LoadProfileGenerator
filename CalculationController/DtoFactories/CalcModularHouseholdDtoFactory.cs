using System;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging.Loggers;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class CalcModularHouseholdDtoFactory
    {
        [NotNull]
        private readonly CalcLoadTypeDtoDictionary _ltDict;
        [NotNull]
        private readonly Random _random;
        [NotNull]
        private readonly CalcPersonDtoFactory _calcPersonDtoFactory;
        [NotNull]
        private readonly CalcDeviceDtoFactory _calcDeviceDtoFactory;
        [NotNull]
        private readonly CalcLocationDtoFactory _calcLocationDtoFactory;
        [NotNull]
        private readonly CalcVariableDtoFactory _calcVariableRepositoryDtoFactory;
        [NotNull]
        private readonly CalcAffordanceDtoFactory _calcAffordanceDtoFactory;
        [NotNull]
        private readonly CalcTransportationDtoFactory _transportationDtoFactory;

        private readonly CalcRepo _calcRepo;


        public CalcModularHouseholdDtoFactory([NotNull] CalcLoadTypeDtoDictionary ltDict, [NotNull] Random random,
            [NotNull] CalcPersonDtoFactory calcPersonDtoFactory,
            [NotNull] CalcDeviceDtoFactory calcDeviceDtoFactory,
            [NotNull] CalcLocationDtoFactory calcLocationDtoFactory,
            [NotNull] CalcVariableDtoFactory calcVariableRepositoryDtoFactory,
            [NotNull] CalcAffordanceDtoFactory calcAffordanceDtoFactory,
                                              [NotNull] CalcTransportationDtoFactory transportationDtoFactory,
                                              CalcRepo calcRepo)
        {
            _ltDict = ltDict;
            _random = random;
            _calcPersonDtoFactory = calcPersonDtoFactory;
            _calcDeviceDtoFactory = calcDeviceDtoFactory;
            _calcLocationDtoFactory = calcLocationDtoFactory;
            _calcVariableRepositoryDtoFactory = calcVariableRepositoryDtoFactory;
            _calcAffordanceDtoFactory = calcAffordanceDtoFactory;
            _transportationDtoFactory = transportationDtoFactory;
            _calcRepo = calcRepo;
        }
        [NotNull]
        public CalcHouseholdDto MakeCalcModularHouseholdDto([NotNull] Simulator sim, [NotNull] ModularHousehold mhh,
            [NotNull] TemperatureProfile temperatureProfile, [NotNull] HouseholdKey householdKey, [NotNull] GeographicLocation geographicLocation,
            [NotNull] out LocationDtoDict locationDict,
            [CanBeNull] TransportationDeviceSet transportationDeviceSet,
            [CanBeNull] TravelRouteSet travelRouteSet, EnergyIntensityType energyIntensity,
                                                            [CanBeNull] ChargingStationSet chargingStationSet)
        {
            //  _lf.RegisterKey(householdKey, mhh.PrettyName);
            var name = CalcAffordanceFactory.FixAffordanceName(mhh.Name, sim.MyGeneralConfig.CSVCharacter);
            if (geographicLocation == null)
            {
                throw new DataIntegrityException("no geographic Location was set");
            }
            var et = energyIntensity;
            if (et == EnergyIntensityType.AsOriginal)
            {
                et = mhh.EnergyIntensityType;
            }
            name = name + " " + householdKey.Key;
                var locations = mhh.CollectLocations();
                //var deviceLocationDict = new Dictionary<CalcLocation, List<IAssignableDevice>>();
                var deviceLocationDtoDict = new Dictionary<CalcLocationDto, List<IAssignableDevice>>();
                locationDict = new LocationDtoDict();
                List<DeviceCategoryDto> deviceCategoryDtos = new List<DeviceCategoryDto>();
                foreach (var deviceCategory in sim.DeviceCategories.It) {
                    deviceCategoryDtos.Add(new DeviceCategoryDto(deviceCategory.FullPath,Guid.NewGuid().ToString()));
                }
                var locationDtos = _calcLocationDtoFactory.MakeCalcLocations(locations,
                    householdKey,
                    et, deviceLocationDtoDict, sim.DeviceActions.It, locationDict,deviceCategoryDtos);
                // persons

                if (mhh.Vacation == null)
                {
                    throw new LPGException("Vacation was null");
                }

                var personDtos = _calcPersonDtoFactory.MakePersonDtos(mhh.Persons.ToList(), householdKey,
                    mhh.Vacation.VacationTimeframes(), mhh.CollectTraitDesires(), mhh.Name);
            _calcRepo.InputDataLogger.SaveList(personDtos.ConvertAll(x => (IHouseholdKey)x));

            //mhh.Persons.ToList(),mhh.Vacation.VacationTimeframes(),  sim.MyGeneralConfig.RepetitionCount,householdKey, locs[0],name);
            //CalcPersonFactory.AddTraitDesires(mhh.CollectTraitDesires(), calcpersons,sim.MyGeneralConfig.TimeStepsPerHour, chh.Name, new Dictionary<Desire, SharedDesireValue>());
            //check if unhungry and unhungry join only have been added both
            //can't check it in the integrity checker, because that would mean having to duplicate the entire
            // desire collection logic

            /*  foreach (CalcPerson person in calcpersons) {
                  var desires =
                      person.PersonDesires.Desires.Values.Where(x => x.Name.ToLower().Contains("unhungry") || x.Name.ToLower().Contains("un-hungry")).ToList();
                  if (desires.Count > 1) {
                      throw new DataIntegrityException("More than one unhungry desire for the person " + person.Name, mhh);
                  }
              }*/

            // devices

            var deviceLocations = new List<DeviceLocationTuple>();

                foreach (var modularHouseholdTrait in mhh.Traits)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (modularHouseholdTrait.HouseholdTrait != null)
                    {
                        CollectDevicesFromTrait(modularHouseholdTrait.HouseholdTrait, deviceLocations);
                    }
                }

                var deviceDtos = _calcDeviceDtoFactory.MakeCalcDevices(locationDtos,
                    deviceLocations, et, householdKey, deviceLocationDtoDict, sim.DeviceActions.It,_ltDict, deviceCategoryDtos);
                _calcRepo.InputDataLogger.SaveList(deviceDtos.ConvertAll(x => (IHouseholdKey)x));

            //autodevs
            var autonomousDevices = mhh.CollectAutonomousDevices();
                if (mhh.Vacation == null)
                {
                    throw new LPGException("Vacation was null");
                }

                var autoDevDtos = _calcDeviceDtoFactory.MakeCalcAutoDevDtos(autonomousDevices,
                    energyIntensity, householdKey, mhh.Vacation.VacationTimeframes(),
                    mhh.Name + "###" + householdKey,
                    sim.DeviceActions.It, locationDict,
                    temperatureProfile, geographicLocation,deviceCategoryDtos);
                _calcRepo.InputDataLogger.SaveList(autoDevDtos.ConvertAll(x=>(IHouseholdKey) x));

                //affordances
                var affordancesAtLoc =
                    new Dictionary<CalcLocationDto, List<AffordanceWithTimeLimit>>();
                foreach (var location in locations)
                {
                    affordancesAtLoc.Add(locationDict.GetDtoForLocation(location), mhh.GetAllAffordancesForLocation(location));
                }
                if (mhh.Vacation == null)
                {
                    throw new LPGException("Vacation was null");
                }

                List<CalcAffordanceDto> allAffordances = _calcAffordanceDtoFactory.SetCalcAffordances(locationDtos, temperatureProfile,
                    _ltDict,
                    geographicLocation, _random, sim.MyGeneralConfig.TimeStepsPerHour,
                    sim.MyGeneralConfig.InternalStepSize, mhh.Vacation.VacationTimeframes(),
                    mhh.Name + "###" + householdKey, sim.DeviceActions.MyItems, affordancesAtLoc, locationDict,
                    out List<DateTime> bridgeDays, householdKey, deviceDtos,deviceCategoryDtos);
                _calcRepo.InputDataLogger.SaveList(allAffordances.ConvertAll(x => (IHouseholdKey)x));
                _calcRepo.InputDataLogger.SaveList(_calcVariableRepositoryDtoFactory.GetAllVariableDtos().ConvertAll(x => (IHouseholdKey)x));
            //                SaveVariableDefinitionsDtos(_calcVariableRepositoryDtoFactory.GetAllVariableDtos());
            //CalcVariableRepository variableRepository = _calcVariableRepositoryDtoFactory.GetRepository(householdKey);
            List<CalcSiteDto> sites = null;
            List<CalcTransportationDeviceDto> transportationDevices = null;
            List<CalcTravelRouteDto> routes = null;
            if (transportationDeviceSet != null && travelRouteSet != null) {
                _transportationDtoFactory.MakeTransportationDtos(sim, mhh, transportationDeviceSet,
                    travelRouteSet,chargingStationSet,
                    out  sites, out  transportationDevices,
                    out routes, locationDtos, householdKey);
                _calcRepo.InputDataLogger.SaveList(sites.ConvertAll(x => (IHouseholdKey)x));
                _calcRepo.InputDataLogger.SaveList(transportationDevices.ConvertAll(x => (IHouseholdKey)x));
                _calcRepo.InputDataLogger.SaveList(routes.ConvertAll(x => (IHouseholdKey)x));
            }
            _calcRepo.CalcParameters.SetInTransportMode(householdKey, mhh.IsTransportationEnabled);
            var chh = new CalcHouseholdDto(name, mhh.IntID, temperatureProfile.Name,householdKey,  Guid.NewGuid().ToString(),
                    geographicLocation.Name,
                    bridgeDays,autoDevDtos,locationDtos,personDtos,deviceDtos,
                    allAffordances, mhh.Vacation.VacationTimeframes(),
                    sites,routes,transportationDevices,
                    mhh.Description);
            BridgeDayEntries bdes = new BridgeDayEntries(householdKey, chh.BridgeDays);
            _calcRepo.InputDataLogger.Save(householdKey, bdes);
            _calcRepo.InputDataLogger.Save(householdKey, chh);
            return chh;
        }
        /*
        private void SaveDeviceDts([NotNull][ItemNotNull] List<CalcDeviceDto> deviceDtos)
        {
            List<HouseholdDefinition> deviceDefs = new List<HouseholdDefinition>();
            foreach (var deviceDto in deviceDtos)
            {
                HouseholdDefinition pd = new HouseholdDefinition();
                pd.Name = deviceDto.Name;
                pd.HouseholdKey = deviceDto.HouseholdKey;
                pd.Json = JsonConvert.SerializeObject(deviceDto, Formatting.Indented);
                deviceDefs.Add(pd);
            }
            _srls.SaveToDatabase(deviceDefs);
        }*/

        private static void CollectDevicesFromTrait([NotNull] HouseholdTrait hht,
            [NotNull][ItemNotNull] List<DeviceLocationTuple> deviceList)
        {
            var allDevices = hht.CollectDevicesFromTrait();
            foreach (var dev in allDevices)
            {
                if (!deviceList.Any(x => x.Device == dev.Item2 && x.Location == dev.Item1))
                {
                    deviceList.Add(new DeviceLocationTuple(dev.Item1, dev.Item2));
                }
            }
        }

        /*
        private void SaveAutoDeviceDtos([NotNull][ItemNotNull] List<CalcAutoDevDto> deviceDtos)
        {
            List<AutoDeviceDefinition> deviceDefs = new List<AutoDeviceDefinition>();
            foreach (var deviceDto in deviceDtos)
            {
                AutoDeviceDefinition pd = new AutoDeviceDefinition();
                pd.Name = deviceDto.Name;
                pd.HouseholdKey = deviceDto.HouseholdKey;
                pd.Json = JsonConvert.SerializeObject(deviceDto, Formatting.Indented);
                deviceDefs.Add(pd);
            }
            _srls.SaveToDatabase(deviceDefs);
        }*/
        /*
        public void SavePersonDtos([NotNull][ItemNotNull] List<CalcPersonDto> persons)
        {
            List<PersonDefinition> persondefs = new List<PersonDefinition>();
            foreach (var personDto in persons)
            {
                PersonDefinition pd = new PersonDefinition(personDto.ID, personDto.Name,
                 personDto.HouseholdKey,
                    JsonConvert.SerializeObject(personDto, Formatting.Indented));
                persondefs.Add(pd);
            }
            _srls.SaveToDatabase(persondefs);
        }
        */
        /*
        private void SaveVariableDefinitionsDtos([NotNull][ItemNotNull] List<CalcVariableDto> variableDtos)
        {
            List<VariableDefinition> variableDefinitions = new List<VariableDefinition>();
            foreach (var variableDto in variableDtos)
            {
                VariableDefinition pd = new VariableDefinition();
                pd.Name = variableDto.Name;
                pd.HouseholdKey = variableDto.HouseholdKey;
                pd.Json = JsonConvert.SerializeObject(variableDto, Formatting.Indented);
                variableDefinitions.Add(pd);
            }
            _srls.SaveToDatabase(variableDefinitions);
        }*/
    }
}