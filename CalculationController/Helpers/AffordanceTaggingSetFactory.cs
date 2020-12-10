using System.Collections.Generic;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using Common.CalcDto;
using Database;
using JetBrains.Annotations;

namespace CalculationController.Helpers
{
    public class AffordanceTaggingSetFactory
    {
        [NotNull]
        private readonly CalcLoadTypeDtoDictionary _ltDict;

        public AffordanceTaggingSetFactory([NotNull] CalcLoadTypeDtoDictionary loadTypeDictionary)
        {
            _ltDict = loadTypeDictionary;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcAffordanceTaggingSetDto> GetAffordanceTaggingSets([NotNull] Simulator sim)
        {
            var calcSets = new List<CalcAffordanceTaggingSetDto>();
            foreach (var affordanceTaggingSet in sim.AffordanceTaggingSets.Items)
            {
                var calcset = new CalcAffordanceTaggingSetDto(affordanceTaggingSet.Name,
                    affordanceTaggingSet.MakeCharts);
                foreach (var entry in affordanceTaggingSet.Entries)
                {
                    if(entry.Affordance == null)
                    {
                        throw new LPGException("Affordance was null");
                    }
                    var affName = CalcAffordanceFactory.FixAffordanceName(entry.Affordance.Name,
                        sim.MyGeneralConfig.CSVCharacter);
                    if (entry.Tag== null)
                    {
                        throw new LPGException("Tag was null");
                    }
                    var tagname = CalcAffordanceFactory.FixAffordanceName(entry.Tag.Name,
                        sim.MyGeneralConfig.CSVCharacter);
                    calcset.AddTag(affName, tagname);

                    foreach (var affordanceSubAffordance in entry.Affordance.SubAffordances)
                    {
                        if (affordanceSubAffordance.SubAffordance == null) {
                            throw new LPGException("Subaffordance was null");
                        }

                        var subname = CalcAffordanceFactory.FixAffordanceName(
                                          affordanceSubAffordance.SubAffordance.Name,
                                          sim.MyGeneralConfig.CSVCharacter) + " (" + affName + ")";
                        if (!calcset.AffordanceToTagDict.ContainsKey(subname)) {
                            calcset.AddTag(subname, tagname);
                        }
                    }
                }

                foreach (var reference in affordanceTaggingSet.TagReferences) {
                    calcset.AddReference(reference.Tag.Name, reference.Gender, reference.MinAge, reference.MaxAge,
                        reference.Percentage);
                }

                foreach (var tag in affordanceTaggingSet.Tags) {
                    calcset.Colors.Add(tag.Name, tag.CarpetPlotColor);
                }

                calcset.AddTag("taking a vacation", "vacation");
                foreach (var affordanceTaggingSetLoadType in affordanceTaggingSet.LoadTypes) {
                    if (affordanceTaggingSetLoadType.LoadType == null)
                    {
                        throw new LPGException("Load type was null");
                    }
                    if (_ltDict.SimulateLoadtype(affordanceTaggingSetLoadType.LoadType)) {
                        calcset.AddLoadType(_ltDict.Ltdtodict[affordanceTaggingSetLoadType.LoadType]);
                    }
                }

                calcSets.Add(calcset);
            }

            return calcSets;
        }
    }
}