using AquaGuard.Models;

namespace AquaGuard.Core
{
    public sealed class Hub
    {
        private readonly AuthenticationService authentication;
        private readonly EquipmentControlService controls;
        private readonly DataTransitBus bus;
        private readonly Dictionary<string, Habitat> habitats;
        private readonly Func<Habitat, bool>? handleFeeding;

        public Hub(AuthenticationService authentication, DataTransitBus bus,
            IEnumerable<Habitat> habitats, Func<Habitat, bool>? handleFeeding = null)
        {
            this.authentication = authentication;
            this.bus = bus;
            this.habitats = habitats.ToDictionary(habitat => habitat.HabitatId);
            this.handleFeeding = handleFeeding;
            controls = new EquipmentControlService(authentication);
        }

        public void PublishSensorReading(DeviceTelemetry reading)
        {
            ArgumentNullException.ThrowIfNull(reading);
            bus.Publish(new SystemMessage(TelemetryKind.SensorReading, reading.HabitatId,
                reading.DeviceId, reading.Value, reading.Timestamp));
        }

        public IReadOnlyList<SensorDisplay> PollSensors(SensorMonitor monitor)
        {
            var readings = monitor.Poll();
            foreach (var reading in readings)
            {
                bus.Publish(new SystemMessage(TelemetryKind.SensorReading, reading.HabitatId,
                    reading.DeviceId, reading.Value.HasValue ? reading.Reading : null, DateTime.UtcNow));
            }

            return readings;
        }

        public void SetEquipmentPower(string habitatId, string deviceId, bool isOn)
        {
            authentication.RequireStaff();
            bus.SendEquipmentCommand(controls, GetHabitat(habitatId), deviceId, isOn);
        }

        public bool RequestFeeding(string habitatId)
        {
            authentication.RequireStaff();
            Habitat habitat = GetHabitat(habitatId);
            return handleFeeding is not null && bus.SendFeedingRequest(habitat, handleFeeding);
        }

        private Habitat GetHabitat(string habitatId)
        {
            return habitats.TryGetValue(habitatId, out var habitat)
                ? habitat
                : throw new KeyNotFoundException($"Unknown habitat '{habitatId}'.");
        }
    }
}
