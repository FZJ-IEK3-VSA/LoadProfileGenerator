using System;
using System.Text;
using Common.JSON;

namespace Common.SQLResultLogging
{
    public class DateStampCreator {
        //TODO: clean up this class thoroughly and eliminate the duplicates
        [JetBrains.Annotations.NotNull]
        private readonly CalcParameters _calcParameters;

        public DateStampCreator([JetBrains.Annotations.NotNull] CalcParameters calcParameters)
        {
            _calcParameters = calcParameters;
        }

        [JetBrains.Annotations.NotNull]
        public string MakeDateStringFromTimeStep([JetBrains.Annotations.NotNull] TimeStep time)
        {
            var totalsecondsPerStep = _calcParameters.InternalStepsize.TotalSeconds;
            var totalseconds = (int)(time.ExternalStep * totalsecondsPerStep);
            var ts = new TimeSpan(0, 0, 0, totalseconds);
            var dt = _calcParameters.OfficialStartTime + ts;
            return dt.ToShortDateString() + " " + dt.ToShortTimeString();
        }
        // gets the external date from a external time step
        // this ignores the settling period
        public DateTime MakeDateFromTimeStep([JetBrains.Annotations.NotNull] TimeStep time)
        {
            var totalsecondsPerStep = _calcParameters.InternalStepsize.TotalSeconds;
            var totalseconds = (int)(time.ExternalStep * totalsecondsPerStep);
            var ts = new TimeSpan(0, 0, 0, totalseconds);
            return _calcParameters.OfficialStartTime + ts;
        }

        [JetBrains.Annotations.NotNull]
        public string MakeDateStringFromTimeStep([JetBrains.Annotations.NotNull] TimeStep time, [JetBrains.Annotations.NotNull] out string weekdayname)
        {
            var totalsecondsPerStep = _calcParameters.InternalStepsize.TotalSeconds;
            var totalseconds = (int)(time.ExternalStep * totalsecondsPerStep);
            var ts = new TimeSpan(0, 0, 0, totalseconds);
            var dt = _calcParameters.OfficialStartTime + ts;
            weekdayname = dt.DayOfWeek.ToString();
            return dt.ToShortDateString() + " " + dt.ToShortTimeString();
        }

        public  void GenerateDateStampForTimestep([JetBrains.Annotations.NotNull] TimeStep calcTimestamp, [JetBrains.Annotations.NotNull] StringBuilder sb) {
            sb.Append(calcTimestamp.ExternalStep);
            sb.Append(_calcParameters.CSVCharacter);
            sb.Append(MakeDateStringFromTimeStep(calcTimestamp));
            sb.Append(_calcParameters.CSVCharacter);
            if (_calcParameters.WriteExcelColumn) {
                sb.Append("'");
                sb.Append(MakeDateStringFromTimeStep(calcTimestamp));
                sb.Append(_calcParameters.CSVCharacter);
            }
        }

        [JetBrains.Annotations.NotNull]
        public  string GenerateDateStampHeader() {
            var s = "Timestep" + _calcParameters.CSVCharacter + "Time" +
                    _calcParameters.CSVCharacter;
            if (_calcParameters.WriteExcelColumn) {
                s += "Excel-Time" +_calcParameters.CSVCharacter;
            }
            return s;
        }

        [JetBrains.Annotations.NotNull]
        public string MakeTimeString([JetBrains.Annotations.NotNull] TimeStep timestep) {
            var sb = new StringBuilder();
            GenerateDateStampForTimestep(timestep, sb);
            return sb.ToString();
        }
    }
}