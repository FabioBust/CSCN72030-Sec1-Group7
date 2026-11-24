using AquaGuard.Models;
using AquaGuard.Data;
using AquaGuard.Devices.Equipment;
using System.Globalization;

namespace AquaGuard.Core
{
    public sealed class Hub
    {
        private readonly AuthenticationService authentication;
        private readonly EquipmentControlService controls;
        private readonly DataTransitBus bus;
        private readonly Dictionary<string, Habitat> habitats;
        private readonly Func<Habitat, bool>? handleFeeding;
        private readonly Dictionary<(string, string), Func<double?>> sensorInputs = new();

        public DataTransitBus Bus => bus;
        public IReadOnlyCollection<Habitat> Habitats => habitats.Values;

        public void RegisterHabitat(Habitat habitat)
        {
            ArgumentNullException.ThrowIfNull(habitat);
            habitats.Add(habitat.HabitatId, habitat);
        }

        public void RegisterSensorInput(string habitatId, string deviceId, Func<double?> read)
        {
            ArgumentNullException.ThrowIfNull(read);
            if (GetHabitat(habitatId).GetSensor(deviceId) is null)
                throw new KeyNotFoundException($"Unknown sensor '{deviceId}'.");
            sensorInputs.Add((habitatId, deviceId), read);
        }

        public IReadOnlyList<SensorDisplay> PollHabitatSensors()
        {
            var readings = new List<SensorDisplay>();
            foreach (var habitat in habitats.Values)
            foreach (var sensor in habitat.Sensors)
            {
                double? value = sensorInputs.TryGetValue((habitat.HabitatId, sensor.DeviceId), out var read)
                    ? read() : null;
                var display = new SensorDisplay(habitat.HabitatId, sensor.DeviceId, value);
                readings.Add(display);
                bus.Publish(new SystemMessage(TelemetryKind.SensorReading, display.HabitatId,
                    display.DeviceId, value.HasValue ? display.Reading : null, DateTime.UtcNow));
            }
            return readings;
        }

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

        internal void SetHeaterTarget(string habitatId, string deviceId, decimal target, DataLogger logger)
        {
            authentication.RequireStaff();
            if (GetHabitat(habitatId).GetEquipment(deviceId) is not HeaterChiller heater)
                throw new InvalidOperationException("No heater/chiller matches the device ID.");

            HeaterChiller.ValidateTarget(target);
            if (heater.TargetTemperature == target)
                return;

            string value = target.ToString("0.0", CultureInfo.InvariantCulture);
            logger.LogEquipmentSetting(habitatId, deviceId, "target_temperature_c", value);
            heater.SetTargetTemperature(target);
            bus.Publish(new SystemMessage(TelemetryKind.EquipmentState, habitatId, deviceId, value, DateTime.UtcNow));
        }

        public bool CanFeed(string habitatId)
        {
            return GetHabitat(habitatId).Equipment.OfType<FoodDispenser>().SingleOrDefault()?.CanFeed == true;
        }

        public bool RequestVisitorFeeding(string habitatId, decimal simulatedDonationCad)
        {
            return GetHabitat(habitatId).Equipment.OfType<FoodDispenser>().SingleOrDefault()
                ?.TryDispense(simulatedDonationCad) == true;
        }

        private Habitat GetHabitat(string habitatId)
        {
            return habitats.TryGetValue(habitatId, out var habitat)
                ? habitat
                : throw new KeyNotFoundException($"Unknown habitat '{habitatId}'.");
        }
    }
}
