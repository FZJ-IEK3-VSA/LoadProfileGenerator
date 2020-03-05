
//using Calculation.Transportation;
/*
namespace CalculationEngine.OnlineLogging {
    public class TransportationLogFile {

        [NotNull]
        private readonly Dictionary<HouseholdKey, List<TransportationEventEntry>> _allEventEntries =
            new Dictionary<HouseholdKey, List<TransportationEventEntry>>();
        [NotNull]
        private readonly FileFactoryAndTracker _fft;
        [NotNull]
        private readonly CalcParameters _calcParameters;
        [NotNull]
        private readonly Dictionary<HouseholdKey, StreamWriter> _sws;

        public TransportationLogFile([NotNull] FileFactoryAndTracker fft, [NotNull] CalcParameters calcParameters)
        {
            _fft = fft;
            _calcParameters = calcParameters;
            _sws = new Dictionary<HouseholdKey, StreamWriter>();
        }

        [NotNull]
        [ItemNotNull]
        public List<HouseholdKey> ProcessedHouseholdEntries { get; } = new List<HouseholdKey>();
        //private TransportationHandler _th;

        public void AddTransportationEvent([NotNull] HouseholdKey householdkey, [NotNull] string person, int timestep, [NotNull] string site, [NotNull] string route,
            [NotNull] string transportationDevice, int transportationDuration, int affordanceDuration, [NotNull] string affordanceName, [NotNull][ItemNotNull] List<CalcTravelRoute.CalcTravelDeviceUseEvent> travelDeviceUseEvents)
        {
            if (!_allEventEntries.ContainsKey(householdkey)) {
                _allEventEntries.Add(householdkey, new List<TransportationEventEntry>());
            }

            if (transportationDuration > 0) {
                int mytimestep = timestep;
                foreach (CalcTravelRoute.CalcTravelDeviceUseEvent calcTravelDeviceUseEvent in travelDeviceUseEvents) {
                    TransportationEventEntry tee = new TransportationEventEntry(person, mytimestep, site,
                        CurrentActivity.InTransport,
                        route + " with " + transportationDevice + " for " + transportationDuration + " steps to " +
                        affordanceName,calcTravelDeviceUseEvent.Device.Name);
                    _allEventEntries[householdkey].Add(tee);
                    mytimestep += calcTravelDeviceUseEvent.DurationInSteps;
                }
            }

            TransportationEventEntry tee2 = new TransportationEventEntry(person, timestep + transportationDuration,
                site,
                CurrentActivity.InAffordance, affordanceName,string.Empty);
            _allEventEntries[householdkey].Add(tee2);
        }

        public void Close()
        {
            foreach (var pair in _sws) {
                pair.Value.Close();
            }

            //TODO: should be external time steps + dummy time handling

            foreach (var householdEntries in _allEventEntries)
            {
                if (ProcessedHouseholdEntries.Contains(householdEntries.Key))
                {
                    continue;
                }
                CreateTransportationStatisticsForOneHousehold(householdEntries);
            }
        }

        private void CreateTransportationStatisticsForOneHousehold(KeyValuePair<HouseholdKey, List<TransportationEventEntry>> householdEntries)
        {
            ProcessedHouseholdEntries.Add(householdEntries.Key);
            var eventList = householdEntries.Value.ToList();
            List<string> personNames = eventList.Select(x => x.Person).Distinct().ToList();
            Dictionary<string, TransportationEntry> states = new Dictionary<string, TransportationEntry>();
            foreach (string personName in personNames)
            {
                states.Add(personName, new TransportationEntry(personName));
            }

            eventList.Sort((x, y) => x.Timestep.CompareTo(y.Timestep));
            var csv = _fft.MakeFile<StreamWriter>("TransportationStatistics." + householdEntries.Key + ".csv",
                "Status of each Person (Traveling or doing an affordance)" + householdEntries.Key,
                false, ResultFileID.TransportationStatistics,
                householdEntries.Key, TargetDirectory.Root, new TimeSpan(0, 1, 0));
            string s = "Timestep;";
            foreach (string name in personNames)
            {
                s += name + " Site;" + name + " Activity;" + name + " in Affordance;" + name + " in Transport;" +
                     name + " Activity Description;Transportation Device;";
            }

            csv.WriteLine(s);
            for (int i = 0; i < _calcParameters.InternalTimesteps; i++)
            {
                while (eventList.Count > 0 && eventList[0].Timestep == i)
                {
                    TransportationEventEntry entry = eventList[0];
                    eventList.RemoveAt(0);
                    states[entry.Person].CurrentSite = entry.Site;
                    states[entry.Person].CurrentActivity = entry.Activity;
                    states[entry.Person].Description = entry.Description;
                    states[entry.Person].TransportationDevice = entry.TransportationDevice;
                    if (entry.Activity == CurrentActivity.InTransport)
                    {
                        states[entry.Person].IsInTransport = 1;
                        states[entry.Person].IsInAffordance = 0;
                    }
                    else
                    {
                        states[entry.Person].IsInAffordance = 1;
                        states[entry.Person].IsInTransport = 0;
                    }
                }

                s = i + ";";
                foreach (string name in personNames)
                {
                    TransportationEntry te = states[name];
                    s += te.CurrentSite + ";" + te.CurrentActivity + ";" + te.IsInAffordance + ";" +
                         te.IsInTransport + ";" + te.Description + ";" + te.TransportationDevice + ";";
                }

                csv.WriteLine(s);
            }

            csv.Close();
        }

        public void InitSw([NotNull] HouseholdKey householdkey)
        {
            if (_sws.ContainsKey(householdkey)) {
                return;
            }

            var sw = _fft.MakeFile<StreamWriter>("TransportationLogfile." + householdkey + ".txt",
                "All Transportation related events for " + householdkey,
                false, ResultFileID.Transportation,
                householdkey, TargetDirectory.Root, new TimeSpan(0, 1, 0));
            _sws.Add(householdkey, sw);
        }

        public void Report([NotNull] HouseholdKey householdkey, [NotNull] string s)
        {
            if (!_sws.ContainsKey(householdkey)) {
                InitSw(householdkey);
            }

            _sws[householdkey].WriteLine(s);
            _sws[householdkey].Flush();
        }

       

      

        /*
        public void SetTransportationHandler(TransportationHandler transportationHandler)
        {
            _th = transportationHandler;
        }
    }
}*/