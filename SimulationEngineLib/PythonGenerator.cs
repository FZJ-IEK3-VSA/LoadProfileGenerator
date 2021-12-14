using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Automation;
using Automation.ResultFiles;
using Database;
using Database.Tables;

namespace SimulationEngineLib
{
    public class PythonGenerator
    {
        private class TemplatePersonEntry {
            public int Age { get; }
            public Gender Gender { get; }
            public string LivingPattern { get; }
            public string TemplateName { get; }
            public string PersonName { get; }

            public string Name { get; }

            public TemplatePersonEntry(int age, Gender gender, string livingPattern, string templateName, string personName, string name)
            {
                Age = age;
                Gender = gender;
                LivingPattern = livingPattern;
                TemplateName = templateName;
                PersonName = personName;
                Name = name;
            }
        }


        [JetBrains.Annotations.NotNull]
        private static List<TemplatePersonEntry> MakeAllTemplatePersonData([JetBrains.Annotations.NotNull] Simulator sim)
        {
            var pds = new List<TemplatePersonEntry>();
            foreach (var item in sim.HouseholdTemplates.Items) {
                for (var index = 0; index < item.Persons.Count; index++) {
                    var person = item.Persons[index];
                    string name = item.Name.Substring(0, item.Name.IndexOf(" ", StringComparison.InvariantCulture)) + "_" + index + "_" + person.Person.Age + person.Person.Gender.ToString().Substring(0,1);
                    var pd = new TemplatePersonEntry(person.Person.Age, (Gender)person.Person.Gender,
                        person.LivingPatternTag?.Name,item.Name, person.Person.Name,name);
                    pds.Add(pd);
                }
            }

            return pds;
        }
            public void MakePythonData([JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] string datafilepath)
            {
                Simulator sim = new Simulator(connectionString);

                StreamWriter sw = new StreamWriter(datafilepath);
                //sw.WriteLine("from dataclasses import dataclass, field");
                //sw.WriteLine("from dataclasses_json import dataclass_json  # type: ignore");
                //sw.WriteLine("from typing import List, Optional, Any, Dict");
                sw.WriteLine("from pylpg.lpgpythonbindings import *");
                //sw.WriteLine("from enum import Enum");
                sw.WriteLine();
                WriteNames(sim.LoadTypes.Items.Select(x => (DBBase)x).ToList(), sw, "LoadTypes");
                WriteNames(sim.HouseTypes.Items.Select(x => (DBBase)x).ToList(), sw, "HouseTypes");
                WriteJsonRefs(sim.ModularHouseholds.Items.Select(x => (DBBase)x).ToList(), sw, "Households");
                WriteJsonRefs(sim.GeographicLocations.Items.Select(x => (DBBase)x).ToList(), sw, "GeographicLocations");
                WriteJsonRefs(sim.TemperatureProfiles.Items.Select(x => (DBBase)x).ToList(), sw, "TemperatureProfiles");
                WriteJsonRefs(sim.TransportationDeviceSets.Items.Select(x => (DBBase)x).ToList(), sw, "TransportationDeviceSets");
                WriteJsonRefs(sim.ChargingStationSets.Items.Select(x => (DBBase)x).ToList(), sw, "ChargingStationSets");
                WriteJsonRefs(sim.TravelRouteSets.Items.Select(x => (DBBase)x).ToList(), sw, "TravelRouteSets");
                WriteJsonRefs(sim.Houses.Items.Select(x => (DBBase)x).ToList(), sw, "Houses");
                WriteJsonRefs(sim.Sites.Items.Select(x => (DBBase)x).ToList(), sw, "Sites");
                WriteJsonRefs(sim.TransportationDeviceCategories.Items.Select(x => (DBBase)x).ToList(), sw, "TransportationDeviceCategories");
            WriteNames(sim.HouseholdTags.Items.Select(x => (DBBase)x).ToList(), sw, "HouseholdTags");
            WriteNames(sim.LivingPatternTags.Items.Select(x => (DBBase)x).ToList(), sw, "LivingPatternTags");
            WriteNames(sim.HouseholdTemplates.Items.Select(x => (DBBase)x).ToList(), sw, "HouseholdTemplates");
            WriteNames(sim.TraitTags.Items.Select(x => (DBBase)x).ToList(), sw, "TraitTags");
            WritePersonDatas(MakeAllTemplatePersonData(sim),sw,"TemplatePersons");
            sw.Close();
            }
            private static void WritePersonDatas([JetBrains.Annotations.NotNull] List<TemplatePersonEntry> items, [JetBrains.Annotations.NotNull] StreamWriter sw, string classname)
            {
                sw.WriteLine();
                sw.WriteLine("# noinspection PyPep8,PyUnusedLocal");
                sw.WriteLine("class " + classname + ":");
                foreach (var item in items)
                {
                    sw.WriteLine("    " + CleanPythonName(item.Name) + ": TemplatePersonEntry = TemplatePersonEntry(Name=\"" + item.Name +
                                 "\",  Age=" + item.Age + ", Gender=Gender."
                                 + item.Gender + ", LivingPattern=\"" + item.LivingPattern + "\", TemplateName=\"" +
                                 item.TemplateName + "\", PersonName=\"" + item.PersonName +  "\")");
                }
                sw.WriteLine();
            }
        private static void WriteJsonRefs([JetBrains.Annotations.NotNull] List<DBBase> items, [JetBrains.Annotations.NotNull] StreamWriter sw, string classname)
            {
                sw.WriteLine();
                sw.WriteLine("# noinspection PyPep8,PyUnusedLocal");
                sw.WriteLine("class " + classname + ":");
                foreach (var item in items)
                {
                    sw.WriteLine("    " + CleanPythonName(item.Name) + ": JsonReference = JsonReference(\"" + item.Name + "\",  StrGuid(\"" + item.Guid + "\"))");
                }
                sw.WriteLine();
            }

            private static void WriteNames([JetBrains.Annotations.NotNull] List<DBBase> items, [JetBrains.Annotations.NotNull] StreamWriter sw, string classname)
            {
                sw.WriteLine();
                sw.WriteLine("# noinspection PyPep8,PyUnusedLocal");
                sw.WriteLine("class " + classname + ":");
                foreach (var item in items)
                {
                    if (item.Name == "None")
                    {
                        continue;
                    }
                    string pythonName = CleanPythonName(item.Name);
                    sw.WriteLine("    " + pythonName + " = \"" + item.Name + "\"");
                }
                sw.WriteLine();
            }

            [JetBrains.Annotations.NotNull]
            private static string CleanPythonName([JetBrains.Annotations.NotNull] string name)
            {
                string s1 = name.Replace(" ",
                        "_")
                    .Replace("(",
                        "")
                    .Replace(")",
                        "")
                    .Replace("+",
                        "").Replace(",", "")
                    .Replace(".", "_")
                    .Replace("/", "_")
                    .Replace(":", "_")
                    .Replace("ü", "ue")
                    .Replace("ö", "oe")
                    .Replace("ä", "ae")
                    .Replace("-", "_");
                while (s1.Contains("__"))
                {
                    s1 = s1.Replace("__", "_");
                }

                while (char.IsDigit(s1[0]) && s1.Length > 0) {
                    s1 = s1.Substring(1);
                }

                while (s1[0] == '_' && s1.Length > 0)
                {
                    s1 = s1.Substring(1);
                }
            if (s1.Length == 0) {
                    throw new LPGException("Completely annihilated name:" + name);
                }
                return s1;
            }

            public void MakePythonBindings([JetBrains.Annotations.NotNull] string bindingfilepath)
            {

                StreamWriter sw = new StreamWriter(bindingfilepath);
                sw.WriteLine("from __future__ import annotations");
                sw.WriteLine("from dataclasses import dataclass, field");
                sw.WriteLine("from dataclasses_json import dataclass_json  # type: ignore");
                sw.WriteLine("from typing import List, Optional, Any, Dict");
                sw.WriteLine("from enum import Enum");

                sw.WriteLine();
                var writtenTypes = new List<string>();
                WriteEnum<LoadTypePriority>(sw, writtenTypes);
                WriteEnum<OutputFileDefault>(sw, writtenTypes);
                WriteEnum<EnergyIntensityType>(sw, writtenTypes);
                WriteEnum<CalcOption>(sw, writtenTypes);
                WriteEnum<HouseDefinitionType>(sw, writtenTypes);
                WriteEnum<Gender>(sw, writtenTypes);
                WriteEnum<HouseholdDataSpecificationType>(sw, writtenTypes);
                WriteEnum<HouseholdKeyType>(sw, writtenTypes);
            var encounteredTypes = new List<string>();
                WriteClass<StrGuid>(sw, encounteredTypes, writtenTypes);
                WriteClass<TransportationPreference>(sw, encounteredTypes, writtenTypes);
                WriteClass<PersonData>(sw, encounteredTypes, writtenTypes);
                WriteClass<JsonReference>(sw, encounteredTypes, writtenTypes);
                WriteClass<TransportationDistanceModifier>(sw, encounteredTypes, writtenTypes);
                WriteClass<JsonCalcSpecification>(sw, encounteredTypes, writtenTypes);
                WriteClass<HouseReference>(sw, encounteredTypes, writtenTypes);
                WriteClass<HouseholdDataPersonSpecification>(sw, encounteredTypes, writtenTypes);
                WriteClass<PersonLivingTag>(sw, encounteredTypes, writtenTypes);
            WriteClass<HouseholdTemplateSpecification>(sw, encounteredTypes, writtenTypes);
                WriteClass<HouseholdNameSpecification>(sw, encounteredTypes, writtenTypes);
                WriteClass<HouseholdData>(sw, encounteredTypes, writtenTypes);
                WriteClass<HouseData>(sw, encounteredTypes, writtenTypes);
                WriteClass<HouseCreationAndCalculationJob>(sw, encounteredTypes, writtenTypes);
                WriteClass<SingleDeviceProfile>(sw, encounteredTypes, writtenTypes);
                WriteClass<TemplatePersonEntry>(sw, encounteredTypes, writtenTypes);
            WriteClass<HouseholdKey>(sw, encounteredTypes, writtenTypes);
            WriteClass<LoadTypeInformation>(sw, encounteredTypes, writtenTypes);
                WriteClass<HouseholdKeyEntry>(sw, encounteredTypes, writtenTypes);
            WriteClass<JsonSumProfile>(sw, encounteredTypes, writtenTypes);
                WriteClass<JsonDeviceProfiles>(sw, encounteredTypes, writtenTypes);
                encounteredTypes.Remove("System.String");
                encounteredTypes.Remove("System.Int32");
                encounteredTypes.Remove("System.Double");
                encounteredTypes.Remove("System.Boolean");
                encounteredTypes.Remove("System.DateTime");
                encounteredTypes.Remove("System.TimeSpan");
            foreach (var encounteredType in encounteredTypes)
                {
                    if (!writtenTypes.Contains(encounteredType))
                    {
                        throw new LPGException("Missing Type:" + encounteredType);
                    }
                }

                sw.Close();
            }


            private static void WriteClass<T>([JetBrains.Annotations.NotNull] StreamWriter sw, List<string> encounteredTypes, [JetBrains.Annotations.NotNull] List<string> writtenTypes)
            {
                sw.WriteLine();
                sw.WriteLine("# noinspection PyPep8Naming, PyUnusedLocal");
                sw.WriteLine("@dataclass_json");
                sw.WriteLine("@dataclass");
                var myclass = typeof(T).Name;
                writtenTypes.Add(typeof(T).FullName);
                sw.WriteLine("class " + myclass + ":");
                var props = typeof(T).GetProperties();
                foreach (var info in props)
                {
                    if (info.CanRead)
                    {
                        MethodInfo getAccessor = info.GetMethod;
                        if (getAccessor?.IsPublic== false)
                        {
                            continue;
                        }
                        if (getAccessor?.IsStatic == true)
                        {
                            continue;
                        }
                    }
                    sw.WriteLine("    " + GetPropLine(info, encounteredTypes, out var parametertype));
                    sw.WriteLine();
                    sw.WriteLine("    def set_" + info.Name + "(self, value: " + parametertype + ") -> " + myclass + ":");
                    sw.WriteLine("        self." + info.Name + " = value");
                    sw.WriteLine("        return self");
                    sw.WriteLine();
                }
            }

            [JetBrains.Annotations.NotNull]
            private static string GetPropLine([JetBrains.Annotations.NotNull] PropertyInfo info, [JetBrains.Annotations.NotNull] List<string> encounteredTypes, [JetBrains.Annotations.NotNull] out string typename)
            {
                string fulltypename = info.PropertyType.FullName;
                if (fulltypename == null)
                {
                    throw new LPGException();
                }
                string shorttypename = info.PropertyType.Name;
                if (info.PropertyType.IsGenericType)
                {
                    var genericfulltypename = info.PropertyType.GenericTypeArguments[0].FullName;
                    var genericshorttypename = info.PropertyType.GenericTypeArguments[0].Name;
                    if (!encounteredTypes.Contains(genericfulltypename))
                    {
                        encounteredTypes.Add(genericfulltypename);
                    }
                    if (fulltypename.StartsWith("System.Nullable`1"))
                    {
                        fulltypename = genericfulltypename;
                        shorttypename = genericshorttypename;
                    }
                }
                else
                {
                    if (!encounteredTypes.Contains(fulltypename))
                    {
                        encounteredTypes.Add(fulltypename);
                    }
                }
                if (fulltypename == null)
                {
                    throw new LPGException();
                }
                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[System.String,"))
                {
                    typename = "List[str]";
                    return info.Name + ": List[str] = field(default_factory=list)";
                }
                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[Automation.CalcOption"))
                {
                    typename = "List[str]";
                    return info.Name + ": List[str] = field(default_factory=list)";
                }
                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[Automation.PersonLivingTag"))
                {
                    typename = "List[PersonLivingTag]";
                    return info.Name + ": List[PersonLivingTag] = field(default_factory=list)";
                }
                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[Automation.PersonData,"))
                {
                    typename = "List[PersonData]";
                    return info.Name + ": List[PersonData] = field(default_factory=list)";
                }
                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[Automation.TransportationPreference"))
                {
                    typename = "List[TransportationPreference]";
                    return info.Name + ": List[TransportationPreference] = field(default_factory=list)";
                }
                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[Automation.JsonReference"))
                {
                    typename = "List[JsonReference]";
                    return info.Name + ": List[JsonReference] = field(default_factory=list)";
                }
                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[Automation.TransportationDistanceModifier, "))
                {
                    typename = "List[TransportationDistanceModifier]";
                    return info.Name + ": Optional[List[TransportationDistanceModifier]] = field(default_factory=list)";
                }
                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[Automation.HouseholdData,"))
                {
                    typename = "List[HouseholdData]";
                    return info.Name + ": List[HouseholdData] = field(default_factory=list)";
                }
                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[System.Double,"))
                {
                    typename = "List[float]";
                    return info.Name + ": List[float] = field(default_factory=list)";
                }

                if (fulltypename.StartsWith("System.Collections.Generic.List`1[[Automation.SingleDeviceProfile,"))
                {
                    typename = "List[SingleDeviceProfile]";
                    return info.Name + ": List[SingleDeviceProfile] = field(default_factory=list)";
                }
                if (fulltypename.StartsWith("System.Collections.Generic.Dictionary`2[[System.String,") && fulltypename.Contains("],[System.String"))
                {
                    typename = "Dict[str, str]";
                    return info.Name + ": Dict[str, str] = field(default_factory=dict)";
                }
            switch (fulltypename)
                {
                    case "Automation.HouseData":
                    case "Automation.JsonCalcSpecification":
                    case "Automation.HouseReference":
                    case "Automation.HouseholdDataPersonSpecification":
                    case "Automation.HouseholdTemplateSpecification":
                    case "Automation.HouseholdNameSpecification":
                    case "Automation.JsonReference":
                    case "Automation.TransportationDistanceModifier":
                    case "Automation.ResultFiles.LoadTypeInformation":
                    case "Automation.ResultFiles.HouseholdKeyEntry":
                    case "Automation.ResultFiles.HouseholdKey":
                    case "Automation.PersonData":
                    case "Automation.TransportationPreference":
                    case "Automation.HouseholdData":
                        typename = shorttypename;
                        return info.Name + ": Optional[" + shorttypename + "] = None";
                    case "Automation.ResultFiles.HouseholdKeyType":
                        typename = shorttypename;
                        return info.Name + ": Optional[str] = \"\"";
                    case "Automation.HouseholdDataSpecificationType":
                    case "Automation.Gender":
                    case "System.String":
                    case "Automation.CalcOption":
                    case "Automation.OutputFileDefault":
                    case "Automation.EnergyIntensityType":
                    case "Automation.LoadTypePriority":
                    case "System.DateTime":
                        typename = "str";
                        return info.Name + ": Optional[str] = \"\"";
                    case "Automation.HouseDefinitionType":
                        typename = "str";
                        return info.Name + ": Optional[str] = HouseDefinitionType." + HouseDefinitionType.HouseData.ToString();
                    case "Automation.StrGuid":
                        typename = "StrGuid";
                        return info.Name + ": Optional[StrGuid] = None";
                    case "System.Double":
                        typename = "float";
                        return info.Name + ": float = 0";
                    case "System.Int32":
                        typename = "int";
                        return info.Name + ": int = 0";
                    case "System.Boolean":
                        typename = "bool";
                        return info.Name + ": bool = False";
                    case "System.TimeSpan":
                        typename = "str";
                        return info.Name + ": str = \"00:01:00\"";

                    //"System.Nullable`1[[Automation.StrGuid, Automation, Version=9.6.0.0, Culture=neutral, PublicKeyToken=null]]'"
            }
                throw new LPGException("unknown type: \n" + fulltypename);
            }

            private static void WriteEnum<T>([JetBrains.Annotations.NotNull] StreamWriter sw, [JetBrains.Annotations.NotNull] List<string> writtenTypes)
            {
                sw.WriteLine();
                var myclass = typeof(T).Name;
                writtenTypes.Add(typeof(T).FullName);
                var enumvals = Enum.GetValues(typeof(T));
                sw.WriteLine("class " + myclass + "(str, Enum):");
                foreach (var val in enumvals)
                {
                    sw.WriteLine("    " + val + " = \"" + val + "\"");
                }
                sw.WriteLine();
            }


        public static void MakeFullPythonBindings([JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] string bindingfilepath, [JetBrains.Annotations.NotNull] string datafilepath)
        {
            PythonGenerator pbg = new PythonGenerator();
            pbg.MakePythonBindings(bindingfilepath);
            pbg.MakePythonData(connectionString, datafilepath);
        }
    }
}
