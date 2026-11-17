using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Models
{
    public sealed record SensorReading(
    string HabitatId,
    string DeviceId,
    string SensorName,
    double? Value,
    string Unit,
    DateTime TimestampUtc);
}
