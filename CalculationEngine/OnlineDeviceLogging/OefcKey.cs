using System;
using Automation.ResultFiles;
using Common.CalcDto;

namespace CalculationEngine.OnlineDeviceLogging {
    using System.Diagnostics.CodeAnalysis;
    using Common.JSON;
    using JetBrains.Annotations;

    public readonly struct OefcKey {
        public override int GetHashCode()
        {
            return _hashCode;
        }
        private readonly int _hashCode;

        [NotNull]
        public string FullKey { get; }
        public OefcKey([NotNull] CalcDeviceDto dto, [CanBeNull] string loadtypeGuid)
        {
            HouseholdKey = dto.HouseholdKey;
            ThisDeviceType =dto.DeviceType;
            DeviceGuid = dto.Guid;
            LocationGuid = dto.LocationGuid;
            LoadtypeGuid = loadtypeGuid;
            DeviceCategory = dto.DeviceCategoryName;
            unchecked
            {
                _hashCode = LocationGuid.GetHashCode();
                if (loadtypeGuid != null)
                {
                    _hashCode = (_hashCode * 397) ^ loadtypeGuid.GetHashCode();
                }

                _hashCode = (_hashCode * 397) ^ DeviceGuid.GetHashCode();
                _hashCode = (_hashCode * 397) ^ HouseholdKey.Key.GetHashCode();
                _hashCode = (_hashCode * 397) ^ (int)ThisDeviceType;
            }
            //needed for the makekey due to compiler error
            FullKey = "";
            FullKey = MakeKey();
        }
        /*
        public OefcKey([NotNull] HouseholdKey householdKey,
                       OefcDeviceType deviceType,
                       [NotNull] string deviceGuid,
                       [NotNull] string locationGuid,
                       [CanBeNull] string loadtypeGuid,
                       [NotNull] string deviceCategory)
        {
            HouseholdKey = householdKey;
            ThisDeviceType = deviceType;
            DeviceGuid = deviceGuid;
            LocationGuid = locationGuid;
            LoadtypeGuid = loadtypeGuid;
            DeviceCategory = deviceCategory;
            unchecked
            {
                _hashCode = LocationGuid.GetHashCode();
                if(loadtypeGuid!= null) {
                    _hashCode = (_hashCode * 397) ^ loadtypeGuid.GetHashCode();
                }

                _hashCode = (_hashCode * 397) ^ DeviceGuid.GetHashCode();
                _hashCode = (_hashCode * 397) ^ HouseholdKey.Key.GetHashCode();
                _hashCode= (_hashCode * 397) ^ (int)ThisDeviceType;
            }
            //needed for the makekey due to compiler error
            FullKey = "";
            FullKey= MakeKey();
        }
        */
        public override bool Equals([CanBeNull] object obj) {
            if (obj is null) {
                return false;
            }
            return obj is OefcKey a && Equals(a);
        }
        [NotNull]
        public  string MakeKey()
        {
            return HouseholdKey + "#" +
                   ThisDeviceType + "#" +
                   DeviceGuid + "#" +
                   LocationGuid + "#" +
                   LoadtypeGuid;
        }
        [NotNull]
        public override string ToString()
        {
            return   FullKey;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool Equals(OefcKey other) => String.Equals(FullKey, other.FullKey);

        public static bool operator ==(in OefcKey point1, OefcKey point2) => point1.Equals(point2);

        public static bool operator !=(in OefcKey point1, OefcKey point2) => !point1.Equals(point2);

        [CanBeNull]
        public string LoadtypeGuid { get; }

        [NotNull]
        public string DeviceCategory { get; }

        [NotNull]
        public string LocationGuid { get; }

        [NotNull]
        public string DeviceGuid { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
        public OefcDeviceType ThisDeviceType { get; }
    }
}