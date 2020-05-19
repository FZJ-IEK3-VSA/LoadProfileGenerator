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
        [NotNull] private readonly SqlResultLoggingService _srls;
        //private readonly List<CalcHouseholdPlanDto> _householdPlans;
        [ItemNotNull] [CanBeNull] private List<CalcAffordanceTaggingSetDto> _affordanceTaggingSets;

        [ItemNotNull] [CanBeNull] private List<CalcLoadTypeDto> _loadtypes;

        [CanBeNull] private CalcParameters _calcParameters;

        public CalcDataRepository([NotNull] SqlResultLoggingService srls)
        {
            _srls = srls;
            CarpetPlotColumnWidth = 5;
        }

        [CanBeNull]
        private CalcObjectInformation _calcObjectInformation;

        [NotNull]
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
        [NotNull]
        public List<ActionEntry> ReadActionEntries([NotNull] HouseholdKey key)
        {
            ActionEntryLogger ael = new ActionEntryLogger(_srls);
            return ael.Read(key);
        }

        [NotNull] private readonly Dictionary<HouseholdKey, List<DeviceActivationEntry>> _deviceActivationEntries = new Dictionary<HouseholdKey, List<DeviceActivationEntry>>();
       [ItemNotNull]
        [NotNull]
        public List<DeviceActivationEntry> LoadDeviceActivations([NotNull] HouseholdKey key)
        {
            if (_deviceActivationEntries.ContainsKey(key)) {
                return _deviceActivationEntries[key];
            }

            DeviceActivationEntryLogger logger = new DeviceActivationEntryLogger(_srls);
            var entries =  logger.Read(key);
            _deviceActivationEntries.Add(key,entries);
            return entries;
        }
        //[NotNull]
        //public AffordanceEnergyUseFile affordanceEnergyUseFile { get; }

        [NotNull]
        [ItemNotNull]
        public List<CalcAffordanceDto> LoadAffordances([NotNull] HouseholdKey key)
        {
            CalcAffordanceDtoLogger cadl = new CalcAffordanceDtoLogger(_srls);
            return cadl.Load(key);
        }

        [NotNull]
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
        [NotNull]
        public Dictionary<string, double> allResults { get; }
        */
        //[NotNull]public ICalcAbleObjectDto CalcObject { get; }

        //[NotNull]
        //public string CalcObjectName { get; }

        [NotNull]
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

        //[NotNull]
        //public Dictionary<string, double> deviceEnergyDict { get; }
        /*
        [NotNull]
        public Dictionary<string, string> DeviceNameToDeviceCategory { get; }*/

        //private List<CalcDeviceDto> _devices;
        [CanBeNull] [ItemNotNull] private List<HouseholdKeyEntry> _householdKeys;

        [NotNull]
        [ItemNotNull]
        public List<CalcDeviceDto> LoadDevices([NotNull] HouseholdKey key)
        {
            CalcDeviceDtoLogger cpl = new CalcDeviceDtoLogger(_srls);
            return cpl.Load(key);
        }

        //[NotNull]
        //public DeviceSumInformationList DeviceSumInformationList { get; }

        //spublic List<AffordanceTaggingSetInformation> taggingSets { get; }
        [NotNull]
        [ItemNotNull]
        public List<DeviceTaggingSetInformation> GetDeviceTaggingSets()
        {
            DeviceTaggingSetLogger dtsl = new DeviceTaggingSetLogger(_srls);
            return dtsl.Load();
        }

        //public List<CalcLocationDto> CalcLocations { get; }
        //public List<CalcAutoDevDto> AutoDevices { get; }
        [NotNull]
        public EnergyFileColumns ReadEnergyFileColumns([NotNull] HouseholdKey key)
        {
            EnergyFileColumns efc = new EnergyFileColumns(_srls, key, CalcParameters);
            return efc;
        }

        [NotNull]
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

        //[NotNull]
        //[ItemNotNull]
        //public List<CalcHouseholdPlanDto> HouseholdPlans => _householdPlans;
        [NotNull]
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

        [NotNull]
        [ItemNotNull]
        public List<CalcPersonDto> GetPersons([NotNull] HouseholdKey key)
        {
            CalcPersonDtoLogger cpl = new CalcPersonDtoLogger(_srls);
            var persons = cpl.Load(key);
            return persons;
        }


        //  [NotNull]
        //public TotalsInformation TotalInformation { get; set; }

        [NotNull]
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
        [NotNull]
        public List<LocationEntry> LoadLocationEntries([NotNull] HouseholdKey householdKey)
        {
            LocationEntryLogger lel = new LocationEntryLogger(_srls);
            return lel.Load(householdKey);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcSiteDto> LoadSites([NotNull] HouseholdKey householdKey)
        {
            CalcSiteDtoLogger lel = new CalcSiteDtoLogger(_srls);
            return lel.Load(householdKey);
        }

        [ItemNotNull]
        [NotNull]
        public List<PersonStatus> LoadPersonStatus([NotNull] HouseholdKey householdKey)
        {
            PersonStatusLogger psl = new PersonStatusLogger(_srls);
            return psl.Read(householdKey);
        }

        [ItemNotNull]
        [NotNull]
        public List<TransportationDeviceStateEntry> LoadTransportationDeviceStates([NotNull] HouseholdKey householdKey)
        {
            TransportationStateEntryLogger tsel = new TransportationStateEntryLogger(_srls);
            return tsel.Load(householdKey);
        }

        [ItemNotNull]
        [NotNull]
        public List<AffordanceEnergyUseEntry> LoadAffordanceEnergyUses([NotNull] HouseholdKey householdKey)
        {
            AffordanceEnergyUseLogger aeul = new AffordanceEnergyUseLogger(_srls);
            return aeul.Load(householdKey);
        }

        [NotNull]
        public ResultTableDefinition FindTableByKey(ResultTableID resultTableID, [NotNull] HouseholdKey key)
        {
            var tables = _srls.LoadTables(key);
            return tables.Single(x => x.ResultTableID == resultTableID);
        }

        [ItemNotNull]
        [NotNull]
        public List<TransportationEventEntry> LoadTransportationEvents([NotNull] HouseholdKey key)
        {
            TransportationEventLogger aeul = new TransportationEventLogger(_srls);
            return aeul.Load(key);
        }

        [NotNull]
        public List<CalcAutoDevDto> LoadAutoDevices([NotNull] HouseholdKey key)
        {
            CalcAutoDevDtoLogger cpl = new CalcAutoDevDtoLogger(_srls);
            return cpl.Load(key);
        }

        [NotNull]
        public List<SingleTimestepActionEntry> ReadSingleTimestepActionEntries([NotNull] HouseholdKey key)
        {
            SingleTimestepActionEntryLogger stael = new SingleTimestepActionEntryLogger(_srls);
            return stael.Read(key);
        }
    }
}