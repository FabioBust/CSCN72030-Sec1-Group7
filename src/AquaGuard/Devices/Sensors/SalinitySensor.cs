using System.Globalization;
using AquaGuard.Models;

namespace AquaGuard.Devices.Sensors
{
    public class SalinitySensor : DeviceBase
    {
        public double? CurrentSalinity { get; private set; }

        public SalinitySensor(string deviceId, string habitatId, string name)
            : base(deviceId, habitatId, name)
        {
        }

        public void UpdateReading(double salinity)
        {
            if (!double.IsFinite(salinity) || salinity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(salinity), "Salinity must be a finite, non-negative value.");
            }

            CurrentSalinity = salinity;
        }

        public DeviceTelemetry GetTelemetry()
        {
            if (CurrentSalinity is not double salinity)
            {
                throw new InvalidOperationException("No salinity reading is available.");
            }

            return new DeviceTelemetry
            {
                DeviceId = DeviceId,
                HabitatId = HabitatId,
                Value = salinity.ToString(CultureInfo.InvariantCulture),
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
