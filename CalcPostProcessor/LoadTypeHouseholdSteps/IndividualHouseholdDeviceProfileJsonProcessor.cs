using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public class IndividualHouseholdDeviceProfileJsonProcessor : HouseholdLoadTypeStepBase {
        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public IndividualHouseholdDeviceProfileJsonProcessor([NotNull] CalcDataRepository repository,
                                                             [NotNull] IFileFactoryAndTracker fft,
                                                             [NotNull] ICalculationProfiler calculationProfiler) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.JsonDeviceProfilesIndividualHouseholds),
            calculationProfiler,
            "Individual Household Json Device Profiles") =>
            _fft = fft;

        protected override void PerformActualStep([NotNull] IStepParameters parameters)
        {
            HouseholdLoadtypeStepParameters p = (HouseholdLoadtypeStepParameters)parameters;
            if (p.Key.HHKey == Constants.GeneralHouseholdKey) {
                return;
            }
            var dstLoadType = p.LoadType;
            var efc = Repository.ReadEnergyFileColumns(p.Key.HHKey);
            if (!efc.ColumnCountByLoadType.ContainsKey(dstLoadType))
            {
                return;
            }
            //var householdKeys = efc.ColumnEntriesByColumn[dstLoadType].Values.Select(entry => entry.HouseholdKey).Distinct().ToList();
            if (!efc.ColumnCountByLoadType.ContainsKey(p.LoadType))
            {
                return;
            }
            var calcParameters = Repository.CalcParameters;
            var key = p.Key.HHKey;
            var columns = efc.ColumnEntriesByColumn[p.LoadType].Values.Where(entry => entry.HouseholdKey == key).Select(entry => entry.Column)
                .ToList();
            var hhname = "." + key ;
            var taggingSets = Repository.GetDeviceTaggingSets();
            taggingSets = taggingSets.Where(x => x.LoadTypesForThisSet.Any(y => y.Name == p.LoadType.Name)).ToList();
            var jrf = new JsonDeviceProfiles( calcParameters.InternalStepsize,
                calcParameters.OfficialStartTime, dstLoadType.Name, dstLoadType.UnitOfSum,
                dstLoadType.ConvertToLoadTypeInformation());
            //if (columns.Count == 0&& p.Key.KeyType == HouseholdKeyType.Household) {
            //    throw new LPGException("Found a household without a single device: " );
            //}
            string header = "";
            foreach (int i in columns)
            {
                var ce = efc.ColumnEntriesByColumn[dstLoadType][i];
                Dictionary<string, string> tagsBySet = new Dictionary<string, string>();
                foreach (var set in taggingSets) {
                    if (set.TagByDeviceName.ContainsKey(ce.Name)) {
                        tagsBySet.Add(set.Name, set.TagByDeviceName[ce.Name]);
                    }
                }
                SingleDeviceProfile sdp = new SingleDeviceProfile(ce.Name + " - " + ce.LocationName + " - " + ce.DeviceGuid,ce.DeviceGuid.StrVal,
                    tagsBySet, ce.CalcDeviceDto.DeviceType.ToString());
                jrf.DeviceProfiles.Add(sdp);
                header += ce.DeviceGuid + ",";
            }
            int colLength = efc.ColumnEntriesByColumn[dstLoadType].Count;
            var deviceProfilecsv = _fft.MakeFile<StreamWriter>("DeviceProfiles." + dstLoadType.FileName + hhname + ".csv",
                "Device profiles " + dstLoadType.Name + " as CSV file", true,
                ResultFileID.JsonDeviceProfilesCsv, p.Key.HHKey, TargetDirectory.Results,
                calcParameters.InternalStepsize, CalcOption.JsonDeviceProfilesIndividualHouseholds,
                dstLoadType.ConvertToLoadTypeInformation());
            deviceProfilecsv.WriteLine(header);
            foreach (var efr in p.EnergyFileRows)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < colLength; i++) {
                    double val = efr.GetValueForSingleCols(i);
                    sb.Append(val).Append(",");
                }
                deviceProfilecsv.WriteLine(sb.ToString());
            }
            deviceProfilecsv.Flush();
            var deviceProfilejson = _fft.MakeFile<StreamWriter>("DeviceProfiles." + dstLoadType.FileName + hhname + ".json",
                "Summed up energy profile for all devices for " + dstLoadType.Name + " as JSON file", true,
                ResultFileID.JsonDeviceProfiles, p.Key.HHKey, TargetDirectory.Results,
                calcParameters.InternalStepsize, CalcOption.JsonDeviceProfilesIndividualHouseholds,
                dstLoadType.ConvertToLoadTypeInformation());
            deviceProfilejson.WriteLine(JsonConvert.SerializeObject( jrf,Formatting.Indented));
            // sumfile.Write(JsonConvert.SerializeObject(jrf, Formatting.Indented));
            deviceProfilejson.Flush();
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.DetailedDatFiles, CalcOption.DeviceTaggingSets};

    }
}