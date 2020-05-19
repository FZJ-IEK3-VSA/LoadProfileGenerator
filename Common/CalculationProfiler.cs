using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common {
    public interface ICalculationProfiler {
        void StartPart([NotNull] string key);

        void StopPart([NotNull] string key);
    }

    public class CalculationProfiler : ICalculationProfiler {
        public CalculationProfiler()
        {
            MainPart = new ProgramPart(null, "Main");
            Current = MainPart;
        }

        [UsedImplicitly]
        [NotNull]
        public ProgramPart MainPart { get; private set; }

        [CanBeNull]
        private ProgramPart Current { get; set; }

        public void StartPart(string key)
        {
            //Logger.Info("Starting "+ key);
            if (Current == null) {
                throw new LPGException("Current was null");
            }

            if (Current.Key == key) {
                throw new LPGException("The current key is already " + key + ". Copy&Paste error?");
            }

            var newCurrent = new ProgramPart(Current, key);
            Current.Children.Add(newCurrent);
            Current = newCurrent;
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public void StopPart(string key)
        {   if (Current == null) {
                throw new LPGException("Current was null");
            }

            if (Current.Key != key) {
                throw new LPGException("Mismatched key: Current: " + Current.Key + " trying to stop: " + key);
            }

            if (Current == MainPart) {
                throw new LPGException("Trying to stop the main");
            }

            Current.Stop = DateTime.Now;

            Logger.Info("Finished " + key +" after " + Current.Duration.ToString() );

            Current = Current.Parent;
        }

        public void LogToConsole()
        {
            if (Current != MainPart) {
                throw new LPGException("Forgot to close: " + Current?.Key);
            }

            MainPart.Stop = DateTime.Now;
            LogOneProgramPartToConsole(MainPart, 0);
        }

        /*
        public static CalculationProfiler Get()
        {
            if (_myself == null) {
                _myself = new CalculationProfiler();
            }
            return _myself;
        }*/
        /*
        public void Clear()
        {
            MainPart = new ProgramPart(parent: null, key: "Main");
            Current = MainPart;
        }*/

        /*private void LogOneProgramPart(StreamWriter sw, ProgramPart part, int level)
        {
            var padding = "";
            for (var i = 0; i < level; i++) {
                padding += "  ";
            }
            sw.WriteLine(padding + part.Key + ";" + part.Duration.TotalSeconds);
            foreach (var child in part.Children) {
                LogOneProgramPart(sw, child, level + 1);
            }
        }*/

        /*        public void LogToFile(string fullFileName)
                {
                    if(Current!= MainPart) {
                        throw new LPGException("Forgot to close: "+ Current.Key);
                    }
                    MainPart.Stop = DateTime.Now;
                    using (var sw = new StreamWriter(fullFileName)) {
                        LogOneProgramPart(sw, MainPart, 0);
                    }
                }*/

        [NotNull]
        public static CalculationProfiler Read([NotNull] string path)
        {
            var dstPath = Path.Combine(path, Constants.CalculationProfilerJson);
            string json;
            using (var sw = new StreamReader(dstPath)) {
                json = sw.ReadToEnd();
            }

            var o = JsonConvert.DeserializeObject<CalculationProfiler>(json);
            return o;
        }

        public void WriteJson([NotNull] StreamWriter sw)
        {
            MainPart.Stop = DateTime.Now;
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Include
            });
            using (sw) {
                sw.WriteLine(json);
            }
        }

        private static void LogOneProgramPartToConsole([NotNull] ProgramPart part, int level)
        {
            var padding = "";
            for (var i = 0; i < level; i++) {
                padding += "  ";
            }

            Logger.Info(padding + part.Key + "\t" + part.Duration.TotalSeconds);
            foreach (var child in part.Children) {
                LogOneProgramPartToConsole(child, level + 1);
            }
        }

        public class ProgramPart {
            public ProgramPart([CanBeNull] ProgramPart parent, [NotNull] string key)
            {
                Parent = parent;
                Key = key;
                Start = DateTime.Now;
            }

            [NotNull]
            [ItemNotNull]
            public List<ProgramPart> Children { get; } = new List<ProgramPart>();

            public TimeSpan Duration => Stop - Start;

            public double Duration2 { get; set; }

            [UsedImplicitly]
            [NotNull]
            public string Key { get; set; }

            [JsonIgnore]
            [CanBeNull]
            public ProgramPart Parent { get; }

            [UsedImplicitly]
            public DateTime Start { get; set; }

            [UsedImplicitly]
            public DateTime Stop { get; set; }

            [NotNull]
            public override string ToString() => Key + " - " + Duration;
        }
    }
}