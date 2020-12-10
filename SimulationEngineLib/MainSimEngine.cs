using System;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using PowerArgs;

namespace SimulationEngineLib
{
    public static class MainSimEngine
    {

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

        public static void RunOptionProcessing([NotNull] string connectionString, [ItemNotNull][NotNull] string[] args, string exename)
        {
            CommandProcessor.ConnectionString = connectionString;
            Config.IsInHeadless = true;
            var definition = new CommandLineArgumentsDefinition(typeof(CommandProcessor))
            {
                ExeName = exename
            };
            //if (!Config.CatchErrors)
            //{
            //    try
            //    {
            //        /* var parsed = Args.ParseAction(definition, args);

            //         // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            //         if (parsed.ActionArgsProperty.Name == null)
            //         {
            //             string s = string.Join(" ", args);
            //             throw new LPGException("Invalid arguments: " + s + ". No action was identified.");
            //         }*/
            //        Args.InvokeAction(definition, args);
            //    }
            //    catch (ArgException ex)
            //    {
            //        throw new LPGException("Invalid arguments entered: " + ex.Message);
            //    }
            //}
            //else {
            if (args.Length == 0) {
                args = new[] {"--help"};
            }
            Args.InvokeAction(definition, args);
            //}
        }

        public static void Run([NotNull] string[] args, string exename)
        {
            Logger.Get().StartCollectingAllMessages();
            void RunThisOptionProcessing()
            {
                //                Logger.LogToFile = true;
                //              Logger.LogFileIndex = MakeRandomAlphaNumericCodeForLogfile();
                //            Logger.Threshold = Severity.Information;
                RunOptionProcessing(GetConnectionString(), args, exename);
            }

            if (!Config.CatchErrors)
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
                try
                {
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

                if (Config.IsInUnitTesting)
                {
                    throw;
                }
            }
        }

        /*
        [JetBrains.Annotations.NotNull]
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
