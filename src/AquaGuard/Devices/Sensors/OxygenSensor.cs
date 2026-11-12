
using System;
using System.Globalization;
using AquaGuard.Data;

namespace AquaGuard.Devices.Sensors
{
    public class OxygenSensor : Sensor
    {
        private readonly FileDataProvider _dataProvider;

        public const string Unit = "mg/L";

        public double? CurrentOxygen { get; private set; }

        public bool HasReading => CurrentOxygen.HasValue;

        public OxygenSensor(
            string deviceId,
            string habitatId,
            string name,
            FileDataProvider dataProvider)
            : base(deviceId, habitatId, name)
        {
            _dataProvider = dataProvider
                ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        public void UpdateReading(double? oxygen)
        {
            if (oxygen.HasValue &&
                (!double.IsFinite(oxygen.Value) || oxygen.Value < 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(oxygen),
                    "Oxygen level must be finite and non-negative.");
            }

            CurrentOxygen = oxygen;
        }

        public void ReadNextReading()
        {
            string? record = _dataProvider.ReadNextRecord();

            if (!double.TryParse(
                    record,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double oxygen)
                || !double.IsFinite(oxygen)
                || oxygen < 0)
            {
                UpdateReading(null);
                return;
            }

            UpdateReading(oxygen);
        }
    }
}
