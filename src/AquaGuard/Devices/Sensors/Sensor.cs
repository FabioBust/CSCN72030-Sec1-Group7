using AquaGuard.Devices;

namespace AquaGuard.Devices.Sensors
{
    public abstract class Sensor : DeviceBase
    {
        protected Sensor(
            string deviceId,
            string habitatId,
            string name)
            : base(deviceId, habitatId, name)
        {
        }
    }
}
