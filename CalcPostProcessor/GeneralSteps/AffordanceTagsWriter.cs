using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.GeneralSteps
{
    public class AffordanceTagsWriter: GeneralStepBase
    {
        [NotNull]
        private readonly FileFactoryAndTracker _fft;

        private void WriteAffordanceTags()
        {
            if (Repository.CalcParameters.IsSet(CalcOption.HouseholdContents))
            {
                using (var swTagSet = _fft.MakeFile<StreamWriter>("AffordanceTags.txt",
                    "All Affordances with tags", false, ResultFileID.AffordanceTagsFile, Constants.GeneralHouseholdKey,
                    TargetDirectory.Root, Repository.CalcParameters.InternalStepsize))
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

        public AffordanceTagsWriter([NotNull] CalcDataRepository repository, [NotNull] ICalculationProfiler calculationProfiler,
                                    [NotNull] FileFactoryAndTracker fft)
            : base(repository, AutomationUtili.GetOptionList(CalcOption.HouseholdContents), calculationProfiler, "Affordance Tags")
        {
            _fft = fft;
        }

        protected override void PerformActualStep([NotNull] IStepParameters parameters)
        {
            WriteAffordanceTags();
        }
    }
}
