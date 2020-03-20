//TODO: Implement the leftovers to the simulationengine options

//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using CommonDataWPF;
//using CommonDataWPF.Enums;
//using DatabaseIO.Tables;

//namespace SimulationEngine.Calculation {
//    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
//    public class OutputFileOptions {
//        public OutputFileOptions(bool writeExcelColumn, bool showSettlementPeroid) {
//            WriteExcelColumn = writeExcelColumn;
//            ShowSettlementPeroid = showSettlementPeroid;
//        }

//        public List<CalcOption> Options { get; } = new List<CalcOption>();
//        public OutputFileDefault OutputFileDefault { get; } = OutputFileDefault.None;

//        public bool ShowSettlementPeroid { get; }

//        public bool WriteExcelColumn { get; }

//        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
//        public void SetOptions(GeneralConfig gc) {
//            gc.ApplyOptionDefault(OutputFileDefault);
//            foreach (var calcOption in Options) {
//                gc.Enable(calcOption);
//            }
//            if (WriteExcelColumn) {
//                gc.WriteExcelColumn = "TRUE";
//            }

//            if (ShowSettlementPeroid) {
//                gc.ShowSettlingPeriod = "TRUE";
//            }
//        }
//    }
//}