﻿//-----------------------------------------------------------------------

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
                    var g = f.GetMethod()?.DeclaringType;
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
        [JetBrains.Annotations.NotNull]
        private static readonly Logger _logger = new Logger();

        private static bool _logToFile;


        private bool? logToConsole;

        /// <summary>
        /// Whether or not this Logger should print log messages to the console.
        /// </summary>
        public bool LogToConsole
        {
            // if null, refer to Config instead
            get { return logToConsole ?? Config.OutputToConsole; }
            set { logToConsole = value; }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly List<LogMessage> _errors;
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly ObservableCollection<LogMessage> _logCol;
        [JetBrains.Annotations.NotNull]
        private readonly object _myLock = new object();

        [CanBeNull]
        [ItemNotNull]
        public List<LogMessage> CollectedCalculationMessages { get; set; }

        [JetBrains.Annotations.NotNull]
        public string ReturnAllLoggedErrors()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var error in _errors) {
                sb.Append(error.Message).Append("\n");
            }
            return sb.ToString();
        }

        public Logger(bool? logToConsole = null)
        {
            _logCol = new ObservableCollection<LogMessage>();
            _errors = new List<LogMessage>();
            this.logToConsole = logToConsole;

            if (LogToConsole)
                Console.WriteLine("Initializing the logger");
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<LogMessage> Errors => _errors;

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<LogMessage> LogCol => _logCol;

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public static string LogFileIndex {
            get => _logFileIndex;
            set {
                _logFileIndex = value;
                WriteCurrentFileLoggingStatus("I");
            }
        }

        public static void SetLogFilePath([JetBrains.Annotations.NotNull] string val) {
            _logFilePath = val;
            WriteCurrentFileLoggingStatus("P");
        }

        private static void WriteCurrentFileLoggingStatus([JetBrains.Annotations.NotNull] string source)
        {
            if (_logger.LogToConsole)
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

        public static void Debug([JetBrains.Annotations.NotNull] string message)
        {
            _logger.DebugMessage(message);
        }

        public void DebugMessage([JetBrains.Annotations.NotNull] string message)
        {
            ReportString(message, Severity.Debug);
        }

        public static void Error([JetBrains.Annotations.NotNull] string message)
        {
            _logger.ErrorMessage(message);
        }

        public void ErrorMessage([JetBrains.Annotations.NotNull] string message)
        {
            ReportString(message, Severity.Error);
        }

        public static void Exception([JetBrains.Annotations.NotNull] Exception ex, bool reporterror = true)
        {
            if (reporterror) {
                var t = new Thread(() => {
                    var er = new ErrorReporter();
                    er.Run(ex.Message, ex.StackTrace ?? "no stacktrace");
                });
                t.Start();
            }
            _logger.ExceptionMessage(ex, Utili.GetCallingMethodAndClass());
        }

        private void ExceptionMessage([JetBrains.Annotations.NotNull] Exception exception, string reporter)
        {
            var sb = new StringBuilder();
            var exceptionName = exception.GetType().FullName;
            sb.Append(exceptionName +" occured in ").Append(reporter).Append(Environment.NewLine);
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

        [JetBrains.Annotations.NotNull]
        public static Logger Get() => _logger;

        [JetBrains.Annotations.NotNull]
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

        public static void ImportantInfo([JetBrains.Annotations.NotNull] string message)
        {
            _logger.ImportantInfoMessage(message);
        }

        private void ImportantInfoMessage([JetBrains.Annotations.NotNull] string message)
        {
            ReportString(message, Severity.ImportantInfo);
        }

        public static void Info([JetBrains.Annotations.NotNull] string message, bool preserveLinebreaks = false)
        {
            //if (message.Contains("ok")) {
            //    var stacktrace = Environment.StackTrace;
            //    _logger.InfoMessage(stacktrace, true);
            //}
            _logger.InfoMessage(message, preserveLinebreaks);
        }

        public void InfoMessage([JetBrains.Annotations.NotNull] string message, bool preserveLinebreaks = false)
        {
            ReportString(message, Severity.Information,preserveLinebreaks);
        }

        [JetBrains.Annotations.NotNull] private static readonly object _fileLogLock = new object();
        [CanBeNull] private static string _logFilePath;

        private static Severity _threshold = Severity.Debug;
        [JetBrains.Annotations.NotNull] private static string _logFileIndex = string.Empty;

        private static void LogStringToFile([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] string logfilename, Severity severity, bool preserveNewLines)
        {
            lock (_fileLogLock) {
                FileInfo fi = new FileInfo(logfilename);
                var dir = fi.Directory;
                if (dir!= null && !dir.Exists) {
                    return;
                }

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
            if (LogToConsole)
                Console.WriteLine("------------ Flushing---------------");
            var messages = GetAndClearAllCollectedMessages();
            string filename = GetFilename(Severity.Debug, true);
            StreamWriter sw = new StreamWriter(filename);
            if (LogToConsole)
                Console.WriteLine("------------ Total messages:  " + messages.Count + "---------------");
            sw.WriteLine("Starting the log");
            sw.Close();
            LogStringToFile("Writing " + messages.Count + " cached collected log messaged.", filename, Severity.ImportantInfo, true);
            foreach (LogMessage message in messages)
            {
                LogStringToFile(message.Message, filename, message.Severity, true);
            }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
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
        private void ReportString([JetBrains.Annotations.NotNull] string message, Severity severity, bool preserveLinebreaks = false)
        {
            if (UnitTestDetector.IsRunningInUnitTest && OutputHelper== null && !Config.OutputToConsole) {
                throw new LPGException("no output helper even though we are in unit testing based on attributes");
            }

            if (Config.IsInUnitTesting&&OutputHelper == null && !Config.OutputToConsole) {
                throw new LPGException("no output helper even though we are in unit testing based on config");
            }
            if (severity <= Threshold)
            {
                if (LogToConsole)
                {
                    Console.WriteLine(message);
                }
                else if (OutputHelper != null)
                {
                    OutputHelper.WriteLine(message);
                }
                // don't log the message here if there is no OutputHelper and LogToConsole is false
            }
            if (LogToFile) {
                try {
                    var logfilename = GetFilename(severity, true);
                    LogStringToFile(message, logfilename, severity, preserveLinebreaks);
                    if (severity == Severity.Error) {
                        var errorlogfilename = GetFilename(severity, false);
                        // check if errors are also logged to another file
                        if (errorlogfilename != logfilename)
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

        public void SafeExecute([JetBrains.Annotations.NotNull] Action action)
        {
            if (SaveExecutionFunction != null) {
                SaveExecutionFunction(action);

            }
            else {
                action();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void SafeExecuteWithWait([JetBrains.Annotations.NotNull] Action action)
        {
            if (SaveExecutionFunctionWithWait != null)
            {
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
                sb.AppendLine(i.ToString(CultureInfo.CurrentCulture));
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

        public static void Warning([JetBrains.Annotations.NotNull] string message)
        {
            _logger.WarningMessage(message);
        }

        public void WarningMessage([JetBrains.Annotations.NotNull] string message)
        {
            ReportString(message, Severity.Warning);
        }

        public class LogMessage {
            [UsedImplicitly]
            public LogMessage([JetBrains.Annotations.NotNull] string message, DateTime dt, Severity sv)
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
            [JetBrains.Annotations.NotNull]
            public string Message { get;  }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
            public Severity Severity { get;  }

            [UsedImplicitly]
            public DateTime Time { get;  }

            [JetBrains.Annotations.NotNull]
            public override string ToString() => "[" + Severity + "] " + Message ;
        }

        public void SetOutputHelper(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public ITestOutputHelper OutputHelper { get; set; }

    }
}
