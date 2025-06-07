using Common;

namespace CitySimulation
{
    /// <summary>
    /// A simple logger wrapper for MPI programs, that only logs to console in a 
    /// specific MPI rank to avoid duplicate log messages.
    /// Uses the LPG Logger.
    /// </summary>
    internal class MPILogger
    {
        // the rank responsible for logging to console
        private const int LoggingRank = 0;

        // rank of the MPI process this logger is used in
        private readonly int rank;
        private readonly Logger logger;

        public MPILogger(bool logToConsole, int rank)
        {
            this.rank = rank;
            logger = new Logger(logToConsole && rank == LoggingRank);
        }

        public void SetLogFilePath(string path)
        {
            Logger.LogToFile = true;
            Logger.SetLogFilePath(path);
        }

        public void Debug(string message)
        {
            logger.DebugMessage(message);
        }

        public void Info(string message)
        {
            logger.InfoMessage(message);
        }

        public void Warning(string message)
        {
            logger.WarningMessage(message);
        }

        public void Error(string message)
        {
            logger.ErrorMessage(message);
        }
    }
}
