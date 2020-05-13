using System;

namespace Automation
{
    public class StrGuid : IEquatable<StrGuid>, IComparable<StrGuid> {
        public int CompareTo(StrGuid? other)
        {
            if (ReferenceEquals(this, other)) {
                return 0;
            }

            if (ReferenceEquals(null, other)) {
                return 1;
            }

            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        public bool Equals(StrGuid? other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Value == other.Value;
        }

        public override bool Equals(object? obj)
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

            return Equals((StrGuid)obj);
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(StrGuid? left, StrGuid? right) => Equals(left, right);

        public static bool operator !=(StrGuid? left, StrGuid? right) => !Equals(left, right);

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
        public string Value { get; set; }
        public static StrGuid Empty { get; set; } = new StrGuid("");
        public override string ToString() => Value;
    }
}
