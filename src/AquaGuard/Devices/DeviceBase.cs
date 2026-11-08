
using System;

namespace AquaGuard.Devices
{
    public abstract class DeviceBase : IDevice
    {
        public string DeviceId { get; }

        public string HabitatId { get; }

        public string Name { get; }

        protected DeviceBase(
            string deviceId,
            string habitatId,
            string name)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException(
                    "Device ID cannot be empty.",
                    nameof(deviceId));

            if (string.IsNullOrWhiteSpace(habitatId))
                throw new ArgumentException(
                    "Habitat ID cannot be empty.",
                    nameof(habitatId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Device name cannot be empty.",
                    nameof(name));

            DeviceId = deviceId;
            HabitatId = habitatId;
            Name = name;
        }
    }
}
