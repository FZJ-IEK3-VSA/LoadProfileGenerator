using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.SQLResultLogging.Loggers {
    public interface IHouseholdKey {
        [NotNull]
        HouseholdKey HouseholdKey { get; }
    }
}