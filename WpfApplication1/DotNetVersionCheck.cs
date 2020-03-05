using Common;
using Microsoft.Win32;

namespace LoadProfileGenerator {
    public static class DotNetVersionCheck {
        public static void CheckForVersion()
        {
            var key = GetReleaseKey();
            if (key < 461808 && key != 0) {
                MessageWindows.ShowInfoMessage(
                    "Your version of the .NET Framework seems to be older than 4.7.2. This will most likely cause all kinds of strange problems. " +
                    "Please install the current version to use the LPG. Your current release key is " + key + " which is less than 461808, which Microsoft defines as .NET Framework 4.7.2.",
                    "Version Error");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static int GetReleaseKey()
        {
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                using (ndpKey.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\")) {
                    if (ndpKey.GetValue("Release") != null) {
                        return (int)ndpKey.GetValue("Release");
                    }
                    Logger.Info("Could not read the .NET Version in the Registry. Probably a permissions issue due to Microsoft changing the permissions on the .NET Framework Setup Key. We are going to assume that the version is probably ok.");
                }
            }
            return 0;
        }
    }
}