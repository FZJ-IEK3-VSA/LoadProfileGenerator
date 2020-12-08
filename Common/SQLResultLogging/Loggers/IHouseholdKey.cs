using Automation.ResultFiles;

namespace Common.SQLResultLogging.Loggers {
    public interface IHouseholdKey {
        [JetBrains.Annotations.NotNull]
        HouseholdKey HouseholdKey { get; }
    }
}