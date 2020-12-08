using System;
using System.Net;
using System.Reflection;

namespace Common {
    public class ErrorReporter {
        public void Run([JetBrains.Annotations.NotNull] string message, [JetBrains.Annotations.NotNull] string stack)
        {
            if (Config.IsInUnitTesting) {
                return;
            }
            try {
                //var webAddr = "http://147.87.175.17:8000/API/Report";
                const string webAddr = "http://www.loadprofilegenerator.de/API/Report";
                //string result;
                using (var webClient = new WebClient()) {
                    string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
                    string versionmessage = "Version " + version  + Environment.NewLine + message;
                    if (versionmessage.Length > 1000) {
                        versionmessage = versionmessage.Substring(0, 1000);
                    }
                    webClient.QueryString.Add("message", versionmessage);
                    if (stack.Length > 1000) {
                        stack = stack.Substring(0, 1000);
                    }
                    webClient.QueryString.Add("stack", stack);
                    //result =
                    webClient.DownloadString(webAddr);
                }
                //Logger.Info(result);
            }
            catch (Exception ex) {
                if (Config.IsInUnitTesting) {
                    Logger.Exception(ex, false);
                }
            }
        }
    }
}