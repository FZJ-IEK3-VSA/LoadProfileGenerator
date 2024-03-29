﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace CalcPostProcessor.GeneralHouseholdSteps {
    public class MakeHouseholdPlanResult : HouseholdStepBase
    {
        [JetBrains.Annotations.NotNull]
        private readonly IFileFactoryAndTracker _fft;

        public MakeHouseholdPlanResult(
                                       [JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                       [JetBrains.Annotations.NotNull] ICalculationProfiler profiler,
                                       [JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft):base(repository,
            AutomationUtili.GetOptionList(CalcOption.HouseholdPlan),
            profiler,"Household Plans",0)
        {
            _fft = fft;
        }

        [JetBrains.Annotations.NotNull]
        private static Dictionary<string, double> GetEnergyUsePerTag([JetBrains.Annotations.NotNull] Dictionary<string, string> affordanceToTag,
            [JetBrains.Annotations.NotNull] Dictionary<string, double> affordanceToEnergy)
        {
            var newDictionary = new Dictionary<string, double>();
            foreach (var pair in affordanceToEnergy) {
                var tag = pair.Key;
                if (affordanceToTag.ContainsKey(pair.Key)) {
                    tag = affordanceToTag[pair.Key];
                }
                if (!newDictionary.ContainsKey(tag)) {
                    newDictionary.Add(tag, 0);
                }
                newDictionary[tag] += pair.Value;
            }
            return newDictionary;
        }

        private  void Run(
            [JetBrains.Annotations.NotNull] Dictionary<StrGuid, Dictionary<string, double>>
                energyByAffordanceByLoadTypeByHousehold, [JetBrains.Annotations.NotNull] string householdName, [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcHouseholdPlanDto> householdPlans,
            [JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft, [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
            [JetBrains.Annotations.NotNull] Dictionary<string, Dictionary<string, Dictionary<string, int>>> affordanceTaggingSetByPersonByTagTimeUse,
            [JetBrains.Annotations.NotNull] Dictionary<string, Dictionary<string, Dictionary<string, int>>>
                affordanceTaggingSetByPersonByTagExecutionCount)
        {
            var fileNumberTracker =
                new Dictionary<Tuple<HouseholdKey, int>, StreamWriter>();
            RunTimePerAffordance(affordanceTaggingSetByPersonByTagTimeUse, householdName, householdPlans, fft,
                householdKey, fileNumberTracker);
            RunExecutionTimes(affordanceTaggingSetByPersonByTagExecutionCount, householdName, householdPlans,
                householdKey, fileNumberTracker);
            RunEnergyPerAffordance(energyByAffordanceByLoadTypeByHousehold, householdName, householdPlans,
                householdKey, fileNumberTracker);
        }

        private  void RunEnergyPerAffordance(
            [JetBrains.Annotations.NotNull]  Dictionary<StrGuid, Dictionary<string, double>>
                energyByAffordanceByLoadtype, [JetBrains.Annotations.NotNull] string householdName, [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcHouseholdPlanDto> householdPlans,
            [JetBrains.Annotations.NotNull] HouseholdKey householdKey, [JetBrains.Annotations.NotNull] Dictionary<Tuple<HouseholdKey, int>, StreamWriter> fileNumberTracker)
        {
            var calcParameters = Repository.CalcParameters;
            var simduration = calcParameters.OfficialEndTime -
                              calcParameters.OfficialStartTime;
            var timefactor = 8760 / simduration.TotalHours;
            Dictionary<StrGuid,CalcLoadTypeDto> loadTypeByGuid = new Dictionary<StrGuid, CalcLoadTypeDto>();
            foreach (CalcLoadTypeDto loadType in Repository.LoadTypes) {
                loadTypeByGuid.Add(loadType.Guid,loadType);
            }

            foreach (var calcHouseholdPlan in householdPlans) {
                if (householdName.StartsWith(calcHouseholdPlan.HouseholdName, StringComparison.Ordinal)) {
                    // found a plan
                    var sw = fileNumberTracker[new Tuple<HouseholdKey, int>(householdKey, calcHouseholdPlan.ID)];
                    sw.WriteLine("##### Energy Use #####");
                    foreach (var realEnergyUse in
                        energyByAffordanceByLoadtype) {
                        var lt = loadTypeByGuid[realEnergyUse.Key];
                        if (!string.Equals(lt.Name, "NONE", StringComparison.InvariantCultureIgnoreCase)) {
                            var tagEnergyUse =
                                GetEnergyUsePerTag(calcHouseholdPlan.AffordanceTags, realEnergyUse.Value);
                            sw.WriteLine("----- Consumption for " + lt.Name + " -----");
                            var c = calcParameters.CSVCharacter;
                            sw.WriteLine("Tag" + c + "Energy in Simulation [" + lt.UnitOfSum + "]" + c +
                                         "Energy in Simulation for 365d [" + lt.UnitOfSum + "]" + c +
                                         "Planned [" + lt.UnitOfSum + "]");
                            var plannedEnergyUse =
                                calcHouseholdPlan.TagEnergyUse[lt];
                            var plannedWithOthers = new Dictionary<string, double>();
                            foreach (var keyValuePair in plannedEnergyUse) {
                                plannedWithOthers.Add(keyValuePair.Key, keyValuePair.Value);
                            }
                            foreach (var keyValuePair in tagEnergyUse) {
                                if (!plannedWithOthers.ContainsKey(keyValuePair.Key)) {
                                    plannedWithOthers.Add(keyValuePair.Key, 0);
                                }
                            }

                            foreach (var plannedenergyByTag in plannedWithOthers) {
                                var s = plannedenergyByTag.Key + c;
                                double realValue = 0;
                                if (tagEnergyUse.ContainsKey(plannedenergyByTag.Key)) {
                                    realValue = tagEnergyUse[plannedenergyByTag.Key] *
                                                lt.ConversionFactor;
                                }
                                s += realValue + c;
                                s += realValue * timefactor + c;
                                s += plannedenergyByTag.Value + c;
                                double percentage = -1;
                                if (Math.Abs(plannedenergyByTag.Value) > Constants.Ebsilon) {
                                    percentage = realValue / plannedenergyByTag.Value;
                                }
                                s += percentage;
                                if (Math.Abs(realValue) > Constants.Ebsilon ||
                                    Math.Abs(plannedenergyByTag.Value) > Constants.Ebsilon) {
                                    sw.WriteLine(s);
                                }
                            }
                        }
                    }
                }
            }
        }

        private  void RunExecutionTimes(
            [JetBrains.Annotations.NotNull] Dictionary<string, Dictionary<string, Dictionary<string, int>>>
                affordanceTaggingSetByPersonByTagExecutionTimes, [JetBrains.Annotations.NotNull] string householdName,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcHouseholdPlanDto> householdPlans, [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
            [JetBrains.Annotations.NotNull] Dictionary<Tuple<HouseholdKey, int>, StreamWriter> fileNumberTracker)
        {
            var calcParameters = Repository.CalcParameters;
            var simduration = calcParameters.OfficialEndTime -
                              calcParameters.OfficialStartTime;
            var timefactor = 8760 / simduration.TotalHours;
            foreach (var calcHouseholdPlan in householdPlans) {
                if (householdName.StartsWith(calcHouseholdPlan.HouseholdName, StringComparison.Ordinal)) {
                    var sw = fileNumberTracker[new Tuple<HouseholdKey, int>(householdKey, calcHouseholdPlan.ID)];
                    var activationsByTagByPerson =
                        affordanceTaggingSetByPersonByTagExecutionTimes[calcHouseholdPlan.TaggingSetName];
                    sw.WriteLine("##### Activations per Affordance #####");
                    foreach (var personTagList in activationsByTagByPerson) {
                        var personName = personTagList.Key;
                        sw.WriteLine("----- Activations for " + personName + " -----");
                        var c = calcParameters.CSVCharacter;

                        sw.WriteLine("Tag" + c + "Activations in Simulation [h]" + c +
                                     "Activations in Simulation for 365d [h]" + c + "Planned Activations");
                        var activationsPerTag = personTagList.Value;
                        var plannedActivationsPerTag =
                            calcHouseholdPlan.PersonExecutionCount[personName];
                        var plannedWithOthers = new Dictionary<string, int>();
                        // list of all the affordancetags from both the plan and the execution merged together
                        foreach (var keyValuePair in plannedActivationsPerTag) {
                            plannedWithOthers.Add(keyValuePair.Key, keyValuePair.Value);
                        }
                        foreach (var keyValuePair in activationsPerTag) {
                            if (!plannedWithOthers.ContainsKey(keyValuePair.Key)) {
                                plannedWithOthers.Add(keyValuePair.Key, 0);
                            }
                        }
                        foreach (var keyValuePair in plannedWithOthers) {
                            var s = keyValuePair.Key + c;
                            double activations = 0;
                            if (activationsPerTag.ContainsKey(keyValuePair.Key)) {
                                activations = activationsPerTag[keyValuePair.Key];
                            }
                            s += activations + c;
                            s += activations * timefactor + c;
                            s += keyValuePair.Value + c;
                            double percentage = -1;
                            if (keyValuePair.Value != 0) {
                                percentage = activations / keyValuePair.Value;
                            }
                            s += percentage;
                            if (Math.Abs(activations) > Constants.Ebsilon || keyValuePair.Value != 0) {
                                sw.WriteLine(s);
                            }
                        }
                    }
                }
            }
        }

        private void RunTimePerAffordance(
            [JetBrains.Annotations.NotNull]
            Dictionary<string, Dictionary<string, Dictionary<string, int>>> affordanceTaggingSetByPersonByTagTimeUse,
            [JetBrains.Annotations.NotNull] string householdName, [JetBrains.Annotations.NotNull] [ItemNotNull] List<CalcHouseholdPlanDto> householdPlans,
            [JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft,
            [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
            [JetBrains.Annotations.NotNull] Dictionary<Tuple<HouseholdKey, int>, StreamWriter> fileNumberTracker)
        {
            var calcParameters = Repository.CalcParameters;
            var simduration = calcParameters.OfficialEndTime -
                              calcParameters.OfficialStartTime;
            var timefactor = 8760 / simduration.TotalHours;
            foreach (var calcHouseholdPlan in householdPlans) {
                if (householdName.StartsWith(calcHouseholdPlan.HouseholdName, StringComparison.Ordinal)) {
                    var cleanedName = AutomationUtili.CleanFileName(calcHouseholdPlan.Name).Replace(".", "_");
                    var fileName = "HouseholdPlan.Times." + cleanedName + "." + householdKey + ".csv";

                    var fileNumber = fileNumberTracker.Count;

                    var sw = fft.MakeFile<StreamWriter>(fileName,
                        "Comparison of the household plan and the reality", true,
                        ResultFileID.HouseholdPlanTime, householdKey, TargetDirectory.Reports,
                        calcParameters.InternalStepsize,CalcOption.HouseholdPlan, null, null,
                        calcHouseholdPlan.Name + "_" + fileNumber);

                    sw.WriteLine("##### Time per Affordance #####");

                    fileNumberTracker.Add(new Tuple<HouseholdKey, int>(householdKey, calcHouseholdPlan.ID), sw);
                    var timeByTagByPerson =
                        affordanceTaggingSetByPersonByTagTimeUse[calcHouseholdPlan.TaggingSetName];
                    foreach (var personTagList in timeByTagByPerson) {
                        var personName = personTagList.Key;
                        sw.WriteLine("----- Time Use for " + personName + " -----");
                        var c = calcParameters.CSVCharacter;
                        sw.WriteLine("Tag" + c + "Time in Simulation [h]" + c + "Time in Simulation for 365d [h]" + c +
                                     "Planned time");
                        var timePerTag = personTagList.Value;
                        var plannedTimePerTag =
                            calcHouseholdPlan.PersonTagTimeUsePlan[personName];
                        var plannedWithOthers = new Dictionary<string, TimeSpan>();
                        foreach (var keyValuePair in plannedTimePerTag) {
                            plannedWithOthers.Add(keyValuePair.Key, keyValuePair.Value);
                        }

                        foreach (var keyValuePair in timePerTag) {
                            if (!plannedWithOthers.ContainsKey(keyValuePair.Key)) {
                                plannedWithOthers.Add(keyValuePair.Key, new TimeSpan(0));
                            }
                        }

                        foreach (var keyValuePair in plannedWithOthers) {
                            var s = keyValuePair.Key + c;
                            double realValueHours = 0;
                            if (timePerTag.ContainsKey(keyValuePair.Key)) {
                                var minutes = timePerTag[keyValuePair.Key];
                                realValueHours = minutes / 60.0;
                            }

                            s += realValueHours + c;
                            s += realValueHours * timefactor + c;
                            s += keyValuePair.Value.TotalHours + c;
                            double percentage = -1;
                            if (Math.Abs(keyValuePair.Value.TotalHours) > Constants.Ebsilon) {
                                percentage = realValueHours / keyValuePair.Value.TotalHours;
                            }

                            s += percentage;
                            if (Math.Abs(realValueHours) > Constants.Ebsilon ||
                                Math.Abs(keyValuePair.Value.TotalSeconds) > Constants.Ebsilon) {
                                sw.WriteLine(s);
                            }
                        }
                    }

                    // found a plan
                }
            }
        }

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        protected override void PerformActualStep(IStepParameters parameters)
        {
            return;

#pragma warning disable S1135 // Track uses of "TODO" tags
//TODO: reimplement
#pragma warning disable 162
            foreach (var key in Repository.HouseholdKeys) {
#pragma warning restore S1135 // Track uses of "TODO" tags
                var activations = Repository.LoadDeviceActivations(key.HHKey);
                Dictionary<StrGuid, Dictionary<string, double>> energyusePerAffordanceByLoadtype = new Dictionary<StrGuid, Dictionary<string, double>>();
                foreach (DeviceActivationEntry activationEntry in activations) {
                    if (!energyusePerAffordanceByLoadtype.ContainsKey(activationEntry.LoadTypeGuid)) {
                        energyusePerAffordanceByLoadtype.Add(activationEntry.LoadTypeGuid, new Dictionary<string, double>());
                    }

                    if (!energyusePerAffordanceByLoadtype[activationEntry.LoadTypeGuid]
                        .ContainsKey(activationEntry.AffordanceName)) {
                        energyusePerAffordanceByLoadtype[activationEntry.LoadTypeGuid].Add(activationEntry.AffordanceName,0);
                    }
                    energyusePerAffordanceByLoadtype[activationEntry.LoadTypeGuid][activationEntry.AffordanceName] += activationEntry.TotalEnergySum;
                }
                Run(energyusePerAffordanceByLoadtype, Repository.CalcObjectInformation.CalcObjectName,
                    new List<CalcHouseholdPlanDto>() , _fft,key.HHKey, MakeActivationsPerFrequencies.AffordanceTaggingSetByPersonByTag,
                    MakeActivationsPerFrequencies.AffordanceTaggingSetByPersonByTagExecutioncount);
            }
#pragma warning restore 162
        }

        [JetBrains.Annotations.NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.DetailedDatFiles};
    }
}