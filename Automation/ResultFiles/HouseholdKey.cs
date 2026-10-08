using System;

namespace Automation.ResultFiles
{
    [Serializable]
    public record HouseholdKey
    {
        public string Key { get; }

        public HouseholdKey(string key)
        {
            Key = key;
        }

        public override string ToString() => Key;

        public int CompareTo(HouseholdKey? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return 1;
            }

            return string.CompareOrdinal(Key, other.Key);
        }
    }
}