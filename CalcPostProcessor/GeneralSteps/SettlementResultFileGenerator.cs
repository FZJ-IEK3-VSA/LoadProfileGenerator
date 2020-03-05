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
/*
namespace CalcPostProcessor.GeneralProcessingSteps {
    /*
    public class SettlementResultFileGenerator {
        private readonly CalcParameters _calcParameters;

        public SettlementResultFileGenerator(CalcParameters calcParameters)
        {
            _calcParameters = calcParameters;
        }

        private readonly List<CalculationResult> _calculationResults;

        [CanBeNull] private readonly Dispatcher _dispatcher;

        private readonly FileFactoryAndTracker _fft;
        private readonly LoadTypeInformation[] _loadTypes;
        private readonly ObservableCollection<ResultFileEntry> _settlementResults;

        public SettlementResultFileGenerator(List<CalculationResult> cr, FileFactoryAndTracker fft,
            ObservableCollection<ResultFileEntry> settlementResults, LoadTypeInformation[] loadTypes,
            [CanBeNull] Dispatcher dispatcher) {
            _calculationResults = cr;
            _fft = fft;
            _loadTypes = loadTypes;
            _settlementResults = settlementResults;
            _dispatcher = dispatcher;
        }

        private void CombineTotals() {
            Logger.Info("Starting to process the total results");

            var swTotal = _fft.MakeFile<StreamWriter>("Totals.csv", "The sum over all households", true,
                ResultFileID.SettlementTotal, Constants.GeneralHouseholdKey, TargetDirectory.Results, _calcParameters.InternalStepsize);
            var headerwritten = false;
            foreach (var calculationResult in _calculationResults) {
                foreach (var resultFileEntry in calculationResult.ResultFileEntries) {
                    if (resultFileEntry.ResultFileID == ResultFileID.Totals) {
                        using (var sr = new StreamReader(resultFileEntry.FullFileName)) {
                            var oldheader = sr.ReadLine();
                            if (!headerwritten) {
                                oldheader = "Household" + _calcParameters.CSVCharacter + oldheader;
                                swTotal.WriteLine(oldheader);
                                headerwritten = true;
                            }
                            while (!sr.EndOfStream) {
                                var s = sr.ReadLine();
                                s = resultFileEntry.HouseholdName + _calcParameters.CSVCharacter + s;
                                swTotal.WriteLine(s);
                            }
                        }
                    }
                }
            }
            swTotal.Close();
            Logger.Info("Processed the total results");
            SaveAdd(_settlementResults, _fft.GetResultFileEntry(ResultFileID.SettlementTotal, null, Constants.GeneralHouseholdKey,null,null));
        }
#pragma warning disable S2930 // "IDisposables" should be disposed
        [CanBeNull]
        private List<BinaryReader> FilterRelevantEntriesAndGetBinaryReaders(LoadTypeInformation loadType,
            List<FileStream> filestreams, ref string headerstring) {
            var allRelevantEnergyFiles = new List<BinaryReader>();
// for each household
            foreach (var calculationResult in _calculationResults) {
                if (calculationResult != null) {
                    BinaryReader br = null;

                    foreach (var resultFileEntry in calculationResult.HiddenResultFileEntries) {
                        if (resultFileEntry.ResultFileID == ResultFileID.OnlineSumActivationFiles &&
                            resultFileEntry.LoadTypeInformation?.ID == loadType.ID) {
                            var fs = new FileStream(resultFileEntry.FullFileName, FileMode.Open);
                            filestreams.Add(fs);
                            br = new BinaryReader(fs, Encoding.ASCII);
                        }
                    }
                    if (br == null) {
                        foreach (var resultFileEntry in calculationResult.HiddenResultFileEntries) {
                            if (resultFileEntry.ResultFileID ==
                                ResultFileID.OnlineDeviceActivationFiles &&
                                resultFileEntry.LoadTypeInformation?.ID == loadType.ID) {
                                var fs = new FileStream(resultFileEntry.FullFileName, FileMode.Open);
                                filestreams.Add(fs);
                                br = new BinaryReader(fs, Encoding.ASCII);
                            }
                        }
                    }
                    if (br == null) {
                        Logger.Error("No files to process for " + loadType.Name);
                        return null;
                    }
                    // save this file and update the header
                    allRelevantEnergyFiles.Add(br);
                    headerstring += calculationResult.CalcObjectName + " [" + loadType.UnitOfSum + "]" +
                                    _calcParameters.CSVCharacter;
                }
            }
            return allRelevantEnergyFiles;
        }
#pragma warning restore S2930 // "IDisposables" should be disposed

        private StreamWriter OpenFilesAndWriteHeaders(LoadTypeInformation loadType, string headerstring, int externalfactor,
            out StreamWriter swTotal, [CanBeNull] out StreamWriter swExternalIndividual,
            [CanBeNull] out StreamWriter swExternalTotal) {
// open files and write headers
            var sw = _fft.MakeFile<StreamWriter>("Sum.Profiles." + loadType.Name + ".csv",
                "Sums of the profile of each household individually for " + loadType.Name, true,
                ResultFileID.SettlementIndividualSumProfile, Constants.GeneralHouseholdKey, TargetDirectory.Results, _calcParameters.InternalStepsize,
                loadType);
            sw.WriteLine(headerstring);
            swTotal = _fft.MakeFile<StreamWriter>("TotalProfile." + loadType.FileName + ".csv",
                "The summed up profile of all households for " + loadType.Name, true,
                ResultFileID.SettlementTotalProfile, Constants.GeneralHouseholdKey, TargetDirectory.Results, _calcParameters.InternalStepsize,
                loadType );
            var totalHeader = "Timestep" + _calcParameters.CSVCharacter + "Time" +
                              _calcParameters.CSVCharacter;
            if (_calcParameters.WriteExcelColumn) {
                totalHeader += "Excel-Time" + _calcParameters.CSVCharacter;
            }
            totalHeader += "Total";
            swTotal.WriteLine(totalHeader);
            swExternalIndividual = null;
            swExternalTotal = null;
            if (externalfactor != 1) {
                var externalFileName =
                    _calcParameters.ExternalStepsize.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                swExternalIndividual =
                    _fft.MakeFile<StreamWriter>("Sum.Profiles." + loadType.Name + "." + externalFileName + "s.csv",
                        "Sums of the profile of each household individually for " + loadType.Name +
                        " for the external time resolution", true, ResultFileID.SettlementIndividualSumProfileExternal,
                        Constants.GeneralHouseholdKey, TargetDirectory.Results, _calcParameters.InternalStepsize, loadType);
                swExternalTotal =
                    _fft.MakeFile<StreamWriter>("TotalProfile." + loadType.FileName + "." + externalFileName + "s.csv",
                        "The summed up profile of all households for " + loadType.Name +
                        " for the external time resolution", true, ResultFileID.SettlementTotalProfileExternal, Constants.GeneralHouseholdKey,
                        TargetDirectory.Results, _calcParameters.InternalStepsize, loadType);
                swExternalIndividual.WriteLine(headerstring);
                swExternalTotal.WriteLine(totalHeader);
            }
            return sw;
        }


        private void ProcessOneLoadType(LoadTypeInformation loadType) {
            Logger.Info("Starting to process the settlement results for " + loadType.Name);
            var reachedLastRow = false;
            var externalfactor =
                (int)
                (_calcParameters.ExternalStepsize.TotalSeconds /
                 _calcParameters.InternalStepsize.TotalSeconds);

            var headerstring = "Timestep" + _calcParameters.CSVCharacter + "Time" +
                               _calcParameters.CSVCharacter;
            if (_calcParameters.WriteExcelColumn) {
                headerstring += "Excel-Time" + _calcParameters.CSVCharacter;
            }
            var filestreams = new List<FileStream>();
            try {
                Logger.Info("Processing " + loadType.Name);
                var allRelevantEnergyFiles = FilterRelevantEntriesAndGetBinaryReaders(loadType,
                    filestreams, ref headerstring);
                if (allRelevantEnergyFiles == null) {
                    return;
                }
                Logger.Info("Found " + allRelevantEnergyFiles.Count + " files to process for " + loadType.Name);
                if (allRelevantEnergyFiles.Count == 0) {
                    return;
                }
                StreamWriter swTotal;
                StreamWriter swExternalIndividual;
                StreamWriter swExternalTotal;
                var sw = OpenFilesAndWriteHeaders(loadType, headerstring, externalfactor, out swTotal,
                    out swExternalIndividual, out swExternalTotal);
                var row = 0;
                var householdValues = new double[allRelevantEnergyFiles.Count];
                DateStampCreator dsc = new DateStampCreator(_calcParameters);
                while (!reachedLastRow) {
                    // deal with the individual house profiles
                    var sb = new StringBuilder();
                    dsc.GenerateDateStampForTimestep(row, sb, false);
                    double total = 0;
                    // read one line from all files

                    for (var index = 0; index < allRelevantEnergyFiles.Count && !reachedLastRow; index++) {
                        var entry = allRelevantEnergyFiles[index];
                        if (entry.BaseStream.Position >= entry.BaseStream.Length) {
                            reachedLastRow = true;
                        }
                        else {
                            var result = entry.ReadDouble();
                            var rowsum = result * loadType.ConversionFaktor;
                            total += rowsum;
                            sb.Append(rowsum);
                            sb.Append(_calcParameters.CSVCharacter);
                            householdValues[index] += rowsum;
                        }
                    }
                    // write the individual household line
                    sw.WriteLine(sb.ToString());
                    // handle the total
                    var sbtotal = new StringBuilder();
                    dsc.GenerateDateStampForTimestep(row, sbtotal, false);

                    sbtotal.Append(total);
                    swTotal.WriteLine(sbtotal);
                    // write the external time resolution
                    if (externalfactor != 1 && row % externalfactor == 0 && swExternalTotal != null) {
                        var externalRow = new StringBuilder();
                        dsc.GenerateDateStampForTimestep(row, externalRow, false);

                        double externaltotal = 0;
                        for (var i = 0; i < householdValues.Length; i++) {
                            externalRow.Append(householdValues[i]);
                            externalRow.Append(_calcParameters.CSVCharacter);
                            externaltotal += householdValues[i];
                            householdValues[i] = 0;
                        }
                        if (swExternalIndividual == null) {
                            throw new LPGException("File was null.");
                        }
                        swExternalIndividual.WriteLine(externalRow);
                        var externalTotalBuilder = new StringBuilder();
                        dsc.GenerateDateStampForTimestep(row, externalTotalBuilder, false);
                        externalTotalBuilder.Append(externaltotal);
                        externalTotalBuilder.Append(_calcParameters.CSVCharacter);
                        swExternalTotal.WriteLine(externalTotalBuilder);
                        for (var i = 0; i < householdValues.Length; i++) {
                            householdValues[i] = 0;
                        }
                    }
                    row++;
                    if (row % 10000 == 0) {
                        Logger.Info("Processed " + row + " lines.");
                    }
                }
                Logger.Info("Processed a total of " + row + " lines.");
                sw.Flush();
                sw.Close();
                swTotal.Flush();
                swTotal.Close();
                SaveAdd(_settlementResults,
                    _fft.GetResultFileEntry(ResultFileID.SettlementIndividualSumProfile, loadType.Name, Constants.GeneralHouseholdKey,null,null));
                SaveAdd(_settlementResults, _fft.GetResultFileEntry(ResultFileID.SettlementTotalProfile, loadType.Name, Constants.GeneralHouseholdKey,null,null));
                if (externalfactor != 1 && swExternalTotal != null) {
                    if (swExternalIndividual == null) {
                        throw new LPGException("File was null.");
                    }
                    swExternalIndividual.Flush();
                    swExternalIndividual.Close();
                    SaveAdd(_settlementResults,
                        _fft.GetResultFileEntry(ResultFileID.SettlementIndividualSumProfileExternal, loadType.Name, Constants.GeneralHouseholdKey,null,null));
                    swExternalTotal.Flush();
                    swExternalTotal.Close();
                    SaveAdd(_settlementResults,
                        _fft.GetResultFileEntry(ResultFileID.SettlementTotalProfileExternal, loadType.Name, Constants.GeneralHouseholdKey,null,null));
                }
            }
            finally {
                foreach (var fileStream in filestreams) {
                    fileStream.Close();
                }
            }
        }

        public void Run() {
            Logger.Info("Starting result processing...");
            foreach (var calcLoadType in _loadTypes) {
                ProcessOneLoadType(calcLoadType);
            }
            CombineTotals();
            Logger.Info("Finished result processing.");
        }

        private void SaveAdd(ObservableCollection<ResultFileEntry> col, ResultFileEntry rfe) {
            Action a = () => {
                col.Add(rfe);
                col.Sort();
            };
            if (_dispatcher != null && Thread.CurrentThread != _dispatcher.Thread) {
                _dispatcher.BeginInvoke(DispatcherPriority.Normal, a);
            }
            else {
                a();
            }
        }
    }
}*/