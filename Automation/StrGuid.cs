using System;

namespace Automation
{
    public struct StrGuid : IEquatable<StrGuid>, IComparable<StrGuid> {
        public int CompareTo(StrGuid? other)
        {
            if (!other.HasValue) {
                return 1;
            }

            return string.Compare(StrVal, other.Value.StrVal, StringComparison.Ordinal);
        }

        public bool Equals(StrGuid? other)
        {
            if (!other.HasValue) {
                return false;
            }

            return StrVal == other.Value.StrVal;
        }

        public bool Equals(StrGuid other)
        {
            return StrVal == other.StrVal;
        }

        public int CompareTo(StrGuid other) => throw new NotImplementedException();

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((StrGuid)obj);
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => StrVal.GetHashCode();

        public static bool operator ==(StrGuid? left, StrGuid? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StrGuid? left, StrGuid? right) => !Equals(left, right);
/*
        public StrGuid(string value)
        {
            Value = value;
        }

        public StrGuid(Guid guid)
        {
            Value = guid.ToString();
        }

        [Obsolete("json only")]
        public StrGuid()
        {
            Value = string.Empty;
        }
        */
        public string StrVal { get; set; }
        public static StrGuid Empty { get; set; } = new StrGuid(){ StrVal =  ""};
        public override string ToString() => StrVal;

        public static StrGuid FromString(string guid)
        {
            var strguid = new StrGuid();
            strguid.StrVal = guid;
            return strguid;
        }

        public static StrGuid FromGuid(Guid myguid)
        {
            var strguid = new StrGuid();
            strguid.StrVal = myguid.ToString();
            return strguid;
        }
    }
}
