using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Automation {
    public class JsonReference : IGuidObject, IEquatable<JsonReference>
    {
        [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public bool Equals(JsonReference other)
            => Name == other?.Name && Guid == other?.Guid;

        public static bool operator ==([CanBeNull] JsonReference point1, [CanBeNull] JsonReference point2)
        {
            if (point1 is null && point2 is null)
            {
                return true;
            }

            if (point1 is null)
            {
                return false;
            }
            if (point2 is null)
            {
                return false;
            }

            return point1.Equals(point2);
        }

        public static bool operator !=([CanBeNull] JsonReference point1, [CanBeNull] JsonReference point2)
        {
            if (point1 is null && point2 is null) {
                return false;
            }

            if (point1 is null ) {
                return true;
            }
            if (point2 is null)
            {
                return true;
            }
            return !point1.Equals(point2);
        }

        public override bool Equals(object obj) => obj is JsonReference other && Equals(other);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked {
                return (Name?.GetHashCode()??0 * 397) ^ (Guid?.GetHashCode()??0);
            }
        }

        public JsonReference([NotNull] string name, [NotNull] StrGuid guid)
        {
            Name = name;
            Guid = guid;
        }


        /// <summary>
        /// for json
        /// </summary>
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public JsonReference()
        {
            Guid = StrGuid.Empty;
        }

        [NotNull]
        public string? Name { get; set; }
        [NotNull]
        public StrGuid Guid { get; set; }

        [NotNull]
        public override string ToString() => Name + "(" + Guid + ")";
    }
}