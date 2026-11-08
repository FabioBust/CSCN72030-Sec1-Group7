
using System;

namespace AquaGuard.Devices.Sensors
{
    public class TemperatureSensor : Sensor
    {
        public const string Unit = "°C";

        public double? CurrentTemperature { get; private set; }

        public bool HasReading => CurrentTemperature.HasValue;

        public TemperatureSensor(
            string deviceId,
            string habitatId,
            string name)
            : base(deviceId, habitatId, name)
        {
        }

        public void UpdateReading(double? temperature)
        {
            if (temperature.HasValue &&
                !double.IsFinite(temperature.Value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(temperature),
                    "Temperature must be a finite number.");
            }

            CurrentTemperature = temperature;
        }
    }
}
