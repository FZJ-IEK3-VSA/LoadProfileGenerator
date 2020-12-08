using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using Common;
using Common.Enums;
using Database;
using Database.Helpers;
using Database.Tables;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class AffordanceChecker : BasicChecker {
        [JetBrains.Annotations.NotNull]
        private readonly Dictionary<double, double> _expansionFactorsByDeviation = new Dictionary<double, double>();

        public AffordanceChecker(bool performCleanupChecks) : base("Affordances", performCleanupChecks) {
        }

        private static void CheckAffordanceDesires([JetBrains.Annotations.NotNull] Affordance aff) {
            foreach (var des1 in aff.AffordanceDesires) {
                foreach (var des2 in aff.AffordanceDesires) {
                    if (des1 != des2 && des1.Desire == des2.Desire) {
                        throw new DataIntegrityException(
                            "In the affordance " + aff.Name + " the desire " + des1.Desire +
                            " exists twice. This is not right.", aff);
                    }
                }
                if (des1.SatisfactionValue <= 0 || des1.SatisfactionValue > 1) {
                    throw new DataIntegrityException(
                        "The satisfaction value for the desire " + des1.Desire.Name +
                        " is over 100% or below 0%. Please fix.", aff);
                }
            }
        }

        private static void CheckAffordanceDevices([JetBrains.Annotations.NotNull] Affordance affordance) {
            var hashSet =
                new HashSet<Tuple<IAssignableDevice, decimal, TimeBasedProfile, VLoadType>>();

            foreach (var devtup in affordance.AffordanceDevices) {
                if (Math.Abs(devtup.Probability) < 0.000001) {
                    throw new DataIntegrityException("The probability for the device " + devtup.Device?.Name +
                                                     " is 0. This is not very useful. Please fix." , affordance);
                }
                var key =
                    new Tuple<IAssignableDevice, decimal, TimeBasedProfile, VLoadType>(devtup.Device, devtup.TimeOffset,
                        devtup.TimeProfile, devtup.LoadType);
                if (hashSet.Contains(key)) {
                    throw new DataIntegrityException(
                        "The device " + devtup.Device + " exists twice in the affordance " + affordance +
                        " with the exact same time, load type and profile. Please fix", affordance);
                }
                hashSet.Add(key);

                if (devtup.Device == null) {
                    throw new DataIntegrityException(
                        "While creating the affordance " + affordance.Name + ", it had no device!", affordance);
                }
                if (devtup.LoadType == null && (devtup.Device.AssignableDeviceType == AssignableDeviceType.Device ||
                                                devtup.Device.AssignableDeviceType ==
                                                AssignableDeviceType.DeviceCategory)) {
                    throw new DataIntegrityException(
                        "In the affordance " + affordance.Name + ", there is a real device with no load type!",
                        affordance);
                }
            }
        }

        private static void CheckAffordancesForStandbyDevices([JetBrains.Annotations.NotNull] Affordance aff,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> actions) {
            var standbyDevices = new List<RealDevice>();
            foreach (var standby in aff.AffordanceStandbys) {
                if(standby.Device == null) {
                    throw new DataIntegrityException("Standby device was null");
                }
                standbyDevices.AddRange(standby.Device.GetRealDevices(actions));
            }
            var totalMessage = string.Empty;
            foreach (var device in aff.AffordanceDevices) {
                if (device.Device == null) {
                    throw new DataIntegrityException("Device was null");
                }
                if (device.Device.IsOrContainsStandbyDevice(actions)) {
                    var foundAll = true;
                    var affdevs = device.Device.GetRealDevices(actions);
                    var missingDevices = string.Empty;
                    foreach (var realDevs in affdevs) {
                        if (!standbyDevices.Contains(realDevs)) {
                            foundAll = false;
                            missingDevices += realDevs.Name + ", ";
                        }
                    }
                    if (!foundAll) {
                        missingDevices = missingDevices.Substring(0, missingDevices.Length - 2);
                        Logger.Warning(device.Device.IsOrContainsStandbyDevice(actions).ToString());
                        totalMessage += "The affordance " + aff.Name + " has the device " + device.Device.Name +
                                        ", which is marked as having a standby power use." +
                                        " But the affordance doesn't have that device as a required standby device. Please fix."+ Environment.NewLine+
                                        "If there is already standby set for this, then maybe it's missing some devices. The missing devices are:"+ Environment.NewLine +
                                        missingDevices + Environment.NewLine;
                    }
                }
            }

            if (totalMessage.Length > 0) {
                throw new DataIntegrityException(totalMessage, aff);
            }
        }

        private static void CheckAffordanceStandbys([JetBrains.Annotations.NotNull][ItemNotNull] List<IHouseholdOrTrait> households) {
            foreach (var hh in households) {
                var deviceToAffordanceNames =
                    new Dictionary<IAssignableDevice, string>();

                var availableAffordances = hh.CollectAffordances(true);
                var requiredDevices = new List<IAssignableDevice>();
                foreach (var affordance in availableAffordances) {
                    foreach (var standby in affordance.AffordanceStandbys) {
                        if (standby.Device == null) {
                            throw new DataIntegrityException("Device was null");
                        }
                        if (!deviceToAffordanceNames.ContainsKey(standby.Device)) {
                            deviceToAffordanceNames.Add(standby.Device, string.Empty);
                        }
                        deviceToAffordanceNames[standby.Device] += affordance.Name + ", ";
                        if (!requiredDevices.Contains(standby.Device)) {
                            requiredDevices.Add(standby.Device);
                        }
                    }
                }
                var existingStandby = new List<IAssignableDevice>();
                existingStandby.AddRange(hh.CollectStandbyDevices());

                var missingStandbys = new List<IAssignableDevice>();
                foreach (var device in requiredDevices) {
                    if (!existingStandby.Contains(device)) {
                        missingStandbys.Add(device);
                    }
                }

                var message = string.Empty;
                if (missingStandbys.Count > 0) {
                    var name = "household";
                    if (hh.GetType() == typeof(HouseholdTrait)) {
                        name = "household trait";
                    }
                    message += "The following autonomous devices are required by the affordances in the " + name + " " +
                               hh.Name + ":"+ Environment.NewLine;
                    foreach (var device in missingStandbys) {
                        var affname = deviceToAffordanceNames[device];
                        affname = affname.Substring(0, affname.Length - 2);
                        message += device.Name + " (Required by " + affname + ")"+ Environment.NewLine;
                    }
                }
                if (message.Length > 0) {
                    throw new DataIntegrityException(message, (DBBase) hh);
                }
            }
        }

        private static void CheckAffordanceStandbysOnTraits([JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<HouseholdTrait> traits) {
            var list = new List<IHouseholdOrTrait>();
            foreach (var hh in traits) {
                list.Add(hh);
            }
            CheckAffordanceStandbys(list);
        }

        private static void CheckAges([JetBrains.Annotations.NotNull] Affordance aff) {
            if (aff.MaximumAge < aff.MinimumAge) {
                throw new DataIntegrityException(
                    "The affordance " + aff.Name + " has a lower maximum age than the minimum age. Please fix.", aff);
            }
            if (aff.MaximumAge == 0) {
                throw new DataIntegrityException("The affordance " + aff.Name + " has a maximum age of 0. Please fix.",
                    aff);
            }
            if (aff.MinimumAge < 0) {
                throw new DataIntegrityException("The affordance " + aff.Name + " has a minimum age of 0. Please fix.",
                    aff);
            }
        }

        private void CheckGeneralAffordanceSettings([JetBrains.Annotations.NotNull] Affordance affordance) {
            if (affordance.AffordanceDesires.Count == 0) {
                throw new DataIntegrityException(
                    "The affordance " + affordance.Name +
                    " has no desires and will therefore never be executed. Please either delete the affordance or add some desires.",
                    affordance);
            }
            if (affordance.AffordanceDevices.Count == 0) {
                throw new DataIntegrityException(
                    "The affordance " + affordance.Name +
                    " has no devices and will therefore not be executed. Please either delete the affordance or add some devices.",
                    affordance);
            }
            if (affordance.TimeLimit == null) {
                throw new DataIntegrityException("Affordance " + affordance.Name + " has no time limit!", affordance);
            }
            if (string.IsNullOrEmpty(affordance.AffCategory)) {
                throw new DataIntegrityException(
                    "In the affordance " + affordance.Name +
                    " the affordance category is empty! This is needed for statistical reasons.", affordance);
            }

            if (affordance.Blue == 255 && affordance.Green == 255 && affordance.Red == 255 && PerformCleanupChecks) {
                throw new DataIntegrityException(
                    "The affordance " + affordance.Name +
                    " has white set as color. This is impossible to read, so please fix.", affordance);
            }

        }

        private void CheckOverlappingDeviceProfiles([JetBrains.Annotations.NotNull] Affordance affordance,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> allDeviceActions, Random rnd) {
            //find overlapping device profiles
            var tpes = new List<TimeProfileEntry>();

            foreach (var affdev in affordance.AffordanceDevices) {
                if (affdev.Device == null) {
                    continue;
                }
                switch (affdev.Device.AssignableDeviceType) {
                    case AssignableDeviceType.Device:
                    case AssignableDeviceType.DeviceCategory:
                        tpes.Add(MakeTimeProfileEntryFromDevice(affdev, affordance, rnd));
                        break;
                    case AssignableDeviceType.DeviceAction:
                        var da = (DeviceAction) affdev.Device;
                        tpes.AddRange(MakeTimeProfileEntryFromDeviceAction(da, affordance, affdev,rnd));
                        break;
                    case AssignableDeviceType.DeviceActionGroup:
                        var dag = (DeviceActionGroup) affdev.Device;
                        var das = dag.GetDeviceActions(allDeviceActions);
                        tpes.AddRange(MakeTimeProfileEntryFromDeviceAction(das[0], affordance, affdev,rnd));
                        break;
                    default:
                        throw new LPGException("Forgotten AssignableDeviceType");
                }
            }
            var maxTime = 0;
            foreach (var timeProfileEntry in tpes) {
                var duration = timeProfileEntry.OffSet + timeProfileEntry.NewLength;
                if (duration > maxTime) {
                    maxTime = duration;
                }
            }
            var keys = tpes.Select(x => x.Key).Distinct().ToList();
            var busySignals = new Dictionary<string, BitArray>();
            foreach (var key in keys) {
                busySignals.Add(key, new BitArray(maxTime));
            }
            foreach (var tpe in tpes) {
                SetBusyBits(tpe, busySignals);
            }
        }

        private static void CheckPersonTimeProfiles([JetBrains.Annotations.NotNull] Affordance affordance) {
            if (affordance.PersonProfile == null) {
                throw new DataIntegrityException("Affordance " + affordance.Name + " has no Person time profile!",
                    affordance);
            }
            var dps = affordance.PersonProfile.ObservableDatapoints;
            for (var i = 0; i < dps.Count - 1; i++) {
                if (Math.Abs(dps[i].Value) < Constants.Ebsilon) {
                    throw new DataIntegrityException(
                        "The Person profile called \"" + affordance.PersonProfile.Name + "\" for the affordance " +
                        affordance.Name +
                        " drops to zero. But a Person profile has to keep the Person occupied. There are no gaps allowed in a Person profile.",
                        affordance);
                }
            }
        }

        private static void CheckRequirements([JetBrains.Annotations.NotNull] Affordance aff) {
            foreach (var requirement in aff.RequiredVariables) {
                var list =
                    aff.RequiredVariables.Where(
                        x =>
                            x.Variable == requirement.Variable && x.LocationMode == requirement.LocationMode &&
                            x.Location == requirement.Location).ToList();
                if (list.Count > 1) {
                    throw new DataIntegrityException("The affordance " + aff.Name + " has more than one requirement ");
                }
            }
        }

        private static void CheckValidLoadtypes([JetBrains.Annotations.NotNull] Affordance affordance, [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> devices) {
            foreach (var affordanceDevice in affordance.AffordanceDevices) {
                if (affordanceDevice.Device == null) {
                    continue;
                }
                switch (affordanceDevice.Device.AssignableDeviceType) {
                    case AssignableDeviceType.Device:
                        CheckValidLoadtypesOnDevices(affordanceDevice, affordance);
                        break;
                    case AssignableDeviceType.DeviceCategory:
                        CheckValidLoadTypesForDeviceCategories(devices, affordanceDevice, affordance);
                        break;
                    case AssignableDeviceType.DeviceAction: // dont need to check those
                        break;
                    case AssignableDeviceType.DeviceActionGroup:
                        break;
                    default:
                        throw new LPGException("Forgot an AssignableDeviceType");
                }
            }
        }

        private static void CheckValidLoadTypesForDeviceCategories([JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> devices,
            [JetBrains.Annotations.NotNull] AffordanceDevice affordanceDevice, [JetBrains.Annotations.NotNull] Affordance affordance) {
            var dc = (DeviceCategory) affordanceDevice.Device;
            var catdevices = devices.Where(x => x.DeviceCategory == dc).ToList();
            foreach (var realDevice in catdevices) {
                if (realDevice.Loads.All(x => x.LoadType != affordanceDevice.LoadType)) {
                    if (affordanceDevice.LoadType == null) {
                        throw new DataIntegrityException("No loadtype set");
                    }
                    throw new DataIntegrityException(
                        "The affordance " + affordance.Name + " has the loadtype " + affordanceDevice.LoadType.Name +
                        " set for the device " + realDevice.Name +
                        ". The device has not defined a power for this loadtype." +
                        " The device is in the device category " + dc?.Name +
                        ". This is not allowed. Please fix by removing the load type from the affordance" +
                        " or adding the load type to the affordance", affordance);
                }

                // check if all the loadtypes on the real device are set on the affordance device
                if (realDevice.ForceAllLoadTypesToBeSet) {
                    foreach (var realDeviceLoadType in realDevice.Loads) {
                        var found2 = false;
                        foreach (var affdev2 in affordance.AffordanceDevices) {
                            if (affdev2.Device == dc && realDeviceLoadType.LoadType == affdev2.LoadType) {
                                // affdev2.TimeOffset == affordanceDevice.TimeOffset && // only device and load profile need to be set.
                                found2 = true;
                                break;
                            }
                        }
                        if (!found2) {
                            throw new DataIntegrityException(
                                "The device " + realDevice.Name + " has the loadtype " + realDeviceLoadType.Name +
                                " set, but the affordance " + affordance.Name +
                                " which uses the device via the device category " + dc?.Name +
                                " hasn't set the load. This should not be.", affordance);
                        }
                    }
                }
            }
        }

        private static void CheckValidLoadtypesOnDevices([JetBrains.Annotations.NotNull] AffordanceDevice affordanceDevice, [JetBrains.Annotations.NotNull] Affordance affordance) {
            // check if all the load types on the affordance device are set on the device
            var rd = (RealDevice) affordanceDevice.Device;
            if (rd == null) {
                throw new DataIntegrityException("Device was null");
            }
            var found = false;
            foreach (var realDeviceLoadType in rd.Loads) {
                if (realDeviceLoadType.LoadType == affordanceDevice.LoadType) {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if(affordanceDevice.LoadType == null) {
                    throw new DataIntegrityException("No load type set");
                }
                throw new DataIntegrityException(

                    "The affordance " + affordance.Name + " has the loadtype " + affordanceDevice.LoadType.Name +
                    " set for the device " + rd.Name + " which the device hasn't set. This is not possible.",
                    affordance);
            }
            // check if all the loadtypes on the real device are set on the affordance device
            foreach (var realDeviceLoadType in rd.Loads) {
                if (rd.ForceAllLoadTypesToBeSet) {
                    var found2 = false;
                    foreach (var affdev2 in affordance.AffordanceDevices) {
                        if (affdev2.Device == rd && affdev2.TimeProfile == affordanceDevice.TimeProfile &&
                            affdev2.TimeOffset == affordanceDevice.TimeOffset &&
                            realDeviceLoadType.LoadType == affdev2.LoadType) {
                            found2 = true;
                            break;
                        }
                    }
                    if (!found2) {
                        throw new DataIntegrityException(
                            "The device " + rd.Name + " has the loadtype " + realDeviceLoadType.Name +
                            " set, but the affordance " + affordance.Name +
                            " which uses the device hasn't set the load. This should not be.", affordance);
                    }
                }
            }
        }

        private double GetMaxExpansionFactor(double standardeviation, Random rnd) {
            if (_expansionFactorsByDeviation.ContainsKey(standardeviation)) {
                return _expansionFactorsByDeviation[standardeviation];
            }
            var nr = new NormalRandom(1, 0.1, rnd);
            double maxfactor = 1;
            for (var i = 0; i < 100; i++) {
                var val = nr.NextDouble(1, standardeviation);
                if (val > maxfactor) {
                    maxfactor = val;
                }
            }
            _expansionFactorsByDeviation.Add(standardeviation, maxfactor);
            return maxfactor;
        }

        [JetBrains.Annotations.NotNull]
        private TimeProfileEntry MakeTimeProfileEntryFromDevice([JetBrains.Annotations.NotNull] AffordanceDevice affdev, [JetBrains.Annotations.NotNull] Affordance affordance, Random rnd) {
            var lt = affdev.LoadType;
            if (lt == null)
            {
                throw new DataIntegrityException("LoadType was null");
            }
            var tp = affdev.TimeProfile;
            if(tp == null) {
                throw new DataIntegrityException("Time profile was null");
            }
            var cp = CalcDeviceFactory.GetCalcProfile(tp, new TimeSpan(0, 1, 0));
            var factor = GetMaxExpansionFactor((double) affordance.TimeStandardDeviation, rnd);
            var newlength = cp.GetNewLengthAfterCompressExpand(factor);
            if (affdev.Device == null) {
                throw new DataIntegrityException("Device was null");
            }
            string name = affdev.Device.Name;
            var tpe = new TimeProfileEntry(affdev, newlength, lt, (int) affdev.TimeOffset, affordance, factor,
                name);
            return tpe;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<TimeProfileEntry> MakeTimeProfileEntryFromDeviceAction([JetBrains.Annotations.NotNull] DeviceAction da, [JetBrains.Annotations.NotNull] Affordance affordance,
            [JetBrains.Annotations.NotNull] AffordanceDevice affdev, Random rnd) {
            var tpes = new List<TimeProfileEntry>();
            foreach (var actionProfile in da.Profiles) {
                var lt = actionProfile.VLoadType;
                if(lt == null) {
                    throw new LPGException("load type was null");
                }
                var tp = actionProfile.Timeprofile;
                if (tp == null) {
                    throw new LPGException("Time profile was null");
                }
                var cp = CalcDeviceFactory.GetCalcProfile(tp, new TimeSpan(0, 1, 0));
                var factor = GetMaxExpansionFactor((double) affordance.TimeStandardDeviation, rnd);
                var newlength = cp.GetNewLengthAfterCompressExpand(factor);
                if (da.Device== null)
                {
                    throw new LPGException("Device was null");
                }
                var tpe = new TimeProfileEntry(affdev, newlength, lt,
                    (int) (affdev.TimeOffset + actionProfile.TimeOffset), affordance, factor, da.Device.Name);
                tpes.Add(tpe);
            }
            return tpes;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void RunFoodDesireCheck([JetBrains.Annotations.NotNull] Simulator sim)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            var foodDesires = sim.Desires.Items.Where(x => x.Name.StartsWith("food / ", StringComparison.OrdinalIgnoreCase)).ToList();
            var unhungry = sim.Desires.Items.First(x => x.Name.ToLower(CultureInfo.InvariantCulture).Contains("hungry"));
            //check if the unhungry afforances have proper desires
            var unhungryaffordances = sim.Affordances.Items.Where(x => x.AffordanceDesires.Any(z => z.Desire == unhungry))
                .ToList();
            foreach (Affordance affordance in unhungryaffordances) {
                if (affordance.AffordanceDesires.Count != 2) {
                    throw new DataIntegrityException("Food affordances should have exactly two desires: Unhungry and the food itself.",affordance);
                }
                bool hasFoodDesire = affordance.AffordanceDesires.Any(x => x.Desire.Name.StartsWith("Food"));
                if (!hasFoodDesire)
                {
                    throw new DataIntegrityException("Food affordances should have exactly two desires: Unhungry and the food itself.",affordance);
                }
            }
            //check the food affordances
            foreach (var foodDesire in foodDesires) {
                var affordances = sim.Affordances.Items.Where(x => x.AffordanceDesires.Any(y => y.Desire == foodDesire))
                    .ToList();
                if (affordances.Count != 1) {
                    throw new DataIntegrityException("Not exactly one affordance for the food desire " + foodDesire.Name, foodDesire);
                }
                foreach (Affordance affordance in affordances) {
                    if(affordance.Name == "cook coffee") {
                        continue;
                    }
                    if(affordance.Name.Contains("(maid)")) {
                        continue;
                    }
                    if (!affordance.RequireAllDesires) {
                        throw new DataIntegrityException("Food affordances should require all the desires.",affordance);
                    }
                    if(affordance.AffordanceDesires.All(x => x.Desire != unhungry)) {
                        throw new DataIntegrityException("The food affordances should also satisfy hunger. " +
                                                         "The affordance " + affordance.PrettyName
                                                         + " does not. Please fix.",affordance);
                    }
                    if(affordance.ExecutedVariables.Count == 0) {
                        throw  new DataIntegrityException("The food affordance " + affordance.PrettyName +
                                                          " does not seem to produce any dirty dishes? Please fix.", affordance);
                    }
                    if (affordance.Name == "bake a cake") {
                        continue;
                    }
                    if (affordance.Name == "bake bread")
                    {
                        continue;
                    }
                    if (affordance.Name == "eat icecream from from freezer")
                    {
                        continue;
                    }
                    if (affordance.Name == "make smoothie")
                    {
                        continue;
                    }
                    if (affordance.SubAffordances.Count < 6) {
                        throw new DataIntegrityException("Less than 6 subaffordances for a food affordance. Please fix.",affordance);
                    }
                }
            }
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            RunFoodDesireCheck(sim);
            CheckAffordanceStandbysOnTraits(sim.HouseholdTraits.Items);
            var affordanceNames = new List<string>();
            Random rnd = new Random();
            foreach (var affordance in sim.Affordances.Items) {
                CheckGeneralAffordanceSettings(affordance);
                CheckPersonTimeProfiles(affordance);
                CheckValidLoadtypes(affordance, sim.RealDevices.Items);
                CheckRequirements(affordance);
                CheckAges(affordance);
                CheckAffordanceDesires(affordance);
                CheckAffordancesForStandbyDevices(affordance, sim.DeviceActions.Items);
                if (affordanceNames.Contains(affordance.Name.ToUpperInvariant())) {
                    throw new DataIntegrityException("There are two affordances with the name " + affordance.Name +
                                                     ". This is not allowed.");
                }
                affordanceNames.Add(affordance.Name.ToUpperInvariant());
                CheckAffordanceDevices(affordance);
                CheckOverlappingDeviceProfiles(affordance, sim.DeviceActions.Items,rnd);
            }

            List<Affordance> broken = new List<Affordance>();
            foreach (var affordance in sim.Affordances.Items) {
                if (PerformCleanupChecks && affordance.BodilyActivityLevel == BodilyActivityLevel.Unknown)
                {
                    broken.Add(affordance);
                }
            }

            if (broken.Count > 0) {
                throw new DataIntegrityException("The opened affordances have no bodily activity level set. Please fix.", broken.Take(10).Cast<BasicElement>().ToList());
            }
        }

        private static void SetBusyBits([JetBrains.Annotations.NotNull] TimeProfileEntry tpe, [JetBrains.Annotations.NotNull] Dictionary<string, BitArray> busySignals) {
            for (var i = tpe.OffSet; i < tpe.NewLength + tpe.OffSet; i++) {
                if (busySignals[tpe.Key][i]) {
                    throw new DataIntegrityException("Overlapping time profiles in the affordance " + tpe.Affordance +
                                                     " for the load type " + tpe.LoadType + " at the time step " + i +
                                                     " minutes for the device " + tpe.AffDev +
                                                     " with a time expansion factor of " + tpe.TimeFactor +
                                                     ". To avoid this, either reduce the standard deviation of the affordance or increase the time offset between the device activations. The affordance has a standard deviation of " +
                                                     tpe.Affordance.TimeStandardDeviation, tpe.Affordance);
                }
                busySignals[tpe.Key][i] = true;
            }
        }

        private class TimeProfileEntry {
            public TimeProfileEntry([JetBrains.Annotations.NotNull] AffordanceDevice affDev, int newLength, [JetBrains.Annotations.NotNull] VLoadType loadType, int offSet,
                [JetBrains.Annotations.NotNull] Affordance affordance, double timeFactor, [JetBrains.Annotations.NotNull] string deviceName) {
                AffDev = affDev;
                NewLength = newLength;
                LoadType = loadType;
                OffSet = offSet;
                Affordance = affordance;
                TimeFactor = timeFactor;
                DeviceName = deviceName;
            }

            [JetBrains.Annotations.NotNull]
            public AffordanceDevice AffDev { get; }
            [JetBrains.Annotations.NotNull]
            public Affordance Affordance { get; }
            [JetBrains.Annotations.NotNull]
            private string DeviceName { get; }

            [JetBrains.Annotations.NotNull]
            public string Key => DeviceName + "#" + LoadType.Name;

            [JetBrains.Annotations.NotNull]
            public VLoadType LoadType { get; }
            public int NewLength { get; }
            public int OffSet { get; }

            public double TimeFactor { get; }
        }
    }
}