namespace Common.CalcDto {
    public class CalcDegreeDayDto
    {
        public CalcDegreeDayDto(int year, int month, int day, double heatingAmount)
        {
            Year = year;
            Month = month;
            Day = day;
            HeatingAmount = heatingAmount;
        }

        public int Day { get;  }
        public double HeatingAmount { get;  }

        public int Month { get;  }

        public int Year { get; }
    }
}