using System.Globalization;
using AquaGuard.Data;

namespace AquaGuard.Core
{
    public sealed record SensorDisplay(string HabitatId, string DeviceId, double? Value)
    {
        public string Reading => Value?.ToString(CultureInfo.InvariantCulture) ?? "No Data";
    }

    public sealed class SensorMonitor
    {
        private readonly List<(string HabitatId, string DeviceId, FileDataProvider Provider)> sources = new();

        public void AddSource(string habitatId, string deviceId, FileDataProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);
            sources.Add((habitatId, deviceId, provider));
        }

        public IReadOnlyList<SensorDisplay> Poll()
        {
            var readings = new List<SensorDisplay>(sources.Count);
            foreach (var source in sources)
            {
                string? record = source.Provider.ReadNextRecord();
                double? value = double.TryParse(record, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out double parsed) && double.IsFinite(parsed) ? parsed : null;
                readings.Add(new SensorDisplay(source.HabitatId, source.DeviceId, value));
            }

            return readings;
        }
    }
}
