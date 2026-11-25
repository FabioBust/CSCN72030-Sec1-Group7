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

        public Alert(string habitatId, string deviceId, string message)
        {
            HabitatId = habitatId;
            DeviceId = deviceId;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }
}

