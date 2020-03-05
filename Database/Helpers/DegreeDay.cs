using System;

namespace Database.Helpers {
    public class DegreeDay {
        private readonly DateTime _date;

        public DegreeDay(DateTime date) => _date = date;

        public double AverageTemperature { get; set; }

        public DateTime Date => _date;

        public double HeatingAmount { get; set; }

        public double Percentage { get; set; }

        public int Tempcounter { get; set; }

        public double TempSum { get; set; }

        public int Year => _date.Year;

        public double GetDegreeDay(double heatingTemperature, double roomTemperature) {
            if (AverageTemperature > heatingTemperature) {
                return 0;
            }
            var degreedayvalue = roomTemperature - AverageTemperature;
            return degreedayvalue;
        }
    }
}