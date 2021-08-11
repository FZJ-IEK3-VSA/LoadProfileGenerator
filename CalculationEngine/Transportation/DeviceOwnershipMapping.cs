using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngine.Transportation
{
    /// <summary>
    /// A class for managing device ownership. Primarily implements a bidirectional 1-to-1 mapping.
    /// </summary>
    /// <typeparam name="TOwner">Type of the owner objects</typeparam>
    /// <typeparam name="TDevice">Type of the device objects</typeparam>
    public class DeviceOwnershipMapping<TOwner, TDevice> where TOwner : class where TDevice : class
    {
        private Dictionary<TOwner, TDevice> OwnerToDevice { get; } = new Dictionary<TOwner, TDevice>();

        private Dictionary<TDevice, TOwner> DeviceToOwner { get; } = new Dictionary<TDevice, TOwner>();

        /// <summary>
        /// Checks whether an owner can use a device, which is the case when he already owns it or when he could own it.
        /// </summary>
        /// <param name="owner">The owner object that might use the device</param>
        /// <param name="device">The device that might be used</param>
        /// <returns>true if the device can be used, else false</returns>
        public bool CanUse(TOwner owner, TDevice device)
        {
            bool couldOwnDevice = !OwnerToDevice.ContainsKey(owner) && !DeviceToOwner.ContainsKey(device);
            return couldOwnDevice || DeviceToOwner[device] == owner;
        }

        /// <summary>
        /// Adds the specified ownership if the owner does not own another device and if the device is not yet owned
        /// by another owner.
        /// </summary>
        /// <param name="owner">The desired owner object</param>
        /// <param name="device">The device to be owned</param>
        /// <returns>true if the ownership was possible and could be added, otherwhise false</returns>
        public bool TrySetOwnership(TOwner owner, TDevice device)
        {
            if (OwnerToDevice.ContainsKey(owner) && OwnerToDevice[owner] != device)
            {
                // owner already owns another device
                return false;
            }
            if (DeviceToOwner.ContainsKey(device) && DeviceToOwner[device] != owner)
            {
                // device is already owned by another owner
                return false;
            }
            OwnerToDevice[owner] = device;
            DeviceToOwner[device] = owner;
            return true;
        }

        /// <summary>
        /// Gets the device owned by the owner, or null if the owner object does not own a device.
        /// </summary>
        /// <param name="owner">The owner object</param>
        /// <returns>The owned device or null, if no device is owned</returns>
        public TDevice? GetDevice(TOwner owner)
        {
            return OwnerToDevice.ContainsKey(owner) ? OwnerToDevice[owner] : null;
        }

        /// <summary>
        /// Gets the owner of a device, or null if the device is currently not owned.
        /// </summary>
        /// <param name="device">The device object</param>
        /// <returns>The owner or null, if the device has no owner</returns>
        public TOwner? GetOwner(TDevice device)
        {
            return DeviceToOwner.ContainsKey(device) ? DeviceToOwner[device] : null;
        }

        /// <summary>
        /// Removes the ownership of the specified owner object, if there is any.
        /// </summary>
        /// <param name="owner">The owner object</param>
        public void RemoveOwnership(TOwner owner)
        {
            if (!OwnerToDevice.ContainsKey(owner))
            {
                return;
            }
            TDevice device = OwnerToDevice[owner];
            RemoveOwnership(owner, device);
        }

        /// <summary>
        /// Removes the ownership of the specified device object, if there is any.
        /// </summary>
        /// <param name="device">The device object</param>
        public void RemoveOwnership(TDevice device)
        {
            if (!DeviceToOwner.ContainsKey(device))
            {
                return;
            }
            TOwner owner = DeviceToOwner[device];
            RemoveOwnership(owner, device);
        }

        /// <summary>
        /// Removes the objects from both dictionaries.
        /// </summary>
        /// <param name="owner">The owner object</param>
        /// <param name="device">The device object</param>
        private void RemoveOwnership(TOwner owner, TDevice device)
        {
            OwnerToDevice.Remove(owner);
            DeviceToOwner.Remove(device);
        }
    }
}
