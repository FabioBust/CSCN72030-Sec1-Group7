using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Devices.Equipment
{
    public class FilterSystem : DeviceBase
    {
        public bool IsOn { get; private set; }

        public FilterSystem(string deviceId, string habitatId, string name)
            : base(deviceId, habitatId, name)
        {
            IsOn = false;
        }

        public void TurnOn()
        {
            IsOn = true;
        }

        public void TurnOff()
        {
            IsOn = false;
        }
    }
}
