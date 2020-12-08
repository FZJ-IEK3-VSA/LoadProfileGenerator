using System.Collections.Generic;
using System.IO;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common {
    public class ResultFileList {
        public ResultFileList([NotNull] string calcObjectName, [NotNull] string originalPath)
        {
            CalcObjectName = calcObjectName;
            OriginalPath = originalPath;
        }

        [UsedImplicitly]
        [NotNull]
        public string CalcObjectName { get; set; }
        /*
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        private Dictionary<string, string> HouseholdNameByHouseholdKey { get; set; } = new Dictionary<string, string>();
        */
        [UsedImplicitly]
        [NotNull]
        public string OriginalPath { get; set; }

        [NotNull]
        [UsedImplicitly]
        public Dictionary<string, ResultFileEntry> ResultFiles { get; set; } =
            new Dictionary<string, ResultFileEntry>();

        public void AddExistingEntries([NotNull] [ItemNotNull] List<ResultFileEntry> rfes)
        {
            foreach (var rfe in rfes) {
                ResultFiles.Add(rfe.HashKey, rfe);
            }
        }

        public void AdjustPath([NotNull] string helperoriginalPath, [NotNull] string newPath, bool tolerateMissingFiles)
        {
            //only temporary until the next run
            string oldPath = helperoriginalPath;
            //if (!string.IsNullOrWhiteSpace(OriginalPath)) {
//                oldPath = OriginalPath;
            //          }
            foreach (var rfe in ResultFiles) {
                if (rfe.Value.FullFileName == null) {
                    throw new LPGException("was null");
                }
                rfe.Value.FullFileName = rfe.Value.FullFileName.Replace(oldPath, newPath);
                if (rfe.Value.ResultFileID == ResultFileID.LogfileForErrors) {
                    continue;
                }

                if (!File.Exists(rfe.Value.FullFileName) && !tolerateMissingFiles) {
                    throw new LPGException("missing: " + rfe.Value.FullFileName);
                }
            }
        }
        /*
        [JetBrains.Annotations.NotNull]
        public static ResultFileEntry LoadAndGetByFileName([JetBrains.Annotations.NotNull] string directory, [JetBrains.Annotations.NotNull] string fileName)
        {
            var rfl = ReadResultEntries(directory);
            return rfl.GetByFilename(fileName);
        }
        */
        /*
        [JetBrains.Annotations.NotNull]
        public static ResultFileList ReadResultEntries([JetBrains.Annotations.NotNull] string directoryWithResultFile)
        {
            var dstPath = Path.Combine(directoryWithResultFile, Constants.ResultJsonFileName);
            string json;
            using (var sw = new StreamReader(dstPath)) {
                json = sw.ReadToEnd();
            }

            try {
                var o = JsonConvert.DeserializeObject<ResultFileList>(json);
                return o;
            }
            catch (Exception e) {
                Logger.Error("Failed to deserialize " + dstPath);
                Logger.Exception(e);
                throw;
            }
        }
        */
        /*
        public void WriteResultEntries([JetBrains.Annotations.NotNull] string path)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Include
            });
            var dstPath = Path.Combine(path, Constants.ResultJsonFileName);
            using (var sw = new StreamWriter(dstPath)) {
                sw.WriteLine(json);
            }
        }
        */
        /*  [JetBrains.Annotations.NotNull]
        private ResultFileEntry GetByFilename([JetBrains.Annotations.NotNull] string fileName) =>
            ResultFiles.Values.First(x => x.FileName == fileName);*/
    }
}