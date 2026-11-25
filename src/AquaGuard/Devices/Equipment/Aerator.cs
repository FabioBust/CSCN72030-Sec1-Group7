using System;
using System.Collections.Generic;
using System.Text;


namespace AquaGuard.Devices.Equipment
{
    public class Aerator : Equipment
    {
        public bool IsOn { get; private set; }

        public int OutputPercentage { get; private set; }

        public Aerator(string deviceId, string habitatId, string name) : base(deviceId, habitatId, name)
        {
            IsOn = false;
            OutputPercentage = 0;
        }

        public void TurnOn()
        {
            IsOn = true;
            OutputPercentage = 100;
        }

        public void TurnOff()
        {
            IsOn = false;
            OutputPercentage = 0;
        }

        public void SetOutput(int percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(
                    nameof(percentage),
                    "Output must be between 0 and 100.");

            OutputPercentage = percentage;
            IsOn = percentage > 0;
        }
    }
}
