using Common;
using System.Text;

namespace MassSimulation
{
    internal class TextLogger(string fileName, string? outputDirectory)
    {

        private readonly string outputDirectory = outputDirectory;
        private List<LogEntry> LogEntries = [];

        private int lastWrittenEntry = 0;
        private string lastPrefix = "";
        public string Filename { get; } = fileName;

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
            var directory = Path.Combine(outputDirectory, "logs");
            Directory.CreateDirectory(directory);
            var logfilePath = Path.Combine(directory, Filename);
            StringBuilder logMessage = new();
            foreach (LogEntry entry in LogEntries.Skip(lastWrittenEntry))
            {
                string linePrefix = $"{entry.Timestep.InternalStep:000000} {entry.DateTime} - ";
                // if there are multiple lines for the same timestep, skip the prefix for better readability
                if (linePrefix == lastPrefix)
                    linePrefix = new string(' ', lastPrefix.Length);
                logMessage.Append(linePrefix + entry.Message + Environment.NewLine);
                lastPrefix = linePrefix;
            }
            // append new entries to the log file
            File.AppendAllText(logfilePath, logMessage.ToString());

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
