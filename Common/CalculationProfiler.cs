using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
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
            MainPart = new Dictionary<string, ProgramPart>();
            Current = new Dictionary<string, ProgramPart>();
            var threadname = Thread.CurrentThread.GetNotNullThreadName();
            MainPart.Add(threadname,  new ProgramPart(null, "Main"));
            Current[threadname] = MainPart[threadname];
        }

        [UsedImplicitly]
        [NotNull]
        public Dictionary<string,ProgramPart> MainPart { get; private set; }

        [NotNull]
        private Dictionary<string, ProgramPart> Current { get; set; }

        public void StartPart(string key)
        {
            lock (MainPart) {
                //Logger.Info("Starting "+ key);

                var threadName = Thread.CurrentThread.GetNotNullThreadName();
                if (!Current.ContainsKey(threadName)) {

                    Current.Add(threadName, new ProgramPart(null, "Main"));
                    MainPart.Add(threadName, new ProgramPart(null, "Main"));
                }

                if (Current[threadName].Key == key) {
                    throw new LPGException("The current key is already " + key + ". Copy&Paste error?");
                }

                Logger.Info("Starting " + threadName + ": " + key);
                var newCurrent = new ProgramPart(Current[threadName], key);
                Current[threadName].Children.Add(newCurrent);
                Current[threadName] = newCurrent;
            }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public void StopPart(string key)
        {
            lock (MainPart) {
                var threadname = Thread.CurrentThread.GetNotNullThreadName();
                if (Current[threadname] == null) {
                    throw new LPGException("Current was null");
                }

                Logger.Info("Stopping " + threadname + ": " + key);

                if (Current[threadname].Key != key) {
                    StreamWriter sw = new StreamWriter("DebuggingCalcProfiler.json");
                    WriteJson(sw);
                    sw.Close();
                    throw new LPGException("Mismatched key: \nCurrent active Key: " + Current[threadname].Key +
                                           "\nBut it's trying to stop the following key: " + key);
                }

                if (Current == MainPart) {
                    throw new LPGException("Trying to stop the main");
                }

                Current[threadname].Stop = DateTime.Now;

                Logger.Info("Finished " + key + " after " + Current[threadname].Duration.ToString());

                Current[threadname] = Current[threadname].Parent;
            }
        }

        public void LogToConsole()
        {
            var threadname = Thread.CurrentThread.GetNotNullThreadName();
            if (Current != MainPart) {
                throw new LPGException("Forgot to close: " + threadname + ": " + Current[threadname].Key);
            }

            MainPart[threadname].Stop = DateTime.Now;
            LogOneProgramPartToConsole(MainPart[threadname], 0);
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
            var threadname = Thread.CurrentThread.GetNotNullThreadName();
            MainPart[threadname].Stop = DateTime.Now;
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