using Common;
using System.Text;

namespace CitySimulation
{
    internal class TextLogger(string fileName, string outputDirectory, string subdirectory = "", bool csvMode = false)
    {
        public const string LOG_SUBDIR = "Logs";
        private readonly string outputDirectory = Path.Combine(outputDirectory, LOG_SUBDIR, subdirectory);
        private readonly bool CsvMode = csvMode;
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
            Directory.CreateDirectory(outputDirectory);
            var logfilePath = Path.Combine(outputDirectory, Filename);
            StringBuilder logMessage = new();
            foreach (LogEntry entry in LogEntries.Skip(lastWrittenEntry))
            {
                string dateString = entry.DateTime.ToString("O");
                string linePrefix;
                if (CsvMode)
                {
                    // csv file mode: log comma-separated values without additional whitespace
                    linePrefix = $"{entry.Timestep.InternalStep},{dateString},";
                } else
                {
                    // log mode: use a more readable format, with whitespaces, number alignment etc.
                    linePrefix = $"{entry.Timestep.InternalStep:000000} {dateString} - ";
                    // if there are multiple lines for the same timestep, skip the prefix for better readability
                    if (linePrefix == lastPrefix)
                        linePrefix = new string(' ', lastPrefix.Length);
                }
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
