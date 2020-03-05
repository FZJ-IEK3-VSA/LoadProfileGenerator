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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Windows;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common
{
    public static class MessageWindows
    {
        [CanBeNull] private static IWindowWithDialog _mainwindow;

        public static void SetMainWindow([NotNull] IWindowWithDialog mainwindow)
        {
            _mainwindow = mainwindow;
        }

        public static void ShowDataIntegrityMessage([NotNull] DataIntegrityException exception)
        {
            if (exception == null)
            {
                throw new LPGException("ShowDataIntegrityMessage Exception == null");
            }
            var errormessage = "The calculation could not be completed because the following error was found:";
            errormessage += Environment.NewLine + Environment.NewLine + exception.Message;
            ShowMessageBox(errormessage, "Data integrity error!", MessageBoxButton.OK, MessageBoxImage.Error,
                MessageBoxResult.OK);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void ShowDebugMessage([NotNull] Exception exception)
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    ErrorReporter er = new ErrorReporter();
                    er.Run(exception.Message, exception.StackTrace);
                }
                catch (Exception ex2)
                {
                    Logger.Exception(ex2);
                }
            });
            t.Start();
            Logger.Exception(exception);
            if (exception == null)
            {
                throw new LPGException("ShowDebugMessage Exception == null");
            }
            var errormessage = "An error occured. The error message is: " + Environment.NewLine + Environment.NewLine;
            errormessage = errormessage + Environment.NewLine + exception.Message + Environment.NewLine + Environment.NewLine;
            if (exception.InnerException != null)
            {
                errormessage = errormessage + exception.InnerException.Message + Environment.NewLine + Environment.NewLine;
            }
            errormessage = errormessage + exception.StackTrace + Environment.NewLine + Environment.NewLine;
            errormessage = errormessage + "The error was logged to the file errorlog.txt. " +
                           "Please send that file to the author of the program to support further development." + Environment.NewLine + Environment.NewLine;
            try
            {
                using (var sw = new StreamWriter("errorlog.txt", true))
                {
                    sw.WriteLine(errormessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Exception(ex);
            }
            ShowMessageBox(errormessage, "Fatal Error!", MessageBoxButton.OK, MessageBoxImage.Error,
                MessageBoxResult.OK);
        }

        public static void ShowInfoMessage([NotNull] string message, [NotNull] string caption)
        {
            Logger.Info(message);
            ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Information,
                MessageBoxResult.OK);
        }

        private static MessageBoxResult ShowMessageBox([NotNull] string message, [NotNull] string caption, MessageBoxButton button,
            MessageBoxImage image, MessageBoxResult result)
        {
            Logger.Warning(message);
            if (_mainwindow != null)
            {
                return _mainwindow.ShowMessageWindow(message, caption, button, image, result);
            }
            return MessageBox.Show(message, caption, button, image, result);
        }

        public static MessageBoxResult ShowYesNoMessage([NotNull] string message, [NotNull] string caption)
        {
            Logger.Warning(message);
            var result = ShowMessageBox(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.Yes);
            Logger.Info("Answer: " + result);

            return result;
        }
    }
}