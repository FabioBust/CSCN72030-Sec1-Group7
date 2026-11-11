
using AquaGuard.Devices;

namespace AquaGuard.Devices.Sensors
{
    public class PHSensor : DeviceBase
    {
        public double? CurrentPH { get; private set; }

        public PHSensor(
            string deviceId,
            string habitatId,
            string name)
            : base(deviceId, habitatId, name)
        {
        }


        public void UpdateReading(double phValue)
        {
            if (double.IsNaN(phValue) ||
                double.IsInfinity(phValue) ||
                phValue < 0 ||
                phValue > 14)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(phValue),
                    "pH value must be between 0 and 14.");
            }

            CurrentPH = phValue;
        }

    }
}
