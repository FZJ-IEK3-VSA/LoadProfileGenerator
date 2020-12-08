using System;
using System.Net;
using System.Reflection;
using JetBrains.Annotations;

namespace Common {
    public class UpdateChecker {
        [NotNull]
        public string GetLatestVersion([CanBeNull] out string question)
        {
            question = null;
            try {
                //var webAddr = "http://147.87.175.17:8000/API/Report";
                const string webAddr = "http://www.loadprofilegenerator.de/API/LatestVersion";
                string version;
                string result;
                using (var webClient = new WebClient()) {
                    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "no version";
                    webClient.QueryString.Add("clientversion", version);
                    result = webClient.DownloadString(webAddr);
                }

                if (result.Contains(":")) {
                    string[] arr = result.Split(':');
                    string latestversionstr = arr[1].Replace("\"","").Replace("}","").Replace(Environment.NewLine, "").Replace("\\n","");
                    string[] versarr = latestversionstr.Split('.');
                    if (versarr.Length < 2) {
                        Logger.Warning("Could not parse version string:" + latestversionstr);
                        return "";
                    }
                    bool success1 = int.TryParse(versarr[0], out int majorversion);
                    bool success2 = int.TryParse(versarr[1], out int minorversion);
                    if (!success1 || !success2) {
                        Logger.Warning("Could not interpret version string:" + latestversionstr);
                        return "";
                    }
                    Version myversion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0,0,0);
                    if (myversion.Major < majorversion || ( myversion.Major == majorversion && myversion.Minor < minorversion)) {
                        question = "The latest version is " + latestversionstr + ". You have " + version + ". This means you should probably update. " +
                            " If you have reasons not to update, such as bugs in the new version, please tell me as soon as possible at noah.pflugradt@gmail.com, so that I can fix the issues. Thank you!";
                    }
                    else if(myversion.Major == majorversion && myversion.Minor == minorversion) {
                        Logger.Info("You are using the current version: " + latestversionstr);
                    }
                    if (myversion.Major > majorversion ||
                        (myversion.Major == majorversion && myversion.Minor > minorversion)) {
                        Logger.Info("You are using the a newer version than the official one. You have " + version  + ", official is " + latestversionstr);
                    }
                    return latestversionstr;
                }
                return "";
            }
            catch (Exception ex) {
                Logger.Exception(ex);
            }
            return "";
        }
    }
}