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
    }
}

