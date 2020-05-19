#region header

// // ProfileGenerator Calculation changed: 2015 12 10 18:10

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.JSON;
using Common.SQLResultLogging;

namespace CalcPostProcessor.GeneralHouseholdSteps {
    public class LocationStatisticsMaker : HouseholdStepBase {
        [JetBrains.Annotations.NotNull] private readonly CalcParameters _calcParameters;

        [JetBrains.Annotations.NotNull] private readonly FileFactoryAndTracker _fft;

        public LocationStatisticsMaker( [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                       [JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                       [JetBrains.Annotations.NotNull] ICalculationProfiler profiler) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.LocationsFile),
            profiler, "Location Statistics",0)
        {
            _calcParameters = repository.CalcParameters;
            _fft = fft;
            MakeStreamWriterFunc = GetWriter;
        }

        [JetBrains.Annotations.NotNull]
        private Func<FileFactoryAndTracker, HouseholdKey, CalcParameters, StreamWriter> MakeStreamWriterFunc { get; }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            var entry = hhp.Key;
            if (entry.KeyType != HouseholdKeyType.Household) {
                return;
            }

            Run(_fft, entry.HouseholdKey);
        }

        /// <summary>
        ///     This function gets a new streamwriter from the fft. It's this way to enable easier unit testing.
        /// </summary>
        [JetBrains.Annotations.NotNull]
        private static StreamWriter GetWriter([JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                              [JetBrains.Annotations.NotNull] CalcParameters calcParameters) => fft
            .MakeFile<StreamWriter>("LocationStatistics." + householdKey + ".csv",
                "How much time the people are spending at each Location", true, ResultFileID.LocationStatistic,
                householdKey, TargetDirectory.Reports, calcParameters.InternalStepsize);

        private void Run([JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            var locationEntries = Repository.LoadLocationEntries(householdKey);
            var maxTimeStep = _calcParameters.OfficalTimesteps;
            if (maxTimeStep == 0) {
                throw new LPGException("Timesteps was not initialized");
            }

            var distinctLocations = locationEntries.Select(x => x.LocationGuid).Distinct().ToList();
            var locLookup = new Dictionary<StrGuid, byte>();
            for (byte i = 0; i < distinctLocations.Count; i++) {
                locLookup.Add(distinctLocations[i], i);
            }

            var distinctPersonGuids = locationEntries.Select(x => x.PersonGuid).Distinct().ToList();
            var locationArray = new byte[maxTimeStep];
            var sw = MakeStreamWriterFunc(fft, householdKey, _calcParameters);
            foreach (var person in distinctPersonGuids) {
                var filteredEntries = locationEntries.Where(x => x.PersonGuid == person).ToList();
                byte lastLocation = 0;
                for (var i = 0; i < maxTimeStep; i++) {
                    if (filteredEntries.Count > 0 && filteredEntries[0].Timestep.InternalStep == i) {
                        lastLocation = locLookup[filteredEntries[0].LocationGuid];
                        filteredEntries.RemoveAt(0);
                    }

                    locationArray[i] = lastLocation;
                }

                var timeUseByLoc = new Dictionary<byte, int>();
                for (byte i = 0; i < locLookup.Count; i++) {
                    timeUseByLoc.Add(i, 0);
                }

                for (var i = 0; i < maxTimeStep; i++) {
                    timeUseByLoc[locationArray[i]]++;
                }

                sw.WriteLine("-----");
                sw.WriteLine(person);
                for (byte i = 0; i < distinctLocations.Count; i++) {
                    var distinctLocationGuid = distinctLocations[i];
                    var distinctLocation = locationEntries.FirstOrDefault(x => x.LocationGuid == distinctLocationGuid)?.LocationName;
                    var s = distinctLocation + _calcParameters.CSVCharacter;
                    var timestepsSpent = timeUseByLoc[i];
                    var percentage = timestepsSpent / (double)maxTimeStep;
                    s += timestepsSpent + _calcParameters.CSVCharacter;
                    s += percentage + _calcParameters.CSVCharacter;
                    sw.WriteLine(s);
                }
            }

            sw.Close();
        }
    }
}