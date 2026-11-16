using AquaGuard.Devices;

namespace AquaGuard.Core
{
    public sealed class EquipmentControlService(AuthenticationService authentication)
    {
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
                default:
                    throw new InvalidOperationException("No controllable equipment matches the device ID.");
            }
        }
    }
}
