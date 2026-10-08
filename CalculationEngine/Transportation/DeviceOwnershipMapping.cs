using System.Collections.Generic;

namespace CalculationEngine.Transportation
{
    /// <summary>
    /// A class for managing device ownership. Primarily implements a bidirectional 1-to-1 mapping.
    /// </summary>
    /// <typeparam name="TOwner">Type of the owner objects</typeparam>
    /// <typeparam name="TDevice">Type of the device objects</typeparam>
    public class DeviceOwnershipMapping<TOwner, TDevice> where TOwner : class where TDevice : class
    {
        private Dictionary<TOwner, TDevice> OwnerToDevice { get; } = [];

        private Dictionary<TDevice, TOwner> DeviceToOwner { get; } = [];

        /// <summary>
        /// Checks whether an owner can use a device, which is the case when he already owns it or when he could own it.
        /// </summary>
        /// <param name="owner">The owner object that might use the device</param>
        /// <param name="device">The device that might be used</param>
        /// <returns>true if the device can be used, else false</returns>
        public bool CanUse(TOwner owner, TDevice device)
        {
            bool couldOwnDevice = !OwnerToDevice.ContainsKey(owner) && !DeviceToOwner.ContainsKey(device);
            bool isAlreadyOwner = DeviceToOwner.ContainsKey(device) && DeviceToOwner[device] == owner;
            return couldOwnDevice || isAlreadyOwner;
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
            if (OwnerToDevice.TryGetValue(owner, out TDevice? ownedDevice) && ownedDevice != device)
            {
                // owner already owns another device
                return false;
            }
            if (DeviceToOwner.TryGetValue(device, out TOwner? deviceOwner) && deviceOwner != owner)
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
            return OwnerToDevice.TryGetValue(owner, out TDevice? device) ? device : null;
        }

        /// <summary>
        /// Gets the owner of a device, or null if the device is currently not owned.
        /// </summary>
        /// <param name="device">The device object</param>
        /// <returns>The owner or null, if the device has no owner</returns>
        public TOwner? GetOwner(TDevice device)
        {
            return DeviceToOwner.TryGetValue(device, out TOwner? owner) ? owner : null;
        }

        /// <summary>
        /// Removes the ownership of the specified owner object, if there is any.
        /// </summary>
        /// <param name="owner">The owner object</param>
        public void RemoveOwnership(TOwner owner)
        {
            if (!OwnerToDevice.TryGetValue(owner, out TDevice? device))
            {
                return;
            }
            RemoveOwnership(owner, device);
        }

        /// <summary>
        /// Removes the ownership of the specified device object, if there is any.
        /// </summary>
        /// <param name="device">The device object</param>
        public void RemoveOwnership(TDevice device)
        {
            if (!DeviceToOwner.TryGetValue(device, out TOwner? owner))
            {
                return;
            }
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
