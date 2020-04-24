using System;
using JetBrains.Annotations;

namespace Automation.ResultFiles {
    public class LoadTypeInformation : IEquatable<LoadTypeInformation> {
        // needed for xml deserialize
        public bool Equals(LoadTypeInformation other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return ConversionFaktor.Equals(other.ConversionFaktor) && FileName == other.FileName && Guid == other.Guid && Name == other.Name && ShowInCharts == other.ShowInCharts && UnitOfPower == other.UnitOfPower && UnitOfSum == other.UnitOfSum;
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

            return Equals((LoadTypeInformation)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = ConversionFaktor.GetHashCode();
                hashCode = (hashCode * 397) ^ FileName.GetHashCode();
                hashCode = (hashCode * 397) ^ Guid.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ ShowInCharts.GetHashCode();
                hashCode = (hashCode * 397) ^ UnitOfPower.GetHashCode();
                hashCode = (hashCode * 397) ^ UnitOfSum.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==([CanBeNull] LoadTypeInformation left, [CanBeNull] LoadTypeInformation right) => Equals(left, right);

        public static bool operator !=([CanBeNull] LoadTypeInformation left, [CanBeNull] LoadTypeInformation right) => !Equals(left, right);

        [UsedImplicitly]
        // ReSharper disable once NotNullMemberIsNotInitialized
        public LoadTypeInformation() {
        }

        public LoadTypeInformation([NotNull] string name, [NotNull] string unitOfSum, [NotNull] string unitOfPower, double conversionFaktor,
            bool showInCharts, [NotNull] string fileName, [NotNull] string guid) {
            Name = name;
            UnitOfSum = unitOfSum;
            UnitOfPower = unitOfPower;
            ConversionFaktor = conversionFaktor;
            ShowInCharts = showInCharts;
            FileName = fileName;
            Guid = guid;
        }

        // needed for xml deserialize
        [UsedImplicitly]
        public double ConversionFaktor { get; set; }

        [UsedImplicitly]
        [NotNull]
        public string FileName { get; set; }

        [NotNull]
        public string Guid { get; }

        // needed for xml deserialize
        [UsedImplicitly]
        [NotNull]
        public string Name { get; set; }

        // needed for xml deserialize
        [UsedImplicitly]
        public bool ShowInCharts { get; set; }

        // needed for xml deserialize
        [UsedImplicitly]
        [NotNull]
        public string UnitOfPower { get; set; }

        // needed for xml deserialize
        [UsedImplicitly]
        [NotNull]
        public string UnitOfSum { get; set; }
    }
}