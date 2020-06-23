using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

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
            var jrf = new JsonDeviceProfiles( calcParameters.InternalStepsize,
                calcParameters.OfficialStartTime, dstLoadType.Name, dstLoadType.UnitOfSum, dstLoadType.ConvertToLoadTypeInformation());
            //if (columns.Count == 0&& p.Key.KeyType == HouseholdKeyType.Household) {
            //    throw new LPGException("Found a household without a single device: " );
            //}
            foreach (int i in columns) {
                var ce = efc.ColumnEntriesByColumn[dstLoadType][i];
                SingleDeviceProfile sdp = new SingleDeviceProfile(ce.Name);
                foreach (var efr in p.EnergyFileRows)
                {
                    if (!efr.Timestep.DisplayThisStep)
                    {
                        continue;
                    }
                    sdp.Values.Add(efr.GetValueForSingleCols(i));

                }
                jrf.DeviceProfiles.Add(sdp);
            }

            var sumfile = _fft.MakeFile<FileStream>("DeviceProfiles." + dstLoadType.FileName + hhname + ".json",
                "Summed up energy profile for all devices for " + dstLoadType.Name + " as JSON file", true,
                ResultFileID.JsonDeviceProfiles, p.Key.HHKey, TargetDirectory.Results,
                calcParameters.InternalStepsize, CalcOption.JsonDeviceProfilesIndividualHouseholds,
                dstLoadType.ConvertToLoadTypeInformation());
            Utf8Json.JsonSerializer.Serialize(sumfile, jrf);
// sumfile.Write(JsonConvert.SerializeObject(jrf, Formatting.Indented));
            sumfile.Flush();
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.DetailedDatFiles};

    }
}