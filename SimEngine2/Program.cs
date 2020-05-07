using System;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using PowerArgs;
using SimulationEngineLib;

namespace SimEngine2
{
    public static class Program
    {
        private static bool _isUnitTest;

        [UsedImplicitly]
        public static bool CatchErrors { get; set; } = true;

        public static bool IsUnitTest
        {
            get => _isUnitTest;
            set => _isUnitTest = value;
        }

        public static void Main([NotNull] [ItemNotNull] string[] args)
        {
            Logger.Get().StartCollectingAllMessages();
            void RunThisOptionProcessing()
            {
                //                Logger.LogToFile = true;
                //              Logger.LogFileIndex = MakeRandomAlphaNumericCodeForLogfile();
                //            Logger.Threshold = Severity.Information;
                RunOptionProcessing(GetConnectionString(), args);
            }

            if (!CatchErrors)
            {
                RunThisOptionProcessing();
                return;
            }

            try
            {
                RunThisOptionProcessing();
            }
            catch (LPGCommandlineException cex)
            {

                Logger.Info("CRITICAL ERROR. Calculation can not continue:");
                Logger.Info(cex.Message);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                try {
                    using var sw = new StreamWriter("errorlog.txt", true);
                    sw.WriteLine("Commandline: ");
                    var cmd = "";
                    foreach (var s in args)
                    {
                        cmd += s + " ";
                    }

                    Logger.Info("Command line:" + cmd);
                    sw.WriteLine(cmd);

                    sw.WriteLine("Error:");
                    sw.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine +
                                 Environment.NewLine);
                }
                catch (Exception ex1)
                {
                    Logger.Exception(ex1);
                }

                if (IsUnitTest)
                {
                    throw;
                }
            }
        }

        public static void RunOptionProcessing([NotNull] string connectionString, [ItemNotNull][NotNull] string[] args)
        {
            CommandProcessor.ConnectionString = connectionString;
            Config.IsInHeadless = true;
            var definition = new CommandLineArgumentsDefinition(typeof(CommandProcessor))
            {
                ExeName = "SimulationEngine.exe"
            };
            if (!CatchErrors)
            {
                try
                {
                    var parsed = Args.ParseAction(definition, args);
                    if (parsed.ActionArgs == null)
                    {
                        throw new LPGException("Invalid args: ");
                    }
                }
                catch (ArgException ex)
                {
                    throw new LPGException("Invalid args: " + ex.Message);
                }
            }

            Args.InvokeAction(definition, args);
        }

       [NotNull]
        private static string GetConnectionString()
        {
            if (!File.Exists("profilegenerator.db3"))
            {
                Logger.Error("The current directory is:" + Directory.GetCurrentDirectory());
                Logger.Error("The profilegenerator.db3 needs to be in the same directory.");
                throw new LPGException("Missing profilegenerator.db3");
            }

            return "Data Source=profilegenerator.db3";
        }
        /*
        [NotNull]
        private static string MakeRandomAlphaNumericCodeForLogfile()
        {
            const string s = "abcdefghijklmnopqrstuvwxyz0123456789";
            var builder = new StringBuilder();
            var random = new Random();
            for (var i = 0; i < 8; i++) {
                var index = random.Next(0, s.Length);
                builder.Append(s[index]);
            }

            return builder.ToString();
        }*/
    }
}
