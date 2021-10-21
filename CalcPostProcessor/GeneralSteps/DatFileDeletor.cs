using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;

namespace CalcPostProcessor.GeneralSteps
{
    /// <summary>
    /// A postprocessing step that deletes all .dat files created during the calculation
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class DatFileDeletor : GeneralStepBase
    {
        [JetBrains.Annotations.NotNull]
        private readonly SqlResultLoggingService _srls;

        public DatFileDeletor([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                    [JetBrains.Annotations.NotNull] SqlResultLoggingService srls)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.DeleteDatFiles), calculationProfiler, "Delete .dat files", 0)
        {
            _srls = srls;
        }

        /// <summary>
        /// Performs the postprocessing step by calling the method to delete .dat files
        /// </summary>
        /// <param name="parameters"></param>
        protected override void PerformActualStep(IStepParameters parameters)
        {
            DeleteDatFiles();
        }

        /// <summary>
        /// Deletes all .dat files if the CalcOption DeleteDatFiles is set
        /// </summary>
        private void DeleteDatFiles()
        {
            if (Repository.CalcParameters.IsSet(CalcOption.DeleteDatFiles))
            {
                // load a list of all created files from the database
                ResultFileEntryLogger rfel = new ResultFileEntryLogger(_srls);
                var resultFileEntries = rfel.Load();
                // filter all result files ending with .dat
                var datFileEntries = resultFileEntries.Where(f => f.FileName.ToUpperInvariant().EndsWith(".DAT", StringComparison.Ordinal));
                foreach (var datFile in datFileEntries)
                {
                    File.Delete(datFile.FullFileName);
                    // remove file entry from the result file list in the database
                    rfel.DeleteEntry(datFile);
                }
            }
        }

        [JetBrains.Annotations.NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() { CalcOption.DeleteDatFiles };
    }
}
