//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Globalization;
//using System.IO;
//using CommonDataWPF;

//namespace ChartCreator.SettlementMergePlots {
//    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
//    internal static class EigenverbrauchsCalculator {
//        private static void FitPhotovoltaik(List<Entry> entries, bool normalize, double pvCorrectionFactor) {
//            // calculate the sums
//            double pvsum = 0;
//            double elecsum = 0;
//            if (normalize) {
//                foreach (var entry in entries) {
//                    pvsum += entry.PhotovoltaikRaw;
//                    elecsum += entry.Electricity;
//                }
//            }

//            var factor = pvCorrectionFactor;

//            if (normalize) {
//                factor = elecsum / pvsum;
//            }
//            foreach (var entry in entries) {
//                if (entry.Battery == null) {
//                    entry.PhotovoltaikFitted = entry.PhotovoltaikRaw * factor;
//                    entry.SelfConsumption = 0;
//                    if (entry.Electricity < entry.PhotovoltaikFitted) {
//                        entry.SelfConsumption = entry.Electricity;
//                        entry.GridEinspeisung = -entry.PhotovoltaikFitted + entry.Electricity;
//                    }
//                    else {
//                        entry.SelfConsumption = entry.PhotovoltaikFitted;
//                        entry.GridLast = entry.Electricity - entry.PhotovoltaikFitted;
//                    }
//                }
//                else {
//                    var battery = entry.Battery.Value;
//                    entry.PhotovoltaikFitted = entry.PhotovoltaikRaw * factor;
//                    if (battery < 0) {
//                        entry.ConsumedFromBattery = battery;
//                    }
//                    if (battery > 0) {
//                        entry.SavedToBattery = battery;
//                    }
//                    // battery self consumption

//                    if (entry.Electricity < entry.PhotovoltaikFitted) {
//                        entry.SelfConsumption = entry.Electricity;
//                        entry.GridEinspeisung = -entry.PhotovoltaikFitted + entry.Electricity;

//                        // electricityFromPV = entry.Electricity
//                        // electrictyFromBattery = 0
//                        entry.SelfConsumptionWithBattery = entry.Electricity;
//                        entry.GridEinspeisungWithBattery = -entry.PhotovoltaikFitted + entry.Electricity + battery;
//                    }
//                    else {
//                        entry.SelfConsumption = entry.PhotovoltaikFitted;
//                        entry.GridLast = entry.Electricity - entry.PhotovoltaikFitted;
//                        // electricity from PV = entry.PV_Fitted + battery
//                        entry.SelfConsumptionWithBattery = entry.PhotovoltaikFitted - battery;
//                        entry.GridLoadWithBattery = entry.Electricity - entry.PhotovoltaikFitted + battery;
//                    }
//                }
//                //if (setDictionary)
//                //  entry.AllSet();
//            }
//        }

//        public static List<Entry> ReadFileAndCalculateMatch(string electricity, int dtCol, int elecCol, int pvCol,
//            bool normalize, int batteryCol = -1, double pvCorrectionFactor = -1) {
//            List<Entry> entries;
//            using (var sr = new StreamReader(electricity)) {
//                entries = new List<Entry>();
//                sr.ReadLine();
//                while (!sr.EndOfStream) {
//                    var s = sr.ReadLine();
//                    if (s == null) {
//                        throw new LPGException("file " + electricity + " was null.");
//                    }
//                    var strarr = s.Split(';');
//                    var pv = Convert.ToDouble(strarr[pvCol], CultureInfo.CurrentCulture);
//                    var elec = Convert.ToDouble(strarr[elecCol], CultureInfo.CurrentCulture);

//                    double? battery = null;
//                    if (batteryCol != -1) {
//                        battery = Convert.ToDouble(strarr[batteryCol], CultureInfo.CurrentCulture);
//                    }
//                    //DateTime dt = Convert.ToDateTime(strarr[dtCol], CultureInfo.CurrentCulture);
//                    entries.Add(new Entry(pv, elec, battery));
//                }
//            }
//            FitPhotovoltaik(entries, normalize, pvCorrectionFactor);
//            return entries;
//        }

//        internal class Entry {
//            public Entry(double photovoltaikRaw, double electricity, double? battery) {
//                PhotovoltaikRaw = photovoltaikRaw;
//                Electricity = electricity;
//                //DateTime = dateTime;
//                Battery = battery;
//            }

//            public double? Battery { get; }
//            public double ConsumedFromBattery { get; set; }
//            public double Electricity { get; }
//            //public double GridEinspeisung { private get; set; }
//            public double GridEinspeisungWithBattery { get; set; }
//            public double GridLast { private get; set; }
//            public double GridLoadWithBattery { get; set; }
//            public double PhotovoltaikFitted { get; set; }

//            //private DateTime DateTime { get; set; }

//            public double PhotovoltaikRaw { get; }
//            public double SavedToBattery { get; set; }
//            public double SelfConsumption { get; set; }
//            public double SelfConsumptionWithBattery { get; set; }

//            //private Dictionary<string, double> Values { get; } = new Dictionary<string, double>();
//
//            public void AllSet()
//            {
//                Values.Clear();
//                Values.Add("Electricity", Electricity);
//                Values.Add("PVRaw", PVRaw);
//                Values.Add("SelfConsumption", SelfConsumption);
//                Values.Add("GridEinspeisung", GridEinspeisung);
//                Values.Add("GridLast", GridLast);
//                Values.Add("ConsumedFromBattery", ConsumedFromBattery);
//                Values.Add("SavedToBattery", SavedToBattery);
//                Values.Add("SelfConsumptionWithBattery", SelfConsumptionWithBattery);
//                Values.Add("GridEinspeisungWithBattery", GridEinspeisungWithBattery);
//                Values.Add("GridLoadWithBattery", GridLoadWithBattery);
//                Values.Add("GridInteractionWithBattery", GridLoadWithBattery + GridEinspeisungWithBattery);
//                Values.Add("GridInteractionWithPV", Electricity + PVRaw);
//            }*/

//            /*public void AddEntry(Entry e) {
//                PhotovoltaikRaw += e.PhotovoltaikRaw;
//                Electricity += e.Electricity;
//                PhotovoltaikFitted += e.PhotovoltaikFitted;
//                SelfConsumption += e.SelfConsumption;
//                GridEinspeisung += e.GridEinspeisung;
//                GridLast += e.GridLast;
//            }*/
//        }
//    }
//}