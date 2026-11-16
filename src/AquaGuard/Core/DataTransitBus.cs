using AquaGuard.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Core
{
    public sealed class DataTransitBus
    {
        public event EventHandler<SensorReading>? ReadingReceived;

        public void Publish(SensorReading reading)
        {
            ArgumentNullException.ThrowIfNull(reading);
            ReadingReceived?.Invoke(this, reading);
        }
    }
}
