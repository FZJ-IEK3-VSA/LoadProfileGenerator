using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;

namespace ChartCreator2.OxyCharts {
    internal class AffordanceEnergyUse : ChartBaseFileStep
    {

        public AffordanceEnergyUse([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                   [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                   [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
                calculationProfiler, new List<ResultFileID>() { ResultFileID.AffordanceEnergyUse },
                "Affordance Energy Use", FileProcessingResult.ShouldCreateFiles)
        {
        }

        private void MakeIntervalBars([JetBrains.Annotations.NotNull] ResultFileEntry rfe, [JetBrains.Annotations.NotNull] string plotName, [JetBrains.Annotations.NotNull] DirectoryInfo basisPath,
            [JetBrains.Annotations.NotNull] [ItemNotNull] List<Tuple<string, double>> consumption, [ItemNotNull] [JetBrains.Annotations.NotNull] List<ChartTaggingSet> taggingSets) {
            IntervallBarMaker ivbm = new IntervallBarMaker();
            foreach (var chartTaggingSet in taggingSets) {
                ivbm.MakeIntervalBars( rfe, plotName, basisPath, consumption,chartTaggingSet,chartTaggingSet.Name,false,this);
            }
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry)
        {
            //AffordanceEnergyUseLogger aeul = new AffordanceEnergyUseLogger(_srls);
            string plotName = "Affordance Energy Use " + srcEntry.HouseholdNumberString + " " +
                              srcEntry.LoadTypeInformation?.Name;
            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            const FileProcessingResult fpr = FileProcessingResult.ShouldCreateFiles;
            var consumption = new List<Tuple<string, double>>();
            var taggingSets = new List<ChartTaggingSet>();
            using (var sr = new StreamReader(srcEntry.FullFileName)) {
                var header = sr.ReadLine();
                if (header == null) {
                    throw new LPGException("Affordance Energy Use file was empty.");
                }
                var headerArr = header.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                for (var i = 2; i < headerArr.Length; i++) {
                    if (!string.IsNullOrWhiteSpace(headerArr[i])) {
                        taggingSets.Add(new ChartTaggingSet(headerArr[i]));
                    }
                }
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed.");
                    }
                    var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                    var d = Convert.ToDouble(cols[1], CultureInfo.CurrentCulture);
                    consumption.Add(new Tuple<string, double>(cols[0], d));
                    if (cols.Length < 2) {
                        throw new LPGException("The string " + s +
                                               " did not contain any csv characters, even though it should. Affordance energy use plot could not be created.");
                    }
                    for (var i = 2; i < cols.Length; i++) {
                        taggingSets[i - 2].AffordanceToCategories.Add(cols[0], cols[i]);
                    }
                }
            }
            if(taggingSets.Count == 0) {
                _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
                return FileProcessingResult.NoFilesTocreate;
            }
            MakeIntervalBars(srcEntry,plotName, _Parameters.BaseDirectory, consumption, taggingSets);
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return fpr;
        }
    }
}