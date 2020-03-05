using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;
using Automation.ResultFiles;

namespace Common {
    public static class DirectoryNames {
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        [NotNull]
        public static string CalculateTargetdirectory(TargetDirectory targetDirectory) {
            switch (targetDirectory) {
                case TargetDirectory.Root:
                    return string.Empty;
                case TargetDirectory.Reports:
                    return "Reports";
                case TargetDirectory.Results:
                    return "Results";
                case TargetDirectory.Charts:
                    return "Charts";
                case TargetDirectory.Debugging:
                    return "Debugging";
                case TargetDirectory.Temporary:
                    return "Temporary Files";
                default:
                    throw new LPGException("Forgotten TargetDirectory");
            }
        }
    }
}