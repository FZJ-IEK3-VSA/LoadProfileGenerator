using System.IO;
using JetBrains.Annotations;

namespace SimulationEngineLib.HouseJobProcessor {
    public class CalcJobQueueEntry {
        public CalcJobQueueEntry([NotNull] FileInfo jsonFile, int index)
        {
            JsonFile = jsonFile;
            Index = index;
        }

        [NotNull]
        public FileInfo JsonFile { get; set; }
        public int Index { get; set; }
    }
}