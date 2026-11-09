using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Devices.Equipment
{
    public class CirculationPump : DeviceBase
    {
        public bool IsOn { get; private set; }

        public int SpeedPercentage { get; private set; }

        public CirculationPump(string deviceId, string habitatId, string name) : base(deviceId, habitatId, name)
        {
            IsOn = false;
            SpeedPercentage = 0;
        }

        public void TurnOn()
        {
            IsOn = true;
        }

        public void TurnOff()
        {
            IsOn = false;
        }

        public void SetSpeed(int speedPercentage)
        {
            if (speedPercentage < 0 || speedPercentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(speedPercentage), "Pump speed must be between 0 and 100.");
            }

            SpeedPercentage = speedPercentage;
        }
    }
}
