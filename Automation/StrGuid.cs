using System;
using Newtonsoft.Json;

namespace Automation
{
    public record StrGuid
    {
        [JsonConstructor]
        public StrGuid(string strVal)
        {
            StrVal = strVal;
        }

        public static StrGuid Empty { get; } = new StrGuid("");

        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }

            return obj is StrGuid other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(StrGuid)}");
        }

        public static bool operator <(StrGuid left, StrGuid right) => left.CompareTo(right) < 0;

        public static bool operator >(StrGuid left, StrGuid right) => left.CompareTo(right) > 0;

        public static bool operator <=(StrGuid left, StrGuid right) => left.CompareTo(right) <= 0;

        public static bool operator >=(StrGuid left, StrGuid right) => left.CompareTo(right) >= 0;

        public int CompareTo(StrGuid other)
        {
            return string.CompareOrdinal(StrVal, other.StrVal);
        }

        public override int GetHashCode() => StrVal.GetHashCode();
        public string StrVal { get; }
        public override string ToString() => StrVal;

        public static StrGuid FromString(string guid)
        {
            return new StrGuid(guid);
        }

        public static StrGuid FromGuid(Guid myguid)
        {
            string guidStr = myguid.ToString();
            return new StrGuid(guidStr);
        }

        public static StrGuid New()
        {
            string newGuidStr = Guid.NewGuid().ToString();
            return new StrGuid(newGuidStr);
        }
    }
}
