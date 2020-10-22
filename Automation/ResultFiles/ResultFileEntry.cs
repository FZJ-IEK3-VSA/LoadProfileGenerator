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

#region

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Automation.ResultFiles {
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
        [NotNull]
        private Dictionary<string, string> HouseholdNameByHouseholdKey { get; set; } = new Dictionary<string, string>();
        */
        [UsedImplicitly]
        [NotNull]
        public string OriginalPath { get; set; }

        [NotNull]
        [UsedImplicitly]
        public Dictionary<string, ResultFileEntry> ResultFiles { get; set; } = new Dictionary<string, ResultFileEntry>();

        public void AddExistingEntries([NotNull] [ItemNotNull] List<ResultFileEntry> rfes)
        {
            foreach (var rfe in rfes) {
                ResultFiles.Add(rfe.HashKey, rfe);
            }
        }

        public void AdjustPath([NotNull] string helperoriginalPath, [NotNull] string newPath, bool tolerateMissingFiles)
        {
            //only temporary until the next run
            var oldPath = helperoriginalPath;
            //if (!string.IsNullOrWhiteSpace(OriginalPath)) {
//                oldPath = OriginalPath;
            //          }
            foreach (var rfe in ResultFiles) {
                rfe.Value.FullFileName = rfe.Value.FullFileName?.Replace(oldPath, newPath);
                if (rfe.Value.ResultFileID == ResultFileID.LogfileForErrors) {
                    continue;
                }

                if (!File.Exists(rfe.Value.FullFileName) && !tolerateMissingFiles) {
                    throw new LPGException("missing: " + rfe.Value.FullFileName);
                }
            }
        }

        /*
        [NotNull]
        public static ResultFileEntry LoadAndGetByFileName([NotNull] string directory, [NotNull] string fileName)
        {
            var rfl = ReadResultEntries(directory);
            return rfl.GetByFilename(fileName);
        }
        */
        /*
        [NotNull]
        public static ResultFileList ReadResultEntries([NotNull] string directoryWithResultFile)
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
        public void WriteResultEntries([NotNull] string path)
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
        /*  [NotNull]
          private ResultFileEntry GetByFilename([NotNull] string fileName) =>
              ResultFiles.Values.First(x => x.FileName == fileName);*/
    }

#pragma warning disable CA1036 // Override methods on comparable types
    public class ResultFileEntry : IComparable {
#pragma warning restore CA1036 // Override methods on comparable types
        public ResultFileEntry([NotNull] string name,
                               [NotNull] FileInfo fileInfo,
                               bool displayEntry,
                               [CanBeNull] StreamWriter streamWriter,
                               [CanBeNull] BinaryWriter binaryWriter,
                               [NotNull] Stream stream,
                               ResultFileID rfid,
                               [NotNull] string householdName,
                               [NotNull] string householdKey,
                               [NotNull] LoadTypeInformation lti,
                               [NotNull] PersonInformation pi,
                               [NotNull] string additionalFileIndex,
                               TimeSpan timeResolution, CalcOption enablingOption)
        {
            EnablingCalcOption = enablingOption;
            TimeResolution = timeResolution;
            Name = name;
            FileName = fileInfo.Name;
            FullFileName = fileInfo.FullName;
            Stream = stream;
            DisplayEntry = displayEntry;
            StreamWriter = streamWriter;
            BinaryWriter = binaryWriter;
            ResultFileID = rfid;
            HouseholdName = householdName;
            LoadTypeInformation = lti;
            HouseholdKey = householdKey;
            PersonInformation = pi;
            AdditionalFileIndex = additionalFileIndex;
            if (FileName == FullFileName) {
                throw new LPGException("Full filename was same as filename");
            }

            if (FileName.Contains(":")) {
                throw new LPGException("Somehow the full path ended up in the Filename: " +FileName);
            }
        }

        public ResultFileEntry([NotNull] string name, [NotNull] FileInfo fileInfo, bool displayEntry, CalcOption enablingOption)
        {
            Name = name;
            FileName = fileInfo.Name;
            FullFileName = fileInfo.FullName;
            DisplayEntry = displayEntry;
            EnablingCalcOption = enablingOption;

            if (FileName == FullFileName) {
                throw new LPGException("Full filename was same as filename");
            }

            if (FileName.Contains(":")) {
                throw new LPGException("Somehow the full path ended up in the Filename: " + FileName);
            }
        }

        public ResultFileEntry([NotNull] string name,
                               [NotNull] FileInfo fileinfo,
                               bool displayEntry,
                               ResultFileID rfid,
                               [NotNull] string householdKey,
                               [CanBeNull] string? additionalFileIndex, CalcOption enablingOption)
        {
            Name = name;
            FileName = fileinfo.Name;
            FullFileName = fileinfo.FullName;
            DisplayEntry = displayEntry;
            ResultFileID = rfid;
            HouseholdKey = householdKey;
            AdditionalFileIndex = additionalFileIndex;
            EnablingCalcOption = enablingOption;
            if (FileName == FullFileName) {
                throw new LPGException("Full filename was same as filename");
            }

            if (FileName.Contains(":")) {
                throw new LPGException("Somehow the full path ended up in the Filename: " + FileName);
            }
        }

        // ReSharper disable once NotNullMemberIsNotInitialized
        [Obsolete("json only")]
        public ResultFileEntry()
        {
        }

        [UsedImplicitly]
        public string? AdditionalFileIndex { get; set; }
        [UsedImplicitly]
        public CalcOption EnablingCalcOption { get; set; }

        [JsonIgnore]
        [CanBeNull]
        public BinaryWriter? BinaryWriter { get; }

        [UsedImplicitly]
        public bool DisplayEntry { get; set; }

        [UsedImplicitly]
        [NotNull]
        public string? FileName { get; set; }

        [UsedImplicitly]
        [NotNull]
        public string? FullFileName { get; set; }

        [NotNull]
        public string HashKey => CalculateHashKey(ResultFileID,
            HouseholdName,
            LoadTypeInformation?.Name,
            HouseholdKey,
            PersonInformation?.Name,
            AdditionalFileIndex);

        [UsedImplicitly]
        [CanBeNull]
        public string? HouseholdKey { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public string? HouseholdName { get; set; }

        [JsonIgnore]
        [UsedImplicitly]
        public string? HouseholdNumberString => HouseholdKey;

        [UsedImplicitly]
        [CanBeNull]
        public LoadTypeInformation? LoadTypeInformation { get; set; }

        [UsedImplicitly]
        public string? Name { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public PersonInformation? PersonInformation { get; set; }

        [UsedImplicitly]
        [NotNull]
        public string PrettySize => AutomationUtili.MakePrettySize(Size);

        [UsedImplicitly]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultFileID ResultFileID { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public long Size {
            get {
                if (FullFileName!= null&& File.Exists(FullFileName)) {
                    var fi = new FileInfo(FullFileName);
                    return fi.Length;
                }

                return 0;
            }
        }

        [JsonIgnore]
        [CanBeNull]
        public Stream? Stream { get; }

        [JsonIgnore]
        [CanBeNull]
        public StreamWriter? StreamWriter { get; }

        [UsedImplicitly]
        public TimeSpan TimeResolution { get; set; }

        public int CompareTo([CanBeNull] object obj)
        {
            if (!(obj is ResultFileEntry rfe)) {
                throw new LPGException("invalid object");
            }

            return string.Compare(FullFileName, rfe.FullFileName, StringComparison.Ordinal);
        }

        [NotNull]
        public static string CalculateHashKey(ResultFileID resultFileID,
                                              [CanBeNull] string? householdName,
                                              [CanBeNull] string? loadTypeName,
                                              [CanBeNull] string? householdKey,
                                              [CanBeNull] string? personName,
                                              [CanBeNull] string? additionalFileIndex)
        {
            var s = "";
            s += resultFileID + "#";
            s += householdName + "#";
            s += loadTypeName + "#";
            s += householdKey + "#";
            s += personName + "#";
            s += additionalFileIndex + "#";
            return s;
        }

        [NotNull]
        public override string ToString() => FileName + " (" + ResultFileID + ")";
    }
}