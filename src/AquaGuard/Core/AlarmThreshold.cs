using System;
using System.Collections.Generic;
using System.Text;


namespace AquaGuard.Core
{
    public class AlarmThreshold
    {
        public string HabitatId { get; }
        public string DeviceId { get; }
        public double Minimum { get; }
        public double Maximum { get; }

        public AlarmThreshold(
            string habitatId,
            string deviceId,
            double minimum,
            double maximum)
        {
            HabitatId = habitatId;
            DeviceId = deviceId;
            Minimum = minimum;
            Maximum = maximum;
        }
    }
}

