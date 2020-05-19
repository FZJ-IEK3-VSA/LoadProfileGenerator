using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using Database.Templating;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class SettlementTemplate : DBBaseElement {
        public const string TableName = "tblSettlementTemplates";

        [NotNull] private readonly SettlementTemplateExecutor _executor = new SettlementTemplateExecutor();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<STHouseholdDistribution> _householdDistributions =
            new ObservableCollection<STHouseholdDistribution>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<STHouseholdTemplate> _householdTemplates =
            new ObservableCollection<STHouseholdTemplate>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<STHouseSize> _houseSizes = new ObservableCollection<STHouseSize>();
        [ItemNotNull] [NotNull] private readonly ObservableCollection<STHouseType> _houseTypes = new ObservableCollection<STHouseType>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<STTraitLimit> _traitLimits = new ObservableCollection<STTraitLimit>();
        [CanBeNull] private string _description;
        private int _desiredHHCount;

        [CanBeNull] private GeographicLocation _geographicLocation;

        [NotNull] private string _newName;

        [CanBeNull] private TemperatureProfile _temperatureProfile;

        public SettlementTemplate([NotNull] string pName, [CanBeNull] int? id, [CanBeNull] string description, [NotNull] string connectionString,
            int desiredHHCount, [NotNull] string newName, [CanBeNull] TemperatureProfile temperatureProfile,
            [CanBeNull] GeographicLocation geographicLocation,StrGuid guid) : base(pName, TableName, connectionString, guid)
        {
            ID = id;
            _description = description;
            TypeDescription = "Settlement Template";
            _desiredHHCount = desiredHHCount;
            _newName = newName;
            _temperatureProfile = temperatureProfile;
            _geographicLocation = geographicLocation;
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [UsedImplicitly]
        public int DesiredHHCount {
            get => _desiredHHCount;
            set {
                SetValueWithNotify(value, ref _desiredHHCount, nameof(DesiredHHCount));
                RefreshPersonCount();
            }
        }

        [CanBeNull]
        public GeographicLocation GeographicLocation {
            get => _geographicLocation;
            set => SetValueWithNotify(value, ref _geographicLocation,false, nameof(GeographicLocation));
        }

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<STHouseholdDistribution> HouseholdDistributions => _householdDistributions;

        [UsedImplicitly]
        public double HouseholdPercentage {
            get {
                double sum = 0;
                foreach (var stHouseholdDistribution in HouseholdDistributions) {
                    sum += stHouseholdDistribution.PercentOfHouseholds;
                }
                return Math.Round(sum, 8);
            }
        }

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<STHouseholdTemplate> HouseholdTemplates => _householdTemplates;

        [UsedImplicitly]
        public double HousePercentage {
            get {
                double sum = 0;
                foreach (var stHouseSize in HouseSizes) {
                    sum += stHouseSize.Percentage;
                }
                return sum;
            }
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<SettlementTemplateExecutor.HouseEntry> HousePreviewEntries => _executor
            .PreviewHouseEntries;

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<STHouseSize> HouseSizes => _houseSizes;

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<STHouseType> HouseTypes => _houseTypes;

        [NotNull]
        [UsedImplicitly]
        public string NewName {
            get => _newName;
            set => SetValueWithNotify(value, ref _newName, nameof(NewName));
        }

        [NotNull]
        [UsedImplicitly]
        public string PersonCount {
            get {
                double minimum = 0;
                double maximum = 0;
                foreach (var stHouseholdDistribution in HouseholdDistributions) {
                    minimum += _desiredHHCount * stHouseholdDistribution.PercentOfHouseholds *
                               stHouseholdDistribution.MinimumNumber;
                    maximum += _desiredHHCount * stHouseholdDistribution.PercentOfHouseholds *
                               stHouseholdDistribution.MaximumNumber;
                }
                if (Math.Abs(minimum - maximum) > Constants.Ebsilon) {
                    return "greater or equal to " + minimum.ToString("N1", CultureInfo.CurrentCulture) +
                           " and smaller or equal to " + maximum.ToString("N1", CultureInfo.CurrentCulture);
                }
                return minimum.ToString("N1", CultureInfo.CurrentCulture) + " Persons";
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string PrettyHouseholdPercentage => (HouseholdPercentage * 100).ToString("N2",
            CultureInfo.CurrentCulture);

        [CanBeNull]
        public TemperatureProfile TemperatureProfile {
            get => _temperatureProfile;
            set => SetValueWithNotify(value, ref _temperatureProfile, nameof(TemperatureProfile));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<STTraitLimit> TraitLimits => _traitLimits;

        [NotNull]
        public STHouseholdDistribution AddHouseholdDistribution(int minimum, int maximum, double percent,
            EnergyIntensityType eit)
        {
            var sthhd = new STHouseholdDistribution(null, ConnectionString, minimum, maximum,
                percent, IntID, "HSD " + minimum + " " + maximum + " " + percent, eit, System.Guid.NewGuid().ToStrGuid());

            _householdDistributions.Add(sthhd);
            _householdDistributions.Sort();
            sthhd.SaveToDB();
            OnPropertyChanged(nameof(PersonCount));
            OnPropertyChanged(nameof(HouseholdPercentage));
            OnPropertyChanged(nameof(PrettyHouseholdPercentage));
            return sthhd;
        }

        public void AddHouseholdTemplate([NotNull] HouseholdTemplate hht)
        {
            foreach (var template in _householdTemplates) {
                if (template.HouseholdTemplate == hht) {
                    return;
                }
            }
            var sthhd = new STHouseholdTemplate(null, ConnectionString, IntID, hht.Name, hht, System.Guid.NewGuid().ToStrGuid());
            _householdTemplates.Add(sthhd);
            _householdTemplates.Sort();
            sthhd.SaveToDB();
        }

        public void AddHouseSize(int minimum, int maximum, double percent)
        {
            var sthhd = new STHouseSize(null, ConnectionString, IntID,
                "HSD " + minimum + " " + maximum + " " + percent, minimum, maximum, percent, System.Guid.NewGuid().ToStrGuid());

            _houseSizes.Add(sthhd);
            sthhd.SaveToDB();
        }

        public void AddHouseType([CanBeNull] HouseType ht)
        {
            foreach (var stHouseType in _houseTypes) {
                if (stHouseType.HouseType == ht) {
                    return;
                }
            }
            if (ht== null)
            {
                throw new LPGException("House type was null");
            }
            var sthhd = new STHouseType(null, ConnectionString, IntID, ht.Name, ht, System.Guid.NewGuid().ToStrGuid());
            _houseTypes.Add(sthhd);
            sthhd.SaveToDB();
        }

        public void AddTraitLimit([NotNull] HouseholdTrait trait, int maximum)
        {
            var stl = new STTraitLimit(null, ConnectionString, IntID, trait.PrettyName, trait, maximum, System.Guid.NewGuid().ToStrGuid());
            stl.SaveToDB();
            _traitLimits.Add(stl);
        }

        [NotNull]
        private static SettlementTemplate AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id =  dr.GetIntFromLong("ID");
            var name =  dr.GetString("Name","(no name");
            var description = dr.GetString("Description");
            var desiredHHCount = dr.GetIntFromLong("DesiredHHCount");
            var newName = dr.GetString("NewName", false, string.Empty, ignoreMissingFields);
            var tempID = dr.GetIntFromLong("TemperatureProfileID", false, ignoreMissingFields);
            var temperatureProfile = aic.TemperatureProfiles.FirstOrDefault(x => x.ID == tempID);
            var geoID = dr.GetIntFromLong("GeographicLocationID", false, ignoreMissingFields);
            var geloc = aic.GeographicLocations.FirstOrDefault(x => x.ID == geoID);
            var guid = GetGuid(dr, ignoreMissingFields);

            return new SettlementTemplate(name, id, description, connectionString, desiredHHCount, newName,
                temperatureProfile, geloc, guid);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
        {
            var newname = FindNewName(isNameTaken, "New Settlement Template");
            return new SettlementTemplate(newname, null, "New Settlement Template", connectionString, 100,
                "Templated from New Settlement Template", null, null, System.Guid.NewGuid().ToStrGuid());
        }

        public void CreateSettlementFromPreview([NotNull] Simulator sim)
        {
            _executor.CreateSettlementFromPreview(sim, this);
        }

        public override void DeleteFromDB()
        {
            while (_householdDistributions.Count > 0) {
                var hhd = _householdDistributions[0];
                DeleteHouseholdDistribution(hhd);
            }
            while (_householdTemplates.Count > 0) {
                var hht = _householdTemplates[0];
                DeleteHouseholdTemplate(hht);
            }
            while (_houseSizes.Count > 0) {
                var housesize = _houseSizes[0];
                DeleteHouseSize(housesize);
            }
            while (_houseTypes.Count > 0) {
                var housesize = _houseTypes[0];
                DeleteHouseType(housesize);
            }
            while (TraitLimits.Count > 0) {
                var housesize = TraitLimits[0];
                DeleteTraitLimit(housesize);
            }
            base.DeleteFromDB();
        }

        public void DeleteHouseholdDistribution([NotNull] STHouseholdDistribution sthd)
        {
            sthd.DeleteFromDB();
            _householdDistributions.Remove(sthd);
            RefreshPersonCount();
        }

        public void DeleteHouseholdTemplate([NotNull] STHouseholdTemplate sthd)
        {
            sthd.DeleteFromDB();
            _householdTemplates.Remove(sthd);
        }

        public void DeleteHouseSize([NotNull] STHouseSize sthd)
        {
            sthd.DeleteFromDB();
            _houseSizes.Remove(sthd);
        }

        public void DeleteHouseType([NotNull] STHouseType sthd)
        {
            sthd.DeleteFromDB();
            _houseTypes.Remove(sthd);
        }

        public void DeleteTraitLimit([NotNull] STTraitLimit stl)
        {
            stl.DeleteFromDB();
            _traitLimits.Remove(stl);
        }

        public void GenerateSettlementPreview([NotNull] Simulator sim)
        {
            _executor.GenerateSettlementPreview( sim, this);
        }

        public void ImportFromExisting([NotNull] SettlementTemplate other)
        {
            _description = other.Description;
            _desiredHHCount = other.DesiredHHCount;
            _newName = other.NewName;
            _temperatureProfile = other.TemperatureProfile;
            _geographicLocation = other.GeographicLocation;

            foreach (var stHouseholdDistribution in other.HouseholdDistributions) {
                var hhd = AddHouseholdDistribution(stHouseholdDistribution.MinimumNumber,
                    stHouseholdDistribution.MaximumNumber, stHouseholdDistribution.PercentOfHouseholds,
                    stHouseholdDistribution.EnergyIntensity);
                foreach (var tags in stHouseholdDistribution.Tags) {
                    if(tags.Tag != null) {
                        hhd.AddTag(tags.Tag);
                    }
                }
            }
            foreach (var stHouseholdTemplate in other.HouseholdTemplates) {
                if(stHouseholdTemplate.HouseholdTemplate!= null) {
                    AddHouseholdTemplate(stHouseholdTemplate.HouseholdTemplate);
                }
            }
            foreach (var stHouseSize in other.HouseSizes) {
                AddHouseSize(stHouseSize.MinimumHouseSize, stHouseSize.MaximumHouseSize, stHouseSize.Percentage);
            }
            foreach (var limit in other.TraitLimits) {
                if(limit.Trait!= null) {
                    AddTraitLimit(limit.Trait, limit.Maximum);
                }
            }
            foreach (var stht in other.HouseTypes) {
                if(stht.HouseType != null) {
                    AddHouseType(stht.HouseType);
                }
            }
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] SettlementTemplate item, [NotNull] Simulator dstsim)
        {
            TemperatureProfile temperatureProfile = null;
            if (item.TemperatureProfile != null) {
                temperatureProfile = GetItemFromListByName(dstsim.TemperatureProfiles.It, item.TemperatureProfile.Name);
            }
            GeographicLocation geoloc = null;
            if (item.GeographicLocation != null) {
                geoloc = GetItemFromListByName(dstsim.GeographicLocations.It, item.GeographicLocation.Name);
            }
            var st = new SettlementTemplate(item.Name, null, item.Description, dstsim.ConnectionString,
                item.DesiredHHCount, item.NewName, temperatureProfile, geoloc, item.Guid);
            st.SaveToDB();
            foreach (var hhdist in item._householdDistributions) {
                var hhd = st.AddHouseholdDistribution(hhdist.MinimumNumber, hhdist.MaximumNumber,
                    hhdist.PercentOfHouseholds, hhdist.EnergyIntensity);
                foreach (var tag in hhdist.Tags) {
                    var hhtag = GetItemFromListByName(dstsim.HouseholdTags.It, tag.Tag?.Name);
                    if (hhtag == null) {
                        Logger.Error("While importing a settlement template, could not find a household tag. Skipping");
                        continue;
                    }
                    hhd.AddTag(hhtag);
                }
            }

            foreach (var housesize in item.HouseSizes) {
                st.AddHouseSize(housesize.MinimumHouseSize, housesize.MaximumHouseSize, housesize.Percentage);
            }

            foreach (var stHouseholdTemplate in item.HouseholdTemplates) {
                var hht =
                    GetItemFromListByName(dstsim.HouseholdTemplates.It, stHouseholdTemplate.HouseholdTemplate?.Name);
                if (hht == null) {
                    Logger.Error(
                        "While importing a settlement template, could not find a household template. Skipping");
                    continue;
                }
                st.AddHouseholdTemplate(hht);
            }

            foreach (var houseTypes in item.HouseTypes) {
                var hht = GetItemFromListByName(dstsim.HouseTypes.It, houseTypes.HouseType?.Name);
                if (hht == null) {
                    Logger.Error("While importing a settlement template, could not find a house type. Skipping");
                    continue;
                }
                st.AddHouseType(hht);
            }
            foreach (var limit in item.TraitLimits) {
                var trait = GetItemFromListByName(dstsim.HouseholdTraits.It, limit.Trait?.Name);
                if (trait == null) {
                    Logger.Error("While importing a settlement template, could not find a trait. Skipping");
                    continue;
                }
                st.AddTraitLimit(trait, limit.Maximum);
            }
            st.SaveToDB();
            return st;
        }

        private static bool IsCorrectParentHHDist([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hhdist = (STHouseholdDistribution) child;
            if (parent.ID == hhdist.SettlementTemplateID) {
                var settlement = (SettlementTemplate) parent;
                settlement._householdDistributions.Add(hhdist);
                return true;
            }
            return false;
        }

        private static bool IsCorrectParentHHTemplate([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hhtemplate = (STHouseholdTemplate) child;
            if (parent.ID == hhtemplate.SettlementTemplateID) {
                var settlement = (SettlementTemplate) parent;
                settlement.HouseholdTemplates.Add(hhtemplate);
                return true;
            }
            return false;
        }

        private static bool IsCorrectParentHouseSize([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var houseSize = (STHouseSize) child;
            if (parent.ID == houseSize.SettlementTemplateID) {
                var settlement = (SettlementTemplate) parent;
                settlement._houseSizes.Add(houseSize);
                return true;
            }
            return false;
        }

        private static bool IsCorrectParentHouseType([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var houseType = (STHouseType) child;
            if (parent.ID == houseType.SettlementTemplateID) {
                var settlement = (SettlementTemplate) parent;
                settlement.HouseTypes.Add(houseType);
                return true;
            }
            return false;
        }

        private static bool IsCorrectParentTraitLimit([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var traitLimit = (STTraitLimit) child;
            if (parent.ID == traitLimit.SettlementTemplateID) {
                var settlement = (SettlementTemplate) parent;
                settlement.TraitLimits.Add(traitLimit);
                return true;
            }
            return false;
        }

        public bool IsHouseGeneratedByThis([NotNull] House house)
        {
            if (house.Source == null) {
                return false;
            }
            var check = "Generated by " + Name;
            check = check.ToUpperInvariant();
            return house.Source.ToUpperInvariant().StartsWith(check, StringComparison.Ordinal);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public bool IsSettlementGeneratedByThis([NotNull] Settlement sett)
        {
            var check = "Generated by " + Name;
            check = check.ToUpperInvariant();
            return sett.Source.ToUpperInvariant().StartsWith(check, StringComparison.Ordinal);
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<SettlementTemplate> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTemplate> householdTemplates, [ItemNotNull] [NotNull] ObservableCollection<HouseType> houseTypes,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<TemperatureProfile> temperaturProfile,
            [ItemNotNull] [NotNull] ObservableCollection<GeographicLocation> geographicLocation, [ItemNotNull] [NotNull] ObservableCollection<HouseholdTag> tags,
            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTrait> traits)
        {
            var aic = new AllItemCollections(temperatureProfiles: temperaturProfile,
                geographicLocations: geographicLocation);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var householdDistributions =
                new ObservableCollection<STHouseholdDistribution>();
            STHouseholdDistribution.LoadFromDatabase(householdDistributions, connectionString, ignoreMissingTables,
                tags);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(householdDistributions), IsCorrectParentHHDist,
                ignoreMissingTables);

            var stHouseholdTemplate =
                new ObservableCollection<STHouseholdTemplate>();
            STHouseholdTemplate.LoadFromDatabase(stHouseholdTemplate, connectionString, ignoreMissingTables,
                householdTemplates);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(stHouseholdTemplate), IsCorrectParentHHTemplate,
                ignoreMissingTables);

            var stHouseTypes = new ObservableCollection<STHouseType>();
            STHouseType.LoadFromDatabase(stHouseTypes, connectionString, ignoreMissingTables, houseTypes);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(stHouseTypes), IsCorrectParentHouseType,
                ignoreMissingTables);

            var stHouseSizes = new ObservableCollection<STHouseSize>();
            STHouseSize.LoadFromDatabase(stHouseSizes, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(stHouseSizes), IsCorrectParentHouseSize,
                ignoreMissingTables);

            var stTraitLimits = new ObservableCollection<STTraitLimit>();
            STTraitLimit.LoadFromDatabase(stTraitLimits, connectionString, ignoreMissingTables, traits);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(stTraitLimits), IsCorrectParentTraitLimit,
                ignoreMissingTables);

            // sort
            foreach (var st in result) {
                st._householdDistributions.Sort();
            }
        }

        private void RefreshPersonCount()
        {
            OnPropertyChanged(nameof(PersonCount));
            OnPropertyChanged(nameof(HouseholdPercentage));
            OnPropertyChanged(nameof(PrettyHouseholdPercentage));
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var hhdist in _householdDistributions) {
                hhdist.SaveToDB();
            }
            foreach (var hhdist in _houseSizes) {
                hhdist.SaveToDB();
            }
            foreach (var hhdist in _houseTypes) {
                hhdist.SaveToDB();
            }
            foreach (var hhdist in _householdTemplates) {
                hhdist.SaveToDB();
            }
            foreach (var stTraitLimit in _traitLimits) {
                stTraitLimit.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            if (_description != null) {
                cmd.AddParameter("Description", _description);
            }

            cmd.AddParameter("DesiredHHCount", _desiredHHCount);
            cmd.AddParameter("NewName", _newName);
            if (_temperatureProfile != null) {
                cmd.AddParameter("TemperatureProfileID", _temperatureProfile.IntID);
            }
            if (_geographicLocation != null) {
                cmd.AddParameter("GeographicLocationID", _geographicLocation.IntID);
            }
        }

        public override string ToString() => Name;

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((SettlementTemplate)toImport, dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}