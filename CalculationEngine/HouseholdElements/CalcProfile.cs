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
//  ‚ÄúThis product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.‚Äù
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

#region step

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.CalcDto;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.HouseholdElements {
    public sealed class CalcProfile : BasicElement, ICalcProfile {
        [ItemNotNull]
        [NotNull] private readonly List<CalcTimeDataPoint> _datapoints = new List<CalcTimeDataPoint>();
        private readonly ProfileType _profileType;
        private readonly TimeSpan _stepSize;
        // ReSharper disable once NotAccessedField.Local
#pragma warning disable IDE0052 // Remove unread private members
        //saving this for information purposes
        private readonly StrGuid _guid;
#pragma warning restore IDE0052 // Remove unread private members

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CalcProfile([NotNull] string name, StrGuid guid, TimeSpan stepSize, ProfileType profileType, [NotNull] string dataSource):base(name)
        {
            _guid = guid;
            _stepSize = stepSize;
            _profileType = profileType;
            TimeFactor = 1;
            DataSource = dataSource;
            StepValues = new List<double>();
        }

        public CalcProfile([NotNull] string name, StrGuid guid, [NotNull] List<double> newValues,
            ProfileType profileType, [NotNull] string dataSource, double timeFactor = 1):base(name)
        {
            _guid = guid;
            TimeFactor = timeFactor;
            StepValues = newValues;
            _profileType = profileType;
            DataSource = dataSource;
        }

        [NotNull]
        public static List<double> MakeListwithValue1AndCustomDuration(int durationInTimesteps)
        {
            var list = new List<double>(durationInTimesteps);
            for (int i = 0; i < durationInTimesteps; i++)
            {
                list.Add(1);
            }
            return list;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcTimeDataPoint> TimeSpanDataPoints => _datapoints;

        public void AddNewTimepoint(TimeSpan ts, double value)
        {
            var tp = new CalcTimeDataPoint(ts, value);
            if (_datapoints == null) {
                throw new LPGException("Datapoints was null");
            }
            _datapoints.Add(tp);
        }

        [NotNull]
        private Dictionary<double, CalcProfile> ChangedProfiles { get; } = new Dictionary<double, CalcProfile>();

        [NotNull]
        public CalcProfile CompressExpandDoubleArray(double timeFactor)
        {
            timeFactor = Math.Round(timeFactor, 2);
            if (ChangedProfiles.ContainsKey(timeFactor)) {
                return ChangedProfiles[timeFactor];
            }

            var newlength = GetNewLengthAfterCompressExpand(StepValues.Count, timeFactor);
            var stepvaluesCompressed = new double[newlength];
            if (timeFactor < 1)
            {
                for (var timeidx = 0; timeidx < StepValues.Count; timeidx++)
                {
                    stepvaluesCompressed[(int)(timeidx * timeFactor)] = StepValues[timeidx];
                }
                if (Math.Abs(stepvaluesCompressed[newlength - 1]) < Constants.Ebsilon)
                {
                    stepvaluesCompressed[newlength - 1] = StepValues[StepValues.Count - 1];
                }
                CalcProfile newcp = new CalcProfile(Name, System.Guid.NewGuid().ToStrGuid(), stepvaluesCompressed.ToList(), ProfileType, DataSource);
                ChangedProfiles.Add(timeFactor,newcp);
                return newcp;
            }
            var lastidx = 0;
            for (var timeidx = 0; timeidx < StepValues.Count; timeidx++)
            {
                var nextidx = (int)((timeidx + 1) * timeFactor);
                for (var i = lastidx; i <= nextidx && i < stepvaluesCompressed.Length; i++)
                {
                    stepvaluesCompressed[i] = StepValues[timeidx];
                }
                lastidx = nextidx;
            }
            CalcProfile cp = new CalcProfile(Name, System.Guid.NewGuid().ToStrGuid(), stepvaluesCompressed.ToList(), ProfileType, DataSource);
            ChangedProfiles.Add(timeFactor, cp);
            return cp;
        }

        public void ConvertToTimesteps()
        {
            ConvertToTimesteps(this);
        }

        private static void ConvertToTimesteps([NotNull] CalcProfile cp)
        {
            if (cp._datapoints == null) {
                throw new LPGException("Datapoints was null");
            }
            if (cp.TimeSpanDataPoints.Count < 2) {
                throw new DataIntegrityException("Less than two datapoints for " + cp.Name);
            }
            var totaltime = cp._datapoints[cp._datapoints.Count - 1].Time;
            if (totaltime.TotalSeconds < cp._stepSize.TotalSeconds) {
                throw new DataIntegrityException("Internal time resolution is " + cp._stepSize.TotalSeconds +
                                                 " seconds while the shortest profile + " + cp.Name +
                                                 " only has a duration of " + totaltime.TotalSeconds +
                                                 " seconds. The time resolution must be at least as long as the shortest profile. Please fix.");
            }
            var stepsAsDouble = totaltime.TotalSeconds / cp._stepSize.TotalSeconds;
            var discreetSteps = (int) Math.Ceiling(stepsAsDouble);

            for (var i = 0; i < discreetSteps; i++) {
                cp.StepValues.Add(0);
            }
            double divfactor = 100;
            // divide by 100 for relative profile to convert to %, but don't change for absolute profiles
            if (cp._profileType == ProfileType.Absolute) {
                divfactor = 1;
            }
            // array mit den sekunden f¸r jeden zeitschritt f¸llen

            var timeRatio = FindShortestTimeSpan(cp).TotalSeconds / cp._stepSize.TotalSeconds;

            if (timeRatio >= 1) // for example shortest = 10 min, stepsize 1 min
            {
                var timeStepArray = new double[discreetSteps];
                timeStepArray[0] = 0;
                for (var idx = 1; idx < discreetSteps; idx++) {
                    timeStepArray[idx] = timeStepArray[idx - 1] + cp._stepSize.TotalSeconds;
                }
                // richtige werte einsetzen

                cp.StepValues[0] = cp._datapoints[0].Value / divfactor;
                var datapointidx = 1;
                for (var idx = 1; idx < discreetSteps; idx++) {
                    if (cp._datapoints[datapointidx].Time.TotalSeconds > timeStepArray[idx]) {
                        cp.StepValues[idx] = cp.StepValues[idx - 1];
                    }
                    else {
                        cp.StepValues[idx] = cp._datapoints[datapointidx].Value / divfactor;
                        datapointidx++;
                    }
                }
            }
            else {
                var timeStepArray = new double[discreetSteps + 1];
                timeStepArray[0] = 0;
                for (var idx = 1; idx < discreetSteps + 1; idx++) {
                    timeStepArray[idx] = timeStepArray[idx - 1] + cp._stepSize.TotalSeconds;
                }

                var dstTimeStep = 1;

                var newList = new List<CalcTimeDataPoint>
                {
                    cp._datapoints[0]
                };
                var srcTimeStep = 1;
                while (srcTimeStep < cp._datapoints.Count)
                    // first build a  list that is guaranteed to contain a value for every dst Timestep
                {
                    while (timeStepArray[dstTimeStep] < cp._datapoints[srcTimeStep].Time.TotalSeconds)
                        // solange wir noch nicht den n‰chsten punkt erreicht haben, weitere st¸tzpunkte einf¸gen
                    {
                        var ts = TimeSpan.FromSeconds(timeStepArray[dstTimeStep]);
                        newList.Add(new CalcTimeDataPoint(ts, cp._datapoints[srcTimeStep - 1].Value));
                        dstTimeStep++;
                    }
                    if (Math.Abs(timeStepArray[dstTimeStep] - cp._datapoints[srcTimeStep].Time.TotalSeconds) <
                        Constants.Ebsilon)
                        // skip identical entries
                    {
                        dstTimeStep++;
                    }
                    newList.Add(cp._datapoints[srcTimeStep]);
                    srcTimeStep++;
                }
                // abschlusspunkt ggf einf¸gen
                if (Math.Abs(timeStepArray[timeStepArray.Length - 1] - newList[newList.Count - 1].Time.TotalSeconds) >
                    Constants.Ebsilon) {
                    // abschlusspunkt benˆtigt
                    var ts = TimeSpan.FromSeconds(timeStepArray[timeStepArray.Length - 1]);
                    var value = newList[newList.Count - 1].Value;
                    newList.Add(new CalcTimeDataPoint(ts, value));
                }

                dstTimeStep = 1;
                srcTimeStep = 1;
                if (Config.IsInUnitTesting) {
                    foreach (var calcTimeDataPoint in newList) {
                        Logger.Info(calcTimeDataPoint.ToString());
                    }
                }

                while (dstTimeStep < timeStepArray.Length) {
                    // collect the entries
                    double totalSeconds = 0;
                    double sum = 0;
                    while (srcTimeStep < newList.Count &&
                           newList[srcTimeStep].Time.TotalSeconds <= timeStepArray[dstTimeStep]) {
                        var seconds = newList[srcTimeStep].Time.TotalSeconds -
                                      newList[srcTimeStep - 1].Time.TotalSeconds;
                        sum += newList[srcTimeStep - 1].Value * seconds;
                        totalSeconds += seconds;
                        srcTimeStep++;
                    }
                    cp.StepValues[dstTimeStep - 1] = sum / totalSeconds;
                    dstTimeStep++;
                }
                if (Config.IsInUnitTesting) {
                    for (var i = 0; i < cp.StepValues.Count; i++) {
                        Logger.Info(timeStepArray[i] + " " + cp.StepValues[i]);
                    }
                }
            }
        }

        private static TimeSpan FindShortestTimeSpan([NotNull] CalcProfile cp)
        {
            var shortest = TimeSpan.MaxValue;
            if (cp._datapoints == null) {
                throw new LPGException("Datapoints was null");
            }
            for (var i = 0; i < cp._datapoints.Count - 1; i++) {
                if (shortest > cp._datapoints[i + 1].Time - cp._datapoints[i].Time) {
                    shortest = cp._datapoints[i + 1].Time - cp._datapoints[i].Time;
                }
            }
            return shortest;
        }

        public int GetNewLengthAfterCompressExpand(double timefactor) => GetNewLengthAfterCompressExpand(
            StepValues.Count, timefactor);

        public static int GetNewLengthAfterCompressExpand(double valuecount, double timefactor)
        {
            var newdoublelength = valuecount * timefactor;
            var newlength = (int) Math.Ceiling(newdoublelength);
            if (newlength == 0) {
                newlength = 1;
            }

            return newlength;
        }

        public override string ToString() => Name + "\t Datapoints:" + TimeSpanDataPoints.Count;

        #region Nested type: CalcTimeDataPoint

        public class CalcTimeDataPoint {
            private readonly TimeSpan _time;

            public CalcTimeDataPoint(TimeSpan ts, double pvalue)
            {
                _time = ts;
                Value = pvalue;
            }

            public TimeSpan Time => _time;

            public double Value { get; }

            [NotNull]
            public override string ToString() => "[" + _time + "] " + Value.ToString(Config.CultureInfo);
        }

        #endregion

        #region ICalcProfile Members

        public List<double> StepValues { get; }
        public double TimeFactor { get; }

        public ProfileType ProfileType => _profileType;

        public string DataSource { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public StrGuid Guid { get; }

        #endregion

        public void AppendProfile([NotNull] ICalcProfile sourcePersonProfile)
        {
            StepValues.AddRange(sourcePersonProfile.StepValues);
        }
    }
}