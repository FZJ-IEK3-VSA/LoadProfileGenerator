using Common;
using System.Diagnostics;
using System.Text;

namespace CitySimulation
{
    internal abstract class TextLogger
    {
        private List<LogEntry> logEntries = [];

        /// <summary>
        /// Determines how many messages are collected until they
        /// are written to file. 0 means every message is immediately
        /// written to file.
        /// </summary>
        private const int MessageFlushCount = 100;

        public TextLogger(string fileName, string outputDirectory, string subdirectory = "")
        {
            var outputSubDirectory = Path.Combine(outputDirectory, Constants.CityLogDirectory, subdirectory);
            Directory.CreateDirectory(outputSubDirectory);
            Filename = fileName;
            Filepath = Path.Combine(outputSubDirectory, Filename);
        }

        /// <summary>
        /// Name of the log file
        /// </summary>
        public string Filename { get; }
        /// <summary>
        /// Full path of the log file
        /// </summary>
        public string Filepath { get; }

        /// <summary>
        /// Log a single string message
        /// </summary>
        /// <param name="timestep">current timestep</param>
        /// <param name="dateTime">current datetime</param>
        /// <param name="message">the message to log</param>
        public void Log(TimeStep timestep, DateTime dateTime, string message)
            => Log(timestep, dateTime, [message]);

        /// <summary>
        /// Logs a list of items. How the data is handled depends on the CreateLine method.
        /// </summary>
        /// <param name="timestep">current timestep</param>
        /// <param name="dateTime">current datetime</param>
        /// <param name="data">the data to log</param>
        public void Log(TimeStep timestep, DateTime dateTime, object[] data)
        {
            logEntries.Add(new(timestep, dateTime, data));

            // write to file if enough log entries have accumulated
            if (logEntries.Count > MessageFlushCount)
                WriteToFile();
        }

        /// <summary>
        /// Turns a single log entry into a line for the log file.
        /// </summary>
        /// <param name="entry">the entry to log</param>
        /// <returns>the line for the log file, without newline char</returns>
        protected abstract string CreateLine(LogEntry entry);

        /// <summary>
        /// Appends all new log entries that have not yet been saved to the log file.
        /// </summary>
        public void WriteToFile()
        {
            StringBuilder logMessage = new();
            foreach (LogEntry entry in logEntries)
            {
                string line = CreateLine(entry);
                logMessage.Append(line + Environment.NewLine);
            }
            // append new entries to the log file
            File.AppendAllText(Filepath, logMessage.ToString());

            // clear the list of log entries
            logEntries = [];
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

        public CsvLogger(string fileName, string outputDirectory, IEnumerable<string> columns, string subdirectory = "") : base(fileName, outputDirectory, subdirectory)
        {
            AddColumns(columns);
        }

        /// <summary>
        /// Column names for the CSV file
        /// </summary>
        public List<string> Columns { get; protected set; } = [];

        /// <summary>
        /// Writes the header line into the CSV file.
        /// </summary>
        protected void WriteHeaderLine()
        {
            string columString = string.Join(Delimiter, Columns);
            File.WriteAllText(Filepath, columString + Environment.NewLine);
        }

        /// <summary>
        /// Adds more columns for the CSV file, and rewrites the header. May only be called
        /// before any actual data is logged.
        /// </summary>
        /// <param name="columns">the column names to add</param>
        public void AddColumns(IEnumerable<string> columns)
        {
            Debug.Assert(File.ReadAllLines(Filepath).Length > 1, "CSV columns may only be added before logging any data.");
            Columns.AddRange(columns);
            WriteHeaderLine();
        }

        protected override string CreateLine(LogEntry entry)
        {
            return string.Join(Delimiter, entry.Data);
        }
    }

    /// <summary>
    /// CSV logger that automatically logs timestep and datetime as first two columns.
    /// </summary>
    internal class CsvIndexDateLogger : CsvLogger
    {
        /// <summary>
        /// Additional index columns that are always included as the first two columns for this logger
        /// </summary>
        protected static readonly string[] indexColumns = ["Timestep", "Datetime"];

        public CsvIndexDateLogger(string fileName, string outputDirectory, IEnumerable<string> columns, string subdirectory = "")
            : base(fileName, outputDirectory, indexColumns.Concat(columns), subdirectory)
        { }

        protected override string CreateLine(LogEntry entry)
        {
            string dateString = entry.DateTime.ToString("O");
            string linePrefix = $"{entry.Timestep.InternalStep}{Delimiter}{dateString}{Delimiter}";
            return linePrefix + base.CreateLine(entry);
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
