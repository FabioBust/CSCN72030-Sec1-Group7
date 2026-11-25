using System;
using System.Collections.Generic;
using System.Text;
using AquaGuard.Devices;
using AquaGuard.Devices.Equipment;

namespace AquaGuard.Core
{
    public class EquipmentStatusMonitor
    {
        public EquipmentStatus GetStatus(DeviceBase device)
        {
            return device switch
            {
                CirculationPump pump => new EquipmentStatus(
                    pump.DeviceId,
                    pump.HabitatId,
                    pump.Name,
                    pump.IsOn,
                    pump.SpeedPercentage),

                FilterSystem filter => new EquipmentStatus(
                    filter.DeviceId,
                    filter.HabitatId,
                    filter.Name,
                    filter.IsOn,
                    null),

                Aerator aerator => new EquipmentStatus(
                    aerator.DeviceId,
                    aerator.HabitatId,
                    aerator.Name,
                    aerator.IsOn,
                    aerator.OutputPercentage),

                _ => throw new ArgumentException(
                    "Unsupported equipment type.",
                    nameof(device))
            };
        }
    }
}

