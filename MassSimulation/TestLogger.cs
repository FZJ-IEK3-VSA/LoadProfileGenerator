using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    internal class TestLogger
    {
        private List<LogEntry> LogEntries = [];

        public void Log(TimeStep timestep, DateTime dateTime, string message)
        {
            LogEntries.Add(new(timestep, dateTime, message));
        }

        public void WriteToFile(string filename)
        {
            var directory = "D:/LPG/MyResults/Logs/";
            Directory.CreateDirectory(directory);
            File.WriteAllLines(directory + filename, LogEntries.Select(e => $"{e.Timestep} - {e.Message}"));
        }
    }


    internal record LogEntry(TimeStep Timestep, DateTime DateTime, string Message)
    { }
}
