using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls.Converters;

namespace LoadProfileGenerator.Controls.Usercontrols {
    /// <summary>
    ///     Interaktionslogik für DeviceSelectorControl.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public partial class DeviceSelectorControl : INotifyPropertyChanged {
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        [SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
        public delegate void AddedDeviceEventDelegate([CanBeNull] object sender, [CanBeNull] DeviceAddedEventArgs e);

        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        [SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
        public delegate void
            RemovedDeviceEventDelegate([CanBeNull] object sender, [CanBeNull] DeviceRemovedEventArgs e);

        [NotNull] public static readonly DependencyProperty AssignableDeviceProperty =
            DependencyProperty.Register("AssignableDevice", typeof(IAssignableDevice), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty ItemsSourceAProperty = DependencyProperty.Register(
            "ItemsSourceA",
            typeof(IEnumerable), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty ItemsSourceBProperty =
            ItemsControl.ItemsSourceProperty.AddOwner(typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty LoadTypeProperty = DependencyProperty.Register("LoadType",
            typeof(VLoadType), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location",
            typeof(object), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register("Locations",
            typeof(IEnumerable), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty ProbabilityProperty = DependencyProperty.Register(
            "Probability",
            typeof(double), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty SelectedVariableProperty =
            DependencyProperty.Register("SelectedVariable", typeof(Variable), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty SimulatorProperty = DependencyProperty.Register("Simulator",
            typeof(Simulator), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty TimeBasedProfileProperty =
            DependencyProperty.Register("TimeBasedProfile", typeof(TimeBasedProfile), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty TimeDeviationProperty = DependencyProperty.Register(
            "TimeDeviation",
            typeof(decimal), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty TimeLimitProperty = DependencyProperty.Register("TimeLimit",
            typeof(TimeLimit), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty TimeOffsetProperty = DependencyProperty.Register(
            "TimeOffset",
            typeof(decimal), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty UseLoadTypeProperty = DependencyProperty.Register(
            "UseLoadType",
            typeof(bool), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty UseLocationProperty = DependencyProperty.Register(
            "UseLocation",
            typeof(bool), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty UseProbabilityProperty = DependencyProperty.Register(
            "UseProbability", typeof(bool), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty UseTimeDeviationProperty =
            DependencyProperty.Register("UseTimeDeviation", typeof(bool), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty UseTimeLimitProperty = DependencyProperty.Register(
            "UseTimeLimit",
            typeof(bool), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty UseTimeOffsetProperty = DependencyProperty.Register(
            "UseTimeOffset",
            typeof(bool), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty UseTimeProfileProperty = DependencyProperty.Register(
            "UseTimeProfile", typeof(bool), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty UseVariableProperty = DependencyProperty.Register(
            "UseVariable",
            typeof(bool), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty VariableConditionProperty =
            DependencyProperty.Register("VariableCondition", typeof(VariableCondition), typeof(DeviceSelectorControl));

        [NotNull] public static readonly DependencyProperty VariableValueProperty = DependencyProperty.Register(
            "VariableValue",
            typeof(double), typeof(DeviceSelectorControl));

        [NotNull] private readonly Dictionary<string, AssignableDeviceType> _deviceTypeDict;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<string> _deviceTypes;

        [NotNull] private string _selectedAddCategory;

        [CanBeNull] private Simulator _sim;

        public DeviceSelectorControl()
        {
            _deviceTypes = new ObservableCollection<string>();
            _deviceTypeDict = new Dictionary<string, AssignableDeviceType>();
            InitializeComponent();
            LayoutRoot.DataContext = this;
            // device types and the assignables device types need to have the same order!
            _deviceTypes.Add("Device");
            _deviceTypes.Add("Device Category");
            _deviceTypes.Add("Device Action");
            _deviceTypes.Add("Device Action Group");
            for (var i = 0; i < _deviceTypes.Count; i++) {
                _deviceTypeDict.Add(_deviceTypes[i], (AssignableDeviceType) i);
            }

            _selectedAddCategory = _deviceTypes[0];
            VariableConditions.SynchronizeWithList(VariableConditionHelper.CollectAllStrings());
            RefreshColumns();
        }

        [CanBeNull]
        [UsedImplicitly]
        public IAssignableDevice AssignableDevice {
            get => (IAssignableDevice) GetValue(AssignableDeviceProperty);
            set => SetValue(AssignableDeviceProperty, value);
        }

        [UsedImplicitly]
        [ItemNotNull]
        [CanBeNull]
        public ObservableCollection<DeviceActionGroup> DeviceActionGroups => _sim?.DeviceActionGroups.Items;

        [ItemNotNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<DeviceAction> DeviceActions => _sim?.DeviceActions.Items;

        [UsedImplicitly]
        [CanBeNull]
        [ItemNotNull]
        public ObservableCollection<DeviceCategory> DeviceCategories => _sim?.DeviceCategories.Items;

        [ItemNotNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<RealDevice> Devices => _sim?.RealDevices.Items;

        [NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public ObservableCollection<string> DeviceTypes => _deviceTypes;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public IEnumerable ItemsSourceA {
            get => (IEnumerable) GetValue(ItemsSourceAProperty);
            set => SetValue(ItemsSourceAProperty, value);
        }

#pragma warning disable WPF0032 // Use same dependency property in get and set.
        [NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public IEnumerable ItemsSourceB {
            get => (IEnumerable) GetValue(ItemsSourceAProperty);
            set => SetValue(ItemsSourceBProperty, value);
        }
#pragma warning restore WPF0032 // Use same dependency property in get and set.

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType LoadType {
            get {
                if (_deviceTypeDict[_selectedAddCategory] != AssignableDeviceType.DeviceActionGroup &&
                    _deviceTypeDict[_selectedAddCategory] != AssignableDeviceType.DeviceAction) {
                    return (VLoadType) GetValue(LoadTypeProperty);
                }

                SetValue(LoadTypeProperty, null);
                return null;
            }
            set => SetValue(LoadTypeProperty, value);
        }

        [ItemCanBeNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<VLoadType> LoadTypes => _sim?.LoadTypes.Items;

        [CanBeNull]
        [UsedImplicitly]
        public object Location {
            get {
                if (UseLocation) {
                    return GetValue(LocationProperty);
                }

                SetValue(LocationProperty, null);
                return null;
            }
            set => SetValue(LocationProperty, value);
        }

        [NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public IEnumerable Locations {
            get => (IEnumerable) GetValue(LocationsProperty);
            set => SetValue(LocationsProperty, value);
        }

        [NotNull]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public Func<object, bool> OpenItem { get; set; }

        [UsedImplicitly]
        public double Probability {
            get => (double) GetValue(ProbabilityProperty);
            set => SetValue(ProbabilityProperty, value);
        }

        [NotNull]
        [UsedImplicitly]
        public string SelectedAddCategory {
            get => _selectedAddCategory;
            set {
                _selectedAddCategory = value;
                RefreshVisibility();
            }
        }

        public AssignableDeviceType SelectedDeviceType => _deviceTypeDict[SelectedAddCategory];

        [CanBeNull]
        [UsedImplicitly]
        public Variable SelectedVariable {
            get => (Variable) GetValue(SelectedVariableProperty);
            set => SetValue(SelectedVariableProperty, value);
        }

        [UsedImplicitly]
        public Visibility ShowDeviceActionDropDown {
            get {
                if (_deviceTypeDict[_selectedAddCategory] == AssignableDeviceType.DeviceAction) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceActionGroupDropDown {
            get {
                if (_deviceTypeDict[_selectedAddCategory] == AssignableDeviceType.DeviceActionGroup) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceCategoryDropDown {
            get {
                if (_deviceTypeDict[_selectedAddCategory] == AssignableDeviceType.DeviceCategory) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceDropDown {
            get {
                if (_deviceTypeDict[_selectedAddCategory] == AssignableDeviceType.Device) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowLoadTypes {
            get {
                if (_deviceTypeDict[_selectedAddCategory] != AssignableDeviceType.DeviceActionGroup &&
                    _deviceTypeDict[_selectedAddCategory] != AssignableDeviceType.DeviceAction && UseLoadType) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowLocation {
            get {
                if (UseLocation) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowProbability {
            get {
                if (UseProbability) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ShowTime")]
        [UsedImplicitly]
        public Visibility ShowTimeDeviation {
            get {
                if (UseTimeDeviation) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ShowTime")]
        [UsedImplicitly]
        public Visibility ShowTimeLimit {
            get {
                if (UseTimeLimit) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ShowTime")]
        [UsedImplicitly]
        public Visibility ShowTimeOffset {
            get {
                if (UseTimeOffset) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ShowTime")]
        [UsedImplicitly]
        public Visibility ShowTimeProfiles {
            get {
                if (UseTimeProfile && _deviceTypeDict[_selectedAddCategory] != AssignableDeviceType.DeviceActionGroup &&
                    _deviceTypeDict[_selectedAddCategory] != AssignableDeviceType.DeviceAction) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowVariable {
            get {
                if (UseVariable) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        [CanBeNull]
        public Simulator Simulator {
            get => (Simulator) GetValue(SimulatorProperty);
            set {
                SetValue(SimulatorProperty, value);
                _sim = value;
                Refresh();
            }
        }

        [UsedImplicitly]
        [CanBeNull]
        public TimeBasedProfile TimeBasedProfile {
            get {
                if (_deviceTypeDict[_selectedAddCategory] == AssignableDeviceType.DeviceActionGroup ||
                    _deviceTypeDict[_selectedAddCategory] == AssignableDeviceType.DeviceAction) {
                    SetValue(TimeBasedProfileProperty, null);
                    return null;
                }

                return (TimeBasedProfile) GetValue(TimeBasedProfileProperty);
            }
            set => SetValue(TimeBasedProfileProperty, value);
        }

        [UsedImplicitly]
        public decimal TimeDeviation {
            get => (decimal) GetValue(TimeDeviationProperty);
            set => SetValue(TimeDeviationProperty, value);
        }

        [UsedImplicitly]
        [CanBeNull]
        public TimeLimit TimeLimit {
            get => (TimeLimit) GetValue(TimeLimitProperty);
            set => SetValue(TimeLimitProperty, value);
        }

        [ItemNotNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<TimeLimit> TimeLimits => _sim?.TimeLimits.Items;

        [UsedImplicitly]
        public decimal TimeOffset {
            get => (decimal) GetValue(TimeOffsetProperty);
            set => SetValue(TimeOffsetProperty, value);
        }

        [ItemNotNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<TimeBasedProfile> Timeprofiles => _sim?.Timeprofiles.Items;

        [UsedImplicitly]
        public bool UseLoadType {
            get => (bool) GetValue(UseLoadTypeProperty);
            set {
                SetValue(UseLoadTypeProperty, value);
                OnPropertyChanged(nameof(ShowLoadTypes));
                RefreshColumns();
            }
        }

        [UsedImplicitly]
        public bool UseLocation {
            get => (bool) GetValue(UseLocationProperty);
            set {
                SetValue(UseLocationProperty, value);
                OnPropertyChanged(nameof(ShowLocation));
                RefreshColumns();
            }
        }

        [UsedImplicitly]
        public bool UseProbability {
            get => (bool) GetValue(UseProbabilityProperty);
            set {
                SetValue(UseProbabilityProperty, value);
                OnPropertyChanged(nameof(UseProbability));
                RefreshColumns();
            }
        }

        [UsedImplicitly]
        public bool UseTimeDeviation {
            get => (bool) GetValue(UseTimeDeviationProperty);
            set {
                SetValue(UseTimeDeviationProperty, value);
                OnPropertyChanged(nameof(ShowTimeDeviation));
                RefreshColumns();
            }
        }

        [UsedImplicitly]
        public bool UseTimeLimit {
            get => (bool) GetValue(UseTimeLimitProperty);
            set {
                SetValue(UseTimeLimitProperty, value);
                OnPropertyChanged(nameof(UseTimeLimit));
                RefreshColumns();
            }
        }

        [UsedImplicitly]
        public bool UseTimeOffset {
            get => (bool) GetValue(UseTimeOffsetProperty);
            set {
                SetValue(UseTimeOffsetProperty, value);
                OnPropertyChanged(nameof(ShowTimeOffset));
                RefreshColumns();
            }
        }

        [UsedImplicitly]
        public bool UseTimeProfile {
            get => (bool) GetValue(UseTimeProfileProperty);
            set {
                SetValue(UseTimeProfileProperty, value);
                OnPropertyChanged(nameof(ShowTimeProfiles));
                RefreshColumns();
            }
        }

        [UsedImplicitly]
        public bool UseVariable {
            get => (bool) GetValue(UseVariableProperty);
            set {
                SetValue(UseVariableProperty, value);
                OnPropertyChanged(nameof(ShowVariable));
                RefreshColumns();
            }
        }

        [UsedImplicitly]
        public VariableCondition VariableCondition {
            get => (VariableCondition) GetValue(VariableConditionProperty);
            set {
                SetValue(VariableConditionProperty, value);
                OnPropertyChanged(nameof(VariableCondition));
                OnPropertyChanged(nameof(VariableConditionStr));
            }
        }

        [ItemNotNull]
        [UsedImplicitly]
        [NotNull]
        public ObservableCollection<string> VariableConditions { get; } = new ObservableCollection<string>();

        [UsedImplicitly]
        [NotNull]
#pragma warning disable WPF0003 // CLR property for a DependencyProperty should match registered name.
        public string VariableConditionStr
        {
#pragma warning restore WPF0003 // CLR property for a DependencyProperty should match registered name.
            get => VariableConditionHelper.ConvertToVariableDescription(
                (VariableCondition) GetValue(VariableConditionProperty));
            set {
                var tc = VariableConditionHelper.ConvertToVariableCondition(value);
                SetValue(VariableConditionProperty, tc);
                OnPropertyChanged(nameof(VariableCondition));
                OnPropertyChanged(nameof(VariableConditionStr));
            }
        }

        [ItemNotNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<Variable> Variables => _sim?.Variables.Items;

        [UsedImplicitly]
        public double VariableValue {
            get => (double) GetValue(VariableValueProperty);
            set => SetValue(VariableValueProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event AddedDeviceEventDelegate OnAddedDevice {
            add {
                if (AddedDeviceEvent == null) {
                    AddedDeviceEvent += value;
                }
            }
            remove {
                if (AddedDeviceEvent != null) {
                    AddedDeviceEvent -= value;
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event RemovedDeviceEventDelegate OnRemovedDevice {
            add {
                if (RemovedDeviceEvent == null) {
                    RemovedDeviceEvent += value;
                }
            }
            remove {
                if (RemovedDeviceEvent != null) {
                    RemovedDeviceEvent -= value;
                }
            }
        }

        public void ResizeColummns() => SelectedDevices.ResizeColummns(ActualWidth * 0.9);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([NotNull] string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private event AddedDeviceEventDelegate AddedDeviceEvent;

        private void AffordanceDevices_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (SelectedDevices.SelectedItem == null) {
                return;
            }

            if (SelectedDevices.SelectedItem is IDeviceSelection ids)
            {
                OpenItem(ids.Device);
            }
        }

        private void BtnAddDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var evt = new DeviceAddedEventArgs();
            AddedDeviceEvent?.Invoke(this, evt);
        }

        private void BtnClearVariable_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            CmbVariable.SelectedItem = null;

        private void BtnRemoveDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (SelectedDevices.SelectedItem == null) {
                Logger.Warning("No device selected");
                return;
            }
            var evt = new DeviceRemovedEventArgs {
                ItemToRemove = SelectedDevices.SelectedItem
            };

            RemovedDeviceEvent?.Invoke(this, evt);
        }

        private void CheckOneColumn([NotNull] string title, [NotNull] string binding, bool percentConverter = false)
        {
            var gv = (GridView) SelectedDevices.View;
            var found = false;

            foreach (var column in gv.Columns) {
                if (column.Header.ToString() == title) {
                    found = true;
                }
            }

            if (!found) {
                var gvc = new GridViewColumn {
                    Header = title
                };
                var b = new Binding(binding);
                if (percentConverter) {
                    b.Converter = new PercentConverter();
                }

                gvc.DisplayMemberBinding = b;

                gv.Columns.Add(gvc);
            }
        }

        private static void ClearBinding([NotNull] TextBox txt) =>
            BindingOperations.ClearBinding(txt, TextBox.TextProperty);

        private static void ClearBinding([NotNull] ComboBox txt)
            => BindingOperations.ClearBinding(txt, ComboBox.SelectedItemProperty);

        private void CmbDevice_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            var cmb = (ComboBox) sender;
            var dev = cmb.SelectedItem as IAssignableDevice;
            AssignableDevice = dev;
        }

        private void CmbLoadType_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            var lt = CmbVLoadtypes.SelectedItem as VLoadType;
            LoadType = lt;
        }

        private void CmbLocation_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            var lt = CmbLocation.SelectedItem;
            Location = lt;
        }

        private void CmbTimeLimit_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            var dt = CmbTimeLimit.SelectedItem as TimeLimit;
            TimeLimit = dt;
        }

        private void CmbTimeprofiles_OnSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            var tp = CmbTimeprofiles.SelectedItem as TimeBasedProfile;
            TimeBasedProfile = tp;
        }

        private void CmbVariable_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (CmbVariable.SelectedItem == null) {
                SelectedVariable = null;
                return;
            }

            SelectedVariable = (Variable) CmbVariable.SelectedItem;
        }

        private void CmbVariableCondition_OnSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (CmbVariableCondition.SelectedItem != null) {
                VariableConditionStr = (string) CmbVariableCondition.SelectedItem;
            }
        }

        private void DeviceSelectorControl_OnLoaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Action a = ()=> RefreshColumns();
            Dispatcher.BeginInvoke(a);
        }

        private void Refresh()
        {
            OnPropertyChanged(nameof(LoadTypes));
            OnPropertyChanged(nameof(DeviceTypes));
            OnPropertyChanged(nameof(DeviceActionGroups));
            OnPropertyChanged(nameof(Variables));
            OnPropertyChanged(nameof(DeviceActions));
            OnPropertyChanged(nameof(Devices));
            OnPropertyChanged(nameof(DeviceCategories));
            OnPropertyChanged(nameof(Timeprofiles));
            OnPropertyChanged(nameof(TimeLimits));
            OnPropertyChanged(nameof(Probability));

            OnPropertyChanged(nameof(VariableValue));
            OnPropertyChanged(nameof(VariableCondition));
            OnPropertyChanged(nameof(VariableConditionStr));
            OnPropertyChanged(nameof(SelectedVariable));
        }

        private void RefreshColumns()
        {
            if (UseTimeOffset) {
                CheckOneColumn("Time Offset [min]", "TimeOffset");
                SetBinding(TxtTimeOffset, "TimeOffset");
            }
            else {
                ClearBinding(TxtTimeOffset);
            }

            if (UseTimeDeviation) {
                CheckOneColumn("Time Deviation", "TimeStandardDeviation");
                SetBinding(TxtTimeStandardDeviation, "TimeStandardDeviation");
            }
            else {
                ClearBinding(TxtTimeStandardDeviation);
            }

            if (UseTimeProfile) {
                CheckOneColumn("Time Profile", "TimeProfile.Name");
                SetBinding(CmbTimeprofiles, "TimeProfile");
            }
            else {
                ClearBinding(CmbTimeprofiles);
            }

            if (UseLocation) {
                CheckOneColumn("Location", "Location.Name");
                SetBinding(CmbLocation, "Location");
            }
            else {
                ClearBinding(CmbLocation);
            }

            if (UseTimeLimit) {
                CheckOneColumn("Time Limit", "TimeLimit.Name");
                SetBinding(CmbTimeLimit, "TimeLimit");
            }
            else {
                ClearBinding(CmbTimeLimit);
            }

            if (UseProbability) {
                CheckOneColumn("Probability [%]", "Probability", true);
                SetBinding(TxtProbability, "Probability", true);
            }
            else {
                ClearBinding(TxtProbability);
            }

            if (UseLoadType) {
                CheckOneColumn("Load Type", "LoadType");
                SetBinding(CmbVLoadtypes, "LoadType");
            }
            else {
                ClearBinding(CmbVLoadtypes);
            }

            if (UseVariable) {
                CheckOneColumn("Variable", "Variable");
                SetBinding(CmbVariable, "Variable");
                CheckOneColumn("Variable Value", "VariableValue");
                SetBinding(TxtVariableValue, "VariableValue");
                CheckOneColumn("Variable Condition", "VariableCondition");
                SetBinding(CmbVariableCondition, "VariableCondition");
            }
            else {
                ClearBinding(CmbVariable);
                ClearBinding(TxtVariableValue);
                ClearBinding(CmbVariableCondition);
            }
        }

        private void RefreshVisibility()
        {
            OnPropertyChanged(nameof(SelectedAddCategory));
            OnPropertyChanged(nameof(ShowDeviceCategoryDropDown));
            OnPropertyChanged(nameof(ShowDeviceDropDown));
            OnPropertyChanged(nameof(ShowDeviceActionDropDown));
            OnPropertyChanged(nameof(ShowDeviceActionGroupDropDown));
            OnPropertyChanged(nameof(ShowTimeProfiles));
            OnPropertyChanged(nameof(ShowTimeOffset));
            OnPropertyChanged(nameof(ShowLoadTypes));
            OnPropertyChanged(nameof(ShowLocation));
            OnPropertyChanged(nameof(ShowTimeLimit));
            OnPropertyChanged(nameof(ShowTimeDeviation));
            OnPropertyChanged(nameof(ShowProbability));
            OnPropertyChanged(nameof(ShowVariable));
            if (_deviceTypeDict[_selectedAddCategory] == AssignableDeviceType.DeviceActionGroup ||
                _deviceTypeDict[_selectedAddCategory] == AssignableDeviceType.DeviceAction) {
                TimeBasedProfile = null;
            }
        }

        private event RemovedDeviceEventDelegate RemovedDeviceEvent;

        private void SelectedDevices_SelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (SelectedDevices.SelectedItem == null) {
                return;
            }

            if (!(SelectedDevices.SelectedItem is IDeviceSelection ids))
            {
                throw new LPGException(
                    "SelectedDevices.SelectedItem is not an IDeviceSelection?!? Please report as bug.");
            }

            var adt = ids.Device.AssignableDeviceType;
            if (SelectedDevices.SelectedItem is IDeviceSelectionWithVariable iswt) {
                VariableCondition = iswt.VariableCondition;
                CmbVariableCondition.SelectedItem = VariableConditionStr;
            }

            // device types and the assignables device types need to have the same order!
            SelectedAddCategory = _deviceTypes[(int) adt];
            RefreshVisibility();
            Refresh();
        }

        private static void SetBinding([NotNull] TextBox txt, [NotNull] string binding, bool percentConverter = false)
        {
            var myBinding = new Binding {
                ElementName = "SelectedDevices",
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
                Path = new PropertyPath("SelectedItem." + binding),
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
                Mode = BindingMode.OneWay
            };
            if (percentConverter) {
                myBinding.Converter = new PercentConverter();
            }
            else {
                myBinding.Converter = new NumberConverter();
            }

            txt.SetBinding(TextBox.TextProperty, myBinding);
        }

        private static void SetBinding([NotNull] ComboBox txt, [NotNull] string binding)
        {
            var myBinding = new Binding("SelectedItem." + binding) {
                ElementName = "SelectedDevices",
                Mode = BindingMode.OneWay
            };
            txt.SetBinding(ComboBox.SelectedItemProperty, myBinding);
        }

        private void TxtProbability_OnTextChanged([CanBeNull] object sender, [CanBeNull] TextChangedEventArgs e)
        {
            var s = TxtProbability.Text;
            var success = double.TryParse(s, out double d);
            if (!success) {
                Logger.Error("Could not convert " + s);
            }

            Probability = d;
        }

        private void TxtTimeOffset_OnTextChanged([CanBeNull] object sender, [CanBeNull] TextChangedEventArgs e)
        {
            var s = TxtTimeOffset.Text;
            var success = decimal.TryParse(s, out decimal d);
            if (!success) {
                Logger.Error("Could not convert " + s);
            }

            TimeOffset = d;
        }

        private void TxtTimeStandardDeviation_OnTextChanged([CanBeNull] object sender,
            [CanBeNull] TextChangedEventArgs e)
        {
            var s = TxtTimeStandardDeviation.Text;
            var success = decimal.TryParse(s, out decimal d);
            if (!success) {
                Logger.Error("Could not convert " + s);
            }

            TimeDeviation = d;
        }

        private void TxtVariableValue_OnTextChanged([CanBeNull] object sender, [CanBeNull] TextChangedEventArgs e)
        {
            var success = double.TryParse(TxtVariableValue.Text, out double val);
            if (!success) {
                Logger.Error("Could not convert " + TxtVariableValue.Text);
            }

            VariableValue = val;
        }

        public class DeviceAddedEventArgs : EventArgs {
        }

        public class DeviceRemovedEventArgs : EventArgs {
            [NotNull]
            public object ItemToRemove { get; set; }
        }
    }
}