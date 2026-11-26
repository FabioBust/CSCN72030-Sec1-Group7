using System;
using System.Collections.Generic;
using System.Text;
using AquaGuard.Devices;
using AquaGuard.Devices.Equipment;

namespace AquaGuard.Core
{
    public class EquipmentController
    {
        public void TurnOn(DeviceBase device)
        {
            ArgumentNullException.ThrowIfNull(device);

            switch (device)
            {
                case CirculationPump pump:
                    pump.TurnOn();
                    break;

                case FilterSystem filter:
                    filter.TurnOn();
                    break;

                case Aerator aerator:
                    aerator.TurnOn();
                    break;

                default:
                    throw new ArgumentException(
                        "Unsupported equipment type.",
                        nameof(device));
            }
        }

        public void TurnOff(DeviceBase device)
        {
            ArgumentNullException.ThrowIfNull(device);

            switch (device)
            {
                case CirculationPump pump:
                    pump.TurnOff();
                    break;

                case FilterSystem filter:
                    filter.TurnOff();
                    break;

                case Aerator aerator:
                    aerator.TurnOff();
                    break;

                default:
                    throw new ArgumentException(
                        "Unsupported equipment type.",
                        nameof(device));
            }
        }
    }
}
