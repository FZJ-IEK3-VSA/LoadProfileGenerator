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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

#endregion

namespace Common {
    public class HouseholdRegistry {
        [ItemNotNull]
        [NotNull]
        private readonly HashSet<HouseholdKey> _householdKeys = new HashSet<HouseholdKey>();

        //[ItemNotNull]
        //[NotNull]
        //private readonly List<HouseholdKeyEntry> _householdEntries = new List<HouseholdKeyEntry>();

        public bool IsHouseholdRegistered([NotNull] HouseholdKey key)
        {
            if (_householdKeys.Contains(key)) {
                return true;
            }
            return false;
        }

        public void RegisterHousehold([NotNull] HouseholdKey key, [NotNull] string name, HouseholdKeyType type, [NotNull] IInputDataLogger idl, [NotNull] string description,
                                      [CanBeNull] string houseName, [CanBeNull] string houseDescription)
        {
            if(_householdKeys.Contains(key)) {
                return;
                //throw new LPGException("Key was already registered: " + key.Key);
            }

            HouseholdKeyEntry hhke = new HouseholdKeyEntry(key,name,type,description, houseName, houseDescription);
            _householdKeys.Add(key);
            //_householdEntries.Add(hhke);
            idl.Save(hhke);
        }

        public void AddSavedEntry([NotNull] HouseholdKeyEntry entry)
        {
            if (_householdKeys.Contains(entry.HouseholdKey))
            {
                throw new LPGException("Key was already registered: " + entry.HouseholdKey);
            }

            _householdKeys.Add(entry.HouseholdKey);
            //_householdEntries.Add(entry);
        }
    }

    public class FileFactoryAndTracker: IDisposable {
        [NotNull]
        //public HashSet<HouseholdKey> HouseholdKeys => _householdKeys;
        public HouseholdRegistry HouseholdRegistry { get; } = new HouseholdRegistry();

        [NotNull]
        private readonly string _baseResultpath;
        [NotNull]
        private readonly string _calcObjectName;

        [NotNull] private readonly IInputDataLogger _inputDataLogger;

        [NotNull]
        private readonly IGetStreams _getStream;

        [NotNull]
        public ResultFileList ResultFileList { get; }

        [NotNull]
        private readonly HashSet<int> _allValidIds = new HashSet<int>();

        public FileFactoryAndTracker([NotNull] string resultpath, [NotNull] string calcObjectName,
                                     [NotNull] IInputDataLogger inputDataLogger,
                                     [CanBeNull] IGetStreams sw = null)
        {
            ResultFileList = new ResultFileList(calcObjectName, resultpath);
            if (sw == null) {
                _getStream = new GetFileStreamGetter();
            }
            else {
                _getStream = sw;
            }
            _baseResultpath = resultpath;
            _calcObjectName = calcObjectName;
            _inputDataLogger = inputDataLogger;
            ResultFileList.CalcObjectName = calcObjectName;

            var invalidPathChars = Path.GetInvalidPathChars();
            foreach (var invalidPathChar in invalidPathChars) {
                if (_baseResultpath.IndexOf(invalidPathChar) > -1) {
                    _baseResultpath = _baseResultpath.Replace(invalidPathChar, '_');
                }
            }

            foreach (TargetDirectory dir in Enum.GetValues(typeof(TargetDirectory))) {
                if (dir != TargetDirectory.Undefined) {
                    if (!Directory.Exists(GetFullPathForTargetdirectry(dir))) {
                        Directory.CreateDirectory(GetFullPathForTargetdirectry(dir));
                    }
                }
            }
            //ResultFileList.ResultFiles = new Dictionary<string, ResultFileEntry>();
            foreach (var rfid in Enum.GetValues(typeof(ResultFileID))) {
                var id = (int) rfid;
                if (_allValidIds.Contains(id)) {
                    throw new LPGException("RFID " + rfid + " duplicate id.");
                }
                _allValidIds.Add(id);
            }
            //ResultFileList.OriginalPath = resultpath;
            //ResultFileList.HouseholdNameByHouseholdKey.Add(Constants.GeneralHouseholdKey.Key,"General Result Files");
            //ResultFileList.HouseholdNameByHouseholdKey.Add(Constants.TotalsKey.Key, "Totals Files");
        }

        public void ReadExistingFilesFromSql()
        {
            SqlResultLoggingService srls = new SqlResultLoggingService(_baseResultpath);
            HouseholdKeyLogger hhKeyLogger = new HouseholdKeyLogger(srls);
            var hhkes = hhKeyLogger.Load();
            foreach (HouseholdKeyEntry entry in hhkes) {
                HouseholdRegistry.AddSavedEntry(entry);
            }

            var rfel = new ResultFileEntryLogger(srls,true);
            var rfes = rfel.Load();
                ResultFileList.AddExistingEntries(rfes);
            _inputDataLogger.AddSaver(rfel);
        }
        public bool CheckForResultFileEntry(ResultFileID rfid, [NotNull] string loadTypeName,
            [NotNull] HouseholdKey householdKey, [CanBeNull] PersonInformation pi,[CanBeNull] string additionalFileIndex)
        {
            var key = ResultFileEntry.CalculateHashKey(rfid, _calcObjectName, loadTypeName, householdKey.Key, pi?.Name,
                additionalFileIndex);
            return ResultFileList.ResultFiles.ContainsKey(key);
        }


        [NotNull]
        private string GetFullPathForTargetdirectry(TargetDirectory targetDirectory)
        {
            var newdirname = DirectoryNames.CalculateTargetdirectory(targetDirectory);
            return Path.Combine(_baseResultpath, newdirname);
        }

        [NotNull]
        public ResultFileEntry GetResultFileEntry(ResultFileID rfid, [CanBeNull] string loadTypeName,
            [NotNull] HouseholdKey householdKey, [CanBeNull] PersonInformation pi, [CanBeNull]  string additionalFileIndex)
        {
            var key = ResultFileEntry.CalculateHashKey(rfid, _calcObjectName, loadTypeName, householdKey.Key, pi?.Name,
                additionalFileIndex);

            if (!ResultFileList.ResultFiles.ContainsKey(key)) {
                throw new LPGException("The file with the ID " + rfid + ", the loadtype " + loadTypeName +
                                       ", and the householdID " + householdKey +
                                       " is missing. This is probably a bug. Please report.");
            }
            return ResultFileList.ResultFiles[key];
        }

        public bool CheckForFile( ResultFileID rfid1, [NotNull] HouseholdKey householdKey, [CanBeNull] string additionalFileIndex = null)
        {
            string key = ResultFileEntry.CalculateHashKey(rfid1, null, null, householdKey.Key, null, additionalFileIndex);
            if (ResultFileList.ResultFiles.ContainsKey(key)) {
                return true;
            }
            return false;
        }

        public void RegisterFile([NotNull] string fileName, [NotNull] string description, bool displayResultFileEntry, ResultFileID rfid1,
                                 [NotNull] HouseholdKey householdKey, TargetDirectory targetDirectory,
                                 [CanBeNull] string additionalFileIndex = null)
        {
            if (!HouseholdRegistry.IsHouseholdRegistered(householdKey)) {
                throw new LPGException("Forgotten Household Key: " + householdKey);
            }

            string fullFileName = Path.Combine(GetFullPathForTargetdirectry(targetDirectory), fileName);
            FileInfo fi = new FileInfo(fullFileName);
            ResultFileEntry rfe =new ResultFileEntry(description, fi,displayResultFileEntry,rfid1,householdKey.Key,
                additionalFileIndex);
            _inputDataLogger.Save(rfe);
            ResultFileList.ResultFiles.Add(rfe.HashKey, rfe);
        }

        [NotNull]
        public T MakeFile<T>([NotNull] string fileName, [NotNull] string description,
                             bool displayResultFileEntry, ResultFileID rfid1,
                             [NotNull] HouseholdKey householdKey,
                             TargetDirectory targetDirectory, TimeSpan timeResolution,
            [CanBeNull] LoadTypeInformation lti = null,
            [CanBeNull] PersonInformation pi = null,
            [CanBeNull] string additionalFileIndex = null)
        {
            if (string.IsNullOrWhiteSpace(fileName)) {
                throw new LPGException("Filename was empty");
            }
            if (!HouseholdRegistry.IsHouseholdRegistered(householdKey)) {
                throw new LPGException("Forgotten Household Key: " + householdKey);
            }
            var rfid = rfid1;
            var cleanFileName = AutomationUtili.CleanFileName(fileName);
            string targetDir = GetFullPathForTargetdirectry(targetDirectory);
            if (!Directory.Exists(targetDir)) {
                Directory.CreateDirectory(targetDir);
            }
            var fileInfo = new FileInfo( Path.Combine(targetDir, cleanFileName));
            if (fileInfo.FullName.Length > 250)
            {
                throw new LPGException("The result filename " + fileInfo.FullName + " was longer than 250 characters which is a problem for Windows. Maybe chose a shorter directory name or nest less deep and try again?");
            }
            if (fileInfo.Exists) {
                throw new LPGException("The file " + fileInfo.Name + " already exists. Can't create it again!");
            }
            bool tryagain;
            if (!_allValidIds.Contains((int) rfid)) {
                throw new LPGException("Invalid result file ID");
            }
            do {
                try {
                    if (typeof(T) == typeof(StreamWriter)) {
                        var stream = _getStream.GetWriterStream(fileInfo.FullName);
#pragma warning disable S2930 // "IDisposables" should be disposed
                        var sw = new StreamWriter(stream);
#pragma warning restore S2930 // "IDisposables" should be disposed
                        var ret = (T) (object) sw;
                        var rfe = new ResultFileEntry(description, fileInfo, displayResultFileEntry, sw,
                                null, stream, rfid, _calcObjectName, householdKey.Key, lti, pi, additionalFileIndex,timeResolution);
                            ResultFileList.ResultFiles.Add(rfe.HashKey, rfe);
                        _inputDataLogger.Save(rfe);
                        return ret;
                    }
                    if (typeof(T) == typeof(BinaryWriter)) {
                        var stream = _getStream.GetWriterStream(fileInfo.FullName);
#pragma warning disable S2930 // "IDisposables" should be disposed
                        var bw = new BinaryWriter(stream);
#pragma warning restore S2930 // "IDisposables" should be disposed
                        var ret = (T) (object) bw;
                        var rfe = new ResultFileEntry(description, fileInfo, displayResultFileEntry, null, bw,
                            stream, rfid, _calcObjectName, householdKey.Key, lti, pi, additionalFileIndex, timeResolution);
                        ResultFileList.ResultFiles.Add(rfe.HashKey, rfe);
                        _inputDataLogger.Save(rfe);
                        return ret;
                    }
                    if (typeof(T) == typeof(Stream)) {
                        var stream = _getStream.GetWriterStream(fileInfo.FullName);
                        var ret = (T) (object) stream;
                        var rfe = new ResultFileEntry(description, fileInfo, displayResultFileEntry, null, null,
                            stream, rfid, _calcObjectName, householdKey.Key, lti, pi, additionalFileIndex, timeResolution);
                        ResultFileList.ResultFiles.Add(rfe.HashKey, rfe);
                        _inputDataLogger.Save(rfe);
                        return ret;
                    }
                    if (typeof(T) == typeof(FileStream)) {
                        var stream = _getStream.GetWriterStream(fileInfo.FullName);
                        var ret = (T) (object) stream;
                        var rfe = new ResultFileEntry(description,  fileInfo, displayResultFileEntry, null, null,
                            stream, rfid, _calcObjectName, householdKey.Key, lti, pi, additionalFileIndex, timeResolution);
                        ResultFileList.ResultFiles.Add(rfe.HashKey, rfe);
                        _inputDataLogger.Save(rfe);
                        return ret;
                    }

                    throw new LPGException("Unknown stream type in Makefile<T>");
                }
                catch (IOException ioe) {
                    if (!Config.IsInUnitTesting) {
                        var errormessage = "The file " + fileInfo.FullName +
                                           " could not be opened. The exact error message was:" + Environment.NewLine + ioe.Message +
                                           Environment.NewLine+ Environment.NewLine;
                        errormessage += " Maybe you forgot to close Excel? Press YES to try again!";
                        var dr = MessageWindowHandler.Mw.ShowYesNoMessage(errormessage, "Error opening file!");
                        if (dr == LPGMsgBoxResult.Yes) {
                            tryagain = true;
                        }
                        else {
                            tryagain = false;
                        }
                    }
                    else {
                        tryagain = false;
                    }
                }
            } while (tryagain);
            throw new DataIntegrityException("Couldn't open file \"" + fileInfo.FullName + "\". Aborting.");
        }

        public void CheckIfAllAreRegistered([NotNull] string resultPath)
        {
            var di = new DirectoryInfo(resultPath);
            var fileInfos = di.GetFiles("*.*", SearchOption.AllDirectories);
            var registeredFiles = ResultFileList.ResultFiles.Values.Select(x => (string)x.FullFileName).ToList();
            foreach (var fileInfo in fileInfos)
            {
                if (!registeredFiles.Any(x=> fileInfo.FullName.EndsWith(x, StringComparison.InvariantCultureIgnoreCase)) && !fileInfo.Name.EndsWith("sqlite-wal", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.EndsWith("sqlite-shm", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.EndsWith(".sqlite", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(".config", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(".pdb", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(".manifest", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(".cmd", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(".json", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith(".db3", StringComparison.InvariantCultureIgnoreCase)
                    && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).EndsWith("logfile.txt", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).StartsWith("log.", StringComparison.InvariantCultureIgnoreCase)
                                                                 && !fileInfo.Name.ToLower(CultureInfo.InvariantCulture).StartsWith("logfile.", StringComparison.InvariantCultureIgnoreCase))

                {
                    throw new LPGException("Unregistered file found: " + fileInfo.FullName);
                }
            }
        }

        public void RegisterHousehold([NotNull] HouseholdKey householdKey, [NotNull] string name, HouseholdKeyType type,
                                      [NotNull] string description, [CanBeNull] string houseName, [CanBeNull] string houseDescription)
        {
            HouseholdRegistry.RegisterHousehold(householdKey, name, type,_inputDataLogger, description, houseName,houseDescription);
        }

        public void RegisterGeneralHouse()
        {
            HouseholdRegistry.RegisterHousehold(Constants.GeneralHouseholdKey, "General Information",
                HouseholdKeyType.General, _inputDataLogger,"General", null, null);
        }

        public void Dispose()
        {
            foreach (var entry in ResultFileList.ResultFiles.Values)
            {
                entry.BinaryWriter?.Close();
                entry.StreamWriter?.Close();
                entry.Stream?.Close();
            }

            if (Config.IsInUnitTesting && !HouseholdRegistry.IsHouseholdRegistered(Constants.GeneralHouseholdKey)) {
                return;
            }
            if (ResultFileList.ResultFiles.All(x => x.Value.ResultFileID != ResultFileID.JsonResultFileList))
            {
                RegisterFile(Constants.ResultJsonFileName, "List of all result files with additional information",
                    false, ResultFileID.JsonResultFileList, Constants.GeneralHouseholdKey, TargetDirectory.Root);
            }
        }
    }
}