using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Automation {
    public static class AutomationUtili {

        [NotNull]
        public static List<CalcOption> GetOptionList([NotNull] params CalcOption[] list)
        {
            List<CalcOption> cos = new List<CalcOption>();
            foreach (var option in list) {
                cos.Add(option);
            }
            return cos;
        }

        [NotNull]
        public static string CombineName([NotNull] this DirectoryInfo di, [NotNull] string name)
        {
            return Path.Combine(di.FullName, name);
        }
        [NotNull]
        public static string CleanFileName([NotNull] string oldname)
        {
            var newname = oldname;
            var forbiddenchars = Path.GetInvalidFileNameChars();
            return forbiddenchars.Aggregate(newname, (current, forbiddenchar) => current.Replace(forbiddenchar, ' '));
        }

        [NotNull]
        public static string MakePrettySize(long size)
        {
            var extension = " B";
            double leftsize = size;
            if (leftsize > 1024)
            {
                leftsize = (leftsize / 1024.0);
                extension = " kB";
            }
            if (leftsize > 1024)
            {
                leftsize = (leftsize / 1024.0);
                extension = " MB";
            }
            if (leftsize > 1024)
            {
                leftsize = (leftsize / 1024.0);
                extension = " GB";
            }
            return leftsize.ToString("0.0",CultureInfo.InvariantCulture) + extension;
        }
        //public static DateTime ConvertToDateTimeWithMessage(string s) {
        //    DateTime value;
        //    var success = DateTime.TryParse(s, out value);
        //    if (!success) {
        //        Logger.Error("Could not convert \"" + s + "\" to a DateTime.");
        //    }
        //    return value;
        //}

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [NotNull]
        public static string GetCurrentMethodAndClass() {
            var stackTrace = new StackTrace(true);
            var frames = stackTrace.GetFrames();
            if (frames == null) {
                throw new LPGException("frames was null");
            }
            var method = frames[1].GetMethod();

            if (method.DeclaringType == null) {
                throw new LPGException("DeclaringType was null");
            }
            return method.DeclaringType.Name + "." + method.Name;
        }
        [NotNull]
        public static string GetCallingMethodAndClass()
        {
            var stackTrace = new StackTrace(true);
            var frames = stackTrace.GetFrames();
            if (frames == null)
            {
                throw new LPGException("frames was null");
            }
            var method = frames[2].GetMethod();

            if (method.DeclaringType == null)
            {
                throw new LPGException("DeclaringType was null");
            }
            return method.DeclaringType.Name + "." + method.Name;
        }

        public static T ParseStringToEnum<T>([NotNull] string s, T defaultValue) where T : struct, IConvertible {
            var defs = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            var result = defaultValue;
            foreach (var def in defs) {
                if (string.Equals(def.ToString(CultureInfo.InvariantCulture), s, StringComparison.OrdinalIgnoreCase)) {
                    result = def;
                }
            }
            return result;
        }

        public static void WriteLine([NotNull] string message) {
#pragma warning disable S2228 // Console logging should not be used
            Console.WriteLine(message);
#pragma warning restore S2228 // Console logging should not be used
        }
    }
}