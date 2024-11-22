using Common;
using System.Text;

namespace MassSimulation
{
    internal class TestLogger(string fileName)
    {
        public string Filename { get; } = fileName;

        private List<LogEntry> LogEntries = [];

        private int lastWrittenEntry = 0;
        private int lastTimestep = -1;

        public void Log(TimeStep timestep, DateTime dateTime, string message)
        {
            LogEntries.Add(new(timestep, dateTime, message));
            WriteToFile();
        }

        /// <summary>
        /// Appends all new log entries that have not yet been saved to the log file.
        /// </summary>
        public void WriteToFile()
        {
            var directory = "D:/LPG/MyResults/Logs/";
            Directory.CreateDirectory(directory);
            StringBuilder logMessage = new();
            foreach (LogEntry entry in LogEntries.Skip(lastWrittenEntry))
            {
                string linePrefix = $"{entry.Timestep.InternalStep:0000} - ";
                // if there are multiple lines for the same timestep, skip the prefix for better readability
                if (entry.Timestep.InternalStep == lastTimestep)
                    linePrefix = new string(' ', linePrefix.Length);
                logMessage.Append(linePrefix + entry.Message + Environment.NewLine);
                lastTimestep = entry.Timestep.InternalStep;
            }
            // append new entries to the log file
            File.AppendAllText(directory + Filename, logMessage.ToString());

            // save which entries have been logged already
            lastWrittenEntry = LogEntries.Count;
        }
    }

    /// <summary>
    /// Stores a single log file entry.
    /// </summary>
    /// <param name="Timestep">simulation timestep of the log message</param>
    /// <param name="DateTime">simulation time for the log message</param>
    /// <param name="Message">the log message</param>
    internal record LogEntry(TimeStep Timestep, DateTime DateTime, string Message);
}
