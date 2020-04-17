using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.Houses;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class HouseTypeChecker : BasicChecker {
        public HouseTypeChecker(bool performCleanupChecks) : base("House Types", performCleanupChecks) {
        }

        private static void CheckStandbyDevicesHouses([NotNull] HouseType ht) {
            if (ht.AdjustYearlyCooling && ht.ReferenceCoolingHours <10) {
                throw new DataIntegrityException("Reference cooling degree hours in " + ht.Name + " was less than 10, but adjusting the cooling amount was turned on.",ht);
            }
            if (ht.AdjustYearlyEnergy && ht.ReferenceDegreeDays < 10)
            {
                throw new DataIntegrityException("Reference heating degree days in " + ht.Name + " was less than 10, but adjusting the heating amount was turned on.",ht);
            }
            foreach (var hhAutonomousDevice in ht.HouseDevices) {
                if (hhAutonomousDevice.Location == null) {
                    throw new DataIntegrityException(
                        "In the house type " + ht.Name + " the autonomous device " + hhAutonomousDevice.Name +
                        " has no location. Please fix.", ht);
                }
                if (hhAutonomousDevice.TimeLimit == null)
                {
                    throw new DataIntegrityException(
                        "In the house type " + ht.Name + " the autonomous device " + hhAutonomousDevice.Name +
                        " has no time limit. Please fix.", ht);
                }
            }
        }





        private static void CheckTrafos([NotNull] HouseType ht)
        {
            List<Variable> usedVariables = new List<Variable>();
            foreach (var trafo in ht.HouseTransformationDevices) {
                if (trafo.TransformationDevice != null) {
                    foreach (var condition in trafo.TransformationDevice.Conditions) {
                        usedVariables.Add(condition.Variable);
                    }
                }

                if (trafo.TransformationDevice == null) {
                    throw new DataIntegrityException("Messed up Transformation device in " + ht,ht);
                }
                if (trafo.TransformationDevice.LoadTypeIn == null) {
                    throw new DataIntegrityException("No input load type set on " + trafo.TransformationDevice, trafo.TransformationDevice);
                }

                if (trafo.TransformationDevice.LoadTypesOut.Count == 0) {
                    throw new DataIntegrityException("Messed up ");
                }
                
            }
            var offeredVariables = new List<Variable>();
            foreach (var storage in ht.HouseEnergyStorages) {
                if (storage.EnergyStorage == null) {
                    throw new DataIntegrityException("Messed up energy storage in " + ht.Name, ht);
                }
                foreach (var signal in storage.EnergyStorage.Signals)
                {
                    offeredVariables.Add(signal.Variable);
                }

            }

            foreach (var variable in usedVariables)
            {
                if (!offeredVariables.Contains(variable))
                {
                    throw new DataIntegrityException("The variable " + variable.Name + " is used as condition in a transformation device in the house type " + ht.Name + " but no energy storage sets that variable.",ht);
                }
            }

        }
        protected override void Run([NotNull] Simulator sim) {
            foreach (var houseType in sim.HouseTypes.It) {
                CheckStandbyDevicesHouses(houseType);
                CheckTrafos(houseType);
                if (houseType.MinimumHouseholdCount == 0 || houseType.MaximumHouseholdCount == 0) {
                    throw new DataIntegrityException(
                        "Please check the minimum / maximum household count in the house type " + houseType.PrettyName +
                        ". 0 is not allowed.", houseType);
                }
            }

            if (sim.MyGeneralConfig.PerformCleanUpChecksBool) {
                foreach (var houseType in sim.HouseTypes.It) {
                    PerformBalanceChecks(houseType,sim);
                }
            }
        }

        private enum StorageStatus {
            Missing,
            Needed,
            NotNeeded
        }
        private static void PerformBalanceChecks([NotNull] HouseType houseType, [NotNull] Simulator sim)
        {
            var spaceheatinglt = sim.LoadTypes.FindFirstByName("Space Heating");
            if (spaceheatinglt == null) {
                throw new DataIntegrityException("Could not find space heating Loadtype");
            }
            foreach (var transformationDevice in houseType.HouseTransformationDevices) {
                if (transformationDevice.TransformationDevice == null) {
                    throw new LPGException("Messed up data in " + houseType.Name);
                }

                if (transformationDevice.TransformationDevice.LoadTypeIn == spaceheatinglt) {
                    StorageStatus status = StorageStatus.Missing;
                    if (transformationDevice.TransformationDevice.Description.Contains("expects storage")) {
                        status = StorageStatus.Needed;
                    }
                    if (transformationDevice.TransformationDevice.Description.Contains("no storage"))
                    {
                        status = StorageStatus.NotNeeded;
                    }

                    if (status == StorageStatus.Missing) {
                        throw  new DataIntegrityException("The transformation device " + transformationDevice.Name + " does not have it's storage needs in the description.", transformationDevice.TransformationDevice);
                    }

                    bool spaceheatingStorageExists = false;
                    foreach (var storage in houseType.HouseEnergyStorages) {
                        if (storage.EnergyStorage == null) {
                            throw new LPGException("Messed up energy storage in " + houseType.Name);
                        }
                        if (storage.EnergyStorage.LoadType == spaceheatinglt) {
                            spaceheatingStorageExists = true;
                        }
                    }

                    if (status == StorageStatus.Needed && !spaceheatingStorageExists) {
                        throw new DataIntegrityException("The " + houseType.Name + " has a transformation device for space heating that requires an energy storage, but no energy storage is in the house", houseType);
                    }
                    if (status == StorageStatus.NotNeeded && spaceheatingStorageExists)
                    {
                        throw new DataIntegrityException("The " + houseType.Name + " has a transformation device for space heating that requires no energy storage, but an energy storage is in the house",houseType);
                    }
                }
                CheckAllTrafosForEnergyStorageDemands(houseType, transformationDevice);
            }
            }

        private static void CheckAllTrafosForEnergyStorageDemands([NotNull] HouseType houseType, [NotNull] HouseTypeTransformationDevice transformationDevice)
        {
            StorageStatus status = StorageStatus.Missing;
            Variable variable = null;
            if (transformationDevice.TransformationDevice == null)
            {
                throw new LPGException("Messed up data in " + houseType.Name);
            }
            if (transformationDevice.TransformationDevice.Description.Contains("expects storage")) {
                status = StorageStatus.Needed;
                if (transformationDevice.TransformationDevice.Conditions.Count == 0) {
                    throw new DataIntegrityException("Transformation Device requires storage, but has no storage conditions: " +
                                                     transformationDevice.TransformationDevice);
                }

                variable = transformationDevice.TransformationDevice.Conditions[0].Variable;
            }

            if (transformationDevice.TransformationDevice.Description.Contains("no storage")) {
                status = StorageStatus.NotNeeded;
            }

            bool storageExists = false;
            foreach (var storage in houseType.HouseEnergyStorages) {
                if (storage.EnergyStorage == null) {
                    throw new LPGException("Messed up energy storage in " + houseType.Name);
                }

                if (storage.EnergyStorage.Signals.Any(x => x.Variable == variable)) {
                    storageExists = true;
                }
            }

            if (status == StorageStatus.Needed && !storageExists) {
                throw new DataIntegrityException("The " + houseType.Name +
                                                 " has a transformation device that requires an energy storage, but no energy storage is in the house",
                    houseType);
            }

            if (status == StorageStatus.NotNeeded && storageExists) {
                throw new DataIntegrityException("The " + houseType.Name +
                                                 " has a transformation device that requires no energy storage, but an energy storage is in the house",
                    houseType);
            }
        }
    }
}