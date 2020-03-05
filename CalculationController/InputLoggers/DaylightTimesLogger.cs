using Automation.ResultFiles;

namespace CalculationController.InputLoggers
{
    //using System.Collections.Generic;
    using CalculationEngine.Helper;
    using Common;
    using Common.JSON;
    using Common.SQLResultLogging;
    using JetBrains.Annotations;

    public class EveryDayLightTimes //: ITypeDescriber
    {
        public EveryDayLightTimes([NotNull] string dateTime, int timeStep, bool isLight)
        {
            DateTime = dateTime;
            TimeStep = timeStep;
            IsLight = isLight;
            HouseholdKey = Constants.GeneralHouseholdKey;
        }

        //public string GetTypeDescription(){return "A list of the calculated daylight times";}

        //public int ID { get; set; }
        [NotNull]
        public HouseholdKey HouseholdKey { get;  }

        [UsedImplicitly]
        [NotNull]
        public string DateTime { get; set; }
        [UsedImplicitly]
        public int TimeStep { get; set; }
        [UsedImplicitly]
        public bool IsLight { get; set; }
    }

    public class DaylightTimesLogger:DataSaverBase
    {
        [NotNull]
        private readonly SqlResultLoggingService _srls;
        [NotNull]
        private readonly CalcParameters _calcParameters;
        [NotNull]
        private readonly DateStampCreator _dsc;

        public DaylightTimesLogger([NotNull] SqlResultLoggingService srls, [NotNull] CalcParameters calcParameters):base(typeof(DayLightStatus),
           new ResultTableDefinition("DaylightTimes",ResultTableID.DaylightTimes,  "Time of daylight each day"),srls)
        {
            _srls = srls;
            _calcParameters = calcParameters;
            _dsc = new DateStampCreator(calcParameters);
        }

        public override void Run([NotNull] HouseholdKey key, [NotNull] object o)
        {
            DayLightStatus dayLightStatus = (DayLightStatus)o;
            //List<EveryDayLightTimes> daylights = new List<EveryDayLightTimes>();
            var startTimeStep = 0;
            if (!_calcParameters.ShowSettlingPeriodTime)
            {
                startTimeStep = _calcParameters.DummyCalcSteps;
            }
            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("Timestep",SqliteDataType.Integer);
            se.AddField("DateTime", SqliteDataType.Integer);
            se.AddField("Daylight", SqliteDataType.Bit);
            for (var i = startTimeStep; i < dayLightStatus.Status.Count; i++)
            {
                TimeStep ts = new TimeStep(i,_calcParameters);
                string timestamp =  _dsc.MakeDateStringFromTimeStep(ts);
                se.AddRow(RowBuilder.Start("Timestep", ts.ExternalStep).Add("DateTime",timestamp).Add("Daylight", dayLightStatus.Status[i]).ToDictionary());
                //EveryDayLightTimes edlt = new EveryDayLightTimes(timestamp,timestep, );
                //daylights.Add(edlt);
            }
            _srls.SaveResultEntry(se);
        }
    }
}
