using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using Common.JSON;
using Database;

namespace CalculationController.Helpers {
    public class CalcDeviceTaggingSetFactory {
        [JetBrains.Annotations.NotNull] private readonly CalcParameters _calcParameters;

        [JetBrains.Annotations.NotNull] private readonly CalcLoadTypeDtoDictionary _ltDict;

        public CalcDeviceTaggingSetFactory([JetBrains.Annotations.NotNull] CalcParameters calcParameters,
                                           [JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary ltDict)
        {
            _calcParameters = calcParameters;
            _ltDict = ltDict;
        }

        [JetBrains.Annotations.NotNull]
        public CalcDeviceTaggingSets GetDeviceTaggingSets([JetBrains.Annotations.NotNull] Simulator sim, int personCount)
        {
            CalcDeviceTaggingSets cs = new CalcDeviceTaggingSets
            {
                AllCalcDeviceTaggingSets = new List<DeviceTaggingSetInformation>()
            };
            foreach (var deviceTaggingSet in sim.DeviceTaggingSets.Items) {
                var calcset = new DeviceTaggingSetInformation(deviceTaggingSet.Name);
                foreach (var entry in deviceTaggingSet.Entries) {
                    if (entry.Device == null) {
                        throw new LPGException("Device was null");
                    }

                    if (entry.Tag == null) {
                        throw new LPGException("Tag was null");
                    }

                    var devname = entry.Device.Name;
                    //sim.MyGeneralConfig.CSVCharacter);
                    var tagname = CalcAffordanceFactory.FixAffordanceName(entry.Tag.Name,
                        _calcParameters.CSVCharacter);
                    calcset.AddTag(devname, tagname);
                }

                foreach (var reference in
                    deviceTaggingSet.References.Where(x => x.PersonCount == personCount)) {
                    if (reference.Tag == null) {
                        throw new LPGException("Tag was null");
                    }

                    calcset.AddRefValue(reference.Tag.Name, reference.ReferenceValue, reference.LoadType.Name);
                }

                foreach (var loadType in deviceTaggingSet.LoadTypes) {
                    if (loadType.LoadType == null) {
                        throw new LPGException("Loadtype was null");
                    }

                    if (_ltDict.SimulateLoadtype(loadType.LoadType)) {
                        var clt = _ltDict.GetLoadtypeDtoByLoadType(loadType.LoadType);
                        calcset.AddLoadType(clt.ConvertToLoadTypeInformation());
                    }
                }

                cs.AllCalcDeviceTaggingSets.Add(calcset);
            }

            return cs;
        }
    }
}