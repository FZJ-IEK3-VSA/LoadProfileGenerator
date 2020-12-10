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

        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public ImportFileCreatorPolysun([NotNull] CalcDataRepository repository,
                                        [NotNull] ICalculationProfiler profiler,
                                        [NotNull] IFileFactoryAndTracker fft) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.PolysunImportFiles),
            profiler,
            "Polysun Import Files")
        {
            _fft = fft;
        }
        /*
        public void RunIndividualHouseholdsPolysun([JetBrains.Annotations.NotNull] CalcLoadTypeDto dstLoadType,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows, [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, [JetBrains.Annotations.NotNull] EnergyFileColumns efc)
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
                               [NotNull] IFileFactoryAndTracker fft,
                               [NotNull] string housename)
        {
            var calcParameters = Repository.CalcParameters;
            var fifteenMin = new TimeSpan(0, 15, 0);
            var externalfactor = (int)(fifteenMin.TotalSeconds / calcParameters.InternalStepsize.TotalSeconds);
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
                fifteenMin,CalcOption.PolysunImportFiles,
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
                var normalstr = ts.TotalSeconds + calcParameters.CSVCharacter + sumPower.ToString("N8", CultureInfo.InvariantCulture);
                sumfile.WriteLine(normalstr);
            }
            //  if (timestepcount != energyFileRows.Count) { //this does not consider the hidden time steps
            //    throw new LPGException("Importfilecreator seems to be broken! Please report!");
            //}
            sumfile.Flush();
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            RunPolysun(p.LoadType, p.EnergyFileRows, _fft, Repository.CalcObjectInformation.CalcObjectName);
            //var efc = Repository.ReadEnergyFileColumns(Constants.GeneralHouseholdKey);
            //RunIndividualHouseholdsPolysun(p.LoadType,p.EnergyFileRows,_fft,efc);
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.DetailedDatFiles, CalcOption.HouseholdContents};

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