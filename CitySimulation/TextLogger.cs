using Common;
using System.Text;

namespace CitySimulation
{
    internal class TextLogger
    {
        private readonly bool csvMode;
        private List<LogEntry> logEntries = [];

        private int lastWrittenEntry = 0;
        private string lastPrefix = "";

        public TextLogger(string fileName, string outputDirectory, string subdirectory = "", bool csvMode = false, string contentTitle = "Message")
        {
            var outputSubDirectory = Path.Combine(outputDirectory, Constants.CityLogDirectory, subdirectory);
            Directory.CreateDirectory(outputSubDirectory);
            this.csvMode = csvMode;
            Filename = fileName;
            Filepath = Path.Combine(outputSubDirectory, Filename);

            if (this.csvMode)
            {
                // write a header line
                var line = $"Index,Datetime,{contentTitle}{Environment.NewLine}";
                File.AppendAllText(Filepath, line);
            }
        }

        public string Filename { get; }
        public string Filepath { get; }

        public void Log(TimeStep timestep, DateTime dateTime, string message)
        {
            logEntries.Add(new(timestep, dateTime, message));
            WriteToFile();
        }

        /// <summary>
        /// Appends all new log entries that have not yet been saved to the log file.
        /// </summary>
        public void WriteToFile()
        {
            StringBuilder logMessage = new();
            foreach (LogEntry entry in logEntries.Skip(lastWrittenEntry))
            {
                string dateString = entry.DateTime.ToString("O");
                string linePrefix;
                if (csvMode)
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
            File.AppendAllText(Filepath, logMessage.ToString());

            // save which entries have been logged already
            lastWrittenEntry = logEntries.Count;
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
