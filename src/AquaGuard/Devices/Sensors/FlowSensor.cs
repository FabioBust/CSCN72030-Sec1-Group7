
using AquaGuard.Devices;

namespace AquaGuard.Devices.Sensors
{
    public class FlowSensor : DeviceBase
    {
        public FlowSensor(
            string deviceId,
            string habitatId,
            string name)
            : base(deviceId, habitatId, name)
        {
        }
    }
}
