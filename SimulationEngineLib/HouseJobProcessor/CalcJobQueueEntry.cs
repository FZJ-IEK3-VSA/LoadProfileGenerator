using System.IO;

namespace SimulationEngineLib.HouseJobProcessor {
    public class CalcJobQueueEntry {
        public CalcJobQueueEntry([JetBrains.Annotations.NotNull] FileInfo jsonFile, int index)
        {
            JsonFile = jsonFile;
            Index = index;
        }

        [JetBrains.Annotations.NotNull]
        public FileInfo JsonFile { get; set; }
        public int Index { get; set; }
    }
}