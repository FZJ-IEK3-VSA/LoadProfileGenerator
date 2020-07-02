using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator
{
    /// <summary>
    ///     Interaction logic for CompleteAffordanceAdd.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class CompleteAffordanceAdd
    {
        [NotNull]
        private readonly ApplicationPresenter _app;

        public CompleteAffordanceAdd([NotNull] ApplicationPresenter app)
        {
            InitializeComponent();
            MainGrid.DataContext = this;
            if (app.Simulator == null) {
                throw new LPGException("Simulator was null");
            }
            _app = app;
            Title = "Complete Affordance Creator";
        }

        [UsedImplicitly]
        [CanBeNull]
        public string AffordanceCategory { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public string AffordanceName { get; set; }

        [UsedImplicitly]
        public double DesireDecay { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public string DesireName { get; set; }

        [UsedImplicitly]
        public double DesireWeight { get; set; }

        [ItemNotNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<DeviceCategory> DeviceCategories => _app.Simulator?.DeviceCategories.Items;

        [UsedImplicitly]
        [CanBeNull]
        public string DeviceName { get; set; }

        [UsedImplicitly]
        public bool ExistingLocation { get; set; }

        [ItemNotNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<VLoadType> LoadTypes => _app.Simulator?.LoadTypes.Items;

        [UsedImplicitly]
        [CanBeNull]
        public string LocationName { get; set; }

        [ItemNotNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<Location> Locations => _app.Simulator?.Locations.Items;

        [UsedImplicitly]
        [CanBeNull]
        public DeviceCategory SelectedDeviceCategory { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public VLoadType SelectedLoadType { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public Location SelectedLocation { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public TimeLimit SelectedTimeLimit { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public TimeBasedProfile SelectedTimeProfile { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public TraitTag SelectedTraitTag { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        [ItemNotNull]
        public ObservableCollection<TimeLimit> TimeLimits => _app.Simulator?.TimeLimits.Items;

        [UsedImplicitly]
        [ItemNotNull]
        [CanBeNull]
        public ObservableCollection<TimeBasedProfile> TimeProfiles => _app.Simulator?.Timeprofiles.Items;

        [UsedImplicitly]
        [NotNull]
        public string TraitClassification { get; set; }

        [UsedImplicitly]
        public int TraitMaximumAge { get; set; }

        [UsedImplicitly]
        public int TraitMinimumAge { get; set; }

        [UsedImplicitly]
        [NotNull]
        public string TraitName { get; set; }

        [UsedImplicitly]
        [ItemNotNull]
        [CanBeNull]
        public ObservableCollection<TraitTag> TraitTags => _app.Simulator?.TraitTags.Items;

        private void CreateClick([CanBeNull]object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (AffordanceName == null)
            {
                Logger.Warning("Affordance Name was null");
                return;
            }
            if (DesireName == null)
            {
                Logger.Warning("Desire Name was null.");
                return;
            }
            if (AffordanceCategory == null)
            {
                Logger.Warning("Affordance Category was null");
                return;
            }
            if (DeviceName == null)
            {
                Logger.Warning("Device Name was null");
                return;
            }
            if (SelectedTraitTag == null)
            {
                Logger.Warning("SelectedTraitTag was null");
                return;
            }
            if (SelectedLoadType == null)
            {
                Logger.Warning("SelectedLoadType was null");
                return;
            }
            if (SelectedDeviceCategory == null)
            {
                Logger.Warning("SelectedDeviceCategory was null");
                return;
            }
            if (SelectedTimeProfile == null)
            {
                Logger.Warning("SelectedTimeProfile was null");
                return;
            }
            if (SelectedLocation == null)
            {
                Logger.Warning("SelectedLocation was null");
                return;
            }
            if (SelectedTimeLimit == null)
            {
                Logger.Warning("SelectedTimeLimit was null");
                return;
            }
            if (LocationName == null)
            {
                Logger.Warning("LocationName was null");
                return;
            }

            Simulator sim = _app.Simulator;
            if (sim == null) {
                Logger.Warning("Simulation was null");
                return;
            }
            CreateItems(sim , AffordanceName, DesireName, DeviceName, TraitName, SelectedLoadType,
                SelectedDeviceCategory, SelectedTimeProfile, DesireWeight, DesireDecay, TraitMinimumAge,
                TraitMaximumAge,
                SelectedLocation, SelectedTraitTag, TraitClassification, SelectedTimeLimit, AffordanceCategory,
                _app, ExistingLocation, LocationName);
        }

        public static void CreateItems([NotNull] Simulator sim, [NotNull] string affordanceName, [NotNull] string desirename, [NotNull] string devicename,
            [NotNull] string traitName, [NotNull] VLoadType loadType, [NotNull] DeviceCategory deviceCategory, [NotNull] TimeBasedProfile timeprofile,
            double desireWeight, double desireDecay, int minimumAge, int maximumAge, [NotNull] Location location,
            [NotNull] TraitTag traitTag, [NotNull] string traitClassification, [NotNull] TimeLimit timelimit, [NotNull] string affordanceCategory,
            [CanBeNull] ApplicationPresenter app, bool useExistingLocation, [NotNull] string locationName)
        {
            Location loc;
            if (useExistingLocation)
            {
                loc = location;
            }
            else
            {
                loc = sim.Locations.CreateNewItem(sim.ConnectionString);
                loc.Name = locationName;
                loc.SaveToDB();
            }
            var aff = sim.Affordances.CreateNewItem(sim.ConnectionString);
            aff.Name = affordanceName;
            aff.PersonProfile = timeprofile;
            aff.MinimumAge = minimumAge;
            aff.MaximumAge = maximumAge;
            aff.TimeLimit = timelimit;
            aff.Red = 255;
            aff.Blue = 0;
            aff.AffCategory = affordanceCategory;
            var device = sim.RealDevices.CreateNewItem(sim.ConnectionString);
            device.Name = devicename;
            device.DeviceCategory = deviceCategory;
            device.AddLoad(loadType, 1, 0, 0);
            device.SaveToDB();

            aff.AddDeviceProfile(device, timeprofile, 0, sim.RealDevices.Items, sim.DeviceCategories.Items, loadType, 1);
            var desire = sim.Desires.CreateNewItem(sim.ConnectionString);
            desire.DefaultDecayRate = (decimal)desireDecay;
            desire.DefaultWeight = (decimal)desireWeight;
            desire.Name = desirename;
            desire.SaveToDB();
            aff.AddDesire(desire, 1, sim.Desires.Items);
            if (loc.LocationDevices.Count > 0)
            {
                aff.NeedsLight = true;
            }
            aff.SaveToDB();
            var trait = sim.HouseholdTraits.CreateNewItem(sim.ConnectionString);
            trait.AddTag(traitTag);
            var webtag = sim.TraitTags.FindFirstByName("web", FindMode.Partial);
            if (webtag != null)
            {
                trait.AddTag(webtag);
            }
            trait.Name = traitName;
            trait.Classification = traitClassification;
            trait.AddDesire(desire, (decimal)desireDecay, "all", 0.5M, (decimal)desireWeight, minimumAge, maximumAge,
                PermittedGender.All);
            trait.AddLocation(loc);
            trait.AddAffordanceToLocation(loc, aff, null, 100, 0, 0, 0, 0);
            trait.CalculateEstimatedTimes();
            trait.SaveToDB();
            if (app != null)
            {
                app.OpenItem(aff);
                app.OpenItem(desire);
                app.OpenItem(device);
                app.OpenItem(trait);
            }
        }
    }
}