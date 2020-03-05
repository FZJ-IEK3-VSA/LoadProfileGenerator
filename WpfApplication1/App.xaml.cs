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
//  ‚ÄúThis product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.‚Äù
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
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;

#endregion

namespace LoadProfileGenerator {
    /// <summary>
    ///     Interaktionslogik f¸r "App.xaml"
    /// </summary>
    public partial class App {
        private readonly DateTime _starttime = DateTime.Now;

        [CanBeNull] private SplashWindow _splashWindow;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
        protected override void OnStartup([NotNull] StartupEventArgs e)
        {
            // hook on error before app really starts
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            base.OnStartup(e);
            /*try {
                var ad = ApplicationDeployment.CurrentDeployment;
                var info = ad.CheckForDetailedUpdate();
                if (info.UpdateAvailable) {
                    ad.Update();
                    Application.Restart();
                }
            }
            catch (Exception ex) {
                Logger.Info("Error:" + ex.Message);
                //Logger.Exception(ex);
            }*/

            // Splash-Fenster ˆffnen
            _splashWindow = new SplashWindow();
            _splashWindow.Show();
            SetSplashMessage("Start..");
            // Delegat f¸r die asynchrone Ausf¸hrung der Initialisierung
            var continueStart = true;
            Action initializer = () => {
                try {
                    SetSplashMessage("Loading data....");
                    Config.InitConfig(AppDomain.CurrentDomain.BaseDirectory);
                    SetSplashMessage("Finished loading....");
                }
                catch (Exception ex) {
                    var message = "Error initialising." + Environment.NewLine;
                    message += ex.GetType() + Environment.NewLine;
                    message += Environment.NewLine + "The exact error message is:" + Environment.NewLine;
                    message += ex.Message;
                    if (ex.InnerException != null) {
                        message += Environment.NewLine + ex.InnerException.Message;
                    }

                    message = message + ex.StackTrace + Environment.NewLine + Environment.NewLine;
                    MessageBox.Show(message, "Splash-Window", MessageBoxButton.OK, MessageBoxImage.Error);
                    continueStart = false;
                    Logger.Exception(ex);
                }
            };

            // Delegat f¸r den Callback der asynchronen Ausf¸hrung
            void SplashCloser(IAsyncResult result)
            {
                if (continueStart) {
                    SetSplashMessage("Opening start window....");
                    // Das Hauptfenster erzeugen, der Anwendung zuweisen
                    // und ˆffnen
                    if(Dispatcher!= null) {
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                            var mainWindow = new Shell();
                            MainWindow = mainWindow;
                            mainWindow.Show();
                        }));
                    }

                    // Schlieﬂen des Splash-Fensters
                }

                if (_splashWindow == null) {
                    throw new LPGException("Splash window was null");
                }

                _splashWindow.Dispatcher?.Invoke(DispatcherPriority.Normal, new Action(_splashWindow.Close));
                SetSplashMessage("Opened start window....");
            }

            // Asynchrones Starten der Initialisierung
            initializer.BeginInvoke(SplashCloser, null);
        }

        private static void CurrentDomain_UnhandledException([NotNull] object sender, [NotNull] UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception) e.ExceptionObject;
            Thread t = new Thread(() => {
                try {
                    ErrorReporter er = new ErrorReporter();
                    er.Run(ex.Message, e.ExceptionObject.ToString());
                }
                catch (Exception ex2) {
                    Logger.Exception(ex2);
                }
            });
            t.Start();

            Logger.Exception(ex);
            MessageBox.Show(e.ExceptionObject.ToString());
        }

        private void SetSplashMessage([NotNull] string s)
        {
            var ts = DateTime.Now - _starttime;
            var tsstr = ts.Seconds + "." + ts.Milliseconds;
            if (_splashWindow == null) {
                throw new LPGException("Splash window was null");
            }

            _splashWindow.Dispatcher?.Invoke(DispatcherPriority.Normal,
                new Action(() => _splashWindow.SetInfo(tsstr + ": " + s)));
        }
    }
}