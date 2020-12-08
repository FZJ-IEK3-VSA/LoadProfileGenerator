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
// All advertising materials mentioning features or use of this software must display the following acknowledgement:
// “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
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
using Automation;
using Automation.ResultFiles;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using Common;
using Common.JSON;
using Common.SQLResultLogging;

namespace CalculationEngine.HouseholdElements {
    /*public class HumanHeatGainSpecification
    {
        public CalcLoadType PowerLoadtype { get; set; }
        public CalcLoadType CountLoadtype { get; set; }

    }*/

    public class CalcRepo: IDisposable {
        public CalcVariableRepository? CalcVariableRepository { get; }
        //public HumanHeatGainSpecification HumanHeatGainSpecification { get; }

        [JetBrains.Annotations.NotNull]
        public DateStampCreator DateStampCreator => _dateStampCreator ?? throw new LPGException("no dsc");

        [JetBrains.Annotations.NotNull]
        public static CalcRepo Make([JetBrains.Annotations.NotNull] CalcParameters calcParameters, [JetBrains.Annotations.NotNull] IInputDataLogger idl,
                                    [JetBrains.Annotations.NotNull] string resultPath, [JetBrains.Annotations.NotNull] string calcObjectName,
                                    CalculationProfiler calculationProfiler)
        {

            DateStampCreator dsc = new DateStampCreator(calcParameters);
            OnlineLoggingData old = new OnlineLoggingData(dsc,idl,calcParameters);
            FileFactoryAndTracker fft = new FileFactoryAndTracker(resultPath, calcObjectName, idl);
            LogFile lf = new LogFile(calcParameters, fft);
            OnlineDeviceActivationProcessor odap = new OnlineDeviceActivationProcessor(old,calcParameters,fft);
                Random rnd = new Random(calcParameters.ActualRandomSeed);
                NormalRandom nr = new NormalRandom(0,0.1,rnd);
                SqlResultLoggingService srls = new SqlResultLoggingService(resultPath);
            CalcRepo cr = new CalcRepo(odap,rnd,calcParameters,old,nr,lf,srls,
                idl,calculationProfiler,fft,dsc);
            return cr;
        }
        private readonly FileFactoryAndTracker? _fft;

        public CalcRepo(
                        IOnlineDeviceActivationProcessor? odap = null,
                        Random? rnd = null,
                        CalcParameters? calcParameters= null,
                        IOnlineLoggingData? onlineLoggingData= null,
                        NormalRandom? normalRandom= null,
                        ILogFile? lf = null,
                        SqlResultLoggingService? srls= null,
                        IInputDataLogger? inputDataLogger=null,
                        CalculationProfiler? calculationProfiler=null,
            FileFactoryAndTracker? fft =null,
                        DateStampCreator? dsc = null,
             CalcVariableRepository? calcVariableRepository = null)
        {
            CalcVariableRepository = calcVariableRepository;
            _dateStampCreator = dsc;
            _fft = fft;
            _odap = odap;
            _rnd = rnd;
            _calcParameters = calcParameters;
            _onlineLoggingData = onlineLoggingData;
            _normalRandom = normalRandom;
            _lf = lf;
            _srls = srls;
            _inputDataLogger = inputDataLogger;
            _calculationProfiler = calculationProfiler;
        }
        private readonly ILogFile? _lf;

        private readonly IOnlineDeviceActivationProcessor? _odap;
        private readonly Random? _rnd;
        private readonly CalcParameters? _calcParameters;
        private readonly IOnlineLoggingData? _onlineLoggingData;
        private readonly NormalRandom? _normalRandom;
        private readonly SqlResultLoggingService? _srls;
        private readonly IInputDataLogger? _inputDataLogger;
        private readonly CalculationProfiler? _calculationProfiler;
        private readonly DateStampCreator? _dateStampCreator;

        [JetBrains.Annotations.NotNull]
        public IOnlineDeviceActivationProcessor Odap => _odap ?? throw new LPGException("no odap");

        [JetBrains.Annotations.NotNull]
        public Random Rnd => _rnd ?? throw new LPGException("no rnd");

        [JetBrains.Annotations.NotNull]
        public CalcParameters CalcParameters => _calcParameters ?? throw new LPGException("no calcparameters");

        [JetBrains.Annotations.NotNull]
        public IOnlineLoggingData OnlineLoggingData => _onlineLoggingData ?? throw new LPGException("no old");

        [JetBrains.Annotations.NotNull]
        public NormalRandom NormalRandom => _normalRandom ?? throw new LPGException("no nr");

        [JetBrains.Annotations.NotNull]
        public ILogFile Logfile => _lf ?? throw new LPGException("no lf");

        [JetBrains.Annotations.NotNull]
        public SqlResultLoggingService Srls => _srls ?? throw new LPGException("no srls");

        [JetBrains.Annotations.NotNull]
        public IInputDataLogger InputDataLogger => _inputDataLogger ?? throw new LPGException("no inputdatalogger");

        [JetBrains.Annotations.NotNull]
        public CalculationProfiler CalculationProfiler => _calculationProfiler ?? throw new LPGException("no calc profiler");

        [JetBrains.Annotations.NotNull]
        public FileFactoryAndTracker FileFactoryAndTracker => _fft ?? throw new LPGException("no fft");


        public void Flush()
        {
            _fft?.Dispose();
            _lf?.Dispose();
            _onlineLoggingData?.Dispose();
        }

        public void Dispose()
        {
            Flush();
        }
    }
    public abstract class CalcBase {
        [JetBrains.Annotations.NotNull]
        private readonly string _name;

        public StrGuid Guid { get; }
        protected CalcBase([JetBrains.Annotations.NotNull] string pName,  StrGuid guid)
        {
            _name = pName;
            Guid = guid;
        }

        [JetBrains.Annotations.NotNull]
        public string Name => _name;

        [JetBrains.Annotations.NotNull]
        public override string ToString() => GetType() + " " + _name + " (" + Guid +")";
    }
}