using AquaGuard.Devices;
using AquaGuard.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AquaGuard.Core
{
    public class EquipmentIntegrationService
    {
        private readonly EquipmentController controller;
        private readonly EquipmentStatusMonitor monitor;

        public EquipmentIntegrationService()
        {
            controller = new EquipmentController();
            monitor = new EquipmentStatusMonitor();
        }

        public EquipmentStatus TurnOn(DeviceBase device)
        {
            controller.TurnOn(device);

            return monitor.GetStatus(device);
        }

        public EquipmentStatus TurnOff(DeviceBase device)
        {
            controller.TurnOff(device);

            return monitor.GetStatus(device);
        }

        public EquipmentStatus SetSetting(
            DeviceBase device,
            int percentage)
        {
            controller.SetSetting(device, percentage);

            return monitor.GetStatus(device);
        }

        public EquipmentStatus GetStatus(DeviceBase device)
        {
            return monitor.GetStatus(device);
        }
    }
}

