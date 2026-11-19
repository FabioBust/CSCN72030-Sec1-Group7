using System;
using System.Collections.Generic;
using System.Text;


namespace AquaGuard.Devices.Equipment
{
    public class Aerator
    {
        public string DeviceId { get; }
        public string HabitatId { get; }
        public string Name { get; }

        public bool IsOn { get; private set; }

        public int OutputPercentage { get; private set; }

        public Aerator(string deviceId, string habitatId, string name)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID cannot be empty.");

            if (string.IsNullOrWhiteSpace(habitatId))
                throw new ArgumentException("Habitat ID cannot be empty.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.");

            DeviceId = deviceId;
            HabitatId = habitatId;
            Name = name;

            IsOn = false;
            OutputPercentage = 0;
        }

        public void TurnOn()
        {
            IsOn = true;
            OutputPercentage = 100;
        }

        public void TurnOff()
        {
            IsOn = false;
            OutputPercentage = 0;
        }

        public void SetOutput(int percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(
                    nameof(percentage),
                    "Output must be between 0 and 100.");

            OutputPercentage = percentage;
            IsOn = percentage > 0;
        }
    }
}
