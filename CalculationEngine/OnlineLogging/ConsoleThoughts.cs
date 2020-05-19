using Automation.ResultFiles;
using Common;

namespace CalculationEngine.OnlineLogging {
    public class ConsoleThoughts : IThoughtsLogFile {
        public void Dispose() {
            // needs to be here for the interface
        }

        public void WriteEntry(ThoughtEntry entry, HouseholdKey householdKey) {
            Logger.Info(entry.Timestep + ":" + householdKey + ":" + entry.Person + ":" + entry.Thought);
        }
    }
}