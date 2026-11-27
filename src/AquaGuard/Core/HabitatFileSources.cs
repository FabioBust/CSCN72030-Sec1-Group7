using System.Globalization;
using System.IO;
using System.Text.Json;
using AquaGuard.Data;
using AquaGuard.Devices;
using AquaGuard.Devices.Equipment;
using AquaGuard.Devices.Sensors;

namespace AquaGuard.Core;

// Adapts external files to the existing sensor APIs without changing their contracts.
public static class HabitatFileSources
{
    public static void Load(Hub hub, string root) => Load(hub, root, null);

    internal static void Load(Hub hub, string root, DataLogger? logger)
    {
        foreach (string directory in Directory.GetDirectories(root).Order())
        {
            var habitat = new Habitat(Path.GetFileName(directory), Path.GetFileName(directory));
            var thresholds = LoadThresholds(hub, directory);
            habitat.AddEquipment(new CirculationPump("circulation-pump", habitat.HabitatId, "Circulation pump"));
            habitat.AddEquipment(new FilterSystem("filter-system", habitat.HabitatId, "Filter system"));
            habitat.AddEquipment(new Aerator("aerator", habitat.HabitatId, "Aerator"));
            habitat.AddEquipment(new HeaterChiller("heater-chiller", habitat.HabitatId, "Heater/chiller"));
            habitat.AddEquipment(new FoodDispenser("food-dispenser", habitat.HabitatId, "Food dispenser") { Logger = logger });
            hub.RegisterHabitat(habitat);
            hub.RegisterThresholdDirectory(habitat.HabitatId, directory);
            foreach (string path in Directory.GetFiles(directory, "*.txt").Order())
            {
                string id = Path.GetFileNameWithoutExtension(path);
                var provider = new FileDataProvider(path);
                IDevice sensor;
                Func<double?> read;
                string? unit = null;
                if (id.Equals("temperature", StringComparison.OrdinalIgnoreCase))
                {
                    var temperature = new TemperatureSensor(id, habitat.HabitatId, id, provider);
                    sensor = temperature;
                    unit = TemperatureSensor.Unit;
                    read = () => { temperature.ReadNextReading(); return temperature.CurrentTemperature; };
                }
                else if (id.Equals("oxygen", StringComparison.OrdinalIgnoreCase))
                {
                    var oxygen = new OxygenSensor(id, habitat.HabitatId, id, provider);
                    sensor = oxygen;
                    unit = OxygenSensor.Unit;
                    read = () => { oxygen.ReadNextReading(); return oxygen.CurrentOxygen; };
                }
                else
                {
                    Action<double> update;
                    switch (id.ToLowerInvariant())
                    {
                        case "ph":
                            var ph = new PHSensor(id, habitat.HabitatId, id);
                            sensor = ph; update = ph.UpdateReading; break;
                        case "flow":
                            var flow = new FlowSensor(id, habitat.HabitatId, id);
                            sensor = flow; update = flow.UpdateReading; unit = "L/min"; break;
                        case "salinity":
                            var salinity = new SalinitySensor(id, habitat.HabitatId, id);
                            sensor = salinity; update = salinity.UpdateReading; unit = "ppt"; break;
                        default:
                            sensor = new FileSensor(id, habitat.HabitatId, id);
                            update = _ => { }; break;
                    }
                    read = () =>
                    {
                        if (!double.TryParse(provider.ReadNextRecord(), NumberStyles.Float,
                            CultureInfo.InvariantCulture, out double value) || !double.IsFinite(value))
                            return null;
                        try { update(value); return value; }
                        catch (ArgumentOutOfRangeException) { return null; }
                    };
                }
                habitat.AddSensor(sensor);
                hub.RegisterSensorInput(habitat.HabitatId, id, read, unit, thresholds.GetValueOrDefault(id));
            }
        }
    }


    internal static void SaveThresholds(
        string directory,
        Dictionary<string, SensorThresholds> thresholds)
    {
        if (thresholds.Values.Any(value => !value.IsValid))
        {
            throw new ArgumentException(
                "Invalid sensor threshold values.");
        }

        string path = Path.Combine(
            directory, "thresholds.json");

        string temporaryPath = Path.Combine(
            directory, $"{Guid.NewGuid()}.tmp");

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(
            thresholds, options);

        try
        {
            File.WriteAllText(temporaryPath, json);

            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }


    private static Dictionary<string, SensorThresholds> LoadThresholds(Hub hub, string directory)
    {
        string path = Path.Combine(directory, "thresholds.json");
        if (File.Exists(path))
        {
            try
            {
                var thresholds = JsonSerializer.Deserialize<Dictionary<string, SensorThresholds>>(File.ReadAllText(path));
                if (thresholds is not null && thresholds.Values.All(value => value is { IsValid: true }))
                    return thresholds;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                // The monitoring view reports the invalid or unreadable configuration.
            }

            hub.ThresholdConfigurationFailed = true;
        }

        return [];
    }

    private sealed record FileSensor(string DeviceId, string HabitatId, string Name) : IDevice;
}
