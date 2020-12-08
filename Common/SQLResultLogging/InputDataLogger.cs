using System;
using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.SQLResultLogging {
    public interface IDataSaverBase {
        [NotNull]
        Type SavingType { get; }

        void Run([NotNull] HouseholdKey key, [NotNull] object o);
        [NotNull]
        ResultTableDefinition ResultTableDefinition { get; }
    }

    public abstract class DataSaverBase : IDataSaverBase {
        public ResultTableDefinition ResultTableDefinition { get; }
        [CanBeNull] private readonly SqlResultLoggingService _srls;

        protected DataSaverBase([NotNull] Type savingType, [NotNull] ResultTableDefinition resultTableDefinition,
                                [CanBeNull] SqlResultLoggingService srls)
        {
            SavingType = savingType;
            //check if no readonly properties
            CheckType(savingType);
            ResultTableDefinition = resultTableDefinition;
            _srls = srls;
        }
        private static readonly HashSet<string> _checkedTypes = new HashSet<string>();
        private static void CheckType([NotNull] Type savingType)
        {
            if (_checkedTypes.Contains(savingType.FullName)) {
                return;
            }

            _checkedTypes.Add(savingType.FullName);
            var properties = savingType.GetProperties();
            //Logger.Info("Checking " + savingType.FullName);
            foreach (var property in properties) {
                //Logger.Info(" checking " + property.Name);
                if (property.PropertyType.IsClass) {
                    CheckType(property.PropertyType);
                }
                var accessors = property.GetAccessors();

                //TODO: throw exceptions
                foreach (var accessor in accessors) {
                    if (accessor.ReturnType == typeof(void)) //setter found
                    {
                        //Logger.Info("    found setter");
                        if (!accessor.IsPublic) {
                            throw new LPGException("Private setter on a json serializer class");
                        }
                    }
                }
            }
        }

        public abstract void Run(HouseholdKey key, object o);

        public Type SavingType { get; }

        [CanBeNull]
        protected SqlResultLoggingService Srls => _srls;

        [NotNull]
        protected SaveableEntry GetStandardSaveableEntry([NotNull] HouseholdKey key)
        {
            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("Name", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            return se;
        }
    }

    public interface IInputDataLogger {
        void Save([NotNull] object o);
        void Save([NotNull] HouseholdKey key, [NotNull] object o);
        void SaveList<T>([ItemNotNull] [NotNull] List<IHouseholdKey> objectsWithKey);
        void AddSaver([NotNull] IDataSaverBase saver);
    }

    public class InputDataLogger : IInputDataLogger {
        //List<IDataSaverBase> datasavers = new List<IDataSaverBase>();
        public InputDataLogger([ItemNotNull] [NotNull] IDataSaverBase[] dataSavers)
        {
            //check the savers if all table ids are unique to prevent stupid mistakes
            HashSet<ResultTableID> tableIDs = new HashSet<ResultTableID>();
            foreach (var saver in dataSavers)
            {
                if (!tableIDs.Add(saver.ResultTableDefinition.ResultTableID))
                {
                    throw new LPGException("Duplicate table id added:" + saver.ResultTableDefinition.ResultTableID);
                }
            }
            //initialize lookup dictionary for later
            foreach (IDataSaverBase dataSaver in dataSavers) {
                FunctionDB.Add(dataSaver.SavingType, dataSaver);
            }
        }

        public void AddSaver(IDataSaverBase saver)
        {
            if(FunctionDB.ContainsKey(saver.SavingType)) {
                throw new LPGException("Saver was already registered for type " + saver.SavingType.FullName);
            }

            FunctionDB.Add(saver.SavingType,saver);
        }

        [NotNull]
        private Dictionary<Type, IDataSaverBase> FunctionDB { get; } = new Dictionary<Type, IDataSaverBase>();

        public void Save(object o)
        {
            Save(Constants.GeneralHouseholdKey,o);
        }
        public void Save(HouseholdKey key, object o)
        {
            Type t = o.GetType();
            if (!FunctionDB.ContainsKey(t)) {
                throw new LPGException("Forgotten Logger for " + t.FullName);
            }

            IDataSaverBase dsb = FunctionDB[t];
            dsb.Run(key, o);
        }

        public void SaveList<T>(List<IHouseholdKey> objectsWithKey)
        {
            if (objectsWithKey.Count == 0) {
                Logger.Error("While trying to save some results, not a single object was contained in the list of the type " + typeof(T).FullName);
                return;
            }
            Type t = objectsWithKey[0].GetType();
            if (typeof(T) != t) {
                throw new LPGException("trying to save with the wrong parameter type. " + typeof(T).FullName + " vs. " + t.FullName);
            }
            if (!FunctionDB.ContainsKey(t)) {
                throw new LPGException("Forgotten Logger for " + t.FullName);
            }

            var keys = objectsWithKey.Select(x => x.HouseholdKey).Distinct().ToList();
            foreach (HouseholdKey householdKey in keys) {
                var filtered = objectsWithKey.Where(x => x.HouseholdKey == householdKey).ToList();
                IDataSaverBase dsb = FunctionDB[t];
                dsb.Run(householdKey, filtered);
            }
        }

        /*
        private class CalculationConfiguration : ITypeDescriber
        {
            public CalculationConfiguration([JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] string value)
            {
                Name = name;
                Value = value;
                HouseholdKey = Constants.GeneralHouseholdKey;
            }

            [JetBrains.Annotations.NotNull]
            public string Name { [UsedImplicitly] get; set; }
            [JetBrains.Annotations.NotNull]
            public string Value { [UsedImplicitly] get; set; }
            public string GetTypeDescription()
            {
                return "Calculation Configuration";
            }

            public int ID { get; set; }
            public HouseholdKey HouseholdKey { get; set; }
        }*/
        /*
        [JetBrains.Annotations.NotNull]
        private readonly SqlResultLoggingService _srfl;
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly List<ISingleInputDataLogger> _dataLoggers;*/
        /*
        public InputDataLogger([JetBrains.Annotations.NotNull] SqlResultLoggingService srfl,  //FileFactoryAndTracker fft,
                               [JetBrains.Annotations.NotNull][ItemNotNull] ISingleInputDataLogger[] dataLoggers)
        {
            _srfl = srfl;
            //_fft = fft;
            _dataLoggers = dataLoggers.ToList();
        }*/

        /*
                public void Save([JetBrains.Annotations.NotNull] CalcStartParameterSet csps, [JetBrains.Annotations.NotNull] ICalcAbleObject calcObject, [JetBrains.Annotations.NotNull] Simulator sim,
                    [JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary ltdict)
                {
                    SaveCalculationStartParameters(csps);

                    SaveLtDict(ltdict);
                    foreach (ISingleInputDataLogger logger in _dataLoggers) {
                        logger.Run(sim,calcObject, ltdict);
                    }
                }*/
        /*
        private void SaveLtDict([JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary ltdict)
        {
            var loadtypedtos = ltdict.Ltdtodict.Values.ToList();
            List<LoadTypeDefinition> ltds = new List<LoadTypeDefinition>();
            foreach (CalcLoadTypeDto dto in loadtypedtos)
            {
                LoadTypeDefinition ltd = new LoadTypeDefinition();
                ltd.HouseholdKey = Constants.GeneralHouseholdKey;
                ltd.Json = JsonConvert.SerializeObject(dto,Formatting.Indented);
                ltd.Name = dto.Name;
                ltds.Add(ltd);
            }
            _srfl.SaveToDatabaseAndClearList(ltds);
        }*/
        /*
        public void SaveCalcParameters([JetBrains.Annotations.NotNull] CalcParameters parameters)
        {
            SaveableEntry se = new SaveableEntry(Constants.GeneralHouseholdKey, CalcParameters.TableName, "All the calculation parameters as Json");
            se.AddField("Name", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            se.AddRow(RowBuilder.Start("Name", "Parameters").Add("Json", JsonConvert.SerializeObject(parameters,Formatting.Indented)).ToDictionary());
            _srfl.SaveResultEntry(se);
        }*/
        /*
        public void SaveHouseholdDto([JetBrains.Annotations.NotNull] CalcHouseholdDto household)
        {
            FindMemorySizeOfProperties(household);
            SaveableEntry se = new SaveableEntry(Constants.GeneralHouseholdKey, CalcHouseholdDto.TableName, "All the households");
            se.AddField("Name", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            se.AddRow(RowBuilder.Start("Name", household.Name + " " + household.HouseholdKey).Add("Json", JsonConvert.SerializeObject(household, Formatting.Indented)).ToDictionary());
            _srfl.SaveResultEntry(se);
        }
        */
        /*
        private static void FindMemorySizeOfProperties([JetBrains.Annotations.NotNull] CalcHouseholdDto household)
        {
            var householdProperties = typeof(CalcHouseholdDto).GetProperties();
            foreach (var propinfo in householdProperties)
            {
                object o = propinfo.GetValue(household, null);
                string json = JsonConvert.SerializeObject(o, Formatting.Indented);
                Logger.Info(propinfo.Name + ": " + json.Length);
            }
        }*/
        /*
        private void SaveCalculationStartParameters([JetBrains.Annotations.NotNull] CalcStartParameterSet csps)
        {
            //_fft.RegisterHouseholdKey("Global","GlobalData");
            //_fft.RegisterFile(_srfl.DstFilename,"SQLite results file",true,ResultFileID.Sqlite,"Global",TargetDirectory.Root);
            List<CalculationConfiguration> cs = new List<CalculationConfiguration>();
            cs.Add(new CalculationConfiguration(ConfigurationKeys.ConfigurationKeysDictionary[ConfigurationKey.LPGVersion], csps.LPGVersion));
            cs.Add(new CalculationConfiguration(ConfigurationKeys.ConfigurationKeysDictionary[ConfigurationKey.EnergyIntensity], csps.EnergyIntensity.ToString()));
            if(csps.CalcTarget != null)
            {
                cs.Add(new CalculationConfiguration(ConfigurationKeys.ConfigurationKeysDictionary[ConfigurationKey.CalculationTarget], csps.CalcTarget.Name));
            }
            cs.Add(new CalculationConfiguration(ConfigurationKeys.ConfigurationKeysDictionary[ConfigurationKey.DeviceSelection], csps.DeviceSelection?.Name ?? "none"));
            cs.Add(new CalculationConfiguration(ConfigurationKeys.ConfigurationKeysDictionary[ConfigurationKey.GeographicLocation], csps.GeographicLocation?.Name ?? "none"));
            cs.Add(new CalculationConfiguration(ConfigurationKeys.ConfigurationKeysDictionary[ConfigurationKey.TemperatureProfile], csps.TemperatureProfile?.Name ?? "none"));
            cs.Add(new CalculationConfiguration(ConfigurationKeys.ConfigurationKeysDictionary[ConfigurationKey.TransportationDeviceSet], csps.TransportationDeviceSet?.Name ?? "none"));
            cs.Add(new CalculationConfiguration(ConfigurationKeys.ConfigurationKeysDictionary[ConfigurationKey.TravelRouteSet], csps.TravelRouteSet?.Name ?? "none"));

            //csps.TravelRouteSet
            _srfl.SaveToDatabaseAndClearList(cs);

        }*/
    }
}