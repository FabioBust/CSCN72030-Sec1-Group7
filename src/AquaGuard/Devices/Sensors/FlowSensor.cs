using System;
using System.Globalization;
using AquaGuard.Devices;
using AquaGuard.Models;
namespace AquaGuard.Devices.Sensors
{
    public class FlowSensor : DeviceBase
    {
        // Current water-flow rate in litres per minute
        public double? CurrentFlowRate { get; private set; }

        public FlowSensor(
            string deviceId,
            string habitatId,
            string name)
            : base(deviceId, habitatId, name)
        {
        }


        public void UpdateReading(double flowRate)
        {
            if (double.IsNaN(flowRate) ||
                double.IsInfinity(flowRate) ||
                flowRate < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(flowRate),
                    "Flow rate must be a finite, non-negative value.");
            }

            CurrentFlowRate = flowRate;
        }

        public DeviceTelemetry GetTelemetry()
        {
            if (!CurrentFlowRate.HasValue)
            {
                throw new InvalidOperationException(
                    "No flow rate reading is available.");
            }

            return new DeviceTelemetry
            {
                DeviceId = DeviceId,
                HabitatId = HabitatId,
                Value = CurrentFlowRate.Value.ToString(
                    CultureInfo.InvariantCulture),
                Timestamp = DateTime.UtcNow
            };
        }

    }
}
