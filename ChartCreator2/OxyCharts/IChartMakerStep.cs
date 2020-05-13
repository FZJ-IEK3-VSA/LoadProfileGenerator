// // ProfileGenerator ChartCreator2 changed: 2018 12 02 18:42

using System.Collections.Generic;
using Automation.ResultFiles;
using Common.SQLResultLogging;

namespace ChartCreator2.OxyCharts {
    public interface IChartMakerStep {
        bool IsEnabled([JetBrains.Annotations.NotNull] ResultFileEntry resultFileEntry);
        FileProcessingResult MakePlot([JetBrains.Annotations.NotNull] ResultFileEntry resultFileEntry);
        [JetBrains.Annotations.NotNull]
        List<ResultFileID> ResultFileIDs { get; }
    }

    public interface ISqlChartMakerStep
    {
        bool IsEnabled([JetBrains.Annotations.NotNull] HouseholdKeyEntry hhkey, [JetBrains.Annotations.NotNull] ResultTableDefinition resultTable);
        FileProcessingResult MakePlot([JetBrains.Annotations.NotNull] HouseholdKeyEntry hhkey);
         [JetBrains.Annotations.NotNull]
         List<ResultTableID> ValidResultTableIds { get; }
    }
}