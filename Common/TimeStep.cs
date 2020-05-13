using System;
using System.Diagnostics.CodeAnalysis;
using Automation.ResultFiles;
using Common.JSON;
using JetBrains.Annotations;

namespace Common {
    [Serializable]
    public class TimeStep : IComparable<TimeStep>, IEquatable<TimeStep> {
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public TimeStep()
        {
        }

        public TimeStep(int internalstep, [NotNull] CalcParameters parameters)
        {
            InternalStep = internalstep;
            ShowSettling = parameters.ShowSettlingPeriodTime;
            ExternalStep = internalstep - parameters.DummyCalcSteps;
            DummyCalcSteps = parameters.DummyCalcSteps;
        }

        public TimeStep(int internalstep, int dummyCalcSteps, bool showSettling)
        {
            InternalStep = internalstep;
            ShowSettling = showSettling;
            DummyCalcSteps = dummyCalcSteps;
            ExternalStep = internalstep - dummyCalcSteps;
        }

        public bool DisplayThisStep {
            get {
                if (ShowSettling) {
                    return true;
                }

                if (ExternalStep >= 0) {
                    return true;
                }

                return false;
            }
        }

        public int DummyCalcSteps { get; set; }
        public int ExternalStep { get; set; }

        public int InternalStep { get; set; }


        public bool ShowSettling { get; set; }

        public int CompareTo([CanBeNull] TimeStep other)
        {
            if (ReferenceEquals(this, other)) {
                return 0;
            }

            if (other is null) {
                return 1;
            }

            return InternalStep.CompareTo(other.InternalStep);
        }

        public bool Equals(TimeStep other)
        {
            if (other is null) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return InternalStep == other.InternalStep;
        }

        [NotNull]
        public TimeStep AddSteps(int i)
        {
            var ts = new TimeStep(i + InternalStep, DummyCalcSteps, ShowSettling);
            if (ts.DummyCalcSteps != DummyCalcSteps) {
                throw new Exception("Dummy calc steps were not copied correctly");
            }

            return ts;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TimeStep)obj);
        }


        [NotNull]
        public TimeStep GetAbsoluteStep(int i) => new TimeStep(i, DummyCalcSteps, ShowSettling);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() => InternalStep.GetHashCode();

        [NotNull]
        public static TimeStep operator +([CanBeNull] TimeStep k1, [CanBeNull] TimeStep k2)
        {
            if (k1 is null) {
                throw new LPGException("Can't add null");
            }

            if (k2 is null) {
                throw new LPGException("Can't add null");
            }

            return new TimeStep(k1.InternalStep + k2.InternalStep, k1.DummyCalcSteps, k1.ShowSettling);
        }

        public static bool operator ==([CanBeNull] TimeStep k1, [CanBeNull] TimeStep k2)
        {
            if (ReferenceEquals(k1, k2)) {
                return true;
            }

            if (k1 is null) {
                return false;
            }

            if (k2 is null) {
                return false;
            }

            return k1.Equals(k2);
        }

        public static bool operator >([CanBeNull] TimeStep k1, [CanBeNull] TimeStep k2)
        {
            if (ReferenceEquals(k1, k2)) {
                return false;
            }

            if (k1 is null) {
                return false;
            }

            if (k2 is null) {
                return false;
            }

            return k1.InternalStep > k2.InternalStep;
        }

        public static bool operator >=([CanBeNull] TimeStep k1, [CanBeNull] TimeStep k2)
        {
            if (k1 is null) {
                return false;
            }

            if (k2 is null) {
                return false;
            }

            return k1.InternalStep >= k2.InternalStep;
        }

        public static bool operator !=([NotNull] TimeStep k1, [NotNull] TimeStep k2) => !(k1 == k2);

        public static bool operator <([CanBeNull] TimeStep k1, [CanBeNull] TimeStep k2)
        {
            if (ReferenceEquals(k1, k2)) {
                return false;
            }

            if (k1 is null) {
                return false;
            }

            if (k2 is null) {
                return false;
            }

            return k1.InternalStep < k2.InternalStep;
        }

        public static bool operator <=([CanBeNull] TimeStep k1, [CanBeNull] TimeStep k2)
        {
            if (k1 is null) {
                return false;
            }

            if (k2 is null) {
                return false;
            }

            return k1.InternalStep <= k2.InternalStep;
        }

        [NotNull]
        public static TimeStep operator -([CanBeNull] TimeStep k1, [CanBeNull] TimeStep k2)
        {
            if (k1 is null) {
                throw new LPGException("Can't add null");
            }

            if (k2 is null) {
                throw new LPGException("Can't add null");
            }

            return new TimeStep(k1.InternalStep - k2.InternalStep, k1.DummyCalcSteps, k1.ShowSettling);
        }

        [NotNull]
        public override string ToString() => "Internal: " + InternalStep + " External:  " + ExternalStep + (DisplayThisStep ? "(D)" : "()");

        [NotNull]
        public static TimeStep Add(TimeStep left, TimeStep right)
        {
            return left + right;
        }

        [NotNull]
        public static TimeStep Subtract(TimeStep left, TimeStep right)
        {
            return left - right;
        }
    }
}