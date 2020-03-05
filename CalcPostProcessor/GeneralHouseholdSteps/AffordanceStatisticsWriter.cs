using System.Collections.Generic;
using System.Linq;
using Automation;
using CalcPostProcessor.Steps;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace CalcPostProcessor.GeneralHouseholdSteps
{
    public class AffordanceStatisticsWriter : HouseholdLoadTypeStepBase
    {
        //private int _maxTime;
        [NotNull] private readonly IInputDataLogger _logger;

        public AffordanceStatisticsWriter([NotNull] CalcDataRepository repository,
                                             [NotNull] ICalculationProfiler profiler, [NotNull] IInputDataLogger logger)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.AffordanceEnergyUse), profiler, "Affordance Energy Use")
        {
            _logger = logger;
         /*   EnergyUseByHouseholdAffordanceAndLoadtype =
                new Dictionary<HouseholdKey, Dictionary<CalcLoadTypeDto, Dictionary<string, double>>>();
            _energyUseListByHouseholdAffordanceAndLoadtype =
                new Dictionary<HouseholdKey, Dictionary<CalcLoadTypeDto, Dictionary<string, List<double>>>>();
            _energyUseListByHouseholdAffordancePersonAndLoadtype =
                new Dictionary<HouseholdKey,
                    Dictionary<CalcLoadTypeDto, Dictionary<string, Dictionary<string, double>>>>();
                    */
        }

        /*
        [NotNull]
        private Dictionary<HouseholdKey, Dictionary<CalcLoadTypeDto, Dictionary<string, Dictionary<string, double>>>>
            _energyUseListByHouseholdAffordancePersonAndLoadtype;

        [NotNull]
        private readonly Dictionary<HouseholdKey, Dictionary<CalcLoadTypeDto, Dictionary<string, List<double>>>>
            _energyUseListByHouseholdAffordanceAndLoadtype;

        [NotNull]
        public Dictionary<HouseholdKey, Dictionary<CalcLoadTypeDto, Dictionary<string, double>>> EnergyUseByHouseholdAffordanceAndLoadtype { get; }
        */
        /*
        public void RegisterDeviceActivation([NotNull] HouseholdKey householdKey, [NotNull] string affordanceName,
                                             [NotNull] CalcLoadTypeDto loadType, double value,
                                             [NotNull] double[] allvalues, [NotNull] string activatorName)
        {

            if (!EnergyUseByHouseholdAffordanceAndLoadtype.ContainsKey(householdKey)) {
                EnergyUseByHouseholdAffordanceAndLoadtype.Add(householdKey,
                    new Dictionary<CalcLoadTypeDto, Dictionary<string, double>>());
                _energyUseListByHouseholdAffordanceAndLoadtype.Add(householdKey,
                    new Dictionary<CalcLoadTypeDto, Dictionary<string, List<double>>>());
                _energyUseListByHouseholdAffordancePersonAndLoadtype.Add(householdKey,
                    new Dictionary<CalcLoadTypeDto, Dictionary<string, Dictionary<string, double>>>());
            }

            if (!EnergyUseByHouseholdAffordanceAndLoadtype[householdKey].ContainsKey(loadType)) {
                EnergyUseByHouseholdAffordanceAndLoadtype[householdKey].Add(loadType, new Dictionary<string, double>());
                _energyUseListByHouseholdAffordanceAndLoadtype[householdKey]
                    .Add(loadType, new Dictionary<string, List<double>>());
                _energyUseListByHouseholdAffordancePersonAndLoadtype[householdKey]
                    .Add(loadType, new Dictionary<string, Dictionary<string, double>>());
            }

            if (!_energyUseListByHouseholdAffordancePersonAndLoadtype[householdKey][loadType]
                .ContainsKey(affordanceName)) {
                _energyUseListByHouseholdAffordancePersonAndLoadtype[householdKey][loadType]
                    .Add(affordanceName, new Dictionary<string, double>());
            }

            if (!_energyUseListByHouseholdAffordancePersonAndLoadtype[householdKey][loadType][affordanceName]
                .ContainsKey(activatorName)) {
                _energyUseListByHouseholdAffordancePersonAndLoadtype[householdKey][loadType][affordanceName]
                    .Add(activatorName, 0);
            }

            if (!EnergyUseByHouseholdAffordanceAndLoadtype[householdKey][loadType].ContainsKey(affordanceName)) {
                EnergyUseByHouseholdAffordanceAndLoadtype[householdKey][loadType].Add(affordanceName, 0);
                _energyUseListByHouseholdAffordanceAndLoadtype[householdKey][loadType]
                    .Add(affordanceName, new List<double>());
            }

            EnergyUseByHouseholdAffordanceAndLoadtype[householdKey][loadType][affordanceName] += value;

            _energyUseListByHouseholdAffordanceAndLoadtype[householdKey][loadType][affordanceName].AddRange(allvalues);
            _energyUseListByHouseholdAffordancePersonAndLoadtype[householdKey][loadType][affordanceName][activatorName]
                +=
                value;
            if (_energyUseListByHouseholdAffordanceAndLoadtype[householdKey][loadType][affordanceName].Count >
                _maxTime) {
                _maxTime = _energyUseListByHouseholdAffordanceAndLoadtype[householdKey][loadType][affordanceName].Count;
            }
        }*/
        /*
        public void WriteResults([NotNull] List<CalcAffordanceTaggingSetDto> affTaggingSets)
        {
            if (_wroteResultsAlready)
            {
                return;
            }

            foreach (var householdDict in
                EnergyUseByHouseholdAffordanceAndLoadtype)
            {
                foreach (var loadTypeDict in householdDict.Value)
                {
                    using (var sw =
                        _fft.MakeFile<StreamWriter>(
                            "AffordanceEnergyUse." + householdDict.Key + "." + loadTypeDict.Key.FileName + ".csv",
                            "Energy use per affordance " + loadTypeDict.Key.Name, true,
                            ResultFileID.AffordanceEnergyUse,
                            householdDict.Key, TargetDirectory.Reports, _calcParameters.InternalStepsize,
                            loadTypeDict.Key.ConvertToLoadTypeInformation()))
                    {
                        var header = "Affordance" + _calcParameters.CSVCharacter + "Energy use [" +
                                     loadTypeDict.Key.UnitOfSum + "]" + _calcParameters.CSVCharacter;
                        foreach (var set in affTaggingSets)
                        {
                            if (set.LoadTypes.Contains(loadTypeDict.Key))
                            {
                                header += set.Name + _calcParameters.CSVCharacter;
                            }
                        }

                        sw.WriteLine(header);
                        foreach (var affordanceTuple in loadTypeDict.Value)
                        {
                            var sb = new StringBuilder();
                            sb.Append(affordanceTuple.Key);
                            sb.Append(_calcParameters.CSVCharacter);
                            sb.Append(affordanceTuple.Value * loadTypeDict.Key.ConversionFactor);
                            foreach (var set in affTaggingSets)
                            {
                                if (set.LoadTypes.Contains(loadTypeDict.Key))
                                {
                                    sb.Append(_calcParameters.CSVCharacter);
                                    if (set.AffordanceToTagDict.ContainsKey(affordanceTuple.Key))
                                    {
                                        sb.Append(set.AffordanceToTagDict[affordanceTuple.Key]);
                                    }
                                    else
                                    {
                                        sb.Append(affordanceTuple.Key);
                                    }
                                }
                            }

                            sw.WriteLine(sb);
                        }
                    }
                }
            }

            foreach (
                var
                    householdDict in _energyUseListByHouseholdAffordancePersonAndLoadtype)
            {
                foreach (var loadTypeDict in
                    householdDict.Value)
                {
                    using (var sw =
                        _fft.MakeFile<StreamWriter>(
                            "AffordanceEnergyUsePerPerson." + householdDict.Key + "." + loadTypeDict.Key.FileName +
                            ".csv", "Energy use per affordance per Person for " + loadTypeDict.Key.Name, true,
                            ResultFileID.AffordancePersonEnergyUse, householdDict.Key,
                            TargetDirectory.Reports, _calcParameters.InternalStepsize,
                            loadTypeDict.Key.ConvertToLoadTypeInformation()))
                    {
                        sw.WriteLine("Energy use per affordance per Person in [" + loadTypeDict.Key.UnitOfSum + "]");
                        var header = "Affordance" + _calcParameters.CSVCharacter;
                        var persons = new List<string>();

                        foreach (var affDict in loadTypeDict.Value)
                        {
                            foreach (var person in affDict.Value)
                            {
                                if (!persons.Contains(person.Key))
                                {
                                    persons.Add(person.Key);
                                }
                            }
                        }

                        foreach (var person in persons)
                        {
                            header += person + _calcParameters.CSVCharacter;
                        }

                        sw.WriteLine(header);

                        foreach (var affdict in loadTypeDict.Value)
                        {
                            var line = affdict.Key + _calcParameters.CSVCharacter;

                            foreach (var person in persons)
                            {
                                if (affdict.Value.ContainsKey(person))
                                {
                                    line += affdict.Value[person] * loadTypeDict.Key.ConversionFactor +
                                            _calcParameters.CSVCharacter;
                                }
                                else
                                {
                                    line += 0 + _calcParameters.CSVCharacter;
                                }
                            }

                            sw.WriteLine(line);
                        }
                    }
                }
            }

            _wroteResultsAlready = true;
        }*/

        protected override void PerformActualStep(IStepParameters parameters)
        {
            HouseholdLoadtypeStepParameters hsp = (HouseholdLoadtypeStepParameters)parameters;
            if (hsp.Key.HouseholdKey == Constants.GeneralHouseholdKey) {
                return;
            }

            var deviceActivations = Repository.LoadDeviceActivations(hsp.Key.HouseholdKey);
            Dictionary<string,AffordanceEnergyUseEntry> energyUseEntries = new Dictionary<string, AffordanceEnergyUseEntry>();
            foreach (DeviceActivationEntry ae in deviceActivations) {
                string key = AffordanceEnergyUseEntry.MakeKey(ae.HouseholdKey, ae.LoadTypeGuid, ae.AffordanceName,
                    ae.ActivatorName);
                if (!energyUseEntries.ContainsKey(key)) {
                    energyUseEntries.Add(key,new AffordanceEnergyUseEntry(
                        ae.HouseholdKey,ae.LoadTypeGuid,ae.AffordanceName,ae.ActivatorName,
                        ae.LoadTypeName));
                }
                var aeu =energyUseEntries[key];
                    aeu.EnergySum += ae.TotalEnergySum;
                aeu.TotalActivationDurationInSteps += ae.DurationInSteps;
                aeu.NumberOfActivations++;
            }
            _logger.Save(hsp.Key.HouseholdKey, energyUseEntries.Values.ToList());
        }
    }
}
