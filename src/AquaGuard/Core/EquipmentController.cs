using System;
using System.Collections.Generic;
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
                case CirculationPump pump:pump.TurnOn();
                    break;

                case FilterSystem filter:filter.TurnOn();
                    break;

                case Aerator aerator:aerator.TurnOn();
                    break;

                default:throw new ArgumentException("Unsupported equipment type.", nameof(device));
            }
        }

        public void TurnOff(DeviceBase device)
        {
            ArgumentNullException.ThrowIfNull(device);

            switch (device)
            {
                case CirculationPump pump:pump.TurnOff();
                    break;

                case FilterSystem filter:filter.TurnOff();
                    break;

                case Aerator aerator:aerator.TurnOff();
                    break;

                default:throw new ArgumentException( "Unsupported equipment type.", nameof(device));
            }
        }

        public void SetSetting( DeviceBase device, int percentage)
        {
            ArgumentNullException.ThrowIfNull(device);

            if (percentage < 0 || percentage > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(percentage),
                    "Setting must be between 0 and 100.");
            }

            switch (device)
            {
                case CirculationPump pump:pump.SetSpeed(percentage);
                    break;

                case Aerator aerator:aerator.SetOutput(percentage);
                    break;

                default:throw new ArgumentException(
                        "This equipment does not support adjustable settings.", nameof(device));
            }
        }
    }
}
