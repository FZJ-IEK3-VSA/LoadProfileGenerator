using System.Collections.Generic;
using System.IO;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.JSON {
    public class ActionEachStepInformation
    {
        [UsedImplicitly]
        [NotNull]
        public Dictionary<string, List<int>> ActionsPyPerson { get; set; } = new Dictionary<string, List<int>>();

        public void AddAction([NotNull] string personName,  int actionSerial)
        {
            if (!ActionsPyPerson.ContainsKey(personName)) {
                ActionsPyPerson.Add(personName,new List<int>() );
            }
            ActionsPyPerson[personName].Add(actionSerial);
        }

        [NotNull]
        public static ActionEachStepInformation Read([NotNull] string fullFilePath)
        {
            string json;
            using (var sw = new StreamReader(fullFilePath))
            {
                json = sw.ReadToEnd();
            }
            var val =  JsonConvert.DeserializeObject<ActionEachStepInformation>(json);
            if (val == null) {
                throw new LPGException("Action Each Timestep was null");
            }
            return val;
        }

    }
}