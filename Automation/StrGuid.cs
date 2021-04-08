using System;
using Newtonsoft.Json;

namespace Automation
{
    public record StrGuid
    {
        public StrGuid()
        {

        }
        [JsonConstructor]
        public StrGuid(string strVal)
        {
            StrVal = strVal;
        }
        // : IEquatable<StrGuid>, IComparable<StrGuid>, IComparable
        //public int CompareTo(StrGuid? other)
        //{
        //    if (!other.HasValue) {
        //        return 1;
        //    }

        //    return string.Compare(StrVal, other.Value.StrVal, StringComparison.Ordinal);
        //}

        //public bool Equals(StrGuid? other)
        //{
        //    if (!other.HasValue) {
        //        return false;
        //    }

        //    return StrVal == other.Value.StrVal;
        //}
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj)) {
                return 1;
            }

            return obj is StrGuid other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(StrGuid)}");
        }

        public static bool operator <(StrGuid left, StrGuid right) => left.CompareTo(right) < 0;

        public static bool operator >(StrGuid left, StrGuid right) => left.CompareTo(right) > 0;

        public static bool operator <=(StrGuid left, StrGuid right) => left.CompareTo(right) <= 0;

        public static bool operator >=(StrGuid left, StrGuid right) => left.CompareTo(right) >= 0;

        //public bool Equals(StrGuid other)
        //{
        //    return StrVal == other.StrVal;
        //}

        public int CompareTo(StrGuid other)
        {
            return string.Compare(StrVal, other.StrVal, StringComparison.Ordinal);
        }

        //public override bool Equals(object? obj)
        //{
        //    if (ReferenceEquals(null, obj)) {
        //        return false;
        //    }

        //    if (obj.GetType() != GetType()) {
        //        return false;
        //    }

        //    return Equals((StrGuid)obj);
        //}

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => StrVal.GetHashCode();

        //public static bool operator ==(StrGuid? left, StrGuid? right)
        //{
        //    return Equals(left, right);
        //}

        //public static bool operator !=(StrGuid? left, StrGuid? right) => !Equals(left, right);
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

        public static StrGuid New()
        {
            var g= new StrGuid();
            g.StrVal = Guid.NewGuid().ToString();
            return g;
        }
    }
}
