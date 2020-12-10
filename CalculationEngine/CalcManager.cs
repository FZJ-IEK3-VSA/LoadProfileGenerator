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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using ChartCreator2;
using ChartCreator2.OxyCharts;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations; //using Common.SQLResultLogging;

namespace CalculationEngine {
    public sealed class CalcManager : IDisposable {
        private static bool _exitCalcFunction;

        [NotNull] private readonly DayLightStatus _lightNeededArray;

        //[ItemNotNull] [JetBrains.Annotations.NotNull] private readonly List<CalcAffordanceTaggingSet> _affordanceTaggingSets;


        //[JetBrains.Annotations.NotNull] private readonly string _name;


        private readonly int _randomSeed;

        [NotNull] private readonly string _resultPath;

        //[JetBrains.Annotations.NotNull] private readonly SqlResultLoggingService _srls;

        [NotNull] private readonly CalcVariableRepository _variableRepository;

        public CalcManager([NotNull] string resultPath,
                           int randomSeed,
                           [NotNull] DayLightStatus lightNeededArray,
                           [NotNull] CalcVariableRepository variableRepository,
                           CalcRepo calcRepo)
        {
            _lightNeededArray = lightNeededArray;
            //_srls = srls;
            _randomSeed = randomSeed;
            //_fileVersion = fileVersion;
            //_calcHouseholdPlans = calcHouseholdPlans;
            //_affordanceTaggingSets = affordanceTaggingSets;
            //_deviceTaggingSets = deviceTaggingSets;
            _resultPath = resultPath;
            //_name = pName;
            _variableRepository = variableRepository;
            CalcRepo = calcRepo;
        }

        /* [JetBrains.Annotations.NotNull]
         [ItemNotNull]
         public List<CalcAffordanceTaggingSet> AffordanceTaggingSets => _affordanceTaggingSets;*/

        public ICalcAbleObject? CalcObject { get; private set; }

        public CalcRepo CalcRepo { get;  }

        public static bool ContinueRunning { get; private set; } = true;

        public static bool ExitCalcFunction {
            get => _exitCalcFunction;
            set {
                _exitCalcFunction = value;
                if (value) {
                    Logger.Warning("Exit Calc Function has been set to true");
                }
            }
        }

        // ReSharper disable once UnusedParameter.Local
        public void Dispose()
        {
            CalcObject?.Dispose();
            CalcRepo.Dispose();
        }

        public bool Run(Func<bool>? reportCancelFunc)
        {
            if (!ContinueRunning && reportCancelFunc != null) {
                reportCancelFunc();
                return false;
            }

            try {
                CalcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());

                try {
                    CalcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Preperation");
                    // init calculation result
                    //var calculationResult = new CalculationResult(_name, DateTime.Now,                _calcParameters.OfficialStartTime, _calcParameters.OfficialEndTime,                calcObjectType, _srls.ReturnMainSqlPath());

                    //if (_lightNeededArray == null) {
                    //throw new LPGException("Light array was null.");
                    //}

                    if (CalcObject == null) {
                        throw new LPGException("CalcObject was null");
                    }

                    CalcObject.Init(_lightNeededArray,
                        //_householdKey,
                        _randomSeed);

                    //check for transportation weirdness stuff


                    PreProcessingLogging();

                    //CalcObject.WriteInformation();
                }
                finally {
                    CalcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Preperation");
                }

                var now = CalcRepo.CalcParameters.InternalStartTime;
                var timestep = new TimeStep(0, CalcRepo.CalcParameters);
                try {
                    CalcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Core Simulation");
                    if (ExitCalcFunction) {
                        if (CalcObject == null) {
                            throw new LPGException("CalcObject was null");
                        }

                        CalcObject.Dispose();
                        return false;
                    }

                    if (CalcObject == null) {
                        throw new LPGException("CalcObject was null");
                    }

                    while (now < CalcRepo.CalcParameters.InternalEndTime && ContinueRunning) {
                        // ReSharper disable once PossibleNullReferenceException
                        CalcObject.RunOneStep(timestep, now, true);
                        SaveVariableStatesIfNeeded(timestep);
                        CalcRepo.OnlineLoggingData.SaveIfNeeded(timestep);
                        now += CalcRepo.CalcParameters.InternalStepsize;
                        timestep = timestep.AddSteps(1);
                    }

                    Logger.Info("Finished the simulation");
                }
                finally {
                    CalcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Core Simulation");
                }

                // ReSharper disable once PossibleNullReferenceException
                CalcObject.FinishCalculation();
                Logger.Info("Generating the logfiles. This might take a while...");

                // post processing
                CalcRepo.Flush();
                CalcObject.Dispose();
                GC.Collect();
                FileFactoryAndTracker.CheckExistingFilesFromSql(_resultPath);

                if (!ContinueRunning && reportCancelFunc != null) {
                    reportCancelFunc();
                    throw new LPGCancelException("Cancelling");
                }

                try {
                    CalcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Post Post Processing");
                    var ppm = new PostProcessingManager(CalcRepo.CalculationProfiler, CalcRepo.FileFactoryAndTracker);
                    ppm.Run(_resultPath);
                    CalcRepo.Flush();
                }
                finally {
                    CalcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Post Post Processing");
                }

                try {
                    FileFactoryAndTracker.CheckExistingFilesFromSql(_resultPath);
                    CalcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Chart Processing");
                    var cpm = new ChartProcessorManager(CalcRepo.CalculationProfiler, CalcRepo.FileFactoryAndTracker);
                    cpm.Run(_resultPath);
                    CalcRepo.Flush();
                }
                catch (TypeInitializationException ex) {
                    Logger.Warning("Could not generate charts. The error message was: " + ex.Message);
                }
                finally {
                    CalcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Chart Processing");
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    try {
                        CalcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Chart Creation");
                        ChartMaker.MakeChartsAndPDF(CalcRepo.CalculationProfiler, _resultPath);
                    }
                    finally {
                        CalcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Chart Creation");
                    }

                    CalcRepo.Flush();
                }

                /*
                try {
                    /var path = Path.Combine(_resultPath, Constants.ResultFileName);
                    Logfile.FileFactoryAndTracker.RegisterFile(Constants.ResultFileName,
                        "Serialized list of the result files", false, ResultFileID.ResultFileXML,
                        Constants.GeneralHouseholdKey, TargetDirectory.Root);
                    using (var sw = new StreamWriter(path)) {
                        var xs = new XmlSerializer(typeof(CalculationResult));
                        xs.Serialize(sw, calculationResult);
                    }
                }
                catch (Exception e) {
                    var s = "Exception while serializing the results:" + Environment.NewLine;
                    s += e.Message;
                    if (e.InnerException != null) {
                        s += Environment.NewLine + Environment.NewLine + e.InnerException;
                    }
    
                    Logger.Error(s);
                    Logger.Exception(e);
                }*/

                CalcObject.Dispose();


                CalcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Logging");

                if (Config.IsInUnitTesting) {
                    CalcRepo.FileFactoryAndTracker.CheckIfAllAreRegistered(_resultPath);
                    Logger.Info("Finished!");
                }

                if (CalcRepo.CalcParameters.IsSet(CalcOption.LogAllMessages) || CalcRepo.CalcParameters.IsSet(CalcOption.LogErrorMessages)) {
                    InitializeFileLogging(CalcRepo.Srls);
                }
                //_fft.FillCalculationResult(_repository.CalculationResult);
                //_repository.CalculationResult.ResultFileEntries.Sort();
                //_repository.CalculationResult.CalcEndTime = DateTime.Now;

                CalcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Logging");
            }

            finally {
                CalcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
                WriteCalcProfilingResults(_resultPath);
            }
            return true;
        }

        public void SetCalcObject([NotNull] ICalcAbleObject calcObject)
        {
            CalcObject = calcObject;
            if (calcObject == null) {
                throw new LPGException("CalcObject was null");
            }
        }

        /*
        public void SetLightNeededArray(DayLightStatus lightNeededArray)
        {
            _lightNeededArray = lightNeededArray;
        }
*/
        public static void StartRunning()
        {
            ContinueRunning = true;
        }

        public static void StopRunning()
        {
            ContinueRunning = false;
        }

        private void InitializeFileLogging([CanBeNull] SqlResultLoggingService srls)
        {
            if (CalcRepo.CalcParameters.IsSet(CalcOption.LogAllMessages)) {
                var messages = Logger.Get().GetAndClearAllCollectedMessages();
                DateTime start;
                if (messages.Count == 0) {
                    start = DateTime.Now;
                }
                else {
                    start = messages[0].Time;
                }

                var lmes = new List<LogMessageEntry>();
                foreach (var message in messages) {
                    var relativeTime = message.Time - start;
                    var lme = new LogMessageEntry(message.Message, message.Time,
                        message.Severity, message.MyStackTrace?.ToString(), relativeTime);
                    lmes.Add(lme);
                }

                var lml = new LogMessageLogger(srls, LogMessageLogger.ErrorMessageType.All);
                lml.Run(Constants.GeneralHouseholdKey, lmes);
            }

            if (CalcRepo.CalcParameters.IsSet(CalcOption.LogErrorMessages)) {
                var messages = Logger.Get().Errors;
                if (messages.Count == 0) {
                    return;
                }

                var start = messages[0].Time;
                var lmes = new List<LogMessageEntry>();
                foreach (var message in messages) {
                    var relativeTime = message.Time - start;
                    var lme = new LogMessageEntry(message.Message, message.Time,
                        message.Severity, message.MyStackTrace?.ToString(), relativeTime);
                    lmes.Add(lme);
                }

                var lml = new LogMessageLogger(srls, LogMessageLogger.ErrorMessageType.Errors);
                lml.Run(Constants.GeneralHouseholdKey, lmes);
            }
        }

        private void PreProcessingLogging()
        {
            if (CalcRepo.CalcParameters.IsSet(CalcOption.HouseholdContents)) {
                if (CalcObject == null) {
                    throw new LPGException("CalcObject was null");
                }

                CalcObject.DumpHouseholdContentsToText();
            }

            if (CalcRepo.CalcParameters.IsSet(CalcOption.DaylightTimesList)) {
                //WriteDaylightTimesToCSV();
            }

            //WriteDeviceTaggingSets();
        }

        private void SaveVariableStatesIfNeeded([NotNull] TimeStep timestep)
        {
            if (!CalcRepo.CalcParameters.IsSet(CalcOption.VariableLogFile)) {
                return;
            }

            foreach (var variable in _variableRepository.GetAllVariables()) {
                CalcRepo.OnlineLoggingData.AddVariableStatus(new CalcVariableEntry(variable.Name,
                    variable.Guid, variable.Value, variable.LocationName, variable.LocationGuid, variable.HouseholdKey,
                    timestep));
            }
        }

        private void WriteCalcProfilingResults([NotNull] string dstPath)
        {
            if (CalcRepo.CalcParameters.Options.Contains(CalcOption.CalculationFlameChart)) {
                var dstfullFilename = Path.Combine(dstPath, Constants.CalculationProfilerJson);
                using var sw = new StreamWriter(dstfullFilename);
                CalcRepo.CalculationProfiler.WriteJson(sw);
            }
        }
    }
}