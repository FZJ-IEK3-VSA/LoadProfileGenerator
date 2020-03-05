// // ProfileGenerator ChartCreator2 changed: 2018 12 02 18:42

using System.Collections.Generic;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace ChartCreator2.OxyCharts {
    public interface IChartMakerStep {
        bool IsEnabled([NotNull] ResultFileEntry resultFileEntry);
        FileProcessingResult MakePlot([NotNull] ResultFileEntry resultFileEntry);
        [NotNull]
        List<ResultFileID> ResultFileIDs { get; }
    }

    public interface ISqlChartMakerStep
    {
        bool IsEnabled([NotNull] HouseholdKeyEntry hhkey, [NotNull] ResultTableDefinition resultTable);
        FileProcessingResult MakePlot([NotNull] HouseholdKeyEntry hhkey);
         [NotNull]
         List<ResultTableID> ValidResultTableIds { get; }
    }
}