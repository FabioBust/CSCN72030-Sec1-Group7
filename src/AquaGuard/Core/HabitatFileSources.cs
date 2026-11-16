using System.Globalization;
using System.IO;
using AquaGuard.Data;
using AquaGuard.Devices;
using AquaGuard.Devices.Sensors;

namespace AquaGuard.Core;

// Adapts external files to the existing sensor APIs without changing their contracts.
public static class HabitatFileSources
{
    public static void Load(Hub hub, string root)
    {
        foreach (string directory in Directory.GetDirectories(root).Order())
        {
            var habitat = new Habitat(Path.GetFileName(directory), Path.GetFileName(directory));
            hub.RegisterHabitat(habitat);
            foreach (string path in Directory.GetFiles(directory, "*.txt").Order())
            {
                string id = Path.GetFileNameWithoutExtension(path);
                var provider = new FileDataProvider(path);
                IDevice sensor;
                Func<double?> read;
                if (id.Equals("temperature", StringComparison.OrdinalIgnoreCase))
                {
                    var temperature = new TemperatureSensor(id, habitat.HabitatId, id, provider);
                    sensor = temperature;
                    read = () => { temperature.ReadNextReading(); return temperature.CurrentTemperature; };
                }
                else if (id.Equals("oxygen", StringComparison.OrdinalIgnoreCase))
                {
                    var oxygen = new OxygenSensor(id, habitat.HabitatId, id, provider);
                    sensor = oxygen;
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
                            sensor = flow; update = flow.UpdateReading; break;
                        case "salinity":
                            var salinity = new SalinitySensor(id, habitat.HabitatId, id);
                            sensor = salinity; update = salinity.UpdateReading; break;
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
                hub.RegisterSensorInput(habitat.HabitatId, id, read);
            }
        }
    }

    private sealed record FileSensor(string DeviceId, string HabitatId, string Name) : IDevice;
}
