using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

namespace Common.SQLResultLogging.Loggers {
    public class LogMessageEntry {
        [UsedImplicitly]
        public LogMessageEntry([NotNull] string message, DateTime dt, Severity sv, [CanBeNull] string stackTrace,TimeSpan relativeTime)
        {
            RelativeTime = relativeTime;
            Message = message;
            Time = dt;
            Severity = sv;
            MyStackTrace = stackTrace;
        }

        [NotNull]
        public string Message { get; }

        [CanBeNull]
        public string MyStackTrace { get; }

        public Severity Severity { get; }

        [UsedImplicitly]
        public DateTime Time { get; }
        [UsedImplicitly]
        public TimeSpan RelativeTime { get; }
    }

    public class LogMessageLogger : DataSaverBase {
        public const string NormalTableName = "LogMessages";
        public const string ErrorTableName = "LogMessages";

        public enum ErrorMessageType {
            All,
            Errors
        }

        public LogMessageLogger([CanBeNull] SqlResultLoggingService srls, ErrorMessageType errorMessageType) :
            base(typeof(PersonStatus), new ResultTableDefinition(errorMessageType== ErrorMessageType.All? NormalTableName:ErrorTableName, ResultTableID.LogMessages,
                "All Log Messages from the calculation", CalcOption.LogAllMessages), srls)
        {
        }

        [ItemNotNull]
        [NotNull]
        public List<PersonStatus> Load([NotNull] HouseholdKey hhkey)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<PersonStatus>(ResultTableDefinition, hhkey, ExpectedResultCount.OneOrMore);
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<LogMessageEntry>)o;
            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("Time", SqliteDataType.DateTime);
            se.AddField("RelativeTime", SqliteDataType.DateTime);
            se.AddField("Message", SqliteDataType.Text);
            se.AddField("Severity", SqliteDataType.Text);
            se.AddField("MyStackTrace", SqliteDataType.Text);
            foreach (var lme in objects) {
                se.AddRow(RowBuilder.Start("Time", lme.Time)
                    .Add("Message", lme.Message)
                    .Add("RelativeTime", lme.RelativeTime)
                    .Add("Severity", lme.Severity.ToString())
                    .Add("MyStackTrace", lme.MyStackTrace)
                    .ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }
    }
}