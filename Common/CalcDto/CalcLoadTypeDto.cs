using System;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
#pragma warning disable S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
    public class CalcLoadTypeDto :IEquatable<CalcLoadTypeDto>, IComparable<CalcLoadTypeDto> {
#pragma warning restore S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
        public override string ToString() => Name + " (" +Guid+")";

        public bool Equals(CalcLoadTypeDto other) => string.Equals(Guid, other?.Guid);

        public override bool Equals(object obj)
        {
            return Equals(this, obj);
        }

        public static bool Equals([CanBeNull]CalcLoadTypeDto obj1, [CanBeNull] CalcLoadTypeDto obj2)
        {
            if (ReferenceEquals(obj2, obj1))
            {
                return true;
            }
            if (obj1 is null)
            {
                return false;
            }
            if (obj2 is null)
            {
                return false;
            }
            if (obj1.GetType() != obj2.GetType())
            {
                return false;
            }

            return obj1.Equals(obj2);
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
        public override int GetHashCode() => Guid.GetHashCode();

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