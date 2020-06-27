using System;
using System.Threading;
using Automation.ResultFiles;
using Common;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Helpers {
    public static class DatabaseVersionChecker {
        [NotNull] private static string _dstVersion = "9.6.0.a";

        [NotNull]
        public static string DstVersion {
            get => _dstVersion;
            set => _dstVersion = value;
        }

        public static void CheckVersion([NotNull] string connectionString) {
            string fullVersion;
            using (var con = new Connection(connectionString)) {
                con.Open();
                using (var cmd = new Command(con)) {
                    using (var dr = cmd.GetTableReader("tblLPGVersion")) {
                        dr.Read();
                        var main = dr.GetIntFromLong("Main");
                        var sub = dr.GetIntFromLong("Sub");
                        var subsub = dr.GetIntFromLong("SubSub");
                        var index = dr.GetString("Index");
                        fullVersion = main + "." + sub + "." + subsub + "." + index;
                        Logger.Info("Successfully read database version: " + fullVersion + ". This is compatible. Program version is " + Config.LPGVersion);
                    }
                }
            }
            Thread.Sleep(500);
            if (!string.Equals(fullVersion.Trim(), _dstVersion.Trim(), StringComparison.OrdinalIgnoreCase)) {
                throw new LPGException("The database version is wrong! Looking for: \"" + _dstVersion +
                                       "\", but found \"" + fullVersion + "\"");
            }
        }
    }
}