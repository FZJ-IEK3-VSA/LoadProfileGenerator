using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.GeneralSteps
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class AffordanceTagsWriter: GeneralStepBase
    {
        [JetBrains.Annotations.NotNull]
        private readonly IFileFactoryAndTracker _fft;

        private void WriteAffordanceTags()
        {
            if (Repository.CalcParameters.IsSet(CalcOption.HouseholdContents))
            {
                using (var swTagSet = _fft.MakeFile<StreamWriter>("AffordanceTags.txt",
                    "All Affordances with tags", false, ResultFileID.AffordanceTagsFile, Constants.GeneralHouseholdKey,
                    TargetDirectory.Root, Repository.CalcParameters.InternalStepsize, CalcOption.HouseholdContents))
                {
                    foreach (var taggingSet in Repository.AffordanceTaggingSets)
                    {
                        swTagSet.WriteLine("###");
                        swTagSet.WriteLine(taggingSet.Name);
                        swTagSet.WriteLine("###");
                        foreach (var tag in taggingSet.AffordanceToTagDict)
                        {
                            swTagSet.WriteLine(tag.Key + Repository.CalcParameters.CSVCharacter + tag.Value);
                        }
                    }
                }
            }
        }

        public AffordanceTagsWriter([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                    [JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.HouseholdContents), calculationProfiler, "Affordance Tags",0)
        {
            _fft = fft;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            WriteAffordanceTags();
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>();
    }
}
