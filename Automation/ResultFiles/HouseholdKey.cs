﻿using System;

namespace Automation.ResultFiles {
    [Serializable]
    public record HouseholdKey {
        [JetBrains.Annotations.NotNull]
        public string Key { get; }

        public HouseholdKey([JetBrains.Annotations.NotNull] string key)
        {
            Key = key;
        }
        //public static bool operator ==(HouseholdKey? k1, HouseholdKey? k2)
        //{
        //    if (ReferenceEquals(k1, k2))
        //    {
        //        return true;
        //    }

        //    if (k1 is null)
        //    {
        //        return false;
        //    }
        //    if (k2 is null)
        //    {
        //        return false;
        //    }
        //    return k1.Equals(k2);
        //}

        [JetBrains.Annotations.NotNull]
        public override string ToString() => Key;

        //public static bool operator !=([JetBrains.Annotations.NotNull] HouseholdKey k1, [JetBrains.Annotations.NotNull] HouseholdKey k2)
        //{
        //    return !(k1 == k2);
        //}

        public int CompareTo(HouseholdKey? other)
        {
            if (ReferenceEquals(this, other)) {
                return 0;
            }

            if (other is null) {
                return 1;
            }

            return string.Compare(Key, other.Key, StringComparison.Ordinal);
        }

        //public bool Equals(HouseholdKey? other)
        //{
        //    if (other is null) {
        //        return false;
        //    }

        //    if (ReferenceEquals(this, other)) {
        //        return true;
        //    }

        //    return string.Equals(Key, other.Key, StringComparison.Ordinal);
        //}

        //public override bool Equals(object? obj)
        //{
        //    if (obj is null) {
        //        return false;
        //    }

        //    if (ReferenceEquals(this, obj)) {
        //        return true;
        //    }

        //    if (obj.GetType() != GetType()) {
        //        return false;
        //    }

        //    return Equals((HouseholdKey)obj);
        //}

        //// ReSharper disable once ConditionIsAlwaysTrueOrFalse
        //public override int GetHashCode() => (Key != null ? Key.GetHashCode() : 0);
    }
}