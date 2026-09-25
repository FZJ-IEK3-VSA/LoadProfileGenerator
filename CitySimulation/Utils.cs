using Common;

namespace CitySimulation
{
    /// <summary>
    /// A simple logger wrapper for MPI programs, that only logs to console in a 
    /// specific MPI rank to avoid duplicate log messages.
    /// Uses the LPG Logger.
    /// </summary>
    internal class MPILogger(bool logToConsole, int rank)
    {
        // the rank responsible for logging to console
        private const int LoggingRank = 0;
        public Logger Logger { get; } = new Logger(logToConsole && rank == LoggingRank);

        public void SetLogFilePath(string path)
        {
            Logger.LogToFile = true;
            Logger.SetLogFilePath(path);
        }

        public void Debug(string message)
        {
            Logger.DebugMessage(message);
        }

        public void Info(string message)
        {
            Logger.InfoMessage(message);
        }

        public void Warning(string message)
        {
            Logger.WarningMessage(message);
        }

        public void Error(string message)
        {
            Logger.ErrorMessage(message);
        }
    }
}
