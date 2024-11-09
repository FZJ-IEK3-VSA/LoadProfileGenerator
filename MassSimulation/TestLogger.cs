using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            File.WriteAllLines(directory + Filename, LogEntries.Select(e => $"{e.Timestep.InternalStep:0000} - {e.Message}"));
        }
    }


    internal record LogEntry(TimeStep Timestep, DateTime DateTime, string Message)
    { }
}
