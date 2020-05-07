using System.Collections.Generic;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;

namespace ChartCreator2.OxyCharts
{
    public class NoOpChart: ChartBaseFileStep
    {
    public NoOpChart([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                        [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                        [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler)
        : base(parameters, fft,
        calculationProfiler, FilesNotToChart(),
        "Noop Chart", FileProcessingResult.NoFilesTocreate
    )
    {
    }

        [JetBrains.Annotations.NotNull]
        private static List<ResultFileID> FilesNotToChart() {
            var l = new List<ResultFileID>
            {
                ResultFileID.Actions,
                ResultFileID.ActionsJson,
                ResultFileID.ActionsPerStep,
                ResultFileID.ActionsPerStepJson,
                ResultFileID.ActivationsPerHour,
                ResultFileID.AffordanceDefinition,
                ResultFileID.AffordanceInformation,
                ResultFileID.AffordanceTagsFile,
                ResultFileID.AffordanceTimeUse2,
                ResultFileID.AffordanceTimeUseJson,
                ResultFileID.BridgeDays,
                ResultFileID.CarpetPlotLabeled,
                ResultFileID.CarpetPlots,
                ResultFileID.CarpetPlotsEnergy,
                ResultFileID.CarpetPlotsLegend,
                ResultFileID.Chart,
                ResultFileID.Daylight,
                ResultFileID.DeviceProfileCSVExternalForHouseholds,
                ResultFileID.DeviceProfileForHouseholds,
                ResultFileID.DeviceSumsJson,
                ResultFileID.DeviceTags,
                ResultFileID.Dump,
                ResultFileID.DumpTime,
                ResultFileID.EnergyStorages,
                ResultFileID.FiveMinuteImportFile,
                ResultFileID.FiveMinuteImportFileForHH,
                ResultFileID.HouseContents,
                ResultFileID.HouseholdNameFile,
                ResultFileID.JsonResultFileList,
                ResultFileID.Locations,
                ResultFileID.Logfile,
                ResultFileID.LogfileForErrors,
                ResultFileID.MissingTags,
                ResultFileID.OnlineDeviceActivationFiles,
                ResultFileID.OnlineSumActivationFiles,
                ResultFileID.OverallSumFile,
                ResultFileID.PDF,
                ResultFileID.PersonFile,
                ResultFileID.PolysunImportFile,
                ResultFileID.PolysunImportFileHH,
                ResultFileID.ResultFileXML,
                ResultFileID.Seed,
                ResultFileID.SettlementIndividualSumProfile,
                ResultFileID.SettlementIndividualSumProfileExternal,
                ResultFileID.SettlementTotal,
                ResultFileID.SettlementTotalProfile,
                ResultFileID.SettlementTotalProfileExternal,
                ResultFileID.Sqlite,
                ResultFileID.ThoughtsPerPerson,
                ResultFileID.Totals,
                ResultFileID.TotalsJson,
                ResultFileID.Transportation,
                ResultFileID.TransportationStatistics,
                ResultFileID.VacationDays,
                ResultFileID.SumProfileForHouseholds,
                ResultFileID.DeviceSumsPerMonth,
                ResultFileID.ExternalSumsForHouseholds,
                ResultFileID.ExternalSumsForHouseholdsJson,
                ResultFileID.JsonSums
            };

            return l;
    }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcResultFileEntry)
        {
            return FileProcessingResult.NoFilesTocreate;
        }
    }
}
