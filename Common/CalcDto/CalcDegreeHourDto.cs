using System;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcDegreeHourDto
    {
        public CalcDegreeHourDto(int year, int month, int day, int hour, double coolingAmount)
        {
            Day = day;
            CoolingAmount = coolingAmount;
            Hour = hour;
            Month = month;
            Year = year;
        }

        public int Day { get;  }
        public double CoolingAmount { get;  }
        public int Hour { get;  }
        public int Month { get; }
        public int Year { get;  }

        [NotNull]
        public override string ToString()
        {
            var dt = new DateTime(Year, Month, Day, Hour, 0, 0);
            var s = dt.ToShortDateString() + " " + dt.ToShortTimeString() + " " + CoolingAmount;
            return s;
        }
    }
}