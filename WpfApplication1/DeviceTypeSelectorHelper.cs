using System.Collections.Generic;
using System.Collections.ObjectModel;
using Database.Helpers;
using JetBrains.Annotations;

namespace LoadProfileGenerator
{
    internal static class DeviceTypeSelectorHelper
    {
        [ItemNotNull] [JetBrains.Annotations.NotNull] private static readonly ObservableCollection<string> _categoryOrDevice = new ObservableCollection<string>();
        [ItemNotNull] [JetBrains.Annotations.NotNull] private static readonly ObservableCollection<string> _deviceTypeStrings = new ObservableCollection<string>();

        [JetBrains.Annotations.NotNull] private static readonly Dictionary<string, AssignableDeviceType> _deviceTypeDict =
            new Dictionary<string, AssignableDeviceType>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DeviceTypeSelectorHelper()
        {
            _categoryOrDevice.Add("Device");
            _categoryOrDevice.Add("Device Category");
            _deviceTypeStrings.Add("Device");
            _deviceTypeDict.Add("Device", AssignableDeviceType.Device);
            _deviceTypeStrings.Add("Device Category");
            _deviceTypeDict.Add("Device Category", AssignableDeviceType.DeviceCategory);
            _deviceTypeStrings.Add("Device Action");
            _deviceTypeDict.Add("Device Action", AssignableDeviceType.DeviceAction);
            _deviceTypeStrings.Add("Device Action Group");
            _deviceTypeDict.Add("Device Action Group", AssignableDeviceType.DeviceActionGroup);
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [UsedImplicitly]
        public static ObservableCollection<string> CategoryOrDevice => _categoryOrDevice;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static ObservableCollection<string> DeviceTypeStrings => _deviceTypeStrings;

        [JetBrains.Annotations.NotNull]
        public static Dictionary<string, AssignableDeviceType> DeviceTypeDict => _deviceTypeDict;
    }
}