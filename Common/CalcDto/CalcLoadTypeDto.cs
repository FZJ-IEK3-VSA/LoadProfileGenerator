using System;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
#pragma warning disable S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
    public class CalcLoadTypeDto :IEquatable<CalcLoadTypeDto>, IComparable<CalcLoadTypeDto> {
#pragma warning restore S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
        [NotNull]
        public override string ToString() => Name+ " (" +Guid+")";

        public bool Equals(CalcLoadTypeDto other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return _lti.Equals(other._lti) && Name == other.Name && UnitOfPower == other.UnitOfPower && UnitOfSum == other.UnitOfSum && ConversionFactor.Equals(other.ConversionFactor) && ShowInCharts == other.ShowInCharts && Guid == other.Guid && FileName == other.FileName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((CalcLoadTypeDto)obj);
        }

        

        public static bool operator ==([CanBeNull]CalcLoadTypeDto obj1, [CanBeNull] CalcLoadTypeDto obj2)
        {
            return Equals(obj1, obj2);
        }

        // this is second one '!='
        public static bool operator !=([CanBeNull]CalcLoadTypeDto obj1, [CanBeNull] CalcLoadTypeDto obj2)
        {
            return !Equals(obj1, obj2);
        }
        public override int GetHashCode()
        {
            unchecked {
                var hashCode = _lti.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ UnitOfPower.GetHashCode();
                hashCode = (hashCode * 397) ^ UnitOfSum.GetHashCode();
                hashCode = (hashCode * 397) ^ ConversionFactor.GetHashCode();
                hashCode = (hashCode * 397) ^ ShowInCharts.GetHashCode();
                hashCode = (hashCode * 397) ^ Guid.GetHashCode();
                hashCode = (hashCode * 397) ^ FileName.GetHashCode();
                return hashCode;
            }
        }

        [NotNull]
        public string Name { get; }
        [NotNull]
        public string UnitOfPower { get; }
        [NotNull]
        public string UnitOfSum { get; }
        public double ConversionFactor { get; }
        public bool ShowInCharts { get; }
        [NotNull]
        public string Guid { get; }
        [NotNull]
        public string FileName { get; }
        public CalcLoadTypeDto([NotNull]string name,  [NotNull]string unitOfPower,
                               [NotNull]string unitOfSum, double conversionFactor,
                               bool showInCharts, [NotNull] string guid)
        {
            Name = name;
            UnitOfPower = unitOfPower;
            UnitOfSum = unitOfSum;
            ConversionFactor = conversionFactor;
            ShowInCharts = showInCharts;
            Guid = guid;
            var fileName = AutomationUtili.CleanFileName(name);
            while (fileName.Contains("  "))
            {
                fileName = fileName.Replace("  ", " ");
            }

            FileName = fileName;
            _lti = new LoadTypeInformation(Name, UnitOfSum, UnitOfPower, ConversionFactor, ShowInCharts, fileName, Guid);
        }
        [NotNull]
        private readonly LoadTypeInformation _lti;
        [NotNull]
        public LoadTypeInformation ConvertToLoadTypeInformation()
        {
            return _lti;
        }

        public int CompareTo([CanBeNull] CalcLoadTypeDto other)
        {
            if (ReferenceEquals(this, other)) {
                return 0;
            }

            if (other is null) {
                return 1;
            }

            return string.Compare(Guid, other.Guid, StringComparison.Ordinal);
        }
    }
}