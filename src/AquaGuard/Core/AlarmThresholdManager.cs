using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;

namespace AquaGuard.Core
{
    public class AlarmThresholdManager
    {
        private readonly Dictionary<
            (string HabitatId, string DeviceId),
            AlarmThreshold> thresholds = new();

        public void SetThreshold(
            string habitatId,
            string deviceId,
            double minimum,
            double maximum)
        {
            if (!double.IsFinite(minimum) ||
                !double.IsFinite(maximum) ||
                minimum > maximum)
            {
                throw new ArgumentException(
                    "Invalid alarm threshold range.");
            }

            var threshold = new AlarmThreshold(
                habitatId,
                deviceId,
                minimum,
                maximum);

            thresholds[(habitatId, deviceId)] = threshold;
        }

        public AlarmThreshold? GetThreshold(
            string habitatId,
            string deviceId)
        {
            if (thresholds.TryGetValue(
                (habitatId, deviceId),
                out var threshold))
            {
                return threshold;
            }

            return null;
        }

        public void RemoveThreshold(
            string habitatId,
            string deviceId)
        {
            thresholds.Remove((habitatId, deviceId));
        }
    }
}

