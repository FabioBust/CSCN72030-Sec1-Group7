
using System;

namespace AquaGuard.Models
{
    public class DeviceTelemetry
    {
        public string HabitatId { get; set; } = string.Empty;

        public string DeviceId { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
    }
}
