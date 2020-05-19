using System;
using System.Diagnostics.CodeAnalysis;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.OnlineDeviceLogging {
    public class ProfileActivationEntry {
        [NotNull]
        private readonly CalcParameters _calcParameters;

        public ProfileActivationEntry([NotNull] string device, [NotNull] string profile, [NotNull] string profileSource, [NotNull] string loadType, [NotNull] CalcParameters calcParameters) {
            _calcParameters = calcParameters;
            Device = device;
            Profile = profile;
            ProfileSource = profileSource;
            LoadType = loadType;
            ActivationCount = 0;
        }

        public int ActivationCount { get; set; }
        [NotNull]
        public string Device { get; }

        [NotNull]
        public string Line {
            get {
                var c = _calcParameters.CSVCharacter;
                return Device + c + LoadType + c + Profile + " [" + ProfileSource + "]" + c + ActivationCount;
            }
        }

        [NotNull]
        public string LoadType { get; }
        [NotNull]
        public string Profile { get; }
        [NotNull]
        private string ProfileSource { get; }

        public ProfileActivationEntryKey GenerateKey() => new ProfileActivationEntryKey(Device, Profile, ProfileSource,
            LoadType);

        public readonly struct ProfileActivationEntryKey : IEquatable<ProfileActivationEntryKey> {
            public ProfileActivationEntryKey([NotNull] string device, [NotNull] string profile, [NotNull] string profileSource, [NotNull] string loadType)
                : this() {
                Device = device;
                Profile = profile;
                ProfileSource = profileSource;
                LoadType = loadType;
            }
            [NotNull]
            private string Device { get; }
            [NotNull]
            private string Profile { get; }
            [NotNull]
            private string ProfileSource { get; }
            [NotNull]
            private string LoadType { get; }

            [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
            public bool Equals(ProfileActivationEntryKey other)
            {
                return Device == other.Device && Profile == other.Profile && ProfileSource == other.ProfileSource && LoadType == other.LoadType;
            }

            public override bool Equals(object obj) {
                return obj is ProfileActivationEntryKey other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = Device.GetHashCode();
                    hashCode = (hashCode * 397) ^ Profile.GetHashCode();
                    hashCode = (hashCode * 397) ^ ProfileSource.GetHashCode();
                    hashCode = (hashCode * 397) ^ LoadType.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(ProfileActivationEntryKey point1, ProfileActivationEntryKey point2) => point1
                .Equals(point2);

            public static bool operator !=(ProfileActivationEntryKey point1,
                ProfileActivationEntryKey point2) => !point1.Equals(point2);
        }
    }
}