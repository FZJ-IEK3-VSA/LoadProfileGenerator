//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

//namespace Common.Enums {
    /*
    public class CalculationResult {
        [UsedImplicitly]
        public CalculationResult() // needed for serializing
        {
    //        ResultFileEntries = new List<ResultFileEntry>();
      //      HiddenResultFileEntries = new List<ResultFileEntry>();
            TotalsPerLoadType = new Dictionary<string, double>();
            CalcObjectName = "no name";
            MainSqlFilePath = "";
        }

        public CalculationResult([JetBrains.Annotations.NotNull] string objectName, DateTime calcStartTime, DateTime simStart, DateTime simEnd,
            CalcObjectType calcObjectType) {
          //  ResultFileEntries = new List<ResultFileEntry>();
//            HiddenResultFileEntries = new List<ResultFileEntry>();
            TotalsPerLoadType = new Dictionary<string, double>();
            CalcObjectName = objectName;
            CalcStartTime = calcStartTime;
            SimStartTime = simStart;
            SimEndTime = simEnd;
            CalcObjectType = calcObjectType;
        }

        public DateTime CalcEndTime { get; set; }
        [JetBrains.Annotations.NotNull]
        public string CalcObjectName { get; }

        public CalcObjectType CalcObjectType { get; }
        [JetBrains.Annotations.NotNull]
        public string MainSqlFilePath { get; }

        public DateTime CalcStartTime { get; }
        //[JetBrains.Annotations.NotNull]
        //[ItemNotNull]
        //public List<ResultFileEntry> HiddenResultFileEntries { get; }

        public int RandomSeed { get; set; }
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<ResultFileEntry> ResultFileEntries { get; }

        public DateTime SimEndTime { get; }

        public DateTime SimStartTime { get; }

        [XmlIgnore]
        [JetBrains.Annotations.NotNull]
        public Dictionary<string, double> TotalsPerLoadType { get; }

        private static void AdjustOnePath([JetBrains.Annotations.NotNull] DirectoryInfo simPath, [JetBrains.Annotations.NotNull] string housename, [JetBrains.Annotations.NotNull] ResultFileEntry entry) {
            if (!File.Exists(entry.FullFileName)) {
                Logger.Error("File " + entry.FullFileName + " does not seem to exist. Probably moved. Adjusting...");
                var oldFi = new FileInfo(entry.FullFileName);

                if (oldFi.Directory?.Parent == null) {
                    throw new LPGException("Parentdirectory was null");
                }
                if (oldFi.Directory.Name == housename) {
                    entry.FullFileName = Path.Combine(simPath.FullName, oldFi.Name);
                }
                else if (oldFi.Directory.Parent.Name == housename) {
                    entry.FullFileName = Path.Combine(simPath.FullName, oldFi.Directory.Name, oldFi.Name);
                }
                else {
                    Logger.Error("could not adjust file path");
                }
                if (!File.Exists(entry.FullFileName)) {
                    Logger.Error("Adjusting to " + entry.FullFileName + " failed. File not found.");
                }
            }
        }

        public void AdjustPaths([JetBrains.Annotations.NotNull] string directory) {
            var simPath = new DirectoryInfo(directory);
            var housename = simPath.Name;
            foreach (var entry in ResultFileEntries) {
                AdjustOnePath(simPath, housename, entry);
            }

            foreach (var entry in HiddenResultFileEntries) {
                AdjustOnePath(simPath, housename, entry);
            }
        }
        
    }*/
//}