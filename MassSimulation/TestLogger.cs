using Common;
using System.Text;

namespace MassSimulation
{
    internal class TestLogger(string fileName)
    {
        public string Filename { get; } = fileName;

        private List<LogEntry> LogEntries = [];

        public void Log(TimeStep timestep, DateTime dateTime, string message)
        {
            LogEntries.Add(new(timestep, dateTime, message));
            WriteToFile(); // TODO: temporary solution for debugging
        }

        public void WriteToFile()
        {
            var directory = "D:/LPG/MyResults/Logs/";
            Directory.CreateDirectory(directory);
            StringBuilder logMessage = new();
            int lastTimestep = -1;
            foreach (LogEntry entry in LogEntries)
            {
                string linePrefix = $"{entry.Timestep.InternalStep:0000} - ";
                // if there are multiple lines for the same timestep, skip the prefix for better readability
                if (entry.Timestep.InternalStep == lastTimestep)
                    linePrefix = new string(' ', linePrefix.Length);
                logMessage.Append(linePrefix + entry.Message + Environment.NewLine);
                lastTimestep = entry.Timestep.InternalStep;
            }
            File.WriteAllText(directory + Filename, logMessage.ToString());
        }
    }


    internal record LogEntry(TimeStep Timestep, DateTime DateTime, string Message)
    { }
}
