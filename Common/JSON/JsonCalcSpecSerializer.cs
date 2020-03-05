using System;
using System.Collections.Generic;
using System.IO;
using Automation;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.JSON {
    /// <summary>
    /// makes an example json  for the calculation and returns it as string
    /// </summary>
    public class JsonCalcSpecSerializer {
        public void WriteJsonToFile([NotNull] string fullPath, [NotNull] JsonCalcSpecification jcs)
        {
            using (StreamWriter sw = new StreamWriter(fullPath)) {
                string json = TurnJsonCalcSpecIntoCommentedString(jcs);
                sw.Write(json);
                sw.Close();
            }
        }
        [NotNull]
        private string TurnJsonCalcSpecIntoCommentedString([NotNull] JsonCalcSpecification jcs)
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

        private int CountSpacesAtBeginning([NotNull] string line)
        {
            int idx = 0;
            while (idx < line.Length && line[idx] == ' ') {
                idx++;
            }
            return idx;
        }

        [NotNull]
        private Dictionary<string, CommentAttribute>  GetCommentDictionary()
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