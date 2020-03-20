/*using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
#pragma warning disable S101 // Types should be named in PascalCase
    public class ImportFileCreatorSMA: LoadTypeStepBase
#pragma warning restore S101 // Types should be named in PascalCase
    {
        [JetBrains.Annotations.NotNull]
        private readonly CalcParameters _calcParameters;
        [JetBrains.Annotations.NotNull]
        private readonly FileFactoryAndTracker _fft;

        public ImportFileCreatorSMA(
                                 [JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                 [JetBrains.Annotations.NotNull] ICalculationProfiler profiler,
                                    [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft):base(repository,CalcOption.SMAImportFiles,profiler,"SMA Import Files")
        {
            _calcParameters = Repository.CalcParameters;
            _fft = fft;
        }

        public void RunIndividualHouseholdsSMA([JetBrains.Annotations.NotNull] CalcLoadTypeDto dstLoadType,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
            [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, [JetBrains.Annotations.NotNull] EnergyFileColumns efc)
        {
            var fivemin = new TimeSpan(0, 5, 0);
            var externalfactor =
                (int) (fivemin.TotalSeconds / _calcParameters.InternalStepsize.TotalSeconds);
            if (externalfactor == 1) {
                return;
            }
            var externalFileName = fivemin.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            var householdNumbers =
                (from entry in efc.ColumnEntriesByColumn[dstLoadType].Values select entry.HouseholdKey).Distinct()
                .ToList();
            if (householdNumbers.Count > 1) {
                foreach (var hhnum in householdNumbers) {
                    var columns =
                    (from entry in efc.ColumnEntriesByColumn[dstLoadType].Values
                        where entry.HouseholdKey == hhnum
                        select entry.Column).ToList();
                    var hhname = "." + hhnum + ".";

                    var sumfile =
                        fft.MakeFile<StreamWriter>(
                            "ImportProfileSMA_" + externalFileName + "s" + hhname + dstLoadType.Name + ".csv",
                            "Summed up energy profile without timestamp for " + externalFileName + "s " +
                            dstLoadType.Name + " and " + hhnum, true,
                            ResultFileID.FiveMinuteImportFileForHH, hhnum, TargetDirectory.Results, fivemin,
                            dstLoadType.ConvertToLoadTypeInformation());
                    var timestepcount = 0;

                    for (var outerIndex = 0; outerIndex < energyFileRows.Count; outerIndex += externalfactor) {
                        double sum = 0;
                        var efr = energyFileRows[outerIndex];
                        if (!efr.Timestep.DisplayThisStep)
                        {
                            continue;
                        }
                        double sum = 0;

                        for (var innerIndex = outerIndex;
                            innerIndex < outerIndex + externalfactor && innerIndex < energyFileRows.Count;
                            innerIndex++)
                        {
                            var efr2 = energyFileRows[innerIndex];
                            sum += efr2.GetSumForCertainCols(columns);
                            timestepcount++;
                            ts = ts.Add(_calcParameters.InternalStepsize);
                        }
                        for (var innerIndex = outerIndex;
                            innerIndex < outerIndex + externalfactor && innerIndex < energyFileRows.Count;
                            innerIndex++) {
                            var efr2 = energyFileRows[innerIndex];
                            sum += efr2.GetSumForCertainCols(columns);
                            timestepcount++;
                        }
                        sum = sum / externalfactor;
                        var normalstr = sum.ToString("N4", CultureInfo.InvariantCulture);
                        sumfile.WriteLine(normalstr);
                    }
                    if (timestepcount != energyFileRows.Count) {
                        throw new LPGException("Importfilecreator seems to be broken! Please report!");
                    }
                }
            }
        }

        public void RunSMA([JetBrains.Annotations.NotNull] CalcLoadTypeDto dstLoadType, [JetBrains.Annotations.NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
            [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft)
        {
            var fivemin = new TimeSpan(0, 5, 0);
            var externalfactor =
                (int) (fivemin.TotalSeconds / _calcParameters.InternalStepsize.TotalSeconds);
            if (externalfactor == 1) {
                return;
            }
            var externalFileName = fivemin.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            var sumfile =
                fft.MakeFile<StreamWriter>("ImportProfile." + externalFileName + "s." + dstLoadType.Name + ".csv",
                    "Sum energy profile without timestamp for " + externalFileName + "s " + dstLoadType.Name, true,
                    ResultFileID.FiveMinuteImportFile, Constants.GeneralHouseholdKey, TargetDirectory.Results, fivemin,
                    dstLoadType.ConvertToLoadTypeInformation());
            var count = energyFileRows[0].EnergyEntries.Count;

            var timestepcount = 0;
            for (var outerIndex = 0; outerIndex < energyFileRows.Count; outerIndex += externalfactor) {
                TimeStep timeStep = new TimeStep(0,0,true);
                var efr = new OnlineEnergyFileRow(timeStep, new List<double>(new double[count]), dstLoadType);
                if (!efr.Timestep.DisplayThisStep) {
                    continue;
                }
                for (var innerIndex = outerIndex;
                    innerIndex < outerIndex + externalfactor && innerIndex < energyFileRows.Count;
                    innerIndex++) {
                    var efr2 = energyFileRows[innerIndex];
                    efr.AddValues(efr2);
                    timestepcount++;
                }
                var sum = efr.SumCached / externalfactor;
                var normalstr = sum.ToString("N4", Config.CultureInfo);
                sumfile.WriteLine(normalstr);
            }
            if (timestepcount != energyFileRows.Count) {
                throw new LPGException("Importfilecreator seems to be broken! Please report!");
            }
            sumfile.Flush();
        }

        protected override void PerformActualStep([JetBrains.Annotations.NotNull] IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            var efc = Repository.ReadEnergyFileColumns(Constants.GeneralHouseholdKey);
            RunIndividualHouseholdsSMA(p.LoadType,p.EnergyFileRows,_fft,efc);
            RunSMA(p.LoadType, p.EnergyFileRows, _fft);
        }
    }
}*/