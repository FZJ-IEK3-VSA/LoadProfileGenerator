using Common;
using Database;
using Database.Tables.Houses;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class TransformationDeviceChecker : BasicChecker {
        public TransformationDeviceChecker(bool performCleanupChecks)
            : base("Transformation Devices", performCleanupChecks) {
        }

        protected override void Run([NotNull] Simulator sim) {
            foreach (var device in sim.TransformationDevices.It) {
                foreach (var condition in device.Conditions) {
                    if (condition.ConditionType == TransformationConditionType.StorageContent && condition.Storage == null)
                    {
                        throw new DataIntegrityException("One condition on the transformation device " +
                                                         device.Name +
                                                         " is not set correctly. The storage is missing.");
                    }
                    if (condition.ConditionType == TransformationConditionType.MinMaxValue) {
                        if (condition.ConditionLoadType == null) {
                            throw new DataIntegrityException("One condition on the transformation device " +
                                                             device.Name +
                                                             " is not set correctly. The load type is missing.");
                        }
                    }
                }
                foreach (var loadType in device.LoadTypesOut) {
                    if (loadType.FactorType == TransformationFactorType.Interpolated) {
                        if (device.FactorDatapoints.Count < 2) {
                            throw new DataIntegrityException(
                                "For interpolation at least two data points are needed in the transformation device " +
                                device.Name, device);
                        }
                    }
                }
            }
        }
    }
}