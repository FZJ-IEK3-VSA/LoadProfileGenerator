using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Automation.ResultFiles;

namespace Common {
    public static class Utili {
        [JetBrains.Annotations.NotNull]
        public static string GetCurrentAssemblyVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown version";
            return version;
        }
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [JetBrains.Annotations.NotNull]
        public static string GetCurrentMethodAndClass() {
            var stackTrace = new StackTrace(true);
            var frames = stackTrace.GetFrames();
            if (frames == null) {
                throw new LPGException("frames was null");
            }
            var method = frames[1].GetMethod();

            if (method?.DeclaringType == null) {
                throw new LPGException("DeclaringType was null");
            }
            return method.DeclaringType.Name + "." + method.Name;
        }
        [JetBrains.Annotations.NotNull]
        public static string GetCallingMethodAndClass()
        {
            var stackTrace = new StackTrace(true);
            var frames = stackTrace.GetFrames();
            if (frames == null)
            {
                throw new LPGException("frames was null");
            }
            var method = frames[2].GetMethod();

            if (method?.DeclaringType == null)
            {
                throw new LPGException("DeclaringType was null");
            }
            return method.DeclaringType.Name + "." + method.Name;
        }

        public static T ParseStringToEnum<T>([JetBrains.Annotations.NotNull] string s, T defaultValue) where T : struct, IConvertible {
            var defs = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            var result = defaultValue;
            foreach (var def in defs) {
                if (string.Equals(def.ToString(CultureInfo.InvariantCulture), s, StringComparison.OrdinalIgnoreCase)) {
                    result = def;
                }
            }
            return result;
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
        public static int ConvertToIntWithMessage([JetBrains.Annotations.NotNull] string s)
        {
            var success = int.TryParse(s, out var value);
            if (!success)
            {
                Logger.Error("Could not convert \"" + s + "\" to a int.");
            }
            return value;
        }
        public static double ConvertToDoubleWithMessage([JetBrains.Annotations.NotNull] string s, [JetBrains.Annotations.NotNull] string message = "")
        {
            var decimalpoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (decimalpoint == "." && s.Contains(","))
            {
                s = s.Replace(",", ".");
            }
            if (decimalpoint == "," && s.Contains("."))
            {
                s = s.Replace(".", ",");
            }
            var success = double.TryParse(s, out double value);
            if (!success)
            {
                Logger.Error("Could not convert \"" + s + "\" to a double. " + message);
            }
            return value;
        }


        public static TimeSpan ConvertToTimeSpanWithMessage([JetBrains.Annotations.NotNull] string s)
        {
            var success = TimeSpan.TryParse(s, out var value);
            if (!success)
            {
                Logger.Error("Could not convert \"" + s + "\" to a TimeSpan.");
            }
            return value;
        }

        public static decimal ConvertToDecimalWithMessage([JetBrains.Annotations.NotNull] string s)
        {
            var success = decimal.TryParse(s, out decimal value);
            if (!success)
            {
                Logger.Error("Could not convert \"" + s + "\" to a decimal.");
            }
            return value;
        }
    }
}