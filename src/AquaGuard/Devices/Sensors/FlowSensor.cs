using System;
using AquaGuard.Devices;

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

    }
}
