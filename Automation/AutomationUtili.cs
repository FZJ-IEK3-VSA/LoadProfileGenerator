using Automation.ResultFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automation
{
    public static class AutomationUtili
    {
        /// <summary>
        /// Reads a JSON file and tries to parse the specified object from it.
        /// Uses the Newtonsoft library.
        /// </summary>
        /// <typeparam name="T">the type of the object to parse</typeparam>
        /// <param name="filename">the name of the file containing the JSON</param>
        /// <returns>the parsed object</returns>
        /// <exception cref="LPGException">if the parsed object is null</exception>
        public static T ParseJsonFileNewtonsoft<T>(string filename)
        {
            // use a StreamReader to avoid loading large files as a single string
            using var filereader = new StreamReader(filename);
            using var jsonreader = new Newtonsoft.Json.JsonTextReader(filereader);
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var parsedObject = serializer.Deserialize<T>(jsonreader);
            return parsedObject ?? throw new LPGException($"Input file {filename} does not contain valid data.");
        }

        /// <summary>
        /// The global JSON serialization options for parsing house jobs and other objects.
        /// </summary>
        private static readonly JsonSerializerOptions jsonOptions = InitJsonOptions();

        /// <summary>
        /// Initializes the JSON serialization options.
        /// </summary>
        /// <returns>the JSON options</returns>
        private static JsonSerializerOptions InitJsonOptions()
        {
            var options = new JsonSerializerOptions()
            {
                RespectNullableAnnotations = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.Converters.Add(new JsonReferenceConverter());
            return options;
        }

        /// <summary>
        /// Reads a JSON file and tries to parse the specified object from it.
        /// Uses the System.Text.Json functions.
        /// </summary>
        /// <typeparam name="T">the type of the object to parse</typeparam>
        /// <param name="filename">the name of the file containing the JSON</param>
        /// <returns>the parsed object</returns>
        /// <exception cref="LPGException">if the parsed object is null</exception>
        public static T ParseJsonFile<T>(string filename)
        {
            using var filestream = new FileStream(filename, FileMode.Open);
            var parsedObject = JsonSerializer.Deserialize<T>(filestream, jsonOptions);
            return parsedObject ?? throw new LPGException($"Input file {filename} does not contain valid data.");
        }

        /// <summary>
        /// Parses a JSON string and returns the contained data.
        /// </summary>
        /// <typeparam name="T">the type of the object to parse</typeparam>
        /// <param name="json">the JSON string to parse</param>
        /// <returns>the parsed object</returns>
        /// <exception cref="LPGException">if the parsed object is null</exception>
        public static T ParseJsonString<T>(string json)
        {
            var parsedObject = JsonSerializer.Deserialize<T>(json, jsonOptions);
            return parsedObject ?? throw new LPGException($"Input json string does not contain valid data.");
        }

        [JetBrains.Annotations.NotNull]
        public static List<CalcOption> GetOptionList([JetBrains.Annotations.NotNull] params CalcOption[] list)
        {
            List<CalcOption> cos = new List<CalcOption>();
            foreach (var option in list)
            {
                cos.Add(option);
            }
            return cos;
        }

        [JetBrains.Annotations.NotNull]
        public static string CombineName([JetBrains.Annotations.NotNull] this DirectoryInfo di, [JetBrains.Annotations.NotNull] string name)
        {
            return Path.Combine(di.FullName, name);
        }
        [JetBrains.Annotations.NotNull]
        public static string CleanFileName([JetBrains.Annotations.NotNull] string oldname)
        {
            var newname = oldname;
            var forbiddenchars = Path.GetInvalidFileNameChars();
            return forbiddenchars.Aggregate(newname, (current, forbiddenchar) => current.Replace(forbiddenchar, ' '));
        }

        [JetBrains.Annotations.NotNull]
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
            return leftsize.ToString("0.0", CultureInfo.InvariantCulture) + extension;
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
        [JetBrains.Annotations.NotNull]
        public static string GetCurrentMethodAndClass()
        {
            var stackTrace = new StackTrace(true);
            var frames = stackTrace.GetFrames();
            if (frames == null)
            {
                throw new LPGException("frames was null");
            }
            var method = frames[1].GetMethod();

            if (method?.DeclaringType == null)
            {
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

        public static T ParseStringToEnum<T>([JetBrains.Annotations.NotNull] string s, T defaultValue) where T : struct, IConvertible
        {
            var defs = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            var result = defaultValue;
            foreach (var def in defs)
            {
                if (string.Equals(def.ToString(CultureInfo.InvariantCulture), s, StringComparison.OrdinalIgnoreCase))
                {
                    result = def;
                }
            }
            return result;
        }

        public static void WriteLine([JetBrains.Annotations.NotNull] string message)
        {
#pragma warning disable S2228 // Console logging should not be used
            Console.WriteLine(message);
#pragma warning restore S2228 // Console logging should not be used
        }
    }
}