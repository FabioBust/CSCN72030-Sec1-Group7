using System.Globalization;
using AquaGuard.Data;
using AquaGuard.Devices;
using AquaGuard.Devices.Equipment;

namespace AquaGuard.Core
{
    public sealed class EquipmentControlService(AuthenticationService authentication)
    {
        private readonly DataLogger? logger;

        internal EquipmentControlService(AuthenticationService authentication, DataLogger? logger)
            : this(authentication)
        {
            this.logger = logger;
        }

        internal string SetPercentage(Habitat habitat, string deviceId, int percentage)
        {
            authentication.RequireStaff();
            switch (habitat.GetEquipment(deviceId))
            {
                case CirculationPump pump:
                    LogPercentage(pump, "speed_percentage", pump.SpeedPercentage, percentage);
                    pump.SetSpeed(percentage);
                    return "Speed";
                case Aerator aerator:
                    LogPercentage(aerator, "output_percentage", aerator.OutputPercentage, percentage);
                    aerator.SetOutput(percentage);
                    return "Output";
                default:
                    throw new InvalidOperationException("No equipment with a percentage setting matches the device ID.");
            }
        }

        private void LogPercentage(IDevice device, string setting, int previous, int percentage)
        {
            if (percentage is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage), percentage, "Percentage must be between 0 and 100.");

            if (previous != percentage)
                logger?.LogEquipmentSetting(device.HabitatId, device.DeviceId, setting,
                    percentage.ToString(CultureInfo.InvariantCulture));
        }

        private void LogPower(IDevice device, string setting, bool previous, bool isOn)
        {
            if (previous != isOn)
                logger?.LogEquipmentSetting(device.HabitatId, device.DeviceId, setting, isOn ? "On" : "Off");
        }


        public void SetPower(Habitat habitat, string deviceId, bool isOn)
        {
            authentication.RequireStaff();

            IDevice? equipment = habitat.GetEquipment(deviceId);

            switch (equipment)
            {
                case CirculationPump pump:
                    LogPower(pump, "power", pump.IsOn, isOn);

                    if (isOn) pump.TurnOn();
                    else pump.TurnOff();

                    break;

                case FilterSystem filter:
                    LogPower(filter, "power", filter.IsOn, isOn);

                    if (isOn) filter.TurnOn();
                    else filter.TurnOff();

                    break;

                case Aerator aerator:
                    LogPower(aerator, "power", aerator.IsOn, isOn);

                    if (isOn) aerator.TurnOn();
                    else aerator.TurnOff();

                    break;

                case FoodDispenser dispenser:
                    LogPower(dispenser, "enabled", dispenser.IsEnabled, isOn);
                    dispenser.SetEnabled(isOn);

                    break;

                default:
                    throw new InvalidOperationException(
                        "No controllable equipment matches the device ID.");
            }
        }

    }
}
