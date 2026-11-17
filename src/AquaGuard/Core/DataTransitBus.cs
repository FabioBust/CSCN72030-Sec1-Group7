using AquaGuard.Models;

namespace AquaGuard.Core
{
    public sealed class DataTransitBus
    {
        public event Action<SystemMessage>? TelemetryReceived;

        internal void Publish(SystemMessage message)
        {
            TelemetryReceived?.Invoke(message);
        }

        internal void SendEquipmentCommand(
            EquipmentControlService controls, Habitat habitat, string deviceId, bool isOn)
        {
            controls.SetPower(habitat, deviceId, isOn);
            Publish(new SystemMessage(TelemetryKind.EquipmentState, habitat.HabitatId, deviceId,
                isOn ? "On" : "Off", DateTime.UtcNow));
        }

        internal bool SendFeedingRequest(Habitat habitat, Func<Habitat, bool> handleFeeding)
        {
            return handleFeeding(habitat);
        }
    }
}
