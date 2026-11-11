
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
            CurrentPH = phValue;
        }
    }
}
