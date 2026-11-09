using System.Collections.Generic;
using AquaGuard.Devices;
using System;

namespace AquaGuard.Core
{
    public class Habitat
    {
        public string HabitatId { get; }
        public string Name { get; }
        private readonly Dictionary<string, IDevice> _sensors = new();

        public Habitat(string habitatId, string name)
        {
            if (string.IsNullOrWhiteSpace(habitatId))
            {
                throw new ArgumentException(
                    "Habitat ID cannot be empty.",
                    nameof(habitatId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Habitat name cannot be empty.",
                    nameof(name));
            }

            HabitatId = habitatId;
            Name = name;
        }

        public void AddSensor(IDevice sensor)
        {
            if (sensor == null)
                throw new ArgumentNullException(nameof(sensor));

            if (sensor.HabitatId != HabitatId)
                throw new ArgumentException(
                    "Sensor belongs to a different habitat.",
                    nameof(sensor));

            _sensors.Add(sensor.DeviceId, sensor);
        }

        public IDevice? GetSensor(string deviceId)
        {
            return _sensors.TryGetValue(deviceId, out var sensor)
                ? sensor
                : null;
        }

    }
}
