namespace Common.CalcDto {
    public class DataPointDto
    {
        public DataPointDto(double myref, double val)
        {
            Ref = myref;
            Val = val;
        }

        public double Ref { get; }

        public double Val { get; }
    }
}