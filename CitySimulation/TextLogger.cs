using Common;
using System.Text;

namespace CitySimulation
{
    internal abstract class TextLogger
    {
        private List<LogEntry> logEntries = [];
        private int lastWrittenEntry = 0;

        public TextLogger(string fileName, string outputDirectory, string subdirectory = "")
        {
            var outputSubDirectory = Path.Combine(outputDirectory, Constants.CityLogDirectory, subdirectory);
            Directory.CreateDirectory(outputSubDirectory);
            Filename = fileName;
            Filepath = Path.Combine(outputSubDirectory, Filename);
        }

        public string Filename { get; }
        public string Filepath { get; }

        public void Log(TimeStep timestep, DateTime dateTime, string message)
        {
            logEntries.Add(new(timestep, dateTime, [message]));
            WriteToFile();
        }

        public void Log(TimeStep timestep, DateTime dateTime, object[] data)
        {
            logEntries.Add(new(timestep, dateTime, data));
            WriteToFile();
        }

        protected abstract string CreateLine(LogEntry entry);

        /// <summary>
        /// Appends all new log entries that have not yet been saved to the log file.
        /// </summary>
        public void WriteToFile()
        {
            StringBuilder logMessage = new();
            foreach (LogEntry entry in logEntries.Skip(lastWrittenEntry))
            {
                string line = CreateLine(entry);
                logMessage.Append(line + Environment.NewLine);
            }
            // append new entries to the log file
            File.AppendAllText(Filepath, logMessage.ToString());

            // save which entries have been logged already
            lastWrittenEntry = logEntries.Count;
        }
    }

    /// <summary>
    /// Logger for logging free text in a human-readable format, with 
    /// additional whitespaces for indentation and number alignment
    /// </summary>
    internal class FreeTextLogger(string fileName, string outputDirectory, string subdirectory = "") : TextLogger(fileName, outputDirectory, subdirectory)
    {
        /// <summary>
        /// Stores the prefix of the last log line to avoid repetitions
        /// </summary>
        private string lastPrefix = "";

        protected override string CreateLine(LogEntry entry)
        {
            string dateString = entry.DateTime.ToString("O");
            string linePrefix = $"{entry.Timestep.InternalStep:000000} {dateString} - ";
            // if there are multiple lines for the same timestep, skip the prefix for better readability
            if (linePrefix == lastPrefix)
                linePrefix = new string(' ', lastPrefix.Length);
            lastPrefix = linePrefix;
            if (entry.Data.Length > 1)
            {
                return linePrefix + entry.Data.ToString();
            }
            else
            {
                return linePrefix + entry.Data[0];
            }
        }
    }

    /// <summary>
    /// Logger for creating a CSV file without additional whitespace or formatting for readability.
    /// </summary>
    internal class CsvLogger : TextLogger
    {
        /// <summary>
        /// Column delimiter for the CSV file
        /// </summary>
        public const string Delimiter = ",";

        /// <summary>
        /// Column names for the CSV file
        /// </summary>
        public readonly string[] Columns;

        public CsvLogger(string fileName, string outputDirectory, string[] columns, string subdirectory = "") : base(fileName, outputDirectory, subdirectory)
        {
            Columns = columns;
            string[] fixedColumns = ["Timestep", "Datetime"];
            var allColumns = fixedColumns.Concat(columns);
            string columString = string.Join(Delimiter, allColumns);
            // write a header line
            File.AppendAllText(Filepath, columString + Environment.NewLine);
        }

        protected override string CreateLine(LogEntry entry)
        {
            string dateString = entry.DateTime.ToString("O");
            string linePrefix = $"{entry.Timestep.InternalStep},{dateString},";
            return linePrefix + string.Join(Delimiter, entry.Data);
        }
    }

    /// <summary>
    /// Stores a single log file entry.
    /// </summary>
    /// <param name="Timestep">simulation timestep of the log message</param>
    /// <param name="DateTime">simulation time for the log message</param>
    /// <param name="Data">the logged data</param>
    internal record LogEntry(TimeStep Timestep, DateTime DateTime, object[] Data);
}
