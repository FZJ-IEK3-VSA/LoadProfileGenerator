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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CalculationEngine;
using Common;
using Common.Enums;
using Database;
using JetBrains.Annotations;

#endregion

namespace CalculationController.Queue {
    // this class sets up the entire environment, starts the entry factory and then the queue runner and the post processing
    public class CalcStarter {
        [NotNull]
        private readonly Simulator _sim;

        public CalcStarter([NotNull] Simulator sim) {
            _sim = sim;
            CalcManager.ExitCalcFunction = false;
        }

        public static bool ContinueRunning { get; private set; } = true;

        public static void CancelRun() {
            ContinueRunning = false;
            CalcManager.StopRunning();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool CheckAndClearDirectory([NotNull] string dstPath, bool preserveLogfile) {
            var di = new DirectoryInfo(dstPath);
            if (di.Exists) {
                try {
                    if (di.GetDirectories().Length > 0 || di.GetFiles().Length > 0) {
                        var dr = LPGMsgBoxResult.Yes;
                        if (!Config.IsInUnitTesting&& !Config.IsInHeadless) {
                            dr =
                                MessageWindowHandler.Mw.ShowYesNoMessage(
                                    "There are already files in :" + Environment.NewLine + dstPath +
                                    Environment.NewLine+ "Delete those? If not, select \"no\" and enter a different path!", "Delete?");
                        }
                        if (dr == LPGMsgBoxResult.Yes) {
                            var dis = di.GetDirectories();
                            foreach (var directoryInfo in dis) {
                                try {
                                    directoryInfo.Delete(true);
                                }
                                catch (Exception e)
                                {
                                    Logger.Exception(e);
                                }
                            }

                            var fis = di.GetFiles();
                            foreach (var fileInfo in fis) {
                                if(preserveLogfile && fileInfo.Name.StartsWith("Log.")) {
                                    continue;
                                }
                                if (preserveLogfile && fileInfo.Name.EndsWith(".db3"))
                                {
                                    continue;
                                }
                                try {
                                    fileInfo.Delete();
                                }
                                catch (Exception e)
                                {
                                    Logger.Exception(e);
                                }
                            }
                        }
                        else {
                            return false;
                        }
                    }
                }
                catch (Exception e) {
                    Logger.Exception(e);
                    MessageWindowHandler.Mw.ShowInfoMessage("An error occured while deleting the existing files:" + e.Message,
                        "Error");
                    return false;
                }
            }
            return true;
        }

        public void Start([NotNull] CalcStartParameterSet csps) {
            if (!csps.ResumeSettlement || csps.CalcTarget.CalcObjectType != CalcObjectType.Settlement) {
                if (!Config.IsInHeadless) {
                    if (!CheckAndClearDirectory(csps.ResultPath, csps.PreserveLogfileWhileClearingFolder)) {
                        csps.ReportFinishFuncForHousehold?.Invoke(false, string.Empty, csps.ResultPath);
                        return;
                    }
                }
            }
            try {
                var cqr = new CalcQueueRunner();
                cqr.Start(csps,  _sim);
            }
            catch (Exception e) {
                Logger.Exception(e);
                CalcQueueRunner.CloseLogfilesAfterError();
                if (!Config.IsInUnitTesting && !Config.IsInHeadless) {
                    csps.ReportCancelFunc?.Invoke();
                    if (e is DataIntegrityException exception)
                    {
                        MessageWindowHandler.Mw.ShowDataIntegrityMessage(exception);
                    }
                    else
                    {
                        MessageWindowHandler.Mw.ShowDebugMessage(e);
                    }
                }
                else {
                    throw;
                }
            }
        }
    }
}