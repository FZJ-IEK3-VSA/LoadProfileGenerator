using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Automation;
using Common;
using Database.Database;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STHouseholdDistribution : DBBase, IComparable<STHouseholdDistribution> {
        public const string TableName = "tblSTHouseholdDistributions";
        private readonly EnergyIntensityType _energyIntensity;
        private readonly int _maximumNumber;
        private readonly int _minimumNumber;
        private readonly double _percentOfHouseholds;
        private readonly int _settlementTemplateID;

        public STHouseholdDistribution([CanBeNull]int? pID, [NotNull] string connectionString, int minimumNumber, int maximumNumber,
            double percentOfHouseholds, int settlementTemplateID, [NotNull] string name, EnergyIntensityType energyIntensity, [NotNull] StrGuid guid)
            : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Settlement Template Household Distribution";
            ID = pID;
            _maximumNumber = maximumNumber;
            _minimumNumber = minimumNumber;
            _percentOfHouseholds = percentOfHouseholds;
            _settlementTemplateID = settlementTemplateID;
            _energyIntensity = energyIntensity;
        }

        public EnergyIntensityType EnergyIntensity => _energyIntensity;

        public double HHCountRoundingChange => PreciseHHCount - RoundedHHCount;

        public int MaximumNumber => _maximumNumber;

        public int MinimumNumber => _minimumNumber;

        public double PercentOfHouseholds => _percentOfHouseholds;

        public double PreciseHHCount { get; set; }

        [NotNull]
        [UsedImplicitly]
        public string PrettyPercentOfHouseholds
            => (_percentOfHouseholds * 100).ToString("N2", CultureInfo.CurrentCulture) + "%";

        public int RoundedHHCount { get; set; }

        public int SettlementTemplateID => _settlementTemplateID;

        [NotNull]
        public string TagDescription {
            get {
                var builder = new StringBuilder();
                foreach (var tag in Tags) {
                    if(tag.Tag != null) {
                        builder.Append(tag.Tag.Name).Append(", ");
                    }
                }
                var s = builder.ToString();
                if (s.Length > 2) {
                    s = s.Substring(0, s.Length - 2);
                }
                return s;
            }
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<STHouseholdDistributionTag> Tags { get; } =
            new ObservableCollection<STHouseholdDistributionTag>();

        public int CompareTo([CanBeNull] STHouseholdDistribution other)
        {
            if (other == null) {
                return 0;
            }
            if (_minimumNumber != other.MinimumNumber) {
                return _minimumNumber.CompareTo(other.MinimumNumber);
            }
            if (_maximumNumber != other.MaximumNumber) {
                return _maximumNumber.CompareTo(other.MaximumNumber);
            }
            return string.Compare(TagDescription, other.TagDescription, StringComparison.CurrentCulture);
        }

        public void AddTag([NotNull] HouseholdTag tag)
        {
            var stdt = new STHouseholdDistributionTag(null, tag, IntID, tag.Name,
                ConnectionString, System.Guid.NewGuid().ToStrGuid());
            Tags.Add(stdt);
            stdt.SaveToDB();
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var tag in Tags) {
                tag.SaveToDB();
            }
        }

        [NotNull]
        private static STHouseholdDistribution AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var settlementtemplateID = dr.GetIntFromLong("SettlementTemplateID", false, ignoreMissingFields, -1);
            var maximumNumber = dr.GetIntFromLong("MaximumNumber", false);
            var minimumNumber = dr.GetIntFromLong("MinimumNumber", false);
            var percentSettlements = dr.GetDouble("PercentSettlements", false);
            var eit = (EnergyIntensityType) dr.GetIntFromLong("EnergyIntensityType", false);
            var name = "between " + maximumNumber + " and " + minimumNumber + " " + percentSettlements + "*100%";
            var guid = GetGuid(dr, ignoreMissingFields);
            var shh = new STHouseholdDistribution(id, connectionString, minimumNumber, maximumNumber,
                percentSettlements, settlementtemplateID, name, eit, guid);
            return shh;
        }

        public override void DeleteFromDB()
        {
            foreach (var distributionTag in Tags) {
                distributionTag.DeleteFromDB();
            }
            base.DeleteFromDB();
        }

        private static bool IsCorrectParentHHDist([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var tag = (STHouseholdDistributionTag) child;
            if (parent.ID == tag.STHouseholdDistributionID) {
                var stHouseholdDistribution = (STHouseholdDistribution) parent;
                stHouseholdDistribution.Tags.Add(tag);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<STHouseholdDistribution> result,
            [NotNull] string connectionString, bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<HouseholdTag> tags)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);

            var stHouseholdDistributionTags =
                new ObservableCollection<STHouseholdDistributionTag>();
            STHouseholdDistributionTag.LoadFromDatabase(stHouseholdDistributionTags, connectionString, tags,
                ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(stHouseholdDistributionTags), IsCorrectParentHHDist,
                ignoreMissingTables);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("MaximumNumber", _maximumNumber);
            cmd.AddParameter("MinimumNumber", _minimumNumber);
            cmd.AddParameter("PercentSettlements", _percentOfHouseholds);
            cmd.AddParameter("SettlementTemplateID", _settlementTemplateID);
            cmd.AddParameter("EnergyIntensityType", _energyIntensity);
        }

        public override string ToString() => PercentOfHouseholds + "% should have equal to or more than " +
                                             _minimumNumber +
                                             " households and equal to or less than " + _maximumNumber;
    }
}