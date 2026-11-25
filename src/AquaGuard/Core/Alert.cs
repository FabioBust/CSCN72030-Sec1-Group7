using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Core
{
    public class Alert
    {
        public string HabitatId { get; }
        public string DeviceId { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }
        public EnvironmentalSeverity? Severity { get; }

        public Alert(string habitatId, string deviceId, string message)
            : this(habitatId, deviceId, message, null, DateTime.Now)
        {
        }

        internal Alert(string habitatId, string deviceId, string message,
            EnvironmentalSeverity? severity, DateTime timestamp)
        {
            HabitatId = habitatId;
            DeviceId = deviceId;
            Message = message;
            Severity = severity;
            Timestamp = timestamp;
        }
    }
}

