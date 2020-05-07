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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using ChartCreator2.OxyCharts;
using ChartCreator2.PDF;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
//using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalculationEngine {
    public sealed class CalcManager : IDisposable {
        private static bool _continueRunning = true;
        private static bool _exitCalcFunction;

        //[ItemNotNull] [NotNull] private readonly List<CalcAffordanceTaggingSet> _affordanceTaggingSets;


        //[NotNull] private readonly string _name;


        private readonly int _randomSeed;

        [NotNull] private readonly string _resultPath;

        //[NotNull] private readonly SqlResultLoggingService _srls;

        [NotNull] private readonly CalcVariableRepository _variableRepository;
        private readonly CalcRepo _calcRepo;

        [NotNull] private readonly DayLightStatus _lightNeededArray;

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
            _calcRepo = calcRepo;
        }

       /* [NotNull]
        [ItemNotNull]
        public List<CalcAffordanceTaggingSet> AffordanceTaggingSets => _affordanceTaggingSets;*/

        [CanBeNull]
        public ICalcAbleObject CalcObject { get; private set; }

        public static bool ContinueRunning => _continueRunning;

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
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId =
            "<Logfile>k__BackingField")]
        [SuppressMessage("ReSharper", "UseNullPropagation")]
        public void Dispose()
        {
            _calcRepo.Logfile.Dispose();
        }

        public void CloseLogfile()
        {
            _calcRepo.Logfile.Close();
        }

        public static bool MakeCharts { get; } = true;
        public CalcRepo CalcRepo => _calcRepo;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool Run([CanBeNull] Func<bool> reportCancelFunc)
        {
            if (!_continueRunning&& reportCancelFunc != null) {
                reportCancelFunc();
                return false;
            }

            _calcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            _calcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Preperation");
            // init calculation result
            //var calculationResult = new CalculationResult(_name, DateTime.Now,                _calcParameters.OfficialStartTime, _calcParameters.OfficialEndTime,                calcObjectType, _srls.ReturnMainSqlPath());

             if (_lightNeededArray == null) {
                throw new LPGException("Light array was null.");
            }
            if(CalcObject == null) {
                throw new LPGException("CalcObject was null");
            }

            CalcObject.Init(_lightNeededArray,
                //_householdKey,
                _randomSeed);

            //check for transportation weirdness stuff


            PreProcessingLogging();

            var now = _calcRepo.CalcParameters.InternalStartTime;
            var timestep = new TimeStep(0,_calcRepo.CalcParameters);
            //CalcObject.WriteInformation();

            _calcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Preperation");
            _calcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Core Simulation");
            if (ExitCalcFunction) {
                if (CalcObject == null)
                {
                    throw new LPGException("CalcObject was null");
                }
                CalcObject.CloseLogfile();
                return false;
            }
            if (CalcObject == null)
            {
                throw new LPGException("CalcObject was null");
            }
            while (now < _calcRepo.CalcParameters.InternalEndTime && _continueRunning) {
                // ReSharper disable once PossibleNullReferenceException
                CalcObject.RunOneStep(timestep, now, true);
                SaveVariableStatesIfNeeded(timestep);
                _calcRepo.Logfile.SaveIfNeeded(timestep);
                now += _calcRepo.CalcParameters.InternalStepsize;
                timestep = timestep.AddSteps(1);
            }
            Logger.Info("Finished the simulation");
            _calcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Core Simulation");
            // ReSharper disable once PossibleNullReferenceException
            CalcObject.FinishCalculation();
            Logger.Info("Generating the logfiles. This might take a while...");

            // post processing
            _calcRepo.Logfile.Close();
            CalcObject.Dispose();
            GC.Collect();
            if (!_continueRunning && reportCancelFunc!= null) {
                reportCancelFunc();
                throw new LPGException("Canceling");
            }
            PostProcessingManager ppm = new PostProcessingManager(_calcRepo.CalculationProfiler, _calcRepo.Logfile.FileFactoryAndTracker);
            ppm.Run(_resultPath);
            Logger.Info("Finished the logfiles.");
            _calcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Post Post Processing");
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

            CalcObject.CloseLogfile();
            _calcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Post Post Processing");

            _calcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Charting");
            Logger.Info("Starting to make the charts");
            if (MakeCharts &&(_calcRepo.CalcParameters.IsSet(CalcOption.MakePDF) ||
                              _calcRepo.CalcParameters.IsSet(CalcOption.MakeGraphics )))
            {
                MakeChartsAndPDF();
            }
            Logger.Info("Finished making the charts");
            _calcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Charting");

            _calcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Logging");

            if (Config.IsInUnitTesting)
            {
                _calcRepo.Logfile.FileFactoryAndTracker.CheckIfAllAreRegistered(_resultPath);
                Logger.Info("Finished!");
            }
            if (_calcRepo.CalcParameters.IsSet(CalcOption.LogAllMessages) || _calcRepo.CalcParameters.IsSet(CalcOption.LogErrorMessages))
            {
                InitializeFileLogging(_calcRepo.Logfile.Srls);
            }
            //_fft.FillCalculationResult(_repository.CalculationResult);
            //_repository.CalculationResult.ResultFileEntries.Sort();
            //_repository.CalculationResult.CalcEndTime = DateTime.Now;

            _calcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Logging");
            _calcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            WriteCalcProfilingResults(_resultPath);
            //return calculationResult;
            return true;
        }

        private void SaveVariableStatesIfNeeded([NotNull] TimeStep timestep)
        {
            if (!_calcRepo.CalcParameters.IsSet(CalcOption.VariableLogFile)) {
                return;
            }

            foreach (var variable in _variableRepository.GetAllVariables()) {
                _calcRepo.Logfile.OnlineLoggingData.AddVariableStatus(new CalcVariableEntry(variable.Name,
                    variable.Guid,variable.Value,variable.LocationName,variable.LocationGuid,variable.HouseholdKey,timestep));
            }
        }

        private void InitializeFileLogging([CanBeNull] SqlResultLoggingService srls)
        {
            if (_calcRepo.CalcParameters.IsSet(CalcOption.LogAllMessages))
            {
                var messages = Logger.Get().GetAndClearAllCollectedMessages();
                DateTime start;
                if (messages.Count == 0) {
                    start = DateTime.Now;
                }
                else {
                    start = messages[0].Time;
                }

                List< LogMessageEntry> lmes = new List<LogMessageEntry>();
                foreach (Logger.LogMessage message in messages) {
                    TimeSpan relativeTime = message.Time - start;
                    LogMessageEntry lme = new LogMessageEntry(message.Message,message.Time,
                        message.Severity,message.MyStackTrace?.ToString(),relativeTime);
                    lmes.Add(lme);
                }
                LogMessageLogger lml = new LogMessageLogger(srls,LogMessageLogger.ErrorMessageType.All);
                lml.Run(Constants.GeneralHouseholdKey,lmes);
            }

            if (_calcRepo.CalcParameters.IsSet(CalcOption.LogErrorMessages))
            {
                var messages = Logger.Get().Errors;
                if (messages.Count == 0) {
                    return;
                }
                DateTime start = messages[0].Time;
                List<LogMessageEntry> lmes = new List<LogMessageEntry>();
                foreach (Logger.LogMessage message in messages)
                {
                    TimeSpan relativeTime = message.Time - start;
                    LogMessageEntry lme = new LogMessageEntry(message.Message, message.Time,
                        message.Severity, message.MyStackTrace?.ToString(),relativeTime);
                    lmes.Add(lme);
                }

                LogMessageLogger lml = new LogMessageLogger(srls, LogMessageLogger.ErrorMessageType.Errors);
                lml.Run(Constants.GeneralHouseholdKey, lmes);
            }
        }
        private void MakeChartsAndPDF()
        {
            Exception innerException = null;
            var t = new Thread(() => {
                try
                {
                    _calcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Chart Generator RunAll");
                    ChartCreationParameters ccp = new ChartCreationParameters(144,1600, 1000,
                        false, _calcRepo.CalcParameters.CSVCharacter, new DirectoryInfo(_resultPath));
                    var cg = new ChartGeneratorManager(_calcRepo.CalculationProfiler, _calcRepo.Logfile.FileFactoryAndTracker, ccp);
                    cg.Run(_resultPath);
                    _calcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Chart Generator RunAll");
                    if (_calcRepo.CalcParameters.IsSet(CalcOption.MakePDF))
                    {
                        _calcRepo.CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - PDF Creation");
                        Logger.ImportantInfo(
                            "Creating the PDF. This will take a really long time without any progress report...");

                        MigraPDFCreator mpc = new MigraPDFCreator(_calcRepo.CalculationProfiler);
                        mpc.MakeDocument(_resultPath, CalcObject?.Name ??"",  false, false,
                            _calcRepo.CalcParameters.CSVCharacter, _calcRepo.Logfile.FileFactoryAndTracker);
                        _calcRepo.CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - PDF Creation");
                    }
                }
                catch (Exception ex)
                {
                    innerException = ex;
                    Logger.Exception(ex);
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            if (innerException != null)
            {
                Logger.Error("Exception during the PDF creation!");
                Logger.Exception(innerException);
                throw innerException;
            }
        }

        private void WriteCalcProfilingResults([NotNull] string dstPath)
        {
            if (_calcRepo.CalcParameters.Options.Contains(CalcOption.CalculationFlameChart))
            {
                string dstfullFilename = Path.Combine(dstPath, Constants.CalculationProfilerJson);
            using (StreamWriter sw = new StreamWriter(dstfullFilename)) {
                _calcRepo.CalculationProfiler.WriteJson(sw);
            }
                CalculationDurationFlameChart cdfc = new CalculationDurationFlameChart();
                Thread t = new Thread(() => {
                    try {
                        cdfc.Run(_calcRepo.CalculationProfiler, dstPath, "CalcManager");
                    }
                    catch (Exception ex) {
                        Logger.Exception(ex);
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
            }
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
            _continueRunning = true;
        }

        public static void StopRunning()
        {
            _continueRunning = false;
        }

        private void PreProcessingLogging()
        {
            if (_calcRepo.CalcParameters.IsSet(CalcOption.HouseholdContents)) {
                if (CalcObject == null)
                {
                    throw new LPGException("CalcObject was null");
                }
                CalcObject.DumpHouseholdContentsToText();
            }

            if (_calcRepo.CalcParameters.IsSet(CalcOption.DaylightTimesList)) {
                //WriteDaylightTimesToCSV();
            }

            //WriteDeviceTaggingSets();
        }
    }
}