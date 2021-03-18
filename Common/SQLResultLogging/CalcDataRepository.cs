using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.SQLResultLogging {
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class CalcDataRepository
    {
        [JetBrains.Annotations.NotNull] private readonly SqlResultLoggingService _srls;
        //private readonly List<CalcHouseholdPlanDto> _householdPlans;
        [ItemNotNull] [CanBeNull] private List<CalcAffordanceTaggingSetDto> _affordanceTaggingSets;

        [ItemNotNull] [CanBeNull] private List<CalcLoadTypeDto> _loadtypes;

        [CanBeNull] private CalcParameters _calcParameters;

        public CalcDataRepository([JetBrains.Annotations.NotNull] SqlResultLoggingService srls)
        {
            _srls = srls;
            CarpetPlotColumnWidth = 5;
        }

        [CanBeNull]
        private CalcObjectInformation _calcObjectInformation;

        [JetBrains.Annotations.NotNull]
        public CalcObjectInformation CalcObjectInformation
        {
            get
            {
                if (_calcObjectInformation == null)
                {
                    CalcObjectInformationLogger cpl = new CalcObjectInformationLogger(_srls);
                    _calcObjectInformation = cpl.Load();
                }
                return _calcObjectInformation;
            }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<ActionEntry> ReadActionEntries([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            ActionEntryLogger ael = new ActionEntryLogger(_srls);
            return ael.Read(key);
        }

        [JetBrains.Annotations.NotNull] private readonly Dictionary<HouseholdKey, List<DeviceActivationEntry>> _deviceActivationEntries = new Dictionary<HouseholdKey, List<DeviceActivationEntry>>();
       [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<DeviceActivationEntry> LoadDeviceActivations([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            if (_deviceActivationEntries.ContainsKey(key)) {
                return _deviceActivationEntries[key];
            }

            DeviceActivationEntryLogger logger = new DeviceActivationEntryLogger(_srls);
            var entries =  logger.Read(key);
            _deviceActivationEntries.Add(key,entries);
            return entries;
        }
        //[JetBrains.Annotations.NotNull]
        //public AffordanceEnergyUseFile affordanceEnergyUseFile { get; }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcAffordanceDto> LoadAffordances([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            CalcAffordanceDtoLogger cadl = new CalcAffordanceDtoLogger(_srls);
            return cadl.Load(key);
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<TimeShiftableDeviceActivation> LoadFlexibilityEvents([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            FlexibilityDataLogger cadl = new FlexibilityDataLogger(_srls);
            return cadl.Read(key);
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcAffordanceTaggingSetDto> AffordanceTaggingSets
        {
            get
            {
                if (_affordanceTaggingSets == null)
                {
                    CalcAffordanceTaggingSetDtoLogger cpl = new CalcAffordanceTaggingSetDtoLogger(_srls);
                    _affordanceTaggingSets = cpl.Load();
                }
                return _affordanceTaggingSets;
            }
        }
        /*
        [JetBrains.Annotations.NotNull]
        public Dictionary<string, double> allResults { get; }
        */
        //[JetBrains.Annotations.NotNull]public ICalcAbleObjectDto CalcObject { get; }

        //[JetBrains.Annotations.NotNull]
        //public string CalcObjectName { get; }

        [JetBrains.Annotations.NotNull]
        public CalcParameters CalcParameters
        {
            get
            {
                if (_calcParameters == null)
                {
                    CalcParameterLogger cpl = new CalcParameterLogger(_srls);
                    _calcParameters = cpl.Load();
                }
                return _calcParameters;
            }
        }

        public int CarpetPlotColumnWidth { get; }

        //[JetBrains.Annotations.NotNull]
        //public Dictionary<string, double> deviceEnergyDict { get; }
        /*
        [JetBrains.Annotations.NotNull]
        public Dictionary<string, string> DeviceNameToDeviceCategory { get; }*/

        //private List<CalcDeviceDto> _devices;
        [CanBeNull] [ItemNotNull] private List<HouseholdKeyEntry> _householdKeys;

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcDeviceDto> LoadDevices([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            CalcDeviceDtoLogger cpl = new CalcDeviceDtoLogger(_srls);
            return cpl.Load(key);
        }

        //[JetBrains.Annotations.NotNull]
        //public DeviceSumInformationList DeviceSumInformationList { get; }

        //spublic List<AffordanceTaggingSetInformation> taggingSets { get; }
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<DeviceTaggingSetInformation> GetDeviceTaggingSets()
        {
            DeviceTaggingSetLogger dtsl = new DeviceTaggingSetLogger(_srls);
            return dtsl.Load();
        }

        //public List<CalcLocationDto> CalcLocations { get; }
        //public List<CalcAutoDevDto> AutoDevices { get; }
        private readonly Dictionary<HouseholdKey, EnergyFileColumns> _previousEfc =
            new Dictionary<HouseholdKey, EnergyFileColumns>();
        [JetBrains.Annotations.NotNull]
        public EnergyFileColumns ReadEnergyFileColumns([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            if (!_previousEfc.ContainsKey(key)) {
                _previousEfc.Clear();
                EnergyFileColumns efc = new EnergyFileColumns(_srls, key, CalcParameters);
                _previousEfc.Add(key,efc);
            }
            return _previousEfc[key];
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<HouseholdKeyEntry> HouseholdKeys
        {
            get
            {
                if (_householdKeys == null)
                {
                    HouseholdKeyLogger cpl = new HouseholdKeyLogger(_srls);
                    _householdKeys = cpl.Load();
                }

                return _householdKeys;
            }
        }

        //[JetBrains.Annotations.NotNull]
        //[ItemNotNull]
        //public List<CalcHouseholdPlanDto> HouseholdPlans => _householdPlans;
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcLoadTypeDto> LoadTypes
        {
            get
            {
                if (_loadtypes == null)
                {
                    CalcLoadTypeDtoLogger cpl = new CalcLoadTypeDtoLogger(_srls);
                    _loadtypes = cpl.Load();
                }

                return _loadtypes;
            }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcPersonDto> GetPersons([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            CalcPersonDtoLogger cpl = new CalcPersonDtoLogger(_srls);
            var persons = cpl.Load(key);
            return persons;
        }


        //  [JetBrains.Annotations.NotNull]
        //public TotalsInformation TotalInformation { get; set; }

        [JetBrains.Annotations.NotNull]
        public CalcLoadTypeDto GetLoadTypeInformationByGuid(StrGuid guid)
        {
            if (_loadtypes == null)
            {
                CalcLoadTypeDtoLogger cpl = new CalcLoadTypeDtoLogger(_srls);
                _loadtypes = cpl.Load();
            }

            return _loadtypes.Single(x => x.Guid == guid);
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<LocationEntry> LoadLocationEntries([JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            LocationEntryLogger lel = new LocationEntryLogger(_srls);
            return lel.Load(householdKey);
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<CalcSiteDto> LoadSites([JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            CalcSiteDtoLogger lel = new CalcSiteDtoLogger(_srls);
            return lel.Load(householdKey);
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<PersonStatus> LoadPersonStatus([JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            PersonStatusLogger psl = new PersonStatusLogger(_srls);
            return psl.Read(householdKey);
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<TransportationDeviceStateEntry> LoadTransportationDeviceStates([JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            TransportationStateEntryLogger tsel = new TransportationStateEntryLogger(_srls);
            return tsel.Load(householdKey);
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<AffordanceEnergyUseEntry> LoadAffordanceEnergyUses([JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            AffordanceEnergyUseLogger aeul = new AffordanceEnergyUseLogger(_srls);
            return aeul.Load(householdKey);
        }

        [JetBrains.Annotations.NotNull]
        public ResultTableDefinition FindTableByKey(ResultTableID resultTableID, [JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            var tables = _srls.LoadTables(key);
            return tables.Single(x => x.ResultTableID == resultTableID);
        }
        public bool DoesTableExist(ResultTableID resultTableID, [JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            var tables = _srls.LoadTables(key);
            return tables.FirstOrDefault(x => x.ResultTableID == resultTableID)!= null;
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<TransportationEventEntry> LoadTransportationEvents([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            TransportationEventLogger aeul = new TransportationEventLogger(_srls);
            return aeul.Load(key);
        }

        [JetBrains.Annotations.NotNull]
        public List<CalcAutoDevDto> LoadAutoDevices([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            CalcAutoDevDtoLogger cpl = new CalcAutoDevDtoLogger(_srls);
            return cpl.Load(key);
        }

        [JetBrains.Annotations.NotNull]
        public IEnumerable<SingleTimestepActionEntry> ReadSingleTimestepActionEntries([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            SingleTimestepActionEntryLogger stael = new SingleTimestepActionEntryLogger(_srls);
            return stael.Read(key);
        }

        [JetBrains.Annotations.NotNull]
        public List<CalcDeviceArchiveDto> LoadDeviceArchiveEntries([JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            var cpl = new CalcDeviceArchiveDtoLogger(_srls);
            return cpl.Load(key);
        }
    }
}