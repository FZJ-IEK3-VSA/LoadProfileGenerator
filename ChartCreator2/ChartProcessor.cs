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
using Automation;
using Automation.ResultFiles;
using ChartCreator2.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace ChartCreator2 {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class ChartOptionDependencyManager {

        public ChartOptionDependencyManager(
            [JetBrains.Annotations.NotNull] [ItemNotNull] ILoadTypeStep[] loadTypePostProcessingSteps,
            [JetBrains.Annotations.NotNull] [ItemNotNull] IGeneralStep[] generalPostProcessingSteps,
            [JetBrains.Annotations.NotNull] [ItemNotNull] IGeneralHouseholdStep[] generalHouseholdSteps,
            [JetBrains.Annotations.NotNull] [ItemNotNull] IHouseholdLoadTypeStep[] householdloadTypePostProcessingSteps,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ILoadTypeSumStep[] loadtypeSumPostProcessingSteps
        )
        {
            List<IRequireOptions> allSteps = new List<IRequireOptions>();
            allSteps.AddRange(loadTypePostProcessingSteps);
            allSteps.AddRange(generalPostProcessingSteps);
            allSteps.AddRange(generalHouseholdSteps);
            allSteps.AddRange(householdloadTypePostProcessingSteps);
            allSteps.AddRange(loadtypeSumPostProcessingSteps);
            foreach (var step in allSteps) {
                foreach (var option in step.Options) {
                    if (!_dependencies.ContainsKey(option)) {
                        _dependencies.Add(option, new List<CalcOption>());
                    }

                    foreach (var neededOption in step.NeededOptions) {
                        if (!_dependencies[option].Contains(neededOption)) {
                            _dependencies[option].Add(neededOption);
                        }
                    }
                }
            }

        }

        public void EnableRequiredOptions(HashSet<CalcOption> options)
        {
            int count = 0;
            foreach (var pair in _dependencies)
            {
                if (options.Contains(pair.Key))
                {
                    foreach (var requiredOption in pair.Value)
                    {
                        if (!options.Contains(requiredOption))
                        {
                            options.Add(requiredOption);
                            count++;
                        }
                    }
                }
            }
            if(count > 0){
                EnableRequiredOptions(options);
            }
            Logger.Info("Enabled " + count + " dependencies");
        }

        private readonly Dictionary<CalcOption, List<CalcOption>> _dependencies = new Dictionary<CalcOption, List<CalcOption>>();
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class ChartProcessor {
        [JetBrains.Annotations.NotNull]
        private readonly IFileFactoryAndTracker _fft;
        [JetBrains.Annotations.NotNull]
        private readonly ICalculationProfiler _calculationProfiler;
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly ILoadTypeStep[] _loadTypePostProcessingSteps;
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly ILoadTypeSumStep[] _loadTypeSumPostProcessingSteps;
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly IGeneralStep[] _generalPostProcessingSteps;

        [JetBrains.Annotations.NotNull] [ItemNotNull] private readonly IGeneralHouseholdStep[] _generalHouseholdSteps;
        [JetBrains.Annotations.NotNull] [ItemNotNull] private readonly IHouseholdLoadTypeStep[] _householdloadTypePostProcessingSteps;

        [JetBrains.Annotations.NotNull]
        private readonly CalcDataRepository _repository;

        public ChartProcessor(
            [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
            [JetBrains.Annotations.NotNull] CalcDataRepository repository,
            [JetBrains.Annotations.NotNull][ItemNotNull] ILoadTypeStep[] loadTypePostProcessingSteps,
                [JetBrains.Annotations.NotNull][ItemNotNull] IGeneralStep[] generalPostProcessingSteps,
            [JetBrains.Annotations.NotNull][ItemNotNull] IGeneralHouseholdStep[] generalHouseholdSteps,
            [JetBrains.Annotations.NotNull][ItemNotNull] IHouseholdLoadTypeStep[] householdloadTypePostProcessingSteps,
            [JetBrains.Annotations.NotNull][ItemNotNull] ILoadTypeSumStep[] loadtypeSumPostProcessingSteps,
            [JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft
            )
        {
            _loadTypeSumPostProcessingSteps = loadtypeSumPostProcessingSteps;
            _fft = fft;
            _calculationProfiler = calculationProfiler;
            _repository = repository;
            _loadTypePostProcessingSteps = loadTypePostProcessingSteps;
            _generalPostProcessingSteps = generalPostProcessingSteps;
            _generalHouseholdSteps = generalHouseholdSteps;
            _householdloadTypePostProcessingSteps = householdloadTypePostProcessingSteps;
        }

        private static void CheckTotalsForChange([JetBrains.Annotations.NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows, double total) {
            if (Config.IsInUnitTesting && Config.ExtraUnitTestChecking) {
                double newtotal = 0;
                foreach (var efr in energyFileRows) {
                    newtotal += efr.SumFresh();
                }
                //Logger.Info("Check for " + loadtypeName + " Nr." + _totalcheck + ": total:" + newtotal);
                //_totalcheck++;
                if (Math.Abs(newtotal - total) > 0.0000001) {
                    throw new LPGException("Bug in postprocessing. EFR was changed. Please report!");
                }

                double cacheTotal = 0;
                foreach (var efr in energyFileRows) {
                    var cache = efr.SumCached;
                    if (Math.Abs(efr.SumFresh() - cache) > Constants.Ebsilon) {
                        throw new LPGException("This should never happen");
                    }
                    cacheTotal += cache;
                }
                if (Math.Abs(cacheTotal - total) > 0.0000001) {
                    throw new LPGException("Bug in postprocessing. EFR was changed. Please report!");
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void RunPostProcessing()
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Preparation");
            if (_repository.CalcParameters.CSVCharacter.Length != 1)
            {
                throw new DataIntegrityException(
                    "The length of the CSV-Character is not 1. Please enter a single, valid character in the settings.");
            }
            /*    var deviceNamesToCategory = new Dictionary<string, string>();
              //  foreach (var calcDevice in _repository.GetDevices().Devices)
                {
                    if (!deviceNamesToCategory.ContainsKey(calcDevice.Name))
                    {
                        deviceNamesToCategory.Add(calcDevice.Name, calcDevice.DeviceCategoryName);
                    }
                }*/
            if (_repository.HouseholdKeys.Count == 0)
            {
                throw new LPGException("No household Numbers!");
            }
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Preparation");
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - EndOfSimulationProcessing");
            ActualFunctionCaller(_repository.CalcParameters.LoadtypesToPostprocess);
            //repository.CalculationResult.RandomSeed = _randomSeed;
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - EndOfSimulationProcessing");
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
        }

        private void ActualFunctionCaller([CanBeNull] [ItemNotNull] List<string> loadTypesToProcess) {
            //var now = DateTime.Now;
            //var step = 1;
            /*
            if (repository.CalcParameters.IsSet(CalcOption.OverallSum)) {
                _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Overall Sum File");
                foreach (var calcLoadType in repository.LoadTypes) {
                    HouseDeviceProfileFileProcessor dpfp = new HouseDeviceProfileFileProcessor(_fft, repository.CalcParameters);
                    dpfp.ProcessSumFile(calcLoadType);
                }
                LogProcessingProgress(ref now, ref step,
                    CalcOptionHelper.CalcOptionDictionary[CalcOption.OverallSum]);
                _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Overall Sum File");
            }*/
            {
                GeneralStepParameters gsp = new GeneralStepParameters();
                foreach (var generalPostProcessingStep in _generalPostProcessingSteps) {
                    if(generalPostProcessingStep.IsEnabled()) {
                        generalPostProcessingStep.Run(gsp);
                    }
                }
            }

            foreach (HouseholdKeyEntry keyEntry in _repository.HouseholdKeys) {
                HouseholdStepParameters gsp = new HouseholdStepParameters(keyEntry);
                var prios = _generalHouseholdSteps.Select(x => x.Priority).OrderBy(x => x).Distinct().ToList();
                foreach (var i in prios) {
                    var filteredSteps = _generalHouseholdSteps.Where(x => x.Priority == i).ToList();
                    foreach (var generalPostProcessingStep in filteredSteps)
                    {
                        if (generalPostProcessingStep.IsEnabled())
                        {
                            generalPostProcessingStep.Run(gsp);
                        }
                    }
                }
            }

            /*
            foreach (var householdKey in repository.HouseholdKeys) {
                if (repository.CalcParameters.IsSet(CalcOption.ActivationFrequencies) &&
                    repository.CalcObjectType == CalcObjectType.ModularHousehold &&
                    repository.CalcParameters.IsSet(CalcOption.HouseholdPlan) && repository.CalcParameters.IsSet(CalcOption.ActionsLogfile)) {
                    _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Household Plans");
                    MakeHouseholdPlanResult mhpr = new MakeHouseholdPlanResult(repository.CalcParameters);
                    mhpr.Run(repository.affordanceEnergyUseFile.EnergyUseByHouseholdAffordanceAndLoadtype,
                        repository.householdNamesByKey[householdKey], repository.householdPlans, _fft, householdKey,
                        MakeActivationsPerFrequencies.AffordanceTaggingSetByPersonByTag,
                        MakeActivationsPerFrequencies.AffordanceTaggingSetByPersonByTagExecutioncount);
                    _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Household Plans");
                }
            }*/
            // make totals per loadtype
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Load Type Dependend");

                RunLoadTypeDependend(loadTypesToProcess);
            //repository.efc, devices, calcLocations, autoDevices, loadTypes, deviceTaggingSets, allResults,persons, deviceNameToDeviceCategory, householdNamesByKey,carpetPlotColumnWidth);
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Load Type Dependend");
        }
        /*
        private  List<ICalcDeviceDto> GetAllCalcDevices() {
            var alldevices = new List<ICalcDeviceDto>();
            alldevices.AddRange(repository.Devices);
            foreach (var calcAutoDev in repository.AutoDevices) {
                alldevices.Add(calcAutoDev);
            }
            foreach (var calcLocation in repository.CalcLocations) {
                    foreach (var lightDevice in calcLocation.LightDevices) {
                        alldevices.Add(lightDevice);
                    }
            }
            return alldevices;
        }*/
        /*
        private static void LogProcessingProgress(ref DateTime lasttime, ref int step, string description) {
            var now = DateTime.Now;
            Logger.Info("Finished the file " + description + " (Step " + step + ") " + " in " +
                        (now - lasttime).TotalMilliseconds.ToString("0", Config.CultureInfo) + " milliseconds");
            step++;
            lasttime = now;
        }*/

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void RunLoadTypeDependend([CanBeNull] [ItemNotNull] List<string> loadTypesForPostProcessing) {
            if (_loadTypeSumPostProcessingSteps.Any(x => x.IsEnabled()) ) {
                _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Calculation of the sums per load type");
                foreach (var calcLoadType in _repository.LoadTypes) {
                    //if (!_fft.CheckForResultFileEntry(ResultFileID.OnlineSumActivationFiles,
                    //    calcLoadType.Name,
                    //    Constants.GeneralHouseholdKey,
                    //    null,
                    //    null)) {
                    //    Logger.Info("Skipping post-processing of load type " + calcLoadType.Name +
                    //                " because there was no sum dat file generated for it.");
                    //    continue;
                    //}

                    if (loadTypesForPostProcessing != null && loadTypesForPostProcessing.Count > 0 &&
                        !loadTypesForPostProcessing.Contains(calcLoadType.Name)) {
                        Logger.Info("Skipping post-processing of load type " + calcLoadType.Name + " because it was not specified.");
                        continue;
                    }

                    _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - " + calcLoadType.Name);

                    foreach (ILoadTypeSumStep loadTypePostProcessingStep in _loadTypeSumPostProcessingSteps) {
                        LoadtypeSumStepParameters ltsp = new LoadtypeSumStepParameters(calcLoadType);
                        loadTypePostProcessingStep.Run(ltsp);
                    }

                    _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - " + calcLoadType.Name);
                }
                _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Calculation of the sums per load type");
            }

            if (!_loadTypePostProcessingSteps.Any(x => x.IsEnabled())
                && !_householdloadTypePostProcessingSteps.Any(x => x.IsEnabled())
            )

            {
                return;
            }
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Calculation of the individual profiles for devices and households");
            // needs to be persistent object to keep the dictionary for all load types
            int fileCount = 0;
            foreach (var calcLoadType in _repository.LoadTypes) {
                if (!_fft.CheckForResultFileEntry(ResultFileID.OnlineDeviceActivationFiles, calcLoadType.Name,
                    Constants.GeneralHouseholdKey, null, null)) {
                    Logger.Info("Skipping post-processing of load type " + calcLoadType.Name + " because there was no dat file generated for it.");
                    continue;
                }

                if (loadTypesForPostProcessing != null && loadTypesForPostProcessing.Count > 0 && !loadTypesForPostProcessing.Contains(calcLoadType.Name)) {
                    Logger.Info("Skipping post-processing of load type " + calcLoadType.Name + " because it was not specified.");
                    continue;
                }
                _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - " + calcLoadType.Name);
                List<OnlineEnergyFileRow> energyFileRows = ReadOnlineEnergyFileRowsIntoMemory(calcLoadType, out var total);
                fileCount++;
                if (Config.IsInUnitTesting && Config.ExtraUnitTestChecking) {
                    Logger.Info("Starting total:" + total);
                    CheckTotalsForChange(energyFileRows, total);
                }

                foreach (ILoadTypeStep loadTypePostProcessingStep in _loadTypePostProcessingSteps) {
                    LoadtypeStepParameters ltsp = new LoadtypeStepParameters(calcLoadType,energyFileRows);
                    loadTypePostProcessingStep.Run(ltsp);
                }
                foreach (HouseholdKeyEntry entry in _repository.HouseholdKeys) {
                    foreach (var ltpps in _householdloadTypePostProcessingSteps) {
                        HouseholdLoadtypeStepParameters ltsp = new HouseholdLoadtypeStepParameters(entry,calcLoadType, energyFileRows);
                        ltpps.Run(ltsp);
                    }
                }
                _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - " + calcLoadType.Name);
            }

            if (fileCount == 0) {
                var ltp = _loadTypePostProcessingSteps.Where(x => x.IsEnabled()).Select(x=> x.StepName).ToList();
                var hhltp = _householdloadTypePostProcessingSteps.Where(x => x.IsEnabled()).Select(x => x.StepName)
                    .ToList();
                string s1 = string.Join("\n", ltp);
                string s2 = string.Join("\n", hhltp);
                throw new LPGException("Not a single file for postprocessing was found, but the following steps were enabled:" + s1 + "\n"+ s2);
            }
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Calculation of the individual profiles for devices and households");
            //if (_repository.DeviceSumInformationList.DeviceSums.Count > 0) {
            //                _repository.DeviceSumInformationList.WriteJson(_fft, _repository.CalcParameters.InternalStepsize);
            //          }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<OnlineEnergyFileRow> ReadOnlineEnergyFileRowsIntoMemory([JetBrains.Annotations.NotNull] CalcLoadTypeDto calcLoadType, out double total)
        {
            var path = _fft.GetResultFileEntry(ResultFileID.OnlineDeviceActivationFiles, calcLoadType.Name,
                    Constants.GeneralHouseholdKey, null, null)
                .FullFileName;
            var energyFileRows = new List<OnlineEnergyFileRow>();

            total = 0;
            if (path == null) {
                throw new LPGException("path was null");
            }
            using (Stream fs = new FileStream(path, FileMode.Open))
            {
                long currentPosition = 0;
#pragma warning disable S2930 // "IDisposables" should be disposed
                using (var br = new BinaryReader(fs))
                {
#pragma warning restore S2930 // "IDisposables" should be disposed

                    while (currentPosition < fs.Length)
                    {
                        var efr = OnlineEnergyFileRow.Read(br, calcLoadType, _repository.CalcParameters);
                        energyFileRows.Add(efr);
                        if (Config.IsInUnitTesting && Config.ExtraUnitTestChecking)
                        {
                            total += efr.SumFresh();
                        }

                        currentPosition += efr.EntryLengthInByte;
                    }
                }
            }

            return energyFileRows;
        }
    }
}