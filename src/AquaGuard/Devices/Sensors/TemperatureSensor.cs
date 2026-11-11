
using System;
using System.Globalization;
using AquaGuard.Data;

namespace AquaGuard.Devices.Sensors
{
    public class TemperatureSensor : Sensor
    {
        private readonly FileDataProvider _dataProvider;

        public const string Unit = "°C";

        public double? CurrentTemperature { get; private set; }

        public bool HasReading => CurrentTemperature.HasValue;

        public TemperatureSensor(
            string deviceId,
            string habitatId,
            string name,
            FileDataProvider dataProvider)
            : base(deviceId, habitatId, name)
        {
            _dataProvider = dataProvider
                ?? throw new ArgumentNullException(nameof(dataProvider));
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

        public void ReadNextReading()
        {
            string? record = _dataProvider.ReadNextRecord();

            if (!double.TryParse(
                    record,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double temperature)
                || !double.IsFinite(temperature))
            {
                UpdateReading(null);
                return;
            }

            UpdateReading(temperature);
        }
    }
}
