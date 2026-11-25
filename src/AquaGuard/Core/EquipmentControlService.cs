using AquaGuard.Devices;
using AquaGuard.Devices.Equipment;

namespace AquaGuard.Core
{
    public sealed class EquipmentControlService(AuthenticationService authentication)
    {
        internal string SetPercentage(Habitat habitat, string deviceId, int percentage)
        {
            authentication.RequireStaff();
            switch (habitat.GetEquipment(deviceId))
            {
                case CirculationPump pump:
                    pump.SetSpeed(percentage);
                    return "Speed";
                case Aerator aerator:
                    aerator.SetOutput(percentage);
                    return "Output";
                default:
                    throw new InvalidOperationException("No equipment with a percentage setting matches the device ID.");
            }
        }

        public void SetPower(Habitat habitat, string deviceId, bool isOn)
        {
            authentication.RequireStaff();

            IDevice? equipment = habitat.GetEquipment(deviceId);
            switch (equipment)
            {
                case Devices.Equipment.CirculationPump pump:
                    if (isOn) pump.TurnOn();
                    else pump.TurnOff();
                    break;
                case Devices.Equipment.FilterSystem filter:
                    if (isOn) filter.TurnOn();
                    else filter.TurnOff();
                    break;
                case Devices.Equipment.FoodDispenser dispenser:
                    dispenser.SetEnabled(isOn);
                    break;
                default:
                    throw new InvalidOperationException("No controllable equipment matches the device ID.");
            }
        }
    }
}
