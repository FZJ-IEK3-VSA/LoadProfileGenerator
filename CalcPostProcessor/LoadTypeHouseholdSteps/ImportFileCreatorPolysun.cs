using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public class ImportFileCreatorPolysun : LoadTypeStepBase {
        [NotNull] private readonly CalcParameters _calcParameters;

        [NotNull] private readonly FileFactoryAndTracker _fft;

        public ImportFileCreatorPolysun([NotNull] CalcDataRepository repository,
                                        [NotNull] ICalculationProfiler profiler,
                                        [NotNull] FileFactoryAndTracker fft) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.PolysunImportFiles),
            profiler,
            "Polysun Import Files")
        {
            _calcParameters = Repository.CalcParameters;
            _fft = fft;
        }
        /*
        public void RunIndividualHouseholdsPolysun([NotNull] CalcLoadTypeDto dstLoadType,
            [NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows, [NotNull] FileFactoryAndTracker fft, [NotNull] EnergyFileColumns efc)
        {
            var fifteenmin = new TimeSpan(0, 15, 0);
            var externalfactor =
                (int) (fifteenmin.TotalSeconds / _calcParameters.InternalStepsize.TotalSeconds);
            if (externalfactor == 1) {
                return;
            }
            var externalFileName = fifteenmin.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            var householdNumbers =
                (efc.ColumnEntriesByColumn[dstLoadType].Values.Select(entry => entry.HouseholdKey)).Distinct()
                .ToList();
            var householdNames = Repository.HouseholdKeys;
            Dictionary<HouseholdKey, string> householdNamesByKey = new Dictionary<HouseholdKey, string>();
            foreach (HouseholdKeyEntry entry in householdNames) {
                householdNamesByKey.Add(entry.HouseholdKey,entry.HouseholdName);
            }
            if (householdNumbers.Count > 1) {
                foreach (var hhnum in householdNumbers) {
                    if (!householdNamesByKey.ContainsKey(hhnum)) {
                        continue;
                    }
                    var columns =
                    (from entry in efc.ColumnEntriesByColumn[dstLoadType].Values
                        where entry.HouseholdKey == hhnum
                        select entry.Column).ToList();
                    var hhname = "." + hhnum + ".";

                    var sumfile =
                        fft.MakeFile<StreamWriter>(
                            "ImportProfile_" + externalFileName + "s" + hhname + dstLoadType.Name + ".csv",
                            "Summed up energy profile without timestamp for " + externalFileName + "s " +
                            dstLoadType.Name + " and HH" + hhnum, true,
                            ResultFileID.PolysunImportFileHH, hhnum, TargetDirectory.Results, fifteenmin,
                            dstLoadType.ConvertToLoadTypeInformation());
                    WritePolysunHeader(sumfile, householdNamesByKey[hhnum], dstLoadType);
                    var timestepcount = 0;
                    var ts = TimeSpan.Zero;
                    for (var outerIndex = 0; outerIndex < energyFileRows.Count; outerIndex += externalfactor) {
                        var efr = energyFileRows[outerIndex];
                        if (!efr.Timestep.DisplayThisStep) {
                            continue;
                        }
                        double sum = 0;

                        for (var innerIndex = outerIndex;
                            innerIndex < outerIndex + externalfactor && innerIndex < energyFileRows.Count;
                            innerIndex++) {
                            var efr2 = energyFileRows[innerIndex];
                            sum += efr2.GetSumForCertainCols(columns);
                            timestepcount++;
                            ts = ts.Add(_calcParameters.InternalStepsize);
                        }
                        sum = sum * dstLoadType.ConversionFactor;
                        var normalstr = ts.TotalSeconds + _calcParameters.CSVCharacter +
                                        sum.ToString("N4", CultureInfo.InvariantCulture);
                        sumfile.WriteLine(normalstr);
                    }
                    if (timestepcount != energyFileRows.Count) {
                        throw new LPGException("Importfilecreator seems to be broken! Please report!");
                    }
                }
            }
        }*/

        public void RunPolysun([NotNull] CalcLoadTypeDto dstLoadType,
                               [NotNull] [ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
                               [NotNull] FileFactoryAndTracker fft,
                               [NotNull] string housename)
        {
            var fifteenMin = new TimeSpan(0, 15, 0);
            var externalfactor = (int)(fifteenMin.TotalSeconds / _calcParameters.InternalStepsize.TotalSeconds);
            if (externalfactor == 1) {
                return;
            }

            var externalFileName = fifteenMin.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            var sumfile = fft.MakeFile<StreamWriter>("ImportProfile." + externalFileName + "s." + dstLoadType.Name + ".csv",
                "Sum energy profile with timestamp for import in Polysun " + externalFileName + "s " + dstLoadType.Name,
                true,
                ResultFileID.PolysunImportFile,
                Constants.GeneralHouseholdKey,
                TargetDirectory.Results,
                fifteenMin,
                dstLoadType.ConvertToLoadTypeInformation());
            //var count = energyFileRows[0].EnergyEntries.Count;
            var ts = TimeSpan.Zero;
            //var timestepcount = 0;
            WritePolysunHeader(sumfile, housename, dstLoadType);

            for (var outerIndex = 0; outerIndex < energyFileRows.Count; outerIndex += externalfactor) {
                var efrOuter = energyFileRows[outerIndex];
                if (!efrOuter.Timestep.DisplayThisStep) {
                    continue;
                }

                double sum = 0;
                for (var innerIndex = outerIndex; innerIndex < outerIndex + externalfactor && innerIndex < energyFileRows.Count; innerIndex++) {
                    var efr2 = energyFileRows[innerIndex];
                    sum += efr2.SumCached;
                    //      timestepcount++;
                }

                var sumPower = sum * dstLoadType.ConversionFactor; // change to power
                var normalstr = ts.TotalSeconds + _calcParameters.CSVCharacter + sumPower.ToString("N8", CultureInfo.InvariantCulture);
                sumfile.WriteLine(normalstr);
            }
            //  if (timestepcount != energyFileRows.Count) { //this does not consider the hidden time steps
            //    throw new LPGException("Importfilecreator seems to be broken! Please report!");
            //}
            sumfile.Flush();
        }

        protected override void PerformActualStep([NotNull] IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            RunPolysun(p.LoadType, p.EnergyFileRows, _fft, Repository.CalcObjectInformation.CalcObjectName);
            //var efc = Repository.ReadEnergyFileColumns(Constants.GeneralHouseholdKey);
            //RunIndividualHouseholdsPolysun(p.LoadType,p.EnergyFileRows,_fft,efc);
        }

        private static void WritePolysunHeader([NotNull] StreamWriter sw, [NotNull] string housename, [NotNull] CalcLoadTypeDto loadtype)
        {
            sw.WriteLine("# Load profile: " + housename);
            sw.WriteLine("# Created with www.loadprofilegenerator.de");
            sw.WriteLine("# For import in Polysun");
            sw.WriteLine("#;");
            sw.WriteLine("# Time [s];" + loadtype.Name + " consumption [" + loadtype.UnitOfSum + "]");
        }
    }
}