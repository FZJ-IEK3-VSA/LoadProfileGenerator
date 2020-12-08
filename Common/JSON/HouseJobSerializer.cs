using System;
using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Newtonsoft.Json;

namespace Common.JSON {
    /// <summary>
    /// makes an example json  for the calculation and returns it as string
    /// </summary>
    public static class HouseJobSerializer {
        public static void WriteJsonToFile([JetBrains.Annotations.NotNull] string fullPath, [JetBrains.Annotations.NotNull] HouseCreationAndCalculationJob hj)
        {
            using (StreamWriter sw = new StreamWriter(fullPath)) {
                if (hj.CalcSpec == null) {
                    throw new LPGException("Trying to write a house job without a calc spec");
                }

                if (hj.House == null) {
                    throw new LPGException("Trying to write a house job without a house definition");
                }
                string json = TurnJsonCalcSpecIntoCommentedString(hj);
                sw.Write(json);
                sw.Close();
            }
        }
        [JetBrains.Annotations.NotNull]
        private static string TurnJsonCalcSpecIntoCommentedString([JetBrains.Annotations.NotNull] HouseCreationAndCalculationJob jcs)
        {
            var comments = GetCommentDictionary();
            string rawJson = JsonConvert.SerializeObject(jcs, Formatting.Indented);
            string[] linearr = rawJson.Split('\n');
            List<string> resultJson = new List<string>();

            foreach (var line in linearr) {
                var trimmed = line.Trim().Replace("\"","");
                foreach (KeyValuePair<string, CommentAttribute> comment in comments) {
                    if (trimmed.StartsWith(comment.Key)) {
                        resultJson.Add("\n");
                        resultJson.AddRange(comment.Value.TurnIntoComment(CountSpacesAtBeginning(line)));
                    }
                }
                resultJson.Add(line.Replace("\n","").Replace("\r",""));
            }
            string joined = string.Join("\n", resultJson);
            return joined;
        }

        private static int CountSpacesAtBeginning([JetBrains.Annotations.NotNull] string line)
        {
            int idx = 0;
            while (idx < line.Length && line[idx] == ' ') {
                idx++;
            }
            return idx;
        }

        [JetBrains.Annotations.NotNull]
        private static Dictionary<string, CommentAttribute>  GetCommentDictionary()
        {
            Type myType = typeof(JsonCalcSpecification);
            var properties = myType.GetProperties();
            Dictionary<string, CommentAttribute> comments = new Dictionary<string, CommentAttribute>();
            foreach (var propertyInfo in properties) {
                var attributes = propertyInfo.GetCustomAttributes(true);
                foreach (var attribute in attributes) {
                    if (attribute is CommentAttribute commentattr)
                    {
                        string propertyname = propertyInfo.Name;
                        comments.Add(propertyname, commentattr);
                    }
                }
            }
            return comments;
        }
    }
}