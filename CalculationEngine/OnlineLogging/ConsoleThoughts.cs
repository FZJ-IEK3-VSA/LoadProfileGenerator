using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;

namespace CalculationEngine.OnlineLogging {
    public class ConsoleThoughts : IThoughtsLogFile {
        public void Close() {
            // needs to be here for the interface
        }

        public void WriteEntry([NotNull] ThoughtEntry entry, [NotNull] HouseholdKey householdKey) {
            Logger.Info(entry.Timestep + ":" + householdKey + ":" + entry.Person + ":" + entry.Thought);
        }
    }
}