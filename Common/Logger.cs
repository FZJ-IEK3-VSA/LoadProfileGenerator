//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using JetBrains.Annotations;
using Xunit.Abstractions;

#endregion

namespace Common {
    public enum Severity {
        Error,
        Warning,
        ImportantInfo,
        Information,
        Debug
    }
    public static class UnitTestDetector
    {
        public static readonly HashSet<string> UnitTestAttributes = new HashSet<string>
        {
            "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute",
            "NUnit.Framework.TestFixtureAttribute",
            "XUnit.FactAttribute"
        };

        private static bool? _isRunning;


        public static bool IsRunningInUnitTest
        {
            get {
                if (_isRunning != null) {
                    return _isRunning.Value;
                }

                var stcktrace = new StackTrace().GetFrames();
                if (stcktrace == null) {
                    throw new LPGException("stacktrace was null");

                }
                foreach (var f in stcktrace) {
                    var g = f.GetMethod().DeclaringType;
                    if (g == null) {
                        throw new LPGException("declaringtype was null");
                    }
                    if (g.GetCustomAttributes(false).Any(x => UnitTestAttributes.Contains(x.GetType().FullName))) {
                        _isRunning = true;
                        return true;
                    }
                }

                _isRunning = false;
                return false;
            }
        }
    }

    public class Logger {
        [NotNull]
        private static readonly Logger _logger = new Logger();

        private static bool _logToFile;

        [ItemNotNull]
        [NotNull]
        private readonly List<LogMessage> _errors;
        [ItemNotNull]
        [NotNull]
        private readonly ObservableCollection<LogMessage> _logCol;
        [NotNull]
        private readonly object _myLock = new object();

        [CanBeNull]
        [ItemNotNull]
        public List<LogMessage> CollectedCalculationMessages { get; set; }

        [NotNull]
        public string ReturnAllLoggedErrors()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var error in _errors) {
                sb.Append(error.Message).Append("\n");
            }
            return sb.ToString();
        }

        private Logger()
        {
            Console.WriteLine("Initializing the logger");
            //LogFilePath = string.Empty;
            _logCol = new ObservableCollection<LogMessage>();
            _errors = new List<LogMessage>();
        }

        [NotNull]
        [ItemNotNull]
        public List<LogMessage> Errors => _errors;

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<LogMessage> LogCol => _logCol;

        [UsedImplicitly]
        [NotNull]
        public static string LogFileIndex {
            get => _logFileIndex;
            set {
                _logFileIndex = value;
                WriteCurrentFileLoggingStatus("I");
            }
        }

        public static void SetLogFilePath([NotNull] string val) {
            _logFilePath = val;
            WriteCurrentFileLoggingStatus("P");
        }

        private static void WriteCurrentFileLoggingStatus([NotNull] string source)
        {
                Console.WriteLine( source + " Logfile path: " + (_logFilePath ?? "(null)") + ", Logging to file: " + _logToFile + ", Severity: " + Threshold.ToString() + ", LogFileIndex: " + _logFileIndex );
        }
        [UsedImplicitly]
        public static bool LogToFile {
            get => _logToFile;
            set {
                _logToFile = value;
                WriteCurrentFileLoggingStatus("L");
            }
        }

        /*[CanBeNull]
        public ListView Lv { get; set; }
        */
        [UsedImplicitly]
        public static Severity Threshold {
            get => _threshold;
            set {
                _threshold = value;
                WriteCurrentFileLoggingStatus("T");
            }
        }

        public void ClearOldErrors()
        {
            _errors.Clear();
        }

        public static void Debug([NotNull] string message)
        {
            _logger.DebugMessage(message);
        }

        private void DebugMessage([NotNull] string message)
        {
            ReportString(message, Severity.Debug);
        }

        public static void Error([NotNull] string message)
        {
            _logger.ErrorMessage(message);
        }

        private void ErrorMessage([NotNull] string message)
        {
            ReportString(message, Severity.Error);
        }

        public static void Exception([NotNull] Exception ex, bool reporterror = true)
        {
            if (reporterror) {
                var t = new Thread(() => {
                    var er = new ErrorReporter();
                    er.Run(ex.Message, ex.StackTrace);
                });
                t.Start();
            }
            _logger.ExceptionMessage(ex);
        }

        private void ExceptionMessage([NotNull] Exception exception)
        {
            var sb = new StringBuilder();
            sb.Append("Exception occured:").Append(Environment.NewLine);
            sb.Append(exception.Message);
            sb.Append(Environment.NewLine);
            sb.Append(exception.StackTrace);
            sb.Append(Environment.NewLine).Append(Environment.NewLine);
            if (exception.InnerException != null) {
                sb.Append("Inner Exception:").Append(Environment.NewLine);
                sb.Append(exception.InnerException.Message);
                sb.Append(Environment.NewLine).Append(Environment.NewLine);
                sb.Append(exception.InnerException.StackTrace);
            }
            ReportString(sb.ToString(), Severity.Error);
        }

        [NotNull]
        public static Logger Get() => _logger;

        [NotNull]
        private static string GetFilename(Severity severity, bool isForCommonFile)
        {
            if (!string.IsNullOrWhiteSpace(_logFilePath)) {
                return _logFilePath;
            }
            var logfilename = "Log.txt";
            if (!string.IsNullOrWhiteSpace(LogFileIndex))
            {
                logfilename = "Log." + LogFileIndex + ".txt";
            }
            if (isForCommonFile) {
                return logfilename;
            }

            var fi = new FileInfo(logfilename);

            var purename = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
            var newfilename = purename + "." + severity + fi.Extension;
            if(fi.Directory == null) {
                throw new LPGException("Directory was null");
            }
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            var newFullName = Path.Combine(fi.Directory.FullName, newfilename);
            return newFullName;
        }

        public static void ImportantInfo([NotNull] string message)
        {
            _logger.ImportantInfoMessage(message);
        }

        private void ImportantInfoMessage([NotNull] string message)
        {
            ReportString(message, Severity.ImportantInfo);
        }

        public static void Info([NotNull] string message, bool preserveLinebreaks = false)
        {
            _logger.InfoMessage(message, preserveLinebreaks);
        }

        private void InfoMessage([NotNull] string message, bool preserveLinebreaks = false)
        {
            ReportString(message, Severity.Information,preserveLinebreaks);
        }
        [NotNull] private static readonly object _fileLogLock = new object();
        [CanBeNull] private static string _logFilePath;

        private static Severity _threshold = Severity.Debug;
        [NotNull] private static string _logFileIndex = string.Empty;

        private static void LogStringToFile([NotNull] string message, [NotNull] string logfilename, Severity severity, bool preserveNewLines)
        {
            lock (_fileLogLock) {
                using (var sw = new StreamWriter(logfilename, true)) {
                    var sev = SeverityHelper.SeverityShortName[severity];
                    string msg = message;
                    if (!preserveNewLines) {
                        msg = message.Replace(Environment.NewLine, " ");
                    }
                    var s = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture) + " [" + sev +
                            "] " +msg;
                    sw.WriteLine(s);
                }
            }
        }

        public void StartCollectingAllMessages()
        {
            CollectedCalculationMessages = new List<LogMessage>();
        }

        public void FlushExistingMessages()
        {

            Console.WriteLine("------------ Flushing---------------");
            var messages = GetAndClearAllCollectedMessages();
            string filename = GetFilename(Severity.Debug, true);
            StreamWriter sw = new StreamWriter(filename);
            Console.WriteLine("------------ Total messages:  " + messages.Count + "---------------");
            sw.WriteLine("Starting the log");
            sw.Close();
            LogStringToFile("Writing" + messages.Count + " cached  collected log messaged.", filename, Severity.ImportantInfo, true);
            foreach (LogMessage message in messages) {
                LogStringToFile(message.Message,filename,message.Severity, true);
            }
            //LogStringToFile("Finished writing" + messages.Count + " cached  collected log messaged.", filename, Severity.ImportantInfo);
        }


        [ItemNotNull]
        [NotNull]
        public List<LogMessage> GetAndClearAllCollectedMessages()
        {
            if(CollectedCalculationMessages == null) {
                throw new LPGException("Tried to collect non-saved log messages");
            }

            var messages = CollectedCalculationMessages;
            CollectedCalculationMessages = null;
            return messages;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ReportString([NotNull] string message, Severity severity, bool preserveLinebreaks = false)
        {
            if (UnitTestDetector.IsRunningInUnitTest && OutputHelper== null) {
                throw new LPGException("no output helper even thoug we are in unit testing based on attributes");
            }

            if (Config.IsInUnitTesting&&OutputHelper ==null) {
                throw new LPGException("no output helper even thoug we are in unit testing based on config");
            }
            if(OutputHelper!= null) {
                OutputHelper.WriteLine(message);
            }
            else {
                Console.WriteLine(message);
            }

            if (LogToFile) {
                try {
                    var logfilename = GetFilename(severity, true);
                    LogStringToFile(message, logfilename, severity, preserveLinebreaks);
                    if (severity == Severity.Error) {
                        var errorlogfilename = GetFilename(severity, false);
                        LogStringToFile(message, errorlogfilename, severity, preserveLinebreaks);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
            if (severity > Threshold) {
                return;
            }
            lock (_myLock) {
                void ReduceCollectionItems()
                {
                    var lm = new LogMessage(message, DateTime.Now, severity);
                    if (CollectedCalculationMessages != null)
                    {
                        CollectedCalculationMessages.Add(lm);
                    }
                    while (_logCol.Count > 100) {
                        _logCol.RemoveAt(100);
                    }

                    _logCol.Insert(0, lm);
                    if (severity == Severity.Error) {
                        _errors.Add(lm);
                        while (_errors.Count > 20) {
                            _errors.RemoveAt(20);
                        }
                    }
                }
                //if running without user interface:
                SafeExecute(ReduceCollectionItems);
            }
        }

        [CanBeNull]
        public Action<Action> SaveExecutionFunction { get; set; } = null;
        [CanBeNull]
        public Action<Action> SaveExecutionFunctionWithWait { get; set; } = null;

        public void SafeExecute([NotNull] Action action)
        {
            if (SaveExecutionFunction != null) {
                SaveExecutionFunction(action);

            }
            else {
                action();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
                public void SafeExecuteWithWait([NotNull] Action action)
                {
                    if(SaveExecutionFunctionWithWait!=null) {
                        SaveExecutionFunctionWithWait(action);
                    }
                    else
                    {
                        action();
                    }
                }


        public void ThrowAllErrors()
        {
            var sb = new StringBuilder();
            int i = 1;
            foreach (var logMessage in _errors) {
                sb.AppendLine();
                sb.AppendLine("##################################################");
                sb.AppendLine(i.ToString());
                sb.AppendLine("##################################################");
                sb.AppendLine();

                sb.AppendLine(logMessage.Message);
                if (logMessage.MyStackTrace != null) {
                    sb.AppendLine(logMessage.MyStackTrace.ToString());
                }
                sb.AppendLine().AppendLine();
                i++;
            }
            _errors.Clear();
            throw new LPGException(sb.ToString());
        }

        public static void Warning([NotNull] string message)
        {
            _logger.WarningMessage(message);
        }

        private void WarningMessage([NotNull] string message)
        {
            ReportString(message, Severity.Warning);
        }

        public class LogMessage {
            [UsedImplicitly]
            public LogMessage([NotNull] string message, DateTime dt, Severity sv)
            {
                Message = message;
                Time = dt;
                Severity = sv;
                if (Severity == Severity.Error) {
                    MyStackTrace = new StackTrace();
                }
            }

            [UsedImplicitly]
            public ColorRGB BackColor {
                get {
                    switch (Severity) {
                        case Severity.Error:
                            return ColorConstants.Red;
                        case Severity.Warning:
                            return ColorConstants.Orange;
                        case Severity.ImportantInfo:
                            return ColorConstants.DarkOrange;
                        case Severity.Information:
                            return ColorConstants.DeepSkyBlue;
                        case Severity.Debug:
                            return ColorConstants.AntiqueWhite;
                        default:
                            return ColorConstants.Black;
                    }
                }
            }
            [CanBeNull]
            public StackTrace MyStackTrace { get; }

            [UsedImplicitly]
            [NotNull]
            public string Message { get;  }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
            public Severity Severity { get;  }

            [UsedImplicitly]
            public DateTime Time { get;  }

            [NotNull]
            public override string ToString() => "[" + Severity + "] " + Message ;
        }

        public void SetOutputHelper(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public ITestOutputHelper OutputHelper { get; set; }
    }
}