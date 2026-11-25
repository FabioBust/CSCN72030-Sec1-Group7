using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace AquaGuard.Core
{
    public class AlertManager
    {
        private readonly List<Alert> activeAlerts = new();

        public IReadOnlyList<Alert> ActiveAlerts
        {
            get { return activeAlerts.AsReadOnly(); }
        }

        public void RaiseAlert(
            string habitatId,
            string deviceId,
            string message)
        {
            bool alertExists = activeAlerts.Any(alert =>
                alert.HabitatId == habitatId &&
                alert.DeviceId == deviceId);

            if (!alertExists)
            {
                activeAlerts.Add(
                    new Alert(habitatId, deviceId, message));
            }
        }

        public void ClearAlert(string habitatId, string deviceId)
        {
            activeAlerts.RemoveAll(alert =>
                alert.HabitatId == habitatId &&
                alert.DeviceId == deviceId);
        }

        internal AlarmTransition? Evaluate(
            string habitatId, string deviceId, EnvironmentalSeverity severity, DateTimeOffset timestamp)
        {
            int index = activeAlerts.FindIndex(alert =>
                alert.HabitatId == habitatId && alert.DeviceId == deviceId);
            if (severity == EnvironmentalSeverity.Normal)
            {
                if (index < 0)
                    return null;

                activeAlerts.RemoveAt(index);
                return new AlarmTransition(habitatId, deviceId, "Resolved", severity, timestamp);
            }

            if (index >= 0 && activeAlerts[index].Severity == severity)
                return null;

            var alert = new Alert(habitatId, deviceId, $"{deviceId}: {severity}",
                severity, index < 0 ? timestamp.UtcDateTime : activeAlerts[index].Timestamp);
            if (index < 0)
                activeAlerts.Add(alert);
            else
                activeAlerts[index] = alert;

            return new AlarmTransition(habitatId, deviceId, index < 0 ? "Raised" : "Updated", severity, timestamp);
        }
    }

    internal sealed record AlarmTransition(string HabitatId, string DeviceId, string Change,
        EnvironmentalSeverity Severity, DateTimeOffset Timestamp);
}

