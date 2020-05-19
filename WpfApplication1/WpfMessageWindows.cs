using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Windows;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;

namespace LoadProfileGenerator
{
    public interface IWindowWithDialog
    {
        MessageBoxResult ShowMessageWindow([NotNull] string txt, [NotNull] string caption, MessageBoxButton button, MessageBoxImage image,
                                           MessageBoxResult defaultresult);
    }
    public class WpfMsgWindows : IMessageWindow
        {
            [CanBeNull] private readonly IWindowWithDialog _mainwindow;

            public WpfMsgWindows(IWindowWithDialog mainwindow)
            {
                _mainwindow = mainwindow;
            }

            public void ShowDataIntegrityMessage(DataIntegrityException exception)
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
            public void ShowDebugMessage(Exception exception)
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

            public void ShowInfoMessage(string message, string caption)
            {
                Logger.Info(message);
                ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK);
            }

            private MessageBoxResult ShowMessageBox([NotNull] string message, [NotNull] string caption, MessageBoxButton button,
                MessageBoxImage image, MessageBoxResult result)
            {
                Logger.Warning(message);
                if (_mainwindow != null)
                {
                    return _mainwindow.ShowMessageWindow(message, caption, button, image, result);
                }
                return MessageBox.Show(message, caption, button, image, result);
            }

            public LPGMsgBoxResult ShowYesNoMessage(string message, string caption)
            {
                Logger.Warning(message);
                var result = ShowMessageBox(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question,
                    MessageBoxResult.Yes);
                Logger.Info("Answer: " + result);
                if (result == MessageBoxResult.Yes)
                {
                    return LPGMsgBoxResult.Yes;
                }

                return LPGMsgBoxResult.No;
            }
        }
}
