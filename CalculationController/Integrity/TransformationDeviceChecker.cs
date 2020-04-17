using System.Linq;
using System.Net.Sockets;
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
                    if (condition.Variable == null) {
                        throw new DataIntegrityException("One condition on the transformation device " +
                                                         device.Name +
                                                         " is not set correctly. The variable is missing.");
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

                if (device.LoadTypeIn == null) {
                    throw new DataIntegrityException("The transformation device " + device.Name + " has no load type selected.", device);
                }
                if (device.LoadTypesOut.Count==0)
                {
                    throw new DataIntegrityException("The transformation device " + device.Name + " has no output load types selected.", device);
                }

                foreach (var lt in device.LoadTypesOut) {
                    if (lt.Factor == 0) {
                        throw new DataIntegrityException("Output factor of 0 doesn't make sense in " + device.Name, device);
                    }
                }
                var inputlt = device.LoadTypeIn;
                var outload = device.LoadTypesOut.Where(x => x.VLoadType == inputlt).ToList();
                foreach (var output in outload) {
                    if (!(output.Factor < 0)) {
                        throw new DataIntegrityException("Positive factor in feedback loadtype in device " + device.Name, device);
                    }
                }
            }


            foreach (var storage in sim.EnergyStorages.It) {
                if (storage.LoadType == null) {
                        throw new DataIntegrityException("Energy storage is missing a load type", storage);
                }
            }
        }
    }
}