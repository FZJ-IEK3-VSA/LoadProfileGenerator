using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.JSON;


namespace CalculationEngine.OnlineDeviceLogging {
    public record OefcKey
    { //: IEquatable<OefcKey>
        public override int GetHashCode()
        {
            unchecked {
                var hashCode = _hashCode;
                hashCode = (hashCode * 397) ^ FullKey.GetHashCode();
                hashCode = (hashCode * 397) ^ (LoadtypeGuid.GetHashCode());
                hashCode = (hashCode * 397) ^ DeviceCategory.GetHashCode();
                hashCode = (hashCode * 397) ^ LocationGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ DeviceInstanceGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ HouseholdKey.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)ThisDeviceType;
                return hashCode;
            }
        }
        private readonly int _hashCode;

        [JetBrains.Annotations.NotNull]
        public string FullKey { get; }
        public string DeviceName { get; }
        public string LocationName { get; }
        public OefcKey([JetBrains.Annotations.NotNull] CalcDeviceDto dto, StrGuid loadtypeGuid)
        {
            HouseholdKey = dto.HouseholdKey;
            ThisDeviceType =dto.DeviceType;
            DeviceInstanceGuid = dto.DeviceInstanceGuid;
            LocationGuid = dto.LocationGuid;
            LoadtypeGuid = loadtypeGuid;
            DeviceCategory = dto.DeviceCategoryName;
            DeviceName = dto.Name;
            LocationName = dto.LocationName;
            unchecked
            {
                _hashCode = LocationGuid.GetHashCode();
                    _hashCode = (_hashCode * 397) ^ loadtypeGuid.GetHashCode();

                _hashCode = (_hashCode * 397) ^ DeviceInstanceGuid.GetHashCode();
                _hashCode = (_hashCode * 397) ^ HouseholdKey.Key.GetHashCode();
                _hashCode = (_hashCode * 397) ^ (int)ThisDeviceType;
            }
            //needed for the makekey due to compiler error
            FullKey = "";
            FullKey = MakeKey();
        }
        /*
        public OefcKey([JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                       OefcDeviceType deviceType,
                       [JetBrains.Annotations.NotNull] string deviceGuid,
                       [JetBrains.Annotations.NotNull] string locationGuid,
                       [CanBeNull] string loadtypeGuid,
                       [JetBrains.Annotations.NotNull] string deviceCategory)
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
        //public override bool Equals(object obj) {
        //    return obj is OefcKey other && Equals(other);
        //}
        [JetBrains.Annotations.NotNull]
        public  string MakeKey()
        {
            return HouseholdKey + "#" +
                   ThisDeviceType + "#" +
                   DeviceInstanceGuid + "#" +
                   LocationGuid + "#" +
                   LoadtypeGuid + "#" + DeviceCategory;
        }
        [JetBrains.Annotations.NotNull]
        public override string ToString()
        {
            return   FullKey;
        }

        //public bool Equals(OefcKey other)
        //{
        //    return FullKey.Equals(other.FullKey);
        //}

        //public static bool operator ==(in OefcKey point1, OefcKey point2) => point1.Equals(point2);

        //public static bool operator !=(in OefcKey point1, OefcKey point2) => !point1.Equals(point2);

        public StrGuid LoadtypeGuid { get; }

        [JetBrains.Annotations.NotNull]
        public string DeviceCategory { get; }

        public StrGuid LocationGuid { get; }

        public StrGuid DeviceInstanceGuid { get; }
        [JetBrains.Annotations.NotNull]
        public HouseholdKey HouseholdKey { get; }
        public OefcDeviceType ThisDeviceType { get; }
    }
}