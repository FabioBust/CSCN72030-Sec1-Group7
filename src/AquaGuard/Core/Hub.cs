using AquaGuard.Models;
using AquaGuard.Data;
using AquaGuard.Devices.Equipment;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AquaGuard.Core
{
    public sealed class Hub
    {
        private readonly AuthenticationService authentication;
        private readonly EquipmentControlService controls;
        private readonly DataTransitBus bus;
        private readonly Dictionary<string, Habitat> habitats;
        private readonly Func<Habitat, bool>? handleFeeding;
        private readonly DataLogger? logger;
        private readonly Dictionary<(string, string), (Func<double?> Read, string? Unit, SensorThresholds? Thresholds)> sensorInputs = new();
        private readonly AlertManager alertManager = new();
        private readonly List<AlarmTransition> pendingAlarmHistory = new();

        public DataTransitBus Bus => bus;
        public IReadOnlyCollection<Habitat> Habitats => habitats.Values;
        internal bool SensorLoggingFailed { get; private set; }
        internal bool AlarmLoggingFailed { get; private set; }
        internal bool ThresholdConfigurationFailed { get; set; }
        internal IReadOnlyList<Alert> ActiveAlerts => alertManager.ActiveAlerts;

        internal IReadOnlyList<HabitatHistoryEntry> ReadHabitatHistory(
            string habitatId, DateTimeOffset through, int offset, int limit)
        {
            authentication.RequireStaff();
            Habitat habitat = GetHabitat(habitatId);
            return logger?.ReadHabitatHistory(habitat.HabitatId, through, offset, limit) ?? [];
        }

        public void RegisterHabitat(Habitat habitat)
        {
            ArgumentNullException.ThrowIfNull(habitat);
            habitats.Add(habitat.HabitatId, habitat);
        }

        public void RegisterSensorInput(string habitatId, string deviceId, Func<double?> read) =>
            RegisterSensorInput(habitatId, deviceId, read, null);

        internal void RegisterSensorInput(string habitatId, string deviceId, Func<double?> read,
            string? unit, SensorThresholds? thresholds = null)
        {
            ArgumentNullException.ThrowIfNull(read);
            if (GetHabitat(habitatId).GetSensor(deviceId) is null)
                throw new KeyNotFoundException($"Unknown sensor '{deviceId}'.");
            sensorInputs.Add((habitatId, deviceId), (read, unit, thresholds));
        }

        private readonly Dictionary<string, string> thresholdDirectories = new();

        internal void RegisterThresholdDirectory( string habitatId, string directory)
        {
            thresholdDirectories[habitatId] = directory;
        }


        internal void SetSensorThresholds(string habitatId, string deviceId, SensorThresholds thresholds)
        {
            // Only staff can change thresholds
            authentication.RequireStaff();
            ArgumentNullException.ThrowIfNull(thresholds);
            // Validate threshold values
            if (!thresholds.IsValid)
            {
                throw new ArgumentException("Invalid sensor threshold values.", nameof(thresholds));
            }
            var habitat = GetHabitat(habitatId);
            // Check whether the sensor exists
            if (habitat.GetSensor(deviceId) is null)
            {
                throw new KeyNotFoundException($"Unknown sensor '{deviceId}'.");
            }
            var key = (habitatId, deviceId);
            if (!sensorInputs.TryGetValue(key, out var input))
            {
                throw new InvalidOperationException("Sensor input is not registered.");
            }
            // Save thresholds for file-based habitats
            if (thresholdDirectories.TryGetValue(habitatId, out string? directory))
            {
                var configuration = new Dictionary<string, SensorThresholds>();
                // Preserve other configured sensors
                foreach (var entry in sensorInputs)
                {
                    if (entry.Key.Item1 == habitatId && entry.Value.Thresholds is { } existing)
                    {
                        configuration[entry.Key.Item2] = existing;
                    }
                }

                // Add or replace the selected sensor
                configuration[deviceId] = thresholds;
                // Save before updating memory
                HabitatFileSources.SaveThresholds(directory, configuration);
            }
            // Update thresholds in memory
            sensorInputs[key] = (input.Read, input.Unit, thresholds);
        }


        internal SensorThresholds? GetSensorThresholds(
            string habitatId,
            string deviceId)
        {
            authentication.RequireStaff();

            if (sensorInputs.TryGetValue(
                (habitatId, deviceId), out var input))
            {
                return input.Thresholds;
            }

            return null;
        }

        public IReadOnlyList<SensorDisplay> PollHabitatSensors()
        {
            var readings = new List<SensorDisplay>();
            List<LoggedSensorReading>? batch = logger is null ? null : new();
            foreach (var habitat in habitats.Values)
            foreach (var sensor in habitat.Sensors)
            {
                sensorInputs.TryGetValue((habitat.HabitatId, sensor.DeviceId), out var input);
                double? value = input.Read?.Invoke();
                DateTimeOffset timestamp = DateTimeOffset.UtcNow;
                EnvironmentalSeverity? classification = value is double valid && double.IsFinite(valid)
                    ? input.Thresholds?.Classify(valid) : null;
                if (classification is { } severity)
                {
                    var transition = alertManager.Evaluate(habitat.HabitatId, sensor.DeviceId, severity, timestamp);
                    if (logger is not null && transition is not null)
                        pendingAlarmHistory.Add(transition);
                }
                var display = new SensorDisplay(habitat.HabitatId, sensor.DeviceId, value)
                {
                    SensorName = sensor.Name,
                    Unit = input.Unit,
                    Classification = classification
                };
                readings.Add(display);
                bus.Publish(new SystemMessage(TelemetryKind.SensorReading, display.HabitatId,
                    display.DeviceId, value.HasValue ? display.Reading : null, timestamp.UtcDateTime));
                if (value is double measured && double.IsFinite(measured))
                    batch?.Add(new LoggedSensorReading(display.HabitatId, display.DeviceId, measured, display.Unit, timestamp));
            }

            if (logger is not null && batch is { Count: > 0 })
            {
                try
                {
                    logger.LogSensorReadings(batch);
                    SensorLoggingFailed = false;
                }
                catch (SqliteException)
                {
                    SensorLoggingFailed = true;
                }
            }
            if (logger is not null && pendingAlarmHistory.Count > 0)
            {
                try
                {
                    logger.LogAlarmTransitions(pendingAlarmHistory);
                    pendingAlarmHistory.Clear();
                    AlarmLoggingFailed = false;
                }
                catch (SqliteException)
                {
                    AlarmLoggingFailed = true;
                }
            }

            return readings;
        }

        public Hub(AuthenticationService authentication, DataTransitBus bus,
            IEnumerable<Habitat> habitats, Func<Habitat, bool>? handleFeeding = null)
            : this(authentication, bus, habitats, handleFeeding, null)
        {
        }

        internal Hub(AuthenticationService authentication, DataTransitBus bus,
            IEnumerable<Habitat> habitats, Func<Habitat, bool>? handleFeeding, DataLogger? logger)
        {
            this.authentication = authentication;
            this.bus = bus;
            this.habitats = habitats.ToDictionary(habitat => habitat.HabitatId);
            this.handleFeeding = handleFeeding;
            this.logger = logger;
            controls = new EquipmentControlService(authentication, logger);
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

        public void SetEquipmentPercentage(string habitatId, string deviceId, int percentage)
        {
            authentication.RequireStaff();
            var habitat = GetHabitat(habitatId);
            string setting = controls.SetPercentage(habitat, deviceId, percentage);
            bus.Publish(new SystemMessage(TelemetryKind.EquipmentState, habitatId, deviceId,
                $"{setting}: {percentage.ToString(CultureInfo.InvariantCulture)}%", DateTime.UtcNow));
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
