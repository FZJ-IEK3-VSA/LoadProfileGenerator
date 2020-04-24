using System.Collections.Generic;
using JetBrains.Annotations;

namespace SimulationEngine.Tests.SimZukunftProcessor
{
    public class YearlyProfile {
        public YearlyProfile([NotNull] string name, [NotNull] string srcFile)
        {
            Name = name;
            SrcFile = srcFile;
        }

        [NotNull]
        public string Name { get; }
        [NotNull]
        public string SrcFile { get; }
        [NotNull]
        public List<double> Values { get; } = new List<double>();
    }
    /*
    public class ExternalSettlementDefinitionJsonReaderTests : UnitTestBaseClass
    {
       /* [Test]
        public void MergeDataForAdaptricity()
        {
            const string filename = @"V:\BurgdorfStatistics\firstExport.json";
            string jsonstr;
            using (StreamReader sr = new StreamReader(filename))
            {
                jsonstr = sr.ReadToEnd();
            }

            var exports = JsonConvert.DeserializeObject<List<ExportEntry>>(jsonstr);
            var gebids = exports.Select(x => x.GebäudeObjektIDs).Distinct().ToList();
            DirectoryInfo di = new DirectoryInfo(@"O:\r8archive");
            Logger.Info("Total count:" + gebids.Count);
            var files = di.GetFiles("SumProfiles_900s.Electricity.csv", SearchOption.AllDirectories);
            List< YearlyProfile > profiles = new List<YearlyProfile>();
            for (var i = 0; i < gebids.Count; i++) {
                var file = files[i];
                var parentDir = file.Directory?.Parent ?? throw new NullReferenceException();
                string des = gebids[i].Replace(",","");
                Logger.Info(des);
                Logger.Info(parentDir.FullName);
                YearlyProfile yp = new YearlyProfile(des, file.FullName);
                profiles.Add(yp);
                using (StreamReader sr = new StreamReader(file.FullName)) {
                    sr.ReadLine();
                    while (!sr.EndOfStream) {
                        string s = sr.ReadLine();
                        Debug.Assert(s != null, nameof(s) + " != null");
                        string[] arr = s.Split(';');
                        if (arr.Length > 2) {
                            double val = Convert.ToDouble(arr[2]);
                            yp.Values.Add(val);
                        }
                    }
                }
            }

            {
                using (StreamWriter sw = new StreamWriter(@"d:\settlement.csv")) {
                    int lines = profiles.Max(x => x.Values.Count);
                    string header = "";
                    foreach (YearlyProfile profile in profiles) {
                        header += profile.Name + ";";
                    }

                    sw.WriteLine(header);
                    for (int i = 0; i < lines; i++) {
                        StringBuilder sb = new StringBuilder();
                        foreach (YearlyProfile profile in profiles) {
                            sb.Append((profile.Values[i] * 60000).ToString("F1") + ";");
                        }

                        sw.WriteLine(sb.ToString());
                    }

                    sw.Close();
                }
            }
            {
                using (StreamWriter sw2 = new StreamWriter(@"d:\adaptricity.csv")) {
                    int lines = profiles.Max(x => x.Values.Count);
                    foreach (YearlyProfile profile in profiles)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(profile.Name + ";");
                        for (int i = 0; i < lines; i++) {
                            sb.Append((profile.Values[i] * 60000).ToString("F1") + ";");
                        }
                        sw2.WriteLine(sb.ToString());
                    }

                    sw2.Close();
                }
            }
        }*/
}