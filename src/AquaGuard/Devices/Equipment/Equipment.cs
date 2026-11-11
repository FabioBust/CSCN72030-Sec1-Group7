using AquaGuard.Devices;

namespace AquaGuard.Devices.Equipment
{
    public abstract class Equipment : DeviceBase
    {
        protected Equipment(
            string deviceId,
            string habitatId,
            string name)
            : base(deviceId, habitatId, name)
        {
        }
    }
}